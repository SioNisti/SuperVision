using Avalonia.Controls;
using Avalonia.Interactivity;
using SuperVision.ViewModels;

namespace SuperVision.Views
{
    public partial class WidgetEditor : Window
    {
        private readonly MainWindowViewModel? _vm;

        public WidgetEditor()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}