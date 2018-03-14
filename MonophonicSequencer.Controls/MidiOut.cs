using Sanford.Multimedia.Midi;
using System;
using System.ComponentModel;
using System.Windows;

namespace MonophonicSequencer.Controls
{
    public sealed class MidiOut
    {
        public static MidiOut Instance { get; } = new MidiOut();

        public OutputDevice Device => outDevice;
        private OutputDevice outDevice;// = new OutputDevice(0);

        private MidiOut()
        {
            if(!DesignerProperties.GetIsInDesignMode(new UIElement()))
                outDevice = new OutputDevice(0);

            Application.Current.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if(!outDevice.IsDisposed)
                outDevice.Dispose();
        }
    }
}
