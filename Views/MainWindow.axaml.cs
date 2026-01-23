using Avalonia.Controls;
using Avalonia.Input;

namespace SuperVision.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Only start dragging on left mouse button
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }
    }
}