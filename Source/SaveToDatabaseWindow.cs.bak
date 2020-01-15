using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectConnect
{
    public partial class SaveToDatabaseWindow : Form
    {

        public SaveToDatabaseWindow(String strCaption, String message)
        {
            InitializeComponent();

            TotalMillisecs = 5000;
            this.Text = strCaption;
            this.Refresh();
        }

        public bool SaveTables => cbSaveSimioTables.Checked;
        public bool SaveLogs => cbSaveSimioLogs.Checked;

        /// <summary>
        /// How many milliseconds to display
        /// </summary>
        private int TotalMillisecs { get; set; }

        /// <summary>
        /// How many millisecs remain until we close.
        /// </summary>
        private int RemainingMillisecs { get; set; }


        public SaveToDatabaseWindow()
        {
            InitializeComponent();
        }

        private void SaveToDatabaseWindow_Load(object sender, EventArgs e)
        {
            timerTop.Enabled = false;
            RemainingMillisecs = TotalMillisecs;
        }

        /// <summary>
        /// Timer tick.
        /// Keep us on top.
        /// If we are alert box, then reduce opacity until we are
        /// invisible, and then close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerTop_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.SetTopLevel(true);

            if ( this.Text == "Alert" )
            {
                RemainingMillisecs -= timerTop.Interval;

                this.Opacity -= ( (double) RemainingMillisecs / (double) TotalMillisecs);
                if ( RemainingMillisecs <= 0 )
                {
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }

        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

 
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ( keyData == (Keys.Control | Keys.H))
            {
                LogWindow form = new LogWindow();
                form.Show();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
