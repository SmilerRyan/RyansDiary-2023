using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RyansDiary
{
    public partial class Form1 : Form {
        public Boolean TypingMode = false;
        public DateTime LastNote;

        public Form1() {
            InitializeComponent();
        }

        private void SaveToLog() {
            string path = @"[Diary] " + LastNote.ToString("yyyy MMMM d") + ".txt";
            using (StreamWriter sw = File.AppendText(path)) {
                sw.WriteLine("[" + DateTime.Now.ToLongDateString() + " at " + DateTime.Now.ToLongTimeString() + "] " + textBox1.Text);
            }
            textBox1.Clear();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            // If the note has been filled out, save the note.
            // If not holding control (force quit key), cancel the event.
            Form1_Deactivate(null, null);
            e.Cancel = !ModifierKeys.HasFlag(Keys.Control);
        }

        private void Form1_Activated(object sender, EventArgs e) {
            if (TypingMode == false) {
                // Move to bottom right, pixels extra down.
                Rectangle workingArea = Screen.GetWorkingArea(this);
                this.Location = new Point(workingArea.Right - 340, workingArea.Bottom - 340 + 300);
                for (int i = 0; i < 300; i++) { this.Location = new Point(this.Location.X, this.Location.Y - 1); }
                this.TopMost = true;
                TypingMode = true;
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e) {
            if (textBox1.TextLength >= 1) {
                LastNote = DateTime.Now;
                try { SaveToLog(); } catch(Exception ex) { MessageBox.Show(ex.Message, "Error"); }
            }

            if (TypingMode == true) {
                this.TopMost = false;
                Rectangle workingArea = Screen.GetWorkingArea(this);
                this.Location = new Point(workingArea.Right - 340, workingArea.Bottom - 340);
                for (int i = 0; i < 300; i++) { this.Location = new Point(this.Location.X, this.Location.Y + 1); }
                TypingMode = false;
            }
        }

        protected override void WndProc(ref Message message)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

            switch (message.Msg)
            {
                case WM_SYSCOMMAND:
                    int command = message.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                        return;
                    break;
            }

            base.WndProc(ref message);
        }

        public string GetRelativeDateTime(DateTime date) {
            TimeSpan ts = DateTime.Now - date;
            if (ts.TotalSeconds < 5) return "just now";
            if (ts.TotalMinutes < 1) return (int)ts.TotalSeconds + " seconds ago";
            if (ts.TotalHours < 1) return (int)ts.TotalMinutes == 1 ? "a minute ago" : (int)ts.TotalMinutes + " minutes ago";
            if (ts.TotalDays < 1) return (int)ts.TotalHours == 1 ? "an hour ago" : (int)ts.TotalHours + " hours ago";
            if (ts.TotalDays < 7) return (int)ts.TotalDays == 1 ? "yesterday" : (int)ts.TotalDays + " days ago";
            if (ts.TotalDays < 30.4368) return (int)(ts.TotalDays / 7) == 1 ? "last Week" : (int)(ts.TotalDays / 7) + " weeks ago";
            if (ts.TotalDays < 365.242) return (int)(ts.TotalDays / 30.4368) == 1 ? "a month ago" : (int)(ts.TotalDays / 30.4368) + " months ago";
            return (int)(ts.TotalDays / 365.242) == 1 ? "a year ago" : (int)(ts.TotalDays / 365.242) + " years ago";
        }

        private void Timer1_Tick(object sender, EventArgs e) {
            var relativeTimeAgo = GetRelativeDateTime(LastNote); // DateTime.Now.Subtract(LastNote).TotalMinutes;
            this.Text = "Diary: Last message " + relativeTimeAgo + ".";
            if(IdleTime.AsTimeSpan.TotalMinutes > 5) {
                this.Activate();
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            LastNote = DateTime.Now;
            Activate();
            Form1_Activated(null, null);
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) {
                Form1_Deactivate(null, null);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }

    public static class IdleTime {
        public static TimeSpan AsTimeSpan {
            get { return TimeSpan.FromMilliseconds(InMilliseconds); }
        }

        public static uint InMilliseconds {
            get {
                var lii = new LASTINPUTINFO { cbSize = SizeOfLASTINPUTINFO };
                if (!GetLastInputInfo(ref lii)) return 0;
                return unchecked((uint)Environment.TickCount - lii.dwTime);
            }
        }

        #region p/Invoke

        [DllImport("User32.dll")]
        extern static bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        static readonly uint SizeOfLASTINPUTINFO = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

        #endregion
    }

}
