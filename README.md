# Library Management System

A console-based library management system built in **C# / .NET 8** for the Programming 2B module.

## Features

- **Books** — add, list, search (title / author / ISBN / genre)
- **Members** — register and list members
- **Borrowing** — borrow a book for 14 days; stock decrements automatically
- **Returns** — return a book; overdue fines calculated at R2.00/day
- **Persistence** — everything stored as JSON; survives app restarts
- **Reports** — active loans, overdue loans, most-borrowed books (LINQ)
- **Events** — `BookBorrowed`, `BookReturned`, `OverdueLoanDetected` (Observer pattern)
- **Reservation Queue** — FIFO waitlist when no copies are available. Auto-borrows the book for the next person in line upon return.
- **CSV Export** — export overdue loans and book inventory to `.csv` for Excel.

## Architecture

The project follows a layered design so that business logic is decoupled from I/O and UI:

```text
┌─────────────────────────────────────────────┐
│  ConsoleMenu  (UI layer)                    │
└───────────────────┬─────────────────────────┘
                    │ calls
┌───────────────────▼─────────────────────────┐
│  LibraryService  (business logic)           │
│  implements IBorrowable, raises events      │
└─────────┬─────────────────────┬─────────────┘
          │ uses                │ raises
┌─────────▼─────────┐  ┌────────▼─────────────┐
│ IRepository<T>    │  │ INotifier            │
│  └ JsonRepository │  │  └ ConsoleNotifier   │
└───────────────────┘  └──────────────────────┘
dotnet run --project LibraryApp
dotnet test
