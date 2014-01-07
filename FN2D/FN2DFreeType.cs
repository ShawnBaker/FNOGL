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
using System.Text;
using System.Runtime.InteropServices;

namespace FrozenNorth.OpenGL.FN2D
{
	public static class FreeType
	{
		private static IntPtr library = IntPtr.Zero;
		private static int error = 0;

		static FreeType()
		{
			error = FT_Init_FreeType(out library);
		}

		public static void Done()
		{
			if (library != IntPtr.Zero)
			{
				FT_Done_FreeType(library);
			}
		}
		
		public static int SetLcdFilter(FTLcdFilter filter)
		{
			return error = FT_Library_SetLcdFilter(library, filter);
		}

		public static int SetLcdFilterWeights(IntPtr library, byte[] weights)
		{
			return error = FT_Library_SetLcdFilterWeights(library, weights);
		}

		public static IntPtr Library
		{
			get { return library; }
		}

		public static int Error
		{
			get { return error; }
		}

		[Flags]
		public enum FTLoad
		{
			DEFAULT = 0x0000,
			NO_SCALE = 0x0001,
			NO_HINTING = 0x0002,
			RENDER = 0x0004,
			NO_BITMAP = 0x0008,
			VERTICAL_LAYOUT = 0x0010,
			FORCE_AUTOHINT = 0x0020,
			CROP_BITMAP = 0x0040,
			PEDANTIC = 0x0080,
			IGNORE_GLOBAL_ADVANCE_WIDTH = 0x0200,
			NO_RECURSE = 0x0400,
			IGNORE_TRANSFORM = 0x0800,
			ONOCHROME = 0x1000,
			LINEAR_DESIGN = 0x2000,
			NO_AUTOHINT = 0x8000,
			TARGET_NORMAL = 0x0000,
			TARGET_LIGHT = 0x10000,
			TARGET_MONO = 0x20000,
			TARGET_LCD = 0x30000,
			TARGET_LCD_V = 0x40000,
			COLOR = 0x00100000
		}

		public enum FTEncoding
		{
			NONE = 0,
			MS_SYMBOL = 0x73796D62,			// 'symb'
			UNICODE = 0x756E6963,			// 'unic'
			SJIS = 0x736A6973,				// 'sjis'
			GB2312 = 0x67622020,			// 'gb  '
			BIG5 = 0x62696735,				// 'big5'
			WANSUNG = 0x77616E73,			// 'wans'
			JOHAB = 0x6A6F6861,				// 'joha'
			ADOBE_STANDARD = 0x41444F42,	// 'ADOB'
			ADOBE_EXPERT = 0x41444245,		// 'ADBE'
			ADOBE_CUSTOM = 0x41444243,		// 'ADBC'
			ADOBE_LATIN_1 = 0x6C617430,		// 'lat1'
			OLD_LATIN_2 = 0x6C617431,		// 'lat2'
			APPLE_ROMAN = 0x61726D6E 		// 'armn'
		}

		public enum FTKerningMode
		{
			DEFAULT = 0,
			UNFITTED,
			UNSCALED
		}

		public enum FTLcdFilter
		{
			NONE	= 0,
			DEFAULT	= 1,
			LIGHT	= 2,
			LEGACY	= 16,
			MAX
		}

