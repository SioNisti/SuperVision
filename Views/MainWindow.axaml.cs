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
        private bool _isUpdatingLayout = false;
        public MainWindow()
        {
            InitializeComponent(); 
        }

        private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }
    }
}