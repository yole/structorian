namespace Structorian
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadStructuresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLocalOffsetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openStructsDialog = new System.Windows.Forms.OpenFileDialog();
            this._openDataDialog = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._structEditControl = new ICSharpCode.TextEditor.TextEditorControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._btnLoadStuctures = new System.Windows.Forms.ToolStripButton();
            this._btnSaveStructures = new System.Windows.Forms.ToolStripButton();
            this.newStructuresToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveStructsDialog = new System.Windows.Forms.SaveFileDialog();
            this.menuStrip1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(438, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newStructuresToolStripMenuItem,
            this.loadStructuresToolStripMenuItem,
            this.loadDataToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // loadStructuresToolStripMenuItem
            // 
            this.loadStructuresToolStripMenuItem.Name = "loadStructuresToolStripMenuItem";
            this.loadStructuresToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.loadStructuresToolStripMenuItem.Text = "Load Structures...";
            this.loadStructuresToolStripMenuItem.Click += new System.EventHandler(this.loadStructuresToolStripMenuItem_Click);
            // 
            // loadDataToolStripMenuItem
            // 
            this.loadDataToolStripMenuItem.Name = "loadDataToolStripMenuItem";
            this.loadDataToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.loadDataToolStripMenuItem.Text = "Load Data...";
            this.loadDataToolStripMenuItem.Click += new System.EventHandler(this.loadDataToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showLocalOffsetsToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(41, 20);
            this.viewToolStripMenuItem.Text = "&View";
            // 
            // showLocalOffsetsToolStripMenuItem
            // 
            this.showLocalOffsetsToolStripMenuItem.Name = "showLocalOffsetsToolStripMenuItem";
            this.showLocalOffsetsToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.showLocalOffsetsToolStripMenuItem.Text = "Show &Local Offsets";
            this.showLocalOffsetsToolStripMenuItem.Click += new System.EventHandler(this.showLocalOffsetsToolStripMenuItem_Click);
            // 
            // _openStructsDialog
            // 
            this._openStructsDialog.Filter = "Structure Definitions (*.strs)|*.strs|All files|*.*";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 24);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this._structEditControl);
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip1);
            this.splitContainer2.Size = new System.Drawing.Size(438, 400);
            this.splitContainer2.SplitterDistance = 108;
            this.splitContainer2.TabIndex = 4;
            // 
            // _structEditControl
            // 
            this._structEditControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this._structEditControl.Location = new System.Drawing.Point(0, 25);
            this._structEditControl.Name = "_structEditControl";
            this._structEditControl.ShowEOLMarkers = true;
            this._structEditControl.ShowInvalidLines = false;
            this._structEditControl.ShowLineNumbers = false;
            this._structEditControl.ShowSpaces = true;
            this._structEditControl.ShowTabs = true;
            this._structEditControl.ShowVRuler = true;
            this._structEditControl.Size = new System.Drawing.Size(438, 83);
            this._structEditControl.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._btnLoadStuctures,
            this._btnSaveStructures});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(438, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _btnLoadStuctures
            // 
            this._btnLoadStuctures.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnLoadStuctures.Image = ((System.Drawing.Image)(resources.GetObject("_btnLoadStuctures.Image")));
            this._btnLoadStuctures.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._btnLoadStuctures.Name = "_btnLoadStuctures";
            this._btnLoadStuctures.Size = new System.Drawing.Size(23, 22);
            this._btnLoadStuctures.Text = "toolStripButton1";
            this._btnLoadStuctures.ToolTipText = "Load Structure Definitions";
            this._btnLoadStuctures.Click += new System.EventHandler(this.loadStructuresToolStripMenuItem_Click);
            // 
            // _btnSaveStructures
            // 
            this._btnSaveStructures.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this._btnSaveStructures.Image = ((System.Drawing.Image)(resources.GetObject("_btnSaveStructures.Image")));
            this._btnSaveStructures.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._btnSaveStructures.Name = "_btnSaveStructures";
            this._btnSaveStructures.Size = new System.Drawing.Size(23, 22);
            this._btnSaveStructures.Text = "Save Structure Definitions";
            this._btnSaveStructures.Click += new System.EventHandler(this._btnSaveStructures_Click);
            // 
            // newStructuresToolStripMenuItem
            // 
            this.newStructuresToolStripMenuItem.Name = "newStructuresToolStripMenuItem";
            this.newStructuresToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.newStructuresToolStripMenuItem.Text = "New Structures";
            this.newStructuresToolStripMenuItem.Click += new System.EventHandler(this.newStructuresToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 424);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Location = new System.Drawing.Point(30, 30);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Structorian";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadStructuresToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog _openStructsDialog;
        private System.Windows.Forms.OpenFileDialog _openDataDialog;
        private System.Windows.Forms.ToolStripMenuItem loadDataToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ICSharpCode.TextEditor.TextEditorControl _structEditControl;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton _btnLoadStuctures;
        private System.Windows.Forms.ToolStripButton _btnSaveStructures;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showLocalOffsetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newStructuresToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog _saveStructsDialog;
    }
}