		public enum FTGlyphFormat
		{
			NONE = 0x00000000,		// 0
			COMPOSITE = 0x636F6D70,	// 'comp'
			BITMAP = 0x62697473,	// 'bits'
			OUTLINE = 0x6F75746C,	// 'outl'
			PLOTTER = 0x706C6F74	// 'plot'

		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTVector
		{
			public int x;
			public int y;

			public FTVector(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

			public static FTVector Empty
			{
				get { return new FTVector(0, 0); }
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTMatrix
		{
			public int xx, xy;
			public int yx, yy;

			public FTMatrix(int xx, int xy, int yx, int yy)
			{
				this.xx = xx;
				this.xy = xy;
				this.yx = yx;
				this.yy = yy;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTGeneric
		{
			public IntPtr	data;
			public IntPtr	finalizer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTBBox
		{
			public int	xMin, yMin;
			public int	xMax, yMax;	
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTSizeMetrics
		{
			public ushort	x_ppem;
			public ushort	y_ppem;
			public int		x_scale;
			public int		y_scale;
			public int		ascender;
			public int		descender;
			public int		height;
			public int		max_advance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTSize
		{
			public IntPtr			face;
			public FTGeneric		generic;
			public FTSizeMetrics	metrics;
			public IntPtr			internal_size;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTBitmap
		{
			public int		rows;
			public int		width;
			public int		pitch;
			public IntPtr	buffer;
			public short	num_grays;
			public char		pixel_mode;
			public char		palette_mode;
			public IntPtr	palette;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTOutline
		{
			public short	n_contours;
			public short	n_points;
			public IntPtr	points;
			public IntPtr	tags;
			public IntPtr	contours;
			public int		flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTGlyphMetrics
		{
			public int	width;
			public int	height;
			public int	horiBearingX;
			public int	horiBearingY;
			public int	horiAdvance;
			public int	vertBearingX;
			public int	vertBearingY;
			public int	vertAdvance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTGlyphSlot
		{
			public IntPtr			library;
			public IntPtr			face;
			public IntPtr			next;
			public uint				reserved;
			public FTGeneric		generic;
			public FTGlyphMetrics	metrics;
			public int				linearHoriAdvance;
			public int				linearVertAdvance;
			public FTVector			advance;
			public FTGlyphFormat	format;
			public FTBitmap			bitmap;
			public int				bitmap_left;
			public int				bitmap_top;
			public FTOutline		outline;
			public uint				num_subglyphs;
			public IntPtr			subglyphs;
			public IntPtr			control_data;
			public int				control_len;
			public int				lsb_delta;
			public int				rsb_delta;
			public IntPtr			other;
			public IntPtr			internal_glyphslot;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FTFace
		{
			public int				num_faces;
			public int				face_index;
			public int				face_flags;
			public int				style_flags;
			public int				num_glyphs;
			public IntPtr			family_name;
			public IntPtr			style_name;
			public int				num_fixed_sizes;
			//[MarshalAs(UnmanagedType.LPStruct)]
			//public FT_Bitmap_Size	available_sizes;
			public IntPtr	available_sizes;
			public int				num_charmaps;
			//[MarshalAs(UnmanagedType.LPStruct)]
			//public FT_CharMap		charmaps;
			public IntPtr		charmaps;
			public FTGeneric		generic;
			public FTBBox			box;
			public ushort			units_per_EM;
			public short			ascender;
			public short			descender;
			public short			height;
			public short			max_advance_width;
			public short			max_advance_height;
			public short			underline_position;
			public short			underline_thickness;
			//[MarshalAs(UnmanagedType.LPStr)]
			//public FT_GlyphSlot		glyph;
			public IntPtr			glyph;
			//[MarshalAs(UnmanagedType.LPStruct)]
			//public FTSize			size;
			public IntPtr			size;
			//[MarshalAs(UnmanagedType.LPStruct)]
			//public FT_CharMap		charmap;
			public IntPtr		charmap;
			public IntPtr			driver;
			public IntPtr			memory;
			public IntPtr			stream;
			//public FT_List			sizes_list;
			public IntPtr			sizes_list;
			public FTGeneric		autohint;
			public IntPtr			extensions;
			public IntPtr			internal_face;
		}

#if FN2D_WIN
		private const string FreeTypeDll = "freetype.dll";
#elif FN2D_IOS
		private const string FreeTypeDll = "__Internal";
#elif FN2D_AND
		private const string FreeTypeDll = "freetype";
#endif
		private const CallingConvention FreeTypeCallingConvention = CallingConvention.Cdecl;

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Init_FreeType(out IntPtr library);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern void FT_Done_FreeType(IntPtr library);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_New_Face(IntPtr library, IntPtr fileName, int index, out IntPtr face);

		//[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		//public static extern int FT_New_Face(IntPtr library, IntPtr fileName, int index, out FTFace face);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_New_Memory_Face(IntPtr library, byte[] file_base, int size, int index, out IntPtr face);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern void FT_Done_Face(IntPtr face);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Select_Charmap(IntPtr face, FTEncoding encoding);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Set_Char_Size(IntPtr face, int width, int height, uint horizontalResolution, uint verticalResolution);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern void FT_Set_Transform(IntPtr face, FTMatrix matrix, FTVector delta);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern void FT_Set_Transform(IntPtr face, ref FTMatrix matrix, int delta);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern uint FT_Get_Char_Index(IntPtr face, char c);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Get_Kerning(IntPtr face, uint leftGlyph, uint rightGlyph, FTKerningMode kerningMode, out FTVector kerning);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Library_SetLcdFilter(IntPtr library, FTLcdFilter filter);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Library_SetLcdFilterWeights(IntPtr library, byte[] weights);

		[DllImport(FreeTypeDll, CallingConvention = FreeTypeCallingConvention)]
		public static extern int FT_Load_Glyph(IntPtr face, uint index, FTLoad flags);

		private static IntPtr StringToIntPtr(string s)
		{
			if (s == null)
			{
				return IntPtr.Zero;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			IntPtr p = Marshal.AllocHGlobal(bytes.Length + 1);
			Marshal.Copy(bytes, 0, p, bytes.Length);
			Marshal.WriteByte(p, bytes.Length, 0);

			return p;
		}

		public class Face : IDisposable
		{
			private IntPtr facePtr = IntPtr.Zero;
			private byte[] memory = null;
			private FTFace face;
			private FTSize size;
			private int index = 0;

			public Face(string fileName)
			{
				IntPtr fileNamePtr = StringToIntPtr(fileName);
				error = FT_New_Face(library, fileNamePtr, index, out facePtr);
				if (error == 0 && facePtr != IntPtr.Zero)
				{
					face = (FTFace)Marshal.PtrToStructure(facePtr, face.GetType());
					size = (FTSize)Marshal.PtrToStructure(face.size, size.GetType());
				}
				if (fileNamePtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(fileNamePtr);
				}
			}
			
			public Face(byte[] memory)
			{
				error = FT_New_Memory_Face(library, memory, memory.Length, index, out facePtr);
				if (error == 0 && facePtr != IntPtr.Zero)
				{
					this.memory = memory;
					face = (FTFace)Marshal.PtrToStructure(facePtr, face.GetType());
					size = (FTSize)Marshal.PtrToStructure(face.size, size.GetType());
				}
			}

			/// <summary>
			/// Destructor - Calls Dispose().
			/// </summary>
			~Face()
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
				}

				if (facePtr != IntPtr.Zero)
				{
					FT_Done_Face(facePtr);
					facePtr = IntPtr.Zero;
				}
				memory = null;
			}

			public int SelectCharmap(FTEncoding encoding)
			{
				return error = FT_Select_Charmap(facePtr, encoding);
			}

			public int SetCharSize(int width, int height, uint horizontalResolution, uint verticalResolution)
			{
				return error = FT_Set_Char_Size(facePtr, width, height, horizontalResolution, verticalResolution);
			}

			public void SetTransform(FTMatrix matrix, FTVector delta)
			{
				FT_Set_Transform(facePtr, matrix, delta);
			}

			public void SetTransform(FTMatrix matrix)
			{
				FT_Set_Transform(facePtr, ref matrix, 0);
			}

			public uint GetCharIndex(char c)
			{
				return FT_Get_Char_Index(facePtr, c);
			}

			public int GetKerning(uint leftGlyph, uint rightGlyph, FTKerningMode kerningMode, out FTVector kerning)
			{
				return FT_Get_Kerning(facePtr, leftGlyph, rightGlyph, kerningMode, out kerning);
			}

			public int LoadGlyph(uint index, FTLoad flags)
			{
				return error = FT_Load_Glyph(facePtr, index, flags);
			}

			public short UnderlinePosition
			{
				get { return face.underline_position; }
			}

			public short UnderlineThickness
			{
				get { return face.underline_thickness; }
			}
			
			public int Ascender
			{
				get { return face.ascender; }
			}
			
			public int Descender
			{
				get { return face.descender; }
			}
			
			public int Height
			{
				get { return face.height; }
			}

			public FTGlyphSlot Glyph
			{
				get
				{
					FTGlyphSlot slot = new FTGlyphSlot();
					return (FTGlyphSlot)Marshal.PtrToStructure(face.glyph, slot.GetType());
				}
			}
		}
	}
}
