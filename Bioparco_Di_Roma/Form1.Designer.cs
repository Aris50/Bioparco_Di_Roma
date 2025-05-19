namespace Bioparco_Di_Roma
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            parentDataGridView = new DataGridView();
            btnConnect = new Button();
            btnAddChild = new Button();
            btnEditChild = new Button();
            btnDeleteChild = new Button();
            txtConnectionString = new TextBox();
            lblStatus = new Label();
            lblConnectionString = new Label();
            lblParent = new Label();
            lblChild = new Label();
            splitContainer = new SplitContainer();
            childDataGridView = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)parentDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)childDataGridView).BeginInit();
            SuspendLayout();
            // 
            // parentDataGridView
            // 
            parentDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            parentDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            parentDataGridView.Location = new Point(3, 25);
            parentDataGridView.Name = "parentDataGridView";
            parentDataGridView.Size = new Size(770, 150);
            parentDataGridView.TabIndex = 0;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(12, 71);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(75, 23);
            btnConnect.TabIndex = 2;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnAddChild
            // 
            btnAddChild.Location = new Point(93, 71);
            btnAddChild.Name = "btnAddChild";
            btnAddChild.Size = new Size(75, 23);
            btnAddChild.TabIndex = 4;
            btnAddChild.Text = "Add Animal";
            btnAddChild.UseVisualStyleBackColor = true;
            // 
            // btnEditChild
            // 
            btnEditChild.Location = new Point(174, 71);
            btnEditChild.Name = "btnEditChild";
            btnEditChild.Size = new Size(75, 23);
            btnEditChild.TabIndex = 5;
            btnEditChild.Text = "Edit Animal";
            btnEditChild.UseVisualStyleBackColor = true;
            // 
            // btnDeleteChild
            // 
            btnDeleteChild.Location = new Point(255, 71);
            btnDeleteChild.Name = "btnDeleteChild";
            btnDeleteChild.Size = new Size(75, 23);
            btnDeleteChild.TabIndex = 6;
            btnDeleteChild.Text = "Delete Animal";
            btnDeleteChild.UseVisualStyleBackColor = true;
            // 
            // txtConnectionString
            // 
            txtConnectionString.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtConnectionString.Location = new Point(12, 25);
            txtConnectionString.Name = "txtConnectionString";
            txtConnectionString.Size = new Size(776, 23);
            txtConnectionString.TabIndex = 7;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(713, 75);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(79, 15);
            lblStatus.TabIndex = 8;
            lblStatus.Text = "Disconnected";
            // 
            // lblConnectionString
            // 
            lblConnectionString.AutoSize = true;
            lblConnectionString.Location = new Point(12, 7);
            lblConnectionString.Name = "lblConnectionString";
            lblConnectionString.Size = new Size(106, 15);
            lblConnectionString.TabIndex = 9;
            lblConnectionString.Text = "Connection String:";
            // 
            // lblParent
            // 
            lblParent.AutoSize = true;
            lblParent.Location = new Point(3, 7);
            lblParent.Name = "lblParent";
            lblParent.Size = new Size(44, 15);
            lblParent.TabIndex = 10;
            lblParent.Text = "Parent:";
            // 
            // lblChild
            // 
            lblChild.AutoSize = true;
            lblChild.Location = new Point(3, 7);
            lblChild.Name = "lblChild";
            lblChild.Size = new Size(38, 15);
            lblChild.TabIndex = 11;
            lblChild.Text = "Child:";
            // 
            // splitContainer
            // 
            splitContainer.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer.Location = new Point(12, 100);
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(lblParent);
            splitContainer.Panel1.Controls.Add(parentDataGridView);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(lblChild);
            splitContainer.Panel2.Controls.Add(childDataGridView);
            splitContainer.Size = new Size(776, 338);
            splitContainer.SplitterDistance = 181;
            splitContainer.TabIndex = 12;
            // 
            // childDataGridView
            // 
            childDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            childDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            childDataGridView.Location = new Point(3, 25);
            childDataGridView.Name = "childDataGridView";
            childDataGridView.Size = new Size(770, 150);
            childDataGridView.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(splitContainer);
            Controls.Add(lblConnectionString);
            Controls.Add(lblStatus);
            Controls.Add(txtConnectionString);
            Controls.Add(btnDeleteChild);
            Controls.Add(btnEditChild);
            Controls.Add(btnAddChild);
            Controls.Add(btnConnect);
            Name = "Form1";
            Text = "Parent-Child Database Manager";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)parentDataGridView).EndInit();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel1.PerformLayout();
            splitContainer.Panel2.ResumeLayout(false);
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)childDataGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // Add this method to the Form1 class
        private void Form1_Load(object sender, EventArgs e)
        {
            // Initialize any logic or data needed when the form loads
            lblStatus.Text = "Ready to connect.";
        }


        #endregion

        private System.Windows.Forms.DataGridView parentDataGridView;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnAddChild;
        private System.Windows.Forms.Button btnEditChild;
        private System.Windows.Forms.Button btnDeleteChild;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblConnectionString;
        private System.Windows.Forms.Label lblParent;
        private System.Windows.Forms.Label lblChild;
        private System.Windows.Forms.SplitContainer splitContainer;
        private DataGridView childDataGridView;
    }
}
