using System.Globalization;
using System.Text;
using LibraryApp.Models;

namespace LibraryApp.Persistence;

public static class CsvExporter
{
    // Quotes fields containing commas, quotes, or newlines (standard CSV escaping)
    private static string Field(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    public static void ExportOverdueLoans(string path, IEnumerable<Loan> loans)
    {
        var sb = new StringBuilder();
        sb.AppendLine("LoanId,Isbn,MemberId,BorrowedOn,DueOn,DaysOverdue");
        foreach (var loan in loans)
        {
            var days = Math.Max(0, (DateTime.Now - loan.DueOn).Days);
            sb.AppendLine(string.Join(",",
                Field(loan.LoanId),
                Field(loan.Isbn),
                Field(loan.MemberId),
                loan.BorrowedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                loan.DueOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                days.ToString(CultureInfo.InvariantCulture)));
        }
        WriteFile(path, sb.ToString());
    }

    public static void ExportBooks(string path, IEnumerable<Book> books)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Isbn,Title,Author,Genre,TotalCopies,AvailableCopies");
        foreach (var b in books)
        {
            sb.AppendLine(string.Join(",",
                Field(b.Isbn),
                Field(b.Title),
                Field(b.Author),
                Field(b.Genre),
                b.TotalCopies.ToString(CultureInfo.InvariantCulture),
                b.AvailableCopies.ToString(CultureInfo.InvariantCulture)));
        }
        WriteFile(path, sb.ToString());
    }

    private static void WriteFile(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        // UTF-8 with BOM so Excel opens special characters correctly
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }
}
