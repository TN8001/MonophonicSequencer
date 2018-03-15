using System.Windows;
using System;
using System.Collections.Generic;


namespace MonophonicSequencer.Controls
{
    ///<summary>OverflowElementsDeletedイベントを受け取るハンドラに使用します。</summary>
    public delegate void OverflowElementsDeletedHandler(object sender, OverflowElementsDeletedEventArgs e);

    ///<summary>OverflowElementsDeletedイベントの引数です。</summary>
    public class OverflowElementsDeletedEventArgs : EventArgs
    {
        ///<summary>削除された要素のリスト</summary>
        public IList<UIElement> Elements { get; }

        public OverflowElementsDeletedEventArgs(IList<UIElement> elements)
            => Elements = elements;
    }
}
