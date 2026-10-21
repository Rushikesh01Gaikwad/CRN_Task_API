# RESTful Backend API Solution — CRN Technical Assessment

An enterprise-grade, scalable, and maintainable RESTful API solution built with **.NET 8**, **C#**, and **Clean Architecture** principles.

---

## 🏛️ Architecture Overview

The solution adheres strictly to **Clean Architecture** (Separation of Concerns and Dependency Inversion Principle). Dependencies point inward toward the core Domain.

```
Solution/
├── src/
│   ├── API/                       # ASP.NET Core Web API (Controllers, Versioning, Middleware, Filters)
│   ├── Application/               # Application Logic (DTOs, Interfaces, Services, Validators, Mapping)
│   ├── Domain/                    # Enterprise Core (Entities, Enums, Events, Exceptions)
│   └── Infrastructure/            # External Concerns (EF Core, Repositories, Unit of Work, Identity, JWT)
├── tests/
│   ├── API.Tests/                 # End-to-end Integration Tests using WebApplicationFactory
│   ├── Application.Tests/         # Unit Tests with xUnit, Moq, and FluentAssertions
│   └── Infrastructure.Tests/      # Repository & Data layer tests with EF Core InMemory
├── Dockerfile                     # Multi-stage optimized container build
└── docker-compose.yml             # Orchestration for SQL Server 2022 & CRN Web API
```

---

## 🛠️ Tech Stack

- **Runtime & Language**: .NET 8, C# 12
- **Web API Framework**: ASP.NET Core Web API with API Versioning (`api/v1/...`)
- **ORM & Data Access**: Entity Framework Core 8 with SQL Server provider
- **Design Patterns**: Repository Pattern, Unit of Work, Dependency Injection
- **Authentication**: JWT Bearer Tokens with cryptographic Refresh Token rotation
- **Validation**: FluentValidation with automatic action filters
- **Logging**: Serilog structured logging (Console & Rolling File sinks)
- **Performance**: Response compression (Brotli/Gzip), `AsNoTracking()` for read queries, and database pagination
- **Documentation**: Swagger / OpenAPI (Swashbuckle) with Bearer token authentication support
- **Testing**: xUnit, Moq, FluentAssertions, and `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`)
- **Containerization**: Docker multi-stage build & Docker Compose

---

## 🗄️ Database Schema

The database model follows the assessment specifications:

```sql
CREATE TABLE [dbo].[Product]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductName] NVARCHAR(255) NOT NULL,
    [CreatedBy] NVARCHAR(100) NOT NULL,
    [CreatedOn] DATETIME NOT NULL,
    [ModifiedBy] NVARCHAR(100) NULL,
    [ModifiedOn] DATETIME NULL
);

CREATE TABLE [dbo].[Item]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [ProductId] INT NOT NULL FOREIGN KEY REFERENCES Product(Id) ON DELETE CASCADE,
    [Quantity] INT NOT NULL
);

CREATE TABLE [dbo].[Users]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [Username] NVARCHAR(50) NOT NULL UNIQUE,
    [Email] NVARCHAR(150) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [Role] NVARCHAR(20) NOT NULL,
    [CreatedAt] DATETIME NOT NULL
);

CREATE TABLE [dbo].[RefreshTokens]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
    [UserId] INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    [Token] NVARCHAR(256) NOT NULL,
    [ExpiresOn] DATETIME NOT NULL,
    [CreatedOn] DATETIME NOT NULL,
    [RevokedOn] DATETIME NULL,
    [ReplacedByToken] NVARCHAR(MAX) NULL
);
```

---

## 🔐 Authentication & Token Rotation Flow

```
+------------+          +---------------+          +--------------------+
|   Client   |  ----->  |  POST /login  |  ----->  | Validates BCrypt   |
|            |  <-----  | Access Token  |  <-----  | Issues Access Token|
|            |          | Refresh Token |          | & Refresh Token    |
+------------+          +---------------+          +--------------------+
      |
      | (When Access Token Expires)
      v
+------------+          +-----------------------+
|   Client   |  ----->  |  POST /refresh-token  |
|            |          | (Old Refresh Token)   |
|            |          +-----------------------+
|            |                      |
|            |                      v
|            |          [Revoke Old Refresh Token]
|            |          [Issue New Refresh Token] (Rotation)
|            |                      |
|            |  <-------------------+
|            |  New Access & Refresh Tokens
+------------+
```

---

## 🚀 API Endpoints Overview

All endpoints follow RESTful resource-oriented naming standards and return a consistent response envelope (`ApiResponse<T>`).

### 1. Products (`/api/v1/products`)
| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/products?page=1&pageSize=10&search=...` | Paged products list | `200 OK` |
| `GET` | `/api/v1/products/{id}` | Get product by ID (includes Items) | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/products` | Create a new product (with optional items) | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/v1/products/{id}` | Update product details | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/v1/products/{id}` | Delete product and cascading items | `204 No Content`, `404 Not Found` |

### 2. Product Items (Sub-resources) (`/api/v1/products/{productId}/items`)
| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/products/{productId}/items` | Get all items for a product | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/products/{productId}/items/{itemId}` | Get specific item details | `200 OK`, `404 Not Found` |
| `POST` | `/api/v1/products/{productId}/items` | Add an item to a product | `201 Created`, `400 Bad Request` |
| `PUT` | `/api/v1/products/{productId}/items/{itemId}` | Update item quantity | `200 OK`, `400 Bad Request` |
| `DELETE` | `/api/v1/products/{productId}/items/{itemId}` | Remove item from product | `204 No Content`, `404 Not Found` |

### 3. Authentication (`/api/v1/auth`)
| Method | Endpoint | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register user account |
| `POST` | `/api/v1/auth/login` | Authenticate and obtain JWT access + refresh token |
| `POST` | `/api/v1/auth/refresh-token` | Rotate refresh token and get fresh access token |
| `POST` | `/api/v1/auth/revoke-token` | Revoke a refresh token |

---

## 🧪 Testing Strategy

The solution includes comprehensive unit and integration tests across multiple test projects:

```bash
dotnet test CRN_Solution.sln
```

- **`Application.Tests`**: Unit tests verifying `ProductService`, `AuthService`, and `FluentValidation` rules using `Moq` and `FluentAssertions`.
- **`Infrastructure.Tests`**: Unit tests for repository queries, pagination, and `AsNoTracking` with Entity Framework Core InMemory provider.
- **`API.Tests`**: Full end-to-end integration tests using `WebApplicationFactory<Program>` validating HTTP status codes, middleware, and request/response payloads.

---

## 🐳 Running with Docker

### Prerequisites
- Docker & Docker Compose installed

### Start SQL Server & API:
```bash
docker-compose up --build -d
```

- **Swagger Documentation**: `http://localhost:5000/swagger`
- **SQL Server**: `localhost:1433` (Password: `YourStrong@Passw0rd!`)

---

## 💻 Local Development Setup

### 1. Configure Database Connection
Update `appsettings.json` in `src/API/` with your local SQL Server instance:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CRNProductDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Run Migrations & Start API
```bash
cd src/API
dotnet run
```
Swagger UI will be accessible at: `https://localhost:7059/swagger` or `http://localhost:5000/swagger`.

### 3. Start Angular UI
```bash
cd ../../CRN_Task_UI
npm install
ng serve
```
Open your browser at `http://localhost:4200/`.
