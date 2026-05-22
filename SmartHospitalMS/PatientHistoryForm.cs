using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace SmartHospitalMS
{
    public class PatientHistoryForm : Form
    {
        private int _patientID;
        private string _patientName;
        private DataGridView dgvAppointments, dgvBills;

        public PatientHistoryForm(int patientID, string patientName)
        {
            _patientID = patientID;
            _patientName = patientName;
            SetupUI();
            LoadHistory();
        }

        private void SetupUI()
        {
            this.Text = $"History: {_patientName}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblTitle = new Label { 
                Text = $"Medical & Billing History for {_patientName}", 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                Location = new Point(20, 20), 
                AutoSize = true,
                ForeColor = Color.SteelBlue
            };

            Label lblAppts = new Label { Text = "Appointment History:", Location = new Point(20, 60), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            dgvAppointments = new DataGridView { 
                Location = new Point(20, 85), 
                Size = new Size(840, 200), 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                RowHeadersVisible = false
            };

            Label lblBills = new Label { Text = "Billing History:", Location = new Point(20, 300), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            dgvBills = new DataGridView { 
                Location = new Point(20, 325), 
                Size = new Size(840, 200), 
                ReadOnly = true, 
                AllowUserToAddRows = false, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.WhiteSmoke,
                RowHeadersVisible = false
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblAppts, dgvAppointments, lblBills, dgvBills });
        }

        private void LoadHistory()
        {
            try {
                // Load Appointments
                string apptQuery = @"
                    SELECT TokenNumber, AppointmentDate, Status 
                    FROM Appointments 
                    WHERE PatientID = @pid 
                    ORDER BY AppointmentDate DESC";
                dgvAppointments.DataSource = DatabaseHelper.ExecuteQuery(apptQuery, new SqlParameter[] { new SqlParameter("@pid", _patientID) });

                // Load Bills
                string billQuery = @"
                    SELECT b.BillID, b.ConsultationFee, b.MedicineFee, b.LabFee, b.TotalAmount, b.CreatedAt as BillDate
                    FROM Bills b
                    JOIN Appointments a ON b.AppointmentID = a.AppointmentID
                    WHERE a.PatientID = @pid
                    ORDER BY b.CreatedAt DESC";
                dgvBills.DataSource = DatabaseHelper.ExecuteQuery(billQuery, new SqlParameter[] { new SqlParameter("@pid", _patientID) });

            } catch (Exception ex) {
                MessageBox.Show("Error loading history: " + ex.Message);
            }
        }
    }
}
