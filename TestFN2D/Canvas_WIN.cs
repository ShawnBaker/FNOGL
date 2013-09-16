using System;
using System.Drawing;
using System.Windows.Forms;

namespace FrozenNorth.TestFN2D
{
	public class Canvas_WIN : Canvas
	{
		private bool panning;
		private Size startPan;
		private Point startLocation;

		public Canvas_WIN(Size size, string fontPath = null)
			: base(size, fontPath)
		{
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			image.Zoom = image.Zoom + e.Delta * 0.1f / 120;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Right)
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
	}
}
