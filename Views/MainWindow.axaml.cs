using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Reactive;

namespace SuperVision.Views
{
    public partial class MainWindow : Window
    {
        //private double _currentZoom = 1.0;
        //private bool _isProgrammaticResize = false;
        public MainWindow()
        {
            InitializeComponent(); 
            /*
            Container.SizeChanged += Container_SizeChanged;
            this.SizeChanged += (s, e) => OnWindowResize(e.NewSize);*/
        }

        private void Window_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        }
        /*
        private void Container_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height > 0 && e.NewSize.Height != e.PreviousSize.Height)
            {
                double targetWindowHeight = e.NewSize.Height * _currentZoom;

                _isProgrammaticResize = true;
                Height = targetWindowHeight;
                _isProgrammaticResize = false;
            }
        }

        private void OnWindowResize(Avalonia.Size newWindowSize)
        {
            if (_isProgrammaticResize) return;
            if (newWindowSize.Height <= 0 || Container.Bounds.Height <= 0) return;

            _currentZoom = newWindowSize.Height / Container.Bounds.Height;

            var transform = Transformer.LayoutTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform();
                Transformer.LayoutTransform = transform;
            }

            transform.ScaleX = _currentZoom;
            transform.ScaleY = _currentZoom;

            Container.Width = newWindowSize.Width / _currentZoom;
        }*/
    }
}