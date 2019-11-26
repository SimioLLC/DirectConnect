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
    public partial class DirectConnectSettingsDialog : Form
    {
        public DirectConnectSettingsDialog()
        {
            InitializeComponent();
        }

        internal void SetSettings(DirectConnectGridDataSettings settings)
        {
            directConnectGridDataSettingsBindingSource.DataSource = settings;
        }

        private void previewButton_Click(object sender, EventArgs e)
        {
            try
            {
                var ds = DirectConnectUtils.GetDataSet(tableViewOrSPNameTextBox.Text, isStoredProcedureCheckbox.Checked);
                dataGridView1.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                string errString = String.Format("Error Connecting To {0}...{1}", tableViewOrSPNameTextBox.Text, ex.Message);
                Alert(errString);
            }
        }

        private void Alert(string message)
        {
            MessageBox.Show(message);
        }

        private void isStoredProcedureCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (isStoredProcedureCheckbox.Checked)
            {
                tableViewOrSPNameLabel.Text = "&Stored Procedure";
            }
            else
            {
                tableViewOrSPNameLabel.Text = "&Table Or View Name";
            }
        }
    }
}
