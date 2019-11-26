using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DirectConnect
{
    public partial class StatusWindow : Form
    {

        private int TotalMillisecs { get; set; }

        private int RemainingMillisecs { get; set; }


        public StatusWindow(String strCaption, String message)
        {            
            InitializeComponent();

            TotalMillisecs = 5000;
            this.Text = strCaption;
            this.messageTextBox.Text = message;
            this.messageTextBox.Select(0, 0);
            this.Refresh();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StatusWindow_Load(object sender, EventArgs e)
        {
            timerTop.Enabled = true;
            RemainingMillisecs = TotalMillisecs;
            


        }

        private void timerTop_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.SetTopLevel(true);

            if (this.Text == "Exported")
            {
                RemainingMillisecs -= timerTop.Interval;

                this.Opacity = ((double)RemainingMillisecs / (double)TotalMillisecs);
                this.Opacity = Math.Max(this.Opacity, 0.3f);
                if (RemainingMillisecs <= 0)
                {
                    this.Close();
                }
            }

        }
    }
}
