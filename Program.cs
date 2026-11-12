using LibraryApp.Models;
using LibraryApp.Services;

var service = new LibraryService();

// Wire up events so we can see them fire
service.BookBorrowed += (_, loan) => System.Console.WriteLine($"[event] BORROWED: {loan}");
service.BookReturned += (_, loan) => System.Console.WriteLine($"[event] RETURNED: {loan}");
service.OverdueLoanDetected += (_, loan) => System.Console.WriteLine($"[event] OVERDUE: {loan}");

// Seed data
service.AddBook(new Book
{
    Isbn = "978-0",
    Title = "Clean Code",
    Author = "Robert Martin",
    Genre = "Programming",
    TotalCopies = 2,
    AvailableCopies = 2
});
service.AddBook(new Book
{
    Isbn = "978-1",
    Title = "The Pragmatic Programmer",
    Author = "Hunt & Thomas",
    Genre = "Programming",
    TotalCopies = 1,
    AvailableCopies = 1
});
service.AddMember(new Member { MemberId = "M001", Name = "Alice", Email = "alice@example.com" });

// Borrow twice
service.TryBorrow("978-0", "M001", out var loan1);
service.TryBorrow("978-1", "M001", out var loan2);

// List active loans
System.Console.WriteLine("\n--- Active loans ---");
foreach (var l in service.GetActiveLoans())
    System.Console.WriteLine(l);

// Search
System.Console.WriteLine("\n--- Search 'clean' ---");
foreach (var b in service.Search("clean"))
    System.Console.WriteLine(b);

// Return one loan
System.Console.WriteLine("\n--- Returning loan 1 ---");
service.TryReturn(loan1!.LoanId, out var fine);
System.Console.WriteLine($"Fine: R{fine:0.00}");

// Popular books
System.Console.WriteLine("\n--- Popular books ---");
foreach (var b in service.PopularBooks())
    System.Console.WriteLine(b.Title);
