using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Shipwreck.Querying
{
    public abstract class QueryProvider
    {
        private sealed class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _CurrentValue;
            private readonly Expression _NewValue;

            public ReplaceExpressionVisitor(Expression currentValue, Expression newValue)
            {
                _CurrentValue = currentValue;
                _NewValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _CurrentValue)
                {
                    return _NewValue;
                }
                return base.Visit(node);
            }
        }

        private static Expression Replace(Expression expression, Expression currentValue, Expression newValue)
            => new ReplaceExpressionVisitor(currentValue, newValue).Visit(expression);

        private readonly Regex _Pattern;

        protected QueryProvider(params string[] prefixes)
        {
            _Pattern = new Regex("^(" + string.Join("|", prefixes.Select(Regex.Escape)) + ")$", RegexOptions.IgnoreCase);
        }

        public bool IsSupported(string prefix)
              => prefix != null && _Pattern.IsMatch(prefix);

        protected static IQueryable<T> FilterCore<T>(IQueryable<T> source, IEnumerable<string> values, Func<string, Expression<Func<T, bool>>> conditionSelector)
        {
            Expression<Func<T, bool>> r = null;
            foreach (var v in values)
            {
                var c = conditionSelector(v);
                r = r == null ? c : Expression.Lambda<Func<T, bool>>(
                                        Expression.OrElse(
                                            r.Body,
                                            Replace(c.Body, c.Parameters[0], r.Parameters[0])),
                                        r.Parameters[0]);
            }

            return source.Where(r);
        }

        protected static IQueryable<T> ExcludeCore<T>(IQueryable<T> source, IEnumerable<string> values, Func<string, Expression<Func<T, bool>>> conditionSelector)
        {
            Expression<Func<T, bool>> r = null;
            foreach (var v in values)
            {
                var c = conditionSelector(v);
                r = r == null
                        ? Expression.Lambda<Func<T, bool>>(Expression.Not(c.Body), c.Parameters)
                        : Expression.Lambda<Func<T, bool>>(
                                        Expression.AndAlso(
                                            r.Body,
                                            Replace(Expression.Not(c.Body), c.Parameters[0], r.Parameters[0])),
                                        r.Parameters[0]);
            }

            return source.Where(r);
        }

        public static IQueryable<TEntity> Filter<TContext, TEntity>(TContext context, IQueryable<TEntity> source, IEnumerable<QueryComponent> query, IEnumerable<IQueryProvider<TContext, TEntity>> providers, IQueryProvider<TContext, TEntity> defaultProvider)
        {
            var r = source;
            foreach (var g in query.GroupBy(q => new
            {
                q.Operator,
                Provider = providers.FirstOrDefault(p => p.IsSupported(q.Prefix))
            }))
            {
                r = Filter(
                        context,
                        r, g.Key.Provider
                            ?? (g.All(c => string.IsNullOrEmpty(c.Prefix)) ? defaultProvider : null),
                        g.Key.Operator,
                        g.Select(c => c.Value).Distinct());
            }

            return r;
        }

        private static IQueryable<TEntity> Filter<TContext, TEntity>(TContext context, IQueryable<TEntity> source, IQueryProvider<TContext, TEntity> provider, ComponentOperator @operator, IEnumerable<string> values)
        {
            var s = source;
            if (provider == null)
            {
                return @operator == ComponentOperator.Excluded ? source : source.Where(_ => false);
            }
            switch (@operator)
            {
                case ComponentOperator.Required:
                    foreach (var v in values)
                    {
                        s = provider.Filter(context, s, Enumerable.Repeat(v, 1));
                    }
                    return s;

                case ComponentOperator.Excluded:
                    return provider.Exclude(context, s, values);

                default:
                    return provider.Filter(context, s, values);
            }
        }

        protected static Expression<Func<TModel1, bool>> CreatePredicate<TModel1, TModel2>(Expression<Func<TModel1, TModel2>> entitySelector, Expression<Func<TModel2, bool>> predicate)
        {
            return Expression.Lambda<Func<TModel1, bool>>(Replace(predicate.Body, predicate.Parameters[0], entitySelector.Body), entitySelector.Parameters[0]);
        }
    }
}