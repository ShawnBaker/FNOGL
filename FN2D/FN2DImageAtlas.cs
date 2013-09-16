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
#elif FN2D_IOS
using OpenTK.Graphics.ES11;
using GetPName = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using TextureTarget = OpenTK.Graphics.ES11.All;
using MonoTouch.CoreVideo;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D image atlas.
	/// </summary>
	public class FN2DImageAtlas : FN2DImage
	{
		// public events
		public EventHandler AtlasResized = null;

		// private constants
		private const int MaxDimension = 1024;

		// instance variables
		private int depth = 0;
		private FN2DImageAtlasRegionList regions = null;
		private FN2DMaxRects packer = null;
		private PixelInternalFormat internalFormat;
		private PixelFormat pixelFormat;
#if FN2D_IOS
		private int frameBuffer = 0;
		//private int renderBuffer = 0;
#endif

		/// <summary>
		/// Constructor - Creates the image atlas.
		/// </summary>
		/// <param name="canvas">Canvas that the font is used within.</param>
		/// <param name="dimension">Starting size of each dimension, must be a power of 2.</param>
		/// <param name="depth">Depth of the atlas.</param>
		public FN2DImageAtlas(FN2DCanvas canvas, int dimension, int depth)
			: base(canvas)
		{
			// save the paramters
			this.canvas = canvas;
			this.depth = depth;

			// initialize the members
			BackgroundColor = Color.Black;
			drawingSize = new SizeF(1, 1);
			regions = new FN2DImageAtlasRegionList();
			packer = new FN2DMaxRects(dimension, dimension, false);
			imageSize = new Size(packer.binWidth, packer.binHeight);
			textureSize = new Size(packer.binWidth, packer.binHeight);
			Size = new Size(packer.binWidth, packer.binHeight);

			// create the texture
			CreateTexture();

			// refresh the arrays
			Refresh();
		}

		/// <summary>
		/// Frees unmanaged resources.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// if we got here via Dispose(), call Dispose() on the member objects
			if (disposing)
			{
				//if (packer != null) packer.Dispose();
				//if (regions != null) regions.Dispose();
			}

			// clear the object references
			packer = null;
			regions = null;

			// call the base handler
			base.Dispose(disposing);
		}

		/// <summary>
		/// Sets the color and draws the atlas.
		/// </summary>
		public override void Draw()
		{
			GL.Color4(255, 255, 255, 255);
			base.Draw();
		}

		/// <summary>
		/// Creates the texture.
		/// </summary>
		private void CreateTexture()
		{
			if (textureId != 0)
			{
				GL.DeleteTextures(1, ref textureId);
				textureId = 0;
			}

			// create the texture
			GL.GenTextures(1, out textureId);
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			internalFormat = (depth == 4) ? PixelInternalFormat.Rgba : ((depth == 3) ? PixelInternalFormat.Rgb : PixelInternalFormat.Alpha);
			pixelFormat = (depth == 4) ? PixelFormat.Rgba : ((depth == 3) ? PixelFormat.Rgb : PixelFormat.Alpha);
#if FN2D_WIN
			GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, Size.Width, Size.Height, 0, pixelFormat, PixelType.UnsignedByte, IntPtr.Zero);
#elif FN2D_IOS
			GL.TexImage2D(All.Texture2D, 0, (int)internalFormat, Size.Width, Size.Height, 0, (All)pixelFormat, All.UnsignedByte, IntPtr.Zero);
			if (frameBuffer != 0)
			{
				GL.Oes.DeleteFramebuffers(1, ref frameBuffer);
			}
			GL.Oes.GenFramebuffers(1, out frameBuffer);
			GL.Oes.BindFramebuffer(All.FramebufferOes, frameBuffer);
/*
			if (renderBuffer != 0)
			{
				GL.Oes.DeleteRenderbuffers(1, ref renderBuffer);
			}
			GL.Oes.GenRenderbuffers(1, out renderBuffer);
			GL.Oes.BindRenderbuffer(All.RenderbufferOes, renderBuffer);
			GL.Oes.RenderbufferStorage(All.RenderbufferOes, internalFormat, Size.Width, Size.Height);
			GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.ColorAttachment0Oes, All.RenderbufferOes, renderBuffer);
*/
			GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes, All.Texture2D, textureId, 0);
			GL.BindTexture(TextureTarget.Texture2D, 0);
