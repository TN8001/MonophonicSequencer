using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MonophonicSequencer.Controls
{
    public class PianoRollGrid : Grid
    {
        private const int KeyCount = 36;
        private static readonly HashSet<int> blackKeys = new HashSet<int> { 1, 3, 5, 8, 10 };
        private static readonly OutputDevice outDevice;// = new OutputDevice(0);
        private static readonly Pen bluePen = new Pen(Brushes.Gray, 1);
        private static readonly Pen grayPen = new Pen(Brushes.Gray, 1);
        private static readonly Pen lightGrayPen = new Pen(Brushes.LightGray, 1);
        private static readonly Pen blackPen = new Pen(Brushes.Black, 1);
        private static readonly Pen transparentPen = new Pen(Brushes.Transparent, 0);

        #region DependencyProperty MeasureCount 小節数
        public int MeasureCount { get => (int)GetValue(MeasureCountProperty); set => SetValue(MeasureCountProperty, value); }
        public static readonly DependencyProperty MeasureCountProperty
            = DependencyProperty.Register(nameof(MeasureCount), typeof(int), typeof(PianoRollGrid),
                new PropertyMetadata(4, OnMeasureCountChanged));
        private static void OnMeasureCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is PianoRollGrid g) g.OnMeasureCountChanged();
        }
        private void OnMeasureCountChanged()
        {
            var c = MeasureCount * 16; // 16分音符のみ
            if(ColumnDefinitions.Count == c) return;

            if(ColumnDefinitions.Count > c)
            {
                while(ColumnDefinitions.Count != c)
                    ColumnDefinitions.RemoveAt(ColumnDefinitions.Count);
            }
            else
            {
                while(ColumnDefinitions.Count != c)
                    ColumnDefinitions.Add(new ColumnDefinition());
            }

            noteArray = ResizeArray(noteArray, ColumnDefinitions.Count, RowDefinitions.Count);
        }
        #endregion

        private byte Note => (byte)(83 - position.Y);
        private Border[,] noteArray = new Border[0, 0];
        private IntPoint position;

        Sequence sequence = new Sequence();
        Sequencer sequencer = new Sequencer();


        static PianoRollGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PianoRollGrid), new FrameworkPropertyMetadata(typeof(PianoRollGrid)));
            if(!DesignerProperties.GetIsInDesignMode(new UIElement()))
                outDevice = new OutputDevice(0);
        }
        public PianoRollGrid()
        {
            while(RowDefinitions.Count != KeyCount)
                RowDefinitions.Add(new RowDefinition());
            OnMeasureCountChanged();

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

            //sequence.LoadProgressChanged += HandleLoadProgressChanged;
            //sequence.LoadCompleted += HandleLoadCompleted;

            //sequence.Format = 1;
            //// 
            //// sequencer1
            //// 
            //sequencer.Position = 0;
            //sequencer.Sequence = sequence;
            //sequencer.PlayingCompleted += Sequencer_PlayingCompleted;
            //sequencer.ChannelMessagePlayed += Sequencer_ChannelMessagePlayed;
            ////sequencer.SysExMessagePlayed += Sequencer_SysExMessagePlayed;
            //sequencer.Chased += Sequencer_Chased;
            //sequencer.Stopped += Sequencer_Stopped;

            //sequence.LoadAsync("noname.mid");
            //sequencer.Sequence = sequence;
            //sequencer.Start();


            //Sequencer player = new Sequencer();
            //Sequence sequence = new Sequence("noname.mid");
            //Track track = sequence[0];

            //foreach(MidiEvent midiEvent in track.Iterator())
            //{
            //    IMidiMessage mess = midiEvent.MidiMessage;
            //    if(mess.MessageType == MessageType.Channel)
            //    {
            //        ChannelMessage chanMess = (ChannelMessage)mess;
            //        if(chanMess.Command == ChannelCommand.NoteOn)
            //        {
            //            Console.WriteLine(chanMess.Data1);
            //        }
            //    }
            //}
            //m();
        }

        private void Sequencer_Stopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Sequencer_Stopped");
        }

        private void Sequencer_Chased(object sender, ChasedEventArgs e)
        {
            Debug.WriteLine("Sequencer_Chased");
        }

        private void Sequencer_SysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
            Debug.WriteLine("Sequencer_SysExMessagePlayed");
        }

        private void Sequencer_ChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            Debug.WriteLine("Sequencer_ChannelMessagePlayed");
            outDevice.Send(e.Message);
        }

        private void Sequencer_PlayingCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("Sequencer_PlayingCompleted");
        }

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            sequencer.Start();

            Debug.WriteLine("HandleLoadCompleted");
        }

        private void HandleLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("HandleLoadProgressChanged");
        }



        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if(!outDevice.IsDisposed)
                outDevice.Dispose();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            RenderGrid(dc);
        }
        private void RenderGrid(DrawingContext dc)
        {
            var index = 0;
            foreach(var row in RowDefinitions)
            {
                if(blackKeys.Contains(index % 12))
                {
                    dc.DrawRectangle(Brushes.WhiteSmoke, transparentPen,
                        new Rect(0, row.Offset, ActualWidth, row.ActualHeight));
                }

                var p1 = new Point(0, row.Offset);
                var p2 = new Point(ActualWidth, row.Offset);
                if(index % 12 == 0)
                    dc.DrawLine(blackPen, p1, p2);
                else
                    dc.DrawLine(lightGrayPen, p1, p2);

                index++;
            }

            index = 0;
            foreach(var column in ColumnDefinitions)
            {
                var p1 = new Point(column.Offset, 0);
                var p2 = new Point(column.Offset, ActualHeight);

                if(index % 16 == 0)
                    dc.DrawLine(blackPen, p1, p2);
                else if(index % 4 == 0)
                    dc.DrawLine(grayPen, p1, p2);
                else
                    dc.DrawLine(lightGrayPen, p1, p2);

                index++;
            }

            dc.DrawRectangle(Brushes.Transparent, blackPen,
                new Rect(0, 0, ActualWidth, ActualHeight));
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            Play(GetCellNumber());
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(e.LeftButton != MouseButtonState.Pressed) return;

            var p = GetCellNumber();
            if(p == position) return;

            Play(p);
        }
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);

            RemoveNote(GetCellNumber());
        }

        private IntPoint GetCellNumber()
        {
            var p = new IntPoint();
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

            return p;
        }
        private void Play(IntPoint p)
        {
            for(var i = 0; i < noteArray.GetLength(1); i++)
                RemoveNote(p.X, i);

            AddNote(p);

            NoteOn();
            position = p;
            NoteOff();
        }
        private void AddNote(IntPoint point) => AddNote(point.X, point.Y);
        private void AddNote(int column, int row)
        {
            var b = new Border { Background = Brushes.Blue };
            SetColumn(b, column);
            SetRow(b, row);

            Children.Add(b);
            noteArray[column, row] = b;
        }
        private void RemoveNote(IntPoint point) => RemoveNote(point.X, point.Y);
        private void RemoveNote(int column, int row)
        {
            var note = noteArray[column, row];
            if(note == null) return;

            Children.Remove(note);
            noteArray[column, row] = null;
        }
        private void NoteOn() => outDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, Note, 127));
        private void NoteOff() => outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, Note, 0));

        //private void Init()
        //{
        //    RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        //    var b = new Border();
        //    b.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/MonophonicSequencer;component/IMG_1771.PNG")));
        //    //b.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/IMG_1771.PNG")));
        //    SetRow(b, 10);
        //    SetColumn(b, 0);
        //    SetRowSpan(b, 12);
        //    b.Width = 30;
        //    Children.Add(b);

        //}



        private void m()
        {
            Sequence sequence = new Sequence();
            Sequencer sequencer = new Sequencer();
            // Track  
            Track track = new Track();

            // Tempo
            TempoChangeBuilder tempoBuilder = new TempoChangeBuilder();
            tempoBuilder.Tempo = 100;
            tempoBuilder.Build();
            track.Insert(0, tempoBuilder.Result);

            ChannelMessageBuilder channelBuilder = new ChannelMessageBuilder();

            //// Choix Intrument
            //channelBuilder.MidiChannel = 0;
            //channelBuilder.Command = ChannelCommand.ProgramChange;
            //channelBuilder.Data1 = 0;
            //channelBuilder.Data2 = 0;
            //channelBuilder.Build();
            //track.Insert(0, channelBuilder.Result);

            // Jouer note
            channelBuilder.MidiChannel = 0;
            channelBuilder.Command = ChannelCommand.NoteOn;
            channelBuilder.Data1 = 60;
            channelBuilder.Data2 = 127;
            channelBuilder.Build();
            track.Insert(0, channelBuilder.Result);

            // Arrêter note
            channelBuilder.MidiChannel = 0;
            channelBuilder.Command = ChannelCommand.NoteOff;
            channelBuilder.Data1 = 60;
            channelBuilder.Data2 = 0;
            channelBuilder.Build();
            track.Insert(479, channelBuilder.Result);

            track.EndOfTrackOffset = 480;

            sequence.Add(track);
            sequence.Format = 0;

            sequencer.Position = 0;
            sequencer.Sequence = sequence;
            sequence.Save("test.mid");
            sequencer.Start();
        }

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
