using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shipwreck.Querying
{
    /// <summary>
    /// 検索クエリーに含まれるコンポーネントを表します。
    /// </summary>
    public sealed class QueryComponent : IEquatable<QueryComponent>
    {
        private sealed class QueryParser
        {
            private bool _IsPrefix;
            private bool _IsQuoted;
            private string _Prefix;
            private StringBuilder _Buffer;
            private ComponentOperator _Operator;
            private int _StartIndex;

            public IEnumerable<QueryComponent> Parse(string query)
            {
                if (query == null)
                {
                    yield break;
                }

                var position = -1;
                ClearState(-1);

                foreach (var c in query)
                {
                    position++;

                    if (IsEmpty())
                    {
                        switch (c)
                        {
                            case '+':
                                _Operator = ComponentOperator.Required;
                                continue;

                            case '-':
                                _Operator = ComponentOperator.Excluded;
                                continue;
                        }
                    }
                    if (_IsPrefix)
                    {
                        if (IsWhitespace(c))
                        {
                            if (_Buffer.Length > 0)
                            {
                                yield return CreateQueryComponent(position - 1);
                            }
                            ClearState(position);
                        }
                        else if (c == '"')
                        {
                            _Prefix = null;
                            _IsPrefix = false;
                            _IsQuoted = _Buffer.Length == 0;
                        }
                        else if (c == ':')
                        {
                            if (_Buffer.Length == 0)
                            {
                                _Buffer.Append(c);
                            }
                            else
                            {
                                _Prefix = _Buffer.ToString();
                                _Buffer.Clear();
                            }
                            _IsPrefix = false;
                            _IsQuoted = false;
                        }
                        else
                        {
                            _Buffer.Append(c);
                        }
                    }
                    else
                    {
                        if (IsWhitespace(c))
                        {
                            if (_IsQuoted)
                            {
                                _Buffer.Append(c);
                            }
                            else
                            {
                                yield return CreateQueryComponent(position - 1);
                                ClearState(position);
                            }
                        }
                        else if (c == '"')
                        {
                            if (_IsQuoted)
                            {
                                if (_Prefix != null || _Buffer.Length > 0)
                                {
                                    yield return CreateQueryComponent(position);
                                }
                                ClearState(position);
                            }
                            else if (_Buffer.Length == 0)
                            {
                                _IsQuoted = true;
                            }
                            else
                            {
                                _Buffer.Append(c);
                            }
                        }
                        else
                        {
                            _Buffer.Append(c);
                        }
                    }
                }

                if (!_IsPrefix || _Buffer.Length > 0)
                {
                    yield return CreateQueryComponent(query.Length - 1);
                }
            }

            private bool IsEmpty()
                => _Operator == ComponentOperator.None && _IsPrefix && _Buffer.Length == 0;

            private static bool IsWhitespace(char c)
                => char.IsWhiteSpace(c);

            private void ClearState(int position)
            {
                _StartIndex = position + 1;
                _Operator = ComponentOperator.None;
                _Prefix = null;
                _IsPrefix = true;
                _IsQuoted = false;
                if (_Buffer == null)
                {
                    _Buffer = new StringBuilder();
                }
                else
                {
                    _Buffer.Clear();
                }
            }

            private QueryComponent CreateQueryComponent(int position)
                => new QueryComponent(_Operator, _Prefix, _Buffer.ToString(), _StartIndex, position);
        }

        public QueryComponent(ComponentOperator condition, string prefix, string value, int startIndex, int lastIndex)
        {
            Operator = condition;
            Prefix = prefix ?? string.Empty;
            Value = value ?? string.Empty;
            StartIndex = startIndex;
            LastIndex = lastIndex;
        }

        /// <summary>
        /// コンポーネントの処理方法を取得します。
        /// </summary>
        public ComponentOperator Operator { get; }

        /// <summary>
        /// コンポーネントの種別を取得します。
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// コンポーネントで指定された文字列を取得します。
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// コンポーネントの先頭の文字列内の<c>0</c>から始まるインデックスを取得します。
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// コンポーネントの末尾の文字列内の<c>0</c>から始まるインデックスを取得します。
        /// </summary>
        public int LastIndex { get; }

        /// <summary>
        /// コンポーネントの文字数を取得します。
        /// </summary>
        public int Length
            => LastIndex - StartIndex + 1;

        /// <summary>
        /// 指定した文字列に含まれるコンポーネントのシーケンスを返します。
        /// </summary>
        /// <param name="query">検索文字列。</param>
        /// <returns>含まれるコンポーネントのシーケンス。</returns>
        public static IEnumerable<QueryComponent> Parse(string query)
            => string.IsNullOrWhiteSpace(query) ? Enumerable.Empty<QueryComponent>() : new QueryParser().Parse(query);

        public static bool operator ==(QueryComponent left, QueryComponent right)
            => (left == (object)null && right == (object)null)
                || ((left != (object)null && right != (object)null)
                    && left.Operator == right.Operator
                    && left.Prefix == right.Prefix
                    && left.Value == right.Value
                    && left.StartIndex == right.StartIndex
                    && left.LastIndex == right.LastIndex);

        public static bool operator !=(QueryComponent left, QueryComponent right)
            => !(left == right);

        public override bool Equals(object obj)
            => this == obj as QueryComponent;

        public bool Equals(QueryComponent other)
            => this == other;

        public override int GetHashCode()
            => ((int)Operator)
                ^ (((Prefix?.GetHashCode() & 0x7f) << 4) ?? 0)
                ^ (((Value?.GetHashCode() & 0x7f) << 11) ?? 0)
                ^ ((StartIndex & 0x7f) << 18)
                ^ ((LastIndex & 0x7f) << 25);
    }
}