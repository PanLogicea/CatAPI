# CatAPI# CatAPI

## Description
CatApi is an ASP.NET Core Web API that fetches cat images from the Cats as a Service (CaaS) API and stores them in a SQL Server. 
The API provides endpoints to fetch all cat images, get a single cat image by ID, 
get cat images with pagination, and retrieve cats with a specific tag with paging support.

## Prerequisites
- .NET 8 SDK
- SQL Server or any other supported database
- Entity Framework tools - .NET Core CLI (If not installed, follow the steps here: https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

## Setup

### 1. Clone the repository
### 2. Configure the database
Update the `appsettings.json` file with your database connection string.
### 3. Apply Migrations 
Run the following command to apply the database migrations:

dotnet ef database update

## Build and Run

### 1. Build the application
### 2. Run the application
The application should now be running at `https://localhost:5001` or `http://localhost:5000`.

## Testing
To run the tests, use the following command:
