using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Shipwreck.Querying
{

    [TestClass]
    public class QueryProviderTest
    {
        #region CreateMatchRankSelectorTest

        private class CreateMatchRankSelectorTestClass
        {
            public string Fuga { get; set; }
            public string Piyo { get; set; }
        }

        private class CreateMatchRankSelectorTestProvider : IQueryOrderProvider<object, CreateMatchRankSelectorTestClass>
        {
            public IQueryable<CreateMatchRankSelectorTestClass> Exclude(object context, IQueryable<CreateMatchRankSelectorTestClass> source, IEnumerable<string> values)
            {
                throw new NotImplementedException();
            }

            public IQueryable<CreateMatchRankSelectorTestClass> Filter(object context, IQueryable<CreateMatchRankSelectorTestClass> source, IEnumerable<string> values)
            {
                throw new NotImplementedException();
            }

            public Expression<Func<CreateMatchRankSelectorTestClass, MatchRank>> GetMatchRank(object context, string value)
                => h => h.Fuga == value ? MatchRank.ExactMatch : h.Fuga.StartsWith(value) ? MatchRank.StartsWith : MatchRank.Contains;

            public bool IsSupported(string prefix)
                => prefix == "1";
        }

        [TestMethod]
        public void CreateMatchRankSelectorTest()
        {
            var qop = new CreateMatchRankSelectorTestProvider();

            var sel = QueryProvider.CreateMatchRankSelector(null, QueryComponent.Parse("1:a 2:b c"), new[] { qop }, qop);
            Console.Write(sel);
        }

        #endregion CreateMatchRankSelectorTest
    }
}