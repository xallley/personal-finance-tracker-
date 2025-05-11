using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Commands;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;

namespace PersonalFinanceTracker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IValidationService _validationService;
        private readonly IBackupService _backupService;
        private readonly IExportService _exportService;
        private readonly IErrorHandler _errorHandler;
        private ObservableCollection<Transaction> _transactions = new();
        private Transaction? _selectedTransaction;
        private string _searchText = string.Empty;
        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        private DateTime _endDate = DateTime.Today;
        private bool _isLoading;
        private string _errorMessage = string.Empty;
        private DateTime? _lastBackupDate;

        public MainViewModel(
            ITransactionService transactionService,
            IValidationService validationService,
            IBackupService backupService,
            IExportService exportService,
            IErrorHandler errorHandler)
        {
            _transactionService = transactionService;
            _validationService = validationService;
            _backupService = backupService;
            _exportService = exportService;
            _errorHandler = errorHandler;
            
            AddTransactionCommand = new AsyncRelayCommand(AddTransactionAsync);
            EditTransactionCommand = new AsyncRelayCommand(EditTransactionAsync, _ => SelectedTransaction != null);
            DeleteTransactionCommand = new AsyncRelayCommand(DeleteTransactionAsync, _ => SelectedTransaction != null);
            RefreshCommand = new AsyncRelayCommand(LoadTransactionsAsync);
            ExportToCsvCommand = new AsyncRelayCommand(ExportToCsvAsync);
            ExportToExcelCommand = new AsyncRelayCommand(ExportToExcelAsync);
            GenerateMonthlyReportCommand = new AsyncRelayCommand(GenerateMonthlyReportAsync);
            CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
            RestoreFromBackupCommand = new AsyncRelayCommand(RestoreFromBackupAsync);

            _ = Task.WhenAll(
                LoadTransactionsAsync(),
                LoadLastBackupDateAsync()
            );
        }

        public ICommand AddTransactionCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ExportToExcelCommand { get; }
        public ICommand GenerateMonthlyReportCommand { get; }
        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreFromBackupCommand { get; }

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        public Transaction? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (SetProperty(ref _selectedTransaction, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    _ = LoadTransactionsAsync();
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                    _ = LoadTransactionsAsync();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (SetProperty(ref _endDate, value))
                    _ = LoadTransactionsAsync();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public DateTime? LastBackupDate
        {
            get => _lastBackupDate;
            set => SetProperty(ref _lastBackupDate, value);
        }

        public decimal TotalIncome => Transactions?.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) ?? 0;
        public decimal TotalExpenses => Transactions?.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount) ?? 0;
        public decimal Balance => TotalIncome - TotalExpenses;

        private async Task LoadTransactionsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var transactions = await _transactionService.GetTransactionsAsync(SearchText, StartDate, EndDate);
                Transactions = new ObservableCollection<Transaction>(transactions);

                OnPropertyChanged(nameof(TotalIncome));
                OnPropertyChanged(nameof(TotalExpenses));
                OnPropertyChanged(nameof(Balance));
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to load transactions";
                _errorHandler.HandleError(ex, "Failed to load transactions. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadLastBackupDateAsync()
        {
            try
            {
                LastBackupDate = await _backupService.GetLastBackupDateAsync();
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to get last backup date.");
            }
        }

        private async Task AddTransactionAsync()
        {
            try
            {
                var dialog = new TransactionDialog();
                if (dialog.ShowDialog() == true)
                {
                    var (isValid, errors) = _validationService.ValidateTransaction(dialog.Transaction);
                    if (!isValid)
                    {
                        _errorHandler.ShowError(string.Join("\n", errors), "Validation Error");
                        return;
                    }

                    IsLoading = true;
                    ErrorMessage = string.Empty;

                    await _transactionService.AddTransactionAsync(dialog.Transaction);
                    await LoadTransactionsAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to add transaction";
                _errorHandler.HandleError(ex, "Failed to add transaction. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditTransactionAsync()
        {
            if (SelectedTransaction == null) return;

            try
            {
                var dialog = new TransactionDialog(SelectedTransaction);
                if (dialog.ShowDialog() == true)
                {
                    var (isValid, errors) = _validationService.ValidateTransaction(dialog.Transaction);
                    if (!isValid)
                    {
                        _errorHandler.ShowError(string.Join("\n", errors), "Validation Error");
                        return;
                    }

                    IsLoading = true;
                    ErrorMessage = string.Empty;

                    await _transactionService.UpdateTransactionAsync(dialog.Transaction);
                    await LoadTransactionsAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to update transaction";
                _errorHandler.HandleError(ex, "Failed to update transaction. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTransactionAsync()
        {
            if (SelectedTransaction == null) return;

            try
            {
                var result = _errorHandler.ShowConfirmation(
                    "Are you sure you want to delete this transaction?",
                    "Confirm Delete");

                if (result)
                {
                    IsLoading = true;
                    ErrorMessage = string.Empty;

                    await _transactionService.DeleteTransactionAsync(SelectedTransaction);
                    await LoadTransactionsAsync();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Failed to delete transaction";
                _errorHandler.HandleError(ex, "Failed to delete transaction. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToCsvAsync()
        {
            try
            {
                IsLoading = true;
                var fileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var filePath = await _exportService.ExportToCsvAsync(Transactions, fileName);
                _errorHandler.ShowInformation($"Transactions exported to:\n{filePath}", "Export Successful");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to export transactions to CSV.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                IsLoading = true;
                var fileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                var filePath = await _exportService.ExportToExcelAsync(Transactions, fileName);
                _errorHandler.ShowInformation($"Transactions exported to:\n{filePath}", "Export Successful");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to export transactions to Excel.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateMonthlyReportAsync()
        {
            try
            {
                IsLoading = true;
                var filePath = await _exportService.GenerateMonthlyReportAsync(DateTime.Now);
                _errorHandler.ShowInformation($"Monthly report generated at:\n{filePath}", "Report Generated");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to generate monthly report.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateBackupAsync()
        {
            try
            {
                IsLoading = true;
                var backupPath = await _backupService.CreateBackupAsync();
                await LoadLastBackupDateAsync();
                _errorHandler.ShowInformation($"Backup created at:\n{backupPath}", "Backup Created");
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to create backup.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RestoreFromBackupAsync()
        {
            try
            {
                var result = _errorHandler.ShowConfirmation(
                    "This will replace all current data with the backup data. Are you sure you want to continue?",
                    "Confirm Restore");

                if (!result) return;

                IsLoading = true;
                
                var dialog = new OpenFileDialog
                {
                    Filter = "Backup files (*.json)|*.json",
                    InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups"),
                    Title = "Select backup file to restore"
                };

                if (dialog.ShowDialog() == true)
                {
                    await _backupService.RestoreFromBackupAsync(dialog.FileName);
                    await LoadTransactionsAsync();
                    _errorHandler.ShowInformation("Data restored successfully.", "Restore Complete");
                }
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, "Failed to restore from backup.");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 