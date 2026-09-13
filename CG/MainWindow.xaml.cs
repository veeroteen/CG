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
            if (Configs.Size.X <= 0 || Configs.Size.Y <= 0) return;
            Vector2 mousePosition = Mouse.GetPosition(openGLControl).ToVector2();

            handler.Update(mousePosition);
        }
        private void openGLControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            handler.MouseClick();
        }
        private void openGLControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            handler.ChangeScale(e.Delta > 0 ? 1 : -1);
        }

        private void openGLControl_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void openGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Configs.changeSize(new Vector2((float)openGLControl.ActualWidth, (float)openGLControl.ActualHeight));
        }

    }
}