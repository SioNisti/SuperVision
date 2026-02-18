using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using System;

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