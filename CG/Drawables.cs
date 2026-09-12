using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Drawing;

namespace CG
{
    enum DrawState 
    {
        MESH,
        POLYGON
    }

    public static class Translator 
    {

        public static Vector3 toScreen(Vector3 cords, Vector2 size, float cameaZ)
        {
            float ndcX = (2.0f * cords.X / size.X) - 1.0f;
            float ndcY = 1.0f - (2.0f * cords.Y / size.Y);

            float visibleHeight = (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0) * Math.Abs(cameaZ));
            float visibleWidth = visibleHeight * (size.X / size.Y);
            return new Vector3(ndcX * (visibleWidth / 2.0f), ndcY * (visibleHeight / 2.0f),cords.Z);

        }
        public static Vector2 toScreen(Vector2 cords, Vector2 size, float cameaZ)
        {
            float ndcX = (2.0f * cords.X / size.X) - 1.0f;
            float ndcY = 1.0f - (2.0f * cords.Y / size.Y);

            float visibleHeight = (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0) * Math.Abs(cameaZ));
            float visibleWidth = visibleHeight * (size.X / size.Y);
            return new Vector2(ndcX * (visibleWidth / 2.0f), ndcY * (visibleHeight / 2.0f));

        }
    }

    class Primitive
    {
        private List<Vector3> Dots;
        private Rect _box;

        public Vector3 color;
        public ref readonly Rect Box => ref _box;
        public Primitive()
        {
            Dots = new List<Vector3>();
        }
        public Primitive(Vector3 dot, Vector3 color)
        {
            Dots = new List<Vector3>();
            Dots.Add(dot);
            this.color = color;
            _box = new Rect(dot.X, dot.Y, dot.X, dot.Y);
        }

        public void addDot(Vector3 dot) 
        {
            Dots.Add(dot);
            _box.Left =  dot.X > _box.Left ? _box.Left : dot.X;
            _box.Right = dot.X < _box.Right ? _box.Right : dot.X; 
            _box.Top = dot.Y < _box.Top ? _box.Top : dot.Y;
            _box.Bottom = dot.Y > _box.Bottom ? _box.Bottom : dot.Y;
        }

        public void changeDotPosFlow(Vector3 dot,int i)
        {
            Dots[i] = dot;
        }
        public void RecalcBox()
        {
            
            foreach (var dot in Dots)
            {
                _box.Left = dot.X > _box.Left ? _box.Left : dot.X;
                _box.Right = dot.X < _box.Right ? _box.Right : dot.X;
                
                _box.Bottom = dot.Y > _box.Bottom ? _box.Bottom : dot.Y;
                _box.Top = dot.Y < _box.Top ? _box.Top : dot.Y;
            }
        }
        public ReadOnlySpan<Vector3> getDots()
        {
            return CollectionsMarshal.AsSpan(Dots);
        }
        public int getAmOfDots()
        {
            return Dots.Count;
        }

    }
}
