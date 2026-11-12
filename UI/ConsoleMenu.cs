using LibraryApp.Exceptions;
using LibraryApp.Models;
using LibraryApp.Persistence;
using LibraryApp.Services;

namespace LibraryApp.UI;

public class ConsoleMenu
{
    private readonly LibraryService _service;

    public ConsoleMenu(LibraryService service) => _service = service;

    public void Run()
    {
        while (true)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=== Library Management System ===");
            System.Console.WriteLine(" 1. Add book");
            System.Console.WriteLine(" 2. List books");
            System.Console.WriteLine(" 3. Search books");
            System.Console.WriteLine(" 4. Add member");
            System.Console.WriteLine(" 5. List members");
            System.Console.WriteLine(" 6. Borrow book");
            System.Console.WriteLine(" 7. Return book");
            System.Console.WriteLine(" 8. Active loans");
            System.Console.WriteLine(" 9. Overdue loans");
            System.Console.WriteLine("10. Popular books");
            System.Console.WriteLine("11. Reserve a book");
            System.Console.WriteLine("12. View reservations for a book");
            System.Console.WriteLine(" 0. Exit");
            System.Console.Write("Choice: ");

            var choice = System.Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": AddBook(); break;
                    case "2": ListBooks(); break;
                    case "3": SearchBooks(); break;
                    case "4": AddMember(); break;
                    case "5": ListMembers(); break;
                    case "6": BorrowBook(); break;
                    case "7": ReturnBook(); break;
                    case "8": ListActiveLoans(); break;
                    case "9": ListOverdueLoans(); break;
                    case "10": ListPopularBooks(); break;
                    case "11": ReserveBook(); break;
                    case "12": ListReservations(); break;
                    case "0": return;
                    default: System.Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (LibraryException ex) { System.Console.WriteLine($"⚠ {ex.Message}"); }
            catch (Exception ex) { System.Console.WriteLine($"Unexpected error: {ex.Message}"); }
        }
    }

    // ---------------- Menu actions ----------------

    private void AddBook()
    {
        var b = new Book
        {
            Isbn = Prompt("ISBN"),
            Title = Prompt("Title"),
            Author = Prompt("Author"),
            Genre = Prompt("Genre"),
        };
        b.TotalCopies = int.Parse(Prompt("Total copies"));
        b.AvailableCopies = b.TotalCopies;
        _service.AddBook(b);
        System.Console.WriteLine("✔ Book added.");
    }

    private void ListBooks()
    {
        var books = _service.GetAllBooks().ToList();
        if (books.Count == 0) { System.Console.WriteLine("(no books)"); return; }
        foreach (var b in books) System.Console.WriteLine(b);
    }

    private void SearchBooks()
    {
        var term = Prompt("Search term");
        var results = _service.Search(term).ToList();
        if (results.Count == 0) { System.Console.WriteLine("(no matches)"); return; }
        foreach (var b in results) System.Console.WriteLine(b);
    }

    private void AddMember()
    {
        _service.AddMember(new Member
        {
            MemberId = Prompt("Member ID"),
            Name = Prompt("Name"),
            Email = Prompt("Email"),
        });
        System.Console.WriteLine("✔ Member added.");
    }

    private void ListMembers()
    {
        var members = _service.GetAllMembers().ToList();
        if (members.Count == 0) { System.Console.WriteLine("(no members)"); return; }
        foreach (var m in members) System.Console.WriteLine(m);
    }

    private void BorrowBook()
    {
        var isbn = Prompt("ISBN");
        var memberId = Prompt("Member ID");
        _service.TryBorrow(isbn, memberId, out var loan);
        System.Console.WriteLine($"✔ Borrowed. Loan {loan!.LoanId[..8]} due {loan.DueOn:yyyy-MM-dd}");
    }

    private void ReturnBook()
    {
        // Show active loans first so the user can copy a loan ID
        var active = _service.GetActiveLoans().ToList();
        if (active.Count == 0) { System.Console.WriteLine("(no active loans)"); return; }
        System.Console.WriteLine("Active loans:");
        foreach (var l in active) System.Console.WriteLine(l);

        var loanId = Prompt("Loan ID (or first 8 chars)");
        var match = active.FirstOrDefault(l =>
            l.LoanId == loanId ||
            l.LoanId.StartsWith(loanId, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            System.Console.WriteLine("⚠ No matching active loan.");
            return;
        }

        // UPDATED: 3 arguments now
        _service.TryReturn(match.LoanId, out var fine, out var reservedFor);

        var msg = fine > 0 ? $"✔ Returned. Fine: R{fine:0.00}" : "✔ Returned. No fine.";
        System.Console.WriteLine(msg);

        if (reservedFor is not null)
            System.Console.WriteLine($">> Copy auto-borrowed to next reservation: {reservedFor}");
    }

    private void ListActiveLoans()
    {
        var loans = _service.GetActiveLoans().ToList();
        if (loans.Count == 0) { System.Console.WriteLine("(no active loans)"); return; }
        foreach (var l in loans) System.Console.WriteLine(l);
    }

    private void ListOverdueLoans()
    {
        var loans = _service.ActiveOverdueLoans().ToList();
        if (loans.Count == 0) { System.Console.WriteLine("(no overdue loans)"); return; }
        foreach (var l in loans) System.Console.WriteLine(l);
    }

    private void ListPopularBooks()
    {
        var books = _service.PopularBooks().ToList();
        if (books.Count == 0) { System.Console.WriteLine("(no borrowing history)"); return; }
        var i = 1;
        foreach (var b in books) System.Console.WriteLine($"{i++}. {b.Title} by {b.Author}");
    }

    // ---------------- New Reservation Actions ----------------

    private void ReserveBook()
    {
        var isbn = Prompt("ISBN");
        var memberId = Prompt("Member ID");

        try
        {
            _service.Reserve(isbn, memberId);
            var pos = _service.ReservationQueueLength(isbn);
            System.Console.WriteLine($"✔ Reserved. You are position {pos} in the queue.");
        }
        catch (LibraryException ex)
        {
            System.Console.WriteLine($"⚠ {ex.Message}");
        }
    }

    private void ListReservations()
    {
        var isbn = Prompt("ISBN");
        var reservations = _service.GetReservationsFor(isbn).ToList();

        if (reservations.Count == 0)
        {
            System.Console.WriteLine("(no active reservations)");
            return;
        }

        var i = 1;
        foreach (var r in reservations)
            System.Console.WriteLine($"{i++}. {r.MemberId} (reserved {r.ReservedOn:yyyy-MM-dd})");
    }

    // ---------------- Helpers ----------------

    private static string Prompt(string label)
    {
        System.Console.Write($"{label}: ");
        return System.Console.ReadLine() ?? "";
    }
}
