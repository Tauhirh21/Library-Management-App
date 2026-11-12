namespace LibraryApp.Exceptions;

public class LibraryException : Exception
{
    public LibraryException(string message) : base(message) { }
}

public class BookNotFoundException : LibraryException
{
    public BookNotFoundException(string isbn)
        : base($"Book with ISBN '{isbn}' was not found.") { }
}

public class MemberNotFoundException : LibraryException
{
    public MemberNotFoundException(string memberId)
        : base($"Member with ID '{memberId}' was not found.") { }
}

public class BookNotAvailableException : LibraryException
{
    public BookNotAvailableException(string isbn)
        : base($"No copies of ISBN '{isbn}' are currently available.") { }
}
