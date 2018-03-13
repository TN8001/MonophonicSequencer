using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace MonophonicSequencer.Controls
{
    public partial class PianoRollControl : UserControl
    {
        private MidiOut m = new MidiOut();
        private DispatcherTimer timer = new DispatcherTimer();
        private Player player = new Player();
        public PianoRollControl()
        {
            InitializeComponent();
            player.PlayingCompleted += Player_PlayingCompleted;
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += (s, e) =>
            {
                piano.Offset2 = player.Position;
                Debug.WriteLine(player.Position);
            };
            piano.MidiOut = m;
            player.MidiOut = m;
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("ThreadID : " + id);

        }

        private void Player_PlayingCompleted(object sender, EventArgs e)
        {
            int id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine("ThreadID : " + id);

            piano.Offset2 = 0;
            timer.Stop();
        }

        public void Play()
        {
            if(piano.IsDirty)
            {
                var n = piano.GetTrack();
                player.Load(n);
            }
            player.Play();
            timer.Start();
        }
        public void Pause()
        {
            player.Pause();
            timer.Stop();
        }
        public void Stop()
        {
            player.Stop();
            timer.Stop();
        }
    }
}
