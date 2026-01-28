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
        public LayoutEditor(LayoutEditorViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
        }
    }
}