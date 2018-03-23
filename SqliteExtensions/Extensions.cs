using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SQLite;


namespace SqliteExtensions
{
    public static class Extensions
    {
        public static string GetLeftMemeberName(this BinaryExpression @this)
        {
            if (@this.Left is MemberExpression left)
            {
                return left.Member.Name;
            }

            return null;
        }

        public static string ReadAsString(this ExpressionType @this)
        {
            switch (@this)
            {
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Or:
                    return "||";
                default:
                    throw new InvalidOperationException($"Given expression type ( {@this} ) is not valid");
            }
        }

        public static BinaryExpression ToBinaryExpression(this Expression @this)
        {
            if (@this is BinaryExpression result)
                return result;
            
            throw new MieszkoQueryException("Only binary expressions are suppored");
        }

        public static QueryBuilder<T> MieszkoQuery<T>(this SQLiteAsyncConnection @this)
            where T : new()
        {
            return new QueryBuilder<T>(@this);
        }

        public static string ToQuotedStringJoin(this IEnumerable<string> @this)
        {
            return string.Join(",", @this.Select(item => $"\"{item}\"").ToList());
        }

        public static string ToSingleQuotedStringJoin(this IEnumerable<char> @this)
        {
            return string.Join(",", @this.Select(item => $"'{item}'").ToList());
        }

        public static object GetValue(this MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}
