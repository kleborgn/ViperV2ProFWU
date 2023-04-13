using CustomShapedFormRegion;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WirelessSetFWU;

namespace CustomerFirmwareUpdater
{
  internal class MyButton : Button
  {
    private bool enabled = true;

    public MyButton()
    {
      this.FlatStyle = FlatStyle.Flat;
      this.FlatAppearance.BorderSize = 0;
      this.FlatAppearance.MouseDownBackColor = Color.Transparent;
      this.FlatAppearance.MouseOverBackColor = Color.Transparent;
      this.BackColor = Color.Transparent;
      this.Size = new Size(90, 27);
    }

    public static Bitmap KiResizeImage(Bitmap bmp, int newW, int newH)
    {
      try
      {
        Bitmap bitmap = new Bitmap(newW, newH);
        Graphics graphics = Graphics.FromImage((Image) bitmap);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.DrawImage((Image) bmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, bmp.Width, bmp.Height), GraphicsUnit.Pixel);
        graphics.Dispose();
        return bitmap;
      }
      catch
      {
        return (Bitmap) null;
      }
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
      base.OnPaint(pevent);
      if (this.BackgroundImage == null)
        return;
      this.Region = BitmapToRegion.getRegionFast(this.Size, new Bitmap(this.BackgroundImage), BitmapToRegion.GetBtnBackImageColor(new Bitmap(this.BackgroundImage)), 1);
    }

    protected override bool ShowFocusCues => false;

    public new Color ForeColor
    {
      get => base.ForeColor;
      set
      {
        if (!this.enabled)
          return;
        base.ForeColor = value;
      }
    }

    public new bool Enabled
    {
      get => this.enabled;
      set
      {
        this.enabled = value;
        if (!value)
        {
          this.Cursor = Cursors.Arrow;
          base.ForeColor = Common.btnfontcolor;
        }
        else
        {
          base.ForeColor = Common.btnfontcolor;
          this.Cursor = Cursors.Hand;
        }
      }
    }

    public bool EnabledSet
    {
      get => base.Enabled;
      set => base.Enabled = value;
    }

    protected override void OnClick(EventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnClick(e);
    }

    protected override void OnDoubleClick(EventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnDoubleClick(e);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnMouseClick(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
      if (this.enabled)
      {
        this.Cursor = Cursors.Hand;
        base.OnMouseEnter(e);
      }
      else
        this.Cursor = Cursors.Arrow;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnMouseLeave(e);
    }

    protected override void OnMouseHover(EventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnMouseHover(e);
    }

    protected override void OnChangeUICues(UICuesEventArgs e)
    {
      if (!this.enabled)
        return;
      base.OnChangeUICues(e);
    }
  }
}
