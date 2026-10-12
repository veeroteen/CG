using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
namespace CG
{

    class GLHandler
    {
        private OpenGL gl;
        private List<Primitive> Primitives;
        private List<List<int>> groups;
        private List<float> groupLineWidths;
        private List<Vector3> groupsColor;
        private int curGroup = 0;
        private OSTATE oState = OSTATE.MESH;
        private IState iState;
        private bool dragCameraState = false;
        private Vector2 mousePosition;
        private Vector3 GLMousePosition;
        private MainWindow _window;
        private Pair<int, TYPE> hoverOn = new Pair<int, TYPE>(-1, TYPE.NONE);
        private int hoverID = -1;
        private Queue<InputEvent> inputQueue = new Queue<InputEvent>();
        private List<Selection> selected;

        private readonly Vector3 _selectedColor = new Vector3(0.1f, 0.1f, 1.0f);
        private readonly Vector3 _hoverColor = new Vector3(1.0f, 1.0f, 1.0f);
        private readonly Vector3 _vertexColor = new Vector3(0.0f, 1.0f, 0.0f);

        private readonly Vector3 _defaultLineColor = new Vector3(1.0f, 0.3f, 0.3f);

        private float hoverObjectWidth = 1.5f;

        public GLHandler(MainWindow window)
        {
            selected = new List<Selection>();
            groups = new List<List<int>>();
            groups.Add(new List<int>());
            groupsColor = new List<Vector3>();
            

            iState = new IState(updateIO, ISTATE.DRAW);

            _window = window;
            Primitives = new List<Primitive>();
            gl = window.openGLControl.OpenGL;
            gl.ClearColor(0.5f, 0.5f, 0.5f, 1.0f);
            gl.PointSize(6.0f);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);

            groupLineWidths = new List<float>();
            groupLineWidths.Add(1.5f);
            groupsColor.Add(_defaultLineColor);
            updateGroupNumber();

        }

        private void draw(List<int> drawables)
        {
            foreach (var s in selected)
            {
                float width = 0;
                for (int i = 0; i < groupLineWidths.Count; i++)
                {
                    if (groups[i].Contains(s.pid))
                    {
                        width = groupLineWidths[i];
                    }
                }
                var dots = Primitives[s.pid].getDots();
                var edges = Primitives[s.pid].getEdges();
                switch (s.type)
                {
                    case TYPE.BODY:
                        {
                            gl.Color(_selectedColor.X, _selectedColor.Y, _selectedColor.Z);
                            gl.LineWidth(4.0f + width);
                            foreach (var edge in edges)
                            {
                                gl.Begin(OpenGL.GL_LINES);
                                gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                                gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                                gl.End();
                            }

                            gl.PointSize(8.0f);
                            gl.Color(_selectedColor.X, _selectedColor.Y, _selectedColor.Z);
                            foreach (var dot in Primitives[s.pid].getDots())
                            {
                                gl.Begin(OpenGL.GL_POINTS);
                                gl.Vertex(dot.X, dot.Y, dot.Z);
                                gl.End();
                            }
                            break;
                        }
                    case TYPE.DOT:
                        {
                            gl.PointSize(8.0f);
                            gl.Color(_selectedColor.X, _selectedColor.Y, _selectedColor.Z);
                            gl.Begin(OpenGL.GL_POINTS);
                            gl.Vertex(dots[s.id].X, dots[s.id].Y, dots[s.id].Z);
                            gl.End();
                            break;
                        }
                    case TYPE.EDGE:
                        {
                            gl.Color(_selectedColor.X, _selectedColor.Y, _selectedColor.Z);
                            gl.LineWidth(4.0f + width);
                            gl.Begin(OpenGL.GL_LINES);
                            gl.Vertex(dots[edges[s.id].left].X, dots[edges[s.id].left].Y, dots[edges[s.id].left].Z);
                            gl.Vertex(dots[edges[s.id].right].X, dots[edges[s.id].right].Y, dots[edges[s.id].right].Z);
                            gl.End();
                            break;
                        }
                }
            }

            if (hoverOn.right == TYPE.EDGE)
            {
                var dots = Primitives[hoverID].getDots();
                gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);

                gl.LineWidth(hoverObjectWidth + 6.0f);

                var edge = Primitives[hoverID].getEdges()[hoverOn.left];
                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                gl.End();
            }

