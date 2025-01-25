using System;
using System.Drawing;
using System.Windows.Forms;

namespace FloatingApp
{
    class Program : Form
    {
        private Point dragOffset;
        private Label timeLabel;

        public Program()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.FromArgb(40, 40, 40); // Dark background
            this.Size = new Size(120, 40); // Reduced height
            this.Opacity = 0.6;
            this.KeyPreview = true; // Enable key events
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - this.Width - 300, 5);

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
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Program());
        }
    }
}
