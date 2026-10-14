using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace wxocr;

public class ScreenshotForm : Form
{
	private Point startPoint;

	private Rectangle selectionRectangle;

	private bool isSelecting = false;

	private Bitmap screenshot;

	private Bitmap screenCapture;

	private IContainer components = null;

	public Bitmap CapturedImage { get; private set; }

	public ScreenshotForm()
	{
		InitializeComponent();
		base.FormBorderStyle = FormBorderStyle.None;
		base.Opacity = 0.6;
		BackColor = Color.Black;
		DoubleBuffered = true;
		base.TopMost = true;
		base.ShowInTaskbar = false;
		base.Bounds = Screen.PrimaryScreen.Bounds;
		screenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
		using Graphics graphics = Graphics.FromImage(screenCapture);
		graphics.CopyFromScreen(0, 0, 0, 0, screenCapture.Size);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			startPoint = e.Location;
			isSelecting = true;
			selectionRectangle = new Rectangle(e.Location, new Size(0, 0));
			Invalidate();
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (isSelecting)
		{
			int num = Math.Min(startPoint.X, e.X);
			int num2 = Math.Min(startPoint.Y, e.Y);
			int num3 = Math.Abs(startPoint.X - e.X);
			int num4 = Math.Abs(startPoint.Y - e.Y);
			selectionRectangle = new Rectangle(num, num2, num3, num4);
			Invalidate();
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button == MouseButtons.Left && isSelecting)
		{
			isSelecting = false;
			CaptureScreenshot();
			base.DialogResult = DialogResult.OK;
			Close();
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		using (SolidBrush brush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
		{
			e.Graphics.FillRectangle(brush, base.ClientRectangle);
		}
		if (isSelecting && selectionRectangle.Width > 0 && selectionRectangle.Height > 0)
		{
			e.Graphics.DrawImage(screenCapture, selectionRectangle, selectionRectangle, GraphicsUnit.Pixel);
			using Pen pen = new Pen(Color.Red, 2f);
			e.Graphics.DrawRectangle(pen, selectionRectangle);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape)
		{
			base.DialogResult = DialogResult.Cancel;
			Close();
		}
	}

	protected override void OnClosed(EventArgs e)
	{
		base.OnClosed(e);
		screenCapture?.Dispose();
	}

	private void CaptureScreenshot()
	{
		if (selectionRectangle.Width > 0 && selectionRectangle.Height > 0)
		{
			screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
			using (Graphics graphics = Graphics.FromImage(screenshot))
			{
				graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
			}
			CapturedImage = new Bitmap(selectionRectangle.Width, selectionRectangle.Height);
			using (Graphics graphics2 = Graphics.FromImage(CapturedImage))
			{
				graphics2.DrawImage(screenshot, 0, 0, selectionRectangle, GraphicsUnit.Pixel);
			}
			screenshot.Dispose();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Name = "ScreenshotForm";
		this.Text = "ScreenshotForm";
		base.ResumeLayout(false);
	}
}
