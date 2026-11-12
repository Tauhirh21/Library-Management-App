using LibraryApp.Exceptions;
using LibraryApp.Interfaces;
using LibraryApp.Models;

namespace LibraryApp.Services;

public class LibraryService : IBorrowable
{
    private readonly List<Book> _books = new();
    private readonly List<Member> _members = new();
    private readonly List<Loan> _loans = new();

    public const int LoanPeriodDays = 14;
    public const decimal FinePerDay = 2.00m;

    public event EventHandler<Loan>? BookBorrowed;
    public event EventHandler<Loan>? BookReturned;
    public event EventHandler<Loan>? OverdueLoanDetected;

    // --- Books ---
    public void AddBook(Book book)
    {
        if (_books.Any(b => b.Isbn == book.Isbn))
            throw new LibraryException($"Book with ISBN '{book.Isbn}' already exists.");
        _books.Add(book);
    }

    public IEnumerable<Book> GetAllBooks() => _books.ToList();

    public Book GetBook(string isbn) =>
        _books.FirstOrDefault(b => b.Isbn == isbn)
        ?? throw new BookNotFoundException(isbn);

    // --- Members ---
    public void AddMember(Member member)
    {
        if (_members.Any(m => m.MemberId == member.MemberId))
            throw new LibraryException($"Member '{member.MemberId}' already exists.");
        _members.Add(member);
    }

    public IEnumerable<Member> GetAllMembers() => _members.ToList();

    public Member GetMember(string id) =>
        _members.FirstOrDefault(m => m.MemberId == id)
        ?? throw new MemberNotFoundException(id);

    // --- Borrow / Return ---
    public bool TryBorrow(string isbn, string memberId, out Loan? loan)
    {
        loan = null;
        var book = GetBook(isbn);
        GetMember(memberId); // validates existence

        if (book.AvailableCopies <= 0)
            throw new BookNotAvailableException(isbn);

        book.AvailableCopies--;
        loan = new Loan
        {
            Isbn = isbn,
            MemberId = memberId,
            BorrowedOn = DateTime.Now,
            DueOn = DateTime.Now.AddDays(LoanPeriodDays)
        };
        _loans.Add(loan);
        BookBorrowed?.Invoke(this, loan);
        return true;
    }

    public bool TryReturn(string loanId, out decimal fine)
    {
        fine = 0m;
        var loan = _loans.FirstOrDefault(l => l.LoanId == loanId)
            ?? throw new LibraryException($"Loan '{loanId}' not found.");

        if (loan.Status == LoanStatus.Returned)
            throw new LibraryException("Loan already returned.");

        loan.ReturnedOn = DateTime.Now;
        loan.Status = LoanStatus.Returned;

        if (loan.ReturnedOn > loan.DueOn)
        {
            var days = (loan.ReturnedOn.Value - loan.DueOn).Days;
            fine = days * FinePerDay;
        }

        var book = GetBook(loan.Isbn);
        book.AvailableCopies++;
        BookReturned?.Invoke(this, loan);
        return true;
    }

    // --- Queries ---
    public IEnumerable<Loan> GetAllLoans() => _loans.ToList();
    public IEnumerable<Loan> GetActiveLoans() => _loans.Where(l => l.Status == LoanStatus.Active);

    public IEnumerable<Book> Search(string term) =>
        _books.Where(b =>
            b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Isbn.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Genre.Contains(term, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Loan> ActiveOverdueLoans()
    {
        var overdue = _loans.Where(l => l.IsOverdue).ToList();
        foreach (var loan in overdue)
            OverdueLoanDetected?.Invoke(this, loan);
        return overdue;
    }

    public IEnumerable<Book> PopularBooks(int top = 5) =>
        _loans.GroupBy(l => l.Isbn)
              .OrderByDescending(g => g.Count())
              .Take(top)
              .Select(g => _books.First(b => b.Isbn == g.Key));
}
