using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace FishnSpots
{
	public class GLCompassViewPort : GLViewPortBase
	{
		public GLCompassViewPort()
		{
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			glControl.CreateControl();
			glControl.MakeCurrent();
			GL.ClearColor(System.Drawing.SystemColors.Control);
			GL.Clear(ClearBufferMask.AccumBufferBit|ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit|ClearBufferMask.StencilBufferBit);
			glControl.SwapBuffers();
		}
	}
}
