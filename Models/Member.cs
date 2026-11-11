namespace LibraryApp.Models;

public class Member
{
    public string MemberId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime JoinedOn { get; set; } = DateTime.Now;

    public override string ToString() =>
        $"{MemberId} | {Name} <{Email}> joined {JoinedOn:yyyy-MM-dd}";
}

