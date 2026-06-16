<div align="center">

<img src="assets/library-logo.png" alt="Library Logo" width="400">

# Library Management System

**Library je projekt zameraný na vytvorenie systému pre správu kníh, pričom systém disponuje technológiami Angular a .NET**

![C#](https://img.shields.io/badge/C%23-passing-brightgreen?logo=csharp)
![.NET](https://img.shields.io/badge/.NET-passing-brightgreen?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-passing-brightgreen?logo=angular)
![SQL](https://img.shields.io/badge/SQL-passing-brightgreen?logo=database)

---

</div>

## Quick Start

### Clone the Repository
```bash
git clone [https://github.com/SamuelSivak/Library.git](https://github.com/SamuelSivak/Library.git)
cd Library
```

### Using Docker Compose (Recommended for full environment)
This approach leverages Docker Compose to orchestrate all required services, ensuring that caching, object storage, and logging work correctly out of the box. The environment includes:
* ASP.NET Core Web API (running on port 5185 internally)
* Angular Client (running on port 4200 internally)
* MS SQL Server 2022 (with custom Full-Text Search enabled, running on port 1433)
* Redis (for distributed cache, running on port 6379)
* MinIO (S3-compatible object storage, running on ports 9000-9001)
* Seq (centralized logging, running on port 5341)
* Nginx (reverse proxy, running on port 80 to tie the frontend and API together)

To build and start all containers simultaneously, run the following command in the root directory:
```bash
docker compose up -d --build
```

Once execution is complete, the services will be available at:
* **Frontend Web Application**: http://localhost
* **API Documentation (Swagger)**: http://localhost/swagger
* **Structured Logs Console (Seq)**: http://localhost:5341
* **MinIO Object Console**: http://localhost:9001

---

## Local Development Setup
This setup is ideal if you prefer to run the backend and frontend services directly on your local machine.

> [!NOTE]
> ### Prerequisites
> * .NET 10 SDK installed
> * Node.js and npm installed
> * A running instance of SQL Server 2022 with Full-Text Search (FTS) feature enabled
> * A running instance of Redis


### A. Backend (C# .NET Core API)
1. **Navigate into the backend directory:**
```bash
cd Library
```
2. **Restore Dependencies:**
```bash
dotnet restore
```
3. **Build the project:**
```bash
dotnet build
```
4. **Apply Migrations and Run the API:**

```bash
dotnet run --project Library
```
Ensure your `appsettings.json` has the correct `ConnectionStrings:DefaultConnection` for your SQL server instance, and check your Redis connection settings.

The API will run locally and listen on:
* **HTTP**: http://localhost:5185
* **HTTPS**: https://localhost:7231

### B. Frontend Client (Angular 18 Standalone)
1. **Navigate into the frontend directory:**
```bash
cd library-client
```
2. **Install node dependencies:**
```bash
npm install
```
3. **Start the development server:**
```bash
npm start
```
The Angular client will compile and run locally on:
* **Frontend URL**: http://localhost:4200

> [!NOTE]
> Make sure the backend API URL configuration matches your running backend instance.

## Technical Details 

- **Framework:** ASP.NET Core Web API on .NET 10, and Angular 18 using standalone components and Signals for state management
- **Database:** MS SQL Server 2022 with Full-Text Search (FTS) and database-level localization translation tables
- **Caching:** Redis distributed caching combined with .NET HybridCache for optimized route serialization
- **Background jobs:** Hangfire task engine executing asynchronous calculations for book analytics
- **Media storage:** MinIO S3-compatible object storage integrated via the AWS S3 SDK for cover uploads
- **Localization:** Slovak, English, and Greek localization engine resolved dynamically via Accept-Language headers
- **Tracing:** Correlation ID propagation across the Nginx reverse proxy, Angular client, and backend API, visualized in Seq
- **Deployment:** Multi-container deployment managed as a single ecosystem via Docker Compose
