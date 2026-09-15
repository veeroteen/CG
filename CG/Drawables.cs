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

    public enum TYPE 
    {
        NONE,
        BOX,
        DOT,
        EDGE,
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
            float distance = Math.Abs(_cameraPos.Z);
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


        private static Vector3 _cameraPos = new Vector3(0,0,-5.0f);
        public static void changeCameraZ(float z)
        {
            _cameraPos.Z = z;
            calcoffset();
        }
        public static ref readonly Vector3 CameraPos => ref _cameraPos;
        public static void changeCameraPos(float x, float y)
        {
            _cameraPos.X = x;
            _cameraPos.Y = y;
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
            return new Vector3(ndcX * (visibleWidth / 2.0f) - Configs.CameraPos.X, ndcY * (visibleHeight / 2.0f) - Configs.CameraPos.Y,0);
        }
    }

    public class Primitive
    {
        private List<Vector3> Dots;
        private List<Pair<int,int>> Edges;
        private Rect _box;
        public bool closed { private set; get; }
        public Vector3 color;
        public ref readonly Rect Box => ref _box;

        public Pair<int, TYPE> hoverOn = new Pair<int, TYPE>(-1,TYPE.NONE);
        public int scopedTo = -1;
        private Primitive()
        {
            Edges = new List<Pair<int, int>>();
            Dots = new List<Vector3>();
        }
        public Primitive(Vector3 dot, Vector3 color)
        {
            
            Edges = new List<Pair<int,int>>();
            Dots = new List<Vector3>();
            Dots.Add(dot);
            this.color = color;
            _box = new Rect(dot.X, dot.Y, dot.X, dot.Y);
            scopedTo = 0;
        }

        public void addDot(Vector3 dot) 
        {
            if(scopedTo != -1) 
            {
                scopedTo = Dots.Count - 1;
                Dots.Add(dot);
                addEdge(scopedTo, Dots.Count - 1);
                recalcBox();
                
                return;
            }
            scopedTo = Dots.Count - 1;
            Dots.Add(dot);
            recalcBox();
            return;

        }
        public void addEdge(int i, int j)
        {
            Edges.Add(new Pair<int,int>(i, j));
        }

        public void divideEdge(int edge, int dot)
        {
            Dots[dot] = Collision.GetClosestPointOnEdge(Dots[Edges[edge].right], Dots[Edges[edge].left], Dots[dot]);
            Edges.Add(new Pair<int, int>(Edges[edge].right, dot));
            var tmp = Edges[edge];
            tmp.right = dot;
            Edges[edge] = tmp;
        }

        public void changeDotPosFlow(Vector3 dot,int i)
        {
            Dots[i] = dot;
        }
        public bool snap()
        {
            switch (hoverOn.right) 
            {
                case TYPE.DOT:
                {
                    if(scopedTo != -1 && scopedTo!=hoverOn.left) 
                    {
                        var tmp = Edges[Edges.Count - 1];
                        tmp.right = scopedTo;
                        tmp.left = hoverOn.left;
                        Edges[Edges.Count - 1] = tmp;
                        Dots.RemoveAt(Dots.Count - 1);
                        scopedTo = -1;
                        return true;
                    }
                    else 
                    {
                        Console.WriteLine("ERROR snap dot");
                    }
                    break;
                }
                case TYPE.EDGE:
                {
                    if (scopedTo != -1)
                    {
                        divideEdge(hoverOn.left, Dots.Count - 1);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("ERROR snap edge");
                    }
                    break;
                }
            }
            return false;
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
        public ReadOnlySpan<Pair<int,int>> getEdges()
        {
            return CollectionsMarshal.AsSpan(Edges);
        }
        public int getAmOfDots()
        {
            return Dots.Count;
        }

        public (TYPE Type, int Index) intersect(Vector3 cords) 
        {

            float offset = Configs.Offset;
            if (Collision.RectCollision(_box,cords))
            {
                for (int i = 0;  i < Dots.Count - (scopedTo == -1 ? 0 : 1); i++) 
                {
                    if (Collision.DotCollision(Dots[i],cords)) 
                    {
                        hoverOn.left = i;
                        hoverOn.right = TYPE.DOT;
                        return (TYPE.DOT, i);
                    }
                }

                for (int i = 0; i < Edges.Count - (scopedTo == -1 ? 0 : 1); i++)
                {
                    if (Collision.LineCollision(Dots[Edges[i].left], Dots[Edges[i].right],cords))
                    {
                        hoverOn.left = i;
                        hoverOn.right = TYPE.EDGE;
                        return (TYPE.EDGE, i);
                    }
                }

                if (closed) 
                {
                    if (Collision.PolygonCollision(this, cords)) 
                    {
                        hoverOn.left = -1;
                        hoverOn.right = TYPE.BODY;
                        return (TYPE.BODY, -1);
                    }
                }
            }
            hoverOn.left = -1;
            hoverOn.right = TYPE.NONE;
            return (TYPE.NONE ,- 1);
        }



    }
}
