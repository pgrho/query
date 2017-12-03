 using System.Linq;
using Xunit;

namespace Shipwreck.Querying
{
     public class QueryComponentTest
    {
        #region Parse

        #region WhiteSpace

        [Fact]
        public void ParseTest_Null()
        {
            var actual = QueryComponent.Parse(null);
            Assert.Equal(new QueryComponent[0], actual.ToList());
        }

        [Fact]
        public void ParseTest_Empty()
        {
            var actual = QueryComponent.Parse("");
            Assert.Equal(new QueryComponent[0], actual.ToList());
        }

        [Fact]
        public void ParseTest_WhiteSpace()
        {
            var actual = QueryComponent.Parse(" ");
            Assert.Equal(new QueryComponent[0], actual.ToList());
        }

        #endregion WhiteSpace

        [Fact]
        public void ParseTest_Required()
        {
            var actual = QueryComponent.Parse(" +abc ").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Required, null, "abc", 1, 4)
            }));
        }

        [Fact]
        public void ParseTest_Excluded()
        {
            var actual = QueryComponent.Parse(" -abc ").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded, null, "abc", 1, 4)
            }));
        }

        [Fact]
        public void ParseTest_Quoted()
        {
            var actual = QueryComponent.Parse(" \"abc\" ").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.None, null, "abc", 1, 5)
            }));
        }

        [Fact]
        public void ParseTest_Prefixed()
        {
            var actual = QueryComponent.Parse(" abc:def ").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.None,  "abc","def", 1, 7)
            }));
        }

        [Fact]
        public void ParseTest_OperatorPrefixedQuoted()
        {
            var actual = QueryComponent.Parse(" -abc:\"def\" ").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded,  "abc","def", 1, 10)
            }));
        }

        [Fact]
        public void ParseTest_Full()
        {
            var actual = QueryComponent.Parse("-abc:\"def\"  ghi").ToList();
            Assert.True(actual.SequenceEqual(new[] {
                new QueryComponent(ComponentOperator.Excluded,  "abc","def", 0, 9),
                new QueryComponent(ComponentOperator.None,  null,"ghi", 12, 14)
            }));
        }

        #endregion Parse
    }
}