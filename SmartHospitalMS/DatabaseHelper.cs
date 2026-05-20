using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SmartHospitalMS
{
    /// <summary>
    /// DatabaseHelper acts like your 'db.js' or 'knexfile.js' in JavaScript.
    /// It handles connecting to the database and executing commands.
    /// </summary>
    public static class DatabaseHelper
    {
        // Connection string read from App.config (Requirement: Module 7)
        private static string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["HospitalDB"].ConnectionString;

        /// <summary>
        /// Gets a new open connection to the database.
        /// </summary>
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(connectionString);
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }

        /// <summary>
        /// Executes a non-query command (INSERT, UPDATE, DELETE).
        /// Returns the number of rows affected.
        /// </summary>
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);
                        
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // In a real app, we'd log this. For now, we throw it to be caught by the UI.
                throw new Exception("Database Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes a query and returns a DataTable (like an array of objects in JS).
        /// Used for filling DataGridViews.
        /// </summary>
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes a query and returns the first column of the first row (e.g., a COUNT or a single ID).
        /// JS Analogy: Like result[0].id
        /// </summary>
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (parameters != null)
                            cmd.Parameters.AddRange(parameters);

                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Database Error: " + ex.Message);
            }
        }
    }
}
