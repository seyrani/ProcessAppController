using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;


namespace App_Controller
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            // simge durum kontrolü
            this.Resize += SetMinimizeState;
            // simgeye tıklayınca pencere durumunu tetikle.       
            notifyIcon1.Click += ToggleMinimizeState;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            xmlOlusturur();
            listele();
            listviewRenk();
            
            logListBox.DataSource = File.ReadAllLines(@"C:\Process Controller\LogsImageServerController.log");
            logListBox.Refresh();
                       
                       
        }


        // Formu minimize et normale dön
        private void ToggleMinimizeState(object sender, EventArgs e)
        {
            bool isMinimized = this.WindowState == FormWindowState.Minimized;
            this.WindowState = (isMinimized) ? FormWindowState.Normal : FormWindowState.Minimized;
        }

        // Simge durumu göster gizle
        private void SetMinimizeState(object sender, EventArgs e)
        {
            try
            {
                bool isMinimized = this.WindowState == FormWindowState.Minimized;

                this.ShowInTaskbar = !isMinimized;
                notifyIcon1.Visible = isMinimized;
                if (isMinimized) notifyIcon1.ShowBalloonTip(500, "'" + txtName.Text + "'", "Servis kontrolü çalışıyor ...", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                //Olay günlüğü [{0}]: [{1}] \r\n
                AppendToLog(string.Format("Olay günlüğü [{0}]: [{1}]", DateTime.Now, ex.Message));
                MessageBox.Show(ex.Message);
            }

        }

        //Tex box temizle
        private void ClearTextBox()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };

            func(Controls);
        }

        //Program çalıştığı anda XML oluşturur varsa kontrolünü yapar
        public void xmlOlusturur()
        {
            if (!File.Exists(@"data.xml"))
            {
                XmlTextWriter xmlOlustur = new XmlTextWriter(@"data.xml", null);
                xmlOlustur.WriteStartDocument();
                xmlOlustur.WriteComment("Process Controller - Seyrani DEMİREL");
                xmlOlustur.WriteStartElement("Uygulamalar");
                xmlOlustur.WriteEndDocument();
                xmlOlustur.Close();

            }
        }

        //listview data listeler
        public void listele()
        {
            if (File.Exists("data.xml"))
            {
                logTextBox.Items.Clear();

                XmlDocument doc = new XmlDocument();

                doc.Load("data.xml");
                XmlElement root = doc.DocumentElement;
                XmlNodeList kayitlar = root.SelectNodes("/Uygulamalar/Uygulama");

                foreach (XmlNode secilen in kayitlar)
                {
                    ListViewItem lv = new ListViewItem();
                    lv.Text = secilen["id"].InnerText;
                    lv.SubItems.Add(secilen["name"].InnerText);
                    lv.SubItems.Add(secilen["path"].InnerText);
                    logTextBox.Items.Add(lv);

                    if (comboBox1.Items.IndexOf(secilen["name"].InnerText) == -1)
                    {
                        comboBox1.Items.Add(secilen["name"].InnerText);
                    }


                }
            }
        }


        //listview renklendir
        public void listviewRenk()
        {
            ListView listView = this.logTextBox;
            int i = 0;
            Color shaded = Color.FromArgb(240, 240, 240);
            foreach (ListViewItem item in listView.Items)
            {
                if (i++ % 2 == 1)
                {
                    item.BackColor = Color.AliceBlue;
                    item.UseItemStyleForSubItems = true;
                }
            }
        }

        //Listview Kayıt Ekle
        public void veriKayitEt()
        {
            bool varmi = false;
            int sonID;

            XmlDocument doc = new XmlDocument();
            doc.Load("data.xml");
            XmlElement root = doc.DocumentElement;
            XmlNodeList kayitlar = root.SelectNodes("/Uygulamalar/Uygulama");
            if (kayitlar.Count > 0)
            {
                varmi = true;
            }

            if (varmi == true)
            {
                int[] kayittakiSayilar = new int[kayitlar.Count];

                int i = 0;
                foreach (XmlNode secilen in kayitlar)
                {
                    kayittakiSayilar[i] = Convert.ToInt32(secilen["id"].InnerText);
                    i = i + 1;
                }

                Array.Sort(kayittakiSayilar);
                sonID = kayittakiSayilar[kayittakiSayilar.Length - 1];
                sonID = sonID + 1;

            }
            else
            {
                sonID = 0;
            }

            if (File.Exists("data.xml"))
            {

                XmlElement UserElement = doc.CreateElement("Uygulama");

                XmlElement id = doc.CreateElement("id");
                id.InnerText = sonID.ToString();
                UserElement.AppendChild(id);

                XmlElement name = doc.CreateElement("name");
                name.InnerText = txtName.Text;
                UserElement.AppendChild(name);

                XmlElement path = doc.CreateElement("path");
                path.InnerText = txtPath.Text;
                UserElement.AppendChild(path);

                doc.DocumentElement.AppendChild(UserElement);

                XmlTextWriter xmleekle = new XmlTextWriter("data.xml", null);
                xmleekle.Formatting = Formatting.Indented;
                doc.WriteContentTo(xmleekle);
                xmleekle.Close();

            }
            listele();
            ClearTextBox();

        }


        string id = null;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (logTextBox.SelectedItems.Count > 0)
            {
                ListViewItem lv = logTextBox.SelectedItems[0];
                txtId.Text = lv.SubItems[0].Text;
                txtName.Text = lv.SubItems[1].Text;
                txtPath.Text = lv.SubItems[2].Text;
                id = lv.SubItems[0].Text;

            }
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {

            if (txtName.Text == String.Empty || txtPath.Text == String.Empty)
            {
                MessageBox.Show("Lütfen program adı ve yolunu giriniz.", "Bilgi", MessageBoxButtons.OK);
            }
            else
            {
                veriKayitEt();
            }
            
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            var mesajSil = MessageBox.Show("Seçilen kaydı silmek istediğinize emin misiniz?", "Uyarı", MessageBoxButtons.YesNoCancel);


            try
            {
                if (mesajSil == DialogResult.Yes)
                {
                    XDocument xDoc = XDocument.Load("data.xml");
                    XElement deletedElement = xDoc.Root.Elements().FirstOrDefault(xe => xe.Element("id").Value == id);

                    deletedElement.Remove();
                    xDoc.Save("data.xml");

                }

                listele();
            }
            catch (Exception)
            {
                MessageBox.Show("Silinecek kayıt yok veya seçilmemiş.");
            }
            ClearTextBox();
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            XDocument xDoc = XDocument.Load("data.xml");

            XElement currentElement = xDoc.Root.Elements().FirstOrDefault(xe => xe.Element("id").Value == id);

            currentElement.SetElementValue("name", txtName.Text);
            currentElement.SetElementValue("path", txtPath.Text);
            xDoc.Save("data.xml");

            ClearTextBox();
            listele();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

            logTextBox.Items.Clear();

            XmlDocument doc = new XmlDocument();

            doc.Load("data.xml");
            XmlElement root = doc.DocumentElement;
            XmlNodeList kayitlar = root.SelectNodes("/Uygulamalar/Uygulama");

            foreach (XmlNode secilen in kayitlar)
            {

                if (secilen["name"].InnerText == comboBox1.Text)
                {

                    ListViewItem lv = new ListViewItem();
                    lv.Text = secilen["id"].InnerText;
                    lv.SubItems.Add(secilen["name"].InnerText);
                    lv.SubItems.Add(secilen["path"].InnerText);
                    logTextBox.Items.Add(lv);

                }

            }
        }

        //loglama
        string logFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments), @"C:\Process Controller\LogsImageServerController.log");

        private void AppendToLog(string info)
        {
            try
            {
                if (File.Exists(logFilename))
                {
                    FileInfo fi = new FileInfo(logFilename);
                    if (fi.Length > 1024 * 1024)
                    {
                        File.WriteAllLines(logFilename, File.ReadAllLines(logFilename).Skip(500));
                    }
                }
                File.AppendAllText(logFilename, info + "\n");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }


        //uygulama başlat
        public void ProcessThreadAsync()
        {


            try
            {

                var path = txtPath.Text;
                Process process = new Process();

                while (true)
                {

                    FileInfo fi = new FileInfo(path);

                    ProcessStartInfo info = new ProcessStartInfo(path);
                    info.WorkingDirectory = fi.DirectoryName;

                    process.StartInfo = info;

                    process.Start();

                    Task.Factory.StartNew(() =>
           {
               int count = Convert.ToInt32(Math.Round(numUpDown.Value, 0));
               Thread.Sleep(count * 60 * 1000);
               process.Kill();

           });
                    process.WaitForExit();

                    // process.TotalProcessorTime cpu kullanım değeri 
                    AppendToLog(string.Format("Servis Restart Time: [{0}]: [{1}]", DateTime.Now, process.StartTime));


                }

            }
            catch (Exception ex)
            {
                AppendToLog(string.Format("Warning: [{0}]: [{1}]", DateTime.Now, ex.Message));
                MessageBox.Show(ex.Message);
            }

        }

      
        Thread baslat;
        private void btnStart_Click(object sender, EventArgs e)
        {
            baslat = new Thread(new ThreadStart(ProcessThreadAsync));
            baslat.Start();

        }

        private void btnStop_Click(object sender, EventArgs e)
        {

            try
            {
                Process.Start("taskkill", "/F /IM " + comboBox1.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                AppendToLog(string.Format("Warning: [{0}]: [{1}]", DateTime.Now, ex.Message));
                MessageBox.Show(ex.Message);
            }
        }


        private void sagTikMenu_Click(object sender, EventArgs e)
        {
            baslat.Abort();
        }
    }






}
