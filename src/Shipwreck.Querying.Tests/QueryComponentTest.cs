using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Shipwreck.Querying
{
    [TestClass]
    public class QueryComponentTest
    {
        #region Parse

        #region WhiteSpace

        [TestMethod]
        public void ParseTest_Null()
        {
            var actual = QueryComponent.Parse(null);
            CollectionAssert.AreEquivalent(new QueryComponent[0], actual.ToList());
        }

        [TestMethod]
        public void ParseTest_Empty()
        {
            var actual = QueryComponent.Parse("");
            CollectionAssert.AreEquivalent(new QueryComponent[0], actual.ToList());
        }

        [TestMethod]
        public void ParseTest_WhiteSpace()
        {
            var actual = QueryComponent.Parse(" ");
            CollectionAssert.AreEquivalent(new QueryComponent[0], actual.ToList());
        }

        #endregion WhiteSpace

        [TestMethod]
        public void ParseTest_Required()
        {
            var actual = QueryComponent.Parse(" +abc ").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Required, null, "abc", 1, 4)
            }));
        }

        [TestMethod]
        public void ParseTest_Excluded()
        {
            var actual = QueryComponent.Parse(" -abc ").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded, null, "abc", 1, 4)
            }));
        }

        [TestMethod]
        public void ParseTest_Quoted()
        {
            var actual = QueryComponent.Parse(" \"abc\" ").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.None, null, "abc", 1, 5)
            }));
        }

        [TestMethod]
        public void ParseTest_Prefixed()
        {
            var actual = QueryComponent.Parse(" abc:def ").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.None,  "abc","def", 1, 7)
            }));
        }

        [TestMethod]
        public void ParseTest_OperatorPrefixedQuoted()
        {
            var actual = QueryComponent.Parse(" -abc:\"def\" ").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded,  "abc","def", 1, 10)
            }));
        }

        [TestMethod]
        public void ParseTest_Full()
        {
            var actual = QueryComponent.Parse("-abc:\"def\"  ghi").ToList();
            Assert.IsTrue(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded,  "abc","def", 0, 9),
                new QueryComponent(ComponentOperator.None,  null,"ghi", 12, 14)
            }));
        }

        #endregion Parse
    }
}