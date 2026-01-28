using Avalonia.Controls;
using Avalonia.Interactivity;
using SuperVision.ViewModels;

namespace SuperVision.Views
{
    public partial class LayoutEditor : Window
    {
        public LayoutEditor()
        {
            InitializeComponent();
        }

        public LayoutEditor(MainWindowViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            button.ContextMenu?.Open(button);
        }
    }
}