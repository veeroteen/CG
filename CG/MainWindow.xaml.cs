using SharpGL;
using SharpGL.WPF;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            handler = new GLHandler(this);
            handler.updateIO();
        }

        private void OpenGLControl_OpenGLDraw(object sender, SharpGL.WPF.OpenGLRoutedEventArgs args)
        {
            if (Configs.Size.X <= 0 || Configs.Size.Y <= 0) return;
            Vector2 mousePosition = Mouse.GetPosition(openGLControl).ToVector2();

            handler.Update(mousePosition);
        }
        private void openGLControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            handler.addToInputQueue(new MouseKeyInput(e));
        }

        private void openGLControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            handler.addToInputQueue(new MouseKeyInput(e));
        }
        private void openGLControl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            handler.addToInputQueue(new MouseWheelInput(e));
        }

        private void openGLControl_KeyDown(object sender, KeyEventArgs e)
        {
            handler.addToInputQueue(new KeyboardInput(e));
        }
        private void openGLControl_KeyUp(object sender, KeyEventArgs e)
        {
            handler.addToInputQueue(new KeyboardInput(e));
        }


        private void openGLControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Configs.changeSize(new Vector2((float)openGLControl.ActualWidth, (float)openGLControl.ActualHeight));
        }

        private void InputModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InputModeComboBox.SelectedItem is ISTATE selectedState)
            {
                handler.addToInputQueue(new UIInputModeChangeEvent(selectedState));

                openGLControl.Focus();
            }
        }

        private void PrevGroup_Click(object sender, RoutedEventArgs e)
        {
            handler.addToInputQueue(new UIGroupChangeEvent(GROUP_ACTION.PREVIOUS));
        }

        private void NextGroup_Click(object sender, RoutedEventArgs e)
        {
            handler.addToInputQueue(new UIGroupChangeEvent(GROUP_ACTION.NEXT));
        }

        private void CurrGroup_Click(object sender, RoutedEventArgs e)
        {
            handler.addToInputQueue(new UIGroupChangeEvent(GROUP_ACTION.CREATE));
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            handler.addToInputQueue(new UIGroupChangeEvent(GROUP_ACTION.DELETE));
        }

        private void LineWidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (handler != null)
            {
                handler.addToInputQueue(new UILineWidthChangeEvent((float)e.NewValue));
            }
        }

        private void ChangeGroupColor_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RgbColorWindow
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                Color color = dialog.SelectedColor;
                handler.addToInputQueue(new UIGroupChangeEvent(GROUP_ACTION.CHANGE_COLOR,new Vector3(color.R/255.0f,color.G/255.0f,color.B/255.0f)));
            }
        }

        private void ChangeModeDraw(object sender, RoutedEventArgs e)
        {
            handler.SetMode(ISTATE.DRAW);
        }
        private void ChangeModeSelect(object sender, RoutedEventArgs e)
        {
            handler.SetMode(ISTATE.SELECTION);
        }
        private void ChangeModeMove(object sender, RoutedEventArgs e)
        {
            handler.SetMode(ISTATE.MOVE);
        }
    }
}