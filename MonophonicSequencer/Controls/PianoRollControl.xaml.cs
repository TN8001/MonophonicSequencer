using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MonophonicSequencer.Controls
{
    //各機能を統括しPianoRollにまとめるクラス
    public partial class PianoRollControl : UserControl
    {
        //MIDIをアプリ内で複数同時に開けないので（このアプリ自体を複数立ち上げるのはｏｋ）
        //誰かが面倒を見る必要がある 今後鍵盤コントロールがつくのでシングルトンに変更する予定
        private MidiOut midiOut = new MidiOut();

        //WPFでは標準的なタイマー
        private DispatcherTimer timer = new DispatcherTimer();

        //デザイナの挙動説明用 使ってはいない
        //使ってはいないので警告が出てうざいのでpragmaで抑制した
#pragma warning disable CS0169
        private DispatcherTimer timer2;
#pragma warning restore CS0169


        //再生に関するクラス
        private Player player = new Player();


        public PianoRollControl()
        {
            //xamlの初期化
            InitializeComponent();

            //playerから再生終了を教えてもらう
            player.PlayingCompleted += Player_PlayingCompleted;

            //10ミリ秒の精度は出ない 16ms程度らしい
            //間隔が指定以上空くのが保証されるだけで早まることはないが
            //基本的にはどんどん遅れが積算していく
            timer.Interval = TimeSpan.FromMilliseconds(10);


            //赤線の移動
            //イベントハンドラにメソッドを使わずラムダ式（Lambda Expressions）を使った指定法
            //普通に書くとこれ
            //timer.Tick += Timer_Tick;
            //private void Timer_Tick(object sender, EventArgs e)
            //{
            //    pianoRollGrid.Offset2 = player.Position;
            //}
            //
            //これはメソッドを＝＞で書いたパターン
            //timer.Tick += Timer_Tick;
            //private void Timer_Tick(object sender, EventArgs e) => pianoRollGrid.Offset2 = player.Position;
            //
            //それをさらに簡略化した感じ
            timer.Tick += (s, e) => pianoRollGrid.Offset2 = player.Position;


            //MidiOut.csで書いたデザイナの挙動の検証用
            //↓のコメントを外し一度ビルドするとMainWindow.xamlのデザイン画面で赤線が動き続ける
            //ステータスバーにあるProgressBarが動いているのと同じ
            //動かないどころか真っ白になっている場合は
            //デザイナ部分左下にある100%のような拡大率の右に並んでいるボタンのうち
            //一番右の[プロジェクト コードの有効化]をONにする（説明が難しいw
            //この下6行
            //if(DesignerProperties.GetIsInDesignMode(this))
            //{
            //    timer2 = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            //    timer2.Tick += (s, e) => pianoRollGrid.Offset = pianoRollGrid.Offset > 200 ? 0 : pianoRollGrid.Offset + 1;
            //    timer2.Start();
            //}



            //PianoRollGrid（編集中に音を出すため）と
            //player（再生するため）にMIDIを渡す
            //筋悪なので変更します
            pianoRollGrid.MidiOut = midiOut;
            player.MidiOut = midiOut;
        }

        private void Player_PlayingCompleted(object sender, EventArgs e)
        {
            //赤線を戻し無駄なのでタイマーを止める
            pianoRollGrid.Offset2 = 0;
            timer.Stop();
        }

        public void Play()
        {
            //前回再生から変更があれば
            if(pianoRollGrid.IsDirty)
            {
                //データを取得し再読み込み
                var n = pianoRollGrid.GetTrack();
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
