using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProjekPL_Gabungan
{
    public partial class FormRiwayatStok : Form
    {
        public class RiwayatStokItem
        {
            public string Id { get; set; }
            public string TanggalSingkat { get; set; }
            public string Jam { get; set; }
            public string WaktuLengkap { get; set; }
            public string Petugas { get; set; }
            public string NamaProduk { get; set; }
            public string Kategori { get; set; }
            public string Satuan { get; set; }
            public string Jenis { get; set; }
            public string KategoriFilter { get; set; }
            public int Perubahan { get; set; }
            public int StokSebelum { get; set; }
            public int StokSesudah { get; set; }
            public int StokMinimum { get; set; }
            public string Keterangan { get; set; }
            public string Referensi { get; set; }
            public string CatatanDetail { get; set; }
        }

        private List<RiwayatStokItem> daftarRiwayat = new List<RiwayatStokItem>();
        private RiwayatStokItem itemTerpilih;
        private string filterJenisAktif = "Semua";
        private int indeksPeriode = 0;
        private readonly string[] daftarPeriode = { "7 hari terakhir", "30 hari terakhir", "Semua waktu" };
        private int halamanAktif = 1;
        private const int DATA_PER_HALAMAN = 7;
        private bool sudahDimuat = false;
        private bool sedangResetPlaceholder = false;

        private TextBox boxSearch;
        private Label lblPlaceholderSearch;
        private DataGridView gridRiwayat;
        private Label lblInfoCatatan;
        private Panel panelTabelContainer;
        private Panel panelDetailContainer;

        private const string PLACEHOLDER_SEARCH = "Cari nama produk, ID, atau kategori...";

        public FormRiwayatStok()
        {
            InitializeComponent();
            DeteksiKontrolOtomatis();
            HubungkanEventOtomatis();
            this.Load += FormRiwayatStok_Load;
            this.Resize += (s, e) => RapikanSemuaElemenUI();
            this.FormClosed += (s, e) => Application.Exit();
        }

        private IEnumerable<Control> SemuaKontrol(Control root)
        {
            foreach (Control c in root.Controls)
            {
                yield return c;
                foreach (Control child in SemuaKontrol(c))
                    yield return child;
            }
        }

        private T CariKontrol<T>(params string[] kandidatNama) where T : Control
        {
            foreach (string nama in kandidatNama)
            {
                var found = Controls.Find(nama, true).FirstOrDefault() as T;
                if (found != null) return found;
            }
            return null;
        }

        private void DeteksiKontrolOtomatis()
        {
            // Deteksi DataGridView utama meskipun namanya bukan dgvRiwayat
            gridRiwayat = CariKontrol<DataGridView>("dgvRiwayat", "dgvProduk")
                          ?? SemuaKontrol(this).OfType<DataGridView>().FirstOrDefault();

            if (gridRiwayat != null)
            {
                panelTabelContainer = gridRiwayat.Parent as Panel;
                if (panelTabelContainer != null)
                {
                    lblInfoCatatan = panelTabelContainer.Controls.OfType<Label>()
                        .FirstOrDefault(l => l.Top >= gridRiwayat.Bottom - 5 || l.Text.Contains("Menampilkan"));
                }
            }

            panelDetailContainer = CariKontrol<Panel>("pnlDetailPerubahan")
                ?? SemuaKontrol(this).OfType<Panel>()
                    .FirstOrDefault(p => p.Controls.OfType<Label>().Any(l => l.Text.Contains("Detail Perubahan")));

            var pnlSearch = CariKontrol<Panel>("pnlSearch");
            if (pnlSearch != null)
            {
                boxSearch = pnlSearch.Controls.OfType<TextBox>().FirstOrDefault();
                lblPlaceholderSearch = pnlSearch.Controls.OfType<Label>().FirstOrDefault();
            }
            if (boxSearch == null)
                boxSearch = CariKontrol<TextBox>("txtSearch");
        }

        private void FormRiwayatStok_Load(object sender, EventArgs e)
        {
            if (sudahDimuat) return;
            sudahDimuat = true;

            RapikanSemuaElemenUI();
            SiapkanSearchBar();
            AturTampilanTabel();
            IsiDataAwalFigma();
            TampilkanDataTabel();
            UpdateKartuStatistik();

            if (daftarRiwayat.Count > 0)
                TampilkanDetailPerubahan(daftarRiwayat[0]);
        }

        private void RapikanSemuaElemenUI()
        {
            // 1. Rapikan Sidebar: pastikan teks "Tambah Produk" dan "Riwayat Stok" tampil penuh & ikon Logout masuk ke kotak
            var pnlSidebar = CariKontrol<Panel>("pnlSidebar");
            if (pnlSidebar != null)
            {
                var tombolSidebar = pnlSidebar.Controls.OfType<Button>().OrderBy(b => b.Top).ToList();
                var ikonSidebar = pnlSidebar.Controls.OfType<PictureBox>()
                    .Where(p => p.Width <= 45 && p.Height <= 45)
                    .OrderBy(p => p.Top)
                    .ToList();

                foreach (var btn in tombolSidebar)
                {
                    btn.AutoSize = false;
                    btn.AutoEllipsis = false;
                    btn.Width = Math.Max(btn.Width, pnlSidebar.ClientSize.Width - 24);
                    btn.Left = (pnlSidebar.ClientSize.Width - btn.Width) / 2;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(40, 0, 4, 0);

                    if (btn.Name == "btnTambahProduk" || btn.Text.StartsWith("Tambah"))
                        btn.Text = "Tambah Produk";
                    else if (btn.Name == "btnRiwayatStok" || btn.Text.StartsWith("Riwayat"))
                    {
                        btn.Text = "Riwayat Stok";
                        btn.BackColor = Color.FromArgb(155, 184, 205);
                        btn.ForeColor = Color.White;
                        btn.Font = new Font(btn.Font.FontFamily, btn.Font.Size, FontStyle.Bold);
                        btn.FlatAppearance.BorderSize = 0;
                    }
                    else if (btn.Name != "btnLogout" && !btn.Text.Contains("Logout"))
                    {
                        btn.BackColor = Color.FromArgb(30, 58, 77);
                        btn.ForeColor = Color.FromArgb(226, 232, 240);
                    }
                }

                // Pasangkan setiap ikon kecil ke tombol sidebar terdekat secara vertikal (termasuk ikon Logout)
                foreach (var pic in ikonSidebar)
                {
                    var targetBtn = tombolSidebar
                        .OrderBy(b => Math.Abs((b.Top + b.Height / 2) - (pic.Top + pic.Height / 2)))
                        .FirstOrDefault();

                    if (targetBtn != null)
                    {
                        pic.Anchor = targetBtn.Anchor;
                        pic.BackColor = targetBtn.BackColor;
                        pic.SizeMode = PictureBoxSizeMode.Zoom;
                        pic.Size = new Size(18, 18);
                        pic.Location = new Point(
                            targetBtn.Left + 14,
                            targetBtn.Top + (targetBtn.Height - pic.Height) / 2
                        );
                        pic.BringToFront();

                        pic.Click -= IkonSidebar_Click;
                        pic.Tag = targetBtn;
                        pic.Click += IkonSidebar_Click;
                    }
                }
            }

            // 2. Rapikan Panel Kanan ("Detail Perubahan") & Tombol Bawahnya agar tidak terpotong di bawah layar
            if (panelDetailContainer != null && panelDetailContainer.Parent != null)
            {
                int batasBawahContent = panelDetailContainer.Parent.ClientSize.Height - 14;
                if (panelDetailContainer.Bottom > batasBawahContent)
                {
                    panelDetailContainer.Height = Math.Max(480, batasBawahContent - panelDetailContainer.Top);
                }

                var btnBatal = CariKontrol<Button>("btnBatalkanPerubahan")
                    ?? panelDetailContainer.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Batalkan"));
                var btnExport = CariKontrol<Button>("btnExportLaporan")
                    ?? panelDetailContainer.Controls.OfType<Button>().FirstOrDefault(b => b.Text.Contains("Export"));

                int marginX = 14;
                int lebarTombol = panelDetailContainer.ClientSize.Width - (marginX * 2);
                int tinggiTombol = 36;

                if (btnBatal != null)
                {
                    btnBatal.Size = new Size(lebarTombol, tinggiTombol);
                    btnBatal.Location = new Point(marginX, panelDetailContainer.ClientSize.Height - tinggiTombol - 12);
                    btnBatal.TextAlign = ContentAlignment.MiddleCenter;
                    btnBatal.Padding = new Padding(0);
                }

                if (btnExport != null)
                {
                    int posYExport = (btnBatal != null) ? (btnBatal.Top - tinggiTombol - 8) : (panelDetailContainer.ClientSize.Height - tinggiTombol - 12);
                    btnExport.Size = new Size(lebarTombol, tinggiTombol);
                    btnExport.Location = new Point(marginX, posYExport);
                    btnExport.TextAlign = ContentAlignment.MiddleCenter;
                    btnExport.Padding = new Padding(16, 0, 0, 0);

                    // Masukkan ikon download yang melayang ke dalam tombol Export Laporan
                    var picExport = panelDetailContainer.Controls.OfType<PictureBox>()
                        .Where(p => p.Width <= 35 && p.Top > 350)
                        .FirstOrDefault();

                    if (picExport != null)
                    {
                        picExport.BackColor = btnExport.BackColor;
                        picExport.SizeMode = PictureBoxSizeMode.Zoom;
                        picExport.Size = new Size(16, 16);
                        int teksW = TextRenderer.MeasureText(btnExport.Text, btnExport.Font).Width;
                        int iconX = btnExport.Left + Math.Max(14, (btnExport.Width - teksW) / 2 - 14);
                        picExport.Location = new Point(iconX, btnExport.Top + (btnExport.Height - 16) / 2);
                        picExport.BringToFront();
                    }
                }
            }

            // 3. Sejajarkan tombol "+ Catat Barang Masuk" agar tidak keluar dari batas kanan konten
            var btnCatat = CariKontrol<Button>("btnCatatBarangMasuk")
                ?? SemuaKontrol(this).OfType<Button>().FirstOrDefault(b => b.Text.Contains("Catat"));
            if (btnCatat != null && btnCatat.Parent != null)
            {
                btnCatat.AutoSize = false;
                btnCatat.Size = new Size(185, 38);
                btnCatat.TextAlign = ContentAlignment.MiddleCenter;
                btnCatat.Padding = new Padding(0);

                int batasKanan = (panelDetailContainer != null)
                    ? panelDetailContainer.Right
                    : (btnCatat.Parent.ClientSize.Width - 16);
                btnCatat.Left = batasKanan - btnCatat.Width;
            }

            // 4. Rapikan semua tombol filter di atas tabel agar sejajar 1 baris & tidak saling bertabrakan
            if (panelTabelContainer != null && gridRiwayat != null)
            {
                if (panelDetailContainer != null && panelTabelContainer.Right > panelDetailContainer.Left - 12)
                {
                    panelTabelContainer.Width = panelDetailContainer.Left - panelTabelContainer.Left - 14;
                }
                if (panelTabelContainer.Parent != null)
                {
                    int batasBawah = panelTabelContainer.Parent.ClientSize.Height - 14;
                    if (panelTabelContainer.Bottom > batasBawah)
                        panelTabelContainer.Height = Math.Max(480, batasBawah - panelTabelContainer.Top);
                }

                var tombolAtasTabel = panelTabelContainer.Controls.OfType<Button>()
                    .Where(b => b.Top < gridRiwayat.Top)
                    .OrderBy(b => b.Left)
                    .ToList();

                var btnPeriode = tombolAtasTabel.FirstOrDefault(b => b.Text.Contains("hari") || b.Text.Contains("waktu") || b.Name == "btnFilterPeriode");
                var tombolKategori = tombolAtasTabel.Where(b => b != btnPeriode).ToList();

                int curX = 14;
                int rowY = 12;
                foreach (var btn in tombolKategori)
                {
                    int lebarPas = TextRenderer.MeasureText(btn.Text, btn.Font).Width + 22;
                    btn.AutoSize = false;
                    btn.Location = new Point(curX, rowY);
                    btn.Size = new Size(lebarPas, 30);
                    btn.TextAlign = ContentAlignment.MiddleCenter;
                    btn.Padding = new Padding(0);
                    curX += lebarPas + 6;
                }

                if (btnPeriode != null)
                {
                    btnPeriode.AutoSize = false;
                    btnPeriode.Size = new Size(126, 30);
                    btnPeriode.Location = new Point(panelTabelContainer.ClientSize.Width - btnPeriode.Width - 14, rowY);
                    btnPeriode.TextAlign = ContentAlignment.MiddleCenter;
                    btnPeriode.Padding = new Padding(18, 0, 4, 0);

                    var picKalender = panelTabelContainer.Controls.OfType<PictureBox>()
                        .FirstOrDefault(p => p.Width <= 35 && p.Top < gridRiwayat.Top);
                    if (picKalender != null)
                    {
                        picKalender.BackColor = btnPeriode.BackColor;
                        picKalender.SizeMode = PictureBoxSizeMode.Zoom;
                        picKalender.Size = new Size(15, 15);
                        picKalender.Location = new Point(btnPeriode.Left + 8, btnPeriode.Top + (btnPeriode.Height - 15) / 2);
                        picKalender.BringToFront();
                    }
                }

                gridRiwayat.Top = rowY + 38;
                int areaBawah = 48;
                gridRiwayat.Height = Math.Max(200, panelTabelContainer.ClientSize.Height - gridRiwayat.Top - areaBawah);
            }
        }

        private void IkonSidebar_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pic && pic.Tag is Button btn)
                btn.PerformClick();
        }

        private bool IsPlaceholderSearch(string teks)
        {
            if (string.IsNullOrWhiteSpace(teks)) return true;
            string t = teks.Trim();
            return t == PLACEHOLDER_SEARCH || t.StartsWith("Cari nama produk");
        }

        private void SiapkanSearchBar()
        {
            if (boxSearch == null) return;

            boxSearch.ReadOnly = false;
            boxSearch.Enabled = true;
            sedangResetPlaceholder = true;
            if (lblPlaceholderSearch != null)
            {
                boxSearch.Clear();
                lblPlaceholderSearch.Visible = true;
            }
            else
            {
                boxSearch.Text = PLACEHOLDER_SEARCH;
                boxSearch.ForeColor = Color.FromArgb(100, 116, 139);
            }
            sedangResetPlaceholder = false;
        }

        private void HubungkanEventOtomatis()
        {
            var bTambahProduk = CariKontrol<Button>("btnTambahProduk")
                ?? SemuaKontrol(this).OfType<Button>().FirstOrDefault(b => b.Text.StartsWith("Tambah"));
            if (bTambahProduk != null)
            {
                bTambahProduk.Click += (s, e) =>
                {
                    FormTambahProduk frm = new FormTambahProduk();
                    frm.StartPosition = FormStartPosition.Manual;
                    frm.Location = this.Location;
                    frm.Size = this.Size;
                    frm.Show();
                    this.Hide();
                };
            }

            var bDashboard = CariKontrol<Button>("btnDashboard");
            if (bDashboard != null)
                bDashboard.Click += (s, e) => { new FormDashboard().Show(); this.Hide(); };

            var bAddKasir = CariKontrol<Button>("btnAddKasir");
            if (bAddKasir != null)
                bAddKasir.Click += (s, e) => { new FormTambahKasir().Show(); this.Hide(); };

            var bLogout = CariKontrol<Button>("btnLogout");
            if (bLogout != null)
            {
                bLogout.Click += (s, e) =>
                {
                    if (MessageBox.Show("Kembali ke halaman login?", "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        new FormLogin().Show();
                        this.Hide();
                    }
                };
            }

            // Hubungkan semua tombol filter di atas tabel berdasarkan teksnya
            if (panelTabelContainer != null && gridRiwayat != null)
            {
                var tombolFilter = panelTabelContainer.Controls.OfType<Button>()
                    .Where(b => b.Top < gridRiwayat.Top)
                    .ToList();

                foreach (var btn in tombolFilter)
                {
                    string teks = btn.Text.Trim();
                    if (teks.Contains("hari") || teks.Contains("waktu") || btn.Name == "btnFilterPeriode")
                    {
                        btn.Click += (s, e) =>
                        {
                            indeksPeriode = (indeksPeriode + 1) % daftarPeriode.Length;
                            btn.Text = daftarPeriode[indeksPeriode];
                            halamanAktif = 1;
                            TampilkanDataTabel();
                        };
                    }
                    else
                    {
                        string kategoriTarget = teks;
                        btn.Click += (s, e) => SetTombolFilterAktif(btn, kategoriTarget);
                    }
                }
            }

            var bCatat = CariKontrol<Button>("btnCatatBarangMasuk")
                ?? SemuaKontrol(this).OfType<Button>().FirstOrDefault(b => b.Text.Contains("Catat"));
            if (bCatat != null) bCatat.Click += BtnCatatBarangMasuk_Click;

            var bExport = CariKontrol<Button>("btnExportLaporan")
                ?? SemuaKontrol(this).OfType<Button>().FirstOrDefault(b => b.Text.Contains("Export"));
            if (bExport != null) bExport.Click += BtnExportLaporan_Click;

            var bBatal = CariKontrol<Button>("btnBatalkanPerubahan")
                ?? SemuaKontrol(this).OfType<Button>().FirstOrDefault(b => b.Text.Contains("Batalkan"));
            if (bBatal != null) bBatal.Click += BtnBatalkanPerubahan_Click;

            // Pagination
            var bPrev = CariKontrol<Button>("btnPrefProduk", "btnPrevRiwayat");
            if (bPrev != null) bPrev.Click += (s, e) => { if (halamanAktif > 1) { halamanAktif--; TampilkanDataTabel(); } };

            var bP1 = CariKontrol<Button>("btnPage1");
            if (bP1 != null) bP1.Click += (s, e) => { halamanAktif = 1; TampilkanDataTabel(); };

            var bP2 = CariKontrol<Button>("btnpage2", "btnPage2");
            if (bP2 != null) bP2.Click += (s, e) => { halamanAktif = 2; TampilkanDataTabel(); };

            var bP3 = CariKontrol<Button>("btnPage3");
            if (bP3 != null) bP3.Click += (s, e) => { halamanAktif = 3; TampilkanDataTabel(); };

            var bNext = CariKontrol<Button>("btnNextProduk", "btnNextRiwayat");
            if (bNext != null)
            {
                bNext.Click += (s, e) =>
                {
                    int totalHal = Math.Max(1, (int)Math.Ceiling((double)AmbilDataTerfilter().Count / DATA_PER_HALAMAN));
                    if (halamanAktif < totalHal) { halamanAktif++; TampilkanDataTabel(); }
                };
            }

            // Search Bar
            if (boxSearch != null)
            {
                if (lblPlaceholderSearch != null)
                    lblPlaceholderSearch.Click += (s, e) => { lblPlaceholderSearch.Visible = false; boxSearch.Focus(); };

                boxSearch.Enter += (s, e) =>
                {
                    if (lblPlaceholderSearch != null) lblPlaceholderSearch.Visible = false;
                    if (IsPlaceholderSearch(boxSearch.Text))
                    {
                        sedangResetPlaceholder = true;
                        boxSearch.Clear();
                        boxSearch.ForeColor = Color.FromArgb(30, 41, 59);
                        sedangResetPlaceholder = false;
                    }
                };

                boxSearch.MouseDown += (s, e) =>
                {
                    if (lblPlaceholderSearch != null) lblPlaceholderSearch.Visible = false;
                    if (IsPlaceholderSearch(boxSearch.Text))
                    {
                        sedangResetPlaceholder = true;
                        boxSearch.Clear();
                        boxSearch.ForeColor = Color.FromArgb(30, 41, 59);
                        sedangResetPlaceholder = false;
                    }
                };

                boxSearch.TextChanged += (s, e) =>
                {
                    if (sedangResetPlaceholder) return;
                    if (lblPlaceholderSearch != null)
                        lblPlaceholderSearch.Visible = string.IsNullOrEmpty(boxSearch.Text);
                    halamanAktif = 1;
                    TampilkanDataTabel();
                };

                boxSearch.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(boxSearch.Text))
                    {
                        sedangResetPlaceholder = true;
                        if (lblPlaceholderSearch != null)
                        {
                            boxSearch.Clear();
                            lblPlaceholderSearch.Visible = true;
                        }
                        else
                        {
                            boxSearch.Text = PLACEHOLDER_SEARCH;
                            boxSearch.ForeColor = Color.FromArgb(100, 116, 139);
                        }
                        sedangResetPlaceholder = false;
                    }
                };
            }

            if (gridRiwayat != null)
            {
                gridRiwayat.CellClick += GridRiwayat_CellClick;
                gridRiwayat.CellPainting += GridRiwayat_CellPainting;
            }
        }

        private void AturTampilanTabel()
        {
            if (gridRiwayat == null) return;

            gridRiwayat.AllowUserToAddRows = false;
            gridRiwayat.AllowUserToDeleteRows = false;
            gridRiwayat.ReadOnly = true;
            gridRiwayat.RowHeadersVisible = false;
            gridRiwayat.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridRiwayat.MultiSelect = false;

            gridRiwayat.RowTemplate.Height = 52;
            gridRiwayat.ColumnHeadersHeight = 38;
            gridRiwayat.EnableHeadersVisualStyles = false;
            gridRiwayat.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridRiwayat.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            gridRiwayat.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            gridRiwayat.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            gridRiwayat.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            gridRiwayat.DefaultCellStyle.BackColor = Color.White;
            gridRiwayat.DefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 250, 252);
            gridRiwayat.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            gridRiwayat.GridColor = Color.FromArgb(241, 245, 249);
            gridRiwayat.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        private void IsiDataAwalFigma()
        {
            daftarRiwayat = new List<RiwayatStokItem>
            {
                new RiwayatStokItem
                {
                    Id = "MVT-0112", TanggalSingkat = "12 Jun", Jam = "10:24",
                    WaktuLengkap = "12 June, 2026, 10:24", Petugas = "Habib (Kasir)",
                    NamaProduk = "Caffe Latte", Kategori = "Coffee", Satuan = "Cup",
                    Jenis = "Keluar", KategoriFilter = "Barang Keluar",
                    Perubahan = -2, StokSebelum = 40, StokSesudah = 38, StokMinimum = 5,
                    Keterangan = "Terjual (TRX-045)", Referensi = "TRX-045",
                    CatatanDetail = "Otomatis dari penjualan"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0111", TanggalSingkat = "12 Jun", Jam = "09:45",
                    WaktuLengkap = "12 June, 2026, 09:45", Petugas = "Habib (Kasir)",
                    NamaProduk = "Chai Tea Latte", Kategori = "Tea", Satuan = "Cup",
                    Jenis = "Keluar", KategoriFilter = "Barang Keluar",
                    Perubahan = -1, StokSebelum = 22, StokSesudah = 21, StokMinimum = 5,
                    Keterangan = "Terjual (TRX-044)", Referensi = "TRX-044",
                    CatatanDetail = "Otomatis dari penjualan"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0110", TanggalSingkat = "12 Jun", Jam = "08:10",
                    WaktuLengkap = "12 June, 2026, 08:10", Petugas = "Admin Gudang",
                    NamaProduk = "Croissant", Kategori = "Snack", Satuan = "Porsi",
                    Jenis = "Masuk", KategoriFilter = "Barang Masuk",
                    Perubahan = 24, StokSebelum = 12, StokSesudah = 36, StokMinimum = 5,
                    Keterangan = "Restock supplier", Referensi = "PO-2026-019",
                    CatatanDetail = "Restock rutin pagi hari"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0109", TanggalSingkat = "11 Jun", Jam = "16:40",
                    WaktuLengkap = "11 June, 2026, 16:40", Petugas = "Habib (Kasir)",
                    NamaProduk = "Dalgona Coffee", Kategori = "Coffee", Satuan = "Cup",
                    Jenis = "Keluar", KategoriFilter = "Penyesuaian",
                    Perubahan = -3, StokSebelum = 3, StokSesudah = 0, StokMinimum = 5,
                    Keterangan = "Rusak / kadaluarsa", Referensi = "ADJ-014",
                    CatatanDetail = "Bahan susu kedaluwarsa"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0108", TanggalSingkat = "11 Jun", Jam = "14:05",
                    WaktuLengkap = "11 June, 2026, 14:05", Petugas = "Admin",
                    NamaProduk = "Cappuccino", Kategori = "Coffee", Satuan = "Cup",
                    Jenis = "Masuk", KategoriFilter = "Produk Baru",
                    Perubahan = 35, StokSebelum = 0, StokSesudah = 35, StokMinimum = 5,
                    Keterangan = "Produk baru ditambahkan", Referensi = "PRD-005",
                    CatatanDetail = "Stok awal menu baru"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0107", TanggalSingkat = "11 Jun", Jam = "11:30",
                    WaktuLengkap = "11 June, 2026, 11:30", Petugas = "Admin",
                    NamaProduk = "Matcha Latte", Kategori = "Tea", Satuan = "Cup",
                    Jenis = "Keluar", KategoriFilter = "Penyesuaian",
                    Perubahan = -5, StokSebelum = 9, StokSesudah = 4, StokMinimum = 5,
                    Keterangan = "Penyesuaian stok", Referensi = "OPN-008",
                    CatatanDetail = "Selisih stock opname mingguan"
                },
                new RiwayatStokItem
                {
                    Id = "MVT-0106", TanggalSingkat = "10 Jun", Jam = "15:20",
                    WaktuLengkap = "10 June, 2026, 15:20", Petugas = "Admin Gudang",
                    NamaProduk = "Caffe Latte", Kategori = "Coffee", Satuan = "Cup",
                    Jenis = "Masuk", KategoriFilter = "Barang Masuk",
                    Perubahan = 30, StokSebelum = 12, StokSesudah = 42, StokMinimum = 5,
                    Keterangan = "Restock supplier", Referensi = "PO-2026-018",
                    CatatanDetail = "Pengiriman biji kopi & susu"
                }
            };
        }

        private List<RiwayatStokItem> AmbilDataTerfilter()
        {
            string keyword = "";
            if (boxSearch != null && !IsPlaceholderSearch(boxSearch.Text))
                keyword = boxSearch.Text.Trim().ToLower();

            return daftarRiwayat.Where(r =>
                (filterJenisAktif == "Semua" || r.KategoriFilter.Equals(filterJenisAktif, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(keyword) ||
                 r.Id.ToLower().Contains(keyword) ||
                 r.NamaProduk.ToLower().Contains(keyword) ||
                 r.Kategori.ToLower().Contains(keyword) ||
                 r.Keterangan.ToLower().Contains(keyword))
            ).ToList();
        }

        private void TampilkanDataTabel()
        {
            if (gridRiwayat == null) return;

            gridRiwayat.Rows.Clear();
            var hasilFilter = AmbilDataTerfilter();
            var dataHalaman = hasilFilter
                .Skip((halamanAktif - 1) * DATA_PER_HALAMAN)
                .Take(DATA_PER_HALAMAN)
                .ToList();

            foreach (var r in dataHalaman)
            {
                string jumlahTeks = r.Perubahan > 0 ? $"+{r.Perubahan}" : r.Perubahan.ToString();
                string stokTeks = $"{r.StokSebelum} → {r.StokSesudah}";
                gridRiwayat.Rows.Add(r.Id, r.TanggalSingkat, r.NamaProduk, r.Jenis, jumlahTeks, stokTeks, r.Keterangan, "Detail");
            }

            if (lblInfoCatatan != null)
            {
                int totalCatatan = Math.Max(86, 79 + hasilFilter.Count);
                lblInfoCatatan.Text = $"Menampilkan {dataHalaman.Count} dari {totalCatatan} catatan";
            }

            PerbaruiTombolHalaman();
        }

        private void PerbaruiTombolHalaman()
        {
            Button[] tombolHal = {
                CariKontrol<Button>("btnPage1"),
                CariKontrol<Button>("btnpage2", "btnPage2"),
                CariKontrol<Button>("btnPage3")
            };

            for (int i = 0; i < tombolHal.Length; i++)
            {
                if (tombolHal[i] == null) continue;
                bool aktif = (i + 1) == halamanAktif;
                tombolHal[i].BackColor = aktif ? Color.FromArgb(30, 58, 77) : Color.White;
                tombolHal[i].ForeColor = aktif ? Color.White : Color.FromArgb(100, 116, 139);
            }
        }

        private void UpdateKartuStatistik()
        {
            int tambahanMasuk = daftarRiwayat.Where(r => r.Perubahan > 0).Sum(r => r.Perubahan) - 89;
            int tambahanKeluar = daftarRiwayat.Where(r => r.Perubahan < 0).Sum(r => Math.Abs(r.Perubahan)) - 11;

            int totalMasuk = 312 + tambahanMasuk;
            int totalKeluar = 248 + tambahanKeluar;
            int hariIni = 8 + Math.Max(0, daftarRiwayat.Count - 7);
            int menipis = Math.Max(3, daftarRiwayat.Count(r => r.StokSesudah <= r.StokMinimum));

            var c1 = CariKontrol<Label>("lblValCard1");
            var c2 = CariKontrol<Label>("lblValCard2");
            var c3 = CariKontrol<Label>("lblValCard3");
            var c4 = CariKontrol<Label>("lblValCard4");

            if (c1 != null) c1.Text = $"+{totalMasuk}";
            if (c2 != null) c2.Text = $"-{totalKeluar}";
            if (c3 != null) c3.Text = hariIni.ToString();
            if (c4 != null) c4.Text = $"{menipis} produk";
        }

        private void SetTombolFilterAktif(Button tombolAktif, string filter)
        {
            filterJenisAktif = filter;
            halamanAktif = 1;

            if (panelTabelContainer != null && gridRiwayat != null)
            {
                var tombolFilter = panelTabelContainer.Controls.OfType<Button>()
                    .Where(b => b.Top < gridRiwayat.Top && !b.Text.Contains("hari") && !b.Text.Contains("waktu"))
                    .ToList();

                foreach (var btn in tombolFilter)
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.FromArgb(100, 116, 139);
                }
            }

            if (tombolAktif != null)
            {
                tombolAktif.BackColor = Color.FromArgb(30, 58, 77);
                tombolAktif.ForeColor = Color.White;
            }

            TampilkanDataTabel();
        }

        private void TampilkanDetailPerubahan(RiwayatStokItem item)
        {
            if (item == null) return;
            itemTerpilih = item;

            var lblId = CariKontrol<Label>("lblDetailId");
            if (lblId != null) lblId.Text = item.Id;

            var lblWaktu = CariKontrol<Label>("lblDetailWaktu");
            if (lblWaktu != null) lblWaktu.Text = $"Waktu: {item.WaktuLengkap}";

            var lblPetugas = CariKontrol<Label>("lblDetailPetugas");
            if (lblPetugas != null) lblPetugas.Text = $"Petugas: {item.Petugas}";

            var lblNama = CariKontrol<Label>("lblDetailNamaProduk");
            if (lblNama != null) lblNama.Text = item.NamaProduk;

            var lblKat = CariKontrol<Label>("lblDetailKategori");
            if (lblKat != null) lblKat.Text = $"{item.Kategori} • {item.Satuan}";

            var lblJenis = CariKontrol<Label>("lblDetailJenis");
            if (lblJenis != null)
            {
                bool masuk = item.Jenis == "Masuk";
                lblJenis.Text = masuk ? "↓ Masuk" : "↑ Keluar";
                lblJenis.BackColor = masuk ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                lblJenis.ForeColor = masuk ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
            }

            var lblSebelum = CariKontrol<Label>("lblDetailStokSebelum");
            if (lblSebelum != null) lblSebelum.Text = item.StokSebelum.ToString();

            var lblPerubahan = CariKontrol<Label>("lblDetailPerubahanVal");
            if (lblPerubahan != null)
            {
                lblPerubahan.Text = item.Perubahan > 0 ? $"+{item.Perubahan}" : item.Perubahan.ToString();
                lblPerubahan.ForeColor = item.Perubahan > 0 ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
            }

            var lblSesudah = CariKontrol<Label>("lblDetailStokSesudah");
            if (lblSesudah != null) lblSesudah.Text = item.StokSesudah.ToString();

            var lblRef = CariKontrol<Label>("lblDetailReferensi", "lblDetailreferensi");
            if (lblRef != null) lblRef.Text = item.Referensi;

            var lblCat = CariKontrol<Label>("lblDetailCatatan");
            if (lblCat != null) lblCat.Text = item.CatatanDetail;

            var lblStokMin = CariKontrol<Label>("lblInfoStokMin");
            if (lblStokMin != null)
            {
                if (item.StokSesudah <= 0)
                    lblStokMin.Text = $"Stok minimum {item.StokMinimum}. Stok habis, segera restock!";
                else if (item.StokSesudah <= item.StokMinimum)
                    lblStokMin.Text = $"Stok minimum {item.StokMinimum}. Stok mulai menipis!";
                else
                    lblStokMin.Text = $"Stok minimum {item.StokMinimum}. Stok masih aman.";
            }
        }

        private void GridRiwayat_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string idTerpilih = gridRiwayat.Rows[e.RowIndex].Cells[0].Value?.ToString();
            var item = daftarRiwayat.FirstOrDefault(r => r.Id == idTerpilih);
            if (item != null)
            {
                TampilkanDetailPerubahan(item);
            }
        }

        private void GridRiwayat_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string idRow = gridRiwayat.Rows[e.RowIndex].Cells[0].Value?.ToString();
            var item = daftarRiwayat.FirstOrDefault(r => r.Id == idRow);
            if (item == null) return;

            if (e.ColumnIndex == 1)
            {
                e.PaintBackground(e.CellBounds, true);
                Rectangle topRect = new Rectangle(e.CellBounds.Left + 6, e.CellBounds.Top + 7, e.CellBounds.Width - 10, 19);
                Rectangle botRect = new Rectangle(e.CellBounds.Left + 6, e.CellBounds.Top + 26, e.CellBounds.Width - 10, 18);

                TextRenderer.DrawText(e.Graphics, item.TanggalSingkat, new Font("Segoe UI", 9F, FontStyle.Regular),
                    topRect, Color.FromArgb(30, 41, 59), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(e.Graphics, item.Jam, new Font("Segoe UI", 8F, FontStyle.Regular),
                    botRect, Color.FromArgb(148, 163, 184), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 2)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int iconSize = 28;
                int iconX = e.CellBounds.Left + 6;
                int iconY = e.CellBounds.Top + (e.CellBounds.Height - iconSize) / 2;
                Rectangle iconRect = new Rectangle(iconX, iconY, iconSize, iconSize);

                using (GraphicsPath path = BuatRoundedRect(iconRect, 6))
                using (SolidBrush bg = new SolidBrush(Color.FromArgb(120, 82, 58)))
                {
                    e.Graphics.FillPath(bg, path);
                }

                TextRenderer.DrawText(e.Graphics, "☕", new Font("Segoe UI Emoji", 9.5F, FontStyle.Regular),
                    iconRect, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                Rectangle textRect = new Rectangle(iconX + iconSize + 10, e.CellBounds.Top,
                    Math.Max(20, e.CellBounds.Width - (iconSize + 20)), e.CellBounds.Height);

                TextRenderer.DrawText(e.Graphics, item.NamaProduk, e.CellStyle.Font,
                    textRect, Color.FromArgb(30, 41, 59), TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 3)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                bool masuk = item.Jenis == "Masuk";
                Color bgBadge = masuk ? Color.FromArgb(209, 250, 229) : Color.FromArgb(254, 226, 226);
                Color fgBadge = masuk ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
                string teksBadge = masuk ? "↓ Masuk" : "↑ Keluar";

                int badgeW = Math.Min(78, e.CellBounds.Width - 6);
                int badgeH = 24;
                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.Left + (e.CellBounds.Width - badgeW) / 2,
                    e.CellBounds.Top + (e.CellBounds.Height - badgeH) / 2,
                    badgeW, badgeH);

                using (GraphicsPath path = BuatRoundedRect(badgeRect, 12))
                using (SolidBrush b = new SolidBrush(bgBadge))
                {
                    e.Graphics.FillPath(b, path);
                }

                TextRenderer.DrawText(e.Graphics, teksBadge, new Font("Segoe UI", 8F, FontStyle.Bold),
                    badgeRect, fgBadge, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 4)
            {
                e.PaintBackground(e.CellBounds, true);
                bool masuk = item.Perubahan > 0;
                Color fgColor = masuk ? Color.FromArgb(21, 128, 61) : Color.FromArgb(185, 28, 28);
                string teksJumlah = masuk ? $"+{item.Perubahan}" : item.Perubahan.ToString();

                TextRenderer.DrawText(e.Graphics, teksJumlah, new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    e.CellBounds, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 5)
            {
                e.PaintBackground(e.CellBounds, true);
                string teksSebelum = $"{item.StokSebelum} → ";
                string teksSesudah = item.StokSesudah.ToString();

                Font fontNormal = new Font("Segoe UI", 9F, FontStyle.Regular);
                Font fontBold = new Font("Segoe UI", 9F, FontStyle.Bold);
                Size szSebelum = TextRenderer.MeasureText(teksSebelum, fontNormal);

                Rectangle rectSebelum = new Rectangle(e.CellBounds.Left + 6, e.CellBounds.Top, szSebelum.Width, e.CellBounds.Height);
                Rectangle rectSesudah = new Rectangle(rectSebelum.Right - 4, e.CellBounds.Top, e.CellBounds.Width - szSebelum.Width, e.CellBounds.Height);

                TextRenderer.DrawText(e.Graphics, teksSebelum, fontNormal, rectSebelum, Color.FromArgb(100, 116, 139), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(e.Graphics, teksSesudah, fontBold, rectSesudah, Color.FromArgb(30, 41, 59), TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 7)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int boxSize = 28;
                int boxX = e.CellBounds.Left + (e.CellBounds.Width - boxSize) / 2;
                int boxY = e.CellBounds.Top + (e.CellBounds.Height - boxSize) / 2;
                Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

                using (GraphicsPath path = BuatRoundedRect(boxRect, 7))
                using (SolidBrush bg = new SolidBrush(Color.White))
                using (Pen border = new Pen(Color.FromArgb(203, 213, 225), 1.3f))
                {
                    e.Graphics.FillPath(bg, path);
                    e.Graphics.DrawPath(border, path);
                }

                int cx = boxX + boxSize / 2;
                int cy = boxY + boxSize / 2;
                using (Pen eyePen = new Pen(Color.FromArgb(71, 85, 105), 1.5f))
                {
                    e.Graphics.DrawArc(eyePen, cx - 7, cy - 5, 14, 10, 0, 180);
                    e.Graphics.DrawArc(eyePen, cx - 7, cy - 5, 14, 10, 180, 180);
                    e.Graphics.DrawEllipse(eyePen, cx - 2, cy - 2, 4, 4);
                }

                e.Handled = true;
            }
        }

        private GraphicsPath BuatRoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnCatatBarangMasuk_Click(object sender, EventArgs e)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Catat Barang Masuk";
                dialog.Size = new Size(380, 320);
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.BackColor = Color.White;

                Label lbl1 = new Label { Text = "Pilih Produk:", Left = 20, Top = 20, AutoSize = true };
                ComboBox cmbProduk = new ComboBox { Left = 20, Top = 44, Width = 320, DropDownStyle = ComboBoxStyle.DropDownList };
                cmbProduk.Items.AddRange(new object[] { "Caffe Latte", "Chai Tea Latte", "Italian Affogato", "Dalgona Coffee", "Cappuccino", "Croissant", "Matcha Latte" });
                cmbProduk.SelectedIndex = 0;

                Label lbl2 = new Label { Text = "Jumlah Masuk:", Left = 20, Top = 82, AutoSize = true };
                NumericUpDown numJumlah = new NumericUpDown { Left = 20, Top = 106, Width = 320, Minimum = 1, Maximum = 1000, Value = 10 };

                Label lbl3 = new Label { Text = "Keterangan / Referensi:", Left = 20, Top = 144, AutoSize = true };
                TextBox txtKet = new TextBox { Left = 20, Top = 168, Width = 320, Text = "Restock supplier" };

                Button btnSimpan = new Button
                {
                    Text = "Simpan Stok Masuk",
                    Left = 180,
                    Top = 218,
                    Width = 160,
                    Height = 36,
                    BackColor = Color.FromArgb(30, 58, 77),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.OK
                };

                dialog.Controls.AddRange(new Control[] { lbl1, cmbProduk, lbl2, numJumlah, lbl3, txtKet, btnSimpan });
                dialog.AcceptButton = btnSimpan;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string namaTerpilih = cmbProduk.SelectedItem.ToString();
                    int jumlahMasuk = (int)numJumlah.Value;
                    var riwayatTerakhir = daftarRiwayat.FirstOrDefault(r => r.NamaProduk == namaTerpilih);
                    int stokAwal = riwayatTerakhir != null ? riwayatTerakhir.StokSesudah : 10;

                    int nomorBaru = 113 + daftarRiwayat.Count;
                    var baru = new RiwayatStokItem
                    {
                        Id = $"MVT-{nomorBaru:D4}",
                        TanggalSingkat = "12 Jun",
                        Jam = DateTime.Now.ToString("HH:mm"),
                        WaktuLengkap = $"12 June, 2026, {DateTime.Now:HH:mm}",
                        Petugas = "Admin Gudang",
                        NamaProduk = namaTerpilih,
                        Kategori = riwayatTerakhir?.Kategori ?? "Coffee",
                        Satuan = riwayatTerakhir?.Satuan ?? "Cup",
                        Jenis = "Masuk",
                        KategoriFilter = "Barang Masuk",
                        Perubahan = jumlahMasuk,
                        StokSebelum = stokAwal,
                        StokSesudah = stokAwal + jumlahMasuk,
                        StokMinimum = 5,
                        Keterangan = string.IsNullOrWhiteSpace(txtKet.Text) ? "Restock supplier" : txtKet.Text.Trim(),
                        Referensi = $"PO-2026-{nomorBaru}",
                        CatatanDetail = "Input manual barang masuk"
                    };

                    daftarRiwayat.Insert(0, baru);
                    halamanAktif = 1;
                    TampilkanDataTabel();
                    UpdateKartuStatistik();
                    TampilkanDetailPerubahan(baru);
                    MessageBox.Show($"Stok {namaTerpilih} berhasil ditambah +{jumlahMasuk}!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnExportLaporan_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Laporan Riwayat Stok";
                sfd.Filter = "File CSV (*.csv)|*.csv";
                sfd.FileName = "Laporan_Riwayat_Stok_Caffeku.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("ID,Waktu,Produk,Kategori,Jenis,Perubahan,Stok Sebelum,Stok Sesudah,Keterangan,Referensi,Petugas");
                    foreach (var r in AmbilDataTerfilter())
                    {
                        sb.AppendLine($"{r.Id},\"{r.WaktuLengkap}\",\"{r.NamaProduk}\",{r.Kategori},{r.Jenis},{r.Perubahan},{r.StokSebelum},{r.StokSesudah},\"{r.Keterangan}\",{r.Referensi},\"{r.Petugas}\"");
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Laporan riwayat stok berhasil di-export!", "Export Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void BtnBatalkanPerubahan_Click(object sender, EventArgs e)
        {
            if (itemTerpilih == null) return;

            if (MessageBox.Show($"Batalkan catatan mutasi '{itemTerpilih.Id}' ({itemTerpilih.NamaProduk})?",
                "Konfirmasi Pembatalan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                daftarRiwayat.Remove(itemTerpilih);
                TampilkanDataTabel();
                UpdateKartuStatistik();
                if (daftarRiwayat.Count > 0)
                    TampilkanDetailPerubahan(daftarRiwayat[0]);
            }
        }

        private void lblDetailJenis_Click(object sender, EventArgs e) { }
        private void lblDetailreferensi_Click(object sender, EventArgs e) { }
        private void btnExportLaporan_Click(object sender, EventArgs e) { BtnExportLaporan_Click(sender, e); }
        private void label6_Click(object sender, EventArgs e) { }
        private void lblPageTitle_Click(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void btnRiwayatStok_Click(object sender, EventArgs e) { }

        private void btnCatatBarangKeluar_Click(object sender, EventArgs e)
        {
            FormCatatBarangKeluar frm = new FormCatatBarangKeluar();
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = this.Location;
            frm.Size = this.Size;
            frm.Show();
            this.Hide();
        }
    }
}