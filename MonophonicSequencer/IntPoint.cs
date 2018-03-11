using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonophonicSequencer
{
    public struct IntPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(IntPoint left, IntPoint right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(IntPoint left, IntPoint right) => !(left == right);
        public override bool Equals(object obj)
        {
            if(!(obj is IntPoint)) return false;
            var comp = (IntPoint)obj;
            return comp.X == X && comp.Y == Y;
        }
        public override int GetHashCode() => X ^ Y;
        public override string ToString() => "{X=" + X + ",Y=" + Y + "}";
    }
}
