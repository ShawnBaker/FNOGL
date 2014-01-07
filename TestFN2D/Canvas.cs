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
using System.IO;
using System.Reflection;
#if IOS
using MonoTouch.UIKit;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
#elif ANDROID
using Android.Content;
using FN2DBitmap = Android.Graphics.Bitmap;
#else
using FN2DBitmap = System.Drawing.Bitmap;
#endif
using FrozenNorth.OpenGL.FN2D;

namespace FrozenNorth.TestFN2D
{
	public abstract class CanvasCommon : FN2DCanvas
	{
#if ANDROID
#endif
		protected FN2DDrawingImage image;
		protected FN2DFont font;
		protected FN2DImage rotating;
		protected FN2DLabel label;
		protected FN2DImageButton button;
		protected FN2DButton btn;
		protected FN2DMessage message;
		protected int ticks = 0;

#if ANDROID
		public CanvasCommon(Context context, Size size, string fontPath = null)
			: base(context, size, fontPath)
		{
		}
#else
		public CanvasCommon(Size size, string fontPath = null)
			: base(size, fontPath)
		{
		}
#endif

		public override void OnInitialized()
		{
			// call the base method
			base.OnInitialized();

			// set the background
			BackgroundColor = Color.FromArgb(0, 100, 50);

			// create the image control
			image = new FN2DDrawingImage(this, Color.Yellow, 2, GetImage("SkaraBrae.jpg"));
			Add(image);

			// display a font atlas
			font = GetFont(16, false);
			font.LoadGlyphs(FontPreloadCharacters);
			font.Atlas.Location = new Point(10, 10);
			Add(font.Atlas);

			// display a rotating image
			rotating = CreateImage("OldPenny", 150, 10);
			rotating.BackgroundColor = Color.FromArgb(128, 255, 255, 255);

			// display a label that cycles through the alignments
			label = new FN2DLabel(this, "Hello\nSome Words\nGoodbye");
			label.Frame = new Rectangle(10, 150, 100, 100);
			label.Alignment = FN2DTextAlignment.MiddleCenter;
			label.TextColor = Color.Yellow;
			label.BackgroundColor = Color.SaddleBrown;
			Add(label);

			// display some image based buttons
			button = CreateButton("red", 150, 150);
			button.Insets = new FN2DRectangleInsets(button.Size);
			button.Tapped += HandleRedButtonTapped;
			button = CreateButton("green", 200, 150);
			button.Insets = new FN2DRectangleInsets(button.Size);
			button.Width *= 2;
			button = CreateButton("blue", 150, 200);
			button.Insets = new FN2DRectangleInsets(button.Size);
			button.Size = new Size(button.Width * 3, button.Height * 3 / 2);
			button.SetIcons(GetImage("PurpleSquares"), GetImage("PurpleSquaresDisabled"));
			button.Tapped += HandleBlueButtonTapped;

			// display a label
			FN2DLabel label2 = new FN2DLabel(this, "The quick brown fox jumps over the lazy dog.");
			label2.Frame = new Rectangle(10, 280, 300, 27);
			label2.BackgroundColor = Color.White;
			label2.TextColor = Color.Black;
			label2.CornerRadius = 5;
			Add(label2);

			// display some color based buttons
			btn = new FN2DButton(this, new Rectangle(10, 320, 50, 40), Color.Red, Color.DarkRed, "Undo");
			btn.Tapped += HandleUndoButtonTapped;
			Add(btn);
			btn = new FN2DButton(this, new Rectangle(70, 320, 80, 40), Color.Green, Color.DarkGreen, "Erase");
			btn.Tapped += HandleEraseButtonTapped;
			Add(btn);
			btn = new FN2DButton(this, new Rectangle(160, 320, 120, 60), "Blue");
			btn.SetIcons(GetImage("YellowCircles"), GetImage("YellowCirclesDisabled"));
			btn.Tapped += HandleBlueButtonTapped;
			Add(btn);

			// update all the controls
			Refresh();

			// create and start the drawing timer
			StartDrawTimer();
		}

