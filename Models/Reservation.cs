namespace LibraryApp.Models;

public class Reservation
{
    public string ReservationId { get; set; } = Guid.NewGuid().ToString("N");
    public string Isbn { get; set; } = "";
    public string MemberId { get; set; } = "";
    public DateTime ReservedOn { get; set; } = DateTime.Now;
    public bool Fulfilled { get; set; }

    public override string ToString() =>
        $"[{ReservationId[..8]}] {Isbn} → {MemberId} | reserved {ReservedOn:yyyy-MM-dd} | {(Fulfilled ? "fulfilled" : "waiting")}";
}
