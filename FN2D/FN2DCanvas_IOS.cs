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
using System.IO;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;

namespace FrozenNorth.OpenGL.FN2D
{
	public partial class FN2DCanvas : UIView, IDisposable
	{
		// static variables
		private static EAGLContext context;

		// instance variables
		private CAEAGLLayer eaglLayer;
		private int colorRenderBuffer = 0, frameBuffer = 0;
		private NSTimer drawTimer = null;

		/// <summary>
		/// Constructor - Creates an OpenGL 2D canvas.
		/// </summary>
		/// <param name="size">Size of the canvas.</param>
		/// <param name="fontPath">Path to the font files.</param>
		public FN2DCanvas(Size size, string fontPath = null)
			: base(new RectangleF(0, 0, size.Width, size.Height))
		{
			// create the OpenGL context
			context = new EAGLContext(EAGLRenderingAPI.OpenGLES1);
			EAGLContext.SetCurrentContext(context);

			// make the layer opaque
			eaglLayer = (CAEAGLLayer)Layer;
			eaglLayer.Opaque = true;

			// create the buffers
			CreateBuffers();

			// get the font path if necessary
			if (string.IsNullOrEmpty(fontPath))
			{
				fontPath = Path.Combine(NSBundle.MainBundle.ResourcePath, "Fonts");
			}

			// initialize the platform independent parts of the canvas
			Initialize(size, fontPath);
		}

		/// <summary>
		/// Required OpenGL ES entry point.
		/// </summary>
		[Export("layerClass")]
		public static Class LayerClass()
		{
			return new Class(typeof(CAEAGLLayer));
		}

		/// <summary>
		/// Gets the OpenGL context.
		/// </summary>
		public static EAGLContext Context
		{
			get { return context; }
		}

		/// <summary>
		/// Creates the frame and render buffers.
		/// </summary>
		private void CreateBuffers()
		{
			if (context != null && eaglLayer != null)
			{
				DestroyBuffers();

				GL.Oes.GenRenderbuffers(1, out colorRenderBuffer);
				GL.Oes.BindRenderbuffer(All.RenderbufferOes, colorRenderBuffer);
				context.RenderBufferStorage((int)All.RenderbufferOes, eaglLayer);

				GL.Oes.GenFramebuffers(1, out frameBuffer);
				GL.Oes.BindFramebuffer(All.FramebufferOes, frameBuffer);
				GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.ColorAttachment0Oes,
				                               All.RenderbufferOes, colorRenderBuffer);
			}
		}

		/// <summary>
		/// Destroys the frame and render buffers.
		/// </summary>
		private void DestroyBuffers()
		{
			if (frameBuffer != 0)
			{
				GL.Oes.DeleteFramebuffers(1, ref frameBuffer);
				frameBuffer = 0;
			}
			if (colorRenderBuffer != 0)
			{
				GL.Oes.DeleteRenderbuffers(1, ref colorRenderBuffer);
				colorRenderBuffer = 0;
			}
		}

		/// <summary>
		/// Gets or sets the frame.
		/// </summary>
		public override RectangleF Frame
		{
			get { return base.Frame; }
			set
			{
				base.Frame = value;
				if (loaded)
				{
					CreateBuffers();
					rootControl.Size = base.Frame.Size.ToSize();
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets the size of the canvas.
		/// </summary>
		public Size Size
		{
			get { return Frame.Size.ToSize(); }
			set { Frame = new RectangleF(0, 0, value.Width, value.Height); }
		}

		/// <summary>
		/// Gets the frame buffer.
		/// </summary>
		/// <value>The frame buffer.</value>
		public int FrameBuffer
		{
			get { return frameBuffer; }
		}

		/// <summary>
		/// Start the drawing timer.
		/// </summary>
		public void StartDrawTimer()
		{
			StopDrawTimer();
			drawTimer = NSTimer.CreateRepeatingScheduledTimer(drawTimerInterval, HandleDrawTimer);
		}

		/// <summary>
		/// Stop the drawing timer.
		/// </summary>
		public void StopDrawTimer()
		{
			if (drawTimer != null)
			{
				drawTimer.Invalidate();
				drawTimer = null;
			}
		}

		/// <summary>
		/// Bind our buffers.
		/// </summary>
		public void Bind()
		{
			// make us the current context
			EAGLContext.SetCurrentContext(context);

			// bind the buffers
			GL.Oes.BindRenderbuffer(All.RenderbufferOes, colorRenderBuffer);
			GL.Oes.BindFramebuffer(All.FramebufferOes, frameBuffer);
		}

		/// <summary>
		/// Unbind our buffers.
		/// </summary>
		public void Unbind()
		{
			GL.Oes.BindRenderbuffer(All.RenderbufferOes, 0);
			GL.Oes.BindFramebuffer(All.FramebufferOes, 0);
		}

		public void SwapBuffers()
		{
			context.PresentRenderBuffer((int)All.RenderbufferOes);
		}

		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);

			PointF location = ((UITouch)touches.AnyObject).LocationInView(this);
			rootControl.TouchDown(new FN2DTouchEventArgs(new Point((int)location.X, (int)location.Y), FN2DTouchButtons.Left));
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			base.TouchesMoved(touches, evt);
			
			PointF location = ((UITouch)touches.AnyObject).LocationInView(this);
			rootControl.TouchMove(new FN2DTouchEventArgs(new Point((int)location.X, (int)location.Y), FN2DTouchButtons.Left));
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);
			
			PointF location = ((UITouch)touches.AnyObject).LocationInView(this);
			rootControl.TouchUp(new FN2DTouchEventArgs(new Point((int)location.X, (int)location.Y), FN2DTouchButtons.Left));
		}

		public override void TouchesCancelled(NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled(touches, evt);
			
			rootControl.TouchCancel(FN2DTouchEventArgs.Empty);
		}
	}
}
