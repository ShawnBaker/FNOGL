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
using System.Collections.Generic;
#if FN2D_WIN
using OpenTK.Graphics.OpenGL;
using FN2DBitmap = System.Drawing.Bitmap;
#elif FN2D_IOS
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
using MatrixMode = OpenTK.Graphics.ES11.All;
#elif FN2D_AND
using OpenTK.Graphics.ES11;
using FN2DBitmap = Android.Graphics.Bitmap;
using MatrixMode = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D base control.
	/// </summary>
	public class FN2DControl : IDisposable
	{
		// public events
		public EventHandler Tapped = null;

		// instance variables
		protected FN2DCanvas canvas;
		protected FN2DControl parent = null;
		protected Rectangle frame;
		private FN2DControlList controls;
		protected FN2DRectangle background = new FN2DRectangle();
		protected FN2DImage backgroundImage = null;
		protected float zoom = 1;
		protected float minZoom = 0.1f;
		protected float maxZoom = 10;
		protected Size pan = Size.Empty;
		protected float rotation = 0;
		protected bool touchEnabled = true;
		protected bool touching = false;
		protected bool enabled = true;
		protected bool visible = true;
		protected bool refreshEnabled = true;
		protected int tag = 0;

		/// <summary>
		/// Constructor - Creates a control with a gradient background with rounded corners.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="frame">Position and size of the control.</param>
		/// <param name="cornerRadius">Radius for rounded corners.</param>
		/// <param name="topColor">Top color for the gradient background.</param>
		/// <param name="bottomColor">Bottom color for the gradient background.</param>
		public FN2DControl(FN2DCanvas canvas, Rectangle frame, int cornerRadius, Color topColor, Color bottomColor)
		{
			if (canvas == null)
			{
				throw new ArgumentNullException("FN2DControl: The canvas cannot be null.");
			}
			controls = new FN2DControlList(this);
			this.canvas = canvas;
			background.CornerRadius = cornerRadius;
			background.TopColor = topColor;
			background.BottomColor = bottomColor;
			Frame = frame;
		}

		/// <summary>
		/// Constructor - Creates a control with a solid background with rounded corners.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="frame">Position and size of the control.</param>
		/// <param name="cornerRadius">Radius for rounded corners.</param>
		/// <param name="backgroundColor">Color of the background.</param>
		public FN2DControl(FN2DCanvas canvas, Rectangle frame, int cornerRadius, Color backgroundColor)
			: this(canvas, frame, cornerRadius, backgroundColor, backgroundColor)
		{
		}

		/// <summary>
		/// Constructor - Creates a control with a gradient background.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="frame">Position and size of the control.</param>
		/// <param name="topColor">Top color for the gradient background.</param>
		/// <param name="bottomColor">Bottom color for the gradient background.</param>
		public FN2DControl(FN2DCanvas canvas, Rectangle frame, Color topColor, Color bottomColor)
			: this(canvas, frame, 0, topColor, bottomColor)
		{
		}

		/// <summary>
		/// Constructor - Creates a control with a solid background.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="frame">Position and size of the control.</param>
		/// <param name="backgroundColor">Color of the background.</param>
		public FN2DControl(FN2DCanvas canvas, Rectangle frame, Color backgroundColor)
			: this(canvas, frame, 0, backgroundColor, backgroundColor)
		{
		}

		/// <summary>
		/// Constructor - Creates a control with no background.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="frame">Position and size of the control.</param>
		public FN2DControl(FN2DCanvas canvas, Rectangle frame)
			: this(canvas, frame, Color.Transparent)
		{
		}

		/// <summary>
		/// Constructor - Creates a control with no background or frame.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		public FN2DControl(FN2DCanvas canvas)
			: this(canvas, Rectangle.Empty, Color.Transparent)
		{
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DControl()
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
				if (controls != null)
				{
					for (int i = controls.Count - 1; i >= 0; i--)
					{
						FN2DControl control = controls[i];
						controls.RemoveAt(i);
						control.Dispose();
					}
				}
				if (background != null) background.Dispose();
				if (backgroundImage != null) backgroundImage.Dispose();
			}

			// clear the object references
			canvas = null;
			controls = null;
			background = null;
			backgroundImage = null;
		}

		/// <summary>
		/// Initializes the drawing state.
		/// </summary>
		/// <param name="parentBounds">Bounds of the parent control.</param>
		public virtual Rectangle DrawBegin(Rectangle parentBounds)
		{
			// get the control bounds
			Rectangle bounds = new Rectangle(parentBounds.X + X, parentBounds.Y + Y, Width, Height);
			//Console.WriteLine("DrawBegin: " + parentBounds + "   " + frame + "   " + bounds);

			// reset the project matrix
			GL.MatrixMode(MatrixMode.Projection);
			GL.PushMatrix();
			GL.LoadIdentity();

			// apply zoom and pan to the bounds
			SizeF zoomOffset = new SizeF((bounds.Width / zoom - bounds.Width) / 2, (bounds.Height / zoom - bounds.Height) / 2);
			PointF zoomTL = new PointF(-zoomOffset.Width, -zoomOffset.Height);
			PointF zoomBR = new PointF(bounds.Width + zoomOffset.Width, bounds.Height + zoomOffset.Height);
			PointF panTL = new PointF(zoomTL.X - pan.Width, zoomTL.Y - pan.Height);
			PointF panBR = new PointF(zoomBR.X - pan.Width, zoomBR.Y - pan.Height);

			// set the orthographic projection for the control
			GL.Ortho(panTL.X, panBR.X, panBR.Y, panTL.Y, -1, 1);

			// bound the control to its screen location
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PushMatrix();
			int y = (canvas != null) ? ((int)canvas.Size.Height - bounds.Y - bounds.Height) : bounds.Y;
			GL.Viewport(bounds.X, y, bounds.Width, bounds.Height);
			bounds.Intersect(parentBounds);
			y = (canvas != null) ? ((int)canvas.Size.Height - bounds.Y - bounds.Height) : bounds.Y;
			GL.Scissor(bounds.X, y, bounds.Width, bounds.Height);
			//Console.WriteLine("DrawBegin: " + parentBounds + "   " + frame + "   " + bounds + "   " + y);

			if (rotation != 0)
			{
				GL.Translate(bounds.Width / 2, bounds.Height / 2, 0);
				GL.Rotate(rotation, 0, 0, 1);
				GL.Translate(-bounds.Width / 2, -bounds.Height / 2, 0);
			}

			return bounds;
		}

		/// <summary>
		/// Restore the state after drawing
		/// </summary>
		public virtual void DrawEnd()
		{
			GL.MatrixMode(MatrixMode.Projection);
			GL.PopMatrix();
			GL.MatrixMode(MatrixMode.Modelview);
			GL.PopMatrix();
		}

		/// <summary>
		/// Draws the background.
		/// </summary>
		public virtual void DrawBackground()
		{
			GL.Color4(0, 0, 0, 0);
			background.Draw();
			if (backgroundImage != null)
			{
				backgroundImage.Draw();
			}
		}

		/// <summary>
		/// Draws the control.
		/// </summary>
		public virtual void Draw()
		{
		}

		/// <summary>
		/// Draws the sub-controls.
		/// </summary>
		public virtual void DrawControls(Rectangle bounds)
		{
			// draw the controls
			foreach (FN2DControl control in controls)
			{
				if (control.Visible)
				{
					// initialize the state
					Rectangle controlBounds = control.DrawBegin(bounds);

					// draw the control and all of its sub-controls
					control.DrawBackground();
					control.Draw();
					control.DrawControls(controlBounds);

					// restore the state
					control.DrawEnd();
				}
			}
		}

		/// <summary>
		/// Gets or sets the canvas.
		/// </summary>
		public virtual FN2DCanvas Canvas
		{
			get { return canvas; }
			set
			{
				canvas = value;
				foreach (FN2DControl control in controls)
				{
					control.Canvas = canvas;
				}
			}
		}

		/// <summary>
		/// Gets the list of sub-controls.
		/// </summary>
		public FN2DControlList Controls
		{
			get { return controls; }
		}

		/// <summary>
		/// Gets or sets the corner radius.
		/// </summary>
		public virtual int CornerRadius
		{
			get { return background.CornerRadius; }
			set
			{
				background.CornerRadius = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the top background color.
		/// </summary>
		public virtual Color TopColor
		{
			get { return background.TopColor; }
			set
			{
				background.TopColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the bottom background color.
		/// </summary>
		public virtual Color BottomColor
		{
			get { return background.BottomColor; }
			set
			{
				background.BottomColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		public virtual Color BackgroundColor
		{
			get { return background.Color; }
			set
			{
				background.Color = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets the background image.
		/// </summary>
		public virtual FN2DBitmap BackgroundImage
		{
			get { return (backgroundImage != null) ? backgroundImage.Image : null; }
			set
			{
				if (value != null)
				{
					if (backgroundImage == null)
					{
						backgroundImage = new FN2DImage(canvas, value);
					}
					else
					{
						backgroundImage.Image = value;
					}
				}
				else
				{
					backgroundImage.Dispose();
					backgroundImage = null;
				}
				Refresh();
			}
		}

		/// <summary>
		/// Gets the background image control.
		/// </summary>
		public virtual FN2DImage BackgroundImageControl
		{
			get { return backgroundImage; }
		}

		/// <summary>
		/// Gets or sets the zoom factor.
		/// </summary>
		public virtual float Zoom
		{
			get { return zoom; }
			set
			{
				// range check the new zoom
				float newZoom = value;
				if (newZoom < minZoom) newZoom = minZoom;
				else if (newZoom > maxZoom) newZoom = maxZoom;

				// if the zoom has changed
				if (newZoom != zoom)
				{
					// set the zoom
					float ratio = newZoom / zoom;
					zoom = newZoom;
					//Console.WriteLine("Zoom = " + zoom + "  " + minZoom + "  " + maxZoom);
					canvas.IsDirty = true;

					// adjust the pan by the relative zoom change
					Pan = new Size((int)(pan.Width * ratio), (int)(pan.Height * ratio));
				}
			}
		}

		/// <summary>
		/// Gets or sets the minimum zoom.
		/// </summary>
		public virtual float MinZoom
		{
			get { return minZoom; }
			set { minZoom = value; }
		}

		/// <summary>
		/// Gets or sets the maximum zoom.
		/// </summary>
		public virtual float MaxZoom
		{
			get { return maxZoom; }
			set { maxZoom = value; }
		}

		/// <summary>
		/// Gets or sets the pan.
		/// </summary>
		public virtual Size Pan
		{
			get { return pan; }
			set
			{
				// range check the new pan
				Size newPan = value;
				Size maxPan = MaxPan;
				if (maxPan.Width == 0) newPan.Width = 0;
				else if (newPan.Width < -maxPan.Width) newPan.Width = -maxPan.Width;
				else if (newPan.Width > maxPan.Width) newPan.Width = maxPan.Width;
				if (maxPan.Height == 0) newPan.Height = 0;
				else if (newPan.Height < -maxPan.Height) newPan.Height = -maxPan.Height;
				else if (newPan.Height > maxPan.Height) newPan.Height = maxPan.Height;

				// if the pan has changed, set it
				if (newPan != pan)
				{
					pan = newPan;
					//Console.WriteLine("Pan = " + pan + "  " + MaxPan);
					canvas.IsDirty = true;
				}
			}
		}

		/// <summary>
		/// Gets the maximum pan.
		/// </summary>
		public virtual Size MaxPan
		{
			get { return new Size((int)Math.Max(Math.Round(-(Width / Zoom - Width) / 2), 0), (int)Math.Max(Math.Round(-(Height / Zoom - Height) / 2), 0)); }
		}

		/// <summary>
		/// Determines whether or not panning is possible.
		/// </summary>
		public virtual bool CanPan
		{
			get { return MaxPan != Size.Empty; }
		}

		/// <summary>
		/// Gets or sets the rotation angle.
		/// </summary>
		public virtual float Rotation
		{
			get { return rotation; }
			set
			{
				if (value != rotation)
				{
					rotation = value;
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets the frame.
		/// </summary>
		public virtual Rectangle Frame
		{
			get { return frame; }
			set
			{
				// determine what has changed
				bool moved = value.X != frame.X || value.Y != frame.Y;
				bool resized = value.Width != frame.Width || value.Height != frame.Height;

				// fire the before events
				if (moved)
				{
					BeforeMove();
				}
				if (resized)
				{
					BeforeResize();
				}

				// change the frame
				frame = value;

				// fire the after events
				if (moved)
				{
					AfterMove();
				}
				if (resized)
				{
					AfterResize();
				}
			}
		}

		/// <summary>
		/// Gets or sets the location.
		/// </summary>
		public virtual Point Location
		{
			get { return frame.Location; }
			set { Frame = new Rectangle(value, frame.Size); }
		}

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		public virtual Size Size
		{
			get { return frame.Size; }
			set { Frame = new Rectangle(frame.Location, value); }
		}

		/// <summary>
		/// Gets or sets the x position.
		/// </summary>
		public virtual int X
		{
			get { return frame.X; }
			set { Frame = new Rectangle(value, frame.Y, frame.Width, frame.Height); }
		}

		/// <summary>
		/// Gets or sets the y position.
		/// </summary>
		public virtual int Y
		{
			get { return frame.Y; }
			set { Frame = new Rectangle(frame.X, value, frame.Width, frame.Height); }
		}

		/// <summary>
		/// Gets or sets the width.
		/// </summary>
		public virtual int Width
		{
			get { return frame.Width; }
			set { Frame = new Rectangle(frame.X, frame.Y, value, frame.Height); }
		}

		/// <summary>
		/// Gets or sets the height.
		/// </summary>
		public virtual int Height
		{
			get { return frame.Height; }
			set { Frame = new Rectangle(frame.X, frame.Y, frame.Width, value); }
		}

		/// <summary>
		/// Gets the right position (x + width).
		/// </summary>
		public virtual int Right
		{
			get { return frame.X + frame.Width; }
		}

		/// <summary>
		/// Gets the bottom position (y + height).
		/// </summary>
		public virtual int Bottom
		{
			get { return frame.Y + frame.Height; }
		}

		/// <summary>
		/// Gets or sets the center position.
		/// </summary>
		public virtual Point Center
		{
			get { return new Point(frame.X + frame.Width / 2, frame.Y + frame.Height / 2); }
			set { Location = new Point(value.X - frame.Width / 2, value.Y - frame.Height / 2); }
		}

		/// <summary>
		/// Sizes the control to fit all the sub-controls with an optional margin.
		/// </summary>
		public virtual void SizeToFit(int margin = 0)
		{
			// make sure the margin is positive
			if (margin < 0)
			{
				margin = 0;
			}

			// find the top-left and bottom-right locations
			Point topLeft = Point.Empty;
			Point bottomRight = Point.Empty;
			foreach (FN2DControl control in controls)
			{
				if (control.X < topLeft.X)
				{
					topLeft.X = control.X;
				}
				if (control.Y < topLeft.Y)
				{
					topLeft.Y = control.Y;
				}
				if (control.Right > bottomRight.X)
				{
					bottomRight.X = control.Right;
				}
				if (control.Bottom > bottomRight.Y)
				{
					bottomRight.Y = control.Bottom;
				}
			}

			// position the controls
			foreach (FN2DControl control in controls)
			{
				control.Location = new Point(control.X - topLeft.X + margin, control.Y - topLeft.Y + margin);
			}

			// set the size
			Size = new Size(bottomRight.X - topLeft.X + margin * 2, bottomRight.Y - topLeft.Y + margin * 2);
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		public virtual int Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		/// <summary>
		/// Gets or sets whether or not the control is visible.
		/// </summary>
		public virtual bool Visible
		{
			get { return visible; }
			set
			{
				if (value != visible)
				{
					visible = value;
					if (!visible)
					{
						Touching = false;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets whether or not the control is enabled.
		/// </summary>
		public virtual bool Enabled
		{
			get { return enabled; }
			set
			{
				if (value != enabled)
				{
					enabled = value;
					if (!enabled)
					{
						Touching = false;
					}
					foreach (FN2DControl control in controls)
					{
						control.Enabled = enabled;
					}
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets whether or not refreshing is enabled.
		/// </summary>
		public virtual bool RefreshEnabled
		{
			get { return refreshEnabled; }
			set
			{
				if (value != refreshEnabled)
				{
					refreshEnabled = value;
					if (refreshEnabled)
					{
						Refresh();
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets whether or not touch input is enabled.
		/// </summary>
		public virtual bool TouchEnabled
		{
			get { return touchEnabled; }
			set
			{
				if (value != touchEnabled)
				{
					touchEnabled = value;
					if (!touchEnabled)
					{
						Touching = false;
					}
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets whether or not touching is active.
		/// </summary>
		public virtual bool Touching
		{
			get { return touching; }
			set
			{
				touching = value;
				canvas.TouchControl = touching ? this : null;
			}
		}

		/// <summary>
		/// Perform special actions before being resized.
		/// </summary>
		public virtual void BeforeResize()
		{
		}

		/// <summary>
		/// Perform special actions after being resized.
		/// </summary>
		public virtual void AfterResize()
		{
			Refresh();
		}

		/// <summary>
		/// Perform special actions before being moved.
		/// </summary>
		public virtual void BeforeMove()
		{
		}

		/// <summary>
		/// Perform special actions after being moved.
		/// </summary>
		public virtual void AfterMove()
		{
			canvas.IsDirty = true;
		}

		/// <summary>
		/// Refreshes the OpenGL drawing values after an update of the control values.
		/// </summary>
		public virtual void Refresh()
		{
			if (refreshEnabled)
			{
				// set the background sizes
				background.Frame = new Rectangle(Point.Empty, Size);
				if (backgroundImage != null)
				{
					backgroundImage.Size = Size;
					backgroundImage.Fill();
				}

				// set the dirty flag
				canvas.IsDirty = true;
			}
		}

		/// <summary>
		/// Gets or sets the control that contains this control.
		/// </summary>
		public virtual FN2DControl Parent
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Adds a control to the root control's list of sub-controls.
		/// </summary>
		/// <param name="control">Control to be added.</param>
		public virtual void Add(FN2DControl control)
		{
			controls.Add(control);
		}

		/// <summary>
		/// Inserts a control into the root control's list of sub-controls at a specific position.
		/// </summary>
		/// <param name="index">Position at which to insert the control.</param>
		/// <param name="control">Control to be inserted.</param>
		public virtual void Insert(int index, FN2DControl control)
		{
			controls.Insert(index, control);
		}

		/// <summary>
		/// Removes a control from the root control's list of sub-controls.
		/// </summary>
		public virtual bool Remove(FN2DControl control)
		{
			return controls.Remove(control);
		}

		/// <summary>
		/// Determines if a control is a sub-control of this control.
		/// </summary>
		/// <param name="control">Control to look for in this control.</param>
		/// <returns>True if found, false if not</returns>
		public virtual bool Contains(FN2DControl control)
		{
			foreach (FN2DControl ctrl in controls)
			{
				if (ctrl == control || ctrl.Contains(control))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the topmost sub-control that contains a specific location.
		/// </summary>
		/// <param name="location">Location to test.</param>
		/// <returns>Topmost control if the location is within this control, null if not.</returns>
		public virtual FN2DControl HitTest(Point location)
		{
			if (Visible && Enabled && TouchEnabled && Frame.Contains(location))
			{
				Point controlLocation = new Point(location.X - Frame.X, location.Y - Frame.Y);
				for (int i = controls.Count - 1; i >= 0; i--)
				{
					FN2DControl hitControl = controls[i].HitTest(controlLocation);
					if (hitControl != null)
					{
						location.X = controlLocation.X;
						location.Y = controlLocation.Y;
						return hitControl;
					}
				}
				return this;
			}
			return null;
		}

		/// <summary>
		/// Called when a touch down event occurs on the control.
		/// </summary>
		/// <param name="e">Touch event arguments.</param>
		public virtual void TouchDown(FN2DTouchEventArgs e)
		{
		}

		/// <summary>
		/// Called when a touch move event occurs when this is the current touch control.
		/// </summary>
		/// <param name="e">Touch event arguments.</param>
		public virtual void TouchMove(FN2DTouchEventArgs e)
		{
		}

		/// <summary>
		/// Called when a touch up event occurs when this is the current touch control.
		/// </summary>
		/// <param name="e">Touch event arguments.</param>
		public virtual void TouchUp(FN2DTouchEventArgs e)
		{
			if (Tapped != null && Touching && e.Location.X >= 0 && e.Location.X < Frame.Width &&
				e.Location.Y >= 0 && e.Location.Y < Frame.Height)
			{
				Tapped(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Called when a touch cancel event occurs when this is the current touch control.
		/// </summary>
		/// <param name="e">Touch event arguments.</param>
		public virtual void TouchCancel(FN2DTouchEventArgs e)
		{
		}
	}

	/// <summary>
	/// OpenGL 2D list of controls.
	/// </summary>
	public class FN2DControlList : List<FN2DControl>
	{
		private FN2DControl owner;

		public FN2DControlList(FN2DControl owner = null)
		{
			this.owner = owner;
		}

		public new void Add(FN2DControl control)
		{
			if (control != null && owner != null)
			{
				control.Parent = owner;
			}
			base.Add(control);
		}

		public new void AddRange(IEnumerable<FN2DControl> collection)
		{
			if (owner != null)
			{
				foreach (FN2DControl control in collection)
				{
					if (control != null)
					{
						control.Parent = owner;
					}
				}
			}
			base.AddRange(collection);
		}

		public new void Insert(int index, FN2DControl control)
		{
			if (control != null && owner != null)
			{
				control.Parent = owner;
			}
			base.Insert(index, control);
		}

		public new void InsertRange(int index, IEnumerable<FN2DControl> collection)
		{
			if (owner != null)
			{
				foreach (FN2DControl control in collection)
				{
					if (control != null)
					{
						control.Parent = owner;
					}
				}
			}
			base.InsertRange(index, collection);
		}

		public new bool Remove(FN2DControl control)
		{
			if (control != null)
			{
				control.Parent = null;
			}
			return base.Remove(control);
		}

		public new void RemoveAt(int index)
		{
			if (index >= 0 && index < Count)
			{
				this[index].Parent = null;
			}
			base.RemoveAt(index);
		}
		
		public new void RemoveRange(int index, int count)
		{
			int n = 0;
			for (int i = index; i < Count && n < count; i++, n++)
			{
				this[i].Parent = null;
			}
			base.RemoveRange(index, count);
		}
		
		public new int RemoveAll(Predicate<FN2DControl> match)
		{
			foreach (FN2DControl control in this)
			{
				control.Parent = null;
			}
			return base.RemoveAll(match);
		}

		public virtual void BringToFront(FN2DControl control)
		{
			int i = IndexOf(control);
			if (i != -1 && i != Count - 1)
			{
				Remove(control);
				Add(control);
			}
		}

		public virtual void SendToBack(FN2DControl control)
		{
			int i = IndexOf(control);
			if (i > 0)
			{
				Remove(control);
				Insert(0, control);
			}
		}
	}

	public class FN2DTouchEventArgs : EventArgs
	{
		public Point Location;

		public FN2DTouchEventArgs(int x, int y)
		{
			Location.X = x;
			Location.Y = y;
		}

		public FN2DTouchEventArgs(Point location)
			: this(location.X, location.Y)
		{
		}

		public FN2DTouchEventArgs(PointF location)
			: this((int)location.X, (int)location.Y)
		{
		}

		public new static FN2DTouchEventArgs Empty
		{
			get { return new FN2DTouchEventArgs(Point.Empty); }
		}
	}
}