		/// <summary>
		/// Frees unmanaged resources and calls Dispose() on the member objects.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (message != null) message.Dispose();
			}
			message = null;
			base.Dispose(disposing);
		}

		public override void Refresh()
		{
			base.Refresh();
			if (image != null)
			{
				image.Size = Size;
				image.Fill();
			}
		}

		public override void Draw(bool force = false)
		{
			// cycle the rotating image through 360 degrees
			if (rotating != null)
			{
				float angle = rotating.Rotation + 2;
				rotating.Rotation = (angle < 360) ? angle : 0;
			}

			// cycle through the label alignments
			if (++ticks % 50 == 0)
			{
				if (label != null)
				{
					switch (label.Alignment)
					{
						case FN2DTextAlignment.TopLeft: label.Alignment = FN2DTextAlignment.TopCenter; break;
						case FN2DTextAlignment.TopCenter: label.Alignment = FN2DTextAlignment.TopRight; break;
						case FN2DTextAlignment.TopRight: label.Alignment = FN2DTextAlignment.MiddleLeft; break;
						case FN2DTextAlignment.MiddleLeft: label.Alignment = FN2DTextAlignment.MiddleCenter; break;
						case FN2DTextAlignment.MiddleCenter: label.Alignment = FN2DTextAlignment.MiddleRight; break;
						case FN2DTextAlignment.MiddleRight: label.Alignment = FN2DTextAlignment.BottomLeft; break;
						case FN2DTextAlignment.BottomLeft: label.Alignment = FN2DTextAlignment.BottomCenter; break;
						case FN2DTextAlignment.BottomCenter: label.Alignment = FN2DTextAlignment.BottomRight; break;
						case FN2DTextAlignment.BottomRight: label.Alignment = FN2DTextAlignment.TopLeft; break;
					}
				}
				if (button != null)
				{
					switch (button.IconAlignment)
					{
						case FN2DIconAlignment.Left: button.IconAlignment = FN2DIconAlignment.Above; break;
						case FN2DIconAlignment.Above: button.IconAlignment = FN2DIconAlignment.Right; break;
						case FN2DIconAlignment.Right: button.IconAlignment = FN2DIconAlignment.Below; break;
						case FN2DIconAlignment.Below: button.IconAlignment = FN2DIconAlignment.Left; break;
					}
				}
			}
			base.Draw(force);
		}

		private void HandleRedButtonTapped(object sender, EventArgs e)
		{
			button.Enabled = !button.Enabled;
		}

		private void HandleUndoButtonTapped(object sender, EventArgs e)
		{
			FN2DArrays lineStrip = image.LineStrips.LineStrip;
			if (lineStrip != null)
			{
				image.LineStrips.Remove(lineStrip);
				IsDirty = true;
			}
		}

		private void HandleEraseButtonTapped(object sender, EventArgs e)
		{
			image.LineStrips.Clear();
			IsDirty = true;
		}

		private void HandleBlueButtonTapped(object sender, EventArgs e)
		{
			if (message != null)
			{
				message.Dispose();
				message = null;
			}
			message = new FN2DMessage(this, "This is a test message just for you.\nNot long, but multi-line.");
			ShowModal(message);
		}

		private FN2DImage CreateImage(string fileName, int x, int y)
		{
			FN2DImage image = new FN2DImage(this, GetImage(fileName));
			image.Location = new Point(x, y);
			Add(image);
			return image;
		}

		private FN2DImageButton CreateButton(string color, int x, int y)
		{
			FN2DImageButton button = new FN2DImageButton(this, GetImage("button_" + color), GetImage("button_" + color + "_pressed"),
			                                           GetImage("button_disabled"));
			button.Location = new Point(x, y);
			button.Label.Font = GetFont(12, true);
			button.Title = color.Substring(0, 1).ToUpper() + color.Substring(1);
			Add(button);
			return button;
		}

		public abstract FN2DBitmap GetImage(string fileName);
	}
}
