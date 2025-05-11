# Personal Finance Tracker

A modern WPF application for managing personal finances, built with .NET. This application helps you track your income and expenses, categorize transactions, and generate reports.

## Features

### Transaction Management
- Add, edit, and delete transactions
- Categorize transactions (Income/Expense)
- Add descriptions and amounts
- Date-based transaction tracking
- Search and filter transactions

### Financial Overview
- Real-time balance calculation
- Total income tracking
- Total expenses tracking
- Visual summary of financial status

### Data Management
- Export transactions to CSV
- Export transactions to Excel
- Generate monthly reports
- Create and restore backups
- Data validation and error handling

### User Interface
- Modern and clean design
- Responsive layout
- Color-coded transactions (green for income, red for expenses)
- Loading indicators
- Error message display

## Technical Details

### Architecture
- MVVM (Model-View-ViewModel) pattern
- Entity Framework Core for data access
- Dependency Injection
- Async/await for responsive UI

### Key Components
- **Models**: Transaction data structure
- **ViewModels**: Business logic and data binding
- **Views**: User interface components
- **Services**: Business logic and data operations
- **Data**: Database context and configuration

### Dependencies
- .NET 6.0 or later
- Entity Framework Core
- CsvHelper
- ClosedXML
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

## Getting Started

### Prerequisites
- .NET 6.0 SDK or later
- Visual Studio 2022 or later (recommended)

### Installation
1. Clone the repository
```bash
git clone https://github.com/yourusername/personal-finance-tracker.git
```

2. Navigate to the project directory
```bash
cd personal-finance-tracker
```

3. Build the solution
```bash
dotnet build
```

4. Run the application
```bash
dotnet run
```

## Usage

### Adding a Transaction
1. Click the "Add Transaction" button
2. Fill in the transaction details:
   - Amount
   - Description
   - Category
   - Type (Income/Expense)
   - Date
3. Click "Save" to add the transaction

### Managing Transactions
- Use the search box to filter transactions
- Select a transaction to edit or delete
- Use the date range picker to view transactions for a specific period

### Exporting Data
- Click "Export to CSV" to export transactions in CSV format
- Click "Export to Excel" to export transactions in Excel format
- Click "Generate Monthly Report" to create a monthly summary

### Backup and Restore
- Click "Backup" to create a backup of your data
- Click "Restore" to restore from a previous backup

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

- Thanks to all contributors who have helped shape this project
- Built with modern .NET technologies
- Inspired by the need for better personal finance management 