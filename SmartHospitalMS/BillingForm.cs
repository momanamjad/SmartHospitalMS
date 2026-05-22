using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SmartHospitalMS
{
    public partial class BillingForm : Form
    {
        private DataGridView dgvAppointments;
        private TextBox txtConsultationFee, txtMedicineFee, txtLabFee, txtTax, txtTotal;
        private Label lblPatientName, lblToken;
        private Button btnCalculate, btnSaveBill, btnPrint;
        private int selectedAppointmentID = 0;

        public BillingForm()
        {
            InitializeComponent();
            SetupUI();
            LoadCompletedAppointments();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Name = "BillingForm";
            this.Text = "Billing & Invoicing";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            this.BackColor = Color.White;

            // 1. Billing Panel (LEFT)
            Panel pnlBill = new Panel { 
                Location = new Point(0, 0),
                Size = new Size(350, 700),
                BackColor = Color.WhiteSmoke, 
                Padding = new Padding(20),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            int y = 20;
            Label lblHeader = new Label { Text = "GENERATE INVOICE", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            pnlBill.Controls.Add(lblHeader);
            y += 40;

            lblToken = new Label { Text = "Token: Select an appointment", Font = new Font("Segoe UI", 9, FontStyle.Italic), Location = new Point(20, y), AutoSize = true };
            pnlBill.Controls.Add(lblToken);
            y += 25;

            lblPatientName = new Label { Text = "Patient: ---", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            pnlBill.Controls.Add(lblPatientName);
            y += 40;

            txtConsultationFee = CreateBillingInput(pnlBill, "Consultation Fee:", "500", ref y);
            txtMedicineFee = CreateBillingInput(pnlBill, "Medicine Fee:", "0", ref y);
            txtLabFee = CreateBillingInput(pnlBill, "Lab/Test Fee:", "0", ref y);
            txtTax = CreateBillingInput(pnlBill, "Tax (%):", "5", ref y);
            
            y += 10;
            Label lblTotalText = new Label { Text = "TOTAL AMOUNT:", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, y), AutoSize = true };
            pnlBill.Controls.Add(lblTotalText);
            txtTotal = new TextBox { Location = new Point(150, y), Size = new Size(160, 25), ReadOnly = true, BackColor = Color.LightCyan, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pnlBill.Controls.Add(txtTotal);
            y += 50;

            btnCalculate = new Button { Text = "Calculate", Location = new Point(20, y), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.SteelBlue, ForeColor = Color.White };
            btnSaveBill = new Button { Text = "Save Bill", Location = new Point(170, y), Size = new Size(140, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.Green, ForeColor = Color.White };
            y += 50;
            btnPrint = new Button { Text = "Print Invoice (.txt)", Location = new Point(20, y), Size = new Size(290, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.DarkSlateGray, ForeColor = Color.White };

            btnCalculate.Click += (s, e) => CalculateTotal();
            btnSaveBill.Click += BtnSaveBill_Click;
            btnPrint.Click += BtnPrint_Click;

            pnlBill.Controls.AddRange(new Control[] { btnCalculate, btnSaveBill, btnPrint });
            this.Controls.Add(pnlBill);

            // 2. Grid Area (RIGHT)
            Label lblGridTitle = new Label { Text = "Completed Appointments Awaiting Billing:", Location = new Point(370, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            dgvAppointments = new DataGridView { 
                Location = new Point(370, 50), 
                Size = new Size(790, 590), 
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            dgvAppointments.CellClick += DgvAppointments_CellClick;

            this.Controls.Add(lblGridTitle);
            this.Controls.Add(dgvAppointments);
        }

        private TextBox CreateBillingInput(Panel p, string label, string defVal, ref int y)
        {
            p.Controls.Add(new Label { Text = label, Location = new Point(20, y), AutoSize = true });
            TextBox tb = new TextBox { Text = defVal, Location = new Point(150, y), Size = new Size(160, 25) };
            p.Controls.Add(tb);
            y += 35;
            return tb;
        }

        private void LoadCompletedAppointments()
        {
            try {
                // Show only Completed appointments that haven't been billed yet
                string query = @"
                    SELECT a.TokenNumber, p.FullName as PatientName, a.AppointmentDate, a.AppointmentID
                    FROM Appointments a
                    JOIN Patients p ON a.PatientID = p.PatientID
                    WHERE a.Status = 'Completed' 
                    AND a.AppointmentID NOT IN (SELECT AppointmentID FROM Bills)";

                dgvAppointments.DataSource = DatabaseHelper.ExecuteQuery(query);
                if (dgvAppointments.Columns.Contains("AppointmentID")) dgvAppointments.Columns["AppointmentID"].Visible = false;
            } catch (Exception ex) {
                MessageBox.Show("Error loading: " + ex.Message);
            }
        }

        private void DgvAppointments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) {
                DataGridViewRow row = dgvAppointments.Rows[e.RowIndex];
                selectedAppointmentID = Convert.ToInt32(row.Cells["AppointmentID"].Value);
                lblToken.Text = "Token: " + row.Cells["TokenNumber"].Value.ToString();
                lblPatientName.Text = "Patient: " + row.Cells["PatientName"].Value.ToString();
                CalculateTotal();
            }
        }

        private decimal CalculateTotal()
        {
            try {
                if (!decimal.TryParse(txtConsultationFee.Text, out decimal con)) con = 0;
                if (!decimal.TryParse(txtMedicineFee.Text, out decimal med)) med = 0;
                if (!decimal.TryParse(txtLabFee.Text, out decimal lab)) lab = 0;
                if (!decimal.TryParse(txtTax.Text, out decimal taxPerc)) taxPerc = 0;

                if (con < 0 || med < 0 || lab < 0 || taxPerc < 0) {
                    MessageBox.Show("Fees and tax cannot be negative!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return 0;
                }

                decimal subtotal = con + med + lab;
                decimal total = subtotal + (subtotal * (taxPerc / 100));
                
                txtTotal.Text = total.ToString("F2");
                return total;
            } catch (Exception ex) {
                MessageBox.Show("Calculation Error: " + ex.Message);
                return 0;
            }
        }

        private void BtnSaveBill_Click(object sender, EventArgs e)
        {
            if (selectedAppointmentID == 0) { MessageBox.Show("Please select an appointment first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try {
                decimal total = CalculateTotal();
                if (total <= 0) {
                    MessageBox.Show("Total amount must be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = @"INSERT INTO Bills (AppointmentID, ConsultationFee, MedicineFee, LabFee, TaxPercentage) 
                                VALUES (@aid, @con, @med, @lab, @tax)";
                
                SqlParameter[] parameters = {
                    new SqlParameter("@aid", selectedAppointmentID),
                    new SqlParameter("@con", decimal.Parse(txtConsultationFee.Text)),
                    new SqlParameter("@med", decimal.Parse(txtMedicineFee.Text)),
                    new SqlParameter("@lab", decimal.Parse(txtLabFee.Text)),
                    new SqlParameter("@tax", decimal.Parse(txtTax.Text))
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Bill saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadCompletedAppointments();
                ClearFields();
            } catch (Exception ex) {
                MessageBox.Show("Database Save Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTotal.Text) || lblPatientName.Text.Contains("---")) {
                MessageBox.Show("Calculate and Save the bill first!");
                return;
            }

            try {
                string fileName = $"Invoice_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string content = "-------------------------------------------\n" +
                                 "       SMART HOSPITAL MANAGEMENT SYSTEM    \n" +
                                 "-------------------------------------------\n" +
                                 $"Date: {DateTime.Now}\n" +
                                 $"{lblToken.Text}\n" +
                                 $"{lblPatientName.Text}\n" +
                                 "-------------------------------------------\n" +
                                 $"Consultation Fee: {txtConsultationFee.Text}\n" +
                                 $"Medicine Fee:     {txtMedicineFee.Text}\n" +
                                 $"Lab/Test Fee:     {txtLabFee.Text}\n" +
                                 $"Tax ({txtTax.Text}%):      {decimal.Parse(txtTotal.Text) - (decimal.Parse(txtConsultationFee.Text)+decimal.Parse(txtMedicineFee.Text)+decimal.Parse(txtLabFee.Text))}\n" +
                                 "-------------------------------------------\n" +
                                 $"TOTAL AMOUNT:     {txtTotal.Text}\n" +
                                 "-------------------------------------------\n" +
                                 "          THANK YOU FOR VISITING!          \n" +
                                 "-------------------------------------------";

                File.WriteAllText(fileName, content);
                MessageBox.Show($"Invoice exported successfully as {fileName}!", "Print Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Open the file automatically
                System.Diagnostics.Process.Start("notepad.exe", fileName);
            } catch (Exception ex) {
                MessageBox.Show("Print Error: " + ex.Message);
            }
        }

        private void ClearFields()
        {
            selectedAppointmentID = 0;
            lblToken.Text = "Token: Select an appointment";
            lblPatientName.Text = "Patient: ---";
            txtMedicineFee.Text = "0";
            txtLabFee.Text = "0";
            txtTotal.Clear();
        }
    }
}
