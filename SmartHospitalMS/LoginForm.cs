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
            this.Size = new System.Drawing.Size(480, 550);
            this.MinimumSize = new System.Drawing.Size(480, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = UIStyles.LightBackground;

            // Simple Card Panel
            Panel card = new Panel {
                Size = new System.Drawing.Size(380, 460),
                BackColor = Color.White,
                Padding = new Padding(30),
                BorderStyle = BorderStyle.None
            };

            // Centering logic
            Action centerCard = () => {
                card.Left = (this.ClientSize.Width - card.Width) / 2;
                card.Top = (this.ClientSize.Height - card.Height) / 2;
            };
            this.Load += (s, e) => centerCard();
            this.Resize += (s, e) => centerCard();

            Label lblTitle = new Label { 
                Text = "Hospital Login", 
                Font = UIStyles.HeaderFont, 
                ForeColor = UIStyles.PrimaryColor,
                TextAlign = ContentAlignment.MiddleCenter, 
                Dock = DockStyle.Top,
                Height = 60,
                Margin = new Padding(0, 0, 0, 10)
            };
            
            Panel inputArea = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };

            // Username
            Label lblUser = new Label { Text = "Username", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 25 };
            txtUsername = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont };
            Panel s1 = new Panel { Dock = DockStyle.Top, Height = 15 };

            // Password
            Label lblPass = new Label { Text = "Password", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 25 };
            txtPassword = new TextBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, PasswordChar = '*' };
            Panel s2 = new Panel { Dock = DockStyle.Top, Height = 15 };

            // Role
            Label lblRole = new Label { Text = "Role", Font = UIStyles.SmallFont, ForeColor = UIStyles.TextSecondary, Dock = DockStyle.Top, Height = 25 };
            cmbRole = new ComboBox { Dock = DockStyle.Top, Font = UIStyles.RegularFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Admin", "Doctor", "Receptionist" });
            cmbRole.SelectedIndex = 0;
            Panel s3 = new Panel { Dock = DockStyle.Top, Height = 25 };

            // Button
            btnLogin = new Button { 
                Text = "LOGIN", 
                Dock = DockStyle.Top, 
                Height = 50, 
                BackColor = UIStyles.PrimaryColor, 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = UIStyles.SubHeaderFont,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            lblMessage = new Label { 
                Text = "", 
                Dock = DockStyle.Top, 
                Height = 40, 
                ForeColor = UIStyles.DangerColor, 
                TextAlign = ContentAlignment.MiddleCenter,
                Font = UIStyles.SmallFont,
                Margin = new Padding(0, 5, 0, 0)
            };

            // Add in reverse order for Dock.Top
            inputArea.Controls.Add(lblMessage);
            inputArea.Controls.Add(btnLogin);
            inputArea.Controls.Add(s3);
            inputArea.Controls.Add(cmbRole);
            inputArea.Controls.Add(lblRole);
            inputArea.Controls.Add(s2);
            inputArea.Controls.Add(txtPassword);
            inputArea.Controls.Add(lblPass);
            inputArea.Controls.Add(s1);
            inputArea.Controls.Add(txtUsername);
            inputArea.Controls.Add(lblUser);

            card.Controls.Add(inputArea);
            card.Controls.Add(lblTitle);

            this.Controls.Add(card);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(384, 261);
            this.Name = "LoginForm";
            this.ResumeLayout(false);
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text; 

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                lblMessage.ForeColor = System.Drawing.Color.Blue;
                lblMessage.Text = "Logging in... please wait.";
                btnLogin.Enabled = false;

                string role = cmbRole.SelectedItem?.ToString() ?? "Admin";

                // Requirement: Hash the input password before checking against the DB
                string hashedPassword = SecurityHelper.HashPassword(password);

                // SQL Query using Parameters to prevent SQL Injection
                string query = "SELECT * FROM Users WHERE Username = @user AND PasswordHash = @pass AND Role = @role";
                SqlParameter[] parameters = {
                    new SqlParameter("@user", username),
                    new SqlParameter("@pass", hashedPassword), 
                    new SqlParameter("@role", role)
                };

                // Use Task.Run to keep UI responsive during DB call
                DataTable dt = await Task.Run(() => DatabaseHelper.ExecuteQuery(query, parameters));

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
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Text = "Invalid username, password, or role.";
                    btnLogin.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Login failed.";
                btnLogin.Enabled = true;
                MessageBox.Show("Security Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
