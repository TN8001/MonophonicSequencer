using Sanford.Multimedia.Midi;

namespace MonophonicSequencer.Controls
{
    public static class TrackExtensions
    {
        private static int Ppqn = 24; // Pulses Per Quarter Note（TPQN Ticks Per Quarter Note）
                                      //4分音符の分割数
        private static int Tick16thNote = Ppqn / 4; // 16分音符のTick数

        /// <summary>TrackにNoteOnを挿入します。</summary>
        /// <param name="track"></param>
        /// <param name="position">16分音符を基準とした絶対位置</param>
        /// <param name="number">ノートナンバー</param>
        /// <param name="velocity">強さ</param>
        public static void NoteOn(this Track track, int position, int number, int velocity = 127)
        {
            var m = new ChannelMessage(ChannelCommand.NoteOn, 0, number, velocity);
            track.Insert(position * Tick16thNote, m);
        }
        /// <summary>TrackにNoteOffを挿入します。</summary>
        /// <param name="track"></param>
        /// <param name="position">16分音符を基準とした絶対位置</param>
        /// <param name="number">ノートナンバー</param>
        /// <param name="velocity">強さ</param>
        public static void NoteOff(this Track track, int position, int number, int velocity = 0)
        {
            var m = new ChannelMessage(ChannelCommand.NoteOff, 0, number, velocity);
            track.Insert(position * Tick16thNote, m);
        }
        /// <summary>Track終了点を設定します。</summary>
        /// <param name="track"></param>
        /// <param name="position">16分音符を基準とした絶対位置</param>
        public static void End(this Track track, int position)
        {
            track.EndOfTrackOffset = position * Tick16thNote;
        }
    }
}
