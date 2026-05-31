# 🚀 Smart Hospital Management System - Quick Start Guide

This guide is designed for **everyone**. Whether you are a pro coder or have never seen a terminal before, follow these steps in order to get the hospital system running on your computer.

---

## 📥 Step 1: Install the Requirements

You need 3 main things installed. If you don't have them, download them here:

1.  **Visual Studio 2022 (Community Edition)** - [Download Here](https://visualstudio.microsoft.com/downloads/)
    *   **IMPORTANT:** When installing, you will see a list of "Workloads". Check the box that says **".NET desktop development"**.
2.  **dotnet 10 SDK** - [Download Here](https://dotnet.microsoft.com/download/dotnet/10.0)
3.  **SQL Server LocalDB** (This is usually included with Visual Studio).

---

## 🛠 Step 2: Setting up the Database (The "Brain")

The application needs a database to store patient and doctor information. You have two ways to set this up:

### Method A: The Easy Way (Using Visual Studio)
1.  Open Visual Studio.
2.  Click **"Open a project or solution"** and select the `SmartHospitalMS.sln` file in this folder.
3.  Go to the top menu: **View -> SQL Server Object Explorer**.
4.  Expand `(localdb)\MSSQLLocalDB`.
5.  Right-click on **Databases** -> **Add New Database**. Name it `HospitalDB`.
6.  Right-click on your new `HospitalDB` and select **"New Query"**.
7.  Open the file `SmartHospitalMS/DatabaseSetup.sql` in a text editor (like Notepad), copy everything, paste it into the Visual Studio query window, and press the **Execute** button (the green play arrow).
8.  Do the same for `SmartHospitalMS/SecurityUpdate.sql`.

### Method B: The Terminal Way (Using Commands)
1. Open PowerShell **in the main folder** (where the `.sln` file is).
2. Copy-paste these one by one:
```powershell
# 1. Start the engine
sqllocaldb start MSSQLLocalDB

# 2. Create the database
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HospitalDB') CREATE DATABASE HospitalDB"

# 3. Create tables
sqlcmd -S "(localdb)\MSSQLLocalDB" -d HospitalDB -i SmartHospitalMS/DatabaseSetup.sql

# 4. Setup secure passwords
sqlcmd -S "(localdb)\MSSQLLocalDB" -d HospitalDB -i SmartHospitalMS/SecurityUpdate.sql
```

*Note: If you are already inside the `SmartHospitalMS` folder, remove the `SmartHospitalMS/` part from the file paths in steps 3 and 4.*

---
## 🏃 Step 3: Running the App

### The "One Click" Way
1.  With the project open in Visual Studio, simply press the **F5** key on your keyboard or click the green **"Start"** button at the top.

### The Terminal Way
1.  Open your terminal in the project folder.
2.  Type this and press Enter:
    ```powershell
    dotnet run --project SmartHospitalMS/SmartHospitalMS.csproj
    ```

---

## 🔑 Step 4: First Time Login

Once the app opens, use these "Test Accounts" to explore:

| Username | Password | Role |
| :--- | :--- | :--- |
| **admin** | `password123` | Can see everything. |
| **doctor** | `password123` | Can see patients and appointments. |
| **reception** | `password123` | Can book appointments and handle billing. |

---

## 🆘 Troubleshooting (If things go wrong)

### ❓ "I get a 'sqlcmd' not recognized error"
*   **Fix:** This means the SQL tools aren't in your computer's "Path". Use **Method A (The Easy Way)** above instead, or download the "SQL Command Line Tools" from Microsoft.

### ❓ "The app says 'Connection Failed' or 'Named Pipes Error'"
*   **Fix:** This means the database engine isn't awake. Open your terminal and type:
    `sqllocaldb start MSSQLLocalDB`
    If that doesn't work, try:
    `sqllocaldb create MSSQLLocalDB`

### ❓ "Database 'HospitalDB' already exists"
*   **Fix:** You already did the setup! You can skip Step 2 and just run the app.

### ❓ "I forgot the passwords"
*   **Fix:** All default accounts use `password123`. If you need to reset them, run the `SecurityUpdate.sql` script again.

---

## 📚 Want to learn how the code works?
Check out the [**ABOUT_PROJECT.md**](./ABOUT_PROJECT.md) file for a full breakdown of every file and function!
