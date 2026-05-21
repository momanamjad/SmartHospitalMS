/* 
  HOSPITAL DATABASE SETUP
  
  HOW TO RUN ON A NEW MACHINE (PowerShell):
  1. Create the database:
     sqllocaldb start MSSQLLocalDB
     sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE HospitalDB"
  
  2. Create tables and seed data:
     sqlcmd -S "(localdb)\MSSQLLocalDB" -d HospitalDB -i DatabaseSetup.sql
  
  3. Update passwords to secure hashes:
     sqlcmd -S "(localdb)\MSSQLLocalDB" -d HospitalDB -i SecurityUpdate.sql
*/

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'HospitalDB')
BEGIN
    CREATE DATABASE HospitalDB;
END
GO

USE HospitalDB;
GO

-- 1. Users Table (Authentication)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL, -- SHA-256 Hash
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Doctor', 'Receptionist')),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 2. Patients Table
CREATE TABLE Patients (
    PatientID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    Age INT NOT NULL,
    BloodGroup NVARCHAR(5),
    Disease NVARCHAR(200),
    Contact NVARCHAR(20) NOT NULL,
    Address NVARCHAR(MAX),
    DoctorAssigned NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 3. Doctors Table
CREATE TABLE Doctors (
    DoctorID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Specialization NVARCHAR(100) NOT NULL,
    Contact NVARCHAR(20) NOT NULL,
    Email NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- 4. Appointments Table
CREATE TABLE Appointments (
    AppointmentID INT PRIMARY KEY IDENTITY(1,1),
    TokenNumber NVARCHAR(20) UNIQUE NOT NULL, -- e.g., APT-0001
    PatientID INT NOT NULL,
    DoctorID INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (PatientID) REFERENCES Patients(PatientID) ON DELETE CASCADE,
    FOREIGN KEY (DoctorID) REFERENCES Doctors(DoctorID) ON DELETE CASCADE
);

-- 5. Bills Table
CREATE TABLE Bills (
    BillID INT PRIMARY KEY IDENTITY(1,1),
    AppointmentID INT NOT NULL,
    ConsultationFee DECIMAL(18, 2) NOT NULL,
    MedicineFee DECIMAL(18, 2) DEFAULT 0,
    LabFee DECIMAL(18, 2) DEFAULT 0,
    TaxPercentage DECIMAL(5, 2) DEFAULT 5.0,
    TotalAmount AS (ConsultationFee + MedicineFee + LabFee) * (1 + TaxPercentage/100),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AppointmentID) REFERENCES Appointments(AppointmentID) ON DELETE CASCADE
);

-- Insert Test Users (Passwords are 'password123' hashed with SHA-256 for later use)
-- For now, we will use plain text to test and update to hashing in Module 7
INSERT INTO Users (Username, PasswordHash, Role) VALUES 
('admin', 'password123', 'Admin'),
('doctor', 'password123', 'Doctor'),
('reception', 'password123', 'Receptionist');
