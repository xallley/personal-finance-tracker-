using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using System.Globalization;
using PersonalFinanceTracker.Models;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PersonalFinanceTracker.Data;

namespace PersonalFinanceTracker.Services
{
    public interface IExportService
    {
        Task<string> ExportToCsvAsync(IEnumerable<Transaction> transactions, string fileName);
        Task<string> ExportToExcelAsync(IEnumerable<Transaction> transactions, string fileName);
        Task<string> GenerateMonthlyReportAsync(DateTime date);
    }

    public class ExportService : IExportService
    {
        private readonly FinanceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExportService> _logger;
        private readonly string _exportDirectory;

        public ExportService(
            FinanceDbContext context,
            IConfiguration configuration,
            ILogger<ExportService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _exportDirectory = _configuration.GetValue<string>("ExportDirectory") ?? 
                             Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exports");
            Directory.CreateDirectory(_exportDirectory);
        }

        public async Task<string> ExportToCsvAsync(IEnumerable<Transaction> transactions, string fileName)
        {
            try
            {
                var filePath = Path.Combine(_exportDirectory, fileName);
                await using var writer = new StreamWriter(filePath);
                await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                await csv.WriteRecordsAsync(transactions);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export to CSV");
                throw;
            }
        }

        public async Task<string> ExportToExcelAsync(IEnumerable<Transaction> transactions, string fileName)
        {
            try
            {
                var filePath = Path.Combine(_exportDirectory, fileName);
                
                // Ensure the file is not in use
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (IOException)
                    {
                        // Wait a bit and try again
                        await Task.Delay(1000);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Transactions");

                // Add headers
                worksheet.Cell(1, 1).Value = "Date";
                worksheet.Cell(1, 2).Value = "Type";
                worksheet.Cell(1, 3).Value = "Category";
                worksheet.Cell(1, 4).Value = "Description";
                worksheet.Cell(1, 5).Value = "Amount";

                // Add data
                int row = 2;
                foreach (var transaction in transactions)
                {
                    worksheet.Cell(row, 1).Value = transaction.Date;
                    worksheet.Cell(row, 2).Value = transaction.Type.ToString();
                    worksheet.Cell(row, 3).Value = transaction.Category;
                    worksheet.Cell(row, 4).Value = transaction.Description;
                    worksheet.Cell(row, 5).Value = transaction.Amount;
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                // Save the workbook
                workbook.SaveAs(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export to Excel");
                throw;
            }
        }

        public async Task<string> GenerateMonthlyReportAsync(DateTime date)
        {
            try
            {
                var startDate = new DateTime(date.Year, date.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var transactions = await _context.Transactions
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .ToListAsync();

                var fileName = $"monthly_report_{date:yyyyMM}.xlsx";
                return await ExportToExcelAsync(transactions, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate monthly report");
                throw;
            }
        }
    }
} 