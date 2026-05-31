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

            // 1. Sidebar Billing Panel
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

            Label lblHeader = new Label { 
                Text = "GENERATE INVOICE", 
                Font = UIStyles.SubHeaderFont, 
                ForeColor = UIStyles.PrimaryColor, 
                Dock = DockStyle.Top, 
                Height = 40 
            };
            pnlSide.Controls.Add(pnlSideScroll);
            pnlSide.Controls.Add(lblHeader);

            lblToken = new Label { Text = "Token: Select an appointment", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 25 };
            pnlSideScroll.Controls.Add(lblToken);

            lblPatientName = new Label { Text = "Patient: ---", Font = UIStyles.RegularFont, ForeColor = UIStyles.TextPrimary, Dock = DockStyle.Top, Height = 35 };
            pnlSideScroll.Controls.Add(lblPatientName);

            TableLayoutPanel tlpInputs = new TableLayoutPanel {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(0, 10, 20, 10)
            };
            tlpInputs.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            txtConsultationFee = CreateBillingInputModern(tlpInputs, "Consultation Fee", "500");
            txtMedicineFee = CreateBillingInputModern(tlpInputs, "Medicine Fee", "0");
            txtLabFee = CreateBillingInputModern(tlpInputs, "Lab/Test Fee", "0");
            txtTax = CreateBillingInputModern(tlpInputs, "Tax (%)", "5");

            pnlSideScroll.Controls.Add(tlpInputs);

            Panel pnlTotal = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 0) };
            Label lblTotalText = new Label { Text = "TOTAL AMOUNT", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 20 };
            txtTotal = new TextBox { Dock = DockStyle.Top, Font = UIStyles.SubHeaderFont, ReadOnly = true, BackColor = Color.LightCyan, ForeColor = UIStyles.PrimaryColor };
            pnlTotal.Controls.Add(txtTotal);
            pnlTotal.Controls.Add(lblTotalText);
            pnlSideScroll.Controls.Add(pnlTotal);

            // Action Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(0, 10, 0, 0) };
            btnCalculate = CreateActionButton("Calculate", UIStyles.PrimaryColor);
            btnSaveBill = CreateActionButton("Save Bill", UIStyles.AccentColor);
            flpButtons.Controls.AddRange(new Control[] { btnCalculate, btnSaveBill });
            pnlSideScroll.Controls.Add(flpButtons);

            btnPrint = new Button { 
                Text = "PRINT INVOICE (.TXT)", 
                Dock = DockStyle.Top, 
                Height = 45, 
                BackColor = UIStyles.SecondaryColor, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont,
                Margin = new Padding(0, 10, 0, 0)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            pnlSideScroll.Controls.Add(btnPrint);

            // 2. Main Grid Area
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            mainLayout.Controls.Add(pnlMain, 1, 0);

            Label lblGridTitle = new Label { Text = "COMPLETED APPOINTMENTS AWAITING BILLING", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 30 };
            
            dgvAppointments = new DataGridView { 
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };
            UIStyles.ApplyModernStyle(dgvAppointments);
            dgvAppointments.CellClick += DgvAppointments_CellClick;

            pnlMain.Controls.Add(dgvAppointments);
            pnlMain.Controls.Add(lblGridTitle);

            btnCalculate.Click += (s, e) => CalculateTotal();
            btnSaveBill.Click += BtnSaveBill_Click;
            btnPrint.Click += BtnPrint_Click;
        }

        private TextBox CreateBillingInputModern(TableLayoutPanel tlp, string label, string defVal)
        {
            tlp.Controls.Add(new Label { Text = label, Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, AutoSize = true, Margin = new Padding(0, 5, 0, 0) });
            TextBox tb = new TextBox { Text = defVal, Dock = DockStyle.Top, Font = UIStyles.RegularFont, Margin = new Padding(0, 0, 0, 10) };
            tlp.Controls.Add(tb);
            return tb;
        }

        private Button CreateActionButton(string text, Color color)
        {
            Button btn = new Button { 
                Text = text, 
                Size = new Size(145, 38), 
                BackColor = color, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = UIStyles.SmallFont 
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
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
