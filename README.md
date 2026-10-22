# 🚀 RESTful Backend API — CRN Technical Solution

An enterprise-grade, scalable, and maintainable RESTful Web API built with **.NET 8**, **C# 12**, and **Clean Architecture** principles, designed to power the Angular frontend client with robust CRUD operations, hierarchical sub-resource management, JWT authentication with token rotation, and resilient database access.

---

## 📑 Table of Contents
1. [Architectural Overview](#-architectural-overview)
2. [What Was Built (Completed Work)](#-what-was-built-completed-work)
3. [End-to-End Connectivity: API & UI Flow](#-end-to-end-connectivity-api--ui-flow)
4. [Tech Stack & Dependencies](#-tech-stack--dependencies)
5. [Database Schema & Relationships](#-database-schema--relationships)
6. [API Endpoints Reference](#-api-endpoints-reference)
7. [Authentication & Security Architecture](#-authentication--security-architecture)
8. [CORS & Frontend Integration](#-cors--frontend-integration)
9. [Running the API Locally](#-running-the-api-locally)
10. [Docker & Containerization](#-docker--containerization)
11. [Testing & Quality Assurance](#-testing--quality-assurance)

---

## 🏛️ Architectural Overview

The backend strictly adheres to **Clean Architecture** (Separation of Concerns and the Dependency Inversion Principle). Dependencies flow strictly inward toward the domain layer:

```
CRUD_API/
├── src/
│   ├── Domain/                 # Enterprise Core
│   │   ├── Entities/           # Product, Item, User, RefreshToken
│   │   ├── Common/             # BaseEntity, AuditableEntity
│   │   └── Exceptions/         # Domain-specific business exceptions
│   │
│   ├── Application/            # Business & Application Logic
│   │   ├── Common/             # ApiResponse<T>, PagedResult<T>
│   │   ├── DTOs/               # Products, Items, Auth request/response DTOs
│   │   ├── Interfaces/         # IProductRepository, IItemRepository, IUnitOfWork, IJwtService
│   │   ├── Services/           # ProductService, ItemService, AuthService
│   │   └── Validators/         # FluentValidation rules for all incoming requests
│   │
│   ├── Infrastructure/         # External Concerns & Persistence
│   │   ├── Data/               # ApplicationDbContext (EF Core 8)
│   │   ├── Repositories/       # Generic & dedicated repositories, Unit of Work
│   │   ├── Services/           # JwtTokenGenerator, BCrypt PasswordHasher
│   │   └── EntityConfigurations/# Fluent API table configurations & cascade rules
│   │
│   └── API/                    # Presentation Layer (HTTP / REST)
│       ├── Controllers/v1/     # ProductsController, ItemsController, AuthController
│       ├── Extensions/         # ServiceCollectionExtensions, SwaggerExtensions
│       ├── Filters/            # ValidationFilter (automatic FluentValidation handling)
│       ├── Middleware/         # ExceptionHandlingMiddleware, SecurityHeadersMiddleware
│       ├── Program.cs          # Pipeline bootstrap & DI composition
│       └── appsettings.json    # Connection strings, JWT settings, Serilog config
│
└── tests/
    ├── API.Tests/              # WebApplicationFactory Integration Tests
    ├── Application.Tests/      # Service & validator unit tests (Moq + FluentAssertions)
    └── Infrastructure.Tests/   # Repository & EF Core in-memory data tests
```

---

## ✅ What Was Built (Completed Work)

### 1. Products Management (`[dbo].[Product]`)
- **Full CRUD Support**: Create, Read (single + paged), Update, and Delete.
- **Auditing**: Automatic tracking of `CreatedBy`, `CreatedOn`, `ModifiedBy`, and `ModifiedOn`.
- **Server-Side Pagination & Filtering**: Real-time keyword search across product name and creator, configurable page size (`page`, `pageSize`).
- **Cascading Deletion**: Deleting a product automatically cleans up all associated inventory stock items.

### 2. Hierarchical Stock Items (`[dbo].[Item]`)
- **RESTful Sub-resources**: Endpoints mapped under `/api/v1/products/{productId}/items`.
- **Isolated Item Lifecycle**: Add stock items, retrieve by ID, adjust quantities, and delete items with foreign key validation against the parent product.

### 3. Authentication & Security System
- **Identity & Registration**: Secure user signup (`/api/v1/auth/register`) with BCrypt password hashing.
- **JWT Bearer Authentication**: Short-lived, digitally signed access tokens.
- **Refresh Token Rotation**: Cryptographically secure refresh tokens stored in database with automatic rotation and revocation logic.
- **Security Headers & Middleware**: Enforces X-Content-Type-Options, X-Frame-Options, CSP, and uniform error logging.

### 4. Enterprise Cross-Cutting Concerns
- **Standardized Response Envelope**: All API responses wrapped in `ApiResponse<T>` (`success`, `data`, `message`, `errors`).
- **FluentValidation Pipeline**: Automatic model state interception with descriptive 400 Bad Request error dictionaries.
- **Serilog Structured Logging**: File & console logs capturing execution context, timings, and HTTP status codes.
- **Brotli & Gzip Compression**: Minimizes network bandwidth between API and UI.

---

## 🔗 End-to-End Connectivity: API & UI Flow

The diagram below illustrates how the **Angular UI** interacts with the **.NET Core Web API**:

```
+-----------------------------------------------------------------------------------+
|                                 ANGULAR 15 UI                                     |
|                               (http://localhost:4200)                             |
+-----------------------------------------------------------------------------------+
       |                    |                    |                     |
   (Products)           (Stock Items)       (Auth/Token)         (Health/Ping)
       |                    |                    |                     |
       v                    v                    v                     v
+-----------------------------------------------------------------------------------+
|                        HTTP CLIENT & AUTH INTERCEPTOR                             |
|  - Injects 'Authorization: Bearer <token>'                                        |
|  - Auto-catches 401 Unauthorized -> Calls Refresh Token -> Retries failed request  |
|  - Targets Dynamic Base URL: https://localhost:7025 (or http://localhost:5287)    |
+-----------------------------------------------------------------------------------+
                                         |
                            (CORS Preflight & REST HTTP)
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                               .NET 8 WEB API                                      |
|                           (https://localhost:7025)                                |
+-----------------------------------------------------------------------------------+
|  1. CORS Middleware          : Validates Origin http://localhost:4200             |
|  2. Security Headers         : Injects XSS, Framing, Content-Type headers         |
|  3. Global Exception Handler : Catches unhandled errors, returns ApiResponse<T>   |
|  4. Response Compression     : Compresses payload (Brotli / Gzip)                 |
|  5. JWT Bearer Auth          : Validates Claims, Expiration, Issuer, Audience     |
|  6. Validation Filter        : Evaluates FluentValidation rules                   |
|  7. Controllers (v1)         : ProductsController, ItemsController, AuthController|
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                       APPLICATION & INFRASTRUCTURE LAYER                          |
|  - Services: ProductService, ItemService, AuthService, JwtService                 |
|  - Repositories: ProductRepository, ItemRepository, RefreshTokenRepository        |
|  - Unit of Work & EF Core 8: Change tracking & atomic transactions                |
+-----------------------------------------------------------------------------------+
                                         |
                                         v
+-----------------------------------------------------------------------------------+
|                               SQL SERVER DATABASE                                 |
|       Tables: [Product], [Item], [Users], [RefreshTokens]                         |
+-----------------------------------------------------------------------------------+
```

---

## 🛠️ Tech Stack & Dependencies

| Area | Technology | Purpose |
| :--- | :--- | :--- |
| **Framework** | .NET 8.0 / C# 12 | High-performance Web API platform |
| **Data Access** | Entity Framework Core 8 | Object-Relational Mapper (ORM) |
| **Database** | Microsoft SQL Server (LocalDB / Express / Docker) | Relational persistence |
| **API Versioning** | `Asp.Versioning.Mvc` (v1.0) | URL segment versioning (`/api/v1/...`) |
| **Validation** | FluentValidation 11 | Strong typing & expressive rule declarations |
| **Authentication** | `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT validation & authorization |
| **Hashing** | BCrypt.Net-Next | Salted password hashing |
| **Logging** | Serilog & Serilog.Sinks.File | Structured contextual logging |
| **Documentation**| Swashbuckle.AspNetCore (Swagger UI) | Interactive API exploration & testing |
| **Unit Testing** | xUnit, Moq, FluentAssertions | Unit test suite |
| **Integration** | `Microsoft.AspNetCore.Mvc.Testing` | Full-pipeline in-memory HTTP integration tests |

---

## 🗄️ Database Schema & Relationships

### Entity Relationship Diagram (ERD)

```
+-----------------------------------+            +-----------------------------------+
|             Product               |            |               Item                |
+-----------------------------------+            +-----------------------------------+
| PK  Id          INT (Identity)    | 1        * | PK  Id         INT (Identity)     |
|     ProductName NVARCHAR(255)     |<-----------| FK  ProductId  INT (Cascade Delete)|
|     CreatedBy   NVARCHAR(100)     |            |     Quantity   INT                 |
|     CreatedOn   DATETIME          |            +-----------------------------------+
|     ModifiedBy  NVARCHAR(100) NULL|
|     ModifiedOn  DATETIME NULL     |
+-----------------------------------+

+-----------------------------------+            +-----------------------------------+
|              Users                |            |           RefreshTokens           |
+-----------------------------------+            +-----------------------------------+
| PK  Id          INT (Identity)    | 1        * | PK  Id              INT (Identity)|
|     Username    NVARCHAR(50) [UQ] |<-----------| FK  UserId          INT           |
|     Email       NVARCHAR(150)[UQ] |            |     Token           NVARCHAR(256) |
|     PasswordHash NVARCHAR(MAX)    |            |     ExpiresOn       DATETIME      |
|     Role        NVARCHAR(20)      |            |     CreatedOn       DATETIME      |
|     CreatedAt   DATETIME          |            |     RevokedOn       DATETIME NULL |
+-----------------------------------+            |     ReplacedByToken NVARCHAR(MAX) |
                                                 +-----------------------------------+
```

---

## 🚀 API Endpoints Reference

All responses conform to the consistent envelope:
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... },
  "errors": null
}
```

### 1. Products Resource (`/api/v1/products`)
| HTTP Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/products?page=1&pageSize=10&search=lap` | Retrieve paginated products with optional search | Optional |
| `GET` | `/api/v1/products/{id}` | Get product details including all attached items | Optional |
| `POST` | `/api/v1/products` | Create a new product (with optional initial items) | Optional |
| `PUT` | `/api/v1/products/{id}` | Update product name and audit information | Optional |
| `DELETE` | `/api/v1/products/{id}` | Delete product and cascade delete all child items | Optional |

### 2. Product Items Sub-Resource (`/api/v1/products/{productId}/items`)
| HTTP Method | Route | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/products/{productId}/items` | List all inventory items for a product | Optional |
| `GET` | `/api/v1/products/{productId}/items/{itemId}` | Get specific item details | Optional |
| `POST` | `/api/v1/products/{productId}/items` | Add a stock item with quantity to a product | Optional |
| `PUT` | `/api/v1/products/{productId}/items/{itemId}` | Update quantity of a stock item | Optional |
| `DELETE` | `/api/v1/products/{productId}/items/{itemId}` | Delete an individual item | Optional |

### 3. Authentication Resource (`/api/v1/auth`)
| HTTP Method | Route | Description |
| :--- | :--- | :--- |
| `POST` | `/api/v1/auth/register` | Register a new account (`username`, `email`, `password`) |
| `POST` | `/api/v1/auth/login` | Authenticate credentials -> returns JWT access & refresh token |
| `POST` | `/api/v1/auth/refresh-token` | Submit expired access token + refresh token -> returns fresh pair |
| `POST` | `/api/v1/auth/revoke-token` | Explicitly invalidate a refresh token |

---

## 🔐 Authentication & Security Architecture

1. **Password Hashing**: Stored with strong one-way BCrypt salt (work factor 11).
2. **JWT Access Tokens**: Encoded with HMAC-SHA256, contains standard claims (`NameIdentifier`, `Email`, `Role`, `Jti`). Configured with a short lifespan (e.g., 60 minutes).
3. **Cryptographic Refresh Tokens**: 64-byte random sequence generated via `RandomNumberGenerator`.
4. **Token Rotation**: When the client requests `/api/v1/auth/refresh-token`, the old refresh token is marked as `RevokedOn` with `ReplacedByToken` tracking, preventing token replay attacks.

---

## 🌐 CORS & Frontend Integration

The API explicitly authorizes requests from the Angular development client in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

- **HTTPS Address**: `https://localhost:7025`
- **HTTP Address**: `http://localhost:5287`
- **Swagger Documentation URL**: `https://localhost:7025/swagger`

---

## 💻 Running the API Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, SQL Server Express, or Docker container)

### Step 1: Configure Connection String
Open `src/API/appsettings.json` and adjust the connection string to point to your SQL Server instance:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CRN_TechSoft_DB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Step 2: Build & Apply Migrations / Create DB
On initial startup, `Program.cs` automatically invokes `dbContext.Database.EnsureCreated()`, auto-generating tables if they don't already exist. Alternatively, apply EF migrations:
```bash
dotnet ef database update --project src/Infrastructure --startup-project src/API
```

### Step 3: Run the API
```bash
cd src/API
dotnet run --launch-profile https
```
The console will display:
```
Now listening on: https://localhost:7025
Now listening on: http://localhost:5287
```

Navigate to `https://localhost:7025/swagger` in your browser to inspect and interact with the endpoints.

---

## 🐳 Docker & Containerization

A complete multi-stage `Dockerfile` and `docker-compose.yml` are provided in the project root:

```bash
# Build and run API along with SQL Server 2022
docker compose up -d --build
```

---

## 🧪 Testing & Quality Assurance

The solution contains a multi-tier automated test suite:

```bash
# Run all unit and integration tests
dotnet test
```

- **`API.Tests`**: Validates full HTTP request/response pipeline via `WebApplicationFactory<Program>`, testing status codes, payload structures, and filter executions.
- **`Application.Tests`**: Tests validation rules and business services using `Moq` and `FluentAssertions`.
- **`Infrastructure.Tests`**: Exercises repository queries, cascading deletes, and EF Core mappings using in-memory databases.
