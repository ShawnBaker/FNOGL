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
using System.Reflection;
using System.Runtime.InteropServices;
#if FN2D_WIN
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using DrawMode = OpenTK.Graphics.OpenGL.BeginMode;
using FN2DBitmap = System.Drawing.Bitmap;
using TextureParamName = OpenTK.Graphics.OpenGL.TextureParameterName;
#elif FN2D_IOS
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using DrawElementsType = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using FN2DBitmap = MonoTouch.UIKit.UIImage;
using PixelType = OpenTK.Graphics.ES11.All;
using StringName = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using TextureParamName = OpenTK.Graphics.ES11.All;
using TextureTarget = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D image control.
	/// </summary>
	public class FN2DImage : FN2DControl
	{
#if FN2D_WIN
		protected static DrawElementsType UnsignedIntElement = DrawElementsType.UnsignedInt;
		protected static OpenTK.Graphics.OpenGL.PixelFormat BitmapPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;
		protected static PixelInternalFormat TexturePixelFormat = PixelInternalFormat.Rgba;
#elif FN2D_IOS
		protected static All UnsignedIntElement = All.UnsignedIntOes;
		protected static All BitmapPixelFormat = All.Rgba;
		protected static int TexturePixelFormat = (int)All.Rgba;
#endif
		// static variables
		private static bool usingPowerOf2 = false;
		private static bool gotUsingPowerOf2 = false;

		// instance variables
		protected FN2DBitmap image = null;
		protected Size imageSize = Size.Empty;
		protected Size textureSize = Size.Empty;
		protected SizeF drawingSize = SizeF.Empty;
		protected int textureId = 0;
		protected FN2DRectangleInsets insets = FN2DRectangleInsets.Empty;
		protected bool tile = false;
		protected FN2DArrays arrays = new FN2DArrays();
		protected FN2DArrays middleArrays = null;
		protected Color middleColor = Color.Transparent;

		/// <summary>
		/// Constructor - Creates an empty image control on a canvas.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		public FN2DImage(FN2DCanvas canvas)
			: base(canvas)
		{
		}

		/// <summary>
		/// Constructor - Creates an image control on a canvas.
		/// </summary>
		/// <param name="canvas">Canvas that the control is on.</param>
		/// <param name="image">Image to be displayed.</param>
		/// <param name="resizable">True to make the image resizable, false to leave it alone.</param>
		public FN2DImage(FN2DCanvas canvas, FN2DBitmap image, bool resizable = false)
			: this(canvas)
		{
			imageSize = new Size((int)image.Size.Width, (int)image.Size.Height);
			if (resizable)
			{
				insets = new FN2DRectangleInsets(imageSize);
			}
			frame.Size = imageSize;
			Image = image;
		}

		/// <summary>
		/// Frees unmanaged resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				if (image != null) image.Dispose();
				if (arrays != null) arrays.Dispose();
				if (middleArrays != null) middleArrays.Dispose();
			}

			// delete the texture
			if (textureId != 0)
			{
				try { GL.DeleteTextures(1, ref textureId); }
				catch { }
				finally { textureId = 0; }
			}

			// clear the object references
			image = null;
			arrays = null;
			middleArrays = null;

			// call the base handler
			base.Dispose(disposing);
		}

		/// <summary>
		/// Gets the texture identifier.
		/// </summary>
		public int TextureId
		{
			get { return textureId; }
		}

		/// <summary>
		/// Creates a texture with a specific size and option intiialization data.
		/// </summary>
		/// <param name="width">Width of the image to be placed into the texture.</param>
		/// <param name="height">Height of the image to be placed into the texture.</param>
		/// <param name="data">Pointer to the image data or IntPtr.Zero if there is no image data.</param>
		public void CreateTexture(int width, int height, IntPtr data)
		{
			// create the texture
			GL.GenTextures(1, out textureId);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			GL.TexParameter(TextureTarget.Texture2D, TextureParamName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParamName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParamName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParamName.TextureWrapT, (int)All.ClampToEdge);

			// copy the image to the texture, set the sizes
			imageSize = new Size(width, height);
			if (UsingPowerOf2)
			{
				textureSize = new Size(NearestPowerOf2(width), NearestPowerOf2(height));
				drawingSize = new SizeF((float)width / textureSize.Width, (float)height / textureSize.Height);
				GL.TexImage2D(TextureTarget.Texture2D, 0, TexturePixelFormat, textureSize.Width, textureSize.Height, 0,
				              BitmapPixelFormat, PixelType.UnsignedByte, IntPtr.Zero);
				if (data != IntPtr.Zero)
				{
					GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, BitmapPixelFormat,
					                 PixelType.UnsignedByte, data);
				}
			}
			else
			{
				textureSize = new Size(width, height);
				drawingSize = new SizeF(1, 1);
				GL.TexImage2D(TextureTarget.Texture2D, 0, TexturePixelFormat, width, height, 0,
				              BitmapPixelFormat, PixelType.UnsignedByte, data);
			}
			GL.BindTexture(TextureTarget.Texture2D, 0);

			// refresh the arrays
			Refresh();
		}

		/// <summary>
		/// Sets the image data.
		/// </summary>
		public virtual IntPtr ImageData
		{
			set
			{
				if (value != IntPtr.Zero)
				{
					GL.BindTexture(TextureTarget.Texture2D, textureId);
					GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, imageSize.Width, imageSize.Height, BitmapPixelFormat, PixelType.UnsignedByte, value);
					GL.BindTexture(TextureTarget.Texture2D, 0);
				}
			}
		}

		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		public virtual FN2DBitmap Image
		{
			get { return image; }
			set
			{
				// delete the current texture
				GL.Enable(EnableCap.Texture2D);
				if (textureId != 0)
				{
					GL.DeleteTextures(1, ref textureId);
					textureId = 0;
				}

				// set the new image
				image = value;

				// if there's an image then create a new texture for it
				if (image != null)
				{
					IntPtr rgbBuffer = IntPtr.Zero;
#if FN2D_WIN
					imageSize = image.Size;
					BitmapData imageData = null;
#elif FN2D_IOS
					imageSize = image.Size.ToSize();
#endif
					if (!insets.IsFull)
					{
						frame.Size = imageSize;
					}
					try
					{
						// get the image data
#if FN2D_WIN
						imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
						                           System.Drawing.Imaging.ImageLockMode.ReadWrite,
						                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						rgbBuffer = imageData.Scan0;
#elif FN2D_IOS
						int rgbBufferSize = imageSize.Width * imageSize.Height * 4;
						rgbBuffer = Marshal.AllocHGlobal(rgbBufferSize);
						CGBitmapContext bitmapContext = new CGBitmapContext(rgbBuffer, imageSize.Width, imageSize.Height, 8, imageSize.Width * 4,
						                                                    image.CGImage.ColorSpace, CGBitmapFlags.PremultipliedLast);
						bitmapContext.DrawImage(new RectangleF(0, 0, imageSize.Width, imageSize.Height), image.CGImage);
						bitmapContext.Dispose();
#endif
						// get the middle color
						if (insets.IsFull && (imageSize.Width - insets.Width) == 1 && (imageSize.Height - insets.Height) == 1)
						{
							int offset = (insets.Top * imageSize.Width + insets.Left) * 4;
#if FN2D_WIN
							middleColor = Color.FromArgb(Marshal.ReadByte(rgbBuffer, offset + 3), Marshal.ReadByte(rgbBuffer, offset + 2),
							                             Marshal.ReadByte(rgbBuffer, offset + 1), Marshal.ReadByte(rgbBuffer, offset + 0));
#elif FN2D_IOS
							middleColor = Color.FromArgb(Marshal.ReadByte(rgbBuffer, offset + 3), Marshal.ReadByte(rgbBuffer, offset + 0),
							                             Marshal.ReadByte(rgbBuffer, offset + 1), Marshal.ReadByte(rgbBuffer, offset + 2));
#endif
						}

						// create a texture from the image data
						CreateTexture(imageSize.Width, imageSize.Height, rgbBuffer);
					}
					catch
					{
					}
					finally
					{
#if FN2D_WIN
						if (imageData != null)
						{
							image.UnlockBits(imageData);
							imageData = null;
						}
#elif FN2D_IOS
						if (rgbBuffer != IntPtr.Zero)
						{
							Marshal.FreeHGlobal(rgbBuffer);
							rgbBuffer = IntPtr.Zero;
						}
#endif
						GL.Disable(EnableCap.Texture2D);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the insets.
		/// </summary>
		public FN2DRectangleInsets Insets
		{
			get { return insets; }
			set
			{
				if (value != insets)
				{
					insets = value;
					Refresh();
				}
			}
		}

		/// <summary>
		/// Gets or sets the tile flag.
		/// </summary>
		public bool Tile
		{
			get { return tile; }
			set
			{
				if (value != tile)
				{
					tile = value;
					Refresh();
				}
			}
		}

		/// <summary>
		/// Fills the control with the image, posibly clipping the image.
		/// </summary>
		public float Fill()
		{
			if (!insets.IsFull && !tile && imageSize.Width != 0 && imageSize.Height != 0)
			{
				float widthZoom = Width / (float)imageSize.Width;
				float heightZoom = Height / (float)imageSize.Height;
				Zoom = (float)Math.Max(widthZoom, heightZoom);
				Pan = Size.Empty;
			}
			return Zoom;
		}

		/// <summary>
		/// Fits the image into the control, possible leaving blank space on two sides.
		/// </summary>
		public float Fit()
		{
			if (!insets.IsFull && !tile && imageSize.Width != 0 && imageSize.Height != 0)
			{
				float widthZoom = Width / (float)imageSize.Width;
				float heightZoom = Height / (float)imageSize.Height;
				Zoom = (float)Math.Min(widthZoom, heightZoom);
				Pan = Size.Empty;
			}
			return Zoom;
		}

		/// <summary>
		/// Gets the maximum pan.
		/// </summary>
		public override Size MaxPan
		{
			get { return new Size((int)Math.Max(Math.Round(-(Width / Zoom - imageSize.Width) / 2), 0), (int)Math.Max(Math.Round(-(Height / Zoom - imageSize.Height) / 2), 0)); }
		}

		/// <summary>
		/// Refreshes the arrays.
		/// </summary>
		public override void Refresh()
		{
			base.Refresh();

			// if there's a texture
			if (textureId != 0)
			{
				// build and draw the arrays
				middleArrays = null;
				if (insets.IsFull)
				{
					// get the number of rectangles
					int middleWidth = Width - insets.Left - insets.Right;
					int middleHeight = Height - insets.Top - insets.Bottom;
					int imageMiddleWidth = imageSize.Width - insets.Left - insets.Right;
					int imageMiddleHeight = imageSize.Height - insets.Top - insets.Bottom;
					int numTopBottom = middleWidth / imageMiddleWidth;
					if (numTopBottom % imageMiddleWidth != 0)
						numTopBottom++;
					if (numTopBottom < 0)
						numTopBottom = 0;
					int numLeftRight = middleHeight / imageMiddleHeight;
					if (numLeftRight % imageMiddleHeight != 0)
						numLeftRight++;
					if (numLeftRight < 0)
						numLeftRight = 0;
					int numRectangles = numTopBottom * 2 + numLeftRight * 2 + 4;
					bool fillMiddle = imageMiddleWidth == 1 && imageMiddleHeight == 1;
					//bool fillMiddle = false;
					if (!fillMiddle)
						numRectangles += numTopBottom * numLeftRight;

					// allocate arrays for all the rectangles
					arrays.AllocRects(numRectangles, true, false);
					//Console.WriteLine("Insets: " + insets + "  " + Size + "  " + imageSize + "  " + textureSize + "  " + drawingSize);

					// create array entries for the top and bottom
					int x = insets.Left;
					int y = Height - insets.Bottom;
					PointF cTopLeft1 = new PointF((float)insets.Left / textureSize.Width, 0);
					PointF cBottomRight1 = new PointF((float)(insets.Left + imageMiddleWidth) / textureSize.Width, (float)insets.Top / textureSize.Height);
					PointF cTopLeft2 = new PointF(cTopLeft1.X, (float)(imageSize.Height - insets.Bottom) / textureSize.Height);
					PointF cBottomRight2 = new PointF(cBottomRight1.X, drawingSize.Height);
					for (int i = 0; i < numTopBottom; i++)
					{
						int x2 = x + imageMiddleWidth;
						arrays.AddRect(x, 0, x2, insets.Top, cTopLeft1.X, cTopLeft1.Y, cBottomRight1.X, cBottomRight1.Y);
						arrays.AddRect(x, y, x2, Height, cTopLeft2.X, cTopLeft2.Y, cBottomRight2.X, cBottomRight2.Y);
						x = x2;
					}

					// create array entries for the sides
					x = Width - insets.Right;
					y = insets.Top;
					cTopLeft1 = new PointF(0, (float)insets.Top / textureSize.Height);
					cBottomRight1 = new PointF((float)insets.Left / textureSize.Width, (float)(insets.Top + imageMiddleHeight) / textureSize.Height);
					cTopLeft2 = new PointF((float)(imageSize.Width - insets.Right) / textureSize.Width, cTopLeft1.Y);
					cBottomRight2 = new PointF(drawingSize.Width, cBottomRight1.Y);
					for (int i = 0; i < numLeftRight; i++)
					{
						int y2 = y + imageMiddleHeight;
						arrays.AddRect(0, y, insets.Left, y2, cTopLeft1.X, cTopLeft1.Y, cBottomRight1.X, cBottomRight1.Y);
						arrays.AddRect(x, y, Width, y2, cTopLeft2.X, cTopLeft2.Y, cBottomRight2.X, cBottomRight2.Y);
						y = y2;
					}

					// create array entries for the middle
					cTopLeft1 = new PointF((float)insets.Left / textureSize.Width, (float)insets.Top / textureSize.Height);
					cBottomRight1 = new PointF((float)(imageSize.Width - insets.Right) / textureSize.Width, (float)(imageSize.Height - insets.Bottom) / textureSize.Height);
					Point vBottomRight = new Point(Width - insets.Right, Height - insets.Bottom);
					if (fillMiddle)
					{
						middleArrays = new FN2DArrays();
						middleArrays.AllocRects(1, false, true);
						middleArrays.AddRect(insets.Left, insets.Top, vBottomRight.X, vBottomRight.Y, middleColor);
					}
					else
					{
						y = insets.Top;
						for (int i = 0; i < numLeftRight; i++)
						{
							x = insets.Left;
							int y2 = y + imageMiddleHeight;
							for (int j = 0; j < numTopBottom; j++)
							{
								int x2 = x + imageMiddleWidth;
								arrays.AddRect(x, y, x2, y2, cTopLeft1.X, cTopLeft1.Y, cBottomRight1.X, cBottomRight1.Y);
								x = x2;
							}
							y = y2;
						}
					}

					// create array entries for the corners
					arrays.AddRect(0, 0, insets.Left, insets.Top, 0, 0, cTopLeft1.X, cTopLeft1.Y);
					arrays.AddRect(vBottomRight.X, 0, Width, insets.Top, cBottomRight1.X, 0, drawingSize.Width, cTopLeft1.Y);
					arrays.AddRect(vBottomRight.X, vBottomRight.Y, Width, Height, cBottomRight1.X, cBottomRight1.Y, drawingSize.Width, drawingSize.Height);
					arrays.AddRect(0, vBottomRight.Y, insets.Left, Height, 0, cBottomRight1.Y, cTopLeft1.X, drawingSize.Height);
				}
				else if (tile)
				{
					// get the number of rows and columns
					int numRows = Height / imageSize.Height;
					if (Height % imageSize.Height != 0) numRows++;
					int numCols = Width / imageSize.Width;
					if (Width % imageSize.Width != 0) numCols++;

					// allocate array rectangles for each row and column
					arrays.AllocRects(numRows * numCols, true, false);

					// create array entries for each tile
					int y = 0;
					for (int row = 0; row < numRows; row++)
					{
						int y2 = y + imageSize.Height;
						int x = 0;
						for (int col = 0; col < numCols; col++)
						{
							int x2 = x + imageSize.Width;
							arrays.AddRect(x, y, x2, y2, 0, 0, drawingSize.Width, drawingSize.Height);
							x = x2;
						}
						y = y2;
					}
				}
				else
				{
					// allocate one array rectangle
					arrays.AllocRects(1, true, false);

					// get the vertices
					Point topLeft = new Point((int)(Width - imageSize.Width) / 2, (int)(Height - imageSize.Height) / 2);
					Point bottomRight = new Point(topLeft.X + imageSize.Width, topLeft.Y + imageSize.Height);
					//Console.WriteLine("Image: " + topLeft + "   " + bottomRight);

					// add the rectangle
					arrays.AddRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y, 0, 0, drawingSize.Width, drawingSize.Height);
				}

				// complete the arrays
				arrays.Complete();
				if (middleArrays != null)
				{
					middleArrays.Complete();
				}
			}
			else
			{
				arrays.Clear();
				middleArrays = null;
			}
		}

		/// <summary>
		/// Draws the image.
		/// </summary>
		public override void Draw()
		{
			base.Draw();

			// if there's a texture
			if (textureId != 0)
			{
				// draw the middle color arrays
				if (middleArrays != null)
				{
					middleArrays.Draw();
				}

				// draw the texture arrays
				GL.BindTexture(TextureTarget.Texture2D, textureId);
				arrays.Draw();
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}
		}

		/// <summary>
		/// Gets the nearest power of 2 that is greater than a specific number.
		/// </summary>
		/// <param name="num">Number to get the greater nearest power of 2 for.</param>
		/// <returns>Greater nearest power of 2.</returns>
		public int NearestPowerOf2(int num)
		{
			int n = num > 0 ? num - 1 : 0;

			n |= n >> 1;
			n |= n >> 2;
			n |= n >> 4;
			n |= n >> 8;
			n |= n >> 16;
			n++;

			return n;
		}

		/// <summary>
		/// determines if texture sizes must be a power of 2
		/// </summary>
		private static bool UsingPowerOf2
		{
			get
			{
				if (!gotUsingPowerOf2)
				{
#if FN2D_IOS
					usingPowerOf2 = false;
#else
					usingPowerOf2 = !GL.GetString(StringName.Extensions).Contains("GL_ARB_texture_non_power_of_two");
#endif
					Console.WriteLine("OpenGL: usingPowerOf2 = " + usingPowerOf2);
					gotUsingPowerOf2 = true;
				}
				return usingPowerOf2;
			}
		}
	}
}
