namespace DirectConnect
{
    partial class SaveToDatabaseWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveToDatabaseWindow));
            this.buttonOK = new System.Windows.Forms.Button();
            this.timerTop = new System.Windows.Forms.Timer(this.components);
            this.cbSaveSimioTables = new System.Windows.Forms.CheckBox();
            this.cbSaveSimioLogs = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(252, 125);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(118, 38);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // timerTop
            // 
            this.timerTop.Interval = 250;
            this.timerTop.Tick += new System.EventHandler(this.timerTop_Tick);
            // 
            // cbSaveSimioTables
            // 
            this.cbSaveSimioTables.AutoSize = true;
            this.cbSaveSimioTables.Checked = true;
            this.cbSaveSimioTables.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSaveSimioTables.Location = new System.Drawing.Point(28, 35);
            this.cbSaveSimioTables.Name = "cbSaveSimioTables";
            this.cbSaveSimioTables.Size = new System.Drawing.Size(236, 21);
            this.cbSaveSimioTables.TabIndex = 2;
            this.cbSaveSimioTables.Text = "Save Simio Tables to Database?";
            this.cbSaveSimioTables.UseVisualStyleBackColor = true;
            // 
            // cbSaveSimioLogs
            // 
            this.cbSaveSimioLogs.AutoSize = true;
            this.cbSaveSimioLogs.Checked = true;
            this.cbSaveSimioLogs.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSaveSimioLogs.Location = new System.Drawing.Point(28, 75);
            this.cbSaveSimioLogs.Name = "cbSaveSimioLogs";
            this.cbSaveSimioLogs.Size = new System.Drawing.Size(224, 21);
            this.cbSaveSimioLogs.TabIndex = 3;
            this.cbSaveSimioLogs.Text = "Save Simio Logs to Database?";
            this.cbSaveSimioLogs.UseVisualStyleBackColor = true;
            // 
            // SaveToDatabaseWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 176);
            this.ControlBox = false;
            this.Controls.Add(this.cbSaveSimioLogs);
            this.Controls.Add(this.cbSaveSimioTables);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SaveToDatabaseWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Save to Database?";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SaveToDatabaseWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Timer timerTop;
        private System.Windows.Forms.CheckBox cbSaveSimioTables;
        private System.Windows.Forms.CheckBox cbSaveSimioLogs;
    }
}