using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace MonophonicSequencer.Controls
{
    public class Player
    {
        public MidiOut MidiOut { get; set; }

        private Sequence sequence = new Sequence();
        private Sequencer sequencer = new Sequencer();
        private bool isPlaying;
        public int Position => sequencer.Position;
        public Player()
        {
            sequence.LoadProgressChanged += Sequence_LoadProgressChanged;
            sequence.LoadCompleted += Sequence_LoadCompleted;

            //sequence.Format = 1;
            //sequencer.Position = 0;
            sequencer.PlayingCompleted += Sequencer_PlayingCompleted;
            sequencer.ChannelMessagePlayed += Sequencer_ChannelMessagePlayed;
            sequencer.SysExMessagePlayed += Sequencer_SysExMessagePlayed;
            sequencer.Chased += Sequencer_Chased;
            sequencer.Stopped += Sequencer_Stopped;
            sequencer.Sequence = sequence;
        }
        public event EventHandler PlayingCompleted;
        protected virtual void OnPlayingCompleted(EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => PlayingCompleted?.Invoke(this, e)));
        }


        public int BPM = 120; // Beats Per Minute 1分あたり4分音符数
        private int tempo => (int)(60 / (double)BPM * 1000000); // 4分音符の長さ（マイクロ秒）
        public void Load(IEnumerable<(int index, int note)> track)
        {
            var t = new Track();
            var tempoBuilder = new TempoChangeBuilder();
            tempoBuilder.Tempo = tempo;
            tempoBuilder.Build();
            t.Insert(0, tempoBuilder.Result);

            foreach(var n in track)
            {
                t.NoteOn(n.index, n.note);
                t.NoteOff(n.index + 1, n.note);
            }
            t.End(t.EndOfTrackOffset);

            sequence.Clear();
            sequence.Add(t);
            sequencer.Sequence = sequence;
        }

        public void Play()
        {
            isPlaying = true;
            sequencer.Continue();
        }
        public void Pause()
        {
            isPlaying = false;
            sequencer.Stop();
        }
        public void PlayPause()
        {
            if(isPlaying)
            {
                isPlaying = false;
                sequencer.Stop();
            }
            else
            {
                isPlaying = true;
                sequencer.Continue();
            }
        }
        public void Stop()
        {
            isPlaying = false;
            sequencer.Stop();
            sequencer.Position = 0;
        }

        private void Sequence_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Debug.WriteLine("Sequence_LoadCompleted");
            //sequencer.Start();
        }
        private void Sequence_LoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Sequence_LoadProgressChanged");
        }
        private void Sequencer_Stopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Sequencer_Stopped");
            isPlaying = false;
            sequencer.Position = 0;
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
            MidiOut.Device.Send(e.Message);
        }
        private void Sequencer_PlayingCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("Sequencer_PlayingCompleted");
            isPlaying = false;
            sequencer.Position = 0;
            OnPlayingCompleted(e);
        }
    }
}
