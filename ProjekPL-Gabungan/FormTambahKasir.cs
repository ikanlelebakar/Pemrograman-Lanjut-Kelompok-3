using System;
using System.Windows.Forms;

namespace ProjekPL_Gabungan
{
    public partial class FormTambahKasir : Form
    {
        public FormTambahKasir()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;

            guna2Button3.Click += (s, e) => { new FormLogin().Show(); this.Hide(); };
            guna2Button5.Click += (s, e) => { new FormRiwayatStok().Show(); this.Hide(); };
            guna2Button6.Click += (s, e) => { new FormTambahProduk().Show(); this.Hide(); };
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pnlForm_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button9_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button10_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void FormTambahKasir_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            FormDashboard formDash = new FormDashboard();
            formDash.Show();  // Buka kembali FormDashboard
            this.Hide();      // Sembunyikan FormTambahKasir
        }

        private void FormTambahKasir_Load(object sender, EventArgs e)
        {

        }
    }
}
