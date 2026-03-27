# Vehicle Service Management System (VSMS) 🚗⚙️
*- A fully-featured, ultra-modern ASP.NET Core MVC application designed to manage automotive service centers.*

![VSMS Showcase](https://images.unsplash.com/photo-1486262715619-67b85e0b08d3?ixlib=rb-4.0.3&auto=format&fit=crop&w=1200&q=80) <!-- Replace with actual screenshot later -->

## 🌟 Overview
The **Vehicle Service Management System (VSMS)** is a comprehensive web application built on **ASP.NET Core MVC**. It bridges the gap between automotive service centers and their customers. The system is divided into two distinct, beautifully crafted modules:

1. **The Admin Panel**: A clean, SaaS-style interface for managing customers, vehicles, service job cards, and billing.
2. **The Customer Portal**: An ultra-modern, 3D-animated front-end for users to book service appointments, track their vehicle's status, and manage their profile.

## ✨ Features

### 👨‍🔧 Customer Portal
- **Stunning UI & 3D Parallax Effects**: Built using `Vanilla-Tilt.js` and custom CSS transforms, creating an incredible glassmorphism and 3D floating experience right in the browser.
- **Service Requests**: Easy-to-use booking system for scheduling vehicle repair and maintenance.
- **Live Status Tracking**: Dynamic dashboard that tracks request status (Pending, In Progress, Completed, Delivered) via beautifully color-coded badges.
- **Customer Profiles**: Secure authentication & profile management.
- **Scroll Animations**: Uses `AOS (Animate On Scroll)` library to fluidly reveal content as users navigate.

### 🛡️ Admin Dashboard
- **Comprehensive Analytics**: Dashboard tracking revenue, active jobs, and customer engagement.
- **Vehicle & Customer Management**: Full intuitive CRUD operations directly mapped to the master SQL Database.
- **Job Cards**: Track issues, assign mechanics, and monitor repair status.
- **Billing System**: Invoice generation and comprehensive payment management.

---

## 🛠️ Technology Stack
- **Framework:** ASP.NET Core MVC (.NET 8/7)
- **Language:** C#
- **Database:** Microsoft SQL Server (`System.Data.SqlClient`)
- **Frontend / Styling:** 
  - HTML5 & CSS3 (Custom Glass UI and 3D utilities)
  - **Bootstrap 5.3**
- **Animations / Interactivity:**
  - `AOS` (Animate On Scroll)
  - `Vanilla-Tilt.js` (For 3D hovering and depth-of-field effects)
  - FontAwesome 6 Icons

---

## 🚀 Getting Started

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/)
- [.NET 7.0/8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

### Installation Steps

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/yourusername/VSMS.git
   ```

2. **Open the Project:**
   Open `VSMS.sln` using Visual Studio.

3. **Database Setup:**
   Run the SQL scripts provided in the `/DbScripts` or execute the creation tables manually in SSMS to create objects like `[dbo].[Customer]`, `[dbo].[Vehical]`, etc. Update the standard connection string located in `appsettings.json` to point to your local SQL Server instance:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=VSMS;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Build and Run:**
   - Press `F5` or click **Run** in Visual Studio.
   - Navigate to `/Customer/Home` for the user portal or `/Admin/Dashboard` for the management view.

---

## 🎨 Design Highlights
We took standard UI design completely out of the box with:
- **Off-Axis Layers:** The About page features overlapping 3D glowing layers (`translateZ`) to provide a depth-of-field experience almost never seen inside standard .NET applications.
- **Glassmorphism:** Frosting blur overlays on the navbars and layered container elements.
- **Modern Color Palette:** Utilizing vivid blues `--primary-color: #2563eb` and amber `--accent-color: #f59e0b` to denote hierarchy and action.

---

## 🔄 Website Workflow
The VSMS application follows a clean, intuitive workflow for both customers and administrators.

### Customer Journey
1. **Registration/Login**: Users create an account or log in to the Customer Portal.
2. **Dashboard Overview**: Upon logging in, users are greeted by the 3D-animated Home page featuring the latest offers and services.
3. **Request Service**: Users navigate to the *Request Service* page, fill out their vehicle details, and describe the issue they are facing.
4. **Tracking**: The request instantly appears on the *My Requests* page with a "Pending" status badge. Users can track the status in real-time.
5. **Profile Management**: Users can update their personal information anytime from the interactive *Profile* page.

### Admin Journey
1. **Dashboard Monitoring**: The admin logs in to see an overview of total customers, active jobs, and recent revenue.
2. **Customer & Vehicle Logs**: Admin manages the customer base and their registered vehicles, directly interacting with the `Customer` and `Vehical` tables.
3. **JobCard Management**: When a new service request arrives, the admin creates a *JobCard*, assigns it to a mechanic, and updates the status to "In Progress".
4. **Billing & Invoicing**: Once the job is finished, the admin finalizes the JobCard, generates a bill in the *Billing* module, and the customer sees their status update to "Completed".

---

## 📜 License
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

---
*Developed with ❤️ as part of modern enterprise applications.*
