using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MonophonicSequencer.Resources
{
    ///<summary>定義済みのPenオブジェクトセット</summary>    
    public static class Pens
    {
        ///<summary>色:単色Gray 太さ:1</summary>    
        public static readonly Pen Gray = new Pen(Brushes.Gray, 1);
        ///<summary>色:単色LightGray 太さ:1</summary>    
        public static readonly Pen LightGray = new Pen(Brushes.LightGray, 1);
        ///<summary>色:単色Black 太さ:1</summary>    
        public static readonly Pen Black = new Pen(Brushes.Black, 1);
        ///<summary>色:単色Black 太さ:4</summary>    
        public static readonly Pen Black4 = new Pen(Brushes.Black, 4);
        ///<summary>色:透明 太さ:0</summary>    
        public static readonly Pen Transparent = new Pen(Brushes.Transparent, 0);
    }
}
