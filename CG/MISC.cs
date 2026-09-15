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

    public struct Pair<L,R>
    {
        public L left;
        public R right;
        
        public Pair(L left,R right)
        {
            this.left = left;
            this.right = right;
        }

    }

    public struct Rect 
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

    public struct Collision
    {
        public readonly TYPE Type;
        public readonly int DotIndex;
        public Primitive primitive;

        public Collision(TYPE Type,int DotIndex,Primitive primitive)
        {
            this.Type = Type;
            this.DotIndex = DotIndex;
            this.primitive = primitive;
        }

        public static bool RectCollision(Rect rect, Vector3 dot)
        {
            float offset = Configs.Offset;
            if (
                dot.X < (rect.Right + offset) &&
                dot.X > (rect.Left - offset) &&
                dot.Y < (rect.Top + offset) &&
                dot.Y > (rect.Bottom - offset)
                )
            {
                return true;
            }
            return false;
        }
        public static bool DotCollision(Vector3 a, Vector3 b)
        {
            float offset = Configs.Offset;
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (dx * dx + dy * dy) < (offset * offset);

        }
        public static bool LineCollision(Vector3 lineStart, Vector3 lineEnd, Vector3 dot) 
        {
            float offset = Configs.Offset;
            Vector3 position = GetClosestPointOnEdge(lineStart, lineEnd, dot);
            position = new Vector3(dot.X - position.X, dot.Y - position.Y, 0);
            float distanceSq = position.X * position.X + position.Y * position.Y;

            return distanceSq < (offset * offset);
        }
        
        public static Vector3 GetClosestPointOnEdge(Vector3 lineStart, Vector3 lineEnd, Vector3 dot) 
        {
            float offset = Configs.Offset;
            float lineX = lineEnd.X - lineStart.X;
            float lineY = lineEnd.Y - lineStart.Y;

            float lineLenSq = lineX * lineX + lineY * lineY;

            if (lineLenSq == 0f)
            {
                return lineStart;
            }

            float mouseVectorX = dot.X - lineStart.X;
            float mouseVectorY = dot.Y - lineStart.Y;

            float dotProduct = mouseVectorX * lineX + mouseVectorY * lineY;

            float t = dotProduct / lineLenSq;

            if (t < 0f) t = 0f;
            else if (t > 1f) t = 1f;

            float closestX = lineStart.X + t * lineX;
            float closestY = lineStart.Y + t * lineY;

            return new Vector3(closestX,closestY, 0);

        }

        public static bool PolygonCollision(Primitive primitive,Vector3 dot) 
        {
            bool inside = false;
            var polygon = primitive.getDots();
            for (int i = 0, j = primitive.getAmOfDots()-1; i < primitive.getAmOfDots(); j = i++)
            {
                if (((polygon[i].Y > dot.Y) != (polygon[j].Y > dot.Y)) &&
                    (dot.X < (polygon[j].X - polygon[i].X) * (dot.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }


}
