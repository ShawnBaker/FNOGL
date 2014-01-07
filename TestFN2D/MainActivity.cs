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
using System.Drawing;
using Android.App;
using Android.Views;
using Android.OS;
using Android.Content.PM;

namespace FrozenNorth.TestFN2D
{
	// the ConfigurationChanges flags set here keep the EGL context
	// from being destroyed whenever the device is rotated or the
	// keyboard is shown (highly recommended for all GL apps)
	[Activity(Label = "TestFN2D", MainLauncher = true,
				ConfigurationChanges=ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
	public class MainActivity : Activity
	{
		private Canvas canvas;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// don't show the title bar
			RequestWindowFeature(WindowFeatures.NoTitle);

			// create the canvas
			canvas = new Canvas(this, new Size(Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels));
			SetContentView(canvas);
		}

		protected override void OnPause()
		{
			base.OnPause();
			canvas.Pause();
		}

		protected override void OnResume()
		{
			base.OnResume();
			canvas.Resume();
		}
	}
}
