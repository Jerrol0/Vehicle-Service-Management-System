# Vehicle Service Management System

A full-stack Vehicle Service Management System for automotive service shops, built with **ASP.NET Core Web API** and **Angular**.

This project was developed as a professional portfolio project to demonstrate practical software engineering skills in backend API development, frontend application development, database design, business-rule implementation, and maintainable application architecture.

---

## About the Project

The Vehicle Service Management System manages customers, vehicles, service records, and vehicle service history for an automotive service shop.

The primary goal was not simply to build CRUD functionality, but to design a maintainable application using clear separation of responsibilities, business rules, validation, API contracts, database persistence, and modern frontend development practices.

The system includes:

- Customer management
- Vehicle management
- Service record management
- Vehicle service history
- Searching, filtering, sorting, and pagination
- Archive and unarchive workflows
- Optimistic concurrency handling
- Business-rule validation
- RESTful API integration
- Modern Angular frontend

---

## Current Status

**Version: v1.0.0 – Full-Stack Functional Release**

The core backend and frontend functionality is complete across all major modules.

### Completed

- [x] Backend REST API
- [x] Database and Entity Framework Core integration
- [x] Customer management
- [x] Vehicle management
- [x] Service record management
- [x] Vehicle service history
- [x] Angular frontend
- [x] API integration
- [x] Searching, filtering, sorting, and pagination
- [x] Archive / unarchive workflows
- [x] Optimistic concurrency handling
- [x] Business-rule validation
- [x] Manual API and application testing

### In Progress

- [ ] Global UI/UX polish and visual refinement

### Future Improvements

- [ ] Automated unit and integration testing
- [ ] Authentication and authorization
- [ ] Docker
- [ ] CI/CD
- [ ] Cloud deployment

---

## Features

### Customer Management

- Create customers
- View customer information
- Update customer details
- Archive / unarchive customers
- Search customers
- Pagination
- Sorting
- Optimistic concurrency

### Vehicle Management

- Create vehicles
- Assign vehicle owners
- Reassign vehicles
- Update vehicle information
- Archive / unarchive vehicles
- Search vehicles
- Pagination
- Sorting
- Optimistic concurrency

### Service Record Management

- Create service records
- View service records
- Update service records
- Archive / unarchive service records
- Filter service records
- Pagination
- Sorting
- Vehicle service history
- Optimistic concurrency

### Business Rules

The application enforces business rules within the service layer, including:

- Unique customer email addresses
- Unique vehicle plate numbers
- Vehicle year validation
- Service date validation
- Mileage validation
- Service cost calculation
- Service status transition rules
- Enum validation
- Optimistic concurrency validation

### Service Status Workflow

```text
Pending
   │
   ▼
InProgress
   │
   ▼
Completed
```

Invalid status transitions are rejected by the service layer.

---

## Technology Stack

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- LINQ
- Dependency Injection
- RESTful API
- OpenAPI / Swagger

### Frontend

- Angular
- TypeScript
- Angular Material
- SCSS
- Generated API client

### Database

- Microsoft SQL Server
- Entity Framework Core Migrations

### Development & Testing Tools

- Visual Studio
- Visual Studio Code
- Git / GitHub
- Swagger UI
- Postman
- SQL Server Management Studio

---

## Architecture

The project follows a **layered architecture inspired by Clean Architecture principles**, with clear separation between HTTP handling, business logic, data access, and database persistence.

```text
Angular Frontend
       │
       ▼
ASP.NET Core Web API
       │
       ▼
Controllers
       │
       ▼
Service Layer
       │
       ▼
Repository Layer
       │
       ▼
Entity Framework Core
       │
       ▼
SQL Server
```

The backend includes:

- Controllers
- Services
- Repositories
- DTOs
- Models
- Entity Framework Core DbContext
- Dependency Injection
- Unit of Work
- Global Exception Middleware
- Shared infrastructure and common components

### Separation of Responsibilities

**Controllers**

Handle HTTP requests, responses, routing, and API behavior.

**Services**

Contain business logic, validation, workflows, and application rules.

**Repositories**

Handle data-access operations and persistence concerns.

**Entity Framework Core**

Provides ORM functionality and database interaction.

This separation keeps controllers thin and prevents business rules from being coupled to HTTP or database implementation details.

