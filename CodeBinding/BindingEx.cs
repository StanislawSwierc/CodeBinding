using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

namespace CodeBinding
{
    public static class BindingEx
    {
        public static BindingBase FromExpression(Expression<Func<bool>> expression)
        {
            return FromExpression(expression, BindingMode.Default);
        }

        public static BindingBase FromExpression(Expression<Func<bool>> expression, BindingMode mode)
        {
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
            if(dproperty == null)
            {
                // target object isn't FrameworkElement or the property isn't DependencyProperty
                if(mode== BindingMode.Default || mode == BindingMode.OneWayToSource || mode == BindingMode.TwoWay)
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
            return binding;
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
            var visitor = new CustomConverterVisitor();

            Expression converterExpression = visitor.ProcessExpression(expression);
            if (visitor.Bindings.Count == 0)
            {
                throw new ArgumentException("expression has 0 binging sources");
            }
            else if (visitor.Bindings.Count == 1)
            {
                Binding result = visitor.Bindings[0];
                result.Mode = mode;
                result.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                if (converterExpression != null)
                {
                    var lambda = Expression.Lambda(converterExpression, visitor.Parameters);
                    result.Converter = new DelegateValueConverter(lambda.Compile());
                }
                return result;
            }
            else
            {
                MultiBinding result = new MultiBinding();
                result.Mode = BindingMode.OneWay;
                result.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                foreach (var binding in visitor.Bindings)
                {
                    result.Bindings.Add(binding);
                }
                // MultiBinding requires converter
                var lambda = Expression.Lambda(converterExpression, visitor.Parameters);
                result.Converter = new DelegateMultiValueConverter(lambda.Compile());
                return result;
            }
        }

        internal class CustomConverterVisitor : ExpressionVisitor
        {
            public List<Binding> Bindings { get; private set; }
            public List<ParameterExpression> Parameters { get; private set; }

            public CustomConverterVisitor()
            {
                Bindings = new List<Binding>();
                Parameters = new List<ParameterExpression>();
            }

            public void Reset()
            {
                Bindings.Clear();
                Parameters.Clear();
            }

            public Expression ProcessExpression(Expression node)
            {
                Expression result = base.Visit(node);
                if (result.NodeType == ExpressionType.Parameter)
                {
                    // No conversion is needed
                    Parameters.Clear();
                    result = null;
                }
                return result;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                string path;
                Expression sourceExpression = ExtractPropertyPath(node, out path);

                Expression<Func<object>> sourceLambda = Expression.Lambda<Func<object>>(sourceExpression);
                object source = sourceLambda.Compile().Invoke();

                // Assign public properties
                Binding binging = new Binding();
                binging.Source = source;
                binging.Path = new System.Windows.PropertyPath(path);
                Bindings.Add(binging);

                // Substitute MemberExpression with ParameterExpression
                var parameter = Expression.Parameter(node.Type, string.Format("p{0}", Parameters.Count));
                Parameters.Add(parameter);
                return parameter;
            }

            public static Expression ExtractPropertyPath(Expression expression, out string path)
            {

                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression memberAccess = (MemberExpression)expression;
                    if (memberAccess.Member is PropertyInfo)
                    {
                        PropertyInfo property = (PropertyInfo)memberAccess.Member;
                        Expression result;
                        string prefix;
                        result = ExtractPropertyPath(memberAccess.Expression, out prefix);
                        path = (prefix == null) ? property.Name : string.Format("{0}.{1}", prefix, property.Name);
                        return result;
                    }
                    else
                    {
                        path = null;
                        return expression;
                    }
                }
                else
                {
                    path = null;
                    return expression;
                }
            }
        }
    }
}
