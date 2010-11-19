using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;
//using OpenTK.Graphics;

using System.Drawing.Imaging;

namespace FishnSpots
{
	public partial class GLViewPortBase : FSViewPort
	{
		Bitmap text_bmp;
		int text_texture;
		UInt64 framesDrawn = 0;

		public GLViewPortBase()
		{
			InitializeComponent();
			glControl.Paint += new PaintEventHandler(glControl_Paint);
			glControl.Load += new EventHandler(glControl_Load);
			glControl.Resize +=new EventHandler(glControl_Resize);
		}

		void  glControl_Resize(object sender, EventArgs e)
		{
			// Ensure Bitmap and texture match window size
			if(text_bmp != null) {
				text_bmp.Dispose();
				text_bmp = new Bitmap(glControl.ClientSize.Width, glControl.ClientSize.Height);

				
				//	GL.BindTexture(TextureTarget.Texture2D, text_texture);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, text_bmp.Width, text_bmp.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, IntPtr.Zero);
			}
		}

		void  glControl_Load(object sender, EventArgs e)
		{





		}

		void drawText(int x, int y, string text)
		{
			// Do this only when text changes.
			using (Graphics gfx = Graphics.FromImage(text_bmp)) {
				gfx.Clear(Color.Transparent);
				gfx.DrawString(text, new Font(FontFamily.GenericSerif, 20.0f), Brushes.Azure, 50f, 50f);
			}

			// Upload the Bitmap to OpenGL.
			// Do this only when text changes.
			BitmapData data = text_bmp.LockBits(new Rectangle(0, 0, text_bmp.Width, text_bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba, glControl.ClientSize.Width, glControl.ClientSize.Height, 0,
			OpenTK.Graphics.OpenGL.PixelFormat.Bgra, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, data.Scan0); 
			text_bmp.UnlockBits(data);
		}
		void glControl_Paint(object sender, PaintEventArgs e)
		{
			framesDrawn++;

			#region glControl painting process
			glControl.MakeCurrent();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.AccumBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

			// Finally, render using a quad. 
			// Do this every frame.
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, glControl.ClientRectangle.Width, glControl.ClientRectangle.Height, 0, -1, 1);

			GL.Enable(EnableCap.Texture2D);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.One, OpenTK.Graphics.OpenGL.BlendingFactorDest.OneMinusSrcAlpha);

			GL.Begin(BeginMode.Quads);
			GL.TexCoord2(0f, 1f);
			GL.Vertex2(0f, 0f);
			GL.TexCoord2(1f, 1f);
			GL.Vertex2(1f, 0f);
			GL.TexCoord2(1f, 0f);
			GL.Vertex2(1f, 1f);
			GL.TexCoord2(0f, 0f);
			GL.Vertex2(0f, 1f);
			GL.End();

			glControl.SwapBuffers();
			#endregion glControl painting process
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// Initialize the GLControl to match the rest of the window
			glControl.CreateControl();
			glControl.MakeCurrent();
			GL.ClearColor(glControl.BackColor);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.AccumBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
			GL.Color4(glControl.ForeColor);
			glControl.SwapBuffers();

			// Create Bitmap and OpenGL texture
			text_bmp = new Bitmap(glControl.ClientSize.Width, glControl.ClientSize.Height); // match window size
			GL.Enable(EnableCap.Texture2D);
			text_texture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, text_texture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexImage2D(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL.PixelInternalFormat.Rgba, text_bmp.Width, text_bmp.Height, 0,
			OpenTK.Graphics.OpenGL.PixelFormat.Bgra, OpenTK.Graphics.OpenGL.PixelType.UnsignedByte, IntPtr.Zero); // just allocate memory, so we can update efficiently using TexSubImage2D

			drawText(100, 100, "Whats up?");
		}
	}

	public class FSGLControl : GLControl
	{
		protected override bool IsInputKey(System.Windows.Forms.Keys keyData)
		{
			/*
			switch(keyData & Keys.KeyCode) {
				case Keys.Up:
					return true;
				case Keys.Down:
					return true;
				case Keys.Right:
					return true;
				case Keys.Left:
					return true;
				default:
					return base.IsInputKey(keyData);
			}
			*/
			return base.IsInputKey(keyData);
		}
	}
}
