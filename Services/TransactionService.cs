using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(string searchText, DateTime startDate, DateTime endDate);
        Task<Transaction> GetTransactionByIdAsync(int id);
        Task AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Transaction transaction);
    }

    public class TransactionService : ITransactionService
    {
        private readonly FinanceDbContext _context;

        public TransactionService(FinanceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsAsync(string searchText, DateTime startDate, DateTime endDate)
        {
            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(t => 
                    t.Description.Contains(searchText) || 
                    t.Category.Contains(searchText));
            }

            query = query.Where(t => 
                t.Date >= startDate && 
                t.Date <= endDate);

            return await query
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transaction> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task AddTransactionAsync(Transaction transaction)
        {
            transaction.CreatedAt = DateTime.Now;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            var existingTransaction = await _context.Transactions.FindAsync(transaction.Id);
            if (existingTransaction == null)
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");

            existingTransaction.Date = transaction.Date;
            existingTransaction.Type = transaction.Type;
            existingTransaction.Category = transaction.Category;
            existingTransaction.Description = transaction.Description;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            var existingTransaction = await _context.Transactions.FindAsync(transaction.Id);
            if (existingTransaction == null)
                throw new KeyNotFoundException($"Transaction with ID {transaction.Id} not found.");

            _context.Transactions.Remove(existingTransaction);
            await _context.SaveChangesAsync();
        }
    }
} 