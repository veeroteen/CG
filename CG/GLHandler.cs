using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
namespace CG
{
    public enum OSTATE
    {
        MESH,
        POLYGON,
    }
    public enum ISTATE
    {
        FREE,
        DRAW,
        CONTINUOUSDRAW,
        MOVE
    }

    class GLHandler
    {
        private OpenGL gl;
        private List<Primitive> Primitives;
        private Primitive scoped;
        private OSTATE oState = OSTATE.MESH;
        private ISTATE iState = ISTATE.FREE;
        private bool dragCameraState = false;
        private Vector2 mousePosition;
        private Vector3 GLMousePosition;
        private Pair<int,TYPE> hoverOn = new Pair<int,TYPE>(-1,TYPE.NONE);
        private void draw() 
        {
            if (scoped != null )
            {
                scoped.changeDotPosFlow(new Vector3((float)GLMousePosition.X, (float)GLMousePosition.Y, 0), scoped.getAmOfDots() - 1);
            }
            bool hovered = false;
            List<Collision> collisions = new List<Collision>();

            foreach (var primitive in Primitives)
            {
                var dots = primitive.getDots();
                primitive.intersect(GLMousePosition);
                if(iState == ISTATE.DRAW && primitive.hoverOn.right == TYPE.BOX) 
                {
                    primitive.hoverOn.right = TYPE.NONE;
                }


                //collisions.Add( new Collision(tmp.Type,tmp.DotIndex, primitive));

                var color = primitive.color;

                /*
                if (primitive.getAmOfDots() > 2)
                {
                    hoverOn = primitive.hoverOn;
                    gl.Color(color.X, color.Y, color.Z);
                    gl.Begin(OpenGL.GL_POLYGON);
                    foreach (var dot in primitive.getDots())
                    {
                        gl.Vertex(dot.X, dot.Y, dot.Z);
                    }
                }
                else
                */
                {

                    if (primitive.hoverOn.right == TYPE.EDGE && !hovered )
                    {
                        //hoverOn = primitive.hoverOn;
                        gl.Color(1.0f, 1.0f, 1.0f);
                        gl.LineWidth(4.0f);

                        var edge = primitive.getEdges()[primitive.hoverOn.left];
                        
                        gl.Begin(OpenGL.GL_LINES);
                        gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                        gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                        gl.End();
                        
                        
                        hovered = true;
                    }

                    gl.Color(color.X, color.Y, color.Z);
                    gl.LineWidth(1.5f);
                    
                    foreach (var edge in primitive.getEdges())
                    {
                        gl.Begin(OpenGL.GL_LINES);
                        gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                        gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                        gl.End();
                    }
                }
                

                for (int i = 0; i < dots.Length; i++)
                {
                    if(primitive.hoverOn.right == TYPE.DOT && primitive.hoverOn.left == i && !hovered) 
                    {
                        gl.PointSize(8.0f);
                        gl.Color(1.0f, 1.0f, 1.0f);
                        gl.Begin(OpenGL.GL_POINTS);
                        gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);
                        gl.End();
                        hovered = true;
                    }
                    gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);

                    gl.PointSize(4.0f);
                    gl.Color(0.0f, 1.0f, 0.0f);
                    gl.Begin(OpenGL.GL_POINTS);
                    gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);
                    gl.End();
                }


            }
        }


        public void Update( Vector2 mousePosition)
        {
            if (dragCameraState) 
            {
                float tx = GLMousePosition.X, ty = GLMousePosition.Y;
                Vector3 tmp = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
                tx = tmp.X - tx;
                ty = tmp.Y - ty;
                Configs.changeCameraPos(tx + Configs.CameraPos.X, ty + Configs.CameraPos.Y);
                GLMousePosition = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
            }
            else
            {
                GLMousePosition = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
            }
                
            this.mousePosition = mousePosition;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(Configs.CameraPos.X, Configs.CameraPos.Y, Configs.CameraPos.Z);

            draw();

            gl.Flush();
        }
        public GLHandler(OpenGLControl control)
        {
            Primitives = new List<Primitive>();
            gl = control.OpenGL;
            gl.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            gl.PointSize(6.0f);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);
        }

        public void ExecuteButtonDown()
        {
            Vector3 color = new Vector3(1.0f, 0.0f, 0.0f);
            
            Console.WriteLine($" OpenGL: X={GLMousePosition.X:F2}, Y={GLMousePosition.Y:F2}, Y={Configs.CameraPos.Z:F2}");

            switch (iState) 
            {
                case ISTATE.FREE:
                {

                    Primitive line = new Primitive(GLMousePosition, color);
                    line.addDot(GLMousePosition);
                    scoped = line;
                    Primitives.Add(line);
                    iState = ISTATE.DRAW;
                    break;
                }
                case ISTATE.CONTINUOUSDRAW:
                {
                    if (scoped != null)
                    {
                        if (!scoped.snap())
                        {
                            scoped.addDot(GLMousePosition);
                        }
                        else 
                        {
                            iState = ISTATE.FREE;
                            scoped = null;
                        }
                    }
                    else 
                    {
                        Primitive line = new Primitive(GLMousePosition, color);
                        line.addDot(GLMousePosition);
                        scoped = line;
                        Primitives.Add(line);
                        iState = ISTATE.DRAW;
                    }
                    break;
                }
                case ISTATE.DRAW:
                {
                    iState = ISTATE.FREE;
                    scoped.snap();
                    scoped.recalcBox();
                    scoped = null;
                    break;
                }
            }
        }

        public void ExecuteButtonUp() 
        {

        }
        public void MiscButtonDown() 
        {
            
        
        
        }


        public void ControlKeyDown() 
        {
            switch (iState)
            {
                case ISTATE.FREE:
                {
                    iState = ISTATE.CONTINUOUSDRAW;
                    break;
                }
                case ISTATE.DRAW:
                {
                    iState = ISTATE.CONTINUOUSDRAW;
                    break;
                }
                case ISTATE.CONTINUOUSDRAW:
                {
                    break;
                }
            }
        }
        public void ControlKeyUp()
        {
            switch (iState)
            {
                case ISTATE.FREE:
                {
                    break;
                }
                case ISTATE.DRAW:
                {
                    break;
                }
                case ISTATE.CONTINUOUSDRAW:
                {
                    if (scoped != null)
                    {
                        iState = ISTATE.DRAW;
                    }
                    else 
                    {
                        iState = ISTATE.FREE;
                    }
                    break;
                }
            }

        }


        public void DragCameraSwitch(bool state) 
        {
            dragCameraState = state;
        }

        public void ChangeScale(int zoom, float amount = 0.25f)
        {
            float cameraZ = Configs.CameraPos.Z;
            cameraZ += zoom * amount;
            
            if (cameraZ > -0.25f) 
            {
                cameraZ = -0.25f;
            }
            Configs.changeCameraZ(cameraZ);
        }

    }
}
