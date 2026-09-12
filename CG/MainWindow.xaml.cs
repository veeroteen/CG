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
        }

        private void OpenGLControl_OpenGLInitialized(object sender, EventArgs e)
        {
            handler = new GLHandler(openGLControl);
            
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            Vector2 size = new Vector2((float)openGLControl.ActualWidth, (float)openGLControl.ActualHeight);
            if (size.X <= 0 || size.Y <= 0) return;
            Vector2 mousePosition = Mouse.GetPosition(openGLControl).ToVector2();

            handler.Update(size, mousePosition);
        }
        private void Mouse_Down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            handler.MouseClick();
        }
        private void Mouse_Wheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            handler.ChangeScale(e.Delta > 0 ? 1 : -1);
        }
    }
}