using System;
using System.Windows;

namespace PersonalFinanceTracker.Services
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex, string message);
        void ShowError(string message, string title);
        void ShowInformation(string message, string title);
        bool ShowConfirmation(string message, string title);
    }

    public class ErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex, string message)
        {
            // Log the error
            System.Diagnostics.Debug.WriteLine($"Error: {ex}");

            // Show user-friendly message
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void ShowError(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void ShowInformation(string message, string title)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message, string title)
        {
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }
    }
} 