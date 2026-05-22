using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace SmartHospitalMS
{
    public partial class Dashboard : Form
    {
        private Panel sidePanel;
        private Panel headerPanel;
        private Panel mainPanel;
        private Label lblClock;
        private Label lblUser;
        private System.Windows.Forms.Timer clockTimer;
        private bool isDarkMode = false;

        // Stats Labels
        private Label lblTotalPatients;
        private Label lblTotalDoctors;
        private Label lblTodayAppointments;
        private Label lblTotalRevenue;

        // Custom Chart Controls
        private Panel chartPanel;
        private DataTable chartData;

        public Dashboard()
        {
            InitializeComponent();
            SetupDashboard();
            LoadStats();
            ApplyRoleSecurity();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Name = "Dashboard";
            this.Text = "Smart Hospital MS - Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void SetupDashboard()
        {
            // 1. Header Panel
            headerPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(41, 128, 185) };
            
            Label lblTitle = new Label { 
                Text = "SMART HOSPITAL MANAGEMENT SYSTEM", 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 14, FontStyle.Bold), 
                Location = new Point(20, 15), 
                AutoSize = true 
            };

            lblClock = new Label { 
                Text = DateTime.Now.ToString("HH:mm:ss"), 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 12), 
                Location = new Point(850, 18), 
                AutoSize = true 
            };

            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            clockTimer.Start();

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblClock);

            // 2. Side Navigation
            sidePanel = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(44, 62, 80) };
            
            lblUser = new Label { 
                Text = $"User: {Session.CurrentUser?.Username}\nRole: {Session.CurrentUser?.Role}", 
                ForeColor = Color.White, 
                Location = new Point(10, 20), 
                Size = new Size(180, 40) 
            };

            Button btnDash = CreateNavButton("Dashboard", 80);
            Button btnPatients = CreateNavButton("Patients", 130);
            Button btnAppointments = CreateNavButton("Appointments", 180);
            Button btnBilling = CreateNavButton("Billing", 230);
            Button btnTheme = CreateNavButton("Toggle Theme", 450);
            Button btnLogout = CreateNavButton("Logout", 500);

            btnPatients.Click += (s, e) => { new PatientForm().ShowDialog(); LoadStats(); };
            btnAppointments.Click += (s, e) => { new AppointmentForm().ShowDialog(); LoadStats(); };
            btnBilling.Click += (s, e) => { new BillingForm().ShowDialog(); LoadStats(); };
            btnTheme.Click += (s, e) => ToggleTheme();
            btnLogout.Click += (s, e) => { Session.Logout(); this.Hide(); new LoginForm().Show(); };

            sidePanel.Controls.AddRange(new Control[] { lblUser, btnDash, btnPatients, btnAppointments, btnBilling, btnTheme, btnLogout });

            // 3. Main Content Area
            mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke, Padding = new Padding(20) };

            // Stats Cards
            FlowLayoutPanel statsPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 120 };
            statsPanel.Controls.Add(CreateStatCard("Patients", out lblTotalPatients, Color.FromArgb(52, 152, 219)));
            statsPanel.Controls.Add(CreateStatCard("Doctors", out lblTotalDoctors, Color.FromArgb(46, 204, 113)));
            statsPanel.Controls.Add(CreateStatCard("Appointments", out lblTodayAppointments, Color.FromArgb(155, 89, 182)));
            statsPanel.Controls.Add(CreateStatCard("Revenue", out lblTotalRevenue, Color.FromArgb(230, 126, 34)));

            // Custom Chart Panel with Scrolling support
            Panel chartContainer = new Panel {
                Location = new Point(20, 150),
                Size = new Size(740, 370), // Slightly taller to account for scrollbar
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            chartPanel = new Panel { 
                Location = new Point(0, 0), 
                Size = new Size(720, 330), 
                BackColor = Color.White
            };
            chartPanel.Paint += ChartPanel_Paint;
            chartContainer.Controls.Add(chartPanel);

            Label lblChartTitle = new Label {
                Text = "Appointments (Last 30 Days)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 125),
                AutoSize = true
            };

            mainPanel.Controls.Add(lblChartTitle);
            mainPanel.Controls.Add(chartContainer);
            mainPanel.Controls.Add(statsPanel);

            this.Controls.Add(mainPanel);
            this.Controls.Add(sidePanel);
            this.Controls.Add(headerPanel);
        }

        private void ChartPanel_Paint(object sender, PaintEventArgs e)
        {
            if (chartData == null || chartData.Rows.Count == 0)
            {
                e.Graphics.DrawString("No data available for the last 30 days", new Font("Segoe UI", 10), Brushes.Gray, 20, 20);
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int margin = 40;
            int barWidth = 80;
            int spacing = 20;
            int chartHeight = chartPanel.Height - (margin * 2);

            // Find max value for scaling
            int maxVal = 1;
            foreach (DataRow row in chartData.Rows)
                maxVal = Math.Max(maxVal, Convert.ToInt32(row["Count"]));

            for (int i = 0; i < chartData.Rows.Count; i++)
            {
                int val = Convert.ToInt32(chartData.Rows[i]["Count"]);
                string day = Convert.ToDateTime(chartData.Rows[i]["Day"]).ToString("MMM dd");

                int h = (int)((float)val / maxVal * chartHeight);
                int x = margin + (i * (barWidth + spacing));
                int y = chartPanel.Height - margin - h;

                // Draw Bar
                g.FillRectangle(new SolidBrush(Color.FromArgb(41, 128, 185)), x, y, barWidth, h);
                
                // Draw Value
                g.DrawString(val.ToString(), new Font("Segoe UI", 8, FontStyle.Bold), Brushes.Black, x + (barWidth/3), y - 18);
                
                // Draw Label
                g.DrawString(day, new Font("Segoe UI", 8), Brushes.Black, x, chartPanel.Height - margin + 5);
            }

            // Draw Axes
            g.DrawLine(new Pen(Color.Gray, 2), margin, margin, margin, chartPanel.Height - margin); // Y
            g.DrawLine(new Pen(Color.Gray, 2), margin, chartPanel.Height - margin, chartPanel.Width - margin, chartPanel.Height - margin); // X
        }

        private void UpdateChartSize()
        {
            if (chartData == null || chartData.Rows.Count == 0) return;

            int margin = 40;
            int barWidth = 80;
            int spacing = 20;
            int requiredWidth = margin + (chartData.Rows.Count * (barWidth + spacing)) + margin;

            // Ensure we use at least the container width
            int containerWidth = chartPanel.Parent != null ? chartPanel.Parent.Width : 740;
            chartPanel.Width = Math.Max(requiredWidth, containerWidth - 5);
        }

        private Button CreateNavButton(string text, int yPos)
        {
            return new Button {
                Text = text,
                Location = new Point(0, yPos),
                Size = new Size(200, 45),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private Panel CreateStatCard(string title, out Label valLabel, Color bgColor)
        {
            Panel p = new Panel { Size = new Size(175, 100), BackColor = bgColor, Margin = new Padding(0, 0, 20, 0) };
            Label t = new Label { Text = title, ForeColor = Color.White, Font = new Font("Segoe UI", 10), Location = new Point(10, 10), AutoSize = true };
            valLabel = new Label { Text = "0", ForeColor = Color.White, Font = new Font("Segoe UI", 20, FontStyle.Bold), Location = new Point(10, 40), AutoSize = true };
            p.Controls.Add(t);
            p.Controls.Add(valLabel);
            return p;
        }

        private async void LoadStats()
        {
            try {
                // Multi-threading: Running DB queries on a background thread to keep UI responsive
                await Task.Run(() => {
                    string patients = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Patients").ToString();
                    string doctors = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Doctors").ToString();
                    string appts = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Appointments WHERE CAST(AppointmentDate AS DATE) = CAST(GETDATE() AS DATE)").ToString();
                    
                    object revResult = DatabaseHelper.ExecuteScalar("SELECT SUM(TotalAmount) FROM Bills");
                    string revenue = revResult == DBNull.Value ? "$0" : string.Format("{0:C0}", revResult);

                    DataTable cData = DatabaseHelper.ExecuteQuery(@"
                        SELECT Day, Count FROM (
                            SELECT TOP 30 CAST(AppointmentDate AS DATE) as Day, COUNT(*) as Count 
                            FROM Appointments 
                            GROUP BY CAST(AppointmentDate AS DATE) 
                            ORDER BY Day DESC
                        ) t ORDER BY Day ASC");

                    // Invoke back to UI thread to update labels
                    this.Invoke((MethodInvoker)delegate {
                        lblTotalPatients.Text = patients;
                        lblTotalDoctors.Text = doctors;
                        lblTodayAppointments.Text = appts;
                        lblTotalRevenue.Text = revenue;
                        chartData = cData;
                        UpdateChartSize();
                        chartPanel.Invalidate();
                    });
                });
            } catch (Exception) {
                // Silent fail for empty DB or network issues
            }
        }

        private void ApplyRoleSecurity()
        {
            string role = Session.CurrentUser?.Role;
            foreach (Control c in sidePanel.Controls) {
                if (c is Button btn) {
                    if (role == "Doctor") {
                        if (btn.Text == "Billing") btn.Visible = false;
                    } else if (role == "Receptionist") {
                        if (btn.Text == "Patients") btn.Visible = false;
                    }
                }
            }
        }

        private void ToggleTheme()
        {
            isDarkMode = !isDarkMode;
            if (isDarkMode) {
                mainPanel.BackColor = Color.FromArgb(33, 33, 33);
                sidePanel.BackColor = Color.Black;
                chartPanel.BackColor = Color.FromArgb(50, 50, 50);
                foreach (Control c in mainPanel.Controls) {
                    if (c is Label l) l.ForeColor = Color.White;
                }
            } else {
                mainPanel.BackColor = Color.WhiteSmoke;
                sidePanel.BackColor = Color.FromArgb(44, 62, 80);
                chartPanel.BackColor = Color.White;
                foreach (Control c in mainPanel.Controls) {
                    if (c is Label l) l.ForeColor = Color.Black;
                }
            }
            chartPanel.Invalidate();
        }
    }
}
