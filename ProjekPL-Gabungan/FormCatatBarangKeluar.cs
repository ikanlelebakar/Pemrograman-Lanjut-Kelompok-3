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
    public partial class FormCatatBarangKeluar : Form
    {
        public FormCatatBarangKeluar()
        {
            InitializeComponent();
            this.FormClosed += (s, e) => { if (!FormLogin.SedangBerpindah && !FormLogin.SedangKeluar) FormLogin.Instance.Show(); };
        }

        private void pnlSearch_Paint(object sender, PaintEventArgs e)
        {

        }

        private void picsearch_Click(object sender, EventArgs e)
        {

        }

        private void lblDeskripsi_Click(object sender, EventArgs e)
        {

        }

        private void lblIdProduk_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new FormRiwayatStok();
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = this.Location;
            frm.Size = this.Size;
            FormLogin.Buka(this, frm);
        }
    }
}
