using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjekPL_Gabungan
{
    public partial class FormKasir : Form
    {
        public FormKasir()
        {
            InitializeComponent();
            guna2Button3.Click += (s, e) => FormLogin.Buka(this, FormLogin.Instance);
            this.FormClosed += (s, e) => { if (!FormLogin.SedangBerpindah && !FormLogin.SedangKeluar) FormLogin.Instance.Show(); };
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            this.Hide();
            using (FormRiwayat halamanRiwayat = new FormRiwayat())
            {
                halamanRiwayat.StartPosition = FormStartPosition.Manual;
                halamanRiwayat.Location = this.Location;
                if (halamanRiwayat.ShowDialog() == DialogResult.Abort)
                {
                    FormLogin.Buka(this, FormLogin.Instance);
                    return;
                }
            }
            this.Show();
        }

        private void guna2HtmlLabel1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button6_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button6_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Button7_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel5_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel34_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel35_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel36_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel40_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox7_Click(object sender, EventArgs e)
        {

        }

        private void guna2ControlBox3_Click(object sender, EventArgs e)
        {

        }

        private void guna2TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton2_Click(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton8_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel32_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel37_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel41_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel38_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel42_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel39_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel43_Click(object sender, EventArgs e)
        {

        }

        private void guna2HtmlLabel44_Click(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton12_Click(object sender, EventArgs e)
        {

        }

        private void guna2ImageButton4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button14_Click(object sender, EventArgs e)
        {
            Form overlay = new Form
            {
                StartPosition = FormStartPosition.Manual,
                FormBorderStyle = FormBorderStyle.None,
                Opacity = 0.45,
                BackColor = Color.Black,
                Size = this.Size,
                Location = this.Location,
                ShowInTaskbar = false
            };

            overlay.Show(this);

            using (ModalBayar modal = new ModalBayar())
            {
                modal.StartPosition = FormStartPosition.CenterParent;
                modal.ShowDialog(overlay);
            }

            overlay.Close();
            overlay.Dispose();
        }
    }
}