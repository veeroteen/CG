using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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


        private static Rect GetScreenRect()
        {
            float visibleHeight =
                   (float)(2.0 * Math.Tan(22.5 * Math.PI / 180.0)
                   * Math.Abs(CameraPos.Z));

            float visibleWidth =
                visibleHeight * Size.X / Size.Y;

            float centerX = -CameraPos.X;
            float centerY = -CameraPos.Y;

            return new Rect(
                centerX - visibleWidth / 2.0f, 
                centerY + visibleHeight / 2.0f,
                centerX + visibleWidth / 2.0f,  
                centerY - visibleHeight / 2.0f 
            );
        }


        private static Vector2 _size = new Vector2(0, 0);
        public static ref readonly Vector2 Size => ref _size;
        public static void changeSize(Vector2 size)
        {
            _size = size;
            calcoffset();
            _screenRect = GetScreenRect();
        }

        private static Rect _screenRect = new Rect();

        public static ref readonly Rect ScreenRect => ref _screenRect;





        private static Vector3 _cameraPos = new Vector3(0,0,-5.0f);
        public static void changeCameraZ(float z)
        {
            _cameraPos.Z = z;
            calcoffset();
            _screenRect = GetScreenRect();
        }
        public static ref readonly Vector3 CameraPos => ref _cameraPos;
        public static void changeCameraPos(float x, float y)
        {
            _cameraPos.X = x;
            _cameraPos.Y = y;
            calcoffset();
            _screenRect = GetScreenRect();
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
        private Vector3 _center;
        public bool closed { private set; get; }
        public Vector3 color;
        public ref readonly Rect Box => ref _box;
        public ref readonly Vector3 Center => ref _center;
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
            _center = dot;
            this.color = color;
            _box = new Rect(dot.X, dot.Y, dot.X, dot.Y);
        }
        public void addDot(Vector3 dot, int scopedTo = -1)
        {
            if (scopedTo != -1)
            {
                Dots.Add(dot);
                addEdge(scopedTo, Dots.Count - 1);
                recalcBox();
                recalcCenter();
                closed = checkClosed();
                return;
            }
            Dots.Add(dot);
            recalcBox();
            recalcCenter();
            closed = checkClosed();
            return;
        }
        public void removeDot(int id) 
        {
            for (int i = Edges.Count - 1; i >= 0; i--)
            {
                if (Edges[i].right == id || Edges[i].left == id)
                {
                    Edges.RemoveAt(i);
                }
            }

            for (int i = 0; i < Edges.Count;i++)
            {
                if (Edges[i].left > id)
                    Edges[i] = new Pair<int, int>(Edges[i].left - 1, Edges[i].right);

                if (Edges[i].right > id)
                    Edges[i] = new Pair<int, int>(Edges[i].left, Edges[i].right-1);
            }
            Dots.RemoveAt(id);
        }


        public void recalcCenter(Vector3 dot) 
        {
            Func<float, float, float> shortcut = (newdot, oldcenter) =>
            {
                return ((oldcenter * (Dots.Count - 1)) + newdot) / Dots.Count;

            };

            _center = new Vector3(shortcut(dot.X, _center.X), shortcut(dot.Y, _center.Y), shortcut(dot.Z, _center.Z));
        }

        public void recalcCenter(int i)
        {
            Func<float, float, float> shortcut = (newdot, oldcenter) =>
            {
                return ((oldcenter * (Dots.Count - 1)) + newdot) / Dots.Count;

            };

            _center = new Vector3(shortcut(Dots[i].X, _center.X), shortcut(Dots[i].Y, _center.Y), shortcut(Dots[i].Z, _center.Z));
        }

        public void recalcCenter()
        {
            var tmp = new Vector3(0.0f, 0.0f,0.0f);

            foreach (var dot in Dots)
            {
                tmp.X += dot.X;
                tmp.Y += dot.Y;
                tmp.Z += dot.Z;
            }
            tmp.X /= Dots.Count;
            tmp.Y /= Dots.Count;
            tmp.Z /= Dots.Count;

            _center = tmp;
        }



        public void addEdge(int i, int j)
        {
            Edges.Add(new Pair<int,int>(i, j));
        }

        public void removeEdge(int id)
        {
            Edges.RemoveAt(id);
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


        public void changeEdgePosFlow(Vector3 delta, int i)
        {
            var edge = Edges[i];
            Dots[edge.right] = new Vector3(Dots[edge.right].X + delta.X, Dots[edge.right].Y + delta.Y, Dots[edge.right].Z + delta.Z);
            Dots[edge.left] = new Vector3(Dots[edge.left].X + delta.X, Dots[edge.left].Y + delta.Y, Dots[edge.left].Z + delta.Z);
        }
        public void changePosFlow(Vector3 delta)
        {

            for(int i = 0; i < Dots.Count; i++) 
            {
                Dots[i] = Dots[i] + delta;
            }
            recalcBox();
            recalcCenter();
        }



        public bool checkClosed() 
        {
            List<int> tmp = Enumerable.Repeat(0, Dots.Count).ToList();
            foreach (var edge in Edges)
            {
                tmp[edge.left]++;
                tmp[edge.right]++;
            }
            foreach(int d in tmp) 
            {
                if(d < 2) 
                {
                    return false;
                }
            
            }
            return true;
        }

        public bool snap(Pair<int,TYPE> hoverOn, int scopedTo = -1)
        {
            switch (hoverOn.right) 
            {
                case TYPE.DOT:
                {
                    if(scopedTo != -1) 
                    {
                        for (int i = 0; i < Edges.Count; i++ ) 
                        {
                            if (Edges[i].left == scopedTo || Edges[i].right == scopedTo) 
                            {
                                Pair<int,int> edge = Edges[i];
                                if (edge.left == scopedTo) 
                                {
                                    edge.left = hoverOn.left;
                                }
                                else 
                                {
                                    edge.right = hoverOn.left;
                                }
                                Edges[i] = edge;
                            }
                        }
                        Dots.RemoveAt(scopedTo);

                        closed = checkClosed();

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
                        divideEdge(hoverOn.left, scopedTo);
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

        public void recalcMISK()
        {
            recalcBox();
            recalcCenter();
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

        public Pair<int,TYPE> intersect(Vector3 cords,int scopedTo = -1) 
        {

            float offset = Configs.Offset;
            if (Collision.RectCollision(_box,cords))
            {
                for (int i = 0;  i < Dots.Count; i++) 
                {
                    if (i != scopedTo)
                    {
                        if (Collision.DotCollision(Dots[i], cords))
                        {
                            return new Pair<int, TYPE>(i, TYPE.DOT);
                        }
                    }
                }


                for (int i = 0; i < Edges.Count; i++)
                {
                    if (Edges[i].left != scopedTo && Edges[i].right != scopedTo)
                    {
                        if (Collision.LineCollision(Dots[Edges[i].left], Dots[Edges[i].right], cords))
                        {
                            return new Pair<int, TYPE>(i, TYPE.EDGE);
                        }
                    }
                }

                if (closed) 
                {
                    if (Collision.PolygonCollision(this, cords)) 
                    {
                        return new Pair<int, TYPE>(-1, TYPE.BODY);
                    }
                }

                return new Pair<int, TYPE>(-1, TYPE.BOX);
            }
            return new Pair<int, TYPE>(-1, TYPE.NONE);
        }

        public bool intersect(Rect rect)
        {
            return _box.Intersects(rect);
        }

    }
}
