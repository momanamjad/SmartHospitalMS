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
            txtFullName = CreateInput(pnlInputs, "Full Name:", ref y);
            cmbGender = CreateComboBox(pnlInputs, "Gender:", new string[] { "Male", "Female", "Other" }, ref y);
            txtAge = CreateInput(pnlInputs, "Age:", ref y);
            cmbBloodGroup = CreateComboBox(pnlInputs, "Blood Group:", new string[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" }, ref y);
            txtDisease = CreateInput(pnlInputs, "Disease:", ref y);
            txtContact = CreateInput(pnlInputs, "Contact (11 Digits):", ref y);
            txtAddress = CreateInput(pnlInputs, "Address:", ref y);
            txtDoctor = CreateInput(pnlInputs, "Assigned Doctor:", ref y);

            btnAdd = new Button { Text = "Add New", Location = new Point(15, y), Size = new Size(90, 40), BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnUpdate = new Button { Text = "Update", Location = new Point(110, y), Size = new Size(90, 40), BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnDelete = new Button { Text = "Delete", Location = new Point(205, y), Size = new Size(90, 40), BackColor = Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            
            y += 50;
            Button btnHistory = new Button { Text = "View Patient History", Location = new Point(15, y), Size = new Size(280, 40), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            
            y += 50;
            btnClear = new Button { Text = "Clear All Fields", Location = new Point(15, y), Size = new Size(280, 35), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnHistory.Click += (s, e) => {
                if (selectedPatientID == 0) { MessageBox.Show("Select a patient from the grid first!"); return; }
                new PatientHistoryForm(selectedPatientID, txtFullName.Text).ShowDialog();
            };
            btnClear.Click += (s, e) => ClearFields();

            pnlInputs.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnHistory, btnClear });
            this.Controls.Add(pnlInputs);

            // 2. Grid Area (RIGHT) - FIXED LAYOUT
            // Requirement 1: Grid must start after the input panel (x=320) and not overlap
            Label lblSearch = new Label { Text = "Search Patient:", Location = new Point(340, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            txtSearch = new TextBox { Location = new Point(450, 18), Size = new Size(400, 25) };
            txtSearch.TextChanged += (s, e) => LoadPatients(txtSearch.Text.Trim());

            dgvPatients = new DataGridView { 
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
            dgvPatients.CellClick += DgvPatients_CellClick;

            this.Controls.Add(lblSearch);
            this.Controls.Add(txtSearch);
            this.Controls.Add(dgvPatients);
        }

        private TextBox CreateInput(Panel p, string label, ref int y)
        {
            p.Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            TextBox tb = new TextBox { Location = new Point(130, y), Size = new Size(160, 25) };
            p.Controls.Add(tb);
            y += 40;
            return tb;
        }

        private ComboBox CreateComboBox(Panel p, string label, string[] items, ref int y)
        {
            p.Controls.Add(new Label { Text = label, Location = new Point(15, y), AutoSize = true, Font = new Font("Segoe UI", 9) });
            ComboBox cb = new ComboBox { Location = new Point(130, y), Size = new Size(160, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cb.Items.AddRange(items);
            p.Controls.Add(cb);
            y += 40;
            return cb;
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
