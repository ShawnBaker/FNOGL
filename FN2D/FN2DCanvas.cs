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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
#if FN2D_WIN
using OpenTK.Graphics.OpenGL;
using FN2DBitmap = System.Drawing.Bitmap;
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using HintMode = OpenTK.Graphics.ES11.All;
using HintTarget = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using StringName = OpenTK.Graphics.ES11.All;
using TextureEnvModeCombine = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D Canvas.
	/// </summary>
	partial class FN2DCanvas
	{
		// public constants
		public const float DEFAULT_DRAW_TIMER_INTERVAL = 0.02f;

		// public default settings
		public static string DefaultFontName = "Vera";
		public static string DefaultBoldFontName = "VeraBd";
		public static float DefaultFontSize = 12;
		public static string TitleFontName = "Vera";
		public static string TitleBoldFontName = "VeraBd";
		public static float TitleFontSize = 20;
		public static string FontPreloadCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz ~!@#$%^&*()_+`-=[]\\{}|;':\",./<>?";
		public static Size DefaultButtonSize = new Size(100, 50);
		public static int DefaultCornerRadius = 5;
		public static int DefaultCornerSteps = 16;
		public static Color DefaultTopColor = Color.Blue;
		public static Color DefaultBottomColor = Color.DarkBlue;
		public static string DefaultMessageTitle = "Message";

		// public delegates
		public delegate void FN2DAnimator(float percent);

		// instance variables
		private FN2DControl rootControl = null;
		private FN2DControl touchControl = null;
		private FN2DControl modalOverlay = null;
		private FN2DControl modalControl = null;
		private bool loaded = false;
		private bool dirty = false;
		private List<FN2DAnimation> animations = new List<FN2DAnimation>();
		private float drawTimerInterval = DEFAULT_DRAW_TIMER_INTERVAL;
		//private int lastDrawTickCount = 0;
		//private Stopwatch stopWatch = new Stopwatch();

		/// <summary>
		/// Constructor - Creates the canvas.
		/// </summary>
		/// <param name="size">Size of the canvas.</param>
		public void Initialize(Size size, string fontPath)
		{
			// set the size
			Size = size;

			// initialize the font system
			FN2DFont.InitFontManager(fontPath);
			FN2DFont font = FN2DFont.Load(this, DefaultFontName, DefaultFontSize);
			font.LoadGlyphs(FontPreloadCharacters);
			font = FN2DFont.Load(this, DefaultBoldFontName, DefaultFontSize);
			font.LoadGlyphs(FontPreloadCharacters);
			font = FN2DFont.Load(this, TitleFontName, TitleFontSize);
			font.LoadGlyphs(FontPreloadCharacters);
			font = FN2DFont.Load(this, TitleBoldFontName, TitleFontSize);
			font.LoadGlyphs(FontPreloadCharacters);

			// create the root control
			rootControl = new FN2DControl(this);
			rootControl.Size = Size;

			// create the modal overlay
			modalOverlay = new FN2DControl(this);
			modalOverlay.BackgroundColor = Color.FromArgb(128, 0, 0, 0);

			// set the loaded flag
			loaded = true;
		}

		/// <summary>
		/// Frees unmanaged resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (rootControl != null) rootControl.Dispose();
			}
			rootControl = null;
			base.Dispose(disposing);
		}

		/// <summary>
		/// Updates the modal controls.
		/// </summary>
#if FN2D_WIN
		public override void Refresh()
#elif FN2D_IOS
		public virtual void Refresh()
#endif
		{
			modalOverlay.Size = Size;
			if (modalControl != null)
			{
				modalControl.Location = new Point((Size.Width - modalControl.Width) / 2, (Size.Height - modalControl.Height) / 2);
			}
		}

		/// <summary>
		/// Gets the loaded flag.
		/// </summary>
		public virtual bool IsLoaded
		{
			get { return loaded; }
		}

		/// <summary>
		/// Gets or sets the dirty flag.
		/// </summary>
		public virtual bool IsDirty
		{
			get { return dirty; }
			set { dirty = value; }
		}

		/// <summary>
		/// Gets the root control.
		/// </summary>
		public FN2DControl RootControl
		{
			get { return rootControl; }
		}

		/// <summary>
		/// Gets or sets the control that is currently being touched.
		/// </summary>
		public FN2DControl TouchControl
		{
			get { return touchControl; }
			internal set { touchControl = value; }
		}

		/// <summary>
		/// Gets or sets the root control's background color.
		/// </summary>
