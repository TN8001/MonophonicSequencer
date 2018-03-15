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
    public class PianoRollGrid : Grid
    {
        private const int KeyCount = 36;
        private static readonly HashSet<int> blackKeys = new HashSet<int> { 1, 3, 5, 8, 10 };

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
            var c = MeasureCount * 16;
            if(ColumnDefinitions.Count == c) return;

            if(ColumnDefinitions.Count > c) // 譜面縮小
            {
                // はみ出した要素を削除
                for(var i = c; i < noteArray.GetLength(0); i++)
                {
                    if(measureText[i] != null)
                        Children.Remove(measureText[i]);
                    for(var j = 0; j < noteArray.GetLength(1); j++)
                        Children.Remove(noteArray[i, j]);
                }

                while(ColumnDefinitions.Count != c)
                    ColumnDefinitions.RemoveAt(ColumnDefinitions.Count);
                IsDirty = true;
            }
            else // 譜面拡大
            {
                while(ColumnDefinitions.Count != c)
                    ColumnDefinitions.Add(new ColumnDefinition());
            }
            noteArray = ResizeArray(noteArray, c, RowDefinitions.Count);
            Array.Resize(ref measureText, c);
            AddMeasureText();

            SetColumnSpan(line, MeasureCount * 16);
        }

        private void AddMeasureText()
        {
            for(var i = 0; i < measureText.GetLength(0); i += 16)
            {
                if(measureText[i] != null) continue;

                var b = new TextBlock { Text = $"{i / 16 + 1}", FontSize = 20 };
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
        static PianoRollGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PianoRollGrid), new FrameworkPropertyMetadata(typeof(PianoRollGrid)));
        }
        public PianoRollGrid()
        {
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            while(RowDefinitions.Count != KeyCount + 1)
                RowDefinitions.Add(new RowDefinition());

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

        #region OnRender
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            RenderGrid(dc);
        }
        private void RenderGrid(DrawingContext dc)
        {
            RenderBlackKeys(dc);
            RenderLightGrayLine(dc);
            RenderGrayLine(dc);
            RenderBlackLine(dc);

            var rect = new Rect(0, 0, ActualWidth, ActualHeight);
            dc.DrawRectangle(Brushes.Transparent, Pens.Black, rect); // 全体囲む四角 黒
        }
        private void RenderBlackKeys(DrawingContext dc) // 黒鍵 極薄グレー
        {
            var index = 0;
            foreach(var row in RowDefinitions)
            {
                if(!blackKeys.Contains((index++ % 12) - 1)) continue;

                var rect = new Rect(0, row.Offset, ActualWidth, row.ActualHeight);
                dc.DrawRectangle(Brushes.WhiteSmoke, Pens.Transparent, rect);
            }
        }
        private void RenderLightGrayLine(DrawingContext dc) // 基本区切り線 薄グレー
        {
            foreach(var row in RowDefinitions)
                DrawLine(dc, Pens.LightGray, 0, row.Offset, ActualWidth, row.Offset);

            foreach(var column in ColumnDefinitions)
                DrawLine(dc, Pens.LightGray, column.Offset, 0, column.Offset, ActualHeight);
        }
        private void RenderGrayLine(DrawingContext dc) // 4分音符区切り線 グレー
        {
            var index = 0;
            foreach(var column in ColumnDefinitions)
            {
                if(index++ % 4 != 0) continue;

                DrawLine(dc, Pens.Gray, column.Offset, 0, column.Offset, ActualHeight);
            }
        }
        private void RenderBlackLine(DrawingContext dc) // 小節・オクターブ区切り線 黒
        {
            var index = 0;
            foreach(var column in ColumnDefinitions)
            {
                if(index++ % 16 != 0) continue;

                DrawLine(dc, Pens.Black, column.Offset, 0, column.Offset, ActualHeight);
            }

            index = 0;
            foreach(var row in RowDefinitions)
            {
                if(index++ % 12 != 1) continue;

                DrawLine(dc, Pens.Black, 0, row.Offset, ActualWidth, row.Offset);
            }
        }
        private void DrawLine(DrawingContext dc, Pen pen, double x1, double x2, double y1, double y2)
            => dc.DrawLine(pen, new Point(x1, x2), new Point(y1, y2));
        #endregion

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
