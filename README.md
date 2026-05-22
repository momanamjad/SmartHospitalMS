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
*   **Patient CRM:** Complete CRUD, live LINQ search, and **Detailed Patient History** (Medical & Billing).
*   **Smart Appointments:** Auto-token generation (`APT-0001`) and duplicate booking prevention.
*   **Billing System:** Automated fee calculation and invoice export to `.txt`.
*   **Interactive Dashboard:** Custom GDI+ bar charts, real-time clock, and **Asynchronous Data Loading** (Multi-threading).
*   **Modern UI:** Dark/Light theme toggle and role-based dynamic menus.

## 🛠️ Technical Stack
*   **Language:** C# 10+
*   **Framework:** Windows Forms (.NET 6.0+)
*   **Database:** ADO.NET with SQL Server LocalDB
*   **Architecture:** Clean OOP with **Inheritance, Encapsulation, and Polymorphism** (Method Overriding).
*   **Performance:** Multi-threaded background tasks for database operations.
*   **Version Control:** Git


When you click Start / Build in Visual Studio, the computer processes files in this exact sequence to turn raw text into a working software application:

SmartHospitalMS.csproj ➔ The computer opens this first to see what compiler rules to use.

App.config ➔ The build engine checks this configuration file to see if any build variables are set

.Models/BaseEntity.cs ➔ Built first because all other data models rely on its structural blueprint.

Models/Models.cs ➔ Built next to create the shapes of your Patients, Doctors, and Bills.

SecurityHelper.cs & DatabaseHelper.cs ➔ The background engine files are compiled next.

Session.cs ➔ The state manager is compiled.

All UI Forms (LoginForm.cs, Dashboard.cs, etc.) ➔ The visual buttons and screens are compiled 

last.Program.cs ➔ Tied together at the very end to seal the application execution package



.Phase 2: Runtime Execution (When a User Double-Clicks the App)When a receptionist or doctor actually launches the application on their computer, the execution flows strictly from start to end down this chain:[START]
   │
   ▼
1. Program.cs ──────────► The absolute entry point. It wakes up the system.
   │
   ▼
2. App.config ──────────► Read immediately to find the connection string to the SQL database.
   │
   ▼
3. LoginForm.cs ────────► The first screen visible to the user.
   │
   ├──► Calls SecurityHelper.cs (to verify the encrypted password string)
   └──► Calls DatabaseHelper.cs (to verify credentials against tables from DatabaseSetup.sql)
   │
   ▼
4. Session.cs ──────────► Saves who successfully logged in (e.g., Role = "Receptionist").
   │
   ▼
5. Dashboard.cs ────────► The primary navigation hub opens; Login screen closes.
   │
   ├──► User clicks "Register Patient" ──► PatientForm.cs opens ──► Saves to Patient model.
   ├──► User clicks "Book Checkup"    ──► AppointmentForm.cs   ──► Saves to Appointment model.
   └──► User clicks "View Records"    ──► PatientHistoryForm.cs
   │
   ▼
6. BillingForm.cs ──────► Opened at checkout. It runs the math formula stored in Bill (Models.cs).
   │
   ▼
7. Invoice_2026...txt ──► The absolute final step. The app prints this physical text receipt.
   │
   ▼
[END]