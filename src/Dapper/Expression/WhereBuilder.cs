using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dapper.Extension;

namespace Dapper.Extension
{
    public class SqlTranslateFormater
    {
        public string Translate<T>(Expression<Func<T, bool>> expression)
        {
            return Recurse(expression.Body, true);
        }

        private string Recurse(Expression expression, bool isUnary = false, bool quote = true)
        {
            if (expression is UnaryExpression)
            {
                var unary = (UnaryExpression)expression;
                var right = Recurse(unary.Operand, true);

                if (unary.NodeType == ExpressionType.Convert)
                {
                    return right;
                }
                else
                {
                    return "(" + NodeTypeToString(unary.NodeType, right == "NULL") + " " + right + ")";
                }
            }

            if (expression is BinaryExpression)
            {
                return BinaryExpression(expression);
            }

            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                return ValueToString(constant.Value, isUnary, quote);
            }

            if (expression is MemberExpression)
            {
                return MemberExpression(expression, isUnary, quote);
            }

            if (expression is MethodCallExpression)
            {
                return MethodExpression(expression);
            }

            throw new Exception("Unsupported expression: " + expression.GetType().Name);
        }

        private string BinaryExpression(Expression expression)
        {
            var body = (BinaryExpression)expression;

            var rightisUnary = false;
            var leftisUnary = false;
            if (expression.NodeType == ExpressionType.AndAlso || expression.NodeType == ExpressionType.OrElse)
            {
                if ((body.Right.NodeType == ExpressionType.MemberAccess || body.Right.NodeType == ExpressionType.Constant) && body.Right.Type == typeof(bool))
                {
                    rightisUnary = true;
                }
                if ((body.Left.NodeType == ExpressionType.MemberAccess || body.Left.NodeType == ExpressionType.Constant) && body.Left.Type == typeof(bool))
                {
                    leftisUnary = true;
                }
            }

            var right = Recurse(body.Right, rightisUnary);
            return "(" + Recurse(body.Left, leftisUnary) + " " + NodeTypeToString(body.NodeType, right == "NULL") + " " + right + ")";
        }

        private string MemberExpression(Expression expression, bool isUnary = false, bool quote = true)
        {
            var member = (MemberExpression)expression;

            if (TryGetValue(member, out var getter))
            {
                return ValueToString(getter(), isUnary, quote);
            }
            else if (member.Member is PropertyInfo)
            {
                var property = (PropertyInfo)member.Member;
                var colName = SqlMapperExtensions.GetCustomColumnName(property);

                if (isUnary && member.Type == typeof(bool))
                {
                    return "([" + colName + "] = 1)";
                }
                return "[" + colName + "]";
            }
            else if (member.Member is FieldInfo)
            {
                var property = (FieldInfo)member.Member;
                var colName = SqlMapperExtensions.GetCustomColumnName(property);

                if (isUnary && member.Type == typeof(bool))
                {
                    return "([" + colName + "] = 1)";
                }
                return "[" + colName + "]";
            }

            throw new Exception($"Expression does not refer to a property or field: {expression}");
        }

        private string MethodExpression(Expression expression)
        {
            var methodCall = (MethodCallExpression)expression;
            // LIKE queries:
            if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
            {
                return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
            }

            if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
            {
                return "(" + Recurse(methodCall.Object) + " LIKE '" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
            }

            if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
            {
                return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "')";
            }

            // IN queries:
            if (methodCall.Method.Name == "Contains")
            {
                Expression collection;
                Expression property;
                if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                {
                    collection = methodCall.Arguments[0];
                    property = methodCall.Arguments[1];
                }
                else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                {
                    collection = methodCall.Object;
                    property = methodCall.Arguments[0];
                }
                else
                {
                    throw new Exception("Unsupported method call: " + methodCall.Method.Name);
                }
                var values = (IEnumerable)GetValue(collection);
                var concated = "";
                foreach (var e in values)
                {
                    concated += ValueToString(e, false, true) + ", ";
                }
                if (concated == "")
                {
                    return ValueToString(false, true, false);
                }
                return "(" + Recurse(property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
            }

            if (methodCall.Method.Name == "IsNotNull")
            {
                return "(" + Recurse(methodCall.Arguments[0]) + " IS NOT NULL)";
            }

            if (methodCall.Method.Name == "IsNull")
            {
                return "(" + Recurse(methodCall.Arguments[0]) + " IS NULL)";
            }

            if (methodCall.Method.Name == "GetDate")
            {
                return "GETDATE()";
            }

            throw new Exception("Unsupported method call: " + methodCall.Method.Name);
        }

        private string ValueToString(object value, bool isUnary, bool quote)
        {
            if (value is bool)
            {
                if (isUnary)
                {
                    return (bool)value ? "(1=1)" : "(1=0)";
                }
                return (bool)value ? "1" : "0";
            }
            else if (value is null)
            {
                return "NULL";
            }
            else if ((value is string ||value is DateTime) && quote)
            {
                return $"'{value}'";
            }
            else if (value is Guid && quote)
            {
                return $"'{{{value}}}'";
            }
            return value.ToString();
        }

//        private static bool IsEnumerableType(Type type)
//        {
//            return type
//                .GetInterfaces()
//                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
//        }

        private static object GetValue(Expression member)
        {
            // source: http://stackoverflow.com/a/2616980/291955
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static bool TryGetValue(Expression member, out Func<object> getter)
        {
            bool result = false;
            try
            {
                // source: http://stackoverflow.com/a/2616980/291955
                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                getter = getterLambda.Compile();
                result = true;
            }
            catch (Exception e)
            {
                getter = null;
            }

            return result;
        }

        private static object NodeTypeToString(ExpressionType nodeType, bool rightIsNull)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return rightIsNull ? "IS" : "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Convert:
                    return "";
            }
            throw new Exception($"Unsupported node type: {nodeType}");
        }
    }
}
