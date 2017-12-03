using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Shipwreck.Querying
{
    public abstract partial class QueryProvider
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

        private static readonly MethodInfo QueryableAny
            = typeof(Queryable).GetMethods().Single(m => m.Name == nameof(Queryable.Any) && m.GetParameters().Length == 2);

        protected static Expression Replace(Expression expression, Expression currentValue, Expression newValue)
            => new ReplaceExpressionVisitor(currentValue, newValue).Visit(expression);

        private readonly Regex _Pattern;

        protected QueryProvider(params string[] prefixes)
        {
            _Pattern = new Regex("^(" + string.Join("|", prefixes.Select(Regex.Escape)) + ")$", RegexOptions.IgnoreCase);
            Prefixes = Array.AsReadOnly(prefixes.ToArray());
        }

        public ReadOnlyCollection<string> Prefixes { get; }

        public virtual string DisplayName
            => Regex.Replace(GetType().Name, "(?<=.)" + nameof(QueryProvider) + "$", string.Empty);

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

        protected static Expression<Func<TModel1, bool>> CreatePredicate<TModel1, TModel2>(
                Expression<Func<TModel1, TModel2>> entitySelector,
                Expression<Func<TModel2, bool>> predicate)
            => Expression.Lambda<Func<TModel1, bool>>(
                    Replace(predicate.Body, predicate.Parameters[0], entitySelector.Body),
                    entitySelector.Parameters[0]);

        protected static Expression<Func<TModel1, bool>> CreatePredicate<TModel1, TModel2>(
                Expression<Func<TModel1, IQueryable<TModel2>>> querySelector,
                Expression<Func<TModel2, bool>> predicate)
            => Expression.Lambda<Func<TModel1, bool>>(
                    Expression.Call(
                            QueryableAny.MakeGenericMethod(typeof(TModel2)),
                            querySelector.Body,
                            Expression.Constant(predicate)),
                    querySelector.Parameters[0]);

        #region CreateComparison

        protected static Expression<Func<T, bool>> CreateComparison<T, TProperty, TTryParser>(Expression<Func<T, TProperty>> propertySelector, string value)
            where TTryParser : struct, ITryParser<TProperty>
        {
            Expression<Func<T, bool>> result;
            if (TryCreateComparison<T, TProperty, TTryParser>(propertySelector, value, out result))
            {
                return result;
            }
            return _ => false;
        }

        protected static Expression<Func<T, bool>> CreateComparison<T, TProperty, TTryParser>(Expression<Func<T, TProperty?>> propertySelector, string value)
            where TProperty : struct
            where TTryParser : struct, ITryParser<TProperty>
        {
            Expression<Func<T, bool>> result;
            if (TryCreateComparison<T, TProperty, TTryParser>(propertySelector, value, out result))
            {
                return result;
            }
            return _ => false;
        }

        private static bool TryCreateComparison<T, TProperty, TTryParser>(LambdaExpression propertySelector, string value, out Expression<Func<T, bool>> result)
            where TTryParser : struct, ITryParser<TProperty>
        {
            if (!string.IsNullOrEmpty(value))
            {
                var op = ExpressionType.Equal;
                var start = 0;
                if (value.Length > 1)
                {
                    if (value[0] == '<')
                    {
                        if (value.Length > 2 && value[1] == '=')
                        {
                            op = ExpressionType.LessThanOrEqual;
                            start = 2;
                        }
                        else
                        {
                            op = ExpressionType.LessThan;
                            start = 1;
                        }
                    }
                    else if (value[0] == '>')
                    {
                        if (value.Length > 2 && value[1] == '=')
                        {
                            op = ExpressionType.GreaterThanOrEqual;
                            start = 2;
                        }
                        else
                        {
                            op = ExpressionType.GreaterThan;
                            start = 1;
                        }
                    }
                    else if (value.Length > 2 && value[0] == '!' && value[1] == '=')
                    {
                        op = ExpressionType.NotEqual;
                        start = 2;
                    }
                }

                TProperty i;
                if (default(TTryParser).TryParse(start == 0 ? value : value.Substring(start), out i))
                {
                    var left = propertySelector.Body;
                    if (left.Type != typeof(TProperty))
                    {
                        left = Expression.Convert(left, typeof(TProperty));
                    }

                    result = Expression.Lambda<Func<T, bool>>(
                        Expression.MakeBinary(op, left, Expression.Constant(i)),
                        propertySelector.Parameters
                        );
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion CreateComparison

        public static Expression<Func<TEntity, int>> CreateMatchRankSelector<TContext, TEntity>(TContext context, IEnumerable<QueryComponent> query, IEnumerable<IQueryProvider<TContext, TEntity>> providers, IQueryOrderProvider<TContext, TEntity> defaultProvider)
        {
            Expression<Func<TEntity, int>> sum = null;
            foreach (var qc in query)
            {
                Expression<Func<TEntity, MatchRank>> pred;
                if (string.IsNullOrEmpty(qc.Prefix))
                {
                    pred = defaultProvider?.GetMatchRank(context, qc.Value);
                }
                else
                {
                    pred = providers.OfType<IQueryOrderProvider<TContext, TEntity>>().FirstOrDefault(p => p.IsSupported(qc.Prefix))?.GetMatchRank(context, qc.Value);
                }

                if (pred != null)
                {
                    if (sum == null)
                    {
                        sum = Expression.Lambda<Func<TEntity, int>>(Expression.Convert(pred.Body, typeof(int)), pred.Parameters);
                    }
                    else
                    {
                        sum = Expression.Lambda<Func<TEntity, int>>(
                                Expression.Add(
                                    sum.Body,
                                    Expression.Convert(
                                        Replace(pred.Body, pred.Parameters[0], sum.Parameters[0]),
                                        typeof(int))),
                                sum.Parameters);
                    }
                }
            }

            return sum;
        }
    }
}