# Library Club API

REST API built with .NET to manage readers, reading clubs, and club subscriptions.

This project is still in development. The main goal is to practice building a backend API with tools and technologies commonly used in a more realistic project workflow, following the same learning purpose as `edu-track-api`.

## What has been done

- Initial API structure with ASP.NET Core and .NET 10.
- Endpoints for readers, reading clubs, and club subscriptions.
- Separation between controllers, services, repositories, DTOs, validators, and models.
- Data access with Dapper and SQL Server.
- Initial database migration with DbUp.
- Domain rules for reader, reading club, and subscription statuses.
- Input validation with FluentValidation.
- Global exception handling with standardized responses.
- Pagination for the main list endpoints.
- Swagger/OpenAPI enabled for API documentation during development.
- Structured logging with Serilog.
- Optional telemetry with Application Insights.
- Unit and integration tests with xUnit, NSubstitute, and Testcontainers.
- Local SQL Server environment with Docker Compose.
- CI/CD pipeline with GitHub Actions for restore, build, tests, and Azure App Service deployment.
- Initial infrastructure with Bicep for App Service, Azure SQL, Log Analytics, and Application Insights.

## Technologies being studied

- ASP.NET Core
- .NET 10
- SQL Server
- Dapper
- DbUp
- FluentValidation
- Serilog
- Swagger/OpenAPI
- xUnit
- NSubstitute
- Testcontainers
- Docker Compose
- GitHub Actions
- Azure App Service
- Azure SQL
- Bicep
