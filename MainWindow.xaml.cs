using System.Windows;
using PersonalFinanceTracker.ViewModels;
using PersonalFinanceTracker.Views;

namespace PersonalFinanceTracker
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddTransactionCommand.Execute(null);
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.EditTransactionCommand.Execute(null);
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DeleteTransactionCommand.Execute(null);
        }
    }
} 