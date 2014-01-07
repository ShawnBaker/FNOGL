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
using FN2DBitmap = System.Drawing.Bitmap;
#elif FN2D_IOS
using MonoTouch.UIKit;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
#elif FN2D_AND
using FN2DBitmap = Android.Graphics.Bitmap;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D button base.
	/// </summary>
	public class FN2DButton : FN2DControl
	{
		// public constants
		public static int IconTextPadding = 3;

		// instance variables
		protected FN2DBitmap normalIcon, disabledIcon, selectedIcon, disabledSelectedIcon;
		protected FN2DImage iconImage;
		protected FN2DLabel titleLabel;
		protected Color topColor, bottomColor, topPressedColor, bottomPressedColor, topDisabledColor, bottomDisabledColor;
		protected bool selected = false;
		protected FN2DIconAlignment iconAlignment = FN2DIconAlignment.Left;
		protected Point iconOffsets = Point.Empty;
		protected Point titleOffsets = Point.Empty;

		/// <summary>
		/// Constructor - Creates a colored button based on top and bottom colors.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="frame">Position and size of the button.</param>
		/// <param name="cornerRadius">Corner radius.</param>
		/// <param name="topColor">Top color.</param>
		/// <param name="bottomColor">Bottom color.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, Rectangle frame, int cornerRadius, Color topColor, Color bottomColor, string title = "")
			: base(canvas, frame, cornerRadius, topColor, bottomColor)
		{
			// disable refreshing
			RefreshEnabled = false;

			// create the icon image
			iconImage = new FN2DImage(canvas);
			Add(iconImage);

			// create the label
			titleLabel = new FN2DLabel(canvas, title);
			titleLabel.Font = canvas.GetFont(FN2DCanvas.DefaultFontSize, true);
			titleLabel.AutoSize = true;
			Add(titleLabel);

			// set the colors
			TopColor = topColor;
			BottomColor = bottomColor;

			// enable refreshing
			RefreshEnabled = true;
		}

		/// <summary>
		/// Constructor - Creates a colored button based on top and bottom colors using the default corner radius.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="frame">Position and size of the button.</param>
		/// <param name="topColor">Top color.</param>
		/// <param name="bottomColor">Bottom color.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, Rectangle frame, Color topColor, Color bottomColor, string title = "")
			: this(canvas, frame, FN2DCanvas.DefaultCornerRadius, topColor, bottomColor, title)
		{
		}

		/// <summary>
		/// Constructor - Creates a colored button based on a single color using the default corner radius.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="frame">Position and size of the button.</param>
		/// <param name="color">Top and bottom color.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, Rectangle frame, Color color, string title = "")
			: this(canvas, frame, FN2DCanvas.DefaultCornerRadius, color, color, title)
		{
		}

		/// <summary>
		/// Constructor - Creates a colored button based on the default top
		///               and bottom colors using the default corner radius.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="frame">Position and size of the button.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, Rectangle frame, string title = "")
			: this(canvas, frame, FN2DCanvas.DefaultCornerRadius, FN2DCanvas.DefaultTopColor, FN2DCanvas.DefaultBottomColor, title)
		{
		}

		/// <summary>
		/// Constructor - Creates a colored button based on the default
		///               top and bottom colors using the default corner radius.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="size">Size of the button.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, Size size, string title = "")
			: this(canvas, new Rectangle(Point.Empty, size), title)
		{
		}

		/// <summary>
		/// Constructor - Creates a colored button based on the default top and
		///               bottom colors using the default corner radius and size.
		/// </summary>
		/// <param name="canvas">Canvas that the button is on.</param>
		/// <param name="title">Title that appears on the button.</param>
		public FN2DButton(FN2DCanvas canvas, string title = "")
			: this(canvas, FN2DCanvas.DefaultButtonSize, title)
		{
		}

		/// <summary>
		/// Frees unmanaged resources and calls Dispose() on the member objects.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				if (normalIcon != null) normalIcon.Dispose();
				if (disabledIcon != null) disabledIcon.Dispose();
				if (selectedIcon != null) selectedIcon.Dispose();
				if (disabledSelectedIcon != null) disabledSelectedIcon.Dispose();
			}

			// clear the object references
			normalIcon = null;
			disabledIcon = null;
			selectedIcon = null;
			disabledSelectedIcon = null;
			iconImage = null;
			titleLabel = null;

			// call the base handler
			base.Dispose(disposing);
		}

		/// <summary>
		/// Sets the button images based on the current state.
		/// </summary>
		public override void Refresh()
		{
			if (refreshEnabled)
			{
				base.Refresh();

				// don't do anyhting until the control has been fully created
				if (iconImage == null || titleLabel == null)
				{
					return;
				}

				// set the background colors
				if (!Enabled)
				{
					background.SetColors(topDisabledColor, bottomDisabledColor);
				}
				else if (touching)
				{
					if (selected)
					{
						background.SetColors(topColor, bottomColor);
					}
					else
					{
						background.SetColors(topPressedColor, bottomPressedColor);
					}
				}
				else if (selected)
				{
					background.SetColors(topPressedColor, bottomPressedColor);
				}
				else
				{
					background.SetColors(topColor, bottomColor);
				}

				// set the icon image
				if (selected && selectedIcon != null)
				{
					iconImage.Image = Enabled ? selectedIcon : (disabledSelectedIcon ?? selectedIcon);
				}
				else
				{
					iconImage.Image = Enabled ? normalIcon : (disabledIcon ?? normalIcon);
				}

				// show/hide the icon and label
				iconImage.Visible = iconImage.Image != null;
				titleLabel.Visible = !string.IsNullOrEmpty(titleLabel.Text);

				// if there's an icon
				if (iconImage.Visible)
				{
					// if there's a title, position the icon relative to it
					if (titleLabel.Visible)
					{
						int width = titleLabel.Width + iconImage.Width + IconTextPadding;
						int height = titleLabel.Height + iconImage.Height + IconTextPadding;
						int x = (Width - width) / 2;
						int y = (Height - height) / 2;
						Point iconLocation = Point.Empty;
						Point titleLocation = Point.Empty;
						switch (iconAlignment)
						{
							case FN2DIconAlignment.Left:
								iconLocation = new Point(x, (Height - iconImage.Height) / 2);
								titleLocation = new Point(x + iconImage.Width + IconTextPadding, (Height - titleLabel.Height) / 2);
								break;
							case FN2DIconAlignment.Right:
								iconLocation = new Point(x + titleLabel.Width + IconTextPadding, (Height - iconImage.Height) / 2);
								titleLocation = new Point(x, (Height - titleLabel.Height) / 2);
								break;
							case FN2DIconAlignment.Above:
								iconLocation = new Point((Width - iconImage.Width) / 2, y);
								titleLocation = new Point((Width - titleLabel.Width) / 2, y + iconImage.Height + IconTextPadding);
								break;
							case FN2DIconAlignment.Below:
								iconLocation = new Point((Width - iconImage.Width) / 2, y + titleLabel.Height + IconTextPadding);
								titleLocation = new Point((Width - titleLabel.Width) / 2, y);
								break;
						}
						iconImage.Location = iconLocation;
						titleLabel.Location = titleLocation;
					}

					// otherwise just center the icon
					else
					{
						iconImage.Location = new Point((Width - iconImage.Width) / 2 + iconOffsets.X, (Height - iconImage.Height) / 2 + iconOffsets.Y);
					}
				}

				// if there's a title, center it
				else if (titleLabel.Visible)
				{
					titleLabel.Location = new Point((Width - titleLabel.Width) / 2 + titleOffsets.X, (Height - titleLabel.Height) / 2 + titleOffsets.Y);
				}
			}
		}

		/// <summary>
		/// Called when a finger goes down on the control.
		/// </summary>
		public override void TouchDown(FN2DTouchEventArgs e)
		{
			Touching = true;
			Refresh();
		}

		/// <summary>
		/// Called when a finger goes up on the control.
		/// </summary>
		public override void TouchUp(FN2DTouchEventArgs e)
		{
			base.TouchUp(e);
			Touching = false;
			Refresh();
		}
		
		/// <summary>
		/// Cancels any touch tracking that is in progress.
		/// </summary>
		public override void TouchCancel(FN2DTouchEventArgs e)
		{
			base.TouchCancel(e);
			Refresh();
		}

		/// <summary>
		/// Gets or sets the background color.
		/// </summary>
		public override Color BackgroundColor
		{
			get { return base.BackgroundColor; }
			set
			{
				TopColor = value;
				BottomColor = value;
			}
		}

		/// <summary>
		/// Gets or sets the top background color.
		/// </summary>
		public override Color TopColor
		{
			get { return topColor; }
			set
			{
				topColor = value;
				if (topColor != Color.Transparent)
				{
					topPressedColor = Lighten(topColor);
					topDisabledColor = Gray(topColor);
				}
				else
				{
					topPressedColor = Color.Transparent;
					topDisabledColor = Color.Transparent;
				}
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the bottom background color.
		/// </summary>
		public override Color BottomColor
		{
			get { return bottomColor; }
			set
			{
				bottomColor = value;
				if (bottomColor != Color.Transparent)
				{
					bottomPressedColor = Lighten(bottomColor);
					bottomDisabledColor = Gray(bottomColor);
				}
				else
				{
					bottomPressedColor = Color.Transparent;
					bottomDisabledColor = Color.Transparent;
				}
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the top color when the button is pressed.
		/// </summary>
		public Color TopPressedColor
		{
			get { return topPressedColor; }
			set
			{
				topPressedColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the bottom color when the button is pressed.
		/// </summary>
		public Color BottomPressedColor
		{
			get { return bottomPressedColor; }
			set
			{
				bottomPressedColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the top color when the button is disabled.
		/// </summary>
		public Color TopDisabledColor
		{
			get { return topDisabledColor; }
			set
			{
				topDisabledColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the bottom color when the button is disabled.
		/// </summary>
		public Color BottomDisabledColor
		{
			get { return bottomDisabledColor; }
			set
			{
				bottomDisabledColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the selected state.
		/// </summary>
		public bool Selected
		{
			get { return selected; }
			set
			{
				selected = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets the title label.
		/// </summary>
		public FN2DLabel Label
		{
			get { return titleLabel; }
		}

		/// <summary>
		/// Sets/gets the title.
		/// </summary>
		public string Title
		{
			get { return (titleLabel != null) ? titleLabel.Text : ""; }
			set
			{
				titleLabel.Text = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the title position offsets.
		/// </summary>
		public Point TitleOffsets
		{
			get { return titleOffsets; }
			set
			{
				titleOffsets = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the icon position offsets.
		/// </summary>
		public Point IconOffsets
		{
			get { return iconOffsets; }
			set
			{
				iconOffsets = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the icon alignment relative to the button text.
		/// </summary>
		public FN2DIconAlignment IconAlignment
		{
			get { return iconAlignment; }
			set
			{
				iconAlignment = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the normal icon image.
		/// </summary>
		public FN2DBitmap Icon
		{
			get { return normalIcon; }
			set
			{
				normalIcon = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the disabled icon image.
		/// </summary>
		public FN2DBitmap DisabledIcon
		{
			get { return disabledIcon; }
			set
			{
				disabledIcon = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the selected icon image.
		/// </summary>
		public FN2DBitmap SelectedIcon
		{
			get { return selectedIcon; }
			set
			{
				selectedIcon = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets/gets the disabled selected icon image.
		/// </summary>
		public FN2DBitmap DisabledSelectedIcon
		{
			get { return disabledSelectedIcon; }
			set
			{
				disabledSelectedIcon = value;
				Refresh();
			}
		}

		/// <summary>
		/// Sets the normal and disabled icons.
		/// </summary>
		/// <param name="icon">Normal icon.</param>
		/// <param name="iconDisabled">Normal disabled icon.</param>
		public void SetIcons(FN2DBitmap icon, FN2DBitmap iconDisabled)
		{
			normalIcon = icon;
			disabledIcon = iconDisabled;
			Refresh();
		}

		/// <summary>
		/// Sets the selected and disabled selected icons.
		/// </summary>
		/// <param name="iconSelected">Selected icon.</param>
		/// <param name="iconSelectedDisabled">Disabled selected icon.</param>
		public void SetSelectedIcons(FN2DBitmap iconSelected, FN2DBitmap iconSelectedDisabled)
		{
			selectedIcon = iconSelected;
			disabledSelectedIcon = iconSelectedDisabled;
			Refresh();
		}

		/// <summary>
		/// Lightens the specified color.
		/// </summary>
		/// <param name="color">Color to be lightened.</param>
		/// <returns>The lightened color.</returns>
		private Color Lighten(Color color)
		{
			float hue = color.GetHue() / 360;
			float saturation = color.GetSaturation();
			float luminosity = color.GetBrightness() * 1.3f;

			double r = 0, g = 0, b = 0;
			if (luminosity != 0)
			{
				if (saturation == 0)
				{
					r = g = b = luminosity;
				}
				else
				{
					double temp2 = (luminosity < 0.5) ? (luminosity * (1.0 + saturation)) : (luminosity + saturation - (luminosity * saturation));
					double temp1 = 2.0 * luminosity - temp2;
					r = GetColorComponent(temp1, temp2, hue + 1.0 / 3.0);
					g = GetColorComponent(temp1, temp2, hue);
					b = GetColorComponent(temp1, temp2, hue - 1.0 / 3.0);
				}
			}

			return Color.FromArgb((byte)(255 * r), (byte)(255 * g), (byte)(255 * b));
		}

		/// <summary>
		/// Gets a color component when lightening a color.
		/// </summary>
		/// <returns>The color component.</returns>
		/// <param name="temp1">Temp1.</param>
		/// <param name="temp2">Temp2.</param>
		/// <param name="temp3">Temp3.</param>
		private static double GetColorComponent(double temp1, double temp2, double temp3)
		{
			if (temp3 < 0.0)
				temp3 += 1.0;
			else if (temp3 > 1.0)
				temp3 -= 1.0;
			if (temp3 < 1.0 / 6.0)
				return temp1 + (temp2 - temp1) * 6.0 * temp3;
			else if (temp3 < 0.5)
				return temp2;
			else if (temp3 < 2.0 / 3.0)
				return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
			else
				return temp1;
		}

		/// <summary>
		/// Gets the gray version of the specified color.
		/// </summary>
		/// <param name="color">Color to be grayed.</param>
		/// <returns>The grayed color.</returns>
		private Color Gray(Color color)
		{
			int rgb = ((int)color.R + color.G + color.B) / 3;
			return Color.FromArgb(color.A, rgb, rgb, rgb);
		}
	}
	
	/// <summary>
	/// OpenGL 2D icon alignment.
	/// </summary>
	public enum FN2DIconAlignment
	{
		Above,
		Below,
		Left,
		Right
	}
}
