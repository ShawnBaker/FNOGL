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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK.Platform.Windows;
using OpenTK.Graphics.OpenGL;

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D Canvas.
	/// </summary>
	public partial class FN2DCanvas : OpenTK.GLControl
	{
		// instance variables
		private Timer drawTimer;

		/// <summary>
		/// Constructor - Creates the canvas.
		/// </summary>
		/// <param name="size">Size of the canvas.</param>
		/// <param name="fontPath">Path to the font files.</param>
		public FN2DCanvas(Size size, string fontPath = null)
			: base()
		{
			// force immediate creation of the handle
			CreateHandle();

			// create the draw timer
			drawTimer = new Timer();
			drawTimer.Tick += new EventHandler(HandleDrawTimerTick);

			// get the font path if necessary
			if (string.IsNullOrEmpty(fontPath))
			{
				// get the path to the executable file
				fontPath = Application.StartupPath;

				// remove the bin\release or bin\debug path if it exists
				string last = Path.GetFileName(fontPath);
				if (string.Compare(last, "debug", true) == 0 || string.Compare(last, "release", true) == 0)
				{
					fontPath = Path.GetDirectoryName(Path.GetDirectoryName(fontPath));
				}

				// add the Fonts subdirectory
				fontPath = Path.Combine(fontPath, "Fonts");
			}

			// initialize the platform independent parts of the canvas
			Initialize(size, fontPath);
		}

		/// <summary>
		/// Resizes the root control whenever the canvas size changes.
		/// </summary>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			try
			{
				if (loaded)
				{
					MakeCurrent();
					rootControl.Size = ClientSize;
					Refresh();
				}
			}
			catch { }
		}

		/// <summary>
		/// Bind our buffers.
		/// </summary>
		public void Bind()
		{
			MakeCurrent();
		}

		/// <summary>
		/// Unbind our buffers.
		/// </summary>
		public void Unbind()
		{
		}

		/// <summary>
		/// Prevent flickering by not painting the background.
		/// </summary>
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			IsDirty = true;
		}

		/// <summary>
		/// Prevent flickering by not painting.
		/// </summary>
		protected override void OnPaint(PaintEventArgs e)
		{
			IsDirty = true;
		}

		/// <summary>
		/// Perform animations and drawing.
		/// </summary>
		private void HandleDrawTimerTick(object sender, EventArgs e)
		{
			HandleDrawTimer();
		}

		/// <summary>
		/// Start the drawing timer.
		/// </summary>
		public void StartDrawTimer()
		{
			drawTimer.Interval = (int)Math.Round(1000 * drawTimerInterval);
			drawTimer.Start();
		}

		/// <summary>
		/// Stop the drawing timer.
		/// </summary>
		public void StopDrawTimer()
		{
			drawTimer.Stop();
		}

		/// <summary>
		/// Convert left mouse down events to touch down events.
		/// </summary>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				OnTouchDown(GetTouchArgs(e));
			}
		}

		/// <summary>
		/// Convert left mouse move events to touch move events.
		/// </summary>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			OnTouchMove(GetTouchArgs(e));
		}

		/// <summary>
		/// Convert left mouse up events to touch up events.
		/// </summary>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			OnTouchUp(GetTouchArgs(e));
		}

		/// <summary>
		/// Gets the touch event arguments from the mouse event arguments.
		/// </summary>
		/// <param name="e">Mouse event arguments.</param>
		/// <returns>Touch event arguments.</returns>
		private FN2DTouchEventArgs GetTouchArgs(MouseEventArgs e)
		{
			FN2DTouchButtons buttons = FN2DTouchButtons.None;
			if (e.Button == MouseButtons.Left) buttons = buttons | FN2DTouchButtons.Left;
			if (e.Button == MouseButtons.Middle) buttons = buttons | FN2DTouchButtons.Middle;
			if (e.Button == MouseButtons.Right) buttons = buttons | FN2DTouchButtons.Right;
			return new FN2DTouchEventArgs(e.Location, buttons);
		}
	}
}
