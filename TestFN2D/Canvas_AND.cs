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
using System.IO;
using System.Drawing;
using Android.Content;
using Android.Content.Res;
using FrozenNorth.OpenGL.FN2D;

namespace FrozenNorth.TestFN2D
{
	public class Canvas : CanvasCommon
	{
		private AssetManager assetManager;

		public Canvas(Context context, Size size, string fontPath = null)
			: base(context, size, fontPath)
		{
			// get the asset manager
			assetManager = context.Assets;
		}
		
		public override Android.Graphics.Bitmap GetImage(string fileName)
		{
			if (!Path.HasExtension(fileName))
			{
				fileName = Path.ChangeExtension(fileName, "png");
			}
			Stream stream = assetManager.Open(Path.Combine("Images", fileName));
			Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
			stream.Dispose();
			return bitmap;
		}
		/*
		public override bool OnTouchEvent(Android.Views.MotionEvent e)
		{
			return base.OnTouchEvent(e);
			switch (e.Action)
			{
				case Android.Views.MotionEventActions.Pointer1Down:
					break;
			}
		}
		*/
	}
}
