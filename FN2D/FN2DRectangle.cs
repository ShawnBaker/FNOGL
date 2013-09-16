/*******************************************************************************
*
* Copyright (C) 2013 Frozen North Computing
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
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using BeginMode = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// A set of FN2DArrays for drawing a rectangle.
	/// </summary>
	public class FN2DRectangle : IDisposable
	{
		// instance variables
		protected Rectangle frame;
		protected Color topColor, bottomColor;
		protected int cornerRadius = FN2DCanvas.DefaultCornerRadius;
		protected int numCornerSteps = FN2DCanvas.DefaultCornerSteps;
		protected FN2DArrays rectArrays = new FN2DArrays();
		protected FN2DArrays topRightArrays = new FN2DArrays(BeginMode.TriangleFan);
		protected FN2DArrays bottomRightArrays = new FN2DArrays(BeginMode.TriangleFan);
		protected FN2DArrays bottomLeftArrays = new FN2DArrays(BeginMode.TriangleFan);
		protected FN2DArrays topLeftArrays = new FN2DArrays(BeginMode.TriangleFan);

		/// <summary>
		/// Constructor - Creates a gradient rectangle with a corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="cornerRadius">Corner radius.</param>
		/// <param name="topColor">Top color.</param>
		/// <param name="bottomColor">Bottom color.</param>
		public FN2DRectangle(Rectangle frame, int cornerRadius, Color topColor, Color bottomColor)
		{
			// save the parameters
			this.frame = frame;
			this.cornerRadius = cornerRadius;
			this.topColor = topColor;
			this.bottomColor = bottomColor;

			// get the arrays
			Refresh();
		}

		/// <summary>
		/// Constructor - Creates a gradient rectangle with the default corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="topColor">Top color.</param>
		/// <param name="bottomColor">Bottom color.</param>
		public FN2DRectangle(Rectangle frame, Color topColor, Color bottomColor)
			: this(frame, FN2DCanvas.DefaultCornerRadius, topColor, bottomColor)
		{
		}

		/// <summary>
		/// Constructor - Creates a single color rectangle with a corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="cornerRadius">Corner radius.</param>
		/// <param name="color">Rectangle color.</param>
		public FN2DRectangle(Rectangle frame, int cornerRadius, Color color)
			: this(frame, FN2DCanvas.DefaultCornerRadius, color, color)
		{
		}

		/// <summary>
		/// Constructor - Creates a single color rectangle with the default corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		/// <param name="color">Rectangle color.</param>
		public FN2DRectangle(Rectangle frame, Color color)
			: this(frame, FN2DCanvas.DefaultCornerRadius, color, color)
		{
		}

		/// <summary>
		/// Constructor - Creates a rectangle with the default colors and corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		public FN2DRectangle(Rectangle frame)
			: this(frame, FN2DCanvas.DefaultCornerRadius, FN2DCanvas.DefaultTopColor, FN2DCanvas.DefaultBottomColor)
		{
		}

		/// <summary>
		/// Constructor - Creates a rectangle with a specific size and the default colors and corner radius.
		/// </summary>
		/// <param name="frame">Frame.</param>
		public FN2DRectangle(Size size)
			: this(new Rectangle(Point.Empty, size))
		{
		}
		
		/// <summary>
		/// Constructor - Creates an empty rectangle.
		/// </summary>
		public FN2DRectangle()
			: this(Rectangle.Empty)
		{
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DRectangle()
		{
			Dispose(false);
		}

		/// <summary>
		/// Releases all resource used by the object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Frees unmanaged resources and calls Dispose() on the member objects.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				if (rectArrays != null) rectArrays.Dispose();
				if (topRightArrays != null) topRightArrays.Dispose();
				if (bottomRightArrays != null) bottomRightArrays.Dispose();
				if (bottomLeftArrays != null) bottomLeftArrays.Dispose();
				if (topLeftArrays != null) topLeftArrays.Dispose();
			}

			// clear the object references
			rectArrays = null;
			topRightArrays = null;
			bottomRightArrays = null;
			bottomLeftArrays = null;
			topLeftArrays = null;
		}

		/// <summary>
		/// Gets or sets the frame.
		/// </summary>
		public Rectangle Frame
		{
			get { return frame; }
			set
			{
				frame = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the corner radius.
		/// </summary>
		public int CornerRadius
		{
			get { return cornerRadius; }
			set
			{
				cornerRadius = (value >= 0) ? value : 0;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the color.
		/// </summary>
		public Color Color
		{
			get { return topColor; }
			set
			{
				topColor = value;
				bottomColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the top color.
		/// </summary>
		public Color TopColor
		{
			get { return topColor; }
			set
			{
				topColor = value;
				Refresh();
			}
		}
		
		/// <summary>
		/// Gets or sets the bottom color.
		/// </summary>
		public Color BottomColor
		{
			get { return bottomColor; }
			set
			{
				bottomColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets the top and bottom colors.
		/// </summary>
		/// <param name="topColor">Top color.</param>
		/// <param name="bottomColor">Bottom color.</param>
		public void SetColors(Color topColor, Color bottomColor)
		{
			this.topColor = topColor;
			this.bottomColor = bottomColor;
			Refresh();
		}

		/// <summary>
		/// Refreshes the arrays when something changes.
		/// </summary>
		private void Refresh()
		{
			// if the colors are both transparent then don't draw anything
			if (frame.Width == 0 || frame.Height == 0 || (topColor == Color.Transparent && bottomColor == Color.Transparent))
			{
				rectArrays.Clear();
				topRightArrays.Clear();
				bottomRightArrays.Clear();
				bottomLeftArrays.Clear();
				topLeftArrays.Clear();
			}

			// if there's no corner radius then draw a rectangle
			else if (cornerRadius == 0)
			{
				rectArrays.AllocRects(1, false, true);
				AddRect(0, 0, frame.Width, frame.Height, topColor, bottomColor);
				rectArrays.Complete();
				topRightArrays.Clear();
				bottomRightArrays.Clear();
				bottomLeftArrays.Clear();
				topLeftArrays.Clear();
			}

			// otherwise, draw a rounded rectangle
			else
			{
				rectArrays.AllocRects(3, false, true);
				AddRect(0, cornerRadius, cornerRadius, frame.Height - cornerRadius);
				AddRect(frame.Width - cornerRadius, cornerRadius, frame.Width, frame.Height - cornerRadius);
				AddRect(cornerRadius, 0, frame.Width - cornerRadius, frame.Height, topColor, bottomColor);
				rectArrays.Complete();

				topRightArrays.Alloc(numCornerSteps + 2, 0, numCornerSteps + 2, 0);
				AddCorner(topRightArrays, frame.Width - cornerRadius, cornerRadius, Math.PI * 1.5);
				topRightArrays.Complete();

				bottomRightArrays.Alloc(numCornerSteps + 2, 0, numCornerSteps + 2, 0);
				AddCorner(bottomRightArrays, frame.Width - cornerRadius, frame.Height - cornerRadius, 0);
				bottomRightArrays.Complete();

				bottomLeftArrays.Alloc(numCornerSteps + 2, 0, numCornerSteps + 2, 0);
				AddCorner(bottomLeftArrays, cornerRadius, frame.Height - cornerRadius, Math.PI * 0.5);
				bottomLeftArrays.Complete();

				topLeftArrays.Alloc(numCornerSteps + 2, 0, numCornerSteps + 2, 0);
				AddCorner(topLeftArrays, cornerRadius, cornerRadius, Math.PI);
				topLeftArrays.Complete();
			}
		}

		/// <summary>
		/// Draws the rectangle.
		/// </summary>
		public void Draw()
		{
			rectArrays.Draw();
			topRightArrays.Draw();
			bottomRightArrays.Draw();
			bottomLeftArrays.Draw();
			topLeftArrays.Draw();
		}

		/// <summary>
		/// Gets the color at a specific height.
		/// </summary>
		/// <param name="y">The y coordinate.</param>
		/// <returns>The color.</returns>
		private Color GetColor(float y)
		{
			float portion = y / frame.Height;
			return Color.FromArgb((int)Math.Round((topColor.A + (bottomColor.A - topColor.A) * portion)),
			                      (int)Math.Round((topColor.R + (bottomColor.R - topColor.R) * portion)),
			                      (int)Math.Round((topColor.G + (bottomColor.G - topColor.G) * portion)),
			                      (int)Math.Round((topColor.B + (bottomColor.B - topColor.B) * portion)));
		}

		/// <summary>
		/// Adds a rectangle to the rectangle arrays.
		/// </summary>
		/// <param name="x1">The first x value.</param>
		/// <param name="y1">The first y value.</param>
		/// <param name="x2">The second x value.</param>
		/// <param name="y2">The second y value.</param>
		/// <param name="topColor">The second y value.</param>
		/// <param name="bottomColor">The second y value.</param>
		private void AddRect(float x1, float y1, float x2, float y2, Color topColor, Color bottomColor)
		{
			rectArrays.AddRect(frame.X + x1, frame.Y + y1, frame.X + x2, frame.Y + y2, topColor, bottomColor);
		}

		/// <summary>
		/// Adds a rectangle to the rectangle arrays.
		/// </summary>
		/// <param name="x1">The first x value.</param>
		/// <param name="y1">The first y value.</param>
		/// <param name="x2">The second x value.</param>
		/// <param name="y2">The second y value.</param>
		private void AddRect(float x1, float y1, float x2, float y2)
		{
			AddRect(x1, y1, x2, y2, GetColor(y1), GetColor(y2));
		}

		/// <summary>
		/// Adds a set of corner vertices to one of the arrays.
		/// </summary>
		/// <param name="arrays">Arrays to add the vertices to.</param>
		/// <param name="x">X coordinate of the center of the corner's circle.</param>
		/// <param name="y">Y coordinate of the center of the corner's circle.</param>
		/// <param name="angle">Angle to start drawing a quarter cirlce at.</param>
		private void AddCorner(FN2DArrays arrays, float x, float y, double angle)
		{
			double angleStep = Math.PI / 2 / numCornerSteps;
			arrays.AddVertex(x, y, GetColor(y));
			for (int i = 0; i <= numCornerSteps; i++)
			{
				float newY = y + (float)Math.Sin(angle) * cornerRadius;
				arrays.AddVertex(x + (float)Math.Cos(angle) * cornerRadius, newY, GetColor(newY));
				angle += angleStep;
			}
		}
	}
}
