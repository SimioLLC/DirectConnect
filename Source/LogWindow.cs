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
    public partial class LogWindow : Form
    {
        public LogWindow()
        {
            InitializeComponent();
        }

        private void RefreshLogs()
        {
            textLogs.Text = Loggerton.Instance.GetLogs(EnumLogFlags.All);
        }

        private void LogWindow_Load(object sender, EventArgs e)
        {
            timerLogs.Enabled = true;
        }

        private void timerLogs_Tick(object sender, EventArgs e)
        {
            RefreshLogs();
        }

        private void textLogs_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            textLogs.Text = "";
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
