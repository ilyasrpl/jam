using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Collections.Generic;

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
        private bool showColorPicker = false;

        // Add these fields at class level
        private ContextMenuStrip contextMenu;
        private ColorDialog colorDialog;
        private List<Label> colorBoxes = new List<Label>();

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
                    contextMenu.Show(timeLabel, e.Location);
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

            // Initialize context menu
            InitializeContextMenu();
            colorDialog = new ColorDialog();

            // Add menu items
            var changeColorItem = new ToolStripMenuItem("Change Color");

            changeColorItem.Click += (s, e) => {
                showColorPicker = !showColorPicker;
                
                // Remove existing color boxes
                foreach (var box in colorBoxes) {
                    this.Controls.Remove(box);
                }
                colorBoxes.Clear();
                
                if (!showColorPicker) {
                    this.Size = new Size(120, this.Size.Height - 20);
                    return;
                }
                
                this.Size = new Size(120, this.Size.Height + 20);
                var colors = new[] { Color.White, Color.Green, Color.Red, Color.Yellow, Color.Blue };
                int totalWidth = colors.Length * 20;
                int startX = (this.ClientSize.Width - totalWidth) / 2;

                for (int i = 0; i < colors.Length; i++) {
                    var colorBox = new Label {
                        BackColor = colors[i],
                        Location = new Point(startX + (i * 20), this.Size.Height - 15),
                        Size = new Size(10, 10)
                    };
                    colorBox.Click += (ls, le) => {
                        timeLabel.ForeColor = colorBox.BackColor;
                        networkLabel.ForeColor = colorBox.BackColor;
                    };
                    this.Controls.Add(colorBox);
                    colorBoxes.Add(colorBox); // Store reference to color box
                }
            };

            var showNetworkItem = new ToolStripMenuItem("Show Network");

            showNetworkItem.Click += (s, e) =>
            {
                showColorPicker = false;
                foreach (var box in colorBoxes) {
                    this.Controls.Remove(box);
                }
                colorBoxes.Clear();
                showNetworkSpeed = !showNetworkSpeed;
                networkLabel.Visible = showNetworkSpeed;
                this.Size = new Size(120, showNetworkSpeed ? networkLabel.Bottom + 5 : timeLabel.Bottom + 5);
                showNetworkItem.Text = showNetworkSpeed ? "Hide Network" : "Show Network";
            };

            contextMenu.Items.Add(changeColorItem);
            contextMenu.Items.Add(showNetworkItem);
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

        private class DarkContextMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                using (var brush = new SolidBrush(Color.FromArgb(180, 30, 30, 30)))
                {
                    e.Graphics.FillRectangle(brush, e.ConnectedArea);
                }
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (!e.Item.Selected) return;
                
                using (var brush = new SolidBrush(Color.FromArgb(100, 200, 200, 200)))
                {
                    e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Color.White;
                base.OnRenderItemText(e);
            }
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Renderer = new DarkContextMenuRenderer();
            contextMenu.BackColor = Color.FromArgb(180, 30, 30, 30);
            contextMenu.ForeColor = Color.White;
            contextMenu.Opacity = 0.95;

            // ...existing menu items code...

            foreach (ToolStripMenuItem item in contextMenu.Items)
            {
                item.BackColor = Color.Transparent;
                if (item.DropDownItems.Count > 0)
                {
                    foreach (ToolStripMenuItem subItem in item.DropDownItems)
                    {
                        subItem.BackColor = Color.Transparent;
                    }
                }
            }
        }
    }
}
