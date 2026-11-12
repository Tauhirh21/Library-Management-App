using LibraryApp.Interfaces;

namespace LibraryApp.Notifiers;

public class ConsoleNotifier : INotifier
{
    public void Notify(string message) =>
        System.Console.WriteLine($"[notify] {message}");
}
