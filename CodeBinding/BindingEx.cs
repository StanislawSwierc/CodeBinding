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
            Expression<Func<object>> targetLambda = Expression.Lambda<Func<object>>(member.Expression);
            object target = targetLambda.Compile().Invoke();
            System.Windows.FrameworkElement element = target as System.Windows.FrameworkElement;
            if (element == null)
            {
                throw new ArgumentException("target object has to be System.Windows.FrameworkElement");
            }

            System.Windows.DependencyProperty dproperty = GetDependencyPropety(element.GetType(), property);

            BindingBase binding = CreateBinding(right, mode);
            element.SetBinding(dproperty, binding);
            //element.GetBindingExpression(dproperty).UpdateTarget();
            return binding;
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

        public class CustomConverterVisitor : ExpressionVisitor
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

    public class DelegateValueConverter : IValueConverter
    {
        private Delegate m_delegate;

        public DelegateValueConverter(Delegate converter)
        {
            m_delegate = converter;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != m_delegate.Method.ReturnType)
            {
                throw new ArgumentException("targetType");
            }
            return m_delegate.DynamicInvoke(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DelegateMultiValueConverter : IMultiValueConverter
    {
        private Delegate m_delegate;
        private Type[] m_ParametersType;
        private Lazy<Lazy<object>[]> m_ParametersDefaultValues;

        public DelegateMultiValueConverter(Delegate converter)
        {
            m_delegate = converter;
            m_ParametersType = m_delegate.Method.GetParameters().Skip(1).Select(p => p.ParameterType).ToArray();
            m_ParametersDefaultValues = new Lazy<Lazy<object>[]>(GetParameterDefaultValues);
        }

        private Lazy<object>[] GetParameterDefaultValues()
        {
            return m_ParametersType.Select(pt => new Lazy<object>(() => pt.IsValueType ?  Activator.CreateInstance(pt) : null)).ToArray();
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != m_delegate.Method.ReturnType)
            {
                throw new ArgumentException("targetType");
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == System.Windows.DependencyProperty.UnsetValue)
                {
                    values[i] = m_ParametersDefaultValues.Value[i].Value;
                }
            }
            return m_delegate.DynamicInvoke(values);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
