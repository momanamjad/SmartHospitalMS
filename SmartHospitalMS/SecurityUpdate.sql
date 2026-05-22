/* 
  SECURITY UPDATE: HASH EXISTING PASSWORDS
  Run this script in SQL Server to update your test users
  so they can log in with the new SHA-256 security system.
*/

USE HospitalDB;
GO

-- Update all test users to have the hashed version of 'password123'
UPDATE Users 
SET PasswordHash = 'ef92b778ba7157a82aa097fe7761fa02274a3e7614bb47434527712ddec35d4f'
WHERE PasswordHash = 'password123';
GO
  
SELECT * FROM Users;
