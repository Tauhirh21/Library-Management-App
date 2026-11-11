namespace LibraryApp.Models;

public class Book
{
    public string Isbn { get; set; } = "";
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }

    public override string ToString() =>
        $"{Isbn} | {Title} by {Author} ({Genre}) — {AvailableCopies}/{TotalCopies} available";
}