#endif
		}

		/// <summary>
		/// Gets the depth.
		/// </summary>
		public int Depth
		{
			get { return depth; }
		}

		/// <summary>
		/// Sets a region of the atlas from an unmanaged pointer.
		/// </summary>
		/// <param name="region">Region to set the data for.</param>
		/// <param name="data">Pointer to the bytes to be copied into the region.</param>
		/// <param name="stride">Stride within the array of bytes.</param>
		public void SetRegion(FN2DImageAtlasRegion region, IntPtr data, int stride = 0)
		{
			GL.BindTexture(TextureTarget.Texture2D, textureId);
			if (stride <= 0)
			{
				stride = region.Width * depth;
			}
			IntPtr rowData = data;
			for (int i = 0; i < region.Height; i++)
			{
#if FN2D_WIN
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, region.X, region.Y + i, region.Width, 1, pixelFormat, PixelType.UnsignedByte, rowData);
#elif FN2D_IOS
				GL.TexSubImage2D(All.Texture2D, 0, region.X, region.Y + i, region.Width, 1, (All)pixelFormat, PixelType.UnsignedByte, rowData);
#endif
				rowData = (IntPtr)(rowData.ToInt32() + stride);
			}
			//GL.TexSubImage2D(TextureTarget.Texture2D, 0, region.X, region.Y, region.Width, region.Height, pixelFormat, PixelType.UnsignedByte, data);
			GL.BindTexture(TextureTarget.Texture2D, 0);
		}

		/// <summary>
		/// Sets a region of the atlas from a byte array.
		/// </summary>
		/// <param name="region">Region to set the data for.</param>
		/// <param name="data">Array of bytes to be copied into the region.</param>
		/// <param name="stride">Stride within the array of bytes.</param>
		public void SetRegion(FN2DImageAtlasRegion region, byte[] data, int stride = 0)
		{
			IntPtr ptr = Marshal.AllocHGlobal(data.Length);
			Marshal.Copy(data, 0, ptr, data.Length);

			SetRegion(region, ptr, stride);

			Marshal.FreeHGlobal(ptr);
		}

		/// <summary>
		/// Creates a new region within the atlas.
		/// </summary>
		/// <param name="width">Width of the new region.</param>
		/// <param name="height">Height of the new region.</param>
		/// <returns>The region.</returns>
		public FN2DImageAtlasRegion CreateRegion(int width, int height)
		{
			// get a free region
			Rectangle rect = packer.Insert(width, height, FN2DMaxRects.FreeRectChoiceHeuristic.RectBottomLeftRule);
			if (rect.Width != 0 && rect.Height != 0)
			{
				FN2DImageAtlasRegion region = new FN2DImageAtlasRegion(this, rect);
				regions.Add(region);
				return region;
			}

			// expand the texture until the region fits
			int newWidth = packer.binWidth;
			int newHeight = packer.binHeight;
			while (newWidth <= MaxDimension && newHeight <= MaxDimension)
			{
				// increment the smaller dimension
				if (newWidth <= newHeight)
					newWidth *= 2;
				else
					newHeight *= 2;

				// create the new rectangle packer and add the current regions
				FN2DMaxRects newPacker = new FN2DMaxRects(newWidth, newHeight, false);
				List<Rectangle> newRects = new List<Rectangle>();
				foreach (FN2DImageAtlasRegion reg in regions)
				{
					Rectangle newRect = newPacker.Insert(reg.Width, reg.Height, FN2DMaxRects.FreeRectChoiceHeuristic.RectBottomLeftRule);
					newRects.Add(newRect);
				}

				// get the new region, if successful copy the region data
				rect = newPacker.Insert(width, height, FN2DMaxRects.FreeRectChoiceHeuristic.RectBottomLeftRule);
				if (rect.Width != 0 && rect.Height != 0)
				{
					// save the current state
					int oldTexture;
					Size oldTextureSize = textureSize;
					GL.GetInteger(GetPName.TextureBinding2D, out oldTexture);

					packer = newPacker;
					frame.Size = new Size(packer.binWidth, packer.binHeight);
					imageSize = new Size(packer.binWidth, packer.binHeight);
					textureSize = new Size(packer.binWidth, packer.binHeight);
					IntPtr dataPtr = Marshal.AllocHGlobal(Size.Width * Size.Height * depth);

					// get the current region data
#if FN2D_IOS
					int oldFrameBuffer;
					//int[] viewport = new int[4];
					GL.GetInteger(All.FramebufferBindingOes, out oldFrameBuffer);
					//GL.GetInteger(PixelInternalFormat.Viewport, viewport);

					GL.Oes.BindFramebuffer(All.FramebufferOes, frameBuffer);
					//GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes, All.Texture2D, textureId, 0);
					//GL.Viewport(0, 0, oldTextureSize.Width, oldTextureSize.Height);
					GL.ReadPixels(0, 0, oldTextureSize.Width, oldTextureSize.Height, All.ImplementationColorReadFormatOes, All.ImplementationColorReadFormatOes, dataPtr);
					All err = GL.GetError();
					Console.Write("ReadPixels: " + err);
					for (int i = 0; i < 8; i++)
						Console.Write(" " + Marshal.ReadByte(dataPtr + i));
					Console.WriteLine();

					if (canvas != null)
					{
						//canvas.Bind();
					}
					GL.Oes.BindFramebuffer(All.FramebufferOes, oldFrameBuffer);
					//GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
#else
					GL.BindTexture(TextureTarget.Texture2D, textureId);
					GL.GetTexImage(TextureTarget.Texture2D, 0, pixelFormat, PixelType.UnsignedByte, dataPtr);
#endif
					CreateTexture();
					GL.BindTexture(TextureTarget.Texture2D, textureId);

					// copy the region data
					for (int i = 0; i < regions.Count; i++)
					{
						FN2DImageAtlasRegion reg = regions[i];
						IntPtr regionPtr = (IntPtr)(dataPtr.ToInt32() + (reg.Y * oldTextureSize.Width + reg.X) * depth);
						reg.Rect = newRects[i];
						SetRegion(reg, regionPtr, oldTextureSize.Width * depth);
					}
					Marshal.FreeHGlobal(dataPtr);

					GL.BindTexture(TextureTarget.Texture2D, 0);

					// fire the AtlasResized event
					if (AtlasResized != null)
					{
						AtlasResized(this, EventArgs.Empty);
					}

					// refresh the arrays
					Refresh();

					// return the new region
					FN2DImageAtlasRegion region = new FN2DImageAtlasRegion(this, rect);
					regions.Add(region);
					return region;
				}
			}

			// indicate failure
			return null;
		}
	}

	/// <summary>
	/// OpenGL 2D image atlas region.
	/// </summary>
	public class FN2DImageAtlasRegion
	{
		private FN2DImageAtlas atlas;
		private Rectangle rect;
		private PointF topLeft;
		private PointF bottomRight;

		public FN2DImageAtlasRegion(FN2DImageAtlas atlas, Rectangle rect)
		{
			this.atlas = atlas;
			this.rect = rect;
			topLeft = new PointF((float)rect.Left / atlas.Width, (float)rect.Top / atlas.Height);
			bottomRight = new PointF((float)rect.Right / atlas.Width, (float)rect.Bottom / atlas.Height);
		}

		public void Refresh()
		{
			topLeft = new PointF((float)rect.Left / atlas.Width, (float)rect.Top / atlas.Height);
			bottomRight = new PointF((float)rect.Right / atlas.Width, (float)rect.Bottom / atlas.Height);
		}

		public int X
		{
			get { return rect.X; }
		}
		
		public int Y
		{
			get { return rect.Y; }
		}
		
		public int Width
		{
			get { return rect.Width; }
		}
		
		public int Height
		{
			get { return rect.Height; }
		}

		public Rectangle Rect
		{
			get { return rect; }
			set
			{
				rect = value;
				Refresh();
			}
		}

		public PointF TopLeft
		{
			get { return topLeft; }
		}
		
		public PointF BottomRight
		{
			get { return bottomRight; }
		}

		public override string ToString()
		{
			return string.Format("{0}  {1}  {2}", rect, topLeft, bottomRight);
		}
	}

	/// <summary>
	/// OpenGL 2D list of image atlas regions.
	/// </summary>
	public class FN2DImageAtlasRegionList : List<FN2DImageAtlasRegion>
	{
		public void Refresh()
		{
			foreach (FN2DImageAtlasRegion region in this)
			{
				region.Refresh();
			}
		}
	}
}
