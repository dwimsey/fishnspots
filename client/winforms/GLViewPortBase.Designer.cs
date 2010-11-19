namespace FishnSpots
{
	partial class GLViewPortBase
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
			if(disposing && (components != null)) {
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
			this.glControl = new FSGLControl();
			this.SuspendLayout();
			// 
			// glControl
			// 
			this.glControl.BackColor = System.Drawing.SystemColors.Control;
			this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.glControl.ForeColor = System.Drawing.SystemColors.ControlText;
			this.glControl.Location = new System.Drawing.Point(0, 0);
			this.glControl.Name = "glControl";
			this.glControl.Size = new System.Drawing.Size(292, 266);
			this.glControl.TabIndex = 0;
			this.glControl.VSync = false;
			// 
			// GLViewPortBase
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.glControl);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "GLViewPortBase";
			this.Text = "GLViewPortBase";
			this.ResumeLayout(false);

		}

		#endregion

		public FSGLControl glControl;
	}
}