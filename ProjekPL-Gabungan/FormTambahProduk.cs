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
    public partial class FormTambahProduk : Form
    {
        public class ProdukItem
        {
            public string Id { get; set; }
            public string Nama { get; set; }
            public string Kategori { get; set; }
            public string Satuan { get; set; }
            public decimal Harga { get; set; }
            public int Stok { get; set; }
            public int StokMinimum { get; set; }
            public string Status { get; set; }
            public string Deskripsi { get; set; }
            public string GambarPath { get; set; }
        }

        private List<ProdukItem> daftarProduk = new List<ProdukItem>();
        private Dictionary<string, Image> cacheGambar = new Dictionary<string, Image>();
        private string kategoriAktif = "Semua";
        private string gambarTerpilihPath = "";
        private Panel panelDropzone;
        private PictureBox picPreview;
        private bool sudahDimuat = false;
        private bool sedangResetPlaceholder = false;
        private int halamanAktif = 1;
        private const int DATA_PER_HALAMAN = 7;

        private TextBox boxIdProduk;
        private TextBox boxNamaProduk;
        private TextBox boxHarga;
        private TextBox boxStokAwal;
        private TextBox boxStokMinimum;
        private TextBox boxDeskripsi;
        private TextBox boxSearch;
        private Label lblPlaceholderSearch;
        private ComboBox boxKategori;
        private ComboBox boxSatuan;
        private ComboBox boxStatus;

        private const string PLACEHOLDER_SEARCH = "Cari nama produk, ID, atau kategori...";
        private const string PLACEHOLDER_NAMA = "Nama produk";
        private const string PLACEHOLDER_DESKRIPSI = "Deskripsi singkat produk";

        public FormTambahProduk()
        {
            InitializeComponent();
            DeteksiSemuaKontrolOtomatis();
            SiapkanDropzoneDanPreview();
            HubungkanSemuaEventOtomatis();
            this.Load += FormTambahProduk_Load;
            this.Resize += (s, e) => RapikanPosisiIkonSidebar();
            this.FormClosed += (s, e) => Application.Exit();
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

        private void DeteksiSemuaKontrolOtomatis()
        {
            boxIdProduk = CariKontrol<TextBox>("txtIdProduk");
            boxNamaProduk = CariKontrol<TextBox>("txtNamaProduk");
            boxHarga = CariKontrol<TextBox>("txtHarga");
            boxStokAwal = CariKontrol<TextBox>("txtStokAwal");
            boxStokMinimum = CariKontrol<TextBox>("txtStokMinimum");
            boxDeskripsi = CariKontrol<TextBox>("txtDeskripsi");
            boxKategori = CariKontrol<ComboBox>("cmbKategori");
            boxSatuan = CariKontrol<ComboBox>("cmbSatuan");
            boxStatus = CariKontrol<ComboBox>("cmbStatus");

            var panelForm = CariKontrol<Panel>("pnlFormProduk");
            if (panelForm != null)
            {
                panelDropzone = CariKontrol<Panel>("pnlDropzone");
                if (panelDropzone == null)
                {
                    panelDropzone = panelForm.Controls.OfType<Panel>()
                        .OrderBy(p => p.Top)
                        .FirstOrDefault();
                }

                var semuaTextBox = panelForm.Controls.OfType<TextBox>()
                    .OrderBy(t => t.Top / 15)
                    .ThenBy(t => t.Left)
                    .ToList();

                if (semuaTextBox.Count >= 6)
                {
                    if (boxIdProduk == null) boxIdProduk = semuaTextBox[0];
                    if (boxNamaProduk == null) boxNamaProduk = semuaTextBox[1];
                    if (boxHarga == null) boxHarga = semuaTextBox[2];
                    if (boxStokAwal == null) boxStokAwal = semuaTextBox[3];
                    if (boxStokMinimum == null) boxStokMinimum = semuaTextBox[4];
                    if (boxDeskripsi == null) boxDeskripsi = semuaTextBox[5];
                }

                var semuaCombo = panelForm.Controls.OfType<ComboBox>()
                    .OrderBy(c => c.Top / 15)
                    .ThenBy(c => c.Left)
                    .ToList();

                if (semuaCombo.Count >= 3)
                {
                    if (boxKategori == null) boxKategori = semuaCombo[0];
                    if (boxSatuan == null) boxSatuan = semuaCombo[1];
                    if (boxStatus == null) boxStatus = semuaCombo[2];
                }
            }

            var pnlSearch = CariKontrol<Panel>("pnlSearch");
            if (pnlSearch != null)
            {
                boxSearch = pnlSearch.Controls.OfType<TextBox>().FirstOrDefault();
                lblPlaceholderSearch = pnlSearch.Controls.OfType<Label>().FirstOrDefault();
            }
            if (boxSearch == null)
                boxSearch = CariKontrol<TextBox>("txtSearch");
        }

        private void RapikanPosisiIkonSidebar()
        {
            string[] daftarTombolSidebar = { "btnDashboard", "btnAddKasir", "btnTambahProduk", "btnRiwayatStok", "btnLogout" };
            foreach (string namaBtn in daftarTombolSidebar)
            {
                var btn = CariKontrol<Button>(namaBtn);
                if (btn == null) continue;

                if (namaBtn == "btnTambahProduk")
                {
                    btn.BackColor = Color.FromArgb(155, 184, 205);
                    btn.ForeColor = Color.White;
                    btn.Font = new Font(btn.Font.FontFamily, btn.Font.Size, FontStyle.Bold);
                    btn.FlatAppearance.BorderSize = 0;
                }
                else if (namaBtn == "btnRiwayatStok" || namaBtn == "btnDashboard" || namaBtn == "btnAddKasir")
                {
                    btn.BackColor = Color.FromArgb(30, 58, 77);
                    btn.ForeColor = Color.FromArgb(226, 232, 240);
                }

                PasangkanIkonOtomatis(btn);
            }
        }

        private void PasangkanIkonOtomatis(Button btn)
        {
            if (btn == null || btn.Parent == null) return;

            int tengahBtnY = btn.Top + (btn.Height / 2);

            var pic = btn.Parent.Controls.OfType<PictureBox>()
                .Where(p => p.Width <= 45 && p.Height <= 45)
                .FirstOrDefault(p => p.Bounds.IntersectsWith(btn.Bounds) || Math.Abs((p.Top + p.Height / 2) - tengahBtnY) <= 32);

            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(44, 0, 8, 0);

            if (pic != null)
            {
                pic.Anchor = btn.Anchor;
                pic.BackColor = btn.BackColor;
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Size = new Size(18, 18);
                pic.Location = new Point(
                    btn.Left + 18,
                    btn.Top + (btn.Height - pic.Height) / 2
                );
                pic.BringToFront();

                pic.Click -= IkonSidebar_Click;
                pic.Tag = btn;
                pic.Click += IkonSidebar_Click;
            }
        }

        private void IkonSidebar_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pic && pic.Tag is Button btn)
            {
                btn.PerformClick();
            }
        }

        private void SiapkanDropzoneDanPreview()
        {
            if (panelDropzone == null) return;

            panelDropzone.AllowDrop = true;
            panelDropzone.Cursor = Cursors.Hand;
            panelDropzone.DragEnter += Dropzone_DragEnter;
            panelDropzone.DragDrop += Dropzone_DragDrop;
            panelDropzone.Click += BtnImportGambar_Click;

            foreach (Control child in panelDropzone.Controls)
            {
                child.Cursor = Cursors.Hand;
                child.Click += BtnImportGambar_Click;
            }

            picPreview = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Visible = false,
                AllowDrop = true,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(247, 249, 252)
            };
            picPreview.DragEnter += Dropzone_DragEnter;
            picPreview.DragDrop += Dropzone_DragDrop;
            picPreview.Click += BtnImportGambar_Click;

            panelDropzone.Controls.Add(picPreview);
            picPreview.BringToFront();
        }

        private void Dropzone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0 && CekFormatGambarValid(files[0]))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void Dropzone_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                MuatGambarKeForm(files[0]);
            }
        }

        private bool CekFormatGambarValid(string path)
        {
            string ext = Path.GetExtension(path)?.ToLower() ?? "";
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png";
        }

        private void MuatGambarKeForm(string filePath)
        {
            if (!File.Exists(filePath)) return;

            if (!CekFormatGambarValid(filePath))
            {
                MessageBox.Show("Format gambar harus JPG atau PNG!", "Format Tidak Didukung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FileInfo info = new FileInfo(filePath);
            if (info.Length > 2 * 1024 * 1024)
            {
                MessageBox.Show("Ukuran file gambar maksimal 2 MB!", "Ukuran Terlalu Besar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Image img = AmbilGambarAman(filePath);
            if (img != null)
            {
                gambarTerpilihPath = filePath;
                if (picPreview != null)
                {
                    picPreview.Image = img;
                    picPreview.Visible = true;
                    picPreview.BringToFront();
                }
            }
        }

        private Image AmbilGambarAman(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;

            if (cacheGambar.TryGetValue(path, out Image cached))
                return cached;

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    Image loaded = Image.FromStream(ms);
                    Image copy = new Bitmap(loaded);
                    cacheGambar[path] = copy;
                    return copy;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool IsPlaceholderSearch(string teks)
        {
            if (string.IsNullOrWhiteSpace(teks)) return true;
            string t = teks.Trim();
            return t == PLACEHOLDER_SEARCH || t.StartsWith("Cari nama produk");
        }

        private void HubungkanSemuaEventOtomatis()
        {
            var bImport = CariKontrol<Button>("btnImportGambar");
            if (bImport != null) bImport.Click += BtnImportGambar_Click;

            var bTambah = CariKontrol<Button>("btnTambah");
            if (bTambah != null) bTambah.Click += BtnTambah_Click;

            var bUpdate = CariKontrol<Button>("btnUpdate");
            if (bUpdate != null) bUpdate.Click += BtnUpdate_Click;

            var bHapus = CariKontrol<Button>("btnHapus");
            if (bHapus != null) bHapus.Click += BtnHapus_Click;

            var bBersihkan = CariKontrol<Button>("btnBersihkan");
            if (bBersihkan != null) bBersihkan.Click += BtnBersihkan_Click;

            var bCoffee = CariKontrol<Button>("btnFilterCoffee");
            if (bCoffee != null) bCoffee.Click += (s, e) => SetTombolFilterAktif(bCoffee, "Coffee");

            var bTea = CariKontrol<Button>("btnFilterTea");
            if (bTea != null) bTea.Click += (s, e) => SetTombolFilterAktif(bTea, "Tea");

            var bDessert = CariKontrol<Button>("btnFilterDessert");
            if (bDessert != null) bDessert.Click += (s, e) => SetTombolFilterAktif(bDessert, "Dessert");

            var bSnack = CariKontrol<Button>("btnFilterSnack");
            if (bSnack != null) bSnack.Click += (s, e) => SetTombolFilterAktif(bSnack, "Snack");

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

            if (boxNamaProduk != null)
            {
                boxNamaProduk.Enter += (s, e) =>
                {
                    if (boxNamaProduk.Text == PLACEHOLDER_NAMA) boxNamaProduk.Clear();
                    boxNamaProduk.ForeColor = Color.FromArgb(30, 41, 59);
                };
                boxNamaProduk.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(boxNamaProduk.Text))
                    {
                        boxNamaProduk.Text = PLACEHOLDER_NAMA;
                        boxNamaProduk.ForeColor = Color.FromArgb(100, 116, 139);
                    }
                };
            }

            if (boxDeskripsi != null)
            {
                boxDeskripsi.Enter += (s, e) =>
                {
                    if (boxDeskripsi.Text == PLACEHOLDER_DESKRIPSI) boxDeskripsi.Clear();
                    boxDeskripsi.ForeColor = Color.FromArgb(30, 41, 59);
                };
                boxDeskripsi.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(boxDeskripsi.Text))
                    {
                        boxDeskripsi.Text = PLACEHOLDER_DESKRIPSI;
                        boxDeskripsi.ForeColor = Color.FromArgb(100, 116, 139);
                    }
                };
            }

            if (dgvProduk != null)
            {
                dgvProduk.CellClick += DgvProduk_CellClick;
                dgvProduk.CellPainting += DgvProduk_CellPainting;
            }

            var bDash = CariKontrol<Button>("btnDashboard");
            if (bDash != null) bDash.Click += (s, e) => { new FormDashboard().Show(); this.Hide(); };

            var bKasir = CariKontrol<Button>("btnAddKasir");
            if (bKasir != null) bKasir.Click += (s, e) => { new FormTambahKasir().Show(); this.Hide(); };

            var bLogout = CariKontrol<Button>("btnLogout");
            if (bLogout != null) bLogout.Click += (s, e) =>
            {
                if (MessageBox.Show("Kembali ke halaman login?", "Konfirmasi Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    new FormLogin().Show();
                    this.Hide();
                }
            };

            HubungkanKlikIkonKeTombol("picImportGambar", "btnImportGambar");
            HubungkanKlikIkonKeTombol("picTambah", "btnTambah");
            HubungkanKlikIkonKeTombol("picUpdate", "btnUpdate");
            HubungkanKlikIkonKeTombol("picHapus", "btnHapus");
        }

        private void HubungkanKlikIkonKeTombol(string namaPic, string namaBtn)
        {
            var pic = CariKontrol<PictureBox>(namaPic);
            var btn = CariKontrol<Button>(namaBtn);
            if (pic != null && btn != null)
                pic.Click += (s, e) => btn.PerformClick();
        }

        private void FormTambahProduk_Load(object sender, EventArgs e)
        {
            if (sudahDimuat) return;
            sudahDimuat = true;

            RapikanPosisiIkonSidebar();
            PerbaikiKontrolInput();
            AturTampilanTabel();
            IsiDataAwalFigma();
            TampilkanDataTabel();
            UpdateKartuStatistik();
            BersihkanForm();
        }

        private void PerbaikiKontrolInput()
        {
            TextBox[] semuaInput = { boxIdProduk, boxNamaProduk, boxHarga, boxStokAwal, boxStokMinimum, boxDeskripsi };
            foreach (var tb in semuaInput)
            {
                if (tb != null)
                {
                    tb.ReadOnly = false;
                    tb.Enabled = true;
                    tb.BackColor = Color.White;
                    tb.ForeColor = Color.FromArgb(30, 41, 59);
                }
            }

            if (boxSearch != null)
            {
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

            if (boxKategori != null)
            {
                boxKategori.Items.Clear();
                boxKategori.Items.AddRange(new object[] { "Coffee", "Tea", "Dessert", "Snack" });
                boxKategori.DropDownStyle = ComboBoxStyle.DropDownList;
                boxKategori.ForeColor = Color.FromArgb(30, 41, 59);
                boxKategori.BackColor = Color.White;
                boxKategori.SelectedIndex = 0;
            }

            if (boxSatuan != null)
            {
                boxSatuan.Items.Clear();
                boxSatuan.Items.AddRange(new object[] { "Cup", "Porsi", "Botol", "Pack" });
                boxSatuan.DropDownStyle = ComboBoxStyle.DropDownList;
                boxSatuan.ForeColor = Color.FromArgb(30, 41, 59);
                boxSatuan.BackColor = Color.White;
                boxSatuan.SelectedIndex = 0;
            }

            if (boxStatus != null)
            {
                boxStatus.Items.Clear();
                boxStatus.Items.AddRange(new object[] { "Tersedia", "Menipis", "Habis" });
                boxStatus.DropDownStyle = ComboBoxStyle.DropDownList;
                boxStatus.ForeColor = Color.FromArgb(30, 41, 59);
                boxStatus.BackColor = Color.White;
                boxStatus.SelectedIndex = 0;
            }
        }

        private void AturTampilanTabel()
        {
            if (dgvProduk == null) return;

            dgvProduk.AllowUserToAddRows = false;
            dgvProduk.AllowUserToDeleteRows = false;
            dgvProduk.ReadOnly = true;
            dgvProduk.RowHeadersVisible = false;
            dgvProduk.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProduk.MultiSelect = false;

            dgvProduk.RowTemplate.Height = 48;
            dgvProduk.ColumnHeadersHeight = 38;
            dgvProduk.EnableHeadersVisualStyles = false;
            dgvProduk.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvProduk.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 116, 139);
            dgvProduk.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);

            dgvProduk.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            dgvProduk.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvProduk.DefaultCellStyle.BackColor = Color.White;
            dgvProduk.RowsDefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgvProduk.RowsDefaultCellStyle.BackColor = Color.White;
            dgvProduk.DefaultCellStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
            dgvProduk.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 41, 59);

            dgvProduk.GridColor = Color.FromArgb(241, 245, 249);
            dgvProduk.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            foreach (DataGridViewColumn col in dgvProduk.Columns)
            {
                col.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
                col.DefaultCellStyle.BackColor = Color.White;
            }
        }

        private void IsiDataAwalFigma()
        {
            daftarProduk = new List<ProdukItem>
            {
                new ProdukItem { Id = "PRD-001", Nama = "Caffe Latte", Kategori = "Coffee", Satuan = "Cup", Harga = 25000, Stok = 40, StokMinimum = 5, Status = "Tersedia", Deskripsi = "Espresso dengan susu steamed lembut." },
                new ProdukItem { Id = "PRD-002", Nama = "Chai Tea Latte", Kategori = "Tea", Satuan = "Cup", Harga = 21000, Stok = 22, StokMinimum = 5, Status = "Tersedia", Deskripsi = "Teh rempah hangat dengan susu creamy." },
                new ProdukItem { Id = "PRD-003", Nama = "Italian Affogato", Kategori = "Dessert", Satuan = "Porsi", Harga = 28000, Stok = 6, StokMinimum = 6, Status = "Menipis", Deskripsi = "Es krim vanilla disiram espresso panas." },
                new ProdukItem { Id = "PRD-004", Nama = "Dalgona Coffee", Kategori = "Coffee", Satuan = "Cup", Harga = 27000, Stok = 0, StokMinimum = 5, Status = "Habis", Deskripsi = "Kopi kocok lembut di atas susu dingin." },
                new ProdukItem { Id = "PRD-005", Nama = "Cappuccino", Kategori = "Coffee", Satuan = "Cup", Harga = 24000, Stok = 35, StokMinimum = 5, Status = "Tersedia", Deskripsi = "Espresso dengan foam susu tebal." },
                new ProdukItem { Id = "PRD-006", Nama = "Croissant", Kategori = "Snack", Satuan = "Porsi", Harga = 18000, Stok = 12, StokMinimum = 5, Status = "Tersedia", Deskripsi = "Pastry renyah berlapis mentega." },
                new ProdukItem { Id = "PRD-007", Nama = "Matcha Latte", Kategori = "Tea", Satuan = "Cup", Harga = 26000, Stok = 4, StokMinimum = 5, Status = "Menipis", Deskripsi = "Matcha premium Jepang dengan susu segar." }
            };
        }

        private string HitungStatusOtomatis(int stok, int stokMin)
        {
            if (stok <= 0) return "Habis";
            if (stok <= stokMin) return "Menipis";
            return "Tersedia";
        }

        private void TampilkanDataTabel()
        {
            if (dgvProduk == null) return;

            dgvProduk.Rows.Clear();

            string keyword = "";
            if (boxSearch != null && !IsPlaceholderSearch(boxSearch.Text))
            {
                keyword = boxSearch.Text.Trim().ToLower();
            }

            var hasilFilter = daftarProduk.Where(p =>
                (kategoriAktif == "Semua" || p.Kategori.Equals(kategoriAktif, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(keyword) ||
                 p.Id.ToLower().Contains(keyword) ||
                 p.Nama.ToLower().Contains(keyword) ||
                 p.Kategori.ToLower().Contains(keyword))
            ).ToList();

            var dataHalaman = hasilFilter
                .Skip((halamanAktif - 1) * DATA_PER_HALAMAN)
                .Take(DATA_PER_HALAMAN)
                .ToList();

            foreach (var p in dataHalaman)
            {
                string hargaFormat = "Rp " + p.Harga.ToString("N0", new CultureInfo("id-ID"));
                dgvProduk.Rows.Add(p.Id, p.Nama, p.Kategori, hargaFormat, p.Stok, p.Status, "Hapus");
            }

            if (lblInfoProduk != null)
            {
                lblInfoProduk.Text = $"Menampilkan {dataHalaman.Count} dari {hasilFilter.Count} produk";
            }

            PerbaruiTampilanTombolHalaman();
        }

        private void PerbaruiTampilanTombolHalaman()
        {
            Button[] tombolHal = { btnPage1, btnpage2, btnPage3 };
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
            int total = daftarProduk.Count;
            int tersedia = daftarProduk.Count(p => p.Status == "Tersedia");
            int menipis = daftarProduk.Count(p => p.Status == "Menipis");
            int habis = daftarProduk.Count(p => p.Status == "Habis");

            var card1 = CariKontrol<Label>("lblValCard1", "lblTotalProdukAngka", "lblCountTotal");
            var card2 = CariKontrol<Label>("lblValCard2", "lblTersediaAngka", "lblCountTersedia");
            var card3 = CariKontrol<Label>("lblValCard3", "lblMenipisAngka", "lblCountMenipis");
            var card4 = CariKontrol<Label>("lblValCard4", "lblHabisAngka", "lblCountHabis");

            if (card1 != null) card1.Text = total.ToString();
            if (card2 != null) card2.Text = tersedia.ToString();
            if (card3 != null) card3.Text = menipis.ToString();
            if (card4 != null) card4.Text = habis.ToString();
        }

        private string BuatIdBaru()
        {
            int maxNomor = 0;
            foreach (var p in daftarProduk)
            {
                if (p.Id.StartsWith("PRD-") && int.TryParse(p.Id.Substring(4), out int num))
                {
                    if (num > maxNomor) maxNomor = num;
                }
            }
            return $"PRD-{(maxNomor + 1):D3}";
        }

        private void BersihkanForm()
        {
            if (boxIdProduk != null)
            {
                boxIdProduk.Text = BuatIdBaru();
                boxIdProduk.ForeColor = Color.FromArgb(30, 41, 59);
            }
            if (boxNamaProduk != null)
            {
                boxNamaProduk.Text = PLACEHOLDER_NAMA;
                boxNamaProduk.ForeColor = Color.FromArgb(100, 116, 139);
            }
            if (boxKategori != null && boxKategori.Items.Count > 0) boxKategori.SelectedIndex = 0;
            if (boxSatuan != null && boxSatuan.Items.Count > 0) boxSatuan.SelectedIndex = 0;
            if (boxHarga != null) { boxHarga.Text = "0"; boxHarga.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStokAwal != null) { boxStokAwal.Text = "0"; boxStokAwal.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStokMinimum != null) { boxStokMinimum.Text = "5"; boxStokMinimum.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStatus != null && boxStatus.Items.Count > 0) boxStatus.SelectedIndex = 0;
            if (boxDeskripsi != null)
            {
                boxDeskripsi.Text = PLACEHOLDER_DESKRIPSI;
                boxDeskripsi.ForeColor = Color.FromArgb(100, 116, 139);
            }

            gambarTerpilihPath = "";
            if (picPreview != null)
            {
                picPreview.Image = null;
                picPreview.Visible = false;
            }
            if (dgvProduk != null) dgvProduk.ClearSelection();
        }

        private void BtnImportGambar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Pilih Gambar Produk";
                ofd.Filter = "File Gambar (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    MuatGambarKeForm(ofd.FileName);
                }
            }
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            if (boxNamaProduk == null || string.IsNullOrWhiteSpace(boxNamaProduk.Text) || boxNamaProduk.Text == PLACEHOLDER_NAMA)
            {
                MessageBox.Show("Nama produk wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                boxNamaProduk?.Focus();
                return;
            }

            string idBaru = (boxIdProduk != null && !string.IsNullOrWhiteSpace(boxIdProduk.Text))
                ? boxIdProduk.Text.Trim()
                : BuatIdBaru();

            if (daftarProduk.Any(p => p.Id.Equals(idBaru, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"ID Produk '{idBaru}' sudah digunakan. Silakan ubah ID Produk atau klik Bersihkan.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                boxIdProduk?.Focus();
                return;
            }

            decimal.TryParse((boxHarga?.Text ?? "0").Replace(".", "").Replace(",", ""), out decimal harga);
            int.TryParse(boxStokAwal?.Text ?? "0", out int stok);
            int.TryParse(boxStokMinimum?.Text ?? "5", out int stokMin);
            string status = HitungStatusOtomatis(stok, stokMin);

            daftarProduk.Add(new ProdukItem
            {
                Id = idBaru,
                Nama = boxNamaProduk.Text.Trim(),
                Kategori = boxKategori?.Text ?? "Coffee",
                Satuan = boxSatuan?.Text ?? "Cup",
                Harga = harga,
                Stok = stok,
                StokMinimum = stokMin,
                Status = status,
                Deskripsi = (boxDeskripsi == null || boxDeskripsi.Text == PLACEHOLDER_DESKRIPSI) ? "" : boxDeskripsi.Text.Trim(),
                GambarPath = gambarTerpilihPath
            });

            TampilkanDataTabel();
            UpdateKartuStatistik();
            BersihkanForm();
            MessageBox.Show("Produk berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            string idTarget = boxIdProduk?.Text?.Trim() ?? "";
            var produk = daftarProduk.FirstOrDefault(p => p.Id.Equals(idTarget, StringComparison.OrdinalIgnoreCase));
            if (produk == null)
            {
                MessageBox.Show("Klik salah satu produk di tabel terlebih dahulu untuk di-update.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (boxNamaProduk == null || string.IsNullOrWhiteSpace(boxNamaProduk.Text) || boxNamaProduk.Text == PLACEHOLDER_NAMA)
            {
                MessageBox.Show("Nama produk tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal.TryParse((boxHarga?.Text ?? "0").Replace(".", "").Replace(",", ""), out decimal harga);
            int.TryParse(boxStokAwal?.Text ?? "0", out int stok);
            int.TryParse(boxStokMinimum?.Text ?? "5", out int stokMin);

            produk.Nama = boxNamaProduk.Text.Trim();
            produk.Kategori = boxKategori?.Text ?? produk.Kategori;
            produk.Satuan = boxSatuan?.Text ?? produk.Satuan;
            produk.Harga = harga;
            produk.Stok = stok;
            produk.StokMinimum = stokMin;
            produk.Status = HitungStatusOtomatis(stok, stokMin);
            produk.Deskripsi = (boxDeskripsi == null || boxDeskripsi.Text == PLACEHOLDER_DESKRIPSI) ? "" : boxDeskripsi.Text.Trim();
            if (!string.IsNullOrEmpty(gambarTerpilihPath)) produk.GambarPath = gambarTerpilihPath;

            TampilkanDataTabel();
            UpdateKartuStatistik();
            BersihkanForm();
            MessageBox.Show("Data produk berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            string idTarget = boxIdProduk?.Text?.Trim() ?? "";
            var produk = daftarProduk.FirstOrDefault(p => p.Id.Equals(idTarget, StringComparison.OrdinalIgnoreCase));
            if (produk == null)
            {
                MessageBox.Show("Pilih produk dari tabel yang ingin dihapus.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Yakin ingin menghapus '{produk.Nama}'?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                daftarProduk.Remove(produk);
                TampilkanDataTabel();
                UpdateKartuStatistik();
                BersihkanForm();
            }
        }

        private void BtnBersihkan_Click(object sender, EventArgs e)
        {
            BersihkanForm();
        }

        private void SetTombolFilterAktif(Button tombolAktif, string kategori)
        {
            kategoriAktif = kategori;
            halamanAktif = 1;
            string[] namaFilter = { "btnFilterSemua", "btnFilterCoffee", "btnFilterTea", "btnFilterDessert", "btnFilterSnack" };

            foreach (string nama in namaFilter)
            {
                var btn = CariKontrol<Button>(nama);
                if (btn != null)
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

        private void DgvProduk_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string idTerpilih = dgvProduk.Rows[e.RowIndex].Cells[0].Value?.ToString();
            var produk = daftarProduk.FirstOrDefault(p => p.Id == idTerpilih);
            if (produk == null) return;

            if (boxIdProduk != null) { boxIdProduk.Text = produk.Id; boxIdProduk.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxNamaProduk != null)
            {
                boxNamaProduk.Text = produk.Nama;
                boxNamaProduk.ForeColor = Color.FromArgb(30, 41, 59);
            }
            if (boxKategori != null) boxKategori.Text = produk.Kategori;
            if (boxSatuan != null) boxSatuan.Text = produk.Satuan;
            if (boxHarga != null) { boxHarga.Text = produk.Harga.ToString("0"); boxHarga.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStokAwal != null) { boxStokAwal.Text = produk.Stok.ToString(); boxStokAwal.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStokMinimum != null) { boxStokMinimum.Text = produk.StokMinimum.ToString(); boxStokMinimum.ForeColor = Color.FromArgb(30, 41, 59); }
            if (boxStatus != null) boxStatus.Text = produk.Status;
            if (boxDeskripsi != null)
            {
                boxDeskripsi.Text = string.IsNullOrEmpty(produk.Deskripsi) ? PLACEHOLDER_DESKRIPSI : produk.Deskripsi;
                boxDeskripsi.ForeColor = string.IsNullOrEmpty(produk.Deskripsi) ? Color.FromArgb(100, 116, 139) : Color.FromArgb(30, 41, 59);
            }

            gambarTerpilihPath = produk.GambarPath ?? "";
            if (picPreview != null)
            {
                Image img = AmbilGambarAman(gambarTerpilihPath);
                if (img != null)
                {
                    picPreview.Image = img;
                    picPreview.Visible = true;
                    picPreview.BringToFront();
                }
                else
                {
                    picPreview.Image = null;
                    picPreview.Visible = false;
                }
            }

            if (e.ColumnIndex == dgvProduk.Columns.Count - 1)
            {
                BtnHapus_Click(sender, EventArgs.Empty);
            }
        }

        private void DgvProduk_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 1)
            {
                e.PaintBackground(e.CellBounds, true);

                string idRow = dgvProduk.Rows[e.RowIndex].Cells[0].Value?.ToString();
                string namaProduk = e.Value?.ToString() ?? "";
                var produk = daftarProduk.FirstOrDefault(p => p.Id == idRow);

                int thumbSize = 30;
                int thumbX = e.CellBounds.Left + 8;
                int thumbY = e.CellBounds.Top + (e.CellBounds.Height - thumbSize) / 2;
                Rectangle thumbRect = new Rectangle(thumbX, thumbY, thumbSize, thumbSize);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Image imgProduk = (produk != null) ? AmbilGambarAman(produk.GambarPath) : null;

                if (imgProduk != null)
                {
                    using (GraphicsPath clipPath = BuatRoundedRect(thumbRect, 6))
                    {
                        Region oldClip = e.Graphics.Clip;
                        e.Graphics.SetClip(clipPath);
                        e.Graphics.DrawImage(imgProduk, thumbRect);
                        e.Graphics.Clip = oldClip;
                    }
                    using (Pen borderPen = new Pen(Color.FromArgb(203, 213, 225), 1))
                    using (GraphicsPath borderPath = BuatRoundedRect(thumbRect, 6))
                    {
                        e.Graphics.DrawPath(borderPen, borderPath);
                    }
                }
                else
                {
                    using (GraphicsPath path = BuatRoundedRect(thumbRect, 6))
                    using (SolidBrush bg = new SolidBrush(Color.FromArgb(241, 245, 249)))
                    {
                        e.Graphics.FillPath(bg, path);
                    }
                    TextRenderer.DrawText(e.Graphics, "☕", new Font("Segoe UI Emoji", 10F, FontStyle.Regular),
                        thumbRect, Color.FromArgb(100, 116, 139), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }

                Rectangle textRect = new Rectangle(
                    thumbX + thumbSize + 8,
                    e.CellBounds.Top,
                    Math.Max(20, e.CellBounds.Width - (thumbSize + 20)),
                    e.CellBounds.Height);

                TextRenderer.DrawText(e.Graphics, namaProduk, e.CellStyle.Font, textRect,
                    Color.FromArgb(30, 41, 59), TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 4)
            {
                e.PaintBackground(e.CellBounds, true);
                int stok = Convert.ToInt32(e.Value ?? 0);
                string status = dgvProduk.Rows[e.RowIndex].Cells[5].Value?.ToString() ?? "Tersedia";

                Color barColor = Color.FromArgb(34, 197, 94);
                if (status == "Menipis") barColor = Color.FromArgb(234, 179, 8);
                else if (status == "Habis") barColor = Color.FromArgb(239, 68, 68);

                int barMaxW = Math.Max(28, e.CellBounds.Width - 42);
                int barH = 5;
                int barX = e.CellBounds.Left + 8;
                int barY = e.CellBounds.Top + (e.CellBounds.Height - barH) / 2;

                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(226, 232, 240)))
                    e.Graphics.FillRectangle(bgBrush, barX, barY, barMaxW, barH);

                int fillW = Math.Min(barMaxW, (int)(barMaxW * (Math.Min(stok, 40) / 40.0)));
                if (stok > 0 && fillW < 6) fillW = 6;
                using (SolidBrush fgBrush = new SolidBrush(barColor))
                    e.Graphics.FillRectangle(fgBrush, barX, barY, fillW, barH);

                TextRenderer.DrawText(e.Graphics, stok.ToString(), e.CellStyle.Font,
                    new Rectangle(barX + barMaxW + 6, e.CellBounds.Top, 32, e.CellBounds.Height),
                    Color.FromArgb(30, 41, 59), TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 5)
            {
                e.PaintBackground(e.CellBounds, true);
                string status = e.Value?.ToString() ?? "Tersedia";

                Color bgBadge = Color.FromArgb(209, 250, 229);
                Color fgBadge = Color.FromArgb(21, 128, 61);
                if (status == "Menipis")
                {
                    bgBadge = Color.FromArgb(254, 243, 199);
                    fgBadge = Color.FromArgb(180, 83, 9);
                }
                else if (status == "Habis")
                {
                    bgBadge = Color.FromArgb(254, 226, 226);
                    fgBadge = Color.FromArgb(185, 28, 28);
                }

                int badgeW = Math.Min(82, e.CellBounds.Width - 8);
                int badgeH = 24;
                Rectangle rect = new Rectangle(
                    e.CellBounds.Left + (e.CellBounds.Width - badgeW) / 2,
                    e.CellBounds.Top + (e.CellBounds.Height - badgeH) / 2,
                    badgeW, badgeH);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = BuatRoundedRect(rect, 12))
                using (SolidBrush b = new SolidBrush(bgBadge))
                {
                    e.Graphics.FillPath(b, path);
                }

                TextRenderer.DrawText(e.Graphics, "● " + status, new Font("Segoe UI", 8F, FontStyle.Bold),
                    rect, fgBadge, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.ColumnIndex == 6)
            {
                e.PaintBackground(e.CellBounds, true);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                int boxSize = 28;
                int boxX = e.CellBounds.Left + (e.CellBounds.Width - boxSize) / 2;
                int boxY = e.CellBounds.Top + (e.CellBounds.Height - boxSize) / 2;
                Rectangle boxRect = new Rectangle(boxX, boxY, boxSize, boxSize);

                using (GraphicsPath boxPath = BuatRoundedRect(boxRect, 7))
                using (SolidBrush bgBrush = new SolidBrush(Color.White))
                using (Pen borderPen = new Pen(Color.FromArgb(228, 174, 174), 1.4f))
                {
                    e.Graphics.FillPath(bgBrush, boxPath);
                    e.Graphics.DrawPath(borderPen, boxPath);
                }

                int cx = boxX + boxSize / 2;
                int cy = boxY + boxSize / 2;
                using (Pen trashPen = new Pen(Color.FromArgb(217, 65, 65), 1.6f))
                {
                    trashPen.StartCap = LineCap.Round;
                    trashPen.EndCap = LineCap.Round;
                    trashPen.LineJoin = LineJoin.Round;

                    e.Graphics.DrawRectangle(trashPen, cx - 2, cy - 6, 4, 2);
                    e.Graphics.DrawLine(trashPen, cx - 6, cy - 4, cx + 6, cy - 4);
                    Point[] bodyPoints = new Point[]
                    {
                        new Point(cx - 4, cy - 4),
                        new Point(cx - 3, cy + 5),
                        new Point(cx + 3, cy + 5),
                        new Point(cx + 4, cy - 4)
                    };
                    e.Graphics.DrawLines(trashPen, bodyPoints);
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

        // ================= EVENT BAWAAN DESIGNER & NAVIGASI =================

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void lblTambahProduk_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void lblTotalProduk_Click(object sender, EventArgs e) { }
        private void dgvProduk_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void lblInfoProduk_Click(object sender, EventArgs e) { }

        private void btnFilterSemua_Click(object sender, EventArgs e)
        {
            SetTombolFilterAktif(btnFilterSemua, "Semua");
        }

        private void btnPage1_Click(object sender, EventArgs e)
        {
            halamanAktif = 1;
            TampilkanDataTabel();
        }

        private void btnpage2_Click(object sender, EventArgs e)
        {
            halamanAktif = 2;
            TampilkanDataTabel();
        }

        private void btnPage3_Click(object sender, EventArgs e)
        {
            halamanAktif = 3;
            TampilkanDataTabel();
        }

        private void btnNextProduk_Click(object sender, EventArgs e)
        {
            int totalHalaman = Math.Max(1, (int)Math.Ceiling((double)daftarProduk.Count / DATA_PER_HALAMAN));
            if (halamanAktif < totalHalaman)
            {
                halamanAktif++;
                TampilkanDataTabel();
            }
        }

        private void btnPrefProduk_Click(object sender, EventArgs e)
        {
            if (halamanAktif > 1)
            {
                halamanAktif--;
                TampilkanDataTabel();
            }
        }

        private void btnRiwayatStok_Click(object sender, EventArgs e)
        {
            FormRiwayatStok frm = new FormRiwayatStok();
            frm.StartPosition = FormStartPosition.Manual;
            frm.Location = this.Location;
            frm.Size = this.Size;
            frm.Show();
            this.Hide();
        }

        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void pictureBox2_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void Form1_Load(object sender, EventArgs e) { }
        private void pnlSidebar_Paint(object sender, PaintEventArgs e) { }

        private void btnTambahProduk_Click(object sender, EventArgs e)
        {

        }
    }
}