using LibraryApp.Models;

namespace LibraryApp.Interfaces;

public interface IBorrowable
{
    bool TryBorrow(string isbn, string memberId, out Loan? loan);
    bool TryReturn(string loanId, out decimal fine);
}
