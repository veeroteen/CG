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
using System.Windows.Input;
namespace CG
{

    class GLHandler
    {
        private OpenGL gl;
        private List<Primitive> Primitives;

        private OSTATE oState = OSTATE.MESH;
        private IState iState = new(ISTATE.FREE);
        private bool dragCameraState = false;
        private Vector2 mousePosition;
        private Vector3 GLMousePosition;

        private Pair<int,TYPE> hoverOn = new Pair<int,TYPE>(-1,TYPE.NONE);
        private int hoverID = -1;
        private Queue<InputEvent> inputQueue = new Queue<InputEvent>();
        private void draw(List<int> drawables) 
        {
            if (iState.pID != -1)
            {
                Primitives[iState.pID].changeDotPosFlow(new Vector3((float)GLMousePosition.X, (float)GLMousePosition.Y, 0), Primitives[iState.pID].getAmOfDots() - 1);
            }

            if (hoverOn.right == TYPE.EDGE)
            {
                var dots = Primitives[hoverID].getDots();
                gl.Color(1.0f, 1.0f, 1.0f);
                gl.LineWidth(4.0f);

                var edge = Primitives[hoverID].getEdges()[hoverOn.left];

                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                gl.End();
            }

            if (hoverOn.right == TYPE.DOT)
            {
                
                var dots = Primitives[hoverID].getDots();
                gl.PointSize(8.0f);
                gl.Color(1.0f, 1.0f, 1.0f);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex(dots[hoverOn.left].X, dots[hoverOn.left].Y, dots[hoverOn.left].Z);
                gl.End();
            }



            foreach (var primitive in drawables)
            {
                var dots = Primitives[primitive].getDots();

                var color = Primitives[primitive].color;

                gl.Color(color.X, color.Y, color.Z);
                gl.LineWidth(1.5f);




                foreach (var edge in Primitives[primitive].getEdges())
                {
                    gl.Begin(OpenGL.GL_LINES);
                    gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                    gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                    gl.End();
                }
                
                

                for (int i = 0; i < dots.Length; i++)
                {
                    
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
            List<int> drawables = new List<int>();
            Rect screenRect = Configs.ScreenRect;


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

            for (int i = 0; i < Primitives.Count; i++)
            {
                if (Primitives[i].intersect(screenRect))
                {
                    drawables.Add(i);
                }
            }
            hoverOn = new Pair<int, TYPE>(-1, TYPE.NONE);
            hoverID = -1;
            
            foreach (int i in drawables) 
            {
                var tmp = Primitives[i].intersect(GLMousePosition,iState.DotID);
                if(tmp.right == TYPE.DOT)
                {
                    hoverID = i;
                    hoverOn = tmp;
                    break;
                }
                else if(tmp.right == TYPE.EDGE) 
                {
                    hoverID = i;
                    hoverOn = tmp;
                    continue;
                }
                else if(tmp.right == TYPE.BODY) 
                {
                    hoverID = i;
                    hoverOn = tmp;
                    continue;
                }
            }
            
            inputHandle();

            this.mousePosition = mousePosition;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(Configs.CameraPos.X, Configs.CameraPos.Y, Configs.CameraPos.Z);




            draw(drawables);

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



        public void addToInputQueue(InputEvent e)
        {
            inputQueue.Enqueue(e);
        }
        public void inputHandle() 
        {
            while (inputQueue.Count > 0)
            {
                var e = inputQueue.Dequeue();
                switch (e.Itype)
                {
                    case KEYT.MOUSE: 
                    {
                        MouseHandle((MouseKeyInput)e);
                        break;
                    }
                    case KEYT.KEYBOARD:
                    {
                        KeyboardHandle((KeyboardInput)e);
                        break;
                    }
                    case KEYT.WHEEL:
                    {
                        MouseWheelInput wheelInput = (MouseWheelInput)e;
                        ChangeScale(wheelInput.delta > 0 ? 1 : -1);
                        break;
                    }
                
                
                }
            }
        }
        private void MouseHandle(MouseKeyInput e) 
        {
            if (e.state == System.Windows.Input.MouseButtonState.Pressed) 
            {
                switch (e.button)
                {
                    case MouseButton.Left:
                        ExecuteButtonDown();
                        break;
                    case MouseButton.Right:
                        break;
                    case MouseButton.Middle:
                        DragCameraSwitch(true);
                        break;
                }
            }
            else 
            {
                switch (e.button)
                {
                    case MouseButton.Left:
                        ExecuteButtonUp();
                        break;
                    case MouseButton.Right:
                        break;
                    case MouseButton.Middle:
                        DragCameraSwitch(false);
                        break;
                }
            }
        }
        private void KeyboardHandle(KeyboardInput e) 
        {
            if (e.down)
            {
                switch (e.key)
                {
                    case Key.LeftCtrl:
                        ControlKeyDown();
                        break;

                }
            }
            else
            {
                switch (e.key)
                {
                    case Key.LeftCtrl:
                        ControlKeyUp();
                        break;
                }
            }
        }
        private void ExecuteButtonDown()
        {
            Vector3 color = new Vector3(1.0f, 0.0f, 0.0f);
            
            Console.WriteLine($" OpenGL: X={GLMousePosition.X:F2}, Y={GLMousePosition.Y:F2}, Y={Configs.CameraPos.Z:F2}");
            
            switch (iState.Istate) 
            {
                case ISTATE.FREE:
                {

                    Primitive line = new Primitive(GLMousePosition, color);
                    Primitives.Add(line);
                    line.addDot(GLMousePosition,0);
                    iState.toggleDraw(Primitives.Count-1,line.getDots().Length-1);
                    break;
                }
                case ISTATE.CONTINUOUSDRAW:
                {
                    if (iState.pID != -1)
                    {
                        if (!Primitives[iState.pID].snap(hoverOn,iState.DotID))
                        {
                            
                            Primitives[iState.pID].addDot(GLMousePosition,iState.DotID);
                            iState.DotID = Primitives[iState.pID].getAmOfDots() - 1;
                        }
                        else 
                        {
                            iState.ToggleFreeState();
                        }
                    }
                    else 
                    {
                        Primitive line = new Primitive(GLMousePosition, color);
                        Primitives.Add(line);
                        line.addDot(GLMousePosition,0);
                        iState.toggleDraw(Primitives.Count - 1, line.getDots().Length - 1);
                        iState.ToggleContiniousDraw(true);
                    }
                    break;
                }
                case ISTATE.DRAW:
                {
                    Primitives[iState.pID].snap(hoverOn,iState.DotID);
                    Primitives[iState.pID].recalcBox();
                    iState.ToggleFreeState();
                    
                    break;
                }
            }
            
        }
        private void ExecuteButtonUp() 
        {

        }
        private void ControlKeyDown() 
        {
            switch (iState.Istate)
            {
                case ISTATE.FREE:
                {
                    iState.ToggleContiniousDraw(true);
                    break;
                }
                case ISTATE.DRAW:
                {
                    iState.ToggleContiniousDraw(true);
                    break;
                }
                case ISTATE.CONTINUOUSDRAW:
                {
                    break;
                }
            }
        }
        private void ControlKeyUp()
        {
            switch (iState.Istate)
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
                    if (iState.pID != -1 )
                    {
                        iState.ToggleContiniousDraw(false);
                    }
                    else 
                    {
                        iState.ToggleFreeState();
                    }
                    break;
                }
            }

        }
        private void DragCameraSwitch(bool state) 
        {
            dragCameraState = state;
        }
        private void ChangeScale(int zoom, float amount = 0.25f)
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
