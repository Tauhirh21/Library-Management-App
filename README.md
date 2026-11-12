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

## Architecture

The project follows a layered design so that business logic is decoupled from I/O and UI:
┌─────────────────────────────────────────────┐
│ ConsoleMenu (UI layer) │
└───────────────────┬─────────────────────────┘
│ calls
┌───────────────────▼─────────────────────────┐
│ LibraryService (business logic) │
│ implements IBorrowable, raises events │
└─────────┬─────────────────────┬─────────────┘
│ uses │ raises
┌─────────▼─────────┐ ┌────────▼─────────────┐
│ IRepository<T> │ │ INotifier │
│ └ JsonRepository │ │ └ ConsoleNotifier │
└───────────────────┘ └──────────────────────┘


| Layer | Folder | Responsibility |
|---|---|---|
| Models | `Models/` | `Book`, `Member`, `Loan`, `LoanStatus` |
| Interfaces | `Interfaces/` | `IRepository<T>`, `IBorrowable`, `INotifier` |
| Services | `Services/` | `LibraryService` — business rules, no I/O |
| Persistence | `Persistence/` | `JsonRepository<T>` — file-backed storage |
| Exceptions | `Exceptions/` | Domain-specific exception hierarchy |
| UI | `UI/` | `ConsoleMenu` — presentation only |
| Notifiers | `Notifiers/` | `ConsoleNotifier` — event subscriber |
| Tests | `tests/` | xUnit tests for core logic |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 **or** VS Code **or** any editor

### Run

```bash
dotnet run --project LibraryApp
dotnet test

EXAMPLE SESSION
=== Library Management System ===
 1. Add book
 2. List books
 ...
Choice: 1
ISBN: 978-0
Title: Clean Code
Author: Robert Martin
Genre: Programming
Total copies: 2
✔ Book added.

Choice: 6
ISBN: 978-0
Member ID: M001
[notify] Book borrowed: 978-0 by M001
✔ Borrowed. Loan a3b5f1c2 due 2026-11-26

Choice: 7
Active loans:
[a3b5f1c2] 978-0 → M001 | due 2026-11-26 | Active
Loan ID (or first 8 chars): a3b5f1c2
[notify] Book returned: 978-0 by M001
✔ Returned. No fine.


