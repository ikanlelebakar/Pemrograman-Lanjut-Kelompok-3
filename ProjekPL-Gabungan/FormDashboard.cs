using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace ProjekPL_Gabungan
{
    public partial class FormDashboard : Form
    {
        public FormDashboard()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        // --- Event handler bawaan desain (dipertahankan agar tidak error) ---
        private void label20_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void panel17_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label37_Click(object sender, EventArgs e)
        {
        }

        private void panel14_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
        }

        private void btnProduk_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel4_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            FormTambahKasir formKasir = new FormTambahKasir();
            formKasir.Show(); // Buka FormTambahKasir
            this.Hide();
        }

        private void FormDashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }

    internal static class UiTheme
    {
        private static readonly Color Accent = Color.FromArgb(114, 163, 191);
        private static readonly Color Border = Color.FromArgb(213, 224, 229);
        private static readonly Color Ink = Color.FromArgb(22, 48, 63);

        private static Font Font(float size, FontStyle style = FontStyle.Regular)
        {
            string family = FontFamily.Families.Any(f => f.Name == "Poppins") ? "Poppins" : "Segoe UI";
            return new Font(family, size, style);
        }

        internal static void StyleGrid(Guna2DataGridView grid, int rowHeight)
        {
            grid.BackgroundColor = Color.White;
            grid.GridColor = Border;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 33;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Accent;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = Font(11, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            grid.RowsDefaultCellStyle.BackColor = Color.White;
            grid.RowsDefaultCellStyle.ForeColor = Ink;
            grid.RowsDefaultCellStyle.Font = Font(10);
            grid.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(232, 240, 245);
            grid.RowsDefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 251);
            grid.RowTemplate.Height = rowHeight;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        internal static void LoadLogo(PictureBox target)
        {
            string logo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "caffeku.png");
            if (System.IO.File.Exists(logo)) target.Image = Image.FromFile(logo);
        }
    }
}
