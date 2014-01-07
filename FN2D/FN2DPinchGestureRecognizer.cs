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
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FrozenNorth.OpenGL.FN2D
{
	public class FN2DPinchGestureRecognizer : UIPinchGestureRecognizer
	{
		// local variables
		private FN2DControl control;

		/// <summary>
		/// Constructor - Creates the recognizer.
		/// </summary>
		/// <param name='control'>Control that the gesture must start in.</param>
		/// <param name='action'>Method that gets called when the gesture is recognized.</param>
		public FN2DPinchGestureRecognizer(FN2DControl control, NSAction action)
			: base(action)
		{
			this.control = control;
			//DelaysTouchesBegan = true;
		}

		/// <summary>
		/// Records the starting values for swipe detection.
		/// </summary>
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			// call the base handler
			base.TouchesBegan(touches, evt);
			UITouch touch = (UITouch)touches.AnyObject;
			PointF point = touch.LocationInView(control.Canvas);
			Point loc = control.Canvas.ControlLocation(control, new Point((int)point.X, (int)point.Y));
			if (loc.X < 0 || loc.X > control.Width || loc.Y < 0 || loc.Y > control.Height)
			{
				State = UIGestureRecognizerState.Failed;
			}
		}

		/// <summary>
		/// Gets/sets the control that the gesture recognizer is attached to.
		/// </summary>
		public FN2DControl Control
		{
			get { return control; }
			set { control = value; }
		}
	}
}
