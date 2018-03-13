using Sanford.Multimedia.Midi;
using System;
using System.ComponentModel;
using System.Windows;

namespace MonophonicSequencer.Controls
{
    public class MidiOut : UIElement // Dispatcher使いたい為 雑い
    {
        public OutputDevice Device => outDevice;
        private OutputDevice outDevice;// = new OutputDevice(0);

        public MidiOut()
        {
            if(!DesignerProperties.GetIsInDesignMode(this))
                outDevice = new OutputDevice(0);

            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            if(!outDevice.IsDisposed)
                outDevice.Dispose();
        }
    }
}
