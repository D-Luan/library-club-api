# Library Club API

## About

Library Club API is an ASP.NET Core Web API for managing readers, reading clubs, and club subscriptions.

The main goal of this project was to practice what happens when a record should stop being used, but should not disappear from the system. Subscriptions are canceled with `CanceledAt`, reading clubs can be archived, and readers or clubs move between statuses instead of being removed from the database.

That made the project a good place to work with cancellation flows, historical records, domain rules, and consistency between the API and the database. It was also a way to consolidate the backend stack I have been practicing: Dapper for SQL access, FluentValidation for request validation, Serilog for logging, DbUp for migrations, automated tests with xUnit and Testcontainers, and the Azure side with App Service, Azure SQL, Application Insights, GitHub Actions, and Bicep.

## Tech Stack

- .NET 10 / ASP.NET Core Web API
- SQL Server / Azure SQL Database
- Dapper
- DbUp
- FluentValidation
- Serilog
- Swagger/OpenAPI
- xUnit, NSubstitute, and Testcontainers
- Docker Compose
- GitHub Actions
- Azure App Service, Application Insights, and Bicep

## Features

- Reader creation, listing, lookup, inactivation, and reactivation
- Reading club creation, update, listing, lookup, inactivation, reactivation, and archiving
- Club subscriptions between readers and reading clubs
- Subscription cancellation with `CanceledAt` preserved as history
- Prevention of duplicate active subscriptions for the same reader and club
- Status rules kept in the domain models
- Paginated list endpoints
- Request validation with FluentValidation
- Centralized error handling
- Health check endpoint
- Database migrations at startup with DbUp
- Unit and integration tests, including SQL Server through Testcontainers

## API Overview

### Readers

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/readers` | Create a reader |
| `GET` | `/api/readers` | List readers with pagination |
| `GET` | `/api/readers/{id}` | Get a reader by ID |
| `GET` | `/api/readers/{readerId}/subscriptions` | List reader subscriptions |
| `PATCH` | `/api/readers/{id}/inactivate` | Inactivate a reader |
| `PATCH` | `/api/readers/{id}/reactivate` | Reactivate a reader |

### Reading Clubs

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/reading-clubs` | Create a reading club |
| `GET` | `/api/reading-clubs` | List reading clubs with pagination |
| `GET` | `/api/reading-clubs/{id}` | Get a reading club by ID |
| `GET` | `/api/reading-clubs/{readingClubId}/subscriptions` | List club subscriptions |
| `PUT` | `/api/reading-clubs/{id}` | Update a reading club |
| `PATCH` | `/api/reading-clubs/{id}/inactivate` | Inactivate a reading club |
| `PATCH` | `/api/reading-clubs/{id}/reactivate` | Reactivate a reading club |
| `PATCH` | `/api/reading-clubs/{id}/archive` | Archive a reading club |

### Club Subscriptions

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/club-subscriptions` | Subscribe a reader to a reading club |
| `GET` | `/api/club-subscriptions/{id}` | Get a subscription by ID |
| `PATCH` | `/api/club-subscriptions/{id}/cancel` | Cancel a subscription |

## Database

The API uses SQL Server as the relational database.

Database scripts are stored in:

```text
src/LibraryClub.Api/Scripts/
```

DbUp runs the scripts during application startup. The current schema includes:

- `Readers`
- `ReadingClubs`
- `ClubSubscriptions`

The `ClubSubscriptions` table keeps canceled records and uses a filtered unique index to allow only one active subscription for the same reader and reading club.

## Running Locally

### Prerequisites

- .NET 10 SDK
- Docker

### Start SQL Server

Create a local `.env` file with the SQL Server settings used by Docker Compose:

```text
SQLSERVER_SA_PASSWORD=Your_password123
SQLSERVER_PORT=1433
```

Then start the container:

```powershell
docker compose up -d
```

### Configure the Connection String

The API expects a connection string named `DefaultConnection`.

Using user secrets:

```powershell
cd src/LibraryClub.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=LibraryClub;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
```

### Run the API

```powershell
cd src
dotnet run --project LibraryClub.Api
```

Swagger is available at:

```text
https://localhost:7284/swagger
```

Health check:

```text
https://localhost:7284/health
```

## Running Tests

```powershell
cd src
dotnet test
```

The integration tests use Testcontainers to start SQL Server and run the API/repository tests against a real database.

## CI/CD and Azure

The GitHub Actions workflow restores dependencies, builds the solution, runs unit and integration tests, and deploys the API to Azure App Service from the `main` branch.

The `infra` folder contains the Bicep setup for the Azure resources used by the project.
