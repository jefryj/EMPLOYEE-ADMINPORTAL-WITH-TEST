# Employee Admin Portal with Unit Tests

## Overview

Employee Admin Portal is an ASP.NET Core Web API project developed as part of a .NET training program. The application provides employee management functionality with authentication, authorization, department and project management, audit logging, caching, filtering, sorting, pagination, and unit testing.

## Features

### Authentication & Authorization
- JWT-based authentication
- Role-based authorization
- Admin and Employee roles
- Secure password hashing using ASP.NET Core Identity PasswordHasher

### Employee Management
- Create employees
- Retrieve employees
- Update employee details
- Delete employees
- Search employees by name or email
- Filter by department
- Sorting and pagination

### Department Management
- Create departments
- Retrieve departments
- Update departments
- Delete departments
- Search and pagination support
- In-memory caching for department listings

### Project Management
- Create projects
- Retrieve projects
- Update projects
- Delete projects
- Project member count tracking

### Audit Logging
- Tracks Create, Update, and Delete operations
- Stores action details, username, and timestamp

### Error Handling
- Global exception handling middleware
- Consistent API error responses

### Testing
- xUnit test project
- In-memory database testing using EF Core InMemory
- Controller unit tests for:
  - EmployeesController
  - DepartmentsController
  - ProjectsController
  - ReportsController

---

## Technologies Used

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- ASP.NET Core Identity PasswordHasher
- IMemoryCache
- Serilog
- xUnit
- Moq
- EF Core InMemory Provider

---

## Project Structure

```text
EmployeeAdminPortal
│
├── EmployeeAdminPortal
│   ├── Controllers
│   ├── Models
│   ├── Data
│   ├── Middleware
│   ├── Migrations
│   └── Program.cs
│
├── EmployeeAdminPortal.Tests
│   ├── EmployeesControllerTests.cs
│   ├── DepartmentsControllerTests.cs
│   ├── ProjectsControllerTests.cs
│   └── ReportsControllerTests.cs
│
└── EmployeeAdminPortal.slnx
