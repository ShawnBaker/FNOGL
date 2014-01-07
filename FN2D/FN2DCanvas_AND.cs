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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using Android.Views;
using Android.Content;
using Android.Util;

namespace FrozenNorth.OpenGL.FN2D
{
	public partial class FN2DCanvas : AndroidGameView
	{
		private Size saveSize;
		private string saveFontPath;
		private int pointerId = -1;
		
		/// <summary>
		/// Constructor - Creates an OpenGL 2D canvas.
		/// </summary>
		/// <param name="context">Android drawing context.</param>
		/// <param name="size">Size of the canvas.</param>
		/// <param name="fontPath">Path to the font files.</param>
		public FN2DCanvas(Context context, Size size, string fontPath = null)
			: base(context)
		{
			saveSize = size;
			saveFontPath = fontPath;
		}

		/// <summary>
		/// Called when the drawing surface is ready.
		/// </summary>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// initialize the platform independent parts of the canvas
			Initialize(saveSize, saveFontPath);
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
		/// Start the drawing timer.
		/// </summary>
		public void StartDrawTimer()
		{
			Run();
		}

		/// <summary>
		/// Stop the drawing timer.
		/// </summary>
		public void StopDrawTimer()
		{
			Stop();
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		//
		// In this particular case, we demonstrate how to set
		// the graphics mode and fallback in case the device doesn't
		// support the defaults
		protected override void CreateFrameBuffer()
		{
			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try
			{
				Log.Verbose("GLCube", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer();
				return;
			} catch (Exception ex)
			{
				Log.Verbose("GLCube", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try
			{
				Log.Verbose("GLCube", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode(0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer();
				return;
			} catch (Exception ex)
			{
				Log.Verbose("GLCube", "{0}", ex);
			}
			throw new Exception("Can't load egl, aborting");
		}

		// This gets called on each frame render
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			Draw();
		}

		/// <summary>
		/// Process single finger touch up events.
		/// </summary>
		public override bool OnTouchEvent(MotionEvent e)
		{
			bool result = base.OnTouchEvent(e);
			if (e.PointerCount == 1)
			{
				switch (e.ActionMasked)
				{
					case MotionEventActions.Down:
						pointerId = e.GetPointerId(0);
						OnTouchDown(new FN2DTouchEventArgs(new Point((int)e.GetX(pointerId), (int)e.GetY(pointerId))));
						break;
					case MotionEventActions.Move:
						if (e.GetPointerId(0) == pointerId)
						{
							OnTouchMove(new FN2DTouchEventArgs(new Point((int)e.GetX(pointerId), (int)e.GetY(pointerId))));
						}
						break;
					case MotionEventActions.Up:
						if (e.GetPointerId(0) == pointerId)
						{
							OnTouchUp(new FN2DTouchEventArgs(new Point((int)e.GetX(pointerId), (int)e.GetY(pointerId))));
						}
						pointerId = -1;
						break;
					case MotionEventActions.Cancel:
						OnTouchCancel(FN2DTouchEventArgs.Empty);
						pointerId = -1;
						break;
				}
				result = true;
			}
			return result;
		}
	}
}
