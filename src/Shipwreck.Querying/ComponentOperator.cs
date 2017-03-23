namespace Shipwreck.Querying
{
    /// <summary>
    /// クエリーコンポーネントの処理方法です。
    /// </summary>
    public enum ComponentOperator
    {
        /// <summary>
        /// 演算子が指定されていません。
        /// </summary>
        None,

        /// <summary>
        /// 必須項目です。
        /// </summary>
        Required,

        /// <summary>
        /// 除外項目です。
        /// </summary>
        Excluded
    }
}