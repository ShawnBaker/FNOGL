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
#elif FN2D_IOS || FN2D_AND
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using BeginMode = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	public class FN2DLine : IDisposable
	{
		// instance variables
		protected Point p1, p2;
		protected float width;
		protected Color color;
		protected FN2DArrays lineArrays = FN2DArrays.Create(BeginMode.TriangleStrip, 8);
		protected FN2DArrays cap1Arrays = FN2DArrays.Create(BeginMode.TriangleStrip, 4);
		protected FN2DArrays cap2Arrays = FN2DArrays.Create(BeginMode.TriangleStrip, 4);
		
		public FN2DLine(Point p1, Point p2, float width, Color color)
		{
			// save the parameters
			this.p1 = p1;
			this.p2 = p2;
			this.width = width;
			this.color = color;

			// get the arrays
			Refresh();
		}

		/// <summary>
		/// Gets or sets the first point.
		/// </summary>
		public Point Point1
		{
			get { return p1; }
			set
			{
				p1 = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the second point.
		/// </summary>
		public Point Point2
		{
			get { return p2; }
			set
			{
				p2 = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		public float Width
		{
			get { return width; }
			set
			{
				width = (value > 0) ? value : 1;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the color.
		/// </summary>
		public Color Color
		{
			get { return color; }
			set
			{
				color = value;
				Refresh();
			}
		}

		/// <summary>
		/// Refreshes the arrays when something changes.
		/// </summary>
		private void Refresh()
		{
			float t, R = 1.08f;
			float f = width - (int)width;
			float alpha = color.A / 255f;
	
			// determine parameters t,R
			if (width >= 0 && width < 1)
			{
				t = 0.05f;
				R = 0.48f + f * 0.32f;
				alpha *= f;
			}
			else if (width >= 1 && width < 2)
			{
				t = 0.05f + f * 0.33f;
				R = 0.768f + f * 0.312f;
			}
			else if (width >= 2 && width < 3)
			{
				t = 0.38f + f * 0.58f;
			}
			else if (width >= 3 && width < 4)
			{
				t = 0.96f + f * 0.48f;
			}
			else if (width >= 4 && width < 5)
			{
				t = 1.44f + f * 0.46f;
			}
			else if (width >= 5 && width < 6)
			{
				t = 1.9f + f * 0.6f;
			}
			else
			{
				t = 2.5f + (width - 6) * 0.5f;
			}
	
			// determine angle of the line to horizontal
			float tx = 0, ty = 0; //core thinkness of a line
			float Rx = 0, Ry = 0; //fading edge of a line
			float cx = 0, cy = 0; //cap of a line
			float ALW = 0.01f;
			float dx = p2.X - p1.X;
			float dy = p2.Y - p1.Y;
			if (Math.Abs(dx) < ALW)
			{
				// vertical
				tx = t;
				Rx = R;
				if (width > 0 && width < 1)
					tx *= 8;
				else if (width == 1)
					tx *= 10;
			}
			else if (Math.Abs(dy) < ALW)
			{
				// horizontal
				ty = t;
				Ry = R;
				if (width > 0 && width < 1)
					ty *= 8;
				else if (width == 1)
					ty *= 10;
			}
			else
			{
				// approximate to make things even faster
				if (width < 3)
				{
					float m = dy / dx;

					// calculate tx,ty,Rx,Ry
					if (m > -0.4142f && m <= 0.4142f)
					{
						// -22.5< angle <= 22.5, approximate to 0 (degree)
						tx = t * 0.1f; ty = t;
						Rx = R * 0.6f; Ry = R;
					}
					else if (m > 0.4142f && m <= 2.4142f)
					{
						// 22.5< angle <= 67.5, approximate to 45 (degree)
						tx = t * -0.7071f; ty = t * 0.7071f;
						Rx = R * -0.7071f; Ry = R * 0.7071f;
					}
					else if (m > 2.4142f || m <= -2.4142f)
					{
						// 67.5 < angle <=112.5, approximate to 90 (degree)
						tx = t; ty = t * 0.1f;
						Rx = R; Ry = R * 0.6f;
					}
					else if (m > -2.4142f && m < -0.4142f)
					{
						// 112.5 < angle < 157.5, approximate to 135 (degree)
						tx = t * 0.7071f; ty = t * 0.7071f;
						Rx = R * 0.7071f; Ry = R * 0.7071f;
					}
					else
					{
						// error in determining angle
						//printf( "error in determining angle: m=%.4f\n",m);
					}
				}

				// calculate to exact
				else
				{
					dx = p1.Y - p2.Y;
					dy = p2.X - p1.X;
					float L = (float)Math.Sqrt(dx * dx + dy * dy);
					dx /= L;
					dy /= L;
					cx = -0.6f * dy; cy = 0.6f * dx;
					tx = t * dx; ty = t * dy;
					Rx = R * dx; Ry = R * dy;
				}
			}

			// clear the arrays
			lineArrays.Clear();
			cap1Arrays.Clear();
			cap2Arrays.Clear();

			// create the colors
			Color color0 = Color.FromArgb(0, color);
			Color colorA = Color.FromArgb((int)(alpha * 255), color);

			// create the line vertices
			lineArrays.AddVertex(p1.X - tx - Rx, p1.Y - ty - Ry, color0);		// fading edge1
			lineArrays.AddVertex(p2.X - tx - Rx, p2.Y - ty - Ry, color0);
			lineArrays.AddVertex(p1.X - tx, p1.Y - ty, colorA);					// core
			lineArrays.AddVertex(p2.X - tx, p2.Y - ty, colorA);
			lineArrays.AddVertex(p1.X + tx, p1.Y + ty, colorA);
			lineArrays.AddVertex(p2.X + tx, p2.Y + ty, colorA);
			if (!(Math.Abs(dx) < ALW || Math.Abs(dy) < ALW) || width > 1)
			{
				lineArrays.AddVertex(p1.X + tx + Rx, p1.Y + ty + Ry, color0);	// fading edge2
				lineArrays.AddVertex(p2.X + tx + Rx, p2.Y + ty + Ry, color0);
			}
	
			// create the cap vertices
			if (width >= 3)
			{
				cap1Arrays.AddVertex(p1.X - Rx + cx, p1.Y - Ry + cy, color0);
				cap1Arrays.AddVertex(p1.X + Rx + cx, p1.Y + Ry + cy, color0);
				cap1Arrays.AddVertex(p1.X - tx - Rx, p1.Y - ty - Ry, colorA);
				cap1Arrays.AddVertex(p1.X + tx + Rx, p1.Y + ty + Ry, colorA);
				cap2Arrays.AddVertex(p2.X - Rx - cx, p2.Y - Ry - cy, color0);
				cap2Arrays.AddVertex(p2.X + Rx - cx, p2.Y + Ry - cy, color0);
				cap2Arrays.AddVertex(p2.X - tx - Rx, p2.Y - ty - Ry, colorA);
				cap2Arrays.AddVertex(p2.X + tx + Rx, p2.Y + ty + Ry, colorA);
			}
			lineArrays.Dump("lines");
			cap1Arrays.Dump("cap1");
			cap2Arrays.Dump("cap2");
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DLine()
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
				if (lineArrays != null) lineArrays.Dispose();
				if (cap1Arrays != null) cap1Arrays.Dispose();
				if (cap2Arrays != null) cap2Arrays.Dispose();
			}

			// clear the object references
			lineArrays = null;
			cap1Arrays = null;
			cap2Arrays = null;
		}

		/// <summary>
		/// Draws the line.
		/// </summary>
		public void Draw()
		{
			lineArrays.Draw();
			cap1Arrays.Draw();
			cap2Arrays.Draw();
		}
	}
}
