using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SmartHospitalMS
{
    public partial class AppointmentForm : Form
    {
        private DataGridView dgvAppointments;
        private ComboBox cmbPatient, cmbDoctor, cmbStatus;
        private DateTimePicker dtpDate;
        private TextBox txtSearch, txtToken;
        private Button btnBook, btnUpdate, btnCancel, btnClear;
        private int selectedAppointmentID = 0;

        public AppointmentForm()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
            GenerateToken();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "AppointmentForm";
            this.Text = "Appointment Management";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;

            // 1. Input Panel (LEFT)
            Panel pnlInputs = new Panel { 
                Location = new Point(0, 0),
                Size = new Size(320, 700),
                BackColor = Color.WhiteSmoke, 
                Padding = new Padding(15),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            int y = 20;
            
            // Token (Read-only)
            pnlInputs.Controls.Add(new Label { Text = "Token Number:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtToken = new TextBox { Location = new Point(130, y), Size = new Size(160, 25), ReadOnly = true, BackColor = Color.LightYellow };
            pnlInputs.Controls.Add(txtToken);
            y += 40;

            // Patient Dropdown
            pnlInputs.Controls.Add(new Label { Text = "Select Patient:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            cmbPatient = new ComboBox { Location = new Point(130, y), Size = new Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInputs.Controls.Add(cmbPatient);
            y += 40;

            // Doctor Dropdown
            pnlInputs.Controls.Add(new Label { Text = "Select Doctor:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            cmbDoctor = new ComboBox { Location = new Point(130, y), Size = new Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            pnlInputs.Controls.Add(cmbDoctor);
            y += 40;

            // Date Picker
            pnlInputs.Controls.Add(new Label { Text = "Date & Time:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            dtpDate = new DateTimePicker { Location = new Point(130, y), Size = new Size(160, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt" };
            pnlInputs.Controls.Add(dtpDate);
            y += 40;

            // Status
            pnlInputs.Controls.Add(new Label { Text = "Status:", Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            cmbStatus = new ComboBox { Location = new Point(130, y), Size = new Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStatus.Items.AddRange(new string[] { "Pending", "Confirmed", "Cancelled", "Completed" });
            cmbStatus.SelectedIndex = 0;
            pnlInputs.Controls.Add(cmbStatus);
            y += 50;

            btnBook = new Button { Text = "Book Appt", Location = new Point(15, y), Size = new Size(90, 40), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnUpdate = new Button { Text = "Update", Location = new Point(110, y), Size = new Size(90, 40), BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnCancel = new Button { Text = "Delete", Location = new Point(205, y), Size = new Size(90, 40), BackColor = Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            
            y += 50;
            btnClear = new Button { Text = "Clear Fields", Location = new Point(15, y), Size = new Size(280, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };

            btnBook.Click += BtnBook_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnCancel.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearFields();

            pnlInputs.Controls.AddRange(new Control[] { btnBook, btnUpdate, btnCancel, btnClear });
            this.Controls.Add(pnlInputs);

            // 2. Grid Area (RIGHT) - FIXED LAYOUT
            // Requirement: Grid must start after the input panel (x=340) and not overlap
            Label lblSearch = new Label { Text = "Search Appointments:", Location = new Point(340, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtSearch = new TextBox { Location = new Point(500, 18), Size = new Size(400, 25) };
            txtSearch.TextChanged += (s, e) => LoadAppointments(txtSearch.Text.Trim());

            dgvAppointments = new DataGridView { 
                Location = new Point(340, 60), 
                Size = new Size(820, 580), 
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            dgvAppointments.CellClick += DgvAppointments_CellClick;

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(dgvAppointments);
        }

        private void LoadData()
        {
            try {
                // Seed Doctors if empty
                int doctorCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Doctors"));
                if (doctorCount == 0) {
                    DatabaseHelper.ExecuteNonQuery("INSERT INTO Doctors (FullName, Specialization, Contact, Email) VALUES ('Dr. Smith', 'General', '03001234567', 'smith@test.com')");
                }

                // Load Patients into ComboBox
                DataTable dtPatients = DatabaseHelper.ExecuteQuery("SELECT PatientID, FullName FROM Patients");
                cmbPatient.DataSource = dtPatients;
                cmbPatient.DisplayMember = "FullName";
                cmbPatient.ValueMember = "PatientID";
                cmbPatient.SelectedIndex = -1;

                // Load Doctors into ComboBox
                DataTable dtDoctors = DatabaseHelper.ExecuteQuery("SELECT DoctorID, FullName FROM Doctors");
                cmbDoctor.DataSource = dtDoctors;
                cmbDoctor.DisplayMember = "FullName";
                cmbDoctor.ValueMember = "DoctorID";
                cmbDoctor.SelectedIndex = -1;

                LoadAppointments();
            } catch (Exception ex) {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void LoadAppointments(string search = "")
        {
            try {
                string query = @"
                    SELECT a.TokenNumber, p.FullName as PatientName, d.FullName as DoctorName, 
                           a.AppointmentDate, a.Status, a.AppointmentID, a.PatientID, a.DoctorID
                    FROM Appointments a
                    JOIN Patients p ON a.PatientID = p.PatientID
                    JOIN Doctors d ON a.DoctorID = d.DoctorID";

                SqlParameter[] parameters = null;
                if (!string.IsNullOrEmpty(search)) {
                    query += " WHERE p.FullName LIKE @search OR d.FullName LIKE @search OR a.TokenNumber LIKE @search";
                    parameters = new SqlParameter[] { new SqlParameter("@search", "%" + search + "%") };
                }
                
                query += " ORDER BY a.AppointmentDate DESC";

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);
                dgvAppointments.DataSource = null;
                dgvAppointments.DataSource = dt;

                if (dgvAppointments.Columns.Contains("AppointmentID")) dgvAppointments.Columns["AppointmentID"].Visible = false;
                if (dgvAppointments.Columns.Contains("PatientID")) dgvAppointments.Columns["PatientID"].Visible = false;
                if (dgvAppointments.Columns.Contains("DoctorID")) dgvAppointments.Columns["DoctorID"].Visible = false;
                
                // Ensure Token is first
                if (dgvAppointments.Columns.Contains("TokenNumber"))
                {
                    dgvAppointments.Columns["TokenNumber"].DisplayIndex = 0;
                    dgvAppointments.Columns["TokenNumber"].HeaderText = "Token #";
                }
            } catch (Exception ex) {
                MessageBox.Show("Error loading appointments: " + ex.Message);
            }
        }

        private void GenerateToken()
        {
            try {
                object result = DatabaseHelper.ExecuteScalar("SELECT MAX(AppointmentID) FROM Appointments");
                int nextID = (result == DBNull.Value) ? 1 : Convert.ToInt32(result) + 1;
                txtToken.Text = "APT-" + nextID.ToString("D4");
            } catch {
                txtToken.Text = "APT-0001";
            }
        }

        private void BtnBook_Click(object sender, EventArgs e)
        {
            if (cmbPatient.SelectedIndex == -1 || cmbDoctor.SelectedIndex == -1) {
                MessageBox.Show("Please select both Patient and Doctor.");
                return;
            }

            if (dtpDate.Value < DateTime.Now) {
                MessageBox.Show("Cannot book an appointment in the past!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try {
                // Block duplicate
                string checkQuery = "SELECT COUNT(*) FROM Appointments WHERE PatientID=@pid AND DoctorID=@did AND CAST(AppointmentDate AS DATE) = @date";
                SqlParameter[] checkParams = {
                    new SqlParameter("@pid", cmbPatient.SelectedValue),
                    new SqlParameter("@did", cmbDoctor.SelectedValue),
                    new SqlParameter("@date", dtpDate.Value.Date)
                };

                if (Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkQuery, checkParams)) > 0) {
                    MessageBox.Show("Duplicate appointment detected for this patient/doctor on this date!");
                    return;
                }

                string query = @"INSERT INTO Appointments (TokenNumber, PatientID, DoctorID, AppointmentDate, Status) 
                                VALUES (@token, @pid, @did, @date, @status)";
                
                SqlParameter[] parameters = {
                    new SqlParameter("@token", txtToken.Text),
                    new SqlParameter("@pid", cmbPatient.SelectedValue),
                    new SqlParameter("@did", cmbDoctor.SelectedValue),
                    new SqlParameter("@date", dtpDate.Value),
                    new SqlParameter("@status", cmbStatus.SelectedItem.ToString())
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Booked! Token: " + txtToken.Text);
                ClearFields();
                LoadAppointments();
                GenerateToken();
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedAppointmentID == 0) { MessageBox.Show("Select an appointment."); return; }

            try {
                string query = "UPDATE Appointments SET Status=@status, AppointmentDate=@date WHERE AppointmentID=@id";
                SqlParameter[] parameters = {
                    new SqlParameter("@status", cmbStatus.SelectedItem.ToString()),
                    new SqlParameter("@date", dtpDate.Value),
                    new SqlParameter("@id", selectedAppointmentID)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Updated!");
                LoadAppointments();
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedAppointmentID == 0) { MessageBox.Show("Select an appointment."); return; }

            if (MessageBox.Show("Delete this appointment?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                try {
                    string query = "DELETE FROM Appointments WHERE AppointmentID=@id";
                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@id", selectedAppointmentID) });
                    MessageBox.Show("Deleted.");
                    ClearFields();
                    LoadAppointments();
                } catch (Exception ex) {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void DgvAppointments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) {
                DataGridViewRow row = dgvAppointments.Rows[e.RowIndex];
                selectedAppointmentID = Convert.ToInt32(row.Cells["AppointmentID"].Value);
                txtToken.Text = row.Cells["TokenNumber"].Value.ToString();
                cmbPatient.SelectedValue = row.Cells["PatientID"].Value;
                cmbDoctor.SelectedValue = row.Cells["DoctorID"].Value;
                dtpDate.Value = Convert.ToDateTime(row.Cells["AppointmentDate"].Value);
                cmbStatus.SelectedItem = row.Cells["Status"].Value.ToString();
            }
        }

        private void ClearFields()
        {
            selectedAppointmentID = 0;
            cmbPatient.SelectedIndex = -1;
            cmbDoctor.SelectedIndex = -1;
            cmbStatus.SelectedIndex = 0;
            dtpDate.Value = DateTime.Now;
            GenerateToken();
        }
    }
}
