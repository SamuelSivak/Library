<div align="center">

<!-- Place the path to the generated Library logo image here (e.g., if saved in the 'assets' folder) -->
<img src="assets/library-logo.png" alt="Library Logo" width="400">

# Library Management System

**Library is a project focused on creating a book management system, featuring Angular and .NET technologies.**

![C#](https://img.shields.io/badge/C%23-passing-brightgreen?logo=csharp)
![.NET](https://img.shields.io/badge/.NET-passing-brightgreen?logo=dotnet)
![Angular](https://img.shields.io/badge/Angular-passing-brightgreen?logo=angular)
![SQL](https://img.shields.io/badge/SQL-passing-brightgreen?logo=database)

---

</div>


# What the Library System Does

Traditional cataloging software is often slow, packaged as resource-heavy monoliths, and struggles to scale under load. Retrieving thousands of books alongside their reviews can quickly saturate databases and degrade response times.

Our Library System provides a single, cohesive ecosystem to manage catalogs, authors, and reader feedback with sub-second latencies.

It leverages a decoupled architecture where the Angular client renders a fluid user experience while the .NET Web API handles optimized data processing, offloading statistics to background workers, and caching intensive database lookups.

> [!NOTE]
> The entire environment runs out of the box. Docker Compose automatically provisions and interconnects all layers, including object storage, proxy routing, and distributed cache.

---

## Application Modules

The application is split into specialized views tailored for both library patrons and system administrators.

| Module | What it covers |
| :--- | :--- |
| **Book Catalog** | Dynamic searching, filtering by genre, and multi-criteria sorting (popularity, overall rating, published date, and alphabetical order) |
| **Author Profiles** | Showcases author details and national origins, complete with dynamic localized translations |
| **User Reviews** | Interactive rating submission and review management directly linked to user profiles |
| **Admin Control Panel** | A secure dashboard for library managers to add, update, and remove books, featuring drag-and-drop cover image uploading |

> [!NOTE]
> All text fields, book titles, descriptions, genres, and author information are fully localized in Slovak, English, and Greek, resolved dynamically at the database query level.

---

## Technical Optimizations

The system employs targeted back-end and database-level optimizations to maintain performance even under high concurrent traffic.

| Optimization | How it works |
| :--- | :--- |
| **Query Routing** | Uses ID-based `UNION` search queries in MS SQL Server to completely avoid sorting large description text columns |
| **Hybrid Caching** | Integrates Redis and local .NET HybridCache to cache book lists and reviewer entities, reducing database hits to zero for frequent routes |
| **Asynchronous Jobs** | Enqueues heavy rating and review analytics to a Hangfire background queue, keeping API requests responsive |
| **Object Storage** | Offloads static cover image assets from the database and local filesystem to MinIO S3-compatible cloud storage |

---

## Infrastructure Tools

Built-in services for administration, API exploration, and diagnostic monitoring.

| Tool | What it does |
| :--- | :--- |
| **Swagger UI** | Interactive API playground to test REST endpoints and security authorization headers |
| **Seq Console** | Centralized structured log viewer with real-world Correlation ID tracing across proxy, client, and API |
| **MinIO Console** | Object storage dashboard to manage, inspect, and configure S3 buckets and cover images |
| **Nginx Reverse Proxy** | Acts as the single entry-point for the host, resolving CORS and routing client/API traffic on port 80 |


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

> [!NOTE]
> Ensure your `appsettings.json` has the correct `ConnectionStrings:DefaultConnection` for your SQL server instance, and check your Redis connection settings.

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

---

## Technical Details 

- **Framework:** ASP.NET Core Web API on .NET 10, and Angular 18 using standalone components and Signals for state management
- **Database:** MS SQL Server 2022 with Full-Text Search (FTS) and database-level localization translation tables
- **Caching:** Redis distributed caching combined with .NET HybridCache for optimized route serialization
- **Background jobs:** Hangfire task engine executing asynchronous calculations for book analytics
- **Media storage:** MinIO S3-compatible object storage integrated via the AWS S3 SDK for cover uploads
- **Localization:** Slovak, English, and Greek localization engine resolved dynamically via Accept-Language headers
- **Tracing:** Correlation ID propagation across the Nginx reverse proxy, Angular client, and backend API, visualized in Seq
- **Deployment:** Multi-container deployment managed as a single ecosystem via Docker Compose