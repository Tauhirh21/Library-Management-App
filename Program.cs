using LibraryApp.Models;
using LibraryApp.Notifiers;
using LibraryApp.Persistence;
using LibraryApp.Services;
using LibraryApp.UI;

const string DataFolder = "data";

var books = new JsonRepository<Book>(Path.Combine(DataFolder, "books.json"), b => b.Isbn);
var members = new JsonRepository<Member>(Path.Combine(DataFolder, "members.json"), m => m.MemberId);
var loans = new JsonRepository<Loan>(Path.Combine(DataFolder, "loans.json"), l => l.LoanId);

var service = new LibraryService(books, members, loans);

// Wire up notifications
var notifier = new ConsoleNotifier();
service.BookBorrowed += (_, loan) => notifier.Notify($"Book borrowed: {loan.Isbn} by {loan.MemberId}");
service.BookReturned += (_, loan) => notifier.Notify($"Book returned: {loan.Isbn} by {loan.MemberId}");
service.OverdueLoanDetected += (_, loan) => notifier.Notify($"OVERDUE loan {loan.LoanId[..8]} ({loan.Isbn})");

var ui = new ConsoleMenu(service);
ui.Run();
