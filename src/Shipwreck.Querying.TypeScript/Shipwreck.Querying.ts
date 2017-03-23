module Shipwreck.Querying {
    /**
     * クエリーコンポーネントの処理方法です。
     */
    export enum ComponentOperator {
        /**
         * 演算子が指定されていません。
         */
        None,

        /**
         * 必須項目です。
         */
        Required,

        /**
         * 除外項目です。
         */
        Excluded
    }

    class QueryParser {
        private isPrefix: boolean;
        private isQuoted: boolean;
        private prefix: string;
        private buffer: string;
        private operator: ComponentOperator;
        private startIndex: number;

        public parse(query: string): QueryComponent[] {
            var r: QueryComponent[] = [];

            if (query === null || query === undefined) {
                return r;
            }

            this.clearState(-1);

            for (var i = 0; i < query.length; i++) {
                var c = query.charAt(i);

                if (this.IsEmpty()) {
                    switch (c) {
                        case '+':
                            this.operator = ComponentOperator.Required;
                            continue;

                        case '-':
                            this.operator = ComponentOperator.Excluded;
                            continue;
                    }
                }

                if (this.isPrefix) {
                    if (QueryParser.isWhiteSpace(c)) {
                        if (this.buffer.length > 0) {
                            r.push(this.createQueryComponent(i - 1));
                        }
                        this.clearState(i);
                    } else if (c === '"') {
                        this.prefix = null;
                        this.isPrefix = false;
                        this.isQuoted = this.buffer.length === 0;
                    } else if (c === ':') {
                        if (this.buffer.length == 0) {
                            this.buffer += c;
                        } else {
                            this.prefix = this.buffer;
                            this.buffer = "";
                        }
                        this.isPrefix = false;
                        this.isQuoted = false;
                    } else {
                        this.buffer += c;
                    }
                } else {
                    if (QueryParser.isWhiteSpace(c)) {
                        if (this.isQuoted) {
                            this.buffer += c;
                        } else {
                            r.push(this.createQueryComponent(i - 1));
                            this.clearState(i);
                        }
                    } else if (c === '"') {
                        if (this.isQuoted) {
                            if (this.prefix != null || this.buffer.length > 0) {
                                r.push(this.createQueryComponent(i));
                            }
                            this.clearState(i);
                        } else if (this.buffer.length === 0) {
                            this.isQuoted = true;
                        } else {
                            this.buffer += c;
                        }
                    } else {
                        this.buffer += c;
                    }
                }
            }

            if (!this.isPrefix || this.buffer.length > 0) {
                r.push(this.createQueryComponent(query.length - 1));
            }

            return r;
        }

        private IsEmpty() {
            return this.operator == ComponentOperator.None && this.isPrefix && this.buffer.length == 0;
        }

        private static isWhiteSpace(c: string) {
            return /^[\r\n\s\t]$/.test(c);
        }

        private clearState(position: number) {
            this.startIndex = position + 1;
            this.operator = ComponentOperator.None;
            this.prefix = null;
            this.isPrefix = true;
            this.isQuoted = false;
            this.buffer = "";
        }

        private createQueryComponent(position: number) {
            return new QueryComponent(this.operator, this.prefix, this.buffer, this.startIndex, position);
        }
    }
    /**
     * 検索クエリーに含まれるコンポーネントを表します。
     */
    export class QueryComponent {
        /**
         * コンポーネントの処理方法です。
         */
        public operator: ComponentOperator;

        /**
         * コンポーネントの種別です。
         */
        public prefix: string;

        /**
         * コンポーネントで指定された文字列です。
         */
        public value: string;

        /**
         * コンポーネントの先頭の文字列内の0から始まるインデックスです。
         */
        public startIndex: number;

        /**
         * コンポーネントの末尾の文字列内の0から始まるインデックスです。
         */
        public lastIndex: number;

        constructor(condition: ComponentOperator, prefix: string, value: string, startIndex: number, lastIndex: number) {
            this.operator = condition;
            this.prefix = prefix === null || prefix === undefined ? "" : prefix;
            this.value = value === null || value === undefined ? "" : value;
            this.startIndex = startIndex;
            this.lastIndex = lastIndex;
        }

        /**
         * コンポーネントの文字数を取得します。
         */
        public get length(): number {
            return this.lastIndex - this.startIndex + 1;
        }

        /**
         * 指定した文字列に含まれるコンポーネントの配列を返します。
         * @param query 検索文字列。
         */
        public static parse(query: string): QueryComponent[] {
            return query === null || query === undefined || /^[\s\r\n\t]+$/.test(query) ? []
                : new QueryParser().parse(query);
        }
    }
}