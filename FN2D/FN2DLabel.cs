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
using System.Runtime.InteropServices;
#if FN2D_WIN
using OpenTK.Graphics.OpenGL;
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using TextureTarget = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D label control.
	/// </summary>
	public class FN2DLabel : FN2DControl
	{
		// instance variables
		protected string text = "";
		protected string[] lines;
		protected FN2DFont font = null;
		protected Color textColor = Color.White;
		protected Color disabledTextColor = Color.Gray;
		protected FN2DArrays[] arrays;
		protected SizeF textSize;
		protected bool autoSize = false;
		protected FN2DTextAlignment alignment = FN2DTextAlignment.Center | FN2DTextAlignment.Middle;

		/// <summary>
		/// Constructor - Creates a label on a canvas.
		/// </summary>
		/// <param name="canvas">Canvas to create the label on.</param>
		/// <param name="text">Label text.</param>
		public FN2DLabel(FN2DCanvas canvas, string text = "")
			: base(canvas)
		{
			font = canvas.GetFont(FN2DCanvas.DefaultFontSize, false);
			if (font != null)
			{
				font.Atlas.AtlasResized += HandleFontAtlasResized;
			}
			Text = text;
		}

		/// <summary>
		/// Frees unmanaged resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				if (font != null) font.Dispose();
				if (arrays != null)
				{
					foreach (FN2DArrays a in arrays)
					{
						a.Dispose();
					}
				}
			}

			// clear the object references
			text = null;
			lines = null;
			font = null;
			arrays = null;

			// call the base handler
			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		public string Text
		{
			get { return text; }
			set
			{
				text = (value != null) ? value : "";
				lines = text.Split(new char[] { '\n' });
				arrays = new FN2DArrays[lines.Length];
				for (int i = 0; i < lines.Length; i++)
				{
					arrays[i] = new FN2DArrays();
				}
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the font.
		/// </summary>
		public FN2DFont Font
		{
			get { return font; }
			set
			{
				if (value != null)
				{
					font.Atlas.AtlasResized -= HandleFontAtlasResized;
					font = value;
					font.Atlas.AtlasResized += HandleFontAtlasResized;
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the text when the label is enabled.
		/// </summary>
		public Color TextColor
		{
			get { return textColor; }
			set
			{
				textColor = value;
				if (enabled)
				{
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the text when the label is disabled.
		/// </summary>
		public Color DisabledTextColor
		{
			get { return disabledTextColor; }
			set
			{
				disabledTextColor = value;
				if (!enabled)
				{
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets whether or not the control is automatically sized to fit the text.
		/// </summary>
		public bool AutoSize
		{
			get { return autoSize; }
			set
			{
				autoSize = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the horizontal alignment.
		/// </summary>
		public FN2DTextAlignment Alignment
		{
			get { return alignment; }
			set
			{
				alignment = value;
				Refresh();
			}
		}

		/// <summary>
		/// Refreshes whenever the font atlas is resized.
		/// </summary>
		private void HandleFontAtlasResized(object sender, EventArgs e)
		{
			Refresh();
		}

		/// <summary>
		/// Refreshes the arrays.
		/// </summary>
		public override void Refresh()
		{
			base.Refresh();

			if (font != null && text != null && text.Length != 0)
			{
				// get the arrays
				textSize = Size.Empty;
				SizeF[] sizes = new SizeF[lines.Length];
				float lineHeight = 0;
				for (int i = 0; i < lines.Length; i++)
				{
					if (lines[i].Length != 0 && lines[i][lines[i].Length - 1] == '\r')
					{
						lines[i] = lines[i].Substring(0, lines[i].Length - 1);
					}
					sizes[i] = font.GetArrays(arrays[i], lines[i], Enabled ? textColor : disabledTextColor);
					if (sizes[i].Width > textSize.Width)
					{
						textSize.Width = sizes[i].Width;
					}
					if (sizes[i].Height > lineHeight)
					{
						lineHeight = sizes[i].Height;
					}
				}
				lineHeight++;
				textSize.Height = lines.Length * lineHeight - 1;

				// for auto-size, fit the control to the text
				if (autoSize)
				{
					frame.Size = textSize.ToSize();
				}

				// get the vertical offset
				PointF offset = PointF.Empty;
				switch (alignment & FN2DTextAlignment.Vertical)
				{
					case FN2DTextAlignment.Top:
						break;
					case FN2DTextAlignment.Middle:
						offset.Y = (int)Math.Round((Height - textSize.Height) / 2);
						break;
					case FN2DTextAlignment.Bottom:
						offset.Y = Height - textSize.Height;
						break;
				}

				// get the horizontal offset
				for (int i = 0; i < lines.Length; i++)
				{
					switch (alignment & FN2DTextAlignment.Horizontal)
					{
						case FN2DTextAlignment.Left:
							break;
						case FN2DTextAlignment.Center:
							offset.X = (int)Math.Round((Width - sizes[i].Width) / 2);
							break;
						case FN2DTextAlignment.Right:
							offset.X = Width - sizes[i].Width;
							break;
					}

					// adjust the vertices
					arrays[i].OffsetVertices(offset);
					offset.Y += lineHeight;
				}
			}
		}

		/// <summary>
		/// Draws the label.
		/// </summary>
		public override void Draw()
		{
			base.Draw();
			if (font != null && font.Atlas != null && font.Atlas.TextureId != 0)
			{
				GL.BindTexture(TextureTarget.Texture2D, font.Atlas.TextureId);
				foreach (FN2DArrays array in arrays)
				{
					array.Draw();
				}
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
		}
	}

	/// <summary>
	/// OpenGL 2D text alignment.
	/// </summary>
	[Flags]
	public enum FN2DTextAlignment
	{
		Left = 0x0001,
		Center = 0x0002,
		Right = 0x0003,
		Top = 0x0010,
		Middle = 0x0020,
		Bottom = 0x0030,
		Horizontal = 0x000F,
		Vertical = 0x00F0,
		TopLeft = Top + Left,
		MiddleLeft = Middle + Left,
		BottomLeft = Bottom + Left,
		TopCenter = Top + Center,
		MiddleCenter = Middle + Center,
		BottomCenter = Bottom + Center,
		TopRight = Top + Right,
		MiddleRight = Middle + Right,
		BottomRight = Bottom + Right,
	}
}
