using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CG
{
    public static class PointExtensions
    {
        public static Vector2 ToVector2(this System.Windows.Point point, float z = 0f)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }
    }
    struct Rect 
    {
        public float Left, Top, Right, Bottom;
        public Rect(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
