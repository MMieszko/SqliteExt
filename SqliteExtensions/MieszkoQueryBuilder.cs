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
    public class QueryBuilder<TTable>
        where TTable : new()
    {
        public string TableName => typeof(TTable).Name;

        public QueryBuilder(SQLiteAsyncConnection conection)
        {
            _conection = conection;
            _query = new StringBuilder($"{Sql.Select} {Sql.Star} {Sql.From} {TableName}");
            _includes = new List<(string propertyname, object includedPropery)>();
        }

        public QueryBuilder<TTable> Where(Expression<Func<TTable, bool>> predicate)
        {
            var body = predicate.Body.ToBinaryExpression();

            object rightValue;

            if (body.Right is MemberExpression expression)
                rightValue = expression.GetValue();
            else
                rightValue = body.Right;

            if (rightValue is string)
                rightValue = $"\"{rightValue}\"";

            return AttatchToQueryChain($"{TableName}.{body.GetLeftMemeberName()} {body.NodeType.ReadAsString()} {rightValue}", Sql.Where);
        }

        public QueryBuilder<TTable> In(Expression<Func<TTable, long>> selector, params long[] collection)
        {
            return this.InImpl(selector, string.Join(",", collection));
        }

        public QueryBuilder<TTable> In(Expression<Func<TTable, string>> selector, params string[] collection)
        {
            return this.InImpl(selector, collection.ToQuotedStringJoin());
        }

        public QueryBuilder<TTable> In(Expression<Func<TTable, char>> selector, params char[] collection)
        {
            return this.InImpl(selector, collection.ToSingleQuotedStringJoin());
        }

        public QueryBuilder<TTable> Like<TProperty>(Expression<Func<TTable, TProperty>> selector, string param)
        {
            var query = $"{TableName}.{GetPropertyName(selector)} {Sql.Like} ({param})";

            _query.Append(_isQueryChainCreated ? $" {Sql.And} {query}" : $" {Sql.Where} {query}");

            return this;
        }

        public QueryBuilder<TTable> OrderBy<TProperty>(Expression<Func<TTable, TProperty>> selector)
        {
            _query.Append($" {Sql.OrderBy} {TableName}.{GetPropertyName(selector)}");

            return this;
        }

        public QueryBuilder<TTable> OrderByDescending<TProperty>(Expression<Func<TTable, TProperty>> selector)
        {
            _query.Append($" {Sql.OrderBy} {TableName}.{GetPropertyName(selector)} {Sql.Desc}");

            return this;
        }

        public QueryBuilder<TTable> Include<TInnerTable>(Expression<Func<TTable, TInnerTable>> selector, object primaryKey)
            where TInnerTable : new()
        {
            var includedPropertyName = GetPropertyName(selector);

            if (string.IsNullOrEmpty(includedPropertyName))
                throw new MieszkoQueryException($"Could not find {includedPropertyName}");

            var includedEntity = AsyncHelpers.RunSync(() => _conection.GetAsync<TInnerTable>(primaryKey));

            if (includedEntity == null)
                throw new MieszkoQueryException($"Could not find {nameof(TInnerTable)}");

            _includes.Add((includedPropertyName, includedEntity));

            return this;
        }
        
        public async Task<IEnumerable<TTable>> ToListAsync()
        {
            var result = (await _conection.QueryAsync<TTable>(_query.ToString())).ToArray();
            AttachIncludes(result);
            return result;
        }

        public async Task<IEnumerable<TTable>> TakeAsync(int count)
        {
            _query.Append($" {Sql.Limit} {count}");
            var result = (await _conection.QueryAsync<TTable>(_query.ToString())).ToArray();
            AttachIncludes(result);
            return result;
        }

        public async Task<TTable> FirstOrDefaultAsync()
        {
            var collectionResult = await TakeAsync(1);
            var result = collectionResult.FirstOrDefault();
            AttachIncludes(result);
            return result;
        }

        #region - Protected - 

        protected virtual void AttachIncludes(params TTable[] entities)
        {
            if (entities == null || entities.Any()) return;

            foreach (var entity in entities)
            {
                foreach (var (propertyname, includedPropery) in _includes)
                {
                    var property = typeof(TTable).GetProperty(propertyname);
                    property.SetValue(entity, includedPropery);
                }
            }
        }

        protected virtual QueryBuilder<TTable> InImpl<T>(Expression<Func<TTable, T>> selector, string @in)
        {
            return AttatchToQueryChain($"{TableName}.{GetPropertyName(selector)} {Sql.In} ({@in})", Sql.In);
        }

        protected virtual QueryBuilder<TTable> AttatchToQueryChain(string query, string caluse)
        {
            _query.Append(_isQueryChainCreated ? $" {Sql.And} {query}" : $" {Sql.Where} {query}");

            _isQueryChainCreated = true; ;
            return this;
        }

        #endregion

        #region - Private -

        private List<(string propertyname, object includedPropery)> _includes;
        private static string GetPropertyName<TProperty>(Expression<Func<TTable, TProperty>> selector)
        {
            return ((PropertyInfo)((MemberExpression)selector.Body).Member).Name;
        }
        private readonly StringBuilder _query;
        private readonly SQLiteAsyncConnection _conection;
        private bool _isQueryChainCreated;
        #endregion
    }
}
