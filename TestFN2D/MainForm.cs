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
using System.IO;
using System.Windows.Forms;

namespace FrozenNorth.TestFN2D
{
	public class MainForm : Form
	{
		private Canvas canvas;

		public MainForm()
		{
			// initialize the form
			Size = new Size(800, 600);
			StartPosition = FormStartPosition.CenterScreen;
			Text = "Test FN2D";

			// create the FN2D canvas
			canvas = new Canvas(ClientRectangle.Size, GetFontPath());
			Controls.Add(canvas);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && canvas != null)
			{
				Controls.Remove(canvas);
				canvas.Dispose();
				canvas = null;
			}
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			if (canvas != null)
			{
				canvas.Size = ClientRectangle.Size;
			}
		}
		
		public string GetFontPath()
		{
			// get the path to the executable file
			string path = Application.StartupPath;

			// remove the bin path if it exists
			string dir = Path.GetFileName(path);
			if (string.Compare(dir, "debug", true) == 0 || string.Compare(dir, "release", true) == 0)
			{
				do
				{
					path = Path.GetDirectoryName(path);
					dir = Path.GetFileName(path);
				}
				while (string.Compare(dir, "bin", true) != 0);
				path = Path.GetDirectoryName(path);
			}

			// add the Fonts directory
			return Path.Combine(path, Path.Combine("Assets", "Fonts"));
		}
	}
}
