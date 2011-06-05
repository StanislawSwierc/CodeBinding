using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace CodeBinding
{
    public static class BindingEx
    {
        /// <summary>
        /// Creates binding and attaches it to the target based on expression provided
        /// </summary>
        /// <remarks>
        /// Expression has to be of type (target object expression).(target property) == (source expression)
        /// </remarks>
        /// <param name="expression">
        /// Input expression of type: (target object expression).(target property) == (source expression)
        /// </param>
        /// <returns>IDisposable object used disable binding.</returns>
        public static IDisposable BindFromExpression(Expression<Func<bool>> expression)
        {
            return BindFromExpression(expression, BindingMode.Default);
        }

        /// <summary>
        /// Creates binding and attaches it to the target based on expression provided
        /// </summary>
        /// <remarks>
        /// Expression has to be of type (target object expression).(target property) == (source expression)
        /// </remarks>
        /// <param name="expression">
        /// Expression of type: (target object expression).(target property) == (source expression)
        /// </param>
        /// <param name="mode">Direction of the data flow in a binding.</param>
        /// <returns>IDisposable object used disable binding.</returns>
        public static IDisposable BindFromExpression(Expression<Func<bool>> expression, BindingMode mode)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<IDisposable>() != null);

            MemberExpression member = null;
            Expression right = null;

            if (expression.Body.NodeType == ExpressionType.Equal)
            {
                var binary = ((BinaryExpression)expression.Body);
                right = binary.Right;
                member = binary.Left as MemberExpression;
                if (member != null && member.Member.MemberType != MemberTypes.Property)
                {
                    // Invalidate member because it isn't a Property
                    member = null;
                }
            }
            if (member == null)
            {
                throw new ArgumentException("expression has to have \"<target object expression>.<target property> == <expr>\" structure");
            }

            PropertyInfo property = (PropertyInfo)member.Member;

            object target = GetInstanceFromExpression(member.Expression);
            BindingBase binding = CreateBinding(right, mode);

            System.Windows.FrameworkElement element = target as System.Windows.FrameworkElement;
            System.Windows.DependencyProperty dproperty = null;
            if (element != null)
            {
                dproperty = GetDependencyPropety(element.GetType(), property);
            }
            if (dproperty == null)
            {
                // target object isn't FrameworkElement or the property isn't DependencyProperty
                if (mode == BindingMode.Default || mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay)
                {
                    throw new ArgumentException(string.Format("Property \"{0}\" needs to be DependencyProperty in order to bind in mode \"{1}\"",
                        property.Name, mode));
                }

                // Property cannot be binding target. Create wrapper
                element = new BindingTarget(target, property);
                binding.FallbackValue = element;
                dproperty = BindingTarget.ValueProperty;
            }
            element.SetBinding(dproperty, binding);

            return new ClearBindingDisposable(element, dproperty, binding);
        }

        /// <summary>
        /// Creates binding base on expression provided.
        /// </summary>
        /// <typeparam name="T">Type of the binding target property.</typeparam>
        /// <param name="expression">Expression of any type.</param>
        /// <returns>Binding object</returns>
        public static BindingBase FromExpression<T>(Expression<Func<T>> expression)
        {
            return FromExpression(expression, BindingMode.Default);
        }

        /// <summary>
        /// Creates binding base on expression provided
        /// </summary>
        /// <typeparam name="T">Type of the binding target property</typeparam>
        /// <param name="expression">Expression of any type</param>
        /// <param name="mode">Direction of the data flow in a binding.</param>
        /// <returns>Binding object.</returns>
        public static BindingBase FromExpression<T>(Expression<Func<T>> expression, BindingMode mode)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<BindingBase>() != null);

            return CreateBinding(expression.Body, mode);
        }

        private static object GetInstanceFromExpression(Expression expression)
        {
            Expression<Func<object>> targetLambda = Expression.Lambda<Func<object>>(expression);
            return targetLambda.Compile().Invoke();
        }

        public static System.Windows.DependencyProperty GetDependencyPropety(Type type, PropertyInfo property)
        {
            FieldInfo field = type.GetField(string.Format("{0}Property", property.Name));
            if (field != null)
            {
                if (field.IsStatic || field.FieldType == typeof(System.Windows.DependencyProperty))
                {
                    return (System.Windows.DependencyProperty)field.GetValue(null);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (type.BaseType != null)
                {
                    return GetDependencyPropety(type.BaseType, property);
                }
                else
                {
                    return null;
                }
            }
        }

        private static BindingBase CreateBinding(Expression expression, BindingMode mode)
        {
            Contract.Requires(expression != null);
            Contract.Ensures(Contract.Result<BindingBase>() != null);

            var visitor = new CustomConverterVisitor();
            visitor.ProcessExpression(expression);

            if (visitor.Bindings.Count == 0)
            {
                throw new ArgumentException("expression has 0 binging sources");
            }
            else if (visitor.Bindings.Count == 1)
            {
                Binding result = visitor.Bindings[0];
                result.Mode = mode;
                if (visitor.ConverterNeeded)
                {
                    result.Converter = new ExpressionValueConverter(expression);
                }
                return result;
            }
            else
            {
                MultiBinding result = new MultiBinding();
                result.Mode = BindingMode.OneWay;
                foreach (var binding in visitor.Bindings)
                {
                    result.Bindings.Add(binding);
                }
                // MultiBinding requires converter
                result.Converter = new ExpressionValueConverter(expression);
                return result;
            }
        }

        internal class CustomConverterVisitor : ExpressionVisitor
        {
            public List<Binding> Bindings { get; private set; }
            public bool ConverterNeeded { get; private set; }
            private StringBuilder m_StringBuilder;
            
            public CustomConverterVisitor()
            {
                Bindings = new List<Binding>();
                m_StringBuilder = new StringBuilder();
            }

            public void ProcessExpression(Expression node)
            {
                CreateBindingFromSimpleExpression(node);
                if (Bindings.Count == 0)
                {
                    ConverterNeeded = true;
                    base.Visit(node);
                }
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                m_StringBuilder.Clear();
                CreateBinding(node, m_StringBuilder, null);
                return base.VisitMember(node);
            }

            /// <summary>
            /// Try to create binding from simple Expression
            /// </summary>
            /// <remarks>
            /// This method will create and add binding to Bindings collection
            /// only if Expression is of type obj.Property
            /// </remarks>
            /// <param name="node">Expression</param>
            private void CreateBindingFromSimpleExpression(Expression node)
            {
                if (node.NodeType == ExpressionType.MemberAccess)
                {
                    var member = (MemberExpression)node;
                    var property = member.Member as PropertyInfo;
                    if (property != null && member.Expression.NodeType == ExpressionType.Constant)
                    {
                        var constant = (ConstantExpression)member.Expression;
                        Binding binding = new Binding();
                        binding.Source = constant.Value;
                        binding.Path = new System.Windows.PropertyPath(property.Name);
                        Bindings.Add(binding);
                    }
                }
            }

            private void CreateBinding(Expression node, StringBuilder path, Expression top)
            {
                if (node.NodeType == ExpressionType.MemberAccess)
                {
                    var member = (MemberExpression)node;
                    var property = member.Member as PropertyInfo;
                    if (property != null)
                    {
                        if (path.Length > 0)
                        {
                            path.Append(".");
                        }
                        path.Append(property.Name);
                        CreateBinding(member.Expression, path, top);
                    }
                    else if (top == null)
                    {
                        CreateBinding(member.Expression, path, member);
                    }
                    else
                    {
                        CreateBinding(member.Expression, path, top);
                    }
                }
                else if (node.NodeType == ExpressionType.Constant)
                {
                    object source = null;
                    var constant = (ConstantExpression)node;
                    if (top != null)
                    {
                        if (top.Type != typeof(object))
                        {
                            top = Expression.Convert(top, typeof(object));
                        }
                        Expression<Func<object>> sourceLambda = Expression.Lambda<Func<object>>(top);
                        source = sourceLambda.Compile().Invoke();
                    }
                    else
                    {
                        source = constant.Value;
                    }
                    // Assign public properties
                    Binding binding = new Binding();
                    binding.Source = source;
                    binding.Path = new System.Windows.PropertyPath(path.ToString());
                    Bindings.Add(binding);
                }
            }
        }
    }
}
