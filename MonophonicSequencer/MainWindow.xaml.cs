using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonophonicSequencer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.PageUp, ModifierKeys.Control));
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.PageDown, ModifierKeys.Control));

            InitializeComponent();

            #region AddCommandBindings
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
            CommandBindings.Add(new CommandBinding(MediaCommands.Play, OnPlay, CanMedia));
            CommandBindings.Add(new CommandBinding(MediaCommands.Pause, OnPause, CanMedia));
            CommandBindings.Add(new CommandBinding(MediaCommands.Stop, OnStop, CanMedia));
            #endregion
        }

        private void OnStop(object sender, ExecutedRoutedEventArgs e) => piano.Stop();
        private void OnPause(object sender, ExecutedRoutedEventArgs e) => piano.Pause();
        private void OnPlay(object sender, ExecutedRoutedEventArgs e) => piano.Play();
        private void CanMedia(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NotCan(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = false;

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            piano.Play();
            Debug.WriteLine("NewExecute");
        }


        private void NumericUpDown_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0) upDown.Value++;
            else upDown.Value--;
        }
    }
}
