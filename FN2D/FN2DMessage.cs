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
	public class FN2DMessage : FN2DControl
	{
		private const int MARGIN = 20;
		private const int PADDING = 20;
		private const int TITLE_MARGIN = 4;
		private const int TITLE_HEIGHT = 30;

		private FN2DLabel titleLabel, messageLabel;

		public FN2DMessage(FN2DCanvas canvas, string title, string message, FN2DButton[] buttons)
			: base(canvas)
		{
			Initialize(title, message, buttons);
		}

		public FN2DMessage(FN2DCanvas canvas, string title, string message, FN2DMessageButtons buttons)
			: base(canvas)
		{
			FN2DButton[] btns = null;
			switch (buttons)
			{
				case FN2DMessageButtons.OK:
					btns = new FN2DButton[1];
					btns[0] = CreateButton("OK", Color.LightGreen, Color.Green, HandleOKButtonTapped);
					break;
				case FN2DMessageButtons.Cancel:
					btns = new FN2DButton[1];
					btns[0] = CreateButton("Cancel", Color.Red, Color.DarkRed, HandleCancelButtonTapped);
					break;
				case FN2DMessageButtons.OKCancel:
					btns = new FN2DButton[2];
					btns[0] = CreateButton("OK", Color.LightGreen, Color.Green, HandleOKButtonTapped);
					btns[1] = CreateButton("Cancel", Color.Red, Color.DarkRed, HandleCancelButtonTapped);
					break;
				case FN2DMessageButtons.YesNo:
					btns = new FN2DButton[2];
					btns[0] = CreateButton("Yes", Color.LightGreen, Color.Green, HandleYesButtonTapped);
					btns[1] = CreateButton("No", Color.Red, Color.DarkRed, HandleNoButtonTapped);
					break;
				case FN2DMessageButtons.YesNoCancel:
					btns = new FN2DButton[3];
					btns[0] = CreateButton("Yes", Color.LightGreen, Color.Green, HandleYesButtonTapped);
					btns[1] = CreateButton("No", Color.DodgerBlue, Color.DarkBlue, HandleNoButtonTapped);
					btns[2] = CreateButton("Cancel", Color.Red, Color.DarkRed, HandleCancelButtonTapped);
					break;
			}
			Initialize(title, message, btns);
		}

		public FN2DMessage(FN2DCanvas canvas, string title, string message)
			: this(canvas, title, message, null)
		{
		}

		public FN2DMessage(FN2DCanvas canvas, string message)
			: this(canvas, null, message, null)
		{
		}

		/// <summary>
		/// Frees unmanaged resources and calls Dispose() on the member objects.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			titleLabel = null;
			messageLabel = null;
			base.Dispose(disposing);
		}

		private void Initialize(string title, string message, FN2DButton[] buttons)
		{
			// check the parameters
			if (string.IsNullOrEmpty(title)) title = FN2DCanvas.DefaultMessageTitle;
			if (string.IsNullOrEmpty(message)) message = "Message";
			if (buttons == null || buttons.Length == 0)
			{
				buttons = new FN2DButton[] { CreateButton("OK", Color.Blue, Color.DarkBlue, HandleOKButtonTapped) };
			}

			// configure the background
			TopColor = Color.LightGray;
			BottomColor = Color.DarkGray;
			CornerRadius = 5;

			// get the width of the buttons
			int buttonsWidth = -PADDING;
			foreach (FN2DButton button in buttons)
			{
				buttonsWidth += button.Width + PADDING;
			}

			// create the title label
			titleLabel = new FN2DLabel(canvas, title);
			titleLabel.Font = canvas.GetTitleFont(18, true);
			titleLabel.AutoSize = true;
			int width = (int)Math.Max(buttonsWidth, titleLabel.Width);
			titleLabel.AutoSize = false;
			titleLabel.TopColor = Color.DarkGray;
			titleLabel.BottomColor = Color.Gray;
			titleLabel.CornerRadius = 5;
			Add(titleLabel);

			// create the message label
			messageLabel = new FN2DLabel(canvas, message);
			messageLabel.Font = canvas.GetFont(14, false);
			messageLabel.TextColor = Color.Black;
			messageLabel.AutoSize = true;
			Add(messageLabel);
			if (messageLabel.Width > width)
			{
				width = messageLabel.Width;
			}
			width += MARGIN * 2;

			// position the controls
			titleLabel.Frame = new Rectangle(TITLE_MARGIN, TITLE_MARGIN, width - TITLE_MARGIN * 2, TITLE_HEIGHT);
			int y = TITLE_HEIGHT + TITLE_MARGIN + PADDING;
			messageLabel.Frame = new Rectangle(new Point((width - messageLabel.Width) / 2, y), messageLabel.Size);
			y += messageLabel.Height + PADDING;
			int x = (width - buttonsWidth) / 2;
			int height = 0;
			foreach (FN2DButton button in buttons)
			{
				button.Location = new Point(x, y);
				Add(button);
				x += button.Width + PADDING;
				if (button.Height > height)
				{
					height = button.Height;
				}
			}
			y += height + MARGIN;
			Size = new Size(width, y);
		}

		private FN2DButton CreateButton(string title, Color topColor, Color bottomColor, EventHandler tappedHandler)
		{
			FN2DButton button = new FN2DButton(canvas, new Rectangle(0, 0, 100, 44), topColor, bottomColor, title);
			button.Tapped = tappedHandler;
			return button;
		}

		private void HandleOKButtonTapped(object sender, EventArgs e)
		{
			canvas.HideModal();
		}
		
		private void HandleCancelButtonTapped(object sender, EventArgs e)
		{
			canvas.HideModal();
		}

		private void HandleYesButtonTapped(object sender, EventArgs e)
		{
			canvas.HideModal();
		}

		private void HandleNoButtonTapped(object sender, EventArgs e)
		{
			canvas.HideModal();
		}
	}

	/// <summary>
	/// Standard buttons supported by FN2DMessage.
	/// </summary>
	public enum FN2DMessageButtons
	{
		OK = 0,
		Cancel = 1,
		OKCancel = 2,
		YesNo = 3,
		YesNoCancel = 4,
	}
}
