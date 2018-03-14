using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MonophonicSequencer.Controls
{
    public class Piano
    {
        public static int GetFirstNote(DependencyObject obj) => (int)obj.GetValue(FirstNoteProperty);
        public static void SetFirstNote(DependencyObject obj, int value) => obj.SetValue(FirstNoteProperty, value);
        public static readonly DependencyProperty FirstNoteProperty
            = DependencyProperty.RegisterAttached("FirstNote", typeof(int), typeof(Piano),
                new FrameworkPropertyMetadata(60, FrameworkPropertyMetadataOptions.Inherits, OnFirstNoteChanged, CoerceFirstNote));
        private static void OnFirstNoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        private static object CoerceFirstNote(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            if(value < 0) value = 0;
            if(value > 127) value = 127;
            return value;
        }
    }

    public class PianoKey : ContentControl
    {
        #region DependencyProperty
        #region Note
        [Category("Appearance")]
        public int Note { get => (int)GetValue(NoteProperty); set => SetValue(NoteProperty, value); }
        public static readonly DependencyProperty NoteProperty
            = DependencyProperty.Register(nameof(Note), typeof(int), typeof(PianoKey),
                new FrameworkPropertyMetadata(-1, OnNoteChanged, CoerceNote));
        private static void OnNoteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (PianoKey)d;
            ctrl.OnNoteChanged(e);
        }
        protected virtual void OnNoteChanged(DependencyPropertyChangedEventArgs e)
        {
            if((int)e.NewValue < 0) SetInnerNote();
            else innerNote = (int)e.NewValue;
        }

        private void SetInnerNote()
        {
            var n = Parent as Panel;
            if(n == null) throw new InvalidOperationException("AutoNoteMode(-1)は、パネルの直下に配置する必要があります。");

            var i = n.Children.IndexOf(this);
            var f = Piano.GetFirstNote(this);
            var value = i + f;
            if(value < 0) value = 0;
            if(value > 127) value = 127;
            innerNote = value;
        }
        private static object CoerceNote(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            if(value < -1) value = -1;
            if(value > 127) value = 127;
            return value;
        }
        #endregion
        #region IsPressed
        [Category("Appearance"), ReadOnly(true)]
        public bool IsPressed { get => (bool)GetValue(IsPressedProperty); protected set => SetValue(IsPressedPropertyKey, value); }
        internal static readonly DependencyPropertyKey IsPressedPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsPressed), typeof(bool), typeof(PianoKey),
                new FrameworkPropertyMetadata(false, OnIsPressedChanged));
        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;
        private static void OnIsPressedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (PianoKey)d;
            ctrl.OnIsPressedChanged(e);
        }
        protected virtual void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue) NoteOn();
            else NoteOff();
        }
        #endregion
        #endregion

        private int innerNote;

        static PianoKey()
        {
            BackgroundProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(Brushes.Snow));
            BorderBrushProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(Brushes.Black));
            BorderThicknessProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(new Thickness(2.0)));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(typeof(PianoKey)));
            FontSizeProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(10.0));
            HeightProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(160.0));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
            PaddingProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(new Thickness(2.0)));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(VerticalAlignment.Bottom));
            WidthProperty.OverrideMetadata(typeof(PianoKey), new FrameworkPropertyMetadata(50.0));
        }
        public PianoKey()
        {
        }
        private static OutputDevice midiOut = MidiOut.Instance.Device; // 初期化に時間がかかるため
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(Note < 0) SetInnerNote();
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsPressed = true;
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            IsPressed = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if(Mouse.LeftButton == MouseButtonState.Pressed)
                IsPressed = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            IsPressed = false;
        }

        private void NoteOn()
        {
            Debug.WriteLine(innerNote);
            midiOut.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, innerNote, 127));
        }

        private void NoteOff() => midiOut.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, innerNote, 0));
    }
}
