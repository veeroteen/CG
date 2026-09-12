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
        private float cameraZ = -5.0f;

        private bool mouseClicked;

        private Vector2 size, mousePosition, GLMousePosition;
        

        public void Update(Vector2 size, Vector2 mousePosition)
        {
            this.size = size;
            this.mousePosition = mousePosition;
            this.GLMousePosition = Translator.toScreen(mousePosition, size, cameraZ);


            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(0.0f, 0.0f, cameraZ);

            if (scoped != null)
            {
                scoped.changeDotPosFlow(new Vector3((float)GLMousePosition.X, (float)GLMousePosition.Y, 0), scoped.getAmOfDots()-1);
            }

            foreach (var primitive in Primitives)
            {
                var color = primitive.color;
                gl.Color(color.X,color.Y,color.Z);
                
                if (primitive.getAmOfDots() > 2)
                {
                    gl.Begin(OpenGL.GL_POLYGON);
                    foreach (var dot in primitive.getDots()) 
                    {
                        gl.Vertex(dot.X, dot.Y, dot.Z);
                    }
                }
                else
                {
                    gl.Begin(OpenGL.GL_LINES);
                    foreach (var dot in primitive.getDots())
                    {
                        gl.Vertex(dot.X, dot.Y, dot.Z);
                    }
                }
                gl.End();

                
                gl.Color(0.0f, 1.0f, 0.0f);
                gl.Begin(OpenGL.GL_POINTS);
                foreach (var dot in primitive.getDots())
                {
                    gl.Vertex(dot.X, dot.Y, dot.Z);
                }
                gl.End();

            }
            gl.Flush();
        }


        public GLHandler(OpenGLControl control)
        {
            Primitives = new List<Primitive>();
            gl = control.OpenGL;
            gl.ClearColor(0.9f, 0.9f, 0.9f, 1.0f);
            gl.PointSize(6.0f);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);
        }




        public void MouseClick()
        {


            Vector3 color = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 position = new Vector3(mousePosition.X, mousePosition.Y, 0.0f);
            Vector3 GLMousePosition = Translator.toScreen(position, size, cameraZ);

            
            Console.WriteLine($"Window: X={mousePosition.X:F0}, Y={mousePosition.Y:F0} | OpenGL: X={GLMousePosition.X:F2}, Y={GLMousePosition.Y:F2}");

            if (mouseClicked) 
            {
                mouseClicked = false;
                scoped.RecalcBox();
                scoped = null;
            }
            else 
            {
                Primitive line = new Primitive(GLMousePosition, color);
                line.addDot(GLMousePosition);
                scoped = line;
                Primitives.Add(line);
                mouseClicked = true;
            }
        }
        public void ChangeScale(int zoom, float amount = 0.25f)
        {
            
            cameraZ += zoom * amount;
            
            if (cameraZ > -0.25f) 
            {
                cameraZ = -0.25f;
            }
        }

    }
}
