using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SQLite;


namespace SqliteExtensions
{
    public class QueryBuilder<TOut>
        where TOut : new()
    {
        public string TableName => typeof(TOut).Name;

        public QueryBuilder(SQLiteAsyncConnection conection)
        {
            _conection = conection;
        }

        public QueryBuilder<TOut> Select()
        {
            Query = new StringBuilder($"{Sql.Select} {Sql.Star} {Sql.From} {TableName}");
            return this;
        }

        public QueryBuilder<TOut> Where(Expression<Func<TOut, bool>> predicate)
        {
            var body = predicate.Body.ToBinaryExpression();

            var query = $"{TableName}.{body.GetLeftMemeberName()} {body.NodeType.ReadAsString()} {body.Right}";

            Query.Append(_hasWhereClause ? $" {Sql.And} {query}" : $" {Sql.Where} {query}");

            _hasWhereClause = true;
            return this;
        }

        public QueryBuilder<TOut> WhereIn<T>(Expression<Func<TOut, T>> selector, IList<T> collection)
        {
            var query = $"{TableName}.{GetPropertyName(selector)} {Sql.In} ({string.Join(",", collection)})";

            Query.Append(_hasWhereClause ? $" {Sql.And} {query}" : $" {Sql.Where} {query}");

            return this;
        }

        public QueryBuilder<TOut> WhereIn<T>(Expression<Func<TOut, T>> selector, params T[] collection)
        {
            return this.WhereIn(selector, new List<T>(collection));
        }

        public QueryBuilder<TOut> Marching<TIn, TOutProperty, TInProperty>(Expression<Func<TOut, TOutProperty>> firstSelector,
            Expression<Func<TIn, TOutProperty>> secondSelector, Expression<Func<TOutProperty, TInProperty, bool>> predicate,
            string joinType = null)
            where TIn : new()
        {
            var body = predicate.Body.ToBinaryExpression();

            var innerTable = typeof(TIn).Name;

            var query =
                $@" {Sql.Join} {innerTable} {Sql.On} {innerTable}.{GetPropertyName(secondSelector)} {
                        body.NodeType.ReadAsString()
                    } {TableName}.{GetPropertyName(firstSelector)}";

            if (!string.IsNullOrEmpty(joinType))
                query = joinType + query;

            Query.Append(query);

            return this;
        }

        public QueryBuilder<TOut> OrderBy<TProperty>(Expression<Func<TOut, TProperty>> selector)
        {
            Query.Append($" {Sql.OrderBy} {TableName}.{GetPropertyName(selector)}");

            return this;
        }

        public QueryBuilder<TOut> OrderByDescending<TProperty>(Expression<Func<TOut, TProperty>> selector)
        {
            Query.Append($" {Sql.OrderBy} {TableName}.{GetPropertyName(selector)} {Sql.Desc}");

            return this;
        }

        public Task<List<TOut>> ToListAsync()
        {
            return _conection.QueryAsync<TOut>(Query.ToString());
        }

        public Task<List<TOut>> TakeAsync(int count)
        {
            Query.Append($" {Sql.Limit} {count}");

            return _conection.QueryAsync<TOut>(Query.ToString());
        }

        public async Task<TOut> FirstOrDefaultAsync()
        {
            var result = await TakeAsync(1);
            return result.FirstOrDefault();
        }

        #region - Private -

        private string GetPropertyName<TTable, TProperty>(Expression<Func<TTable, TProperty>> selector)
            where TTable : new()
        {
            return ((PropertyInfo)((MemberExpression)selector.Body).Member).Name;
        }

        private StringBuilder Query
        {
            get
            {
                if (_query == null)
                    throw new MieszkoQueryException($"Use {nameof(Select)} as first method");
                return _query;
            }
            set { _query = value; }
        }

        private readonly SQLiteAsyncConnection _conection;
        private bool _hasWhereClause;
        private StringBuilder _query;

        #endregion

    }

}
