using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Windows.Forms;

namespace SmartHospitalMS
{
    public partial class LoginForm : Form
    {
        private TextBox txtUsername;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private Button btnLogin;
        private Label lblMessage;

        public LoginForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "Smart Hospital MS - Login";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblTitle = new Label() { Text = "Hospital Login", Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(100, 20), Size = new System.Drawing.Size(200, 30), TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            
            Label lblUser = new Label() { Text = "Username:", Location = new System.Drawing.Point(50, 70), Size = new System.Drawing.Size(80, 20) };
            txtUsername = new TextBox() { Location = new System.Drawing.Point(150, 70), Size = new System.Drawing.Size(180, 20) };

            Label lblPass = new Label() { Text = "Password:", Location = new System.Drawing.Point(50, 110), Size = new System.Drawing.Size(80, 20) };
            txtPassword = new TextBox() { Location = new System.Drawing.Point(150, 110), Size = new System.Drawing.Size(180, 20), PasswordChar = '*' };

            Label lblRole = new Label() { Text = "Role:", Location = new System.Drawing.Point(50, 150), Size = new System.Drawing.Size(80, 20) };
            cmbRole = new ComboBox() { Location = new System.Drawing.Point(150, 150), Size = new System.Drawing.Size(180, 20), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Admin", "Doctor", "Receptionist" });
            cmbRole.SelectedIndex = 0;

            btnLogin = new Button() { Text = "Login", Location = new System.Drawing.Point(150, 190), Size = new System.Drawing.Size(180, 35), BackColor = System.Drawing.Color.SteelBlue, ForeColor = System.Drawing.Color.White };
            btnLogin.Click += BtnLogin_Click;

            lblMessage = new Label() { Text = "", Location = new System.Drawing.Point(50, 235), Size = new System.Drawing.Size(300, 20), ForeColor = System.Drawing.Color.Red, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };

            this.Controls.AddRange(new Control[] { lblTitle, lblUser, txtUsername, lblPass, txtPassword, lblRole, cmbRole, btnLogin, lblMessage });
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Name = "LoginForm";
            this.ResumeLayout(false);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text; 
            string role = cmbRole.SelectedItem.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                // Requirement: Hash the input password before checking against the DB
                string hashedPassword = SecurityHelper.HashPassword(password);

                // SQL Query using Parameters to prevent SQL Injection
                string query = "SELECT * FROM Users WHERE Username = @user AND PasswordHash = @pass AND Role = @role";
                SqlParameter[] parameters = {
                    new SqlParameter("@user", username),
                    new SqlParameter("@pass", hashedPassword), // Now comparing against hash
                    new SqlParameter("@role", role)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    Session.CurrentUser = new User {
                        ID = Convert.ToInt32(row["UserID"]),
                        Username = row["Username"].ToString(),
                        Role = row["Role"].ToString()
                    };

                    MessageBox.Show($"Welcome {Session.CurrentUser.Username}!", "Login Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.Hide();
                    Dashboard dashboard = new Dashboard();
                    dashboard.Show();
                }
                else
                {
                    lblMessage.Text = "Invalid username, password, or role.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Security Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
