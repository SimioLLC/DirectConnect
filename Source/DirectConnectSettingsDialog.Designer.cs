namespace DirectConnect
{
    partial class DirectConnectSettingsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DirectConnectSettingsDialog));
            this.isStoredProcedureCheckbox = new System.Windows.Forms.CheckBox();
            this.directConnectGridDataSettingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tableViewOrSPNameLabel = new System.Windows.Forms.Label();
            this.tableViewOrSPNameTextBox = new System.Windows.Forms.TextBox();
            this.previewButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.directConnectGridDataSettingsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // isStoredProcedureCheckbox
            // 
            this.isStoredProcedureCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.isStoredProcedureCheckbox.AutoSize = true;
            this.isStoredProcedureCheckbox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.isStoredProcedureCheckbox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.directConnectGridDataSettingsBindingSource, "IsStoredProcedure", true));
            this.isStoredProcedureCheckbox.Location = new System.Drawing.Point(487, 16);
            this.isStoredProcedureCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.isStoredProcedureCheckbox.Name = "isStoredProcedureCheckbox";
            this.isStoredProcedureCheckbox.Size = new System.Drawing.Size(164, 21);
            this.isStoredProcedureCheckbox.TabIndex = 28;
            this.isStoredProcedureCheckbox.Text = "Is Stored Procedure?";
            this.isStoredProcedureCheckbox.UseVisualStyleBackColor = true;
            this.isStoredProcedureCheckbox.CheckedChanged += new System.EventHandler(this.isStoredProcedureCheckbox_CheckedChanged);
            // 
            // tableViewOrSPNameLabel
            // 
            this.tableViewOrSPNameLabel.AutoSize = true;
            this.tableViewOrSPNameLabel.Location = new System.Drawing.Point(10, 18);
            this.tableViewOrSPNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.tableViewOrSPNameLabel.Name = "tableViewOrSPNameLabel";
            this.tableViewOrSPNameLabel.Size = new System.Drawing.Size(142, 17);
            this.tableViewOrSPNameLabel.TabIndex = 25;
            this.tableViewOrSPNameLabel.Text = "&Table Or View Name:";
            // 
            // tableViewOrSPNameTextBox
            // 
            this.tableViewOrSPNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableViewOrSPNameTextBox.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.directConnectGridDataSettingsBindingSource, "TableOrViewName", true));
            this.tableViewOrSPNameTextBox.Location = new System.Drawing.Point(160, 15);
            this.tableViewOrSPNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.tableViewOrSPNameTextBox.Name = "tableViewOrSPNameTextBox";
            this.tableViewOrSPNameTextBox.Size = new System.Drawing.Size(298, 22);
            this.tableViewOrSPNameTextBox.TabIndex = 26;
            // 
            // previewButton
            // 
            this.previewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewButton.Location = new System.Drawing.Point(316, 61);
            this.previewButton.Margin = new System.Windows.Forms.Padding(4);
            this.previewButton.Name = "previewButton";
            this.previewButton.Size = new System.Drawing.Size(100, 28);
            this.previewButton.TabIndex = 27;
            this.previewButton.Text = "&Preview";
            this.previewButton.UseVisualStyleBackColor = true;
            this.previewButton.Click += new System.EventHandler(this.previewButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(551, 61);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 24;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(430, 61);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 23;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(4, 19);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(4);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(640, 449);
            this.dataGridView1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Location = new System.Drawing.Point(13, 97);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(648, 472);
            this.groupBox1.TabIndex = 29;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Data preview";
            // 
            // DirectConnectSettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(698, 601);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.isStoredProcedureCheckbox);
            this.Controls.Add(this.tableViewOrSPNameLabel);
            this.Controls.Add(this.tableViewOrSPNameTextBox);
            this.Controls.Add(this.previewButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "DirectConnectSettingsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DirectConnectSettingsDialog";
            ((System.ComponentModel.ISupportInitialize)(this.directConnectGridDataSettingsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox isStoredProcedureCheckbox;
        private System.Windows.Forms.Label tableViewOrSPNameLabel;
        private System.Windows.Forms.TextBox tableViewOrSPNameTextBox;
        private System.Windows.Forms.Button previewButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.BindingSource directConnectGridDataSettingsBindingSource;
    }
}