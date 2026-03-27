---
description: How to setup and run the Vehicle Service Management System
---
# Setup and Run VSMS

This workflow outlines the steps to correctly initialize and run the Vehicle Service Management System ASP.NET Core project.

## Step 1: Restore Dependencies
Make sure all required NuGet packages are restored and up to date for both the Admin and Customer features to work correctly.
// turbo
```powershell
dotnet restore
```

## Step 2: Build the Solution
Verify that the complete project builds without any errors or warnings.
// turbo
```powershell
dotnet build
```

## Step 3: Configure Database Connection
1. Open the file `appsettings.json` located in your project root.
2. Locate the `"ConnectionStrings"` block.
3. Update `"DefaultConnection"` to point exactly to your local SQL Server instance using Windows Authentication (or specific credentials).

## Step 4: Run Database Scripts
You must run the provided SQL scripts from your `/DbScripts` or `VSMS_DB` folder manually in SQL Server Management Studio (SSMS) to ensure all tables (Customer, Vehical, JobCard, etc.) exist before the application runs.

## Step 5: Start the Application
Launch the web application locally. The website will open in your default browser. Notice the console output for the specific localhost port.
// turbo
```powershell
dotnet run
```

## Step 6: Verify Access
Once launched, navigate to:
- **Customer Portal**: `https://localhost:<port>/Customer/Home`
- **Admin Dashboard**: `https://localhost:<port>/Admin/Dashboard`
