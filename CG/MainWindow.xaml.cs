using SharpGL;
using SharpGL.WPF;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CG // Укажите ваше имя проекта
{
    public partial class MainWindow : Window
    {
        private GLHandler handler;
        public MainWindow()
        {
            InitializeComponent();
            openGLControl.Focus();
        }

        private void OpenGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            handler = new GLHandler(openGLControl);
            
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            if (Configs.Size.X <= 0 || Configs.Size.Y <= 0) return;
            Vector2 mousePosition = Mouse.GetPosition(openGLControl).ToVector2();

            handler.Update(mousePosition);
        }
        private void openGLControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    handler.ExecuteButtonDown();
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:
                    handler.DragCameraSwitch(true);
                    break;
            }
        }

        private void openGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    handler.ExecuteButtonUp();
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:
                    handler.DragCameraSwitch(false);
                    break;
            }
        }
        private void openGLControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            handler.ChangeScale(e.Delta > 0 ? 1 : -1);
        }

        private void openGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key) 
            {
                case Key.LeftCtrl:
                    handler.ControlKeyDown();
                    break;
            
            }
        }
        private void openGLControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.LeftCtrl:
                    handler.ControlKeyUp();
                    break;

            }
        }


        private void openGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Configs.changeSize(new Vector2((float)openGLControl.ActualWidth, (float)openGLControl.ActualHeight));
        }


    }
}