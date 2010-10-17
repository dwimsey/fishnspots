using GMap.NET;

namespace FishnSpots
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
			this.statusBar = new System.Windows.Forms.StatusStrip();
			this.statusBarMapModeLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this.depthStrip1 = new FishnSpots.GPSViewPort();
			this.statusBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// statusBar
			// 
			this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusBarMapModeLabel});
			this.statusBar.Location = new System.Drawing.Point(0, 438);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(552, 22);
			this.statusBar.TabIndex = 1;
			// 
			// statusBarMapModeLabel
			// 
			this.statusBarMapModeLabel.Name = "statusBarMapModeLabel";
			this.statusBarMapModeLabel.Size = new System.Drawing.Size(60, 17);
			this.statusBarMapModeLabel.Text = "Map Mode:";
			// 
			// depthStrip1
			// 
			this.depthStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.depthStrip1.Location = new System.Drawing.Point(0, -3);
			this.depthStrip1.Name = "depthStrip1";
			this.depthStrip1.Size = new System.Drawing.Size(552, 438);
			this.depthStrip1.TabIndex = 2;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(552, 460);
			this.Controls.Add(this.depthStrip1);
			this.Controls.Add(this.statusBar);
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "FishnSpots";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.statusBar.ResumeLayout(false);
			this.statusBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.StatusStrip statusBar;
		private System.Windows.Forms.ToolStripStatusLabel statusBarMapModeLabel;
		private GPSViewPort depthStrip1;
    }
}

