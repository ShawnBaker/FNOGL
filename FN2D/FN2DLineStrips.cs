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
using System.Collections.Generic;
#if FN2D_WIN
using OpenTK.Graphics.OpenGL;
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using BeginMode = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	public class FN2DLineStrips : List<FN2DArrays>, IDisposable
	{
		// instance variables
		private Color lineColor;
		private int lineWidth;
		private PointF lastPoint = PointF.Empty;

		/// <summary>
		/// Constructor - Creates an empty set of line strips.
		/// </summary>
		/// <param name="lineColor">Line color.</param>
		/// <param name="lineWidth">Line width.</param>
		public FN2DLineStrips(Color lineColor, int lineWidth = 1)
			: base()
		{
			// save the parameters
			this.lineColor = lineColor;
			this.lineWidth = lineWidth;
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DLineStrips()
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
				Clear();
			}
		}

		/// <summary>
		/// Gets the current/last line strip.
		/// </summary>
		public FN2DArrays LineStrip
		{
			get { return (Count != 0) ? this[Count - 1] : null; }
		}

		public new void Remove(FN2DArrays lineStrip)
		{
			if (lineStrip != null)
			{
				lineStrip.Dispose();
				base.Remove(lineStrip);
			}
		}

		/// <summary>
		/// Adds a line strip.
		/// </summary>
		public void Add()
		{
			if (Count == 0 || this[Count - 1].NumVertices != 0)
			{
				Add(new FN2DArrays(BeginMode.LineStrip));
			}
		}

		/// <summary>
		/// Adds a point.
		/// </summary>
		/// <param name="x">The point's X coordinate.</param>
		/// <param name="y">The point's Y coordinate.</param>
		public void Add(float x, float y)
		{
			FN2DArrays lineStrip = LineStrip;
			if (lineStrip == null)
			{
				lineStrip = new FN2DArrays(BeginMode.LineStrip);
				Add(lineStrip);
			}
			lineStrip.AddVertex(x, y);
			lastPoint.X = x;
			lastPoint.Y = y;
		}

		/// <summary>
		/// Adds a point.
		/// </summary>
		/// <param name="point">Point to be added.</param>
		public void Add(PointF point)
		{
			Add(point.X, point.Y);
		}

		/// <summary>
		/// Adds a point.
		/// </summary>
		/// <param name="point">Point to be added.</param>
		public void Add(Point point)
		{
			Add(point.X, point.Y);
		}

		/// <summary>
		/// Adds a line.
		/// </summary>
		/// <param name="x1">The start point's X coordinate.</param>
		/// <param name="y1">The start point's Y coordinate.</param>
		/// <param name="x2">The end point's X coordinate.</param>
		/// <param name="y2">The end point's Y coordinate.</param>
		public void Add(float x1, float y1, float x2, float y2)
		{
			FN2DArrays lineStrip = LineStrip;
			if (lineStrip == null || (lineStrip.NumVertices != 0 && (lastPoint.X != x1 || lastPoint.Y != y1)))
			{
				lineStrip = new FN2DArrays(BeginMode.LineStrip);
				lineStrip.AddVertex(x1, y1);
				Add(lineStrip);
			}
			lineStrip.AddVertex(x2, y2);
			lastPoint.X = x2;
			lastPoint.Y = y2;
		}

		/// <summary>
		/// Adds a line.
		/// </summary>
		/// <param name="fromPoint">Start point of the line.</param>
		/// <param name="toPoint">End point of the line.</param>
		public void Add(PointF fromPoint, PointF toPoint)
		{
			Add(fromPoint.X, fromPoint.Y, toPoint.X, toPoint.Y);
		}

		/// <summary>
		/// Adds a line.
		/// </summary>
		/// <param name="fromPoint">Start point of the line.</param>
		/// <param name="toPoint">End point of the line.</param>
		public void Add(Point fromPoint, Point toPoint)
		{
			Add(fromPoint.X, fromPoint.Y, toPoint.X, toPoint.Y);
		}

		/// <summary>
		/// Removes all existing line strips.
		/// </summary>
		public new void Clear()
		{
			foreach (FN2DArrays lineStrip in this)
			{
				lineStrip.Dispose();
			}
			base.Clear();
		}

		/// <summary>
		/// Draws the line.
		/// </summary>
		public virtual void Draw()
		{
			// set the line color
			if (lineColor.R != 0 || lineColor.G != 0 || lineColor.B != 0)
				GL.Color4(lineColor.R, lineColor.G, lineColor.B, lineColor.A);
			else
				GL.Color4(1, 1, 1, 255);

			// set the line width
			GL.LineWidth(lineWidth);

			// draw the line strips
			foreach (FN2DArrays lineStrip in this)
			{
				lineStrip.Draw();
			}
		}
	}

	/// <summary>
	/// OpenGL 2D list of line strips.
	/// </summary>
	public class FN2DLineStripsList : List<FN2DLineStrips> {}
}
