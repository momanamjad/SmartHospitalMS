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
            this.Text = $"Medical History: {_patientName}";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = UIStyles.LightBackground;
            this.DoubleBuffered = true;

            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(20) };
            Label lblTitle = new Label { 
                Text = $"HISTORY: {_patientName.ToUpper()}", 
                Font = UIStyles.SubHeaderFont, 
                ForeColor = UIStyles.PrimaryColor, 
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            Panel pnlContent = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };

            TableLayoutPanel tlp = new TableLayoutPanel { 
                Dock = DockStyle.Fill, 
                ColumnCount = 1, 
                RowCount = 4 
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            Label lblAppts = new Label { Text = "APPOINTMENT RECORD", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Bottom };
            dgvAppointments = new DataGridView { Dock = DockStyle.Fill };
            UIStyles.ApplyModernStyle(dgvAppointments);

            Label lblBills = new Label { Text = "BILLING RECORD", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Bottom };
            dgvBills = new DataGridView { Dock = DockStyle.Fill };
            UIStyles.ApplyModernStyle(dgvBills);

            tlp.Controls.Add(lblAppts, 0, 0);
            tlp.Controls.Add(dgvAppointments, 0, 1);
            tlp.Controls.Add(lblBills, 0, 2);
            tlp.Controls.Add(dgvBills, 0, 3);

            pnlContent.Controls.Add(tlp);

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
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
