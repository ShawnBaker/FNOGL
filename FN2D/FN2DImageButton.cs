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
using FN2DBitmap = System.Drawing.Bitmap;
#elif FN2D_IOS
using MonoTouch.UIKit;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D image button.
	/// </summary>
	public class FN2DImageButton : FN2DButton
	{
		// instance variables
		protected FN2DBitmap normalImage, pressedImage, selectedImage;
		protected FN2DBitmap disabledImage, disabledSelectedImage;

		/// <summary>
		/// Constructor - Creates a button based on normal, pressed and selected images.
		/// </summary>
		/// <param name="normalImage">Normal button image.</param>
		/// <param name="pressedImage">Image used when the button is pressed.</param>
		/// <param name="disabledImage">Image used when the button is disabled.</param>
		/// <param name="selectedImage">Image used when the button is selected.</param>
		/// <param name="disabledSelectedImage">Image used when the button is disabled and selected.</param>
		public FN2DImageButton(FN2DCanvas canvas, FN2DBitmap normalImage, FN2DBitmap pressedImage, FN2DBitmap disabledImage,
								FN2DBitmap selectedImage, FN2DBitmap disabledSelectedImage)
			: base(canvas, new Rectangle(0, 0, (int)normalImage.Size.Width, (int)normalImage.Size.Height))
		{
			// disable refreshing
			RefreshEnabled = false;

			// set the images
			NormalImage = normalImage;
			PressedImage = pressedImage;
			DisabledImage = disabledImage;
			SelectedImage = selectedImage;
			DisabledSelectedImage = disabledSelectedImage;

			// configure the background
			TopColor = Color.Transparent;
			BottomColor = Color.Transparent;
			BackgroundColor = Color.Transparent;
			BackgroundImage = normalImage;

			// enable refreshing
			RefreshEnabled = true;
		}

		/// <summary>
		/// Constructor - Creates a button based on normal and pressed images.
		/// </summary>
		/// <param name="normalImage">Normal button image.</param>
		/// <param name="pressedImage">Image used when the button is pressed.</param>
		/// <param name="disabledImage">Image used when the button is disabled.</param>
		public FN2DImageButton(FN2DCanvas canvas, FN2DBitmap normalImage, FN2DBitmap pressedImage, FN2DBitmap disabledImage)
			: this(canvas, normalImage, pressedImage, disabledImage, null, null)
		{
		}

		/// <summary>
		/// Frees unmanaged resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				if (normalImage != null) normalImage.Dispose();
				if (pressedImage != null) pressedImage.Dispose();
				if (selectedImage != null) selectedImage.Dispose();
				if (disabledImage != null) disabledImage.Dispose();
				if (disabledSelectedImage != null) disabledSelectedImage.Dispose();
			}

			// clear the object references
			normalImage = null;
			pressedImage = null;
			selectedImage = null;
			disabledImage = null;
			disabledSelectedImage = null;

			// call the base handler
			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets or sets the insets to be used with the background image.
		/// </summary>
		public FN2DRectangleInsets Insets
		{
			get { return (backgroundImage != null) ? backgroundImage.Insets : FN2DRectangleInsets.Empty; }
			set
			{
				if (backgroundImage != null)
				{
					backgroundImage.Insets = value;
				}
			}
		}

		/// <summary>
		/// Sets the button images based on the current state.
		/// </summary>
		public override void Refresh()
		{
			if (refreshEnabled)
			{
				base.Refresh();

				// don't do anything until the control has been fully created
				if (backgroundImage == null)
				{
					return;
				}

				// get the background image
				FN2DBitmap image = null;
				if (!Enabled)
				{
					image = (selected && disabledSelectedImage != null) ? disabledSelectedImage : disabledImage;
				}
				else if (selected)
				{
					image = touching ? normalImage : (selectedImage ?? (pressedImage ?? normalImage));
				}
				else
				{
					image = touching ? (pressedImage ?? normalImage) : normalImage;
				}

				// set the background image
				backgroundImage.Image = image;
				backgroundImage.Size = Size;
				background.Color = Color.Transparent;
			}
		}

		/// <summary>
		/// Sets/gets the normal background image.
		/// </summary>
		public FN2DBitmap NormalImage
		{
			get { return normalImage; }
			set
			{
				normalImage = value;
				Size = new Size((int)normalImage.Size.Width, (int)normalImage.Size.Height);
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the pressed background image.
		/// </summary>
		public FN2DBitmap PressedImage
		{
			get { return pressedImage; }
			set
			{
				pressedImage = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the selected background image.
		/// </summary>
		public FN2DBitmap SelectedImage
		{
			get { return selectedImage; }
			set
			{
				selectedImage = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the disabled background image.
		/// </summary>
		public FN2DBitmap DisabledImage
		{
			get { return disabledImage; }
			set
			{
				disabledImage = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the disabled selected background image.
		/// </summary>
		public FN2DBitmap DisabledSelectedImage
		{
			get { return disabledSelectedImage; }
			set
			{
				disabledSelectedImage = value;
				Refresh();
			}
		}
	}
}
