using MonophonicSequencer.Resources;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MonophonicSequencer.Controls
{
    public class PianoRollGrid : AutoGrid
    {
        private const int KeyCount = 36;
        private const int Division = 16;

        #region DependencyProperty MeasureCount 小節数
        public int MeasureCount { get => (int)GetValue(MeasureCountProperty); set => SetValue(MeasureCountProperty, value); }
        public static readonly DependencyProperty MeasureCountProperty
            = DependencyProperty.Register(nameof(MeasureCount), typeof(int), typeof(PianoRollGrid),
                new PropertyMetadata(16, OnMeasureCountChanged));
        private static void OnMeasureCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is PianoRollGrid g) g.OnMeasureCountChanged();
        }
        private void OnMeasureCountChanged()
        {
            var c = MeasureCount * Division;
            if(ColumnDefinitions.Count == c) return;

            ColumnCount = c + 1;

            noteArray = ResizeArray(noteArray, c, RowDefinitions.Count);
            Array.Resize(ref measureText, c);
            AddMeasureText();

            SetColumnSpan(line, MeasureCount * Division);
        }

        private void AddMeasureText()
        {
            for(var i = 0; i < measureText.GetLength(0); i += Division)
            {
                if(measureText[i] != null) continue;

                var b = new TextBlock
                {
                    Text = "" + (i / Division + 1),
                    FontSize = 20,
                    Margin = new Thickness(5, 0, 0, 0),
                };
                SetColumn(b, i);
                SetColumnSpan(b, 4);
                SetRow(b, 0);

                Children.Add(b);
                measureText[i] = b;
            }
        }
        #endregion
        public double Offset
        {
            get => line.Margin.Left;
            set => line.Margin = new Thickness(value, 0, 0, 0);
        }
        public int Offset2
        {
            set
            {
                value -= 2;
                if(value < 0) value = 0;
                var col = ColumnDefinitions[value / 6];
                var delta = (value % 6) * (col.ActualWidth / 6);
                Offset = col.Offset + delta;
                Debug.WriteLine($"c:{value / 6} n:{col.Offset} Offset:{Offset} value:{value}");
            }
        }

        public bool IsDirty { get; private set; }
        public MidiOut MidiOut { get; set; }

        private byte note => (byte)(83 - position.Y);
        private Border[,] noteArray = new Border[0, 0];
        private TextBlock[] measureText = new TextBlock[0];
        private (int X, int Y) position;
        private Border line;
        private Border measurer;
        private GridRenderer renderer;

        static PianoRollGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PianoRollGrid), new FrameworkPropertyMetadata(typeof(PianoRollGrid)));
        }
        public PianoRollGrid()
        {
            renderer = new GridRenderer(this);

            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            RowCount = KeyCount + 1;

            AddLine();
            measurer = new Border { Visibility = Visibility.Hidden, };
            Children.Add(measurer);

            OnMeasureCountChanged();
        }
        public IEnumerable<(int index, int note)> GetTrack()
        {
            IsDirty = false;
            return noteArray.Select((n, x, y) => (n, x, y))
                            .Where(x => x.n != null)
                            .Select(x => (x.x, 83 - x.y));
        }

        private void AddLine()
        {
            line = new Border
            {
                Background = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 2,
            };

            SetColumn(line, 0);
            SetRow(line, 0);
            SetRowSpan(line, 37);
            SetZIndex(line, 100);
            Children.Add(line);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            renderer.OnRender(dc);
        }

        private double GetMeasurerX(int col)
        {
            SetColumn(measurer, col);
            UpdateLayout();
            var p = measurer.PointToScreen(new Point(0, 0));
            return PointFromScreen(p).X - 1;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            e.Handled = true;

            var p = GetCellNumber();
            if(p.Y == -1) // 小節ヘッダー
                Offset = GetMeasurerX(p.X);
            else
                PlayNote(p);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if(e.LeftButton != MouseButtonState.Pressed) return;
            var p = GetCellNumber();
            if(p.X == position.X && p.Y == position.Y) return;

            PlayNote(p);
        }
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            RemoveNote(GetCellNumber());
        }

        private (int X, int Y) GetCellNumber()
        {
            var p = (X: 0, Y: -1);
            var height = 0.0;
            var width = 0.0;
            var point = Mouse.GetPosition(this);

            foreach(var columnDefinition in ColumnDefinitions)
            {
                width += columnDefinition.ActualWidth;
                if(width >= point.X) break;

                p.X++;
            }
            foreach(var rowDefinition in RowDefinitions)
            {
                height += rowDefinition.ActualHeight;
                if(height >= point.Y) break;

                p.Y++;
            }
            Debug.WriteLine(p);
            return p;
        }
        private void PlayNote((int X, int Y) p)
        {
            if(p.Y == -1) return; // 小節ヘッダー

            for(var i = 0; i < noteArray.GetLength(1); i++) // 単音のため同カラムのnoteを削除
                RemoveNote(p.X, i);

            AddNote(p);

            NoteOff();
            position = p;
            NoteOn();
            Debug.WriteLine(note);
        }
        private void AddNote((int X, int Y) point) => AddNote(point.X, point.Y);
        private void AddNote(int column, int row)
        {
            var border = new Border { Background = Brushes.Blue };
            SetColumn(border, column);
            SetRow(border, row + 1);

            Children.Add(border);
            noteArray[column, row] = border;
            IsDirty = true;
        }
        private void RemoveNote((int X, int Y) point) => RemoveNote(point.X, point.Y);
        private void RemoveNote(int column, int row)
        {
            var note = noteArray[column, row];
            if(note == null) return;

            Children.Remove(note);
            noteArray[column, row] = null;
            IsDirty = true;
        }
        private void NoteOn() => MidiOut?.Device?.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, note, 127));
        private void NoteOff() => MidiOut?.Device?.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, note, 0));



        private static T[,] ResizeArray<T>(T[,] original, int x, int y)
        {
            var newArray = new T[x, y];
            var minX = Math.Min(original.GetLength(0), newArray.GetLength(0));
            var minY = Math.Min(original.GetLength(1), newArray.GetLength(1));

            for(var i = 0; i < minY; ++i)
                Array.Copy(original, i * original.GetLength(0), newArray, i * newArray.GetLength(0), minX);

            return newArray;
        }
    }
}
