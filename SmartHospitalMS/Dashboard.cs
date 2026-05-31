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
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.MinimumSize = new System.Drawing.Size(1000, 700);
            this.Name = "Dashboard";
            this.Text = "Smart Hospital MS - Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void SetupDashboard()
        {
            this.BackColor = UIStyles.LightBackground;
            this.DoubleBuffered = true;

            // 1. Header Panel
            headerPanel = new Panel { 
                Dock = DockStyle.Top, 
                Height = 70, 
                BackColor = UIStyles.PrimaryColor,
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(headerPanel);
            
            Label lblTitle = new Label { 
                Text = "SMART HOSPITAL MS", 
                ForeColor = Color.White, 
                Font = UIStyles.HeaderFont, 
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true 
            };

            lblClock = new Label { 
                Text = DateTime.Now.ToString("HH:mm:ss"), 
                ForeColor = Color.White, 
                Font = UIStyles.SubHeaderFont, 
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true 
            };

            clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            clockTimer.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
            clockTimer.Start();

            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(lblClock);

            // Main Body Layout (Below Header)
            TableLayoutPanel mainBodyLayout = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainBodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            mainBodyLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(mainBodyLayout);
            mainBodyLayout.BringToFront(); // Ensure it's not covered by Header if docked Top

            // 2. Side Navigation
            sidePanel = new Panel { 
                Dock = DockStyle.Fill, 
                BackColor = UIStyles.SecondaryColor,
                Padding = new Padding(0, 20, 0, 0)
            };
            mainBodyLayout.Controls.Add(sidePanel, 0, 0);
            
            Panel sideScroll = new Panel {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            lblUser = new Label { 
                Text = $"{Session.CurrentUser?.Username}\n({Session.CurrentUser?.Role})", 
                ForeColor = Color.White, 
                Font = UIStyles.RegularFont,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60
            };

            Panel navContainer = new Panel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 20, 0, 0) };
            
            Button btnDash = CreateNavButton("Dashboard", 0);
            Button btnPatients = CreateNavButton("Patients", 50);
            Button btnAppointments = CreateNavButton("Appointments", 100);
            Button btnBilling = CreateNavButton("Billing", 150);
            Button btnTheme = CreateNavButton("Toggle Theme", 200);
            Button btnLogout = CreateNavButton("Logout", 250);

            btnDash.Dock = DockStyle.Top;
            btnPatients.Dock = DockStyle.Top;
            btnAppointments.Dock = DockStyle.Top;
            btnBilling.Dock = DockStyle.Top;
            btnTheme.Dock = DockStyle.Top;
            btnLogout.Dock = DockStyle.Top;

            btnPatients.Click += (s, e) => { new PatientForm().ShowDialog(); LoadStats(); };
            btnAppointments.Click += (s, e) => { new AppointmentForm().ShowDialog(); LoadStats(); };
            btnBilling.Click += (s, e) => { new BillingForm().ShowDialog(); LoadStats(); };
            btnTheme.Click += (s, e) => ToggleTheme();
            btnLogout.Click += (s, e) => { Session.Logout(); this.Hide(); new LoginForm().Show(); };

            navContainer.Controls.AddRange(new Control[] { btnLogout, btnTheme, btnBilling, btnAppointments, btnPatients, btnDash });

            sideScroll.Controls.Add(navContainer);
            sidePanel.Controls.Add(sideScroll);
            sidePanel.Controls.Add(lblUser);

            // 3. Main Content Area
            mainPanel = new Panel { 
                Dock = DockStyle.Fill, 
                BackColor = UIStyles.LightBackground, 
                Padding = new Padding(25),
                AutoScroll = true
            };
            mainBodyLayout.Controls.Add(mainPanel, 1, 0);

            // Stats Cards - Responsive Grid
            TableLayoutPanel statsGrid = new TableLayoutPanel { 
                Dock = DockStyle.Top, 
                Height = 140,
                ColumnCount = 4,
                RowCount = 1
            };
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            statsGrid.Controls.Add(CreateStatCard("Patients", out lblTotalPatients, UIStyles.PrimaryColor), 0, 0);
            statsGrid.Controls.Add(CreateStatCard("Doctors", out lblTotalDoctors, UIStyles.AccentColor), 1, 0);
            statsGrid.Controls.Add(CreateStatCard("Appointments", out lblTodayAppointments, Color.FromArgb(155, 89, 182)), 2, 0);
            statsGrid.Controls.Add(CreateStatCard("Revenue", out lblTotalRevenue, Color.FromArgb(230, 126, 34)), 3, 0);

            // Chart Section
            Panel chartSection = new Panel {
                Dock = DockStyle.Top,
                Height = 500,
                Padding = new Padding(0, 20, 0, 0)
            };

            Label lblChartTitle = new Label {
                Text = "Appointments (Last 30 Days)",
                Font = UIStyles.SubHeaderFont,
                ForeColor = UIStyles.TextPrimary,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.BottomLeft
            };

            Panel chartContainer = new Panel {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            chartPanel = new Panel { 
                Location = new Point(0, 0), 
                Size = new Size(720, 330), 
                BackColor = Color.White
            };
            chartPanel.Paint += ChartPanel_Paint;
            chartContainer.Controls.Add(chartPanel);

            chartSection.Controls.Add(chartContainer);
            chartSection.Controls.Add(lblChartTitle);

            mainPanel.Controls.Add(chartSection);
            mainPanel.Controls.Add(statsGrid);
        }

        private void ChartPanel_Paint(object sender, PaintEventArgs e)
        {
            if (chartData == null || chartData.Rows.Count == 0)
            {
                e.Graphics.DrawString("No data available for the last 30 days", UIStyles.RegularFont, Brushes.Gray, 20, 20);
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int margin = 50;
            int barWidth = 60;
            int spacing = 30;
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

                // Draw Bar with rounded top
                using (var brush = new SolidBrush(UIStyles.PrimaryColor))
                {
                    g.FillRectangle(brush, x, y, barWidth, h);
                }
                
                // Draw Value
                g.DrawString(val.ToString(), UIStyles.SmallFont, Brushes.Black, x + (barWidth/2) - 10, y - 20);
                
                // Draw Label rotated or slanted
                g.DrawString(day, UIStyles.SmallFont, Brushes.Black, x, chartPanel.Height - margin + 10);
            }

            // Draw Axes
            using (var pen = new Pen(Color.LightGray, 1))
            {
                g.DrawLine(pen, margin, margin, margin, chartPanel.Height - margin); // Y
                g.DrawLine(pen, margin, chartPanel.Height - margin, chartPanel.Width - margin, chartPanel.Height - margin); // X
            }
        }

        private void UpdateChartSize()
        {
            if (chartData == null || chartData.Rows.Count == 0) return;

            int margin = 50;
            int barWidth = 60;
            int spacing = 30;
            int requiredWidth = margin + (chartData.Rows.Count * (barWidth + spacing)) + margin;

            int containerWidth = chartPanel.Parent != null ? chartPanel.Parent.Width : 740;
            chartPanel.Width = Math.Max(requiredWidth, containerWidth - 20);
            chartPanel.Height = chartPanel.Parent != null ? chartPanel.Parent.Height - 20 : 330;
        }

        private Button CreateNavButton(string text, int yPos)
        {
            Button btn = new Button {
                Text = "  " + text,
                Size = new Size(220, 50),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(200, 200, 200),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Font = UIStyles.RegularFont,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(52, 73, 94);
            
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.ForeColor = Color.FromArgb(200, 200, 200);

            return btn;
        }

        private Panel CreateStatCard(string title, out Label valLabel, Color accentColor)
        {
            Panel card = new Panel { 
                BackColor = Color.White, 
                Margin = new Padding(10),
                Padding = new Padding(15),
                Dock = DockStyle.Fill
            };
            
            // Accent line at top
            Panel accent = new Panel { BackColor = accentColor, Dock = DockStyle.Top, Height = 4 };
            card.Controls.Add(accent);

            Label t = new Label { 
                Text = title.ToUpper(), 
                ForeColor = UIStyles.TextSecondary, 
                Font = UIStyles.SmallFont, 
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.BottomLeft
            };
            
            valLabel = new Label { 
                Text = "0", 
                ForeColor = UIStyles.TextPrimary, 
                Font = UIStyles.StatValueFont, 
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            card.Controls.Add(valLabel);
            card.Controls.Add(t);
            
            // Add to a wrapper to create margins in TableLayoutPanel
            Panel wrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            wrapper.Controls.Add(card);
            
            return wrapper;
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
