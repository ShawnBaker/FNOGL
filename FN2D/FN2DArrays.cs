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
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using BufferTarget = OpenTK.Graphics.ES11.All;
using BufferUsageHint = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using StringName = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
#endif

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// OpenGL 2D vertex, texture coordinate, color and index arrays for drawing.
	/// </summary>
	public class FN2DArrays : IDisposable
	{
		// internal types
#if FN2D_WIN
		protected static DrawElementsType UnsignedIntElement = DrawElementsType.UnsignedInt;
#elif FN2D_IOS
		protected static All UnsignedIntElement = All.UnsignedIntOes;
#endif
		// static variables
		private static bool usingVBO = false;
		private static bool usingOESVBO = false;
		private static bool gotUsingVBO = false;

		// instance variables
		private BeginMode drawMode;
		private int allocInc;
		private FN2DVertex[] buffer;
		private float[] vertices;
		private float[] texCoords;
		private byte[] colors;
		private uint[] indices;
		private uint numVertices;
		private uint numTexCoords;
		private uint numColors;
		private uint numIndices;
		private int arrayId;
		private int indexId;
		private bool changed;

		/// <summary>
		/// Constructor - Creates an empty set of arrays.
		/// </summary>
		public FN2DArrays(BeginMode drawMode = BeginMode.Triangles, int allocInc = 64)
		{
			this.drawMode = drawMode;
			this.allocInc = allocInc;
			Clear();
		}

		/// <summary>
		/// Destructor - Calls Dispose().
		/// </summary>
		~FN2DArrays()
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
			// delete the buffers
			if (arrayId != 0)
			{
				GL.DeleteBuffers(1, ref arrayId);
				arrayId = 0;
			}
			if (indexId != 0)
			{
				GL.DeleteBuffers(1, ref indexId);
				indexId = 0;
			}

			// clear the arrays
			Clear();
		}

		/// <summary>
		/// Writes the VBO buffers to the GPU if they've changed.
		/// </summary>
		private void SetVboData()
		{
			if (UsingVBO && changed)
			{
				// create, bind and upload the vertices to a buffer
				if (arrayId == 0)
				{
					GL.GenBuffers(1, out arrayId);
				}
				GL.BindBuffer(BufferTarget.ArrayBuffer, arrayId);
				GL.BufferData<FN2DVertex>(BufferTarget.ArrayBuffer, (IntPtr)(Marshal.SizeOf(buffer[0]) * numVertices), buffer, BufferUsageHint.DynamicDraw);
				GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

				// create, bind and upload the indices to a buffer
				if (indexId == 0)
				{
					GL.GenBuffers(1, out indexId);
				}
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexId);
				GL.BufferData<uint>(BufferTarget.ElementArrayBuffer, (IntPtr)(Marshal.SizeOf(indices[0]) * numIndices), indices, BufferUsageHint.DynamicDraw);
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
			}
			changed = false;
		}

		/// <summary>
		/// Allocates the arrays and resets the array indexes.
		/// </summary>
		/// <param name="numVertices">Number of vertices.</param>
		/// <param name="numTexCoords">Number of texture coordinates.</param>
		/// <param name="numColors">Number of colors.</param>
		/// <param name="numIndices">Number of indices.</param>
		public void Alloc(int numVertices, int numTexCoords, int numColors, int numIndices)
		{
			Clear();
			if (UsingVBO)
			{
				buffer = new FN2DVertex[numVertices];
			}
			else
			{
				vertices = new float[numVertices * 2];
				texCoords = new float[numTexCoords * 2];
				colors = new byte[numColors * 4];
			}
			indices = new uint[numIndices];
		}

		/// <summary>
		/// Allocates the arrays for a number of rectangles and resets the array indexes.
		/// </summary>
		/// <param name="numRects">Number of rectangles.</param>
		/// <param name="withTextures">True to allocate the texture coordinates array, false not to.</param>
		/// <param name="withColors">True to allocate the colors array, false not to.</param>
		public void AllocRects(int numRects, bool withTextures, bool withColors)
		{
			Alloc(numRects * 4, withTextures ? (numRects * 4) : 0, withColors ? (numRects * 4) : 0, numRects * 6);
		}

		/// <summary>
		/// Clears the arrays and resets the array indexes.
		/// </summary>
		public void Clear()
		{
			// clear the object references
			buffer = null;
			vertices = null;
			texCoords = null;
			colors = null;
			indices = null;

			// reset the array counts
			numVertices = 0;
			numTexCoords = 0;
			numColors = 0;
			numIndices = 0;

			// set the changed flag
			changed = true;
		}

		/// <summary>
		/// Adds a vertex.
		/// </summary>
		/// <param name="x">The X value.</param>
		/// <param name="y">The Y value.</param>
		/// <returns>The index at which the vertex was added.</returns>
		public uint AddVertex(float x, float y)
		{
			if (UsingVBO)
			{
				if (buffer == null)
				{
					buffer = new FN2DVertex[allocInc];
				}
				else if (numVertices == buffer.Length)
				{
					Array.Resize(ref buffer, buffer.Length + allocInc);
				}
				buffer[numVertices].x = x;
				buffer[numVertices].y = y;
			}
			else
			{
				if (vertices == null)
				{
					vertices = new float[allocInc * 2];
				}
				else if (numVertices * 2 == vertices.Length)
				{
					Array.Resize(ref vertices, vertices.Length + allocInc * 2);
				}
				vertices[numVertices * 2] = x;
				vertices[numVertices * 2 + 1] = y;
			}
			numVertices++;
			changed = true;
			return numVertices - 1;
		}
		
		/// <summary>
		/// Adds a vertex with a color.
		/// </summary>
		/// <param name="x">The X value.</param>
		/// <param name="y">The Y value.</param>
		/// <param name="r">The color's red value.</param>
		/// <param name="g">The color's green value.</param>
		/// <param name="b">The color's blue value.</param>
		/// <param name="a">The color's alpha value.</param>
		/// <returns>The index at which the vertex was added.</returns>
		public uint AddVertex(float x, float y, byte r, byte g, byte b, byte a)
		{
			AddColor(r, g, b, a);
			return AddVertex(x, y);
		}

		/// <summary>
		/// Adds a vertex with a color.
		/// </summary>
		/// <param name="x">The X value.</param>
		/// <param name="y">The Y value.</param>
		/// <param name="color">The color value.</param>
		/// <returns>The index at which the vertex was added.</returns>
		public uint AddVertex(float x, float y, Color color)
		{
			return AddVertex(x, y, color.R, color.G, color.B, color.A);
		}

		/// <summary>
		/// Adds the vertices for a rectangle.
		/// </summary>
		/// <param name="x1">The top-left X value.</param>
		/// <param name="y1">The top-left Y value.</param>
		/// <param name="x2">The bottom-right X value.</param>
		/// <param name="y2">The bottom-right Y value.</param>
		/// <returns>The index at which the vertices were added.</returns>
		public uint AddRectVertices(float x1, float y1, float x2, float y2)
		{
			uint index = AddVertex(x1, y1);
			AddVertex(x2, y1);
			AddVertex(x2, y2);
			AddVertex(x1, y2);
			return index;
		}

		/// <summary>
		/// Adds a texture coordinate.
		/// </summary>
		/// <param name="x">The X value.</param>
		/// <param name="y">The Y value.</param>
		/// <returns>The index at which the texture coordinate was added.</returns>
		public uint AddTexCoord(float x, float y)
		{
			if (UsingVBO)
			{
				if (buffer == null)
				{
					buffer = new FN2DVertex[allocInc];
				}
				else if (numTexCoords == buffer.Length)
				{
					Array.Resize(ref buffer, buffer.Length + allocInc);
				}
				buffer[numTexCoords].s = x;
				buffer[numTexCoords].t = y;
			}
			else
			{
				if (texCoords == null)
				{
					texCoords = new float[allocInc * 2];
				}
				else if (numTexCoords * 2 == texCoords.Length)
				{
					Array.Resize(ref texCoords, texCoords.Length + allocInc * 2);
				}
				texCoords[numTexCoords * 2] = x;
				texCoords[numTexCoords * 2 + 1] = y;
			}
			numTexCoords++;
			changed = true;
			return numTexCoords - 1;
		}

		/// <summary>
		/// Adds the texture coordinates for a rectangle.
		/// </summary>
		/// <param name="x1">The top-left X value.</param>
		/// <param name="y1">The top-left Y value.</param>
		/// <param name="x2">The bottom-right X value.</param>
		/// <param name="y2">The bottom-right Y value.</param>
		/// <returns>The index at which the texture coordinates were added.</returns>
		public uint AddRectTexCoords(float x1, float y1, float x2, float y2)
		{
			uint index = AddTexCoord(x1, y1);
			AddTexCoord(x2, y1);
			AddTexCoord(x2, y2);
			AddTexCoord(x1, y2);
			return index;
		}

		/// <summary>
		/// Adds a color.
		/// </summary>
		/// <param name="r">The color's red value.</param>
		/// <param name="g">The color's green value.</param>
		/// <param name="b">The color's blue value.</param>
		/// <param name="a">The color's alpha value.</param>
		/// <returns>The index at which the color was added.</returns>
		public uint AddColor(byte r, byte g, byte b, byte a)
		{
			if (UsingVBO)
			{
				if (buffer == null)
				{
					buffer = new FN2DVertex[allocInc];
				}
				else if (numColors == buffer.Length)
				{
					Array.Resize(ref buffer, buffer.Length + allocInc);
				}
				buffer[numColors].r = r;
				buffer[numColors].g = g;
				buffer[numColors].b = b;
				buffer[numColors].a = a;
			}
			else
			{
				uint i = numColors * 4;
				if (colors == null)
				{
					colors = new byte[allocInc * 4];
				}
				else if (i == colors.Length)
				{
					Array.Resize(ref colors, colors.Length + allocInc * 4);
				}
				colors[i++] = r;
				colors[i++] = g;
				colors[i++] = b;
				colors[i++] = a;
			}
			numColors++;
			changed = true;
			return numColors - 1;
		}

		/// <summary>
		/// Adds the colors for a rectangle.
		/// </summary>
		/// <param name="r">The color's red value.</param>
		/// <param name="g">The color's green value.</param>
		/// <param name="b">The color's blue value.</param>
		/// <param name="a">The color's alpha value.</param>
		/// <returns>The index at which the colors were added.</returns>
		public uint AddRectColors(byte r, byte g, byte b, byte a)
		{
			uint index = AddColor(r, g, b, a);
			AddColor(r, g, b, a);
			AddColor(r, g, b, a);
			AddColor(r, g, b, a);
			return index;
		}

		/// <summary>
		/// Adds the colors for a rectangle.
		/// </summary>
		/// <param name="color">The color value.</param>
		/// <returns>The index at which the colors were added.</returns>
		public uint AddRectColors(Color color)
		{
			return AddRectColors(color.R, color.G, color.B, color.A);
		}

		/// <summary>
		/// Adds the colors for a rectangle.
		/// </summary>
		/// <param name="color">The color value.</param>
		/// <returns>The index at which the colors were added.</returns>
		public uint AddRectColors(Color topColor, Color bottomColor)
		{
			uint index = AddColor(topColor.R, topColor.G, topColor.B, topColor.A);
			AddColor(topColor.R, topColor.G, topColor.B, topColor.A);
			AddColor(bottomColor.R, bottomColor.G, bottomColor.B, bottomColor.A);
			AddColor(bottomColor.R, bottomColor.G, bottomColor.B, bottomColor.A);
			return index;
		}

		/// <summary>
		/// Adds an index.
		/// </summary>
		/// <param name="el">The element index to be added.</param>
		/// <returns>The index at which the index was added.</returns>
		public uint AddIndex(uint el)
		{
			if (indices == null)
			{
				indices = new uint[allocInc];
			}
			else if (numIndices == indices.Length)
			{
				Array.Resize(ref indices, indices.Length + allocInc);
			}
			indices[numIndices++] = el;
			changed = true;
			return numIndices - 1;
		}

		/// <summary>
		/// Adds the indices for a rectangle.
		/// </summary>
		/// <param name="el">The element index at which the rectangle begins.</param>
		/// <returns>The index at which the indices were added.</returns>
		public uint AddRectIndices(uint el)
		{
			uint index = AddIndex(el + 2);
			AddIndex(el + 3);
			AddIndex(el + 1);
			AddIndex(el + 3);
			AddIndex(el + 1);
			AddIndex(el);
			return index;
		}

		/// <summary>
		/// Adds a rectangle with texture coordinates to the arrays.
		/// </summary>
		/// <param name="vx1">Vertex x1.</param>
		/// <param name="vy1">Vertex y1.</param>
		/// <param name="vx2">Vertex x2.</param>
		/// <param name="vy2">Vertex y2.</param>
		/// <param name="tx1">Texture coordinate x1.</param>
		/// <param name="ty1">Texture coordinate y1.</param>
		/// <param name="tx2">Texture coordinate x2.</param>
		/// <param name="ty2">Texture coordinate y2.</param>
		public void AddRect(float vx1, float vy1, float vx2, float vy2, float tx1, float ty1, float tx2, float ty2)
		{
			uint i = AddRectVertices(vx1, vy1, vx2, vy2);
			AddRectTexCoords(tx1, ty1, tx2, ty2);
			AddRectIndices(i);
		}

		/// <summary>
		/// Adds a rectangle with texture coordinates and a color to the arrays.
		/// </summary>
		/// <param name="vx1">Vertex x1.</param>
		/// <param name="vy1">Vertex y1.</param>
		/// <param name="vx2">Vertex x2.</param>
		/// <param name="vy2">Vertex y2.</param>
		/// <param name="tx1">Texture coordinate x1.</param>
		/// <param name="ty1">Texture coordinate y1.</param>
		/// <param name="tx2">Texture coordinate x2.</param>
		/// <param name="ty2">Texture coordinate y2.</param>
		/// <param name="color">Color to draw within the rectangle.</param>
		public void AddRect(float vx1, float vy1, float vx2, float vy2, float tx1, float ty1, float tx2, float ty2, Color color)
		{
			AddRect(vx1, vy1, vx2, vy2, tx1, ty1, tx2, ty2);
			AddRectColors(color);
		}

		/// <summary>
		/// Adds a rectangle with a color to the arrays.
		/// </summary>
		/// <param name="vx1">Vertex x1.</param>
		/// <param name="vy1">Vertex y1.</param>
		/// <param name="vx2">Vertex x2.</param>
		/// <param name="vy2">Vertex y2.</param>
		/// <param name="color">Color to draw within the rectangle.</param>
		public void AddRect(float vx1, float vy1, float vx2, float vy2, Color color)
		{
			uint i = AddRectVertices(vx1, vy1, vx2, vy2);
			AddRectColors(color);
			AddRectIndices(i);
		}

		/// <summary>
		/// Adds a rectangle with a horizontal gradient color to the arrays.
		/// </summary>
		/// <param name="vx1">Vertex x1.</param>
		/// <param name="vy1">Vertex y1.</param>
		/// <param name="vx2">Vertex x2.</param>
		/// <param name="vy2">Vertex y2.</param>
		/// <param name="color">Color to draw at the top of the rectangle.</param>
		/// <param name="color">Color to draw at the bottom of the rectangle.</param>
		public void AddRect(float vx1, float vy1, float vx2, float vy2, Color topColor, Color bottomColor)
		{
			uint i = AddRectVertices(vx1, vy1, vx2, vy2);
			AddRectColors(topColor, bottomColor);
			AddRectIndices(i);
		}

		/// <summary>
		/// Offsets the vertices by a specific amount.
		/// </summary>
		/// <param name="offset">Offset to be applied to all vertices.</param>
		public void OffsetVertices(PointF offset)
		{
			if (UsingVBO)
			{
				for (int i = 0; i < numVertices; i++)
				{
					buffer[i].x += offset.X;
					buffer[i].y += offset.Y;
				}
			}
			else
			{
				for (int i = 0; i < numVertices; i++)
				{
					vertices[i * 2] += offset.X;
					vertices[i * 2 + 1] += offset.Y;
				}
			}
			changed = true;
		}

		/// <summary>
		/// Gets the current number of vertices.
		/// </summary>
		public uint NumVertices
		{
			get { return numVertices; }
		}

		/// <summary>
		/// Gets the current number of texture coordinates.
		/// </summary>
		public uint NumTexCoords
		{
			get { return numTexCoords; }
		}

		/// <summary>
		/// Gets the current number of colors.
		/// </summary>
		public uint NumColors
		{
			get { return numColors; }
		}

		/// <summary>
		/// Gets the current number of indices.
		/// </summary>
		public uint NumIndices
		{
			get { return numIndices; }
		}

		/// <summary>
		/// Gets or sets the draw mode.
		/// </summary>
		public BeginMode DrawMode
		{
			get { return drawMode; }
			set { drawMode = value; }
		}

		/// <summary>
		/// Draws the arrays.
		/// </summary>
		public void Draw()
		{
			SetVboData();
			if (numVertices != 0 || numTexCoords != 0 || numColors != 0 || numIndices != 0)
			{
				// enable the appropriate arrays
				if (numVertices != 0)
				{
					GL.EnableClientState(ArrayCap.VertexArray);
				}
				if (numTexCoords != 0)
				{
					GL.EnableClientState(ArrayCap.TextureCoordArray);
				}
				if (numColors != 0)
				{
					GL.EnableClientState(ArrayCap.ColorArray);
				}

				// draw from our buffer if we're using VBO's
				if (UsingVBO)
				{
					// bind the array buffer
					GL.BindBuffer(BufferTarget.ArrayBuffer, arrayId);

					// set the arrays
					int stride = Marshal.SizeOf(buffer[0]);
					if (numVertices != 0)
					{
						GL.VertexPointer(2, VertexPointerType.Float, stride, IntPtr.Zero);
					}
					int offset = Marshal.SizeOf(buffer[0].x) + Marshal.SizeOf(buffer[0].y);
					if (numTexCoords != 0)
					{
						GL.TexCoordPointer(2, TexCoordPointerType.Float, stride, (IntPtr)offset);
					}
					offset += Marshal.SizeOf(buffer[0].s) + Marshal.SizeOf(buffer[0].t);
					if (numColors != 0)
					{
						GL.ColorPointer(4, ColorPointerType.UnsignedByte, stride, (IntPtr)offset);
					}

					// draw the arrays
					if (numIndices != 0)
					{
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexId);
						GL.DrawElements(drawMode, (int)numIndices, UnsignedIntElement, IntPtr.Zero);
						GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
					}
					else
					{
						GL.DrawArrays(drawMode, 0, (int)numVertices);
					}

					// clear the array buffer binding
					GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
				}

				// draw from our arrays if we're not using VBO's
				else
				{
					// set the arrays, initialize the state
					if (numVertices != 0)
					{
						GL.VertexPointer<float>(2, VertexPointerType.Float, 0, vertices);
					}
					if (numTexCoords != 0)
					{
						GL.TexCoordPointer<float>(2, TexCoordPointerType.Float, 0, texCoords);
					}
					if (numColors != 0)
					{
						GL.ColorPointer<byte>(4, ColorPointerType.UnsignedByte, 0, colors);
					}

					// draw the arrays
					if (numIndices != 0)
					{
						GL.DrawElements<uint>(drawMode, (int)numIndices, UnsignedIntElement, indices);
					}
					else
					{
						GL.DrawArrays(drawMode, 0, (int)numVertices);
					}
					/*
					Console.WriteLine("Insets: " + vertices.Length + "  " + texCoords.Length + "  " + colors.Length + "  " + indices.Length);
					foreach (float item in vertices) Console.Write(" " + item); Console.WriteLine();
					foreach (float item in texCoords) Console.Write(" " + item); Console.WriteLine();
					foreach (byte item in colors) Console.Write(" " + item); Console.WriteLine();
					foreach (int item in indices) Console.Write(" " + item); Console.WriteLine();
					*/
				}

				// clear the arrays
				if (numVertices != 0)
				{
					GL.VertexPointer(2, VertexPointerType.Float, 0, IntPtr.Zero);
				}
				if (numTexCoords != 0)
				{
					GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, IntPtr.Zero);
				}
				if (numColors != 0)
				{
					GL.ColorPointer(4, ColorPointerType.UnsignedByte, 0, IntPtr.Zero);
				}

				// disable the arrays
				GL.DisableClientState(ArrayCap.TextureCoordArray);
				GL.DisableClientState(ArrayCap.VertexArray);
				GL.DisableClientState(ArrayCap.ColorArray);
			}
		}

		/// <summary>
		/// determines if we can use vertex buffer objects (VBO's)
		/// </summary>
		private static bool UsingVBO
		{
			get
			{
				if (!gotUsingVBO)
				{
					usingVBO = GL.GetString(StringName.Extensions).Contains("GL_ARB_vertex_buffer_object");
					usingOESVBO = GL.GetString(StringName.Extensions).Contains("GL_OES_vertex_array_object");
					Console.WriteLine("OpenGL: usingVBO = " + usingVBO + "   usingOESVBO = " + usingOESVBO);
					gotUsingVBO = true;
				}
				//return false;
				return usingVBO;
			}
		}
	}

	/// <summary>
	/// OpenGL 2D vertex used for VBO access.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct FN2DVertex
	{
		public float x, y;
		public float s, t;
		public byte r, g, b, a;
	}
}
