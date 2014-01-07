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

namespace FrozenNorth.OpenGL.FN2D
{
	/// <summary>
	/// Rectangle insets.
	/// </summary>
	public class FN2DRectangleInsets : IEquatable<FN2DRectangleInsets>
	{
		// pulic members
		public int Top, Left, Bottom, Right;

		/// <summary>
		/// Constructor - Creates a set of rectangle insets with specific values.
		/// </summary>
		/// <param name="top">Top inset.</param>
		/// <param name="left">Left inset.</param>
		/// <param name="bottom">Bottom inset.</param>
		/// <param name="right">Right inset.</param>
		public FN2DRectangleInsets(int top, int left, int bottom, int right)
		{
			Top = top;
			Left = left;
			Bottom = bottom;
			Right = right;
		}
		
		/// <summary>
		/// Constructor - Creates a set of rectangle insets based on a width and height.
		/// </summary>
		/// <param name="width">Rectangle width.</param>
		/// <param name="height">Rectangle height.</param>
		public FN2DRectangleInsets(int width, int height)
		{
			Top = Bottom = height / 2;
			Left = Right = width / 2;
			if (width % 2 == 0)
			{
				Left--;
			}
			if (height % 2 == 0)
			{
				Top--;
			}
		}

		/// <summary>
		/// Constructor - Creates a set of rectangle insets based on a size.
		/// </summary>
		/// <param name="size">Rectangle size.</param>
		public FN2DRectangleInsets(Size size)
			: this(size.Width, size.Height)
		{
		}

		/// <summary>
		/// Determines if all the insets are zero.
		/// </summary>
		public bool IsEmpty
		{
			get { return Top == 0 && Left == 0 && Bottom == 0 && Right == 0; }
		}

		/// <summary>
		/// Determines if all the insets are non-zero.
		/// </summary>
		public bool IsFull
		{
			get { return Top != 0 && Left != 0 && Bottom != 0 && Right != 0; }
		}

		/// <summary>
		/// Compares this object with a generic object.
		/// </summary>
		public override bool Equals(object obj)
		{
			return Equals(obj as FN2DRectangleInsets);
		}

		/// <summary>
		/// Gets the total width of the insets.
		/// </summary>
		public int Width
		{
			get { return Left + Right; }
		}

		/// <summary>
		/// Gets the total height of the insets.
		/// </summary>
		public int Height
		{
			get { return Top + Bottom; }
		}

		/// <summary>
		/// Compares this object with another FN2DRectangleInsets object.
		/// </summary>
		public bool Equals(FN2DRectangleInsets insets)
		{
			return (insets != null)
						? (insets.Top == Top && insets.Left == Left && insets.Bottom == Bottom && insets.Right == Right)
						: false;
		}

		/// <summary>
		/// Gets the hash code for this instance.
		/// </summary>
		/// <returns>Hash code for this instance.</returns>
		public override int GetHashCode()
		{
			int hash = 319786951;
			hash = (hash * 37) + Top;
			hash = (hash * 37) + Left;
			hash = (hash * 37) + Bottom;
			hash = (hash * 37) + Right;
			return hash;
		}

		/// <summary>
		/// Gets the an empty set of insets.
		/// </summary>
		public static FN2DRectangleInsets Empty
		{
			get { return new FN2DRectangleInsets(0, 0, 0, 0); }
		}

		/// <summary>
		/// Gets a string representing the instance values.
		/// </summary>
		public override string ToString()
		{
			return string.Format("{0},{1} {2},{3}", Left, Top, Right, Bottom);
		}
	}
}
