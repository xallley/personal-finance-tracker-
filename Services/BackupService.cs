using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Models;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace PersonalFinanceTracker.Services
{
    public interface IBackupService
    {
        Task<string> CreateBackupAsync();
        Task<DateTime?> GetLastBackupDateAsync();
        Task RestoreFromBackupAsync(string backupPath);
    }

    public class BackupService : IBackupService
    {
        private readonly FinanceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BackupService> _logger;
        private readonly string _backupDirectory;

        public BackupService(
            FinanceDbContext context,
            IConfiguration configuration,
            ILogger<BackupService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _backupDirectory = _configuration.GetValue<string>("BackupDirectory") ?? 
                             Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            Directory.CreateDirectory(_backupDirectory);
        }

        public async Task<string> CreateBackupAsync()
        {
            try
            {
                var transactions = await _context.Transactions.ToListAsync();
                var backupPath = Path.Combine(_backupDirectory, $"backup_{DateTime.Now:yyyyMMddHHmmss}.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(transactions, options);
                await File.WriteAllTextAsync(backupPath, json);
                return backupPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create backup");
                throw;
            }
        }

        public async Task<DateTime?> GetLastBackupDateAsync()
        {
            try
            {
                var files = Directory.GetFiles(_backupDirectory, "backup_*.json");
                if (files.Length == 0)
                    return null;

                var lastFile = files.OrderByDescending(f => f).First();
                return File.GetLastWriteTime(lastFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last backup date");
                throw;
            }
        }

        public async Task RestoreFromBackupAsync(string backupPath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(backupPath);
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(json);
                if (transactions == null)
                    throw new InvalidOperationException("Invalid backup file");

                // Clear existing transactions
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Transactions");
                
                // Reset the context to clear any tracked entities
                _context.ChangeTracker.Clear();

                // Add new transactions with new IDs
                foreach (var transaction in transactions)
                {
                    transaction.Id = 0; // Reset the ID to let the database generate a new one
                    transaction.CreatedAt = DateTime.Now;
                    transaction.UpdatedAt = DateTime.Now;
                }

                await _context.Transactions.AddRangeAsync(transactions);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore from backup");
                throw;
            }
        }
    }
} 