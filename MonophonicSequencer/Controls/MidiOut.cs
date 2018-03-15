using Sanford.Multimedia.Midi;
using System;
using System.ComponentModel;
using System.Windows;

namespace MonophonicSequencer.Controls
{
    //シングルトンに変更予定だが PlayerクラスにMIDIのOutputDeviceを提供＆破棄処理クラス
    public class MidiOut : UIElement // Dispatcher使いたい為 雑い
    {
        //外部ライブラリのMIDI出力をよしなにしてくれるクラス
        public OutputDevice Device => outDevice;
        private OutputDevice outDevice;

        public MidiOut()
        {
            //VisualStudioでコーディング中にデザイナがエラーになるため
            //OutputDeviceを実行時でなければ作らないようにする
            //VisualStudioは高性能なのでデザイン中もインスタンスを生成し裏で動かしています
            if(!DesignerProperties.GetIsInDesignMode(this))
                outDevice = new OutputDevice(0);

            //アプリが終了するときのイベント
            //大体..ed+= と過去形+=となっていたらイベント購読と思っていいです
            //この場合終了イベントが発生したら Dispatcher_ShutdownStarted()が呼ばれる
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            //一応すでに破棄されてないか確認し
            if(!outDevice.IsDisposed)
                //破棄する（音を出したまま終了しても止めてから終了する。。はずｗ）
                outDevice.Dispose();
        }
    }
}
