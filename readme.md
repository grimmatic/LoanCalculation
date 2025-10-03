# Loan Calculation System

A comprehensive loan calculation and application management system built with ASP.NET Core 9.0 and Angular 19.

## Overview

This system provides a complete solution for loan calculations, customer management, and bank product applications. It features user authentication, bank membership management, and detailed loan payment schedules with interest calculations.

## Features

### Core Functionality

- **Loan Calculator**: Calculate monthly payments with detailed amortization schedules
- **Two Calculation Modes**:
  - Product-based: Select from bank products with predefined rates
  - Manual: Enter custom interest rates for calculations
- **Loan Applications**: Submit applications with validation and tracking
- **User Authentication**: Secure signup/login with session management
- **Bank Membership**: Join/leave banks to access their loan products
- **Application History**: Track past loan applications

### Technical Features

- Real-time validation of loan amounts and terms
- Interest calculation using standard amortization formulas
- Comprehensive logging system
- PostgreSQL database with Entity Framework Core
- RESTful API architecture
- Session-based authentication

## Technology Stack

### Backend

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL with Npgsql
- **ORM**: Entity Framework Core
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI

### Frontend

- **Framework**: Angular 19 (Standalone Components)
- **Styling**: Custom SCSS
- **HTTP Client**: Angular HttpClient
- **Forms**: Template-driven forms with ngModel

## Database Schema

### Main Tables

- **bankalar** (Banks): Bank information
- **urunler** (Products): Loan product types
- **banka_urunleri** (Bank Products): Bank-specific product configurations
- **musteriler** (Customers): User accounts
- **musteri_bankalar** (Customer Banks): Bank membership relationships
- **hesaplamalar** (Calculations): Loan calculation records
- **odeme_plani** (Payment Plans): Amortization schedules
- **loglar** (Logs): System activity logs

## Installation

### Prerequisites

- .NET 9.0 SDK
- Node.js 18+ and npm
- PostgreSQL 12+

### Backend Setup

1. Clone the repository

```bash
git clone <repository-url>
cd LoanCalculation
```

2. Configure database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Database=loan_db;Username=postgres;Password=yourpassword"
  }
}
```

3. Run database migrations:

```bash
dotnet ef database update
```

4. Start the backend server:

```bash
dotnet run
```

The API will be available at `http://localhost:5188`

### Frontend Setup

1. Navigate to the frontend directory:

```bash
cd Frontend
```

2. Install dependencies:

```bash
npm install
```

3. Start the development server:

```bash
npm start
```

The application will be available at `http://localhost:4200`

## API Endpoints

### Authentication

- `POST /api/auth/signup` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Get current user info

### Banks & Products

- `GET /api/bankalar` - List all active banks
- `GET /api/urunler` - List all active products
- `GET /api/banka-urunleri` - List all bank products
- `GET /api/banka-urunleri/banka/{bankaId}` - Get products for specific bank
- `GET /api/banka-urunleri/{id}` - Get specific bank product

### Bank Membership

- `GET /api/banka-uyelik/available-banks` - Get banks user can join
- `GET /api/banka-uyelik/my-banks` - Get user's bank memberships
- `POST /api/banka-uyelik/join/{bankaId}` - Join a bank
- `DELETE /api/banka-uyelik/leave/{bankaId}` - Leave a bank

### Loan Operations

- `POST /api/kredi-hesapla` - Calculate loan payment schedule
- `POST /api/kredi-basvuru` - Submit loan application
- `GET /api/kredi-basvuru/my-applications` - Get user's past applications

## Usage

### Calculating a Loan

1. **Product-Based Calculation**:

   - Select a bank from the dropdown
   - Choose a loan product
   - Enter loan amount (within min/max limits)
   - Enter loan term in months
   - Click "Hesapla" (Calculate)

2. **Manual Calculation**:
   - Select "Manuel Faiz Gir" mode
   - Enter loan amount
   - Enter loan term
   - Enter monthly interest rate
   - Click "Hesapla" (Calculate)

### Applying for a Loan

1. Login or signup for an account
2. Join relevant banks via "Banka Müşterisi Ol" tab
3. Go to "Kredi Başvuru" tab
4. Fill in personal information
5. Select bank and product (only member banks shown)
6. Enter loan amount and term
7. Submit application

### Viewing Past Applications

1. Login to your account
2. Go to "Kredi Başvuru" tab
3. Click "Geçmiş Başvurularım"
4. View all submitted applications with status

## Business Logic

### Interest Calculation Formula

The system uses the standard loan amortization formula:

```
Monthly Payment = P × (r / (1 - (1 + r)^(-n)))

Where:
P = Principal (loan amount)
r = Monthly interest rate (as decimal)
n = Number of payments (loan term)
```

### Validation Rules

- Loan amounts must be within bank product limits
- Loan terms must be within bank product limits
- Users must be bank members to apply for their products
- Users cannot apply to the same product twice
- Email addresses must be unique
- Passwords must be at least 6 characters

## Security Considerations

⚠️ **Note**: This is a demonstration project. For production use:

- Replace simple password hashing with BCrypt or Argon2
- Implement JWT tokens instead of session-based auth
- Add rate limiting and request validation
- Use HTTPS only
- Implement proper CORS policies
- Add input sanitization
- Implement proper error handling without exposing internal details

## Development

### Running Tests

```bash
dotnet test
```

### Database Migrations

```bash
dotnet ef migrations add MigrationName

dotnet ef database update

dotnet ef database update PreviousMigrationName
```

## Project Structure

```
LoanCalculation/
├── Business/
│   ├── Interfaces/        # Service interfaces
│   └── Services/          # Service implementations
├── Controllers/           # API controllers
├── Data/                  # Database context
├── Models/
│   └── Entities/          # Database models
├── Frontend/
│   └── src/
│       └── app/           # Angular application
└── Program.cs             # Application entry point
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Contact

For questions or support, please open an issue in the repository.
