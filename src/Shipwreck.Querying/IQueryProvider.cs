using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Querying
{
    public interface IQueryProvider<TContext, TEntity>
    {
        bool IsSupported(string prefix);

        IQueryable<TEntity> Filter(TContext context, IQueryable<TEntity> source, IEnumerable<string> values);

        IQueryable<TEntity> Exclude(TContext context, IQueryable<TEntity> source, IEnumerable<string> values);
    }
}
