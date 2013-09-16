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
using System.IO;
using System.Runtime.InteropServices;
#if FN2D_IOS
using MonoTouch.Foundation;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D font.
	/// </summary>
	public class FN2DFont : IDisposable
	{
		// public constants
		public static char SpecialChar = char.MaxValue;

		// font system variables
		private static string fontPath = null;
		private static FN2DFontList fonts = null;

		// instance variables
		private FreeType.Face face;
		private FN2DFontGlyphList glyphs;
		private FN2DImageAtlas atlas;
		private string faceName;
		private float size;
		private bool kerning = true;
		private bool hinting = true;
		private bool filtering = true;
		//private byte[] lcdWeights;
		private float height;
		private float lineGap;
		private float ascender;
		private float descender;
		private float underlinePosition;
		private float underlineThickness;

		/// <summary>
		/// Initializes the font system.
		/// </summary>
		public static bool InitFontManager(string path = null)
		{
			if (fontPath == null)
			{
				if (string.IsNullOrEmpty(path))
				{
#if FN2D_WIN
					path = Path.Combine(Directory.GetCurrentDirectory(), "Fonts");
#elif FN2D_IOS
					path = Path.Combine(NSBundle.MainBundle.ResourcePath, "Fonts");
#endif
				}
				fontPath = path;
				fonts = new FN2DFontList();
			}
			return true;
		}

		/// <summary>
		/// Creates a new face at a specific size.
		/// </summary>
		/// <param name="canvas">Canvas that the font is used within.</param>
		/// <param name="faceName">Name of the font face.</param>
		/// <param name="size">Size of the font.</param>
		public static FN2DFont Load(FN2DCanvas canvas, string faceName, float size)
		{
			InitFontManager();

			FN2DFont font = fonts.Find(delegate (FN2DFont f) { return f.faceName == faceName && f.size == size; });
			if (font == null)
			{
				font = new FN2DFont(canvas, faceName, size);
				if (font != null)
				{
					fonts.Add(font);
				}
			}
			return font;
		}

		/// <summary>
		/// Constructor - Creates a new face at a specific size.
		/// </summary>
		/// <param name="canvas">Canvas that the font is used within.</param>
		/// <param name="faceName">Name of the font face.</param>
		/// <param name="size">Size of the font.</param>
		private FN2DFont(FN2DCanvas canvas, string faceName, float size)
			: base()
		{
			// save the parameters
			this.faceName = faceName;
			this.size = size;

			// get the font face
			face = new FreeType.Face(Path.ChangeExtension(Path.Combine(fontPath, faceName), "ttf"));
			if (face == null)
			{
				return;
			}

			// configure the face
			uint hRes = 64;
			face.SelectCharmap(FreeType.FTEncoding.UNICODE);
			face.SetCharSize((int)(size * 64), 0, 72 * hRes, 72);
			FreeType.FTMatrix matrix = new FreeType.FTMatrix((int)(1.0 / hRes * 0x10000), 0, 0, 0x10000);
			face.SetTransform(matrix);

			// set the LCD weights
			// FT_LCD_FILTER_LIGHT   is (0x00, 0x55, 0x56, 0x55, 0x00)
			// FT_LCD_FILTER_DEFAULT is (0x10, 0x40, 0x70, 0x40, 0x10)
			//lcdWeights = new byte[5] { 0x10, 0x40, 0x70, 0x40, 0x10 };

			// calculate the underline position and thickness
			underlinePosition = (float)Math.Round(face.UnderlinePosition / 4096f * size);
			if (underlinePosition > -2)
			{
				underlinePosition = -2;
			}
			underlineThickness = face.UnderlineThickness / 4096f * size;
			underlineThickness = (float)Math.Round(underlineThickness);
			if (underlineThickness < 1)
			{
				underlineThickness = 1;
			}

			// calculate some other font metrics
			ascender = face.Ascender >> 6;
			descender = face.Descender >> 6;
			height = face.Height >> 6;
			lineGap = height - ascender + descender;

			// create the atlas and glyphs, add the special glyph
			atlas = new FN2DImageAtlas(canvas, (size < 20) ? 128 : 256, 1);
			glyphs = new FN2DFontGlyphList();
			GetGlyph(SpecialChar);
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DFont()
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
				if (face != null) face.Dispose();
				if (atlas != null) atlas.Dispose();
			}

			// clear the object references
			face = null;
			glyphs = null;
			atlas = null;
		}

		/// <summary>
		/// Gets the atlas.
		/// </summary>
		public FN2DImageAtlas Atlas
		{
			get { return atlas; }
		}

		/// <summary>
		/// Gets or sets whether or not kerning is used.
		/// </summary>
		public bool FN2DFontKerning
		{
			get { return kerning; }
			set { kerning = value; }
		}

		/// <summary>
		/// Gets or sets whether or not hinting is used.
		/// </summary>
		public bool Hinting
		{
			get { return hinting; }
			set { hinting = value; }
		}

		/// <summary>
		/// Gets or sets whether or not filtering is used.
		/// </summary>
		public bool Filtering
		{
			get { return filtering; }
			set { filtering = value; }
		}

		/// <summary>
		/// Gets the height.
		/// </summary>
		public float Height
		{
			get { return height; }
		}

		/// <summary>
		/// Gets the line gap.
		/// </summary>
		public float LineGap
		{
			get { return lineGap; }
		}

		/// <summary>
		/// Gets the ascender.
		/// </summary>
		public float Ascender
		{
			get { return ascender; }
		}

		/// <summary>
		/// Gets the descender.
		/// </summary>
		public float Descender
		{
			get { return descender; }
		}

		/// <summary>
		/// Gets the underline position.
		/// </summary>
		public float UnderlinePosition
		{
			get { return underlinePosition; }
		}

		/// <summary>
		/// Gets the underline thickness.
		/// </summary>
		public float UnderlineThickness
		{
			get { return underlineThickness; }
		}

		/// <summary>
		/// Gets the glyph for a char code.
		/// </summary>
		/// <param name="unicode">Unicode character.</param>
		/// <returns>The glyph.</returns>
		public FN2DFontGlyph GetGlyph(char unicode)
		{
			// see if the glyph is already loaded
			foreach (FN2DFontGlyph glyph in glyphs)
			{
				if (glyph.unicode == unicode)
				{
					return glyph;
				}
			}

			// create the special chgaracter used for line and background drawing
			if (unicode == SpecialChar)
			{
				FN2DImageAtlasRegion region = atlas.CreateRegion(4, 4);
				byte[] data = new byte[atlas.Depth * 16];
				for (int i = 0; i < data.Length; i++)
				{
					data[i] = 0xFF;
				}
				atlas.SetRegion(region, data);
				FN2DFontGlyph glyph = new FN2DFontGlyph(SpecialChar, region, Point.Empty);
				glyphs.Add(glyph);
				return glyph;
			}

			// create a new glyph
			if (LoadGlyphs(unicode.ToString()) == 0)
			{
				return glyphs[glyphs.Count - 1];
			}

			// indicate failure
			return null;
		}

		/// <summary>
		/// Loads the glyphs for a string of Unicode characters.
		/// </summary>
		/// <param name="unicode">String of Unicode characters.</param>
		/// <returns>The number of glyphs that couldn't be loaded.</returns>
		public int LoadGlyphs(string unicode)
		{
			// count the number of glyphs that couldn't be loaded
			int missed = 0;

			// load each glyph
			for (int i = 0; i < unicode.Length; i++)
			{
				// get the glyph flags
				FreeType.FTLoad target = FreeType.FTLoad.TARGET_NORMAL;
				FreeType.FTLoad flags = FreeType.FTLoad.RENDER | (hinting ? FreeType.FTLoad.FORCE_AUTOHINT : (FreeType.FTLoad.NO_HINTING | FreeType.FTLoad.NO_AUTOHINT));
				if (atlas.Depth == 3)
				{
					FreeType.SetLcdFilter(FreeType.FTLcdFilter.LIGHT);
					target = FreeType.FTLoad.TARGET_LCD;
					if (filtering)
					{
						//FreeType.SetLcdFilterWeights(lcdWeights);
					}
				}

				// load the glyph
				uint glyphIndex = face.GetCharIndex(unicode[i]);
				face.LoadGlyph(glyphIndex, target | flags);
				//if (face.Glyph == null)
				//{
				//	missed++;
				//	continue;
				//}

				// get the glyph bitmap
				Point offset = Point.Empty;
				FreeType.FTGlyphSlot glyphSlot = face.Glyph;
				offset.Y = glyphSlot.bitmap_top;
				offset.X = glyphSlot.bitmap_left;

				// create and set the atlas region from the bitmap
				FN2DImageAtlasRegion region = null;
				if (glyphSlot.bitmap.buffer != IntPtr.Zero && glyphSlot.bitmap.width != 0 && glyphSlot.bitmap.rows != 0)
				{
					region = atlas.CreateRegion(glyphSlot.bitmap.width / atlas.Depth, glyphSlot.bitmap.rows);
					atlas.SetRegion(region, glyphSlot.bitmap.buffer, glyphSlot.bitmap.pitch);
				}
				else
				{
					region = new FN2DImageAtlasRegion(atlas, Rectangle.Empty);
				}
				//Debug.WriteLine("region = " + region + "  " + bitmap.Rows + "  " + bitmap.Width + "  " + bitmap.Pitch);

				// create the glyph
				FN2DFontGlyph textureGlyph = new FN2DFontGlyph(unicode[i], region, offset);

				// discard hinting to get advance
				face.LoadGlyph(glyphIndex, target | FreeType.FTLoad.RENDER | FreeType.FTLoad.NO_HINTING);
				textureGlyph.advance = new SizeF(glyphSlot.advance.x / 64f, glyphSlot.advance.y / 64f);

				// add the glyph
				glyphs.Add(textureGlyph);
			}

			//texture_atlas_upload(atlas);
			GenerateKerning();

			// return the number of glyphs that couldn't be loaded
			return missed;
		}

		/// <summary>
		/// Generates the kerning pairs.
		/// </summary>
		public void GenerateKerning()
		{
			// For each glyph couple combination, check if kerning is necessary
			// Starts at index 1 since 0 is for the special background glyph
			for (int i = 1; i < glyphs.Count; i++)
			{
				FN2DFontGlyph glyph = glyphs[i];
				uint index = face.GetCharIndex(glyph.unicode);
				glyph.kernings.Clear();

				for (int j = 1; j < glyphs.Count; j++)
				{
					FN2DFontGlyph prevGlyph = glyphs[j];
					uint prevIndex = face.GetCharIndex(glyph.unicode);
					FreeType.FTVector kerning;
					face.GetKerning(prevIndex, index, FreeType.FTKerningMode.UNFITTED, out kerning);
					if (kerning.x != 0)
					{
						glyph.kernings.Add(new FN2DFontKerning(prevGlyph.unicode, kerning.x / 4096f));
					}
				}
			}
		}

		/// <summary>
		/// Gets the size of a string in pixels.
		/// </summary>
		/// <param name="text">String to get the size of.</param>
		/// <returns>The size of the string in pixels.</returns>
		public Size StringSize(string text)
		{
			SizeF size = Size.Empty;
			float underhang = 0;
			FN2DFontGlyph prevGlyph = null;
			for (int i = 0; i < text.Length; i++)
			{
				FN2DFontGlyph glyph = GetGlyph(text[i]);
				if (glyph != null)
				{
					float u = glyph.region.Height - glyph.offset.Y;
					if (u > underhang)
					{
						underhang = u;
					}
					float h = glyph.region.Height - u;
					if (h > size.Height)
					{
						size.Height = h;
					}
					size.Width += glyph.advance.Width + ((prevGlyph != null) ? glyph.GetKerning(prevGlyph.unicode) : 0);
					prevGlyph = glyph;
				}
			}
			size.Width = (float)Math.Ceiling(size.Width);
			size.Height += underhang;
			return size.ToSize();
		}

		/// <summary>
		/// Gets the texture coordinate, vertex, color and index arrays for a string and color.
		/// </summary>
		/// <param name="arrays">Drawing arrays (filled in).</param>
		/// <param name="text">Text to get the arrays for.</param>
		/// <param name="color">Color of the text.</param>
		/// <returns>The size of the string in pixels.</returns>
		public SizeF GetArrays(FN2DArrays arrays, string text, Color color)
		{
			// get the glyphs, calculate the height and underhang
			SizeF size = Size.Empty;
			float underhang = 0;
			FN2DFontGlyphList glyphs = new FN2DFontGlyphList();
			for (int i = 0; i < text.Length; i++)
			{
				FN2DFontGlyph glyph = GetGlyph(text[i]);
				if (glyph != null)
				{
					glyphs.Add(glyph);
					float u = glyph.region.Height - glyph.offset.Y;
					if (u > underhang)
					{
						underhang = u;
					}
					float h = glyph.region.Height - u;
					if (h > size.Height)
					{
						size.Height = h;
					}
				}
			}

			// create the arrays
			arrays.AllocRects(text.Length, true, true);

			// fill the arrays from the glyphs
			float x = 0;
			FN2DFontGlyph prevGlyph = null;
			foreach (FN2DFontGlyph glyph in glyphs)
			{
				// get the vertices and coordinates
				x += (prevGlyph != null) ? glyph.GetKerning(prevGlyph.unicode) : 0;
				float u = glyph.region.Height - glyph.offset.Y;
				float x0 = (int)Math.Round(x + glyph.offset.X);
				float y1 = (int)(size.Height + u);
				arrays.AddRect(x0, (int)(y1 - glyph.region.Height), (int)(x0 + glyph.region.Width), y1, glyph.region.TopLeft.X,
				               glyph.region.TopLeft.Y, glyph.region.BottomRight.X, glyph.region.BottomRight.Y, color);
				x += glyph.advance.Width;
				prevGlyph = glyph;
			}

			// return the size
			size.Width = (float)Math.Ceiling(x);
			size.Height += underhang;
			return size;
		}
	}

	/// <summary>
	/// OpenGL 2D list of fonts.
	/// </summary>
	public class FN2DFontList : List<FN2DFont> {};

	/// <summary>
	/// OpenGL 2D font glyph.
	/// </summary>
	public class FN2DFontGlyph
	{
		public char unicode;
		public FN2DImageAtlasRegion region;
		public Point offset = Point.Empty;
		public SizeF advance = SizeF.Empty;
		public FN2DFontKerningList kernings = new FN2DFontKerningList();

		public FN2DFontGlyph(char unicode, FN2DImageAtlasRegion region, Point offset)
		{
			this.unicode = unicode;
			this.region = region;
			this.offset = offset;
		}

		public float GetKerning(char unicode)
		{
			foreach (FN2DFontKerning kerning in kernings)
			{
				if (kerning.unicode == unicode)
				{
					return kerning.kerning;
				}
			}
			return 0;
		}
	}
	
	/// <summary>
	/// OpenGL 2D list of font glyphs.
	/// </summary>
	public class FN2DFontGlyphList : List<FN2DFontGlyph> {};

	/// <summary>
	/// OpenGL 2D font kerning.
	/// </summary>
	public class FN2DFontKerning
	{
		public char unicode;
		public float kerning;

		public FN2DFontKerning(char unicode, float kerning)
		{
			this.unicode = unicode;
			this.kerning = kerning;
		}
	}
	
	/// <summary>
	/// OpenGL 2D list of font kernings.
	/// </summary>
	public class FN2DFontKerningList : List<FN2DFontKerning> {};
}
