/*******************************************************************************
*
* Copyright (C) 2013-2014 Frozen North Computing
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*
*******************************************************************************/
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace FrozenNorth.TestFN2D
{
	public class Canvas : CanvasCommon
	{
		private bool panning;
		private Size startPan;
		private Point startLocation;

		public Canvas(Size size, string fontPath = null)
			: base(size, fontPath)
		{
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (image != null)
			{
				image.Zoom = image.Zoom + e.Delta * 0.1f / 120;
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Right && image != null)
			{
				panning = true;
				startPan = image.Pan;
				startLocation = e.Location;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (panning && e.Button == MouseButtons.Right)
			{
				Point offset = new Point((int)Math.Round((e.X - startLocation.X) / image.Zoom), (int)Math.Round((e.Y - startLocation.Y) / image.Zoom));
				image.Pan = new Size(startPan.Width + offset.X, startPan.Height + offset.Y);
				//image.Pan = new Point(startPan.X + e.X - startLocation.X, startPan.Y + e.Y - startLocation.Y);
			}
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Right)
			{
				if (panning)
				{
					Point offset = new Point((int)Math.Round((e.X - startLocation.X) / image.Zoom), (int)Math.Round((e.Y - startLocation.Y) / image.Zoom));
					image.Pan = new Size(startPan.Width + offset.X, startPan.Height + offset.Y);
					//image.Pan = new Point(startPan.X + e.X - startLocation.X, startPan.Y + e.Y - startLocation.Y);
				}
				panning = false;
			}
		}

		public override Bitmap GetImage(string fileName)
		{
			if (!Path.HasExtension(fileName))
			{
				fileName = Path.ChangeExtension(fileName, "png");
			}
			Assembly assembly = Assembly.GetExecutingAssembly();
			Stream stream = assembly.GetManifestResourceStream("FrozenNorth.TestFN2D.Assets.Images." + fileName);
			return (Bitmap)Bitmap.FromStream(stream);
		}
	}
}
