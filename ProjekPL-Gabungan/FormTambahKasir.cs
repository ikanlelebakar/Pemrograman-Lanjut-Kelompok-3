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

            guna2Button3.Click += (s, e) => FormLogin.Buka(this, FormLogin.Instance);
            guna2Button5.Click += (s, e) => FormLogin.Buka(this, new FormRiwayatStok());
            guna2Button6.Click += (s, e) => FormLogin.Buka(this, new FormTambahProduk());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            FormLogin.Keluar();
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
            if (!FormLogin.SedangBerpindah && !FormLogin.SedangKeluar) FormLogin.Instance.Show();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            FormLogin.Buka(this, new FormDashboard());
        }

        private void FormTambahKasir_Load(object sender, EventArgs e)
        {

        }
    }
}
