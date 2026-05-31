using System;
using System.Drawing;
using System.Windows.Forms;

namespace SmartHospitalMS
{
    public static class UIStyles
    {
        // Colors
        public static readonly Color PrimaryColor = Color.FromArgb(41, 128, 185); // Blue
        public static readonly Color SecondaryColor = Color.FromArgb(44, 62, 80); // Dark Gray-Blue
        public static readonly Color AccentColor = Color.FromArgb(46, 204, 113); // Green
        public static readonly Color DangerColor = Color.FromArgb(231, 76, 60); // Red
        public static readonly Color WarningColor = Color.FromArgb(241, 196, 15); // Yellow
        public static readonly Color LightBackground = Color.FromArgb(245, 247, 250);
        public static readonly Color DarkBackground = Color.FromArgb(33, 37, 41);
        public static readonly Color White = Color.White;
        public static readonly Color TextPrimary = Color.FromArgb(45, 52, 54);
        public static readonly Color TextSecondary = Color.FromArgb(99, 110, 114);

        // Fonts
        public static readonly Font HeaderFont = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static readonly Font RegularFont = new Font("Segoe UI", 10);
        public static readonly Font SmallFont = new Font("Segoe UI", 9);
        public static readonly Font StatValueFont = new Font("Segoe UI", 22, FontStyle.Bold);

        public static void ApplyModernStyle(Control control)
        {
            if (control is Button btn)
            {
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = RegularFont;
                btn.Cursor = Cursors.Hand;
            }
            else if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = White;
                dgv.BorderStyle = BorderStyle.None;
                dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                dgv.EnableHeadersVisualStyles = false;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = White;
                dgv.ColumnHeadersDefaultCellStyle.Font = SubHeaderFont;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryColor;
                dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 241, 248);
                dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
                dgv.DefaultCellStyle.Font = RegularFont;
                dgv.RowHeadersVisible = false;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 253);
            }
        }

        public static Panel CreateCard()
        {
            return new Panel
            {
                BackColor = White,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.None
            };
        }
    }
}
