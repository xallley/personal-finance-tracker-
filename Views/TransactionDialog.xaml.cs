using System.Windows;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.ViewModels;

namespace PersonalFinanceTracker.Views
{
    public partial class TransactionDialog : Window
    {
        private readonly TransactionDialogViewModel _viewModel;

        public TransactionDialog(Transaction? transaction = null)
        {
            InitializeComponent();
            _viewModel = new TransactionDialogViewModel(transaction);
            DataContext = _viewModel;
        }

        public Transaction Transaction => _viewModel.Transaction;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Validate())
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 