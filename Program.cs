using LibraryApp.Models;
using LibraryApp.Notifiers;
using LibraryApp.Persistence;
using LibraryApp.Services;
using LibraryApp.UI;

const string DataFolder = "data";

// 1. Initialize all 4 repositories
var books = new JsonRepository<Book>(Path.Combine(DataFolder, "books.json"), b => b.Isbn);
var members = new JsonRepository<Member>(Path.Combine(DataFolder, "members.json"), m => m.MemberId);
var loans = new JsonRepository<Loan>(Path.Combine(DataFolder, "loans.json"), l => l.LoanId);
var reservations = new JsonRepository<Reservation>(Path.Combine(DataFolder, "reservations.json"), r => r.ReservationId);

// 2. Pass all 4 into the LibraryService constructor
var service = new LibraryService(books, members, loans, reservations);

// 3. Wire up notifications (Observer pattern)
var notifier = new ConsoleNotifier();
service.BookBorrowed += (_, loan) => notifier.Notify($"Book borrowed: {loan.Isbn} by {loan.MemberId}");
service.BookReturned += (_, loan) => notifier.Notify($"Book returned: {loan.Isbn} by {loan.MemberId}");
service.OverdueLoanDetected += (_, loan) => notifier.Notify($"OVERDUE loan {loan.LoanId[..8]} ({loan.Isbn})");

// 4. Start the UI
var ui = new ConsoleMenu(service);
ui.Run();
