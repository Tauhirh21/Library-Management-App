namespace LibraryApp.Models;

public enum LoanStatus { Active, Returned }

public class Loan
{
    public string LoanId { get; set; } = Guid.NewGuid().ToString("N");
    public string Isbn { get; set; } = "";
    public string MemberId { get; set; } = "";
    public DateTime BorrowedOn { get; set; }
    public DateTime DueOn { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    public bool IsOverdue => Status == LoanStatus.Active && DateTime.Now > DueOn;

    public override string ToString() =>
        $"[{LoanId[..8]}] {Isbn} → {MemberId} | due {DueOn:yyyy-MM-dd} | {Status}" +
        (ReturnedOn.HasValue ? $" | returned {ReturnedOn:yyyy-MM-dd}" : "");
}

