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
            this.BackColor = UIStyles.LightBackground;
            this.DoubleBuffered = true;

            // Main Layout Container
            TableLayoutPanel mainLayout = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 350F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainLayout);

            // 1. Sidebar Input Panel
            Panel pnlSide = new Panel { 
                Dock = DockStyle.Fill,
                BackColor = Color.White, 
                Padding = new Padding(20),
                BorderStyle = BorderStyle.None
            };
            mainLayout.Controls.Add(pnlSide, 0, 0);
            
            Panel pnlSideScroll = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            Label lblInputTitle = new Label { 
                Text = "APPOINTMENT DETAILS", 
                Font = UIStyles.SubHeaderFont, 
                ForeColor = UIStyles.PrimaryColor, 
                Dock = DockStyle.Top, 
                Height = 40 
            };
            pnlSide.Controls.Add(pnlSideScroll);
            pnlSide.Controls.Add(lblInputTitle);

            TableLayoutPanel tlpInputs = new TableLayoutPanel {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 10,
                Padding = new Padding(0, 10, 20, 10)
            };
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            tlpInputs.Controls.Add(new Label { Text = "Token Number", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true });
            txtToken = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, ReadOnly = true, BackColor = Color.LightYellow, Margin = new Padding(0, 0, 0, 15) };
            tlpInputs.Controls.Add(txtToken);

            tlpInputs.Controls.Add(new Label { Text = "Select Patient", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true });
            cmbPatient = new ComboBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 15) };
            tlpInputs.Controls.Add(cmbPatient);

            tlpInputs.Controls.Add(new Label { Text = "Select Doctor", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true });
            cmbDoctor = new ComboBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 15) };
            tlpInputs.Controls.Add(cmbDoctor);

            tlpInputs.Controls.Add(new Label { Text = "Date & Time", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true });
            dtpDate = new DateTimePicker { Dock = DockStyle.Top, Font = UIStyles.RegularFont, Format = DateTimePickerFormat.Custom, CustomFormat = "MM/dd/yyyy hh:mm tt", Margin = new Padding(0, 0, 0, 15) };
            tlpInputs.Controls.Add(dtpDate);

            tlpInputs.Controls.Add(new Label { Text = "Status", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true });
            cmbStatus = new ComboBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 15) };
            cmbStatus.Items.AddRange(new string[] { "Pending", "Confirmed", "Cancelled", "Completed" });
            cmbStatus.SelectedIndex = 0;
            tlpInputs.Controls.Add(cmbStatus);

            pnlSideScroll.Controls.Add(tlpInputs);

            // Action Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 10, 0, 0) };
            btnBook = CreateActionButton("Book Appt", UIStyles.AccentColor);
            btnUpdate = CreateActionButton("Update", Color.Orange);
            btnCancel = CreateActionButton("Delete", UIStyles.DangerColor);
            
            flpButtons.Controls.AddRange(new Control[] { btnBook, btnUpdate, btnCancel });
            pnlSideScroll.Controls.Add(flpButtons);

            btnClear = new Button { 
                Text = "Clear Fields", 
                Dock = DockStyle.Top, 
                Height = 35, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont,
                ForeColor = UIStyles.TextSecondary
            };
            btnClear.FlatAppearance.BorderSize = 0;
            pnlSideScroll.Controls.Add(btnClear);

            // 2. Main Grid Area
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            mainLayout.Controls.Add(pnlMain, 1, 0);
            
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 60 };
            Label lblSearch = new Label { Text = "SEARCH APPOINTMENTS", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 20 };
            txtSearch = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont };
            txtSearch.TextChanged += (s, e) => LoadAppointments(txtSearch.Text.Trim());
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(lblSearch);

            dgvAppointments = new DataGridView { 
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 20, 0, 0)
            };
            UIStyles.ApplyModernStyle(dgvAppointments);
            dgvAppointments.CellClick += DgvAppointments_CellClick;

            pnlMain.Controls.Add(dgvAppointments);
            pnlMain.Controls.Add(pnlSearch);

            btnBook.Click += BtnBook_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnCancel.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearFields();
        }

        private Button CreateActionButton(string text, Color color)
        {
            Button btn = new Button { 
                Text = text, 
                Size = new Size(95, 38), 
                BackColor = color, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont 
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
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
