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
    public partial class FormLogin : Form
    {
        internal static FormLogin Instance { get; private set; }
        internal static bool SedangBerpindah { get; private set; }
        internal static bool SedangKeluar { get; private set; }

        internal static void Buka(Form dari, Form tujuan)
        {
            SedangBerpindah = true;
            tujuan.Show();
            dari.Close();
            SedangBerpindah = false;
        }

        internal static void Keluar()
        {
            SedangKeluar = true;
            Application.Exit();
        }

        public FormLogin()
        {
            InitializeComponent();
            Instance = this;
            btnLogin.Click += btnLogin_Click;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (username == "admin" && password == "admin123")
            {
                new FormDashboard().Show();
                this.Hide();
            }
            else if (username == "kasir" && password == "kasir123")
            {
                new FormKasir().Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Username atau password salah.", "Login Gagal",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Keluar();
        }
    }
}
