using System;
using System.Collections.Generic;
using System.Linq;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public interface IValidationService
    {
        (bool IsValid, IEnumerable<string> Errors) ValidateTransaction(Transaction transaction);
    }

    public class ValidationService : IValidationService
    {
        public (bool IsValid, IEnumerable<string> Errors) ValidateTransaction(Transaction transaction)
        {
            var errors = new List<string>();

            if (transaction == null)
            {
                errors.Add("Transaction cannot be null");
                return (false, errors);
            }

            if (transaction.Amount <= 0)
            {
                errors.Add("Amount must be greater than zero");
            }

            if (string.IsNullOrWhiteSpace(transaction.Category))
            {
                errors.Add("Category is required");
            }
            else if (transaction.Category.Length > 50)
            {
                errors.Add("Category cannot exceed 50 characters");
            }

            if (string.IsNullOrWhiteSpace(transaction.Description))
            {
                errors.Add("Description is required");
            }
            else if (transaction.Description.Length > 200)
            {
                errors.Add("Description cannot exceed 200 characters");
            }

            if (transaction.Date > DateTime.Now)
            {
                errors.Add("Transaction date cannot be in the future");
            }

            return (!errors.Any(), errors);
        }
    }
} 