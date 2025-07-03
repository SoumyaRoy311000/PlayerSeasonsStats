using System.Windows;
using LMStudioClone.ViewModels;

namespace LMStudioClone
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Handle navigation
            NavigationView.SelectionChanged += (s, e) =>
            {
                if (e.SelectedItem is ModernWpf.Controls.NavigationViewItem item)
                {
                    viewModel.NavigateToView(item.Tag?.ToString() ?? "Chat");
                }
            };

            // Set default selection
            NavigationView.SelectedItem = NavigationView.MenuItems[0];
        }
    }
}

