using System;
using System.Linq.Expressions;

namespace Shipwreck.Querying
{
    public interface IQueryOrderProvider<TContext, TEntity> : IQueryProvider<TContext, TEntity>
    {
        Expression<Func<TEntity, MatchRank>> GetMatchRank(TContext context, string value);
    }
}