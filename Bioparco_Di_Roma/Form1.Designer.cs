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
            this.components = new System.ComponentModel.Container();
            this.parentDataGridView = new System.Windows.Forms.DataGridView();
            this.childDataGridView = new System.Windows.Forms.DataGridView();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnAddChild = new System.Windows.Forms.Button();
            this.btnEditChild = new System.Windows.Forms.Button();
            this.btnDeleteChild = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblConnectionString = new System.Windows.Forms.Label();
            this.lblParent = new System.Windows.Forms.Label();
            this.lblChild = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.parentDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.childDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // parentDataGridView
            // 
            this.parentDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parentDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.parentDataGridView.Location = new System.Drawing.Point(3, 25);
            this.parentDataGridView.Name = "parentDataGridView";
            this.parentDataGridView.RowTemplate.Height = 25;
            this.parentDataGridView.Size = new System.Drawing.Size(770, 150);
            this.parentDataGridView.TabIndex = 0;
            // 
            // childDataGridView
            // 
            this.childDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.childDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.childDataGridView.Location = new System.Drawing.Point(3, 25);
            this.childDataGridView.Name = "childDataGridView";
            this.childDataGridView.RowTemplate.Height = 25;
            this.childDataGridView.Size = new System.Drawing.Size(770, 150);
            this.childDataGridView.TabIndex = 1;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 71);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnAddChild
            // 
            this.btnAddChild.Location = new System.Drawing.Point(93, 71);
            this.btnAddChild.Name = "btnAddChild";
            this.btnAddChild.Size = new System.Drawing.Size(75, 23);
            this.btnAddChild.TabIndex = 4;
            this.btnAddChild.Text = "Add Animal";
            this.btnAddChild.UseVisualStyleBackColor = true;
            // 
            // btnEditChild
            // 
            this.btnEditChild.Location = new System.Drawing.Point(174, 71);
            this.btnEditChild.Name = "btnEditChild";
            this.btnEditChild.Size = new System.Drawing.Size(75, 23);
            this.btnEditChild.TabIndex = 5;
            this.btnEditChild.Text = "Edit Animal";
            this.btnEditChild.UseVisualStyleBackColor = true;
            // 
            // btnDeleteChild
            // 
            this.btnDeleteChild.Location = new System.Drawing.Point(255, 71);
            this.btnDeleteChild.Name = "btnDeleteChild";
            this.btnDeleteChild.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteChild.TabIndex = 6;
            this.btnDeleteChild.Text = "Delete Animal";
            this.btnDeleteChild.UseVisualStyleBackColor = true;
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtConnectionString.Location = new System.Drawing.Point(12, 25);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(776, 23);
            this.txtConnectionString.TabIndex = 7;
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(713, 75);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(75, 15);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Disconnected";
            // 
            // lblConnectionString
            // 
            this.lblConnectionString.AutoSize = true;
            this.lblConnectionString.Location = new System.Drawing.Point(12, 7);
            this.lblConnectionString.Name = "lblConnectionString";
            this.lblConnectionString.Size = new System.Drawing.Size(103, 15);
            this.lblConnectionString.TabIndex = 9;
            this.lblConnectionString.Text = "Connection String:";
            // 
            // lblParent
            // 
            this.lblParent.AutoSize = true;
            this.lblParent.Location = new System.Drawing.Point(3, 7);
            this.lblParent.Name = "lblParent";
            this.lblParent.Size = new System.Drawing.Size(43, 15);
            this.lblParent.TabIndex = 10;
            this.lblParent.Text = "Parent:";
            // 
            // lblChild
            // 
            this.lblChild.AutoSize = true;
            this.lblChild.Location = new System.Drawing.Point(3, 7);
            this.lblChild.Name = "lblChild";
            this.lblChild.Size = new System.Drawing.Size(38, 15);
            this.lblChild.TabIndex = 11;
            this.lblChild.Text = "Child:";
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 100);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.lblParent);
            this.splitContainer.Panel1.Controls.Add(this.parentDataGridView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.lblChild);
            this.splitContainer.Panel2.Controls.Add(this.childDataGridView);
            this.splitContainer.Size = new System.Drawing.Size(776, 338);
            this.splitContainer.SplitterDistance = 181;
            this.splitContainer.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.lblConnectionString);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtConnectionString);
            this.Controls.Add(this.btnDeleteChild);
            this.Controls.Add(this.btnEditChild);
            this.Controls.Add(this.btnAddChild);
            this.Controls.Add(this.btnConnect);
            this.Name = "Form1";
            this.Text = "Parent-Child Database Manager";
            ((System.ComponentModel.ISupportInitialize)(this.parentDataGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.childDataGridView)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.DataGridView parentDataGridView;
        private System.Windows.Forms.DataGridView childDataGridView;
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
    }
}
