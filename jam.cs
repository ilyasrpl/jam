using System;
using System.Drawing;
using System.Windows.Forms;

namespace FloatingApp
{
    class Program : Form
    {
        private bool isDragging = false;
        private Point startPoint = new Point(0, 0);
        private Label timeLabel;
        private Timer timer;

        public Program()
        {
            // Konfigurasi utama form
            this.Text = "Floating App";
            this.FormBorderStyle = FormBorderStyle.None; // Hilangkan border
            this.TopMost = true; // Selalu di atas
            this.StartPosition = FormStartPosition.CenterScreen; // Mulai di tengah layar
            this.BackColor = Color.Aquamarine; // Warna latar belakang
            this.Size = new Size(300, 200); // Ukuran form
            this.Opacity = 0.9; // Transparansi 90%

            // Label untuk menampilkan waktu
            timeLabel = new Label();
            timeLabel.Font = new Font("Arial", 14, FontStyle.Bold); // Font custom
            timeLabel.AutoSize = true; // Sesuaikan ukuran label dengan teks
            timeLabel.ForeColor = Color.Black; // Warna teks
            timeLabel.Location = new Point((this.ClientSize.Width - timeLabel.Width) / 2, 50); // Posisi tengah
            timeLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(timeLabel);

            // Timer untuk memperbarui waktu setiap detik
            timer = new Timer();
            timer.Interval = 1000; // 1000 ms = 1 detik
            timer.Tick += Timer_Tick;
            timer.Start();

            // Tombol untuk menutup aplikasi
            Button closeButton = new Button();
            closeButton.Text = "Close";
            closeButton.Size = new Size(80, 30);
            closeButton.Location = new Point((this.ClientSize.Width - closeButton.Width) / 2, 120); // Posisi bawah
            closeButton.Click += (s, e) => Application.Exit();
            this.Controls.Add(closeButton);

            // Event handler untuk drag form
            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);

            // Update waktu pertama kali
            UpdateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            timeLabel.Text = DateTime.Now.ToString("HH:mm:ss"); // Format jam:menit:detik
            timeLabel.Location = new Point((this.ClientSize.Width - timeLabel.Width) / 2, 50); // Posisikan ulang agar tetap di tengah
        }

        // Event saat mouse ditekan
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            startPoint = new Point(e.X, e.Y);
        }

        // Event saat mouse digerakkan
        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
            }
        }

        // Event saat mouse dilepaskan
        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }
    }
}
