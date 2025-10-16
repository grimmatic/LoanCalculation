# Loan Calculation System

A modern loan calculation and application management system built with ASP.NET Core 9.0 and Angular 19.

## Overview

This project provides end‑to‑end functionality for calculating loans, managing customers, and submitting loan applications across bank products. It includes authentication, bank membership management, and detailed amortization schedules with validations and logging.

## Key Features

- Loan calculator with detailed amortization schedule
- Two calculation modes: bank product–based and manual interest entry
- Loan application submission and tracking
- Session-based authentication (signup/login/logout, current user info)
- Bank membership management (join/leave and view memberships)
- Input validation for amounts and terms
- Structured logging and audit trail

## Tech Stack

### Backend

- ASP.NET Core 9.0
- Entity Framework Core (PostgreSQL via Npgsql)
- Serilog for logging
- Swagger/OpenAPI for API exploration

### Frontend

- Angular 19 (standalone components)
- SCSS styling
- Angular HttpClient

## Project Structure

```
LoanCalculation/
├── Business/
│   ├── Interfaces/
│   └── Services/
├── Controllers/
├── Data/
├── Models/
│   └── Entities/
├── Frontend/
│   └── src/
│       └── app/
└── Program.cs
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+ and npm
- PostgreSQL 12+

### Backend Setup

1. Clone the repository and enter the project directory:

```bash
git clone https://github.com/grimmatic/LoanCalculation.git
cd LoanCalculation
```

2. Configure the database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=loan_db;Username=postgres;Password=yourpassword"
  }
}
```

3. Apply database migrations:

```bash
dotnet ef database update
```

4. Run the backend:

```bash
dotnet run
```

By default the API runs at `http://localhost:5188`.

### Frontend Setup

1. Move to the frontend folder:

```bash
cd Frontend
```

2. Install dependencies and start the dev server:

```bash
npm install
npm start
```

The app runs at `http://localhost:4200`.

## Configuration

Key settings are in `appsettings.json` and `appsettings.Development.json`:

- Connection string under `ConnectionStrings:Postgres`
- Logging and allowed hosts configuration

## API Surface

### Authentication

- `POST /api/auth/signup` — Register a new user
- `POST /api/auth/login` — Login
- `POST /api/auth/logout` — Logout
- `GET /api/auth/me` — Get current session user

### Banks & Products

- `GET /api/bankalar` — List active banks
- `GET /api/urunler` — List active products
- `GET /api/banka-urunleri` — List bank products
- `GET /api/banka-urunleri/banka/{bankaId}` — Products for a specific bank
- `GET /api/banka-urunleri/{id}` — Bank product by id

### Bank Membership

- `GET /api/banka-uyelik/available-banks` — Banks the user can join
- `GET /api/banka-uyelik/my-banks` — User's memberships
- `POST /api/banka-uyelik/join/{bankaId}` — Join a bank
- `DELETE /api/banka-uyelik/leave/{bankaId}` — Leave a bank

### Loan Operations

- `POST /api/kredi-hesapla` — Calculate loan and payment plan
- `POST /api/kredi-basvuru` — Submit loan application
- `GET /api/kredi-basvuru/my-applications` — List my applications

## Usage

### Product-Based Calculation

1. Select a bank and product (predefined interest and limits)
2. Enter loan amount and term (months) within limits
3. Click Calculate to get the monthly payment and full schedule

### Manual Calculation

1. Choose manual interest mode
2. Enter principal, term (months), and monthly interest rate
3. Calculate to get the schedule and totals

### Applying for a Loan

1. Sign up or log in
2. Join the relevant banks
3. Open the application page and complete your information
4. Select bank and product, enter amount and term
5. Submit and track the status under your history

## Business Logic

### Amortization Formula

```
Monthly Payment = P × ( r / (1 − (1 + r)^(−n) ) )

Where:
P = Principal (loan amount)
r = Monthly interest rate (decimal)
n = Number of payments (months)
```

### Validation

- Amount and term must respect product limits
- Applications require membership of the selected bank
- Duplicate applications to the same product are prevented
- Email addresses must be unique; password length ≥ 6

## Database

Primary tables include: banks, products, bank_products, customers, customer_banks, calculations, payment_plans, logs.

## Development

### Run Tests

```bash
dotnet test
```

### Migrations

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Security Notes

This is a demonstration project. For production usage consider:

- Strong password hashing (BCrypt/Argon2)
- JWT-based authentication (instead of sessions)
- Rate limiting and request validation
- HTTPS-only and explicit CORS policies
- Robust input sanitization and error handling

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your branch
5. Open a Pull Request
