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
    class GLHandler
    {
        private OpenGL gl;
        private List<Primitive> Primitives;
        private Primitive scoped;

        private DRAWSTATE drawState = DRAWSTATE.MESH;
        private CURSORSTATE cursorState;
        private Vector3 GLMousePosition;
        private void draw() 
        {
            if (scoped != null)
            {
                scoped.changeDotPosFlow(new Vector3((float)GLMousePosition.X, (float)GLMousePosition.Y, 0), scoped.getAmOfDots() - 1);
            }
            List<Collision> collisions = new List<Collision>();
            foreach (var primitive in Primitives)
            {
                var tmp = primitive.intersect(GLMousePosition);
                //collisions.Add( new Collision(tmp.Type,tmp.DotIndex, primitive));

                var color = primitive.color;


                if (primitive.getAmOfDots() > 2)
                {
                    gl.Color(color.X, color.Y, color.Z);
                    gl.Begin(OpenGL.GL_POLYGON);
                    foreach (var dot in primitive.getDots())
                    {
                        gl.Vertex(dot.X, dot.Y, dot.Z);
                    }
                }
                else
                {
                    if (tmp.Type == INTERSECTION.LINE)
                    {
                        gl.Color(1.0f, 1.0f, 1.0f);
                        gl.LineWidth(4.0f);
                        gl.Begin(OpenGL.GL_LINES);
                        foreach (var dot in primitive.getDots()) { gl.Vertex(dot.X, dot.Y, dot.Z); }
                        gl.End();
                    }

                    gl.Color(color.X, color.Y, color.Z);
                    gl.LineWidth(1.5f);
                    gl.Begin(OpenGL.GL_LINES);
                    foreach (var dot in primitive.getDots())
                    {
                        gl.Vertex(dot.X, dot.Y, dot.Z);
                    }
                }
                gl.End();


                
                var dotsList = primitive.getDots();
                for (int i = 0; i < dotsList.Length; i++)
                {
                    if(tmp.Type == INTERSECTION.DOT && tmp.DotIndex == i) 
                    {
                        gl.PointSize(8.0f);
                        gl.Color(1.0f, 1.0f, 1.0f);
                        gl.Begin(OpenGL.GL_POINTS);
                        gl.Vertex(dotsList[i].X, dotsList[i].Y, dotsList[i].Z);
                        gl.End();
                    }
                    gl.Vertex(dotsList[i].X, dotsList[i].Y, dotsList[i].Z);

                    gl.PointSize(4.0f);
                    gl.Color(0.0f, 1.0f, 0.0f);
                    gl.Begin(OpenGL.GL_POINTS);
                    gl.Vertex(dotsList[i].X, dotsList[i].Y, dotsList[i].Z);
                    gl.End();
                }


            }
        }


        public void Update( Vector2 mousePosition)
        {
            this.GLMousePosition = Translator.toScreen(mousePosition, Configs.CameraZ);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(0.0f, 0.0f, Configs.CameraZ);

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

        public void MouseClick()
        {

            Vector3 color = new Vector3(1.0f, 0.0f, 0.0f);
            
            Console.WriteLine($" OpenGL: X={GLMousePosition.X:F2}, Y={GLMousePosition.Y:F2}");

            if (cursorState == CURSORSTATE.DRAW) 
            {
                cursorState = CURSORSTATE.FREE;
                scoped.recalcBox();
                scoped = null;
            }
            else 
            {
                Primitive line = new Primitive(GLMousePosition, color);
                line.addDot(GLMousePosition);
                scoped = line;
                Primitives.Add(line);
                cursorState = CURSORSTATE.DRAW;
            }
        }
        public void ChangeScale(int zoom, float amount = 0.25f)
        {
            float cameraZ = Configs.CameraZ;
            cameraZ += zoom * amount;
            
            if (cameraZ > -0.25f) 
            {
                cameraZ = -0.25f;
            }
            Configs.changeCameraZ(cameraZ);
        }

    }
}
