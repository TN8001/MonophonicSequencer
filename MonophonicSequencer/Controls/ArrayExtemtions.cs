// (c) Gushwell 2010
// http://gushwell.ldblog.jp/archives/52103538.html


using System;
using System.Collections.Generic;
using System.Linq;

namespace MonophonicSequencer.Controls
{
    public static class ArrayExtemtions
    {
        /// <summary>シーケンスの各要素を新しいフォームに射影します。</summary>
        /// <param name="source">変換関数を呼び出す対象となる値のシーケンス。</param>
        /// <param name="selector">各要素に適用する変換関数。</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static IEnumerable<TResult> Select<TSource, TResult>(this TSource[,] source, Func<TSource, int, int, TResult> selector)
        {
            if(source == null) throw new ArgumentNullException(nameof(source));
            if(selector == null) throw new ArgumentNullException(nameof(selector));

            return from x in Enumerable.Range(0, source.GetLength(0))
                   from y in Enumerable.Range(0, source.GetLength(1))
                   select selector(source[x, y], x, y);
        }
    }
}