---

## Project Structure

```text
Vehicle-Service-Management-System
│
├── Backend
│   └── VehicleServiceManagement.API
│       ├── Controllers
│       ├── Services
│       ├── Repositories
│       ├── Models
│       ├── DTOs
│       ├── Enums
│       ├── Infrastructure
│       ├── Common
│       ├── Program.cs
│       └── appsettings.json
│
└── Frontend
    └── vehicle-service-management
        └── src
```

> The exact directory structure may evolve as the project continues to be refined.

---

## API

The application exposes a RESTful API for managing:

### Customers

- CRUD operations
- Search
- Pagination
- Sorting
- Archive / unarchive

### Vehicles

- CRUD operations
- Owner assignment
- Owner reassignment
- Search
- Pagination
- Sorting
- Archive / unarchive

### Service Records

- CRUD operations
- Filtering
- Pagination
- Sorting
- Vehicle service history
- Archive / unarchive
- Status workflow validation

API documentation is provided through **Swagger / OpenAPI** during development.

---

## Testing

The application has been manually tested throughout development, including:

- API endpoint verification
- Request and response validation
- Business-rule verification
- Validation testing
- Regression testing
- Optimistic concurrency testing
- Frontend and backend integration testing
- End-to-end feature verification

Automated testing is intentionally deferred to a future development phase.

---

## Quick Start

### Prerequisites

- .NET SDK
- SQL Server / SQL Server Express
- Node.js and npm
- Angular CLI

### Clone the Repository

```bash
git clone https://github.com/Jerrol0/Vehicle-Service-Management-System.git
cd Vehicle-Service-Management-System
```

### Backend Setup

Restore dependencies:

```bash
dotnet restore
```

Apply Entity Framework Core migrations:

```bash
dotnet ef database update
```

Run the API:

```bash
dotnet run
```

Swagger UI will be available at the HTTPS development URL shown by the application.

### Frontend Setup

Navigate to the Angular application:

```bash
cd vehicle-service-management
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
ng serve
```

The Angular application will be available at:

```text
http://localhost:4200
```

> Database connection settings should be configured for the local SQL Server environment before running the backend.

---

## Key Engineering Practices

The project emphasizes maintainability and separation of concerns through:

- DTO-based API contracts
- Service-layer business logic
- Repository-based data access
- Dependency Injection
- Unit of Work
- Global exception handling
- Validation
- Generic pagination
- Searching and sorting
- Archive / unarchive workflows
- Optimistic concurrency
- Entity Framework Core migrations
- RESTful API design
- Angular service-based API integration
- Reusable frontend components and directives

---

## Roadmap

### Completed

- [x] Database design
- [x] Entity Framework Core integration
- [x] Backend REST API
- [x] Business logic and validation
- [x] Customer management
- [x] Vehicle management
- [x] Service record management
- [x] Vehicle service history
- [x] Angular frontend
- [x] API integration
- [x] Manual testing

### Current

- [ ] Global UI/UX polish

### Future

- [ ] Automated unit testing
- [ ] Integration testing
- [ ] Authentication and authorization
- [ ] Docker
- [ ] CI/CD pipeline
- [ ] Cloud deployment

---

## Learning & Engineering Goals

This project was built to gain practical experience with:

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- Angular
- TypeScript
- REST API design
- Layered application architecture
- Dependency Injection
- Repository and Unit of Work patterns
- Business-rule implementation
- Optimistic concurrency
- API and application testing
- Maintainable software design

The project also serves as a practical foundation for exploring modern approaches to improving and modernizing legacy applications.

---

## About the Developer

I am an Information Technology graduate majoring in **Network and Web Applications**, with a focus on full-stack software development and modern application development.

This repository is part of my professional portfolio and demonstrates my hands-on experience building a full-stack application using Microsoft's backend ecosystem together with Angular.

### Current Technology Focus

- C#
- ASP.NET Core
- Entity Framework Core
- SQL Server
- Angular
- TypeScript
- Software Architecture
- Software Testing
- Modern Development Tools

---

## Author

**Jerrol Lure Gutierrez**

GitHub: [Jerrol0](https://github.com/Jerrol0)

---

## License

This project is intended for educational and portfolio purposes.
