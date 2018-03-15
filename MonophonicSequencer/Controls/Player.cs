using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace MonophonicSequencer.Controls
{
    //MIDI出力を管理するクラス
    public class Player
    {
        //分けるほどでもないかもしれない
        public MidiOut MidiOut { get; set; }

        //シーケンス（トラックの集まり≒MIDファイル？）を読み書きや作成する
        private Sequence sequence = new Sequence();
        //Sequenceを再生する
        private Sequencer sequencer = new Sequencer();
        private bool isPlaying;

        //MIDIの位置の数値 分解能（TPQN）によってかわる
        //  TPQN（Pulses Per Quarter Note）またはTpqn（Ticks Per Quarter Note）4分音符の分割数
        //480あたりが標準っぽいが16部音符が分かればいいので24になっています
        //つまり16分音符を6個に分けて移動する TPQNをもうちょっと上げれば赤線がスムースになるかも？
        //（タイマー精度があまりないのでそれほど期待できなそう）
        public int Position => sequencer.Position;

        public Player()
        {
            //イベント購読
            //実際使ってるのは↓３つだけ 他は検証用
            //PlayingCompleted
            //ChannelMessagePlayed
            //Stopped
            sequence.LoadProgressChanged += Sequence_LoadProgressChanged;
            sequence.LoadCompleted += Sequence_LoadCompleted;
            sequencer.PlayingCompleted += Sequencer_PlayingCompleted;
            sequencer.ChannelMessagePlayed += Sequencer_ChannelMessagePlayed;
            sequencer.SysExMessagePlayed += Sequencer_SysExMessagePlayed;
            sequencer.Chased += Sequencer_Chased;
            sequencer.Stopped += Sequencer_Stopped;

            //名前が似ていて紛らわしいがシーケンサーにシーケンスを設定
            sequencer.Sequence = sequence;


            var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Playerコンストラクタ ここはUIスレッド ThreadID : {id}");
        }

        //PianoRollControlに再生終了を伝える用イベント
        //ほかに伝える方法はいろいろあるがイベントは依存がないのが利点
        //デメリットは購読の解除をしないとメモリリークする
        //今回はアプリ終了までお互い1個しか生成しないため心配ない
        //別の方法としてにはPianoRollControlがPlayerを生成するので
        //Playerのコンストラクタでデリゲートを渡しておく等が考えられる
        public event EventHandler PlayingCompleted;

        //再生終了時にイベントを発砲する
        //protected virtualにする意味は特にないが継承先で処理を挟みたい時 等のために
        //こういった形にしておくのが暗黙のルールっぽい??
        protected virtual void OnPlayingCompleted(EventArgs e)
        {
            //（?.で購読者がいなければ何もしないし
            //  複数いたらそれぞれ登録されたメソッドを(object sender, EventArgs e)の形で呼び出す）
            //senderはこのPlayer自身 eはsequencer.PlayingCompletedの引数そのまま（実際PlayingCompletedのeは空だが）
            //通常は↓でいいが
            //PlayingCompleted?.Invoke(this, e);
            //このメソッドを呼ぶSequencer_PlayingCompleted()がsequencerのスレッドで呼ばれるため
            //この場所はUIスレッドではないのでUIスレッドで呼んでもらう（テンプレ）
            Application.Current.Dispatcher.Invoke(new Action(() => PlayingCompleted?.Invoke(this, e)));


            var id = System.Threading.Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"OnPlayingCompleted ここはUIスレッドでない!! ThreadID : {id}");
        }

        // テンポ Beats Per Minute 1分あたりの4分音符数
        public int BPM = 120;

        // MIDI上のテンポ値 4分音符の長さ（マイクロ秒）
        private int tempo => (int)(60 / (double)BPM * 1000000);

        //音符の位置と音程のリストをsequenceに読み込む
        //PianoRollControlがPianoRollGridから取得した変種中のデータ
        public void Load(IEnumerable<(int index, int note)> track)
        {
            //Trackは編集するような作りになっていないので作り直す
            var t = new Track();

            //Builderパターンですね
            var tempoBuilder = new TempoChangeBuilder();
            tempoBuilder.Tempo = tempo;
            tempoBuilder.Build();

            //このMIDIライブラリではなにかしらを作ってInsert(位置, データ)という作りのようです
            t.Insert(0, tempoBuilder.Result);

            //青点の個数だけループ
            foreach(var n in track)
            {
                //作ってInsertがちょっと冗長だったので拡張メソッド内でやっています（TrackExtensions.cs）
                t.NoteOn(n.index, n.note);
                t.NoteOff(n.index + 1, n.note);
            }
            //これも
            t.End(t.EndOfTrackOffset);

            //Track再設定
            sequence.Clear();
            sequence.Add(t);
        }

        public void Play()
        {
            isPlaying = true;
            //止めた場所から再生
            sequencer.Continue();
        }
        public void Pause()
        {
            isPlaying = false;
            sequencer.Stop();
        }
        public void PlayPause()
        {
            //トグル動作のためにisPlayingを作っているが現状このメソッド使ってないｗ
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

        //MIDIファイルから読み込みが完了したときのイベント
        private void Sequence_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Debug.WriteLine("Sequence_LoadCompleted");
        }
        //MIDIファイル読み込み中に進捗状態報告のイベント だと思われる
        private void Sequence_LoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("Sequence_LoadProgressChanged");
        }
        //再生を止めた時のイベント
        private void Sequencer_Stopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("Sequencer_Stopped");
            isPlaying = false;
            sequencer.Position = 0;
        }
        //よくわからん
        private void Sequencer_Chased(object sender, ChasedEventArgs e)
        {
            Debug.WriteLine("Sequencer_Chased");
        }


        //MIDI機器に音を送るタイミングが来たから送ってね」というイベント
        //SequencerはMIDI情報を読みながらタイミングを計ってイベントを飛ばしてくるが
        //実際にMIDIに送るのは使用側（このアプリ）の責任らしい
        //データを加工したり等 自由度を高めたいという思想なんだろうが
        //全部やってくれるお任せモードも欲しかった
        //これがないと一切音が鳴らない しばらく悩んだ
        private void Sequencer_ChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            Debug.WriteLine("Sequencer_ChannelMessagePlayed");
            MidiOut.Device.Send(e.Message);
        }
        //MIDI機器に送るオプションデータ的な？鳴らすだけなら必要ない
        private void Sequencer_SysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
            Debug.WriteLine("Sequencer_SysExMessagePlayed");
            //MidiOut.Device.Send(e.Message);
        }


        //曲が最後まで終わったとき
        private void Sequencer_PlayingCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("Sequencer_PlayingCompleted");
            isPlaying = false;
            sequencer.Position = 0;


            //PianoRollControlに再生終了を伝えるためイベント発砲メソッドを呼ぶ
            //PianoRollControlは一時停止や停止は判断できるが（自身にボタンやメニューがあるため）
            //再生終了は検知できないので教えてあげる
            OnPlayingCompleted(e);
        }
    }
}
