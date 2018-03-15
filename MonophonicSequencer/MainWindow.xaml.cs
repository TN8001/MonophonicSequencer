using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MonophonicSequencer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //キーボードショートカット登録
            //[Ctrl]+[PageUp] [Ctrl]+[PageDown]をZoomコマンドに割り当てる
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.PageUp, ModifierKeys.Control));
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.PageDown, ModifierKeys.Control));

            InitializeComponent();

            #region AddCommandBindings
            //コマンドを実行メソッドと実行可否判定メソッドに割り当てる
            //それぞれOn○○Executeと Can○○Executeにするのが慣習？だが Executeがくどいので省略
            //まだほとんど未実装
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(NavigationCommands.IncreaseZoom, OnNew, NotCan));
            CommandBindings.Add(new CommandBinding(NavigationCommands.DecreaseZoom, OnNew, NotCan));
            //これだけｗ
            CommandBindings.Add(new CommandBinding(MediaCommands.Play, OnPlay, CanMedia));
            CommandBindings.Add(new CommandBinding(MediaCommands.Pause, OnPause, CanMedia));
            CommandBindings.Add(new CommandBinding(MediaCommands.Stop, OnStop, CanMedia));
            #endregion
        }

        //pianoはxamlで名前を付けたPianoRollControlのインスタンス
        private void OnStop(object sender, ExecutedRoutedEventArgs e) => piano.Stop();
        private void OnPause(object sender, ExecutedRoutedEventArgs e) => piano.Pause();
        private void OnPlay(object sender, ExecutedRoutedEventArgs e) => piano.Play();

        //常に使用可
        private void CanMedia(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        //常に使用不可
        private void NotCan(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
        }

        //アップダウンコントロールをホイールで上下する
        //まだテンポを変える処理は書いていない
        private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0) upDown.Value++;
            else upDown.Value--;
        }
    }
}
