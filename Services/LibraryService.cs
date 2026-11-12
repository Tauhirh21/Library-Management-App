using LibraryApp.Exceptions;
using LibraryApp.Interfaces;
using LibraryApp.Models;

namespace LibraryApp.Services;

public class LibraryService : IBorrowable
{
    private readonly IRepository<Book> _books;
    private readonly IRepository<Member> _members;
    private readonly IRepository<Loan> _loans;
    private readonly IRepository<Reservation> _reservations;


    public const int LoanPeriodDays = 14;
    public const decimal FinePerDay = 2.00m;

    public event EventHandler<Loan>? BookBorrowed;
    public event EventHandler<Loan>? BookReturned;
    public event EventHandler<Loan>? OverdueLoanDetected;

    public LibraryService(IRepository<Book> books,
                          IRepository<Member> members,
                          IRepository<Loan> loans,
                          IRepository<Reservation> reservations)
    {
        _books = books;
        _members = members;
        _loans = loans;
        _reservations = reservations;
    }

    // --- Books ---
    public void AddBook(Book book)
    {
        if (_books.GetById(book.Isbn) is not null)
            throw new LibraryException($"Book with ISBN '{book.Isbn}' already exists.");
        _books.Add(book);
    }

    public IEnumerable<Book> GetAllBooks() => _books.GetAll();

    public Book GetBook(string isbn) =>
        _books.GetById(isbn) ?? throw new BookNotFoundException(isbn);

    // --- Members ---
    public void AddMember(Member member)
    {
        if (_members.GetById(member.MemberId) is not null)
            throw new LibraryException($"Member '{member.MemberId}' already exists.");
        _members.Add(member);
    }

    public IEnumerable<Member> GetAllMembers() => _members.GetAll();

    public Member GetMember(string id) =>
        _members.GetById(id) ?? throw new MemberNotFoundException(id);

    // --- Borrow / Return ---
    public bool TryBorrow(string isbn, string memberId, out Loan? loan)
    {
        loan = null;
        var book = GetBook(isbn);
        GetMember(memberId); // validates existence

        if (book.AvailableCopies <= 0)
            throw new BookNotAvailableException(isbn);

        book.AvailableCopies--;
        _books.Update(book);

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

    public bool TryReturn(string loanId, out decimal fine, out string? reservedFor)
    {
        fine = 0m;
        reservedFor = null;
        var loan = _loans.GetById(loanId)
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

        // NEW: If someone is waiting in the reservation queue, give them the copy
        var next = FulfilNextReservation(loan.Isbn);
        if (next is not null)
        {
            book.AvailableCopies--;
            var reservedLoan = new Loan
            {
                Isbn = loan.Isbn,
                MemberId = next.MemberId,
                BorrowedOn = DateTime.Now,
                DueOn = DateTime.Now.AddDays(LoanPeriodDays)
            };
            _loans.Add(reservedLoan);
            reservedFor = next.MemberId;
            BookBorrowed?.Invoke(this, reservedLoan);
        }

        _books.Update(book);
        _loans.Update(loan);

        BookReturned?.Invoke(this, loan);
        return true;
    }

    // --- Queries ---
    public IEnumerable<Loan> GetAllLoans() => _loans.GetAll();

    public IEnumerable<Loan> GetActiveLoans() =>
        _loans.GetAll().Where(l => l.Status == LoanStatus.Active);

    public IEnumerable<Book> Search(string term) =>
        _books.GetAll().Where(b =>
            b.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Isbn.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            b.Genre.Contains(term, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Loan> ActiveOverdueLoans()
    {
        var overdue = _loans.GetAll().Where(l => l.IsOverdue).ToList();
        foreach (var loan in overdue)
            OverdueLoanDetected?.Invoke(this, loan);
        return overdue;
    }

    public IEnumerable<Book> PopularBooks(int top = 5) =>
        _loans.GetAll()
              .GroupBy(l => l.Isbn)
              .OrderByDescending(g => g.Count())
              .Take(top)
              .Select(g => _books.GetById(g.Key)!);
    // --- Reservations ---

    public Reservation Reserve(string isbn, string memberId)
    {
        var book = GetBook(isbn);
        GetMember(memberId);

        if (book.AvailableCopies > 0)
            throw new LibraryException(
                $"ISBN '{isbn}' has copies available — borrow instead of reserving.");

        // Prevent duplicate active reservation for the same member/book pair
        var existing = _reservations.GetAll()
            .FirstOrDefault(r => r.Isbn == isbn && r.MemberId == memberId && !r.Fulfilled);
        if (existing is not null)
            return existing;

        var reservation = new Reservation
        {
            Isbn = isbn,
            MemberId = memberId,
            ReservedOn = DateTime.Now
        };
        _reservations.Add(reservation);
        return reservation;
    }

    public IEnumerable<Reservation> GetReservationsFor(string isbn) =>
        _reservations.GetAll()
            .Where(r => r.Isbn == isbn && !r.Fulfilled)
            .OrderBy(r => r.ReservedOn);

    public int ReservationQueueLength(string isbn) =>
        _reservations.GetAll().Count(r => r.Isbn == isbn && !r.Fulfilled);

    // Returns the next reservation in line (FIFO) and marks it fulfilled.
    // Called from TryReturn — see Step 10.4.
    private Reservation? FulfilNextReservation(string isbn)
    {
        var next = _reservations.GetAll()
            .Where(r => r.Isbn == isbn && !r.Fulfilled)
            .OrderBy(r => r.ReservedOn)   // FIFO — real queue semantics
            .FirstOrDefault();

        if (next is null) return null;

        next.Fulfilled = true;
        _reservations.Update(next);
        return next;
    }
}

