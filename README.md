# Smart Hospital Management System 🏥

A professional-grade C# Windows Forms application for managing hospital operations, including patient records, appointments, billing, and real-time reporting.

## 🚀 Getting Started on a New Machine

Follow these steps to set up the project from scratch.

### 1. Prerequisites
You must have the following installed:
*   **Visual Studio 2022 Community** (with the ".NET desktop development" workload).
*   **.NET 6.0 SDK** or higher.
*   **SQL Server Express LocalDB** (included with Visual Studio).

### 2. Database Setup (Crucial)
The application uses a local SQL database. You must create it before the app will run:
1.  Open **Visual Studio**.
2.  Go to `View` -> `SQL Server Object Explorer`.
3.  Expand `(localdb)\MSSQLLocalDB`.
4.  Right-click on **Databases** -> **Add New Database** -> Name it `HospitalDB`.
5.  Right-click on the new `HospitalDB` and select **New Query**.
6.  Open the file `SmartHospitalMS/DatabaseSetup.sql` from this project, copy its content, and paste it into the query window.
7.  Press **Execute** (the green play button).
8.  **Security Note:** Open `SmartHospitalMS/SecurityUpdate.sql`, copy its content, and run it as a new query to hash the default test passwords.

### 3. Build & Run
1.  Open the terminal in the project root folder.
2.  Restore dependencies:
    ```bash
    dotnet restore
    ```
3.  Run the application:
    ```bash
    dotnet run --project SmartHospitalMS/SmartHospitalMS.csproj
    ```

---

## 🔐 Default Login Credentials

Use these to test the system (Password for all is `password123`):

| Username | Role | Access Level |
| :--- | :--- | :--- |
| **admin** | Admin | Full System Access |
| **doctor** | Doctor | Patients & Appointments |
| **reception** | Receptionist | Appointments & Billing |

---

## ✨ Key Features
*   **Secure Authentication:** SHA-256 password hashing and SQL Injection prevention.
*   **Patient CRUD:** Complete management with live LINQ-based search.
*   **Smart Appointments:** Auto-token generation (`APT-0001`) and duplicate booking prevention.
*   **Billing System:** Automated fee calculation and invoice export to `.txt`.
*   **Interactive Dashboard:** Custom GDI+ bar charts and real-time clock.
*   **Modern UI:** Dark/Light theme toggle and role-based dynamic menus.

## 🛠️ Technical Stack
*   **Language:** C# 10+
*   **Framework:** Windows Forms (.NET 6.0+)
*   **Database:** ADO.NET with SQL Server LocalDB
*   **Architecture:** Object-Oriented Programming (Inheritance, Encapsulation, Polymorphism)
*   **Version Control:** Git