            if (hoverOn.right == TYPE.DOT)
            {
                var dots = Primitives[hoverID].getDots();
                gl.PointSize(hoverObjectWidth + 8.0f);
                gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                gl.Begin(OpenGL.GL_POINTS);
                gl.Vertex(dots[hoverOn.left].X, dots[hoverOn.left].Y, dots[hoverOn.left].Z);
                gl.End();
            }

            if (hoverOn.right == TYPE.BODY)
            {
                var dots = Primitives[hoverID].getDots();
                gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);

                gl.LineWidth(hoverObjectWidth + 6.0f);

                foreach (var edge in Primitives[hoverID].getEdges())
                {
                    gl.Begin(OpenGL.GL_LINES);
                    gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                    gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                    gl.End();
                }

                gl.PointSize(hoverObjectWidth + 8.0f);
                gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                for (int i = 0; i < dots.Length; i++)
                {
                    gl.Begin(OpenGL.GL_POINTS);
                    gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);
                    gl.End();
                }

                var center = Primitives[hoverID].Center;
                float size = 0.1f;
                gl.LineWidth(3.0f);
                gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(center.X - size, center.Y, center.Z);
                gl.Vertex(center.X + size, center.Y, center.Z);
                gl.Vertex(center.X, center.Y - size, center.Z);
                gl.Vertex(center.X, center.Y + size, center.Z);
                gl.End();
            }

            if (iState.Istate == ISTATE.MOVE && iState.pID != -1)
            {
                float width = 0;
                for (int i = 0; i < groupLineWidths.Count; i++)
                {
                    if (groups[i].Contains(iState.pID))
                    {
                        width = groupLineWidths[i];
                    }
                }

                switch (iState.type)
                {
                    case TYPE.DOT:
                        {
                            var dots = Primitives[iState.pID].getDots();
                            gl.PointSize(8.0f);
                            gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                            gl.Begin(OpenGL.GL_POINTS);
                            gl.Vertex(dots[iState.DotID].X, dots[iState.DotID].Y, dots[iState.DotID].Z);
                            gl.End();
                            break;
                        }
                    case TYPE.EDGE:
                        {
                            var dots = Primitives[iState.pID].getDots();
                            gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                            gl.LineWidth(4.0f + width);

                            var edge = Primitives[iState.pID].getEdges()[iState.DotID];

                            gl.Begin(OpenGL.GL_LINES);
                            gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                            gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                            gl.End();

                            gl.PointSize(8.0f);
                            gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                            gl.Begin(OpenGL.GL_POINTS);
                            gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                            gl.End();
                            gl.Begin(OpenGL.GL_POINTS);
                            gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                            gl.End();
                            break;
                        }
                    case TYPE.BODY:
                        {
                            var dots = Primitives[iState.pID].getDots();
                            gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);
                            gl.LineWidth(4.0f + width);

                            foreach (var edge in Primitives[iState.pID].getEdges())
                            {
                                gl.Begin(OpenGL.GL_LINES);
                                gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                                gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                                gl.End();
                            }
                            gl.PointSize(8.0f);
                            gl.Color(_hoverColor.X, _hoverColor.Y, _hoverColor.Z);

                            for (int i = 0; i < dots.Length; i++)
                            {
                                gl.Begin(OpenGL.GL_POINTS);
                                gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);
                                gl.End();
                            }
                            break;
                        }
                }
            }

            for (int g = 0; g < groups.Count; g++)
            {
                gl.LineWidth(groupLineWidths[g]);
                var color = groupsColor[g];
                foreach (int primitive in groups[g])
                {
                    if (!drawables.Contains(primitive)) continue;

                    var dots = Primitives[primitive].getDots();
                    


                    gl.Color(color.X, color.Y, color.Z);
                    

                    foreach (var edge in Primitives[primitive].getEdges())
                    {
                        gl.Begin(OpenGL.GL_LINES);
                        gl.Vertex(dots[edge.left].X, dots[edge.left].Y, dots[edge.left].Z);
                        gl.Vertex(dots[edge.right].X, dots[edge.right].Y, dots[edge.right].Z);
                        gl.End();
                    }

                    for (int i = 0; i < dots.Length; i++)
                    {
                        gl.PointSize(6.0f);
                        gl.Color(_vertexColor.X, _vertexColor.Y, _vertexColor.Z);
                        gl.Begin(OpenGL.GL_POINTS);
                        gl.Vertex(dots[i].X, dots[i].Y, dots[i].Z);
                        gl.End();
                    }
                }
            }
        }

        public void Update(Vector2 mousePosition)
        {
            List<int> drawables = new List<int>();
            float dx = GLMousePosition.X, dy = GLMousePosition.Y;
            this.mousePosition = mousePosition;

            if (dragCameraState)
            {
                dx = GLMousePosition.X;
                dy = GLMousePosition.Y;
                Vector3 tmp = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
                dx = tmp.X - dx;
                dy = tmp.Y - dy;
                Configs.changeCameraPos(dx + Configs.CameraPos.X, dy + Configs.CameraPos.Y);
                GLMousePosition = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
            }
            else
            {
                GLMousePosition = Translator.toScreen(mousePosition, Configs.CameraPos.Z);
                dx = GLMousePosition.X - dx;
                dy = GLMousePosition.Y - dy;
            }
            inputHandle();

            for (int i = 0; i < Primitives.Count; i++)
            {
                if (Primitives[i].intersect(Configs.ScreenRect))
                {
                    drawables.Add(i);
                }
            }
            hoverOn = new Pair<int, TYPE>(-1, TYPE.NONE);
            hoverID = -1;


            foreach (int i in drawables)
            {
                var tmp = Primitives[i].intersect(GLMousePosition, iState.DotID);

                if (tmp.right == TYPE.DOT)
                {
                    hoverID = i;
                    hoverOn = tmp;
                    UpdateHoverOutlineWidth(i);
                    break;
                }
                else if (tmp.right == TYPE.EDGE)
                {
                    hoverID = i;
                    hoverOn = tmp;
                    UpdateHoverOutlineWidth(i);
                    continue;
                }
                else if (tmp.right == TYPE.BODY && (iState.pID != i))
                {
                    hoverID = i;
                    hoverOn = tmp;
                    UpdateHoverOutlineWidth(i);
                    continue;
                }
            }

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate(Configs.CameraPos.X, Configs.CameraPos.Y, Configs.CameraPos.Z);

            if (iState.pID != -1)
            {
                switch (iState.type)
                {
                    case TYPE.DOT:
                        Primitives[iState.pID].changeDotPosFlow(new Vector3((float)GLMousePosition.X, (float)GLMousePosition.Y, 0), iState.DotID);
                        break;
                    case TYPE.EDGE:
                        Primitives[iState.pID].changeEdgePosFlow(new Vector3(dx, dy, 0), iState.DotID);
                        break;
                    case TYPE.BODY:
                        Primitives[iState.pID].changePosFlow(new Vector3(dx, dy, 0));
                        break;
                }
            }

            draw(drawables);
            gl.Flush();
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
                        MouseHandle((MouseKeyInput)e);
                        break;

                    case KEYT.KEYBOARD:
                        KeyboardHandle((KeyboardInput)e);
                        break;

                    case KEYT.WHEEL:
                        MouseWheelInput wheelInput = (MouseWheelInput)e;
                        ChangeScale(wheelInput.delta > 0 ? 1 : -1);
                        break;

                    case KEYT.UISTATE:
                        UIInputModeChangeEvent uiEvent = (UIInputModeChangeEvent)e;
                        ApplyStateFromUI(uiEvent.NewState);
                        break;

                    case KEYT.UIGROUP:
                        if (e is UILineWidthChangeEvent widthEvent)
                            groupLineWidths[curGroup] = widthEvent.Width;
                        else
                            UIHandle((UIGroupChangeEvent)e);
                        break;
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
                        ExecuteAction();
                        break;
                    case MouseButton.Right:
                        CancelAction();
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
                    case Key.T:
                        ToggleIMode();
                        break;
                    case Key.G:
                        {
                            toggleGroup();
                            break;
                        }
                    case Key.F:
                        {
                            groups.Add(new List<int>());
                            groupLineWidths.Add(1.5f);
                            groupsColor.Add(_defaultLineColor);
                            curGroup = groups.Count - 1;
                            updateGroupNumber();
                            break;
                        }
                    case Key.A:
                        {
                            if (iState.Istate == ISTATE.SELECTION)
                            {
                                for (int i = 0; i < groups[curGroup].Count; i++)
                                {
                                    SelectedAdd(new Selection(TYPE.BODY, groups[curGroup][i], -1));
                                }
                            }
                            break;
                        }
                    case Key.Delete:
                        {
                            if (iState.Istate == ISTATE.SELECTION)
                            {
                                DeleteButtonDown();
                            }
                            break;
                        }
                }
            }
        }
        private void UIHandle(UIGroupChangeEvent e)
        {
            switch (e.Action)
            {
                case GROUP_ACTION.PREVIOUS:
                    curGroup--;
                    if (curGroup < 0) curGroup = groups.Count - 1;
                    updateGroupNumber();
                    break;

                case GROUP_ACTION.NEXT:
                    curGroup++;
                    if (curGroup > groups.Count - 1) curGroup = 0;
                    updateGroupNumber();
                    break;

                case GROUP_ACTION.CREATE:
                    groups.Add(new List<int>());
                    groupLineWidths.Add(1.5f);
                    groupsColor.Add(_defaultLineColor);
                    curGroup = groups.Count - 1;
                    updateGroupNumber();
                    break;

                case GROUP_ACTION.DELETE:
                    DeletePrimitivesByIDs(groups[curGroup].ToList());

                    if (groups.Count > 1)
                    {
                        groups.RemoveAt(curGroup);
                        groupLineWidths.RemoveAt(curGroup);
                        curGroup--;
                        if (curGroup < 0) curGroup = 0;
                    }
                    else if (groups.Count == 1)
                    {
                        groupsColor[curGroup] = _defaultLineColor;
                        groupLineWidths[curGroup] = 1.5f;
                    }

                    selected.Clear();
                    iState.SetState(ISTATE.DRAW);
                    updateGroupNumber();
                    break;
                case GROUP_ACTION.CHANGE_GROUP:
                    groupsColor[curGroup] = e.vector;
                    groupLineWidths[curGroup] = e.value;
                    break;
            }
        }
        private void ToggleIMode()
        {
            if (iState.Istate == ISTATE.DRAW) iState.SetState(ISTATE.MOVE);
            else if (iState.Istate == ISTATE.MOVE) iState.SetState(ISTATE.SELECTION);
            else if (iState.Istate == ISTATE.SELECTION) iState.SetState(ISTATE.DRAW);
        }
        public void SetMode(ISTATE state) 
        {
            iState.SetState(state);
        }


        public void updateIO()
        {
            if (_window.InputModeComboBox != null)
            {
                if (_window.InputModeComboBox.ItemsSource == null)
                {
                    _window.InputModeComboBox.ItemsSource = new ISTATE[]
                    {
                        ISTATE.DRAW,
                        ISTATE.MOVE,
                        ISTATE.SELECTION
                    };
                }

                _window.InputModeComboBox.SelectedItem = iState.Istate;
            }
        }
        public void updateGroupNumber()
        {
            _window.CurrGroup.Content = curGroup.ToString();
        }
        public float GetCurrentGroupLineWidth()
        {
            return groupLineWidths[curGroup];
        }

        private void ExecuteAction()
        {

            switch (iState.Istate)
            {
                case ISTATE.DRAW:
                    if (iState.pID != -1)
                    {
                        if (iState.pID != hoverID)
                        {
                            Primitives[iState.pID].addDot(GLMousePosition, iState.DotID);
                            iState.SetActiveDrawPrimitive(iState.pID, Primitives[iState.pID].getAmOfDots() - 1);
                        }
                        else if(Primitives[iState.pID].snap(hoverOn, iState.DotID))
                        {
                            Primitives[iState.pID].recalcCenter();
                            iState.SetState(ISTATE.DRAW);
                        }
                    }
                    else
                    {
                        if (hoverID == -1)
                        {
                            Primitive line = AddPrimitive(GLMousePosition);
                            line.addDot(GLMousePosition, 0);
                            iState.SetActiveDrawPrimitive(Primitives.Count - 1, line.getDots().Length - 1);
                        }
                        else
                        {
                            if (hoverOn.right == TYPE.DOT)
                            {
                                Primitives[hoverID].addDot(GLMousePosition, hoverOn.left);
                                iState.SetActiveDrawPrimitive(hoverID, Primitives[hoverID].getDots().Length - 1);
                            }
                            else if (hoverOn.right == TYPE.EDGE)
                            {
                                Primitives[hoverID].addDot(GLMousePosition);
                                Primitives[hoverID].divideEdge(hoverOn.left, Primitives[hoverID].getDots().Length - 1);
                                Primitives[hoverID].addDot(GLMousePosition, Primitives[hoverID].getDots().Length - 1);
                                iState.SetActiveDrawPrimitive(hoverID, Primitives[hoverID].getDots().Length - 1);
                            }
                        }
                    }
                    break;

                case ISTATE.MOVE:
                    if (hoverID != -1)
                    {
                        iState.pID = hoverID;
                        iState.DotID = hoverOn.left;
                        iState.type = hoverOn.right;
                    }
                    else if (iState.pID != -1)
                    {
                        Primitives[iState.pID].recalcCenter();
                        Primitives[iState.pID].recalcBox();
                        iState.SetState(ISTATE.MOVE);
                    }
                    break;

                case ISTATE.SELECTION:
                    if (hoverID != -1)
                    {
                        SelectedAdd(new Selection(hoverOn.right, hoverID, hoverOn.left));
                    }
                    break;
            }
        }
        private void CancelAction()
        {
            switch (iState.Istate)
            {
                case ISTATE.DRAW:
                    if (iState.pID != -1)
                    {
                        Primitives[iState.pID].removeDot(iState.DotID);
                        Primitives[iState.pID].recalcMISK();
                        iState.SetState(ISTATE.DRAW);
                    }
                    break;

                case ISTATE.SELECTION or ISTATE.MOVE:
                    selected.Clear();
                    break;
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

        private void DeleteButtonDown()
        {
            var bodyIDs = selected
                .Where(x => x.type == TYPE.BODY)
                .Select(x => x.pid);

            DeletePrimitivesByIDs(bodyIDs);


            foreach (var select in selected
                .Where(x => x.type == TYPE.EDGE)
                .OrderByDescending(x => x.id))
            {
                if (select.pid < Primitives.Count && Primitives[select.pid] != null)
                    Primitives[select.pid].removeEdge(select.id);
            }

            foreach (var select in selected
                .Where(x => x.type == TYPE.DOT)
                .OrderByDescending(x => x.id))
            {
                if (select.pid < Primitives.Count && Primitives[select.pid] != null)
                    Primitives[select.pid].removeDot(select.id);
            }

            selected.Clear();
        }

        private void SelectedAdd(Selection select)
        {
            switch (select.type)
            {
                case TYPE.BODY:
                    {
                        selected.RemoveAll(x => x.pid == select.pid);
                        selected.Add(select);
                        break;
                    }
                case TYPE.DOT:
                case TYPE.EDGE:
                    {
                        if (!selected.Contains(select) && !selected.Any(x => x.pid == select.pid && x.type == TYPE.BODY))
                        {
                            selected.Add(select);
                        }
                        break;
                    }
            }
        }
        private Primitive AddPrimitive(Vector3 dot)
        {
            Primitive line = new Primitive(dot);
            Primitives.Add(line);
            groups[curGroup].Add(Primitives.Count - 1);
            return line;
        }

        private void toggleGroup()
        {
            curGroup++;
            if (curGroup > groups.Count - 1)
            {
                curGroup = 0;
            }
            updateGroupNumber();
        }


        public void ApplyStateFromUI(ISTATE state)
        {
            iState.SetState(state);
        }
        private void DeletePrimitivesByIDs(IEnumerable<int> ids)
        {
            var toDelete = ids.OrderByDescending(id => id).ToList();

            foreach (int id in toDelete)
            {
                MarkPrimitiveDeletion(id);
            }

            Primitives.RemoveAll(x => x == null);
        }
        private void MarkPrimitiveDeletion(int id)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                var j = groups[i].IndexOf(id);
                if (j != -1)
                {
                    groups[i].RemoveAt(j);
                    break;
                }
            }
            Primitives[id] = null;

            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = 0; j < groups[i].Count; j++)
                {
                    if (groups[i][j] > id)
                    {
                        groups[i][j]--;
                    }
                }
            }
        }

        private void UpdateHoverOutlineWidth(int i)
        {
            for (int g = 0; g < groups.Count; g++)
            {
                if (groups[g].Contains(i))
                {
                    hoverObjectWidth = groupLineWidths[g];
                    break;
                }
            }
        }

    }
}
