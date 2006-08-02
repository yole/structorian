namespace Structorian
{
    partial class DataView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._structTreeView = new System.Windows.Forms.TreeView();
            this._structGridView = new System.Windows.Forms.DataGridView();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._structGridView)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this._structTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this._structGridView);
            this.splitContainer1.Size = new System.Drawing.Size(549, 286);
            this.splitContainer1.SplitterDistance = 181;
            this.splitContainer1.TabIndex = 5;
            // 
            // _structTreeView
            // 
            this._structTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._structTreeView.Location = new System.Drawing.Point(0, 0);
            this._structTreeView.Name = "_structTreeView";
            this._structTreeView.Size = new System.Drawing.Size(181, 286);
            this._structTreeView.TabIndex = 3;
            this._structTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this._structTreeView_BeforeExpand);
            this._structTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this._structTreeView_AfterSelect);
            // 
            // _structGridView
            // 
            this._structGridView.AllowUserToAddRows = false;
            this._structGridView.AllowUserToDeleteRows = false;
            this._structGridView.AllowUserToResizeRows = false;
            this._structGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._structGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._structGridView.Location = new System.Drawing.Point(0, 0);
            this._structGridView.MultiSelect = false;
            this._structGridView.Name = "_structGridView";
            this._structGridView.RowHeadersVisible = false;
            this._structGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._structGridView.Size = new System.Drawing.Size(364, 286);
            this._structGridView.TabIndex = 2;
            this._structGridView.SelectionChanged += new System.EventHandler(this._structGridView_SelectionChanged);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            this.splitContainer2.Size = new System.Drawing.Size(549, 423);
            this.splitContainer2.SplitterDistance = 286;
            this.splitContainer2.TabIndex = 6;
            // 
            // DataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "DataView";
            this.Size = new System.Drawing.Size(549, 423);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._structGridView)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView _structTreeView;
        private System.Windows.Forms.DataGridView _structGridView;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}
