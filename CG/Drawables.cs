using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CG
{
    public enum DRAWSTATE 
    {
        MESH,
        POLYGON
    }
    public enum CURSORSTATE 
    {
        FREE,
        DRAW
    }
    public enum INTERSECTION 
    {
        NONE,
        BOX,
        DOT,
        LINE,
        BODY
    }


    public static class Configs 
    {
        private static float _mouseRadius = 4;
        public static ref readonly float MouseRadius => ref _mouseRadius;
        public static void changeRadius(float radius)
        {
            _mouseRadius = radius;
            calcoffset();
        }
        private static void calcoffset()
        {
            float distance = Math.Abs(_cameraZ);
            float visibleHeight = (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0) * distance);
            _offset = (_mouseRadius / _size.Y) * visibleHeight;
        } 
        private static float _offset = 0;
        public static ref readonly float Offset => ref _offset;


        private static Vector2 _size = new Vector2(0, 0);
        public static ref readonly Vector2 Size => ref _size;
        public static void changeSize(Vector2 size)
        {
            _size = size;
            calcoffset();
        }


        private static float _cameraZ = -5.0f;
        public static ref readonly float CameraZ => ref _cameraZ;
        public static void changeCameraZ(float z)
        {
            _cameraZ = z;
            calcoffset();
        }


    }

    public static class Translator 
    {

        public static Vector3 toScreen(Vector3 cords, float cameaZ)
        {
            float ndcX = (2.0f * cords.X / Configs.Size.X) - 1.0f;
            float ndcY = 1.0f - (2.0f * cords.Y / Configs.Size.Y);

            float visibleHeight = (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0) * Math.Abs(cameaZ));
            float visibleWidth = visibleHeight * (Configs.Size.X / Configs.Size.Y);
            return new Vector3(ndcX * (visibleWidth / 2.0f), ndcY * (visibleHeight / 2.0f),cords.Z);

        }
        public static Vector3 toScreen(Vector2 cords, float cameaZ)
        {
            float ndcX = (2.0f * cords.X / Configs.Size.X) - 1.0f;
            float ndcY = 1.0f - (2.0f * cords.Y / Configs.Size.Y);

            float visibleHeight = (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0) * Math.Abs(cameaZ));
            float visibleWidth = visibleHeight * (Configs.Size.X / Configs.Size.Y);
            return new Vector3(ndcX * (visibleWidth / 2.0f), ndcY * (visibleHeight / 2.0f),0);

        }


    }

    public class Primitive
    {
        private List<Vector3> Dots;
        private Rect _box;
        public bool closed { private set; get; }
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
            if (dot == Dots[getAmOfDots() - 1] && getAmOfDots() > 1)
            {
                closed = true;
            }
            else
            {
                Dots.Add(dot);
                recalcBox();
            }
        }

        public void changeDotPosFlow(Vector3 dot,int i)
        {
            Dots[i] = dot;
        }
        public void recalcBox()
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

        public (INTERSECTION Type, int DotIndex) intersect(Vector3 cords) 
        {

            float offset = Configs.Offset;
            if (
                Collision.RectCollision(_box,cords,offset)
               )
            {

                for (int i = 0;  i < Dots.Count; i++) 
                {
                    if (Collision.DotCollision(Dots[i],cords,offset)) 
                    {
                        return (INTERSECTION.DOT, i);
                    }
                }

                for (int i = 0; i < Dots.Count - 1; i++)
                {
                    if (Collision.LineCollision(Dots[i], Dots[i+1],cords))
                    {
                        return (INTERSECTION.LINE, -1);
                    }
                }
                if (closed) 
                {
                    if (Collision.LineCollision(Dots[0], Dots[getAmOfDots()-1], cords))
                    {
                        return (INTERSECTION.LINE, -1);
                    }
                    if (Collision.PolygonCollision(this, cords)) 
                    {
                        return (INTERSECTION.BODY, -1);
                    }
                }
            }
            return (INTERSECTION.NONE ,- 1);
        }



    }
}