#if FN2D_WIN
		public Color BackgroundColor
#elif FN2D_IOS
		public new Color BackgroundColor
#endif
		{
			get { return loaded ? rootControl.BackgroundColor : Color.Transparent; }
			set
			{
				if (loaded)
				{
					rootControl.BackgroundColor = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets the root control's background image.
		/// </summary>
#if FN2D_WIN
		public new FN2DBitmap BackgroundImage
#elif FN2D_IOS
		public FN2DBitmap BackgroundImage
#endif
		{
			get { return loaded ? rootControl.BackgroundImage : null; }
			set
			{
				if (loaded)
				{
					rootControl.BackgroundImage = value;
				}
			}
		}

		/// <summary>
		/// Gets the root control's background image control.
		/// </summary>
		public FN2DImage BackgroundImageControl
		{
			get { return rootControl.BackgroundImageControl; }
		}

		/// <summary>
		/// Adds a control to the root control's list of sub-controls.
		/// </summary>
		/// <param name="control">Control to be added.</param>
		public void Add(FN2DControl control)
		{
			if (loaded)
			{
				rootControl.Add(control);
			}
		}

		/// <summary>
		/// Inserts a control into the root control's list of sub-controls at a specific position.
		/// </summary>
		/// <param name="index">Position at which to insert the control.</param>
		/// <param name="control">Control to be inserted.</param>
		public void Insert(int index, FN2DControl control)
		{
			if (loaded)
			{
				rootControl.Insert(index, control);
			}
		}

		/// <summary>
		/// Removes a control from the root control's list of sub-controls.
		/// </summary>
		/// <param name="control">The control to be removed.</param>
		/// <returns>True if the control was found and removed, false otherwise.</returns>
		public bool Remove(FN2DControl control)
		{
			return loaded ? rootControl.Remove(control) : false;
		}

		public Point ControlLocation(FN2DControl control, Point location)
		{
			if (control == rootControl || control == null)
			{
				return location;
			}
			else
			{
				location = ControlLocation(control.Parent, location);
				return new Point(location.X - control.Frame.X, location.Y - control.Frame.Y);
			}
		}

		/// <summary>
		/// Get the default font in a specific size and boldness.
		/// </summary>
		/// <param name="size">Size of the font to get.</param>
		/// <param name="bold">True to get a bold font, false to get a normal font.</param>
		/// <returns>The font.</returns>
		public FN2DFont GetFont(float size, bool bold)
		{
			return FN2DFont.Load(this, bold ? DefaultBoldFontName : DefaultFontName, (size > 0) ? size : DefaultFontSize);
		}

		/// <summary>
		/// Get the title font in a specific size and boldness.
		/// </summary>
		/// <param name="size">Size of the font to get.</param>
		/// <param name="bold">True to get a bold font, false to get a normal font.</param>
		/// <returns>The title font.</returns>
		public FN2DFont GetTitleFont(float size, bool bold)
		{
			return FN2DFont.Load(this, bold ? TitleBoldFontName : TitleFontName, (size > 0) ? size : TitleFontSize);
		}

		/// <summary>
		/// Draws the controls.
		/// </summary>
		public virtual void Draw(bool force = false)
		{
			try
			{
				if (loaded && (dirty || force))
				{
					// bind the drawing buffers
					Bind();

					// initialize the matrices
					GL.MatrixMode(MatrixMode.Projection);
					GL.PushMatrix();
					GL.LoadIdentity();
					GL.MatrixMode(MatrixMode.Modelview);
					GL.PushMatrix();
					GL.LoadIdentity();

					// enable/disable various OpenGL settings
					GL.Disable(EnableCap.DepthTest);
					GL.Enable(EnableCap.Texture2D);
					GL.Enable(EnableCap.ScissorTest);
					GL.Enable(EnableCap.LineSmooth);
					//GL.Enable(EnableCap.PolygonSmooth);
					GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
					//GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
					GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

					// configure the blending
					GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeCombine.Replace);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					GL.Enable(EnableCap.Blend);

					// draw the root control and all sub-controls
					if (rootControl.Visible)
					{
						//Console.WriteLine("--------------------------------------------------------------");
						Rectangle bounds = rootControl.DrawBegin(new Rectangle(Point.Empty, Size));
						rootControl.DrawBackground();
						rootControl.Draw();
						rootControl.DrawControls(bounds);
						rootControl.DrawEnd();
					}

					// update the screen
					SwapBuffers();

					// restore various OpenGL settings
					GL.Disable(EnableCap.LineSmooth);
					//GL.Disable(EnableCap.PolygonSmooth);
					GL.Disable(EnableCap.ScissorTest);
					GL.Disable(EnableCap.Texture2D);
					GL.Enable(EnableCap.DepthTest);

					// restore the matrices
					GL.MatrixMode(MatrixMode.Projection);
					GL.PopMatrix();
					GL.MatrixMode(MatrixMode.Modelview);
					GL.PopMatrix();

					// unbind the drawing buffers
					Unbind();

					// clear the dirty flag
					dirty = false;
					//lastDrawTickCount = Environment.TickCount;
				}
			}
			catch { }
		}

		/// <summary>
		/// Gets or sets the draw timer interval.
		/// </summary>
		public float DrawTimerInterval
		{
			get { return drawTimerInterval; }
			set
			{
				StopDrawTimer();
				drawTimerInterval = value;
				StartDrawTimer();
			}
		}

		/// <summary>
		/// Perform animations and drawing.
		/// </summary>
		private void HandleDrawTimer()
		{
			// perform the active animations
			bool force = animations.Count != 0;// || (Environment.TickCount - lastDrawTickCount) > 1000;
			foreach (FN2DAnimation animation in animations)
			{
				animation.currentTicks++;
				animation.animator(animation.currentTicks / (float)animation.totalTicks);
			}

			// remove any elapsed animations
			for (int i = animations.Count - 1; i >= 0; i--)
			{
				if (animations[i].currentTicks == animations[i].totalTicks)
				{
					animations.RemoveAt(i);
				}
			}

			// draw the controls
			//stopWatch.Reset();
			//stopWatch.Start();
			Draw(force);
			//stopWatch.Stop();
			//Console.WriteLine("Draw: " + stopWatch.ElapsedMilliseconds);
		}

		/// <summary>
		/// Shows a modal control over top of all other controls.
		/// </summary>
		/// <param name='control'>The control to be displayed.</param>
		public void ShowModal(FN2DControl control)
		{
			if (control != null)
			{
				// add the overlay
				modalOverlay.Size = Size;
				Add(modalOverlay);

				// center and add the control
				control.Location = new Point((Size.Width - control.Width) / 2, (Size.Height - control.Height) / 2);
				Add(control);

				// set the modal control
				modalControl = control;
			}
		}

		/// <summary>
		/// Hides the current modal control if there is one.
		/// </summary>
		public void HideModal()
		{
			if (modalControl != null)
			{
				Remove(modalControl);
				Remove(modalOverlay);
				modalControl = null;
			}
		}

		/// <summary>
		/// Gets the current modal control.
		/// </summary>
		public FN2DControl ModalControl
		{
			get { return modalControl; }
		}

		/// <summary>
		/// Adds an animation to the list of animations.
		/// </summary>
		/// <param name="duration">Duration of the animation in seconds.</param>
		/// <param name="animator">Function that performs the animation steps.</param>
		/// <returns>Identifier of the new animation object.</returns>
		public int Animate(float duration, FN2DAnimator animator)
		{
			int ticks = (int)Math.Round(1000 * drawTimerInterval * duration);
			FN2DAnimation animation = new FN2DAnimation(ticks, animator);
			animations.Add(animation);
			return animation.id;
		}

		/// <summary>
		/// Completes an animation and removes it from the list of animations.
		/// </summary>
		/// <param name="id">Identifier of the animation object to be completed.</param>
		public void CompleteAnimation(int id)
		{
			foreach (FN2DAnimation animation in animations)
			{
				if (animation.id == id)
				{
					animation.animator(1);
					animations.Remove(animation);
					break;
				}
			}
		}

		/// <summary>
		/// Represents an animation in progress.
		/// </summary>
		private class FN2DAnimation
		{
			private static int NextId = 0;

			public int id;
			public int totalTicks, currentTicks;
			public FN2DAnimator animator;

			public FN2DAnimation(int ticks, FN2DAnimator animator)
			{
				id = ++NextId;
				totalTicks = ticks;
				currentTicks = 0;
				this.animator = animator;
			}
		}
	}
}
