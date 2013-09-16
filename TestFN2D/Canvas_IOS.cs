using System;
using System.Drawing;
using MonoTouch.UIKit;
using FrozenNorth.OpenGL.FN2D;

namespace FrozenNorth.TestFN2D
{
	public class Canvas_IOS : Canvas
	{
		private FN2DPinchGestureRecognizer pinchGestureRecognizer;
		private UIPanGestureRecognizer panGestureRecognizer;
		private float pinchStart;
		private PointF panLocation;
		private Size panStart;

		public Canvas_IOS(Size size, string fontPath = null)
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
	}
}
