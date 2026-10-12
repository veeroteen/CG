
using System.Windows;
using System.Windows.Media;

namespace CG
{
    public partial class RgbColorWindow : Window
    {
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
        public float Width { get; private set; }

        public RgbColorWindow()
        {
            InitializeComponent();
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!byte.TryParse(RedBox.Text, out byte r) ||
                !byte.TryParse(GreenBox.Text, out byte g) ||
                !byte.TryParse(BlueBox.Text, out byte b))
            {
                MessageBox.Show(
                    "Введите целые числа от 0 до 255.",
                    "Ошибка ввода",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            Red = r;
            Green = g;
            Blue = b;
            Width = (float)LineWidthSlider.Value;

            DialogResult = true;
        }

        public Color SelectedColor =>
            Color.FromRgb(Red, Green, Blue);

        public float SelectedWidth => Width;
    }
}