using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;

namespace FloatingApp
{
    class Program : Form
    {
        // Add constants
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;
        
        private Point dragOffset;
        private Label timeLabel;
        private Label networkLabel;  // Changed: single label for network stats
        private Timer networkTimer;
        private long lastBytesReceived = 0;
        private long lastBytesSent = 0;
        private bool showNetworkSpeed = false;  // Changed to false

        public Program()
        {

            // hide from navbar
            this.ShowInTaskbar = false;

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(40, 40, 40); // Dark background
            this.Size = new Size(120, 40); // Reduced height
            this.Opacity = 0.7;
            this.KeyPreview = true; // Enable key events
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width - 300, 5);

            // Set window style to tool window
            SetWindowLong(this.Handle, -20, GetWindowLong(this.Handle, -20) | 0x80);

            timeLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(30, 8), // Adjusted Y position
                ForeColor = Color.LightGray // Light text color
            };
            this.Controls.Add(timeLabel);

            var timer = new Timer { Interval = 1000 };
            timer.Tick += (s, e) => 
            {
                timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                timeLabel.Location = new Point((ClientSize.Width - timeLabel.Width) / 2, 8); // Centered horizontally
            };
            timer.Start();

            // Updated mouse drag events
            Action<object, MouseEventArgs> mouseDown = (s, e) => { dragOffset = e.Location; };
            Action<object, MouseEventArgs> mouseMove = (s, e) => 
            {
                if (e.Button == MouseButtons.Left)
                {
                    Point currentScreenPos = PointToScreen(e.Location);
                    Location = new Point(currentScreenPos.X - dragOffset.X, currentScreenPos.Y - dragOffset.Y);
                }
            };

            // Apply mouse events to both form and label
            this.MouseDown += new MouseEventHandler(mouseDown);
            this.MouseMove += new MouseEventHandler(mouseMove);
            timeLabel.MouseDown += new MouseEventHandler(mouseDown);
            timeLabel.MouseMove += new MouseEventHandler(mouseMove);

            // Add Esc key handler
            this.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Escape)
                    Application.Exit();
            };

            // Replace the timeLabel.Click handler with this MouseUp handler
            timeLabel.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    showNetworkSpeed = !showNetworkSpeed;
                    networkLabel.Visible = showNetworkSpeed;
                    this.Size = new Size(120, showNetworkSpeed ? networkLabel.Bottom + 5 : timeLabel.Bottom + 5);
                }
                else if (e.Button == MouseButtons.Left)
                {
                    // add button options here
                    Label options = new Label();
                    options.Text = "Show Network";
                    options.Location = new Point(timeLabel.Location.X, timeLabel.Bottom + 2);
                    this.Controls.Add(options);
                }
            };

            // Create and position network label
            networkLabel = new Label
            {
                Font = new Font("Arial", 8, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(5, timeLabel.Bottom + 2),
                Visible = false  // Changed to false
            };
            this.Controls.Add(networkLabel);

            // Adjust form height
            this.Size = new Size(120, timeLabel.Bottom + 5);  // Changed initial size

            // Setup timer
            networkTimer = new Timer();
            networkTimer.Interval = 1000;
            networkTimer.Tick += NetworkTimer_Tick;
            networkTimer.Start();
        }

        // Add WndProc override
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND && m.WParam.ToInt32() == SC_CLOSE)
            {
                return; // Ignore close command
            }
            base.WndProc(ref m);
        }

        // Add P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void NetworkTimer_Tick(object sender, EventArgs e)
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            long bytesReceived = 0;
            long bytesSent = 0;

            foreach (NetworkInterface ni in interfaces)
            {
            if (ni.OperationalStatus == OperationalStatus.Up)
            {
                bytesReceived += ni.GetIPv4Statistics().BytesReceived;
                bytesSent += ni.GetIPv4Statistics().BytesSent;
            }
            }

            double downloadSpeed = (bytesReceived - lastBytesReceived) / 1024.0;
            double uploadSpeed = (bytesSent - lastBytesSent) / 1024.0;

            // Format speeds adaptively
            string downloadText = downloadSpeed > 1024 ? 
                string.Format("{0:F1} MB/s", downloadSpeed / 1024) : 
                string.Format("{0:F1} KB/s", downloadSpeed);
            string uploadText = uploadSpeed > 1024 ? 
                string.Format("{0:F1} MB/s", uploadSpeed / 1024) : 
                string.Format("{0:F1} KB/s", uploadSpeed);
            
            networkLabel.Text = string.Format("{0} {1} {2} {3}", "↓", downloadText, "↑", uploadText);

            // networkLabel.Text = $"↓ {downloadText} ↑ {uploadText}";
            networkLabel.Location = new Point((ClientSize.Width - networkLabel.Width) / 2, timeLabel.Bottom + 2);

            lastBytesReceived = bytesReceived;
            lastBytesSent = bytesSent;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Program());
        }
    }
}
