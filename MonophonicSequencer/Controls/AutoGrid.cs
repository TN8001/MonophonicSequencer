using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System;
using System.Collections.Generic;


namespace MonophonicSequencer.Controls
{
    ///<summary>バインドできるRowCount ColumnCountで、行 列を拡縮するGridコントロール</summary>
    public class AutoGrid : Grid
    {
        //DependencyProperty（依存関係プロパティ）はxaml上でバインドに使用する特別なプロパティシステム
        #region DependencyProperty ColumnCount 

        //Description Categoryはデザイナのプロパティ表示で使用する設定
        [Description("列数を設定します。"), Category(nameof(CategoryAttribute.Layout))]

        //ColumnCountは通常のプロパティ構文だが、バインド経由で変更された場合は通らない
        //通常はGetValue SetValueの呼び出しのみにする
        public int ColumnCount { get => (int)GetValue(ColumnCountProperty); set => SetValue(ColumnCountProperty, value); }

        //GetValue SetValueで使用されるDependencyPropertyの実体
        public static readonly DependencyProperty ColumnCountProperty
            = DependencyProperty.Register(nameof(ColumnCount), typeof(int), typeof(AutoGrid),
                //デフォルト値、変更コールバック、強制コールバック
                new PropertyMetadata(1, OnColumnCountChanged, CoerceCount));

        //変更コールバック プロパティのsetにあたるもの
        //dに自身が渡されるのでキャストして必要な処理をする
        private static void OnColumnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //必ずしもインスタンスメソッドに回す必要はないが、
            //利便性のためこのような実装なっていることが多い
            if(d is AutoGrid g) g.OnColumnCountChanged((int)e.OldValue, (int)e.NewValue);
        }

        protected virtual void OnColumnCountChanged(int oldValue, int newValue)
        {
            //初期化前に呼ばれた場合は処理しない
            if(!IsInitialized) return;

            Debug.WriteLine("OnColumnCountChanged");
            if(ColumnDefinitions.Count == newValue) return;

            if(ColumnDefinitions.Count > newValue) // 縮小
            {
                //列を削除
                while(ColumnDefinitions.Count > newValue)
                    ColumnDefinitions.RemoveAt(ColumnDefinitions.Count - 1);

                //はみ出した要素を削除するモードなら
                if(DeleteOverflowElements)
                {
                    // はみ出した要素の一時避難
                    var tmp = new List<UIElement>();

                    //ループ中に中の要素は削除できないのでいったん配列にコピーし
                    //コピーのほうでループを回す
                    foreach(var e in Children.OfType<UIElement>().ToArray())
                    {
                        if(GetColumn(e) > newValue - 1)
                        {
                            // はみ出した要素を削除
                            Children.Remove(e);
                            tmp.Add(e);
                        }
                    }

                    //イベント発砲メソッドを呼び出し 削除した要素をイベント引数として渡す
                    OnOverflowElementsDeleted(new OverflowElementsDeletedEventArgs(tmp));
                }
            }
            else // 拡大
            {
                //拡大は単に列を追加するだけ 分割はStar（均等分割）のみ
                while(ColumnDefinitions.Count < newValue)
                    ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        //強制コールバック 値に制約をつける場合に使用
        //今回は分割数を指定する値なので１以下にはならないようにする
        private static object CoerceCount(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            return value < 1 ? 1 : value;
        }
        #endregion

        //ColumnCountとほぼ同じ
        #region DependencyProperty RowCount 
        [Description("行数を設定します。"), Category(nameof(CategoryAttribute.Layout))]
        public int RowCount { get => (int)GetValue(RowCountProperty); set => SetValue(RowCountProperty, value); }
        public static readonly DependencyProperty RowCountProperty
            = DependencyProperty.Register(nameof(RowCount), typeof(int), typeof(AutoGrid),
                new PropertyMetadata(1, OnRowCountChanged, CoerceCount));
        private static void OnRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is AutoGrid g) g.OnRowCountChanged((int)e.OldValue, (int)e.NewValue);
        }
        protected virtual void OnRowCountChanged(int oldValue, int newValue)
        {
            if(!IsInitialized) return;

            Debug.WriteLine("OnRowCountChanged");
            if(RowDefinitions.Count == newValue) return;

            if(RowDefinitions.Count > newValue) // 縮小
            {
                while(RowDefinitions.Count > newValue)
                    RowDefinitions.RemoveAt(RowDefinitions.Count - 1);

                if(DeleteOverflowElements)
                {
                    foreach(UIElement e in Children.OfType<UIElement>().ToArray())
                        if(GetRow(e) > newValue - 1) Children.Remove(e);
                }
            }
            else // 拡大
            {
                while(RowDefinitions.Count < newValue)
                    RowDefinitions.Add(new RowDefinition());
            }
        }
        #endregion

        #region DependencyProperty DeleteOverflowElements
        [Description("分割が減った時に、はみ出した要素を削除するかどうかを設定します。"), Category(nameof(CategoryAttribute.Layout))]

        //デフォルトで はみ出した要素を削除すると混乱するかもしれないので
        //明示的に指定してもらう
        //バインドでOnOffするような使い方は思いつかないので
        //依存関係プロパティにせずに通常のプロパティでもいいかもしれない。。
        public bool DeleteOverflowElements { get => (bool)GetValue(DeleteOverflowElementsProperty); set => SetValue(DeleteOverflowElementsProperty, value); }
        public static readonly DependencyProperty DeleteOverflowElementsProperty
            = DependencyProperty.Register(nameof(DeleteOverflowElements), typeof(bool), typeof(AutoGrid),
                new PropertyMetadata(false));
        #endregion

        ///<summary>列または行の減少で、はみ出た要素が削除されたときに発生します。</summary>
        public event OverflowElementsDeletedHandler OverflowElementsDeleted;

        //イベント発砲メソッド
        protected virtual void OnOverflowElementsDeleted(OverflowElementsDeletedEventArgs e)
            => OverflowElementsDeleted?.Invoke(this, e);


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            //初期化が終わった後に適用
            //xamlで事前にヘッダ等を入れれるような仕様
            OnRowCountChanged(RowDefinitions.Count, RowCount);
            OnColumnCountChanged(ColumnDefinitions.Count, ColumnCount);
        }
    }
}
