# Vehicle Service Management System

A RESTful Vehicle Service Management System built with **ASP.NET Core Web API**, following Clean Architecture principles and modern backend development practices.

This project was deveoped as a portfolio project to strengthen my software engineering skills and demonstrate my understanding of designing, implementing, and testing a production-style backend application.

---

# About the Project

The Vehicle Service Management System manages customers, vehicles, and vehicle service records for an automotive service shop.

The primary objective of this project was not only to build CRUD functionality, but to practice writing maintainable backend code using layered architecture, business rules, optimistic concurrency, archive/unarchive, filtering, sorting, pagination, clear separation of responsibilities across the Controller, Service, and Repository layers.

Current Status:

**Version:** `v0.1.0 – Backend Foundation Complete`

Backend foundation is complete and considered feature-complete for Version 0.1.0.

The next milestone is building the Angular frontend before returning to automated testing and deployment.

---

# Features

## Customer Management

- Create customers
- View customer information
- Update customer details
- Archive / Unarchive customers
- Search customers
- Pagination
- Sorting
- Optimistic concurrency

---

## Vehicle Management

- Create vehicles
- Assign vehicle owners
- Reassign vehicles
- Update vehicles
- Archive / Unarchive vehicles
- Search vehicles
- Pagination
- Sorting
- Optimistic concurrency

---

## Service Record Management

- Create service records
- View vehicle service history
- Update service records
- Archive / Unarchive service records
- Filter service records
- Pagination
- Sorting
- Optimistic concurrency

Implemented business rules include:

- Mileage validation
- Cost calculation
- Service date validation
- Enum validation
- Service status state machine

---

# Technology Stack

## Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- LINQ
- Dependency Injection

## Database

- Microsoft SQL Server
- Entity Framework Core Migrations

## API Documentation

- Swagger / OpenAPI

## Testing

Current

- Manual API Testing
- Business Rule Verification
- Regression Testing
- Optimistic Concurrency Testing

Planned

- xUnit
- FluentAssertions
- Moq
- EF Core SQLite Provider

---

# Quick Start

## Clone the repository

```bash
git clone https://github.com/Jerrol0/Vehicle-Service-Management-System.git
cd Vehicle-Service-Management-System
```

## Restore dependencies
```bash
dotnet restore
```

## Update the database

```bash
dotnet ef database update
```
## Run the application

```bash
dotnet run
```

Once the application is running, Swagger UI will be available at:

```bash
https://localhost:<port>/swagger
```

---

# Architecture

The project follows a layered architecture inspired by Clean Architecture principles.

```text
Controller
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

The architecture includes:

- Controllers
- Services
- Repositories
- DTOs
- Models
- DbContext
- Dependency Injection
- Global Exception Middleware
- Unit of Work

Business rules are implemented inside the Service Layer while repositories are responsible only for data access.

---

# Project Structure

```text
VehicleServiceManagement.API
│
├── Controllers
├── Services
├── Repositories
├── Models
├── DTOs
├── Enums
├── Infrastructure
├── Common
│
├── Program.cs
└── appsettings.json
```

---

# Business Rules

Examples of implemented business rules include:

Customer

- Email must be unique

Vehicle

- Plate number must be unique
- Vehicle year cannot exceed next calendar year
- Vehicle can exist without an assigned owner

Service Record

- TotalCost = LaborCost + PartsCost
- Mileage cannot decrease
- Service date cannot be in the future
- Service status transitions are enforced through a business rule state machine.

```text
Pending
   │
   ▼
InProgress
   │
   ▼
Completed
```

Invalid transitions are rejected by the Service Layer.

---

# REST API

Customer

- CRUD
- Search
- Pagination
- Sorting

Vehicle

- CRUD
- Reassign owner
- Search
- Pagination
- Sorting

Service Record

- CRUD
- Filtering
- Vehicle service history
- State machine validation
- Pagination
- Sorting

---

# Implemented Backend Features

- RESTful API
- Repository Pattern
- Service Layer
- DTO Mapping
- Dependency Injection
- Unit of Work
- Generic Repository
- Generic Sorting
- Pagination
- Searching
- Filtering
- Archive / Unarchive
- Optimistic Concurrency
- Global Exception Middleware
- Business Rule Validation
- State Machine Validation
- SQL Server Integration
- Swagger Documentation

---

# Roadmap

Completed

- Backend API
- Database Design
- Business Logic
- Manual Testing

In Progress

- Angular Frontend

Planned

- Automated Unit Testing
- Integration Testing
- Authentication & Authorization (JWT)
- Docker
- CI/CD Pipeline
- Cloud Deployment

---

# Learning Goals

This project helped me gain hands-on experience with:

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Repository Pattern
- Service Layer Architecture
- Dependency Injection
- Clean Code Principles
- REST API Design
- Optimistic Concurrency
- Software Testing Fundamentals

---

# About Me

I am an **Information Technology graduate**, majoring in **Network and Web Applications**, and I am currently pursuing a career as a **Junior Software Engineer**.

This repository is part of my professional portfolio and reflects my ongoing learning in modern backend software development using Microsoft's technology stack.

Current learning focus:

- ASP.NET Core
- Entity Framework Core
- SQL Server
- Angular
- Software Architecture
- Automated Testing
- Docker
- CI/CD

---

# Version

Current Release

**v0.1.0 – Backend Foundation Complete**

This release represents a stable backend foundation and serves as the baseline before frontend development begins.

---

# Author

**Jerrol Lure Gutierrez**

---

# License

This project is intended for educational and portfolio purposes.
