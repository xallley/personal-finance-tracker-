using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using PersonalFinanceTracker.Data;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace PersonalFinanceTracker
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<App> _logger;

        public App()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<IConfiguration>(_configuration);

            
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(_configuration.GetSection("Logging"));
                builder.AddDebug();
                builder.AddConsole();
                builder.AddEventLog();
            });

            
            services.AddDbContext<FinanceDbContext>(options =>
                options.UseSqlite(_configuration.GetConnectionString("DefaultConnection")));

            
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IExportService, ExportService>();
            services.AddScoped<IErrorHandler, ErrorHandler>();
            services.AddScoped<IValidationService, ValidationService>();

           
            services.AddTransient<MainViewModel>();
            services.AddTransient<TransactionDialogViewModel>();

            
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

               
                Directory.CreateDirectory("Backups");
                Directory.CreateDirectory("Exports");

                _logger.LogInformation("Application starting...");

               
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
                    _logger.LogInformation("Creating database if it doesn't exist...");
                    dbContext.Database.EnsureCreated();
                    _logger.LogInformation("Applying any pending migrations...");
                    dbContext.Database.Migrate();
                    _logger.LogInformation("Database setup completed.");
                }

                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application startup");
                MessageBox.Show($"Error during application startup: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation("Application shutting down...");
            base.OnExit(e);
        }
    }
} 