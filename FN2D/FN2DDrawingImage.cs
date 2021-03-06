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
#if FN2D_WIN
using OpenTK.Graphics.OpenGL;
using FN2DBitmap = System.Drawing.Bitmap;
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
#elif FN2D_AND
using OpenTK.Graphics.ES11;
using FN2DBitmap = Android.Graphics.Bitmap;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	public class FN2DDrawingImage : FN2DImage
	{
		// instance variables
		protected Color lineColor;
		protected int lineWidth;
		protected FN2DLineStripsList linesStripsList;
		protected FN2DLineStrips lineStrips;

		/// <summary>
		/// Constructor - Creates an image that can be drawn on.
		/// </summary>
		/// <param name="canvas">Canvas that the image will be on.</param>
		/// <param name="lineColor">Line color.</param>
		/// <param name="lineWidth">Line width.</param>
		/// <param name="image">Image to be displayed.</param>
		public FN2DDrawingImage(FN2DCanvas canvas, Color lineColor, int lineWidth = 1, FN2DBitmap image = null)
			: base(canvas, image)
		{
			// save the parameters
			this.lineColor = lineColor;
			this.lineWidth = lineWidth;

			// create the line strips list and the first line strip
			linesStripsList = new FN2DLineStripsList();
			lineStrips = new FN2DLineStrips(lineColor, lineWidth);
			linesStripsList.Add(lineStrips);

			// enable touch events
			TouchEnabled = true;
		}

		/// <summary>
		/// Frees unmanaged resources and calls Dispose() on the member objects.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				for (int i = linesStripsList.Count - 1; i >= 0; i--)
				{
					FN2DLineStrips lineStrips = linesStripsList[i];
					linesStripsList.RemoveAt(i);
					lineStrips.Dispose();
				}
			}
			linesStripsList = null;
			this.lineStrips = null;
			base.Dispose(disposing);
		}

		public FN2DLineStrips LineStrips
		{
			get { return lineStrips; }
		}

		public override void Draw()
		{
			base.Draw();
			foreach (FN2DLineStrips lineStrips in linesStripsList)
			{
				lineStrips.Draw();
			}
		}

		private Point GetZoomedLocation(Point location)
		{
			PointF offset = new PointF((location.X - Center.X) / Zoom, (location.Y - Center.Y) / Zoom);
			PointF center = new PointF(Center.X - pan.Width, Center.Y - pan.Height);
			return new Point((int)Math.Round(center.X + offset.X), (int)Math.Round(center.Y + offset.Y));
		}

		public override void TouchDown(FN2DTouchEventArgs e)
		{
			base.TouchDown(e);
			Touching = true;
			lineStrips.Add();
			lineStrips.Add(GetZoomedLocation(e.Location));
		}

		public override void TouchMove(FN2DTouchEventArgs e)
		{
			base.TouchMove(e);
			if (Touching)
			{
				lineStrips.Add(GetZoomedLocation(e.Location));
				canvas.IsDirty = true;
			}
		}

		public override void TouchUp(FN2DTouchEventArgs e)
		{
			base.TouchUp(e);
			if (Touching)
			{
				lineStrips.Add(GetZoomedLocation(e.Location));
				canvas.IsDirty = true;
			}
		}

		public override void TouchCancel(FN2DTouchEventArgs e)
		{
			base.TouchCancel(e);
			if (Touching)
			{
				lineStrips.Remove(lineStrips.LineStrip);
			}
		}
	}
}
