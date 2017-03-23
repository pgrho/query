using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.Querying
{
    public sealed class QueryComponent
    {
        private sealed class QueryParser
        {
            private bool _IsPrefix;
            private bool _IsQuoted;
            private string _Prefix;
            private StringBuilder _Buffer;
            private bool? _Condition;
            private int _StartIndex;

            public IEnumerable<QueryComponent> Parse(string query)
            {
                if (query == null)
                {
                    yield break;
                }

                var position = 0;
                ClearState(0);

                foreach (var c in query)
                {
                    position++;

                    if (IsEmpty())
                    {
                        switch (c)
                        {
                            case '+':
                                _Condition = true;
                                continue;

                            case '-':
                                _Condition = false;
                                continue;
                        }
                    }
                    if (_IsPrefix)
                    {
                        if (IsWhitespace(c))
                        {
                            if (_Buffer.Length > 0)
                            {
                                yield return CreateQueryComponent(position);
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
                                yield return CreateQueryComponent(position);
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
                => _Condition == null && _IsPrefix && _Buffer.Length == 0;

            private static bool IsWhitespace(char c)
                => char.IsWhiteSpace(c);

            private void ClearState(int position)
            {
                _StartIndex = position + 1;
                _Condition = null;
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
                => new QueryComponent(_Condition, _Prefix, _Buffer.ToString(), _StartIndex, position);
        }

        public QueryComponent(bool? condition, string prefix, string value, int startIndex, int lastIndex)
        {
            Condition = condition;
            Prefix = prefix;
            Value = value;
            StartIndex = startIndex;
            LastIndex = lastIndex;
        }

        public bool? Condition { get; }

        public string Prefix { get; }
        public string Value { get; }

        public int StartIndex { get; }
        public int LastIndex { get; }

        public int Length
            => LastIndex - StartIndex + 1;

        public static IEnumerable<QueryComponent> Parse(string query)
            => string.IsNullOrWhiteSpace(query) ? Enumerable.Empty<QueryComponent>() : new QueryParser().Parse(query);
    }
}
