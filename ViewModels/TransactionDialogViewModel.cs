using PersonalFinanceTracker.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Windows.Input;
using PersonalFinanceTracker.Commands;

namespace PersonalFinanceTracker.ViewModels
{
    public class TransactionDialogViewModel : ViewModelBase
    {
        private Transaction _transaction;
        private string? _errorMessage;
        public bool IsEditMode { get; private set; }
        public string Title => IsEditMode ? "Edit Transaction" : "Add Transaction";
        public IEnumerable<TransactionType> TransactionTypes => Enum.GetValues<TransactionType>();
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public TransactionDialogViewModel(Transaction? transaction = null)
        {
            _transaction = transaction ?? new Transaction
            {
                Date = DateTime.Now,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            SaveCommand = new RelayCommand(_ => Validate());
            CancelCommand = new RelayCommand(_ => { });
        }

        public Transaction Transaction
        {
            get => _transaction;
            set => SetProperty(ref _transaction, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string Amount
        {
            get => _transaction.Amount.ToString("0.00");
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _errorMessage = "Amount is required";
                    OnPropertyChanged(nameof(ErrorMessage));
                    return;
                }

                try
                {
                    if (decimal.TryParse(value, out decimal amount))
                    {
                        if (amount <= 0)
                        {
                            _errorMessage = "Amount must be greater than 0";
                            OnPropertyChanged(nameof(ErrorMessage));
                            return;
                        }

                        if (amount > 999999999.99m)
                        {
                            _errorMessage = "Amount is too large";
                            OnPropertyChanged(nameof(ErrorMessage));
                            return;
                        }

                        _transaction.Amount = amount;
                        _errorMessage = null;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(ErrorMessage));
                    }
                    else
                    {
                        _errorMessage = "Amount must be a valid number";
                        OnPropertyChanged(nameof(ErrorMessage));
                    }
                }
                catch (OverflowException)
                {
                    _errorMessage = "Amount is too large";
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        public bool Validate()
        {
            try
            {
                if (Transaction.Amount <= 0)
                {
                    ErrorMessage = "Amount must be greater than 0";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Transaction.Description))
                {
                    ErrorMessage = "Description is required";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Transaction.Category))
                {
                    ErrorMessage = "Category is required";
                    return false;
                }

                ErrorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
    }
} 