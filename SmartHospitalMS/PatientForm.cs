using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace SmartHospitalMS
{
    public partial class PatientForm : Form
    {
        private DataGridView dgvPatients;
        private TextBox txtSearch;
        private TextBox txtFullName, txtAge, txtContact, txtAddress, txtDisease, txtDoctor;
        private ComboBox cmbGender, cmbBloodGroup;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private int selectedPatientID = 0;

        public PatientForm()
        {
            InitializeComponent();
            SetupUI();
            LoadPatients();
            PerformOneTimeCleanup(); // Requirement 3: Cleanup existing duplicates
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "PatientForm";
            this.Text = "Patient Management";
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
                Text = "PATIENT DETAILS", 
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
                RowCount = 16,
                Padding = new Padding(0, 10, 20, 10)
            };
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            txtFullName = CreateInputModern(tlpInputs, "Full Name");
            cmbGender = CreateComboBoxModern(tlpInputs, "Gender", new string[] { "Male", "Female", "Other" });
            txtAge = CreateInputModern(tlpInputs, "Age");
            cmbBloodGroup = CreateComboBoxModern(tlpInputs, "Blood Group", new string[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" });
            txtDisease = CreateInputModern(tlpInputs, "Disease");
            txtContact = CreateInputModern(tlpInputs, "Contact (11 Digits)");
            txtAddress = CreateInputModern(tlpInputs, "Address");
            txtDoctor = CreateInputModern(tlpInputs, "Assigned Doctor");

            pnlSideScroll.Controls.Add(tlpInputs);

            // Action Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 100, Padding = new Padding(0, 10, 0, 0) };
            btnAdd = CreateActionButton("Add New", UIStyles.AccentColor);
            btnUpdate = CreateActionButton("Update", Color.Orange);
            btnDelete = CreateActionButton("Delete", UIStyles.DangerColor);
            
            flpButtons.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete });
            pnlSideScroll.Controls.Add(flpButtons);

            Button btnHistory = new Button { 
                Text = "VIEW PATIENT HISTORY", 
                Dock = DockStyle.Top, 
                Height = 45, 
                BackColor = UIStyles.PrimaryColor, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont 
            };
            btnHistory.FlatAppearance.BorderSize = 0;
            
            btnClear = new Button { 
                Text = "Clear All Fields", 
                Dock = DockStyle.Top, 
                Height = 35, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont,
                ForeColor = UIStyles.TextSecondary
            };
            btnClear.FlatAppearance.BorderSize = 0;

            pnlSideScroll.Controls.Add(btnClear);
            pnlSideScroll.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10 }); // Spacer
            pnlSideScroll.Controls.Add(btnHistory);

            // 2. Main Grid Area
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            mainLayout.Controls.Add(pnlMain, 1, 0);
            
            Panel pnlSearch = new Panel { Dock = DockStyle.Top, Height = 60 };
            Label lblSearch = new Label { Text = "SEARCH PATIENTS", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 20 };
            txtSearch = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont };
            txtSearch.TextChanged += (s, e) => LoadPatients(txtSearch.Text.Trim());
            pnlSearch.Controls.Add(txtSearch);
            pnlSearch.Controls.Add(lblSearch);

            dgvPatients = new DataGridView { 
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 20, 0, 0)
            };
            UIStyles.ApplyModernStyle(dgvPatients);
            dgvPatients.CellClick += DgvPatients_CellClick;

            pnlMain.Controls.Add(dgvPatients);
            pnlMain.Controls.Add(pnlSearch);

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnHistory.Click += (s, e) => {
                if (selectedPatientID == 0) { MessageBox.Show("Select a patient from the grid first!"); return; }
                new PatientHistoryForm(selectedPatientID, txtFullName.Text).ShowDialog();
            };
            btnClear.Click += (s, e) => ClearFields();
        }

        private TextBox CreateInputModern(TableLayoutPanel tlp, string label)
        {
            tlp.Controls.Add(new Label { Text = label, Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true, Margin = new Padding(0, 5, 0, 0) });
            TextBox tb = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, Margin = new Padding(0, 0, 0, 10) };
            tlp.Controls.Add(tb);
            return tb;
        }

        private ComboBox CreateComboBoxModern(TableLayoutPanel tlp, string label, string[] items)
        {
            tlp.Controls.Add(new Label { Text = label, Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true, Margin = new Padding(0, 5, 0, 0) });
            ComboBox cb = new ComboBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 0, 10) };
            cb.Items.AddRange(items);
            tlp.Controls.Add(cb);
            return cb;
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

        private List<Patient> allPatients = new List<Patient>();

        private void LoadPatients(string search = "")
        {
            try {
                // Requirement: Collections (List<T>)
                string query = "SELECT FullName, Gender, Age, BloodGroup, Disease, Contact, Address, DoctorAssigned, PatientID FROM Patients";
                DataTable dt = DatabaseHelper.ExecuteQuery(query);
                
                allPatients.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    allPatients.Add(new Patient {
                        ID = Convert.ToInt32(row["PatientID"]),
                        FullName = row["FullName"].ToString(),
                        Gender = row["Gender"].ToString(),
                        Age = Convert.ToInt32(row["Age"]),
                        BloodGroup = row["BloodGroup"].ToString(),
                        Disease = row["Disease"].ToString(),
                        Contact = row["Contact"].ToString(),
                        Address = row["Address"].ToString(),
                        DoctorAssigned = row["DoctorAssigned"].ToString()
                    });
                }

                // Requirement: LINQ for filtering in-memory collection
                var filtered = allPatients.AsEnumerable();
                if (!string.IsNullOrEmpty(search))
                {
                    filtered = allPatients.Where(p => 
                        p.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                        p.Contact.Contains(search) || 
                        p.Disease.Contains(search, StringComparison.OrdinalIgnoreCase)
                    );
                }

                dgvPatients.DataSource = null;
                dgvPatients.DataSource = filtered.ToList();
                
                if (dgvPatients.Columns.Contains("ID"))
                    dgvPatients.Columns["ID"].Visible = false;

                if (dgvPatients.Columns.Contains("CreatedAt"))
                    dgvPatients.Columns["CreatedAt"].Visible = false;

            } catch (Exception ex) {
                MessageBox.Show("Error loading patients: " + ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateAllInputs()) return;

            try {
                // Requirement 2: Fixed Duplicate Check logic
                string checkQuery = "SELECT COUNT(*) FROM Patients WHERE FullName = @name AND Contact = @contact";
                SqlParameter[] checkParams = {
                    new SqlParameter("@name", txtFullName.Text.Trim()),
                    new SqlParameter("@contact", txtContact.Text.Trim())
                };

                object result = DatabaseHelper.ExecuteScalar(checkQuery, checkParams);
                if (result != null && Convert.ToInt32(result) > 0)
                {
                    MessageBox.Show("Patient already exists!", "Duplicate Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = @"INSERT INTO Patients (FullName, Gender, Age, BloodGroup, Disease, Contact, Address, DoctorAssigned) 
                                VALUES (@name, @gender, @age, @bg, @disease, @contact, @address, @doctor)";
                
                DatabaseHelper.ExecuteNonQuery(query, GetParameters());
                MessageBox.Show("Patient added successfully!");
                ClearFields();
                LoadPatients();
            } catch (Exception ex) {
                MessageBox.Show("Error adding patient: " + ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedPatientID == 0) { MessageBox.Show("Select a patient from the list first!"); return; }
            if (!ValidateAllInputs()) return;

            try {
                string query = @"UPDATE Patients SET FullName=@name, Gender=@gender, Age=@age, BloodGroup=@bg, 
                                Disease=@disease, Contact=@contact, Address=@address, DoctorAssigned=@doctor 
                                WHERE PatientID=@id";
                
                var p = GetParameters();
                Array.Resize(ref p, p.Length + 1);
                p[p.Length - 1] = new SqlParameter("@id", selectedPatientID);

                DatabaseHelper.ExecuteNonQuery(query, p);
                MessageBox.Show("Patient updated successfully!");
                ClearFields();
                LoadPatients();
            } catch (Exception ex) {
                MessageBox.Show("Error updating patient: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPatientID == 0) { MessageBox.Show("Select a patient first!"); return; }
            
            if (MessageBox.Show("Are you sure?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                try {
                    string query = "DELETE FROM Patients WHERE PatientID=@id";
                    DatabaseHelper.ExecuteNonQuery(query, new SqlParameter[] { new SqlParameter("@id", selectedPatientID) });
                    MessageBox.Show("Patient deleted!");
                    ClearFields();
                    LoadPatients();
                } catch (Exception ex) {
                    MessageBox.Show("Error deleting patient: " + ex.Message);
                }
            }
        }

        private void DgvPatients_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) {
                DataGridViewRow row = dgvPatients.Rows[e.RowIndex];
                selectedPatientID = Convert.ToInt32(row.Cells["ID"].Value);
                
                txtFullName.Text = row.Cells["FullName"].Value?.ToString() ?? "";
                cmbGender.SelectedItem = row.Cells["Gender"].Value?.ToString();
                txtAge.Text = row.Cells["Age"].Value?.ToString() ?? "";
                cmbBloodGroup.SelectedItem = row.Cells["BloodGroup"].Value?.ToString();
                txtDisease.Text = row.Cells["Disease"].Value?.ToString() ?? "";
                txtContact.Text = row.Cells["Contact"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtDoctor.Text = row.Cells["DoctorAssigned"].Value?.ToString() ?? "";
            }
        }

        private SqlParameter[] GetParameters()
        {
            return new SqlParameter[] {
                new SqlParameter("@name", txtFullName.Text.Trim()),
                new SqlParameter("@gender", cmbGender.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@age", int.Parse(txtAge.Text)),
                new SqlParameter("@bg", cmbBloodGroup.SelectedItem?.ToString() ?? ""),
                new SqlParameter("@disease", txtDisease.Text.Trim()),
                new SqlParameter("@contact", txtContact.Text.Trim()),
                new SqlParameter("@address", txtAddress.Text.Trim()),
                new SqlParameter("@doctor", txtDoctor.Text.Trim())
            };
        }

        private bool ValidateAllInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text)) {
                MessageBox.Show("FullName: cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cmbGender.SelectedIndex == -1) {
                MessageBox.Show("Gender: must be selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!int.TryParse(txtAge.Text, out int age) || age < 1 || age > 120) {
                MessageBox.Show("Age: must be a number between 1 and 120", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (cmbBloodGroup.SelectedIndex == -1) {
                MessageBox.Show("Blood Group: must be selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Regex.IsMatch(txtContact.Text, @"^\d{11}$")) {
                MessageBox.Show("Please enter a valid contact number (11 digits)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtDisease.Text)) {
                MessageBox.Show("Disease: cannot be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void ClearFields()
        {
            selectedPatientID = 0;
            txtFullName.Clear();
            txtAge.Clear();
            txtContact.Clear();
            txtAddress.Clear();
            txtDisease.Clear();
            txtDoctor.Clear();
            cmbGender.SelectedIndex = -1;
            cmbBloodGroup.SelectedIndex = -1;
        }

        private void PerformOneTimeCleanup()
        {
            try {
                // Requirement 3: Cleanup query
                string cleanupQuery = "DELETE FROM Patients WHERE PatientID NOT IN (SELECT MIN(PatientID) FROM Patients GROUP BY FullName, Contact)";
                DatabaseHelper.ExecuteNonQuery(cleanupQuery);
            } catch { /* Silent fail if already clean */ }
        }
    }
}
