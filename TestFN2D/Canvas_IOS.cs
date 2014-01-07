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
using MonoTouch.UIKit;
using FrozenNorth.OpenGL.FN2D;

namespace FrozenNorth.TestFN2D
{
	public class Canvas : CanvasCommon
	{
		private FN2DPinchGestureRecognizer pinchGestureRecognizer;
		private UIPanGestureRecognizer panGestureRecognizer;
		private float pinchStart;
		private PointF panLocation;
		private Size panStart;

		public Canvas(Size size, string fontPath = null)
			: base(size, fontPath)
		{
			// create the recognizers
			pinchGestureRecognizer = new FN2DPinchGestureRecognizer(image, HandlePinch);
			AddGestureRecognizer(pinchGestureRecognizer);
			panGestureRecognizer = new UIPanGestureRecognizer(HandlePan);
			panGestureRecognizer.MinimumNumberOfTouches = 2;
			panGestureRecognizer.MaximumNumberOfTouches = 2;
			AddGestureRecognizer(panGestureRecognizer);
		}
		
		private void HandlePinch()
		{
			if (ModalControl != null)
			{
				pinchGestureRecognizer.State = UIGestureRecognizerState.Failed;
				return;
			}
			if (TouchControl != null)
			{
				TouchControl.TouchCancel(FN2DTouchEventArgs.Empty);
			}
			if (pinchGestureRecognizer.State == UIGestureRecognizerState.Began)
			{
				pinchStart = image.Zoom;
			}
			else
			{
				float scale = 1;
				if (pinchGestureRecognizer.Scale > 1)
				{
					scale = (pinchGestureRecognizer.Scale - 1) / 2 + 1;
				}
				else if (pinchGestureRecognizer.Scale < 1)
				{
					scale = 1 - (1 - pinchGestureRecognizer.Scale) / 2;
				}
				float saveZoom = image.Zoom;
				image.Zoom = pinchStart * scale;
				//Console.WriteLine("Pinch: " + pinchStart + "  " + pinchGestureRecognizer.Scale + "  " + scale + "  " + saveZoom + "  " + image.Zoom);
			}
		}

		public void HandlePan()
		{
			if (ModalControl != null)
			{
				panGestureRecognizer.State = UIGestureRecognizerState.Failed;
				return;
			}
			if (TouchControl != null)
			{
				TouchControl.TouchCancel(FN2DTouchEventArgs.Empty);
			}
			if (image.CanPan)
			{
				if (panGestureRecognizer.State == UIGestureRecognizerState.Began)
				{
					panLocation = panGestureRecognizer.TranslationInView(this);
					panStart = image.Pan;
				}
				else
				{
					PointF location = panGestureRecognizer.TranslationInView(this);
					Point offset = new Point((int)Math.Round((location.X - panLocation.X) / image.Zoom), (int)Math.Round((location.Y - panLocation.Y) / image.Zoom));
					image.Pan = new Size(panStart.Width + offset.X, panStart.Height + offset.Y);
				}
			}
		}

		public override UIImage GetImage(string fileName)
		{
			if (!Path.HasExtension(fileName))
			{
				fileName = Path.ChangeExtension(fileName, "png");
			}
			return UIImage.FromBundle(Path.Combine("Assets", "Images", fileName));
		}
	}
}
