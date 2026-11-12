using LibraryApp.Models;
using LibraryApp.Persistence;
using LibraryApp.Services;

const string DataFolder = "data";

var books = new JsonRepository<Book>(Path.Combine(DataFolder, "books.json"), b => b.Isbn);
var members = new JsonRepository<Member>(Path.Combine(DataFolder, "members.json"), m => m.MemberId);
var loans = new JsonRepository<Loan>(Path.Combine(DataFolder, "loans.json"), l => l.LoanId);

var service = new LibraryService(books, members, loans);

service.BookBorrowed += (_, loan) => System.Console.WriteLine($"[event] BORROWED: {loan}");
service.BookReturned += (_, loan) => System.Console.WriteLine($"[event] RETURNED: {loan}");

// Seed only if empty — so we can tell "first run" from "second run"
if (!service.GetAllBooks().Any())
{
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
    System.Console.WriteLine("Seeded 2 books.");
}
else
{
    System.Console.WriteLine($"Loaded {service.GetAllBooks().Count()} book(s) from disk.");
}

if (!service.GetAllMembers().Any())
{
    service.AddMember(new Member { MemberId = "M001", Name = "Alice", Email = "alice@example.com" });
    System.Console.WriteLine("Seeded 1 member.");
}
else
{
    System.Console.WriteLine($"Loaded {service.GetAllMembers().Count()} member(s) from disk.");
}

System.Console.WriteLine();
foreach (var b in service.GetAllBooks())
    System.Console.WriteLine(b);

