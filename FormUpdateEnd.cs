using CustomerFirmwareUpdater;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WirelessSetFWU.Resources;

namespace WirelessSetFWU
{
  public class FormUpdateEnd : Form
  {
    private Point lastPoint = Point.Empty;
    private IContainer components;
    private Label labelHeader;
    private Label labeldev1;
    private Label labeldevupdatenew;
    private Label labelupdatefinish;
    private Label labeldev2;
    private MyButton buttonClose;
    private Label labeldev3;
    private MyButton myButtonstartagain;
    private Label label1;
    private Label labelCongratulation;
    private Label labelDevFWver;

    public FormUpdateEnd() => this.InitializeComponent();

    private void FormUpdateEnd_Load(object sender, EventArgs e)
    {
      this.DoubleBuffered = true;
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.DoubleBuffer, true);
      if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
      {
        if (Common.fordummy)
          this.labelHeader.Text = string.Format(ResourceStr.Title.ToUpper(), (object) Common.updateInfo.GetDeviceName(2).ToUpper()) + "(DUMMY)";
        else
          this.labelHeader.Text = string.Format(ResourceStr.Title.ToUpper(), (object) Common.updateInfo.GetDeviceName(2).ToUpper());
      }
      else if (Common.fordummy)
        this.labelHeader.Text = string.Format(ResourceStr.Title.ToUpper(), (object) Common.updateInfo.GetProductName(2).ToUpper()) + "(DUMMY)";
      else
        this.labelHeader.Text = string.Format(ResourceStr.Title.ToUpper(), (object) Common.updateInfo.GetProductName(2).ToUpper());
      this.labelHeader.ForeColor = Common.greendarktheme;
      this.Text = this.labelHeader.Text;
      if (Common.fordummy)
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.blank;
      switch (Common.updateInfo.GetPID(1))
      {
        case "0072":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.mialcmambawirless;
          break;
        case "0077":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.ProClick;
          break;
        case "007B":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.viper_ult;
          break;
        case "007D":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.deathadderv2pro;
          break;
        case "0088":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.basilisk_ult;
          break;
        case "0090":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.nagapro;
          break;
        case "00A6":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.viperv2pro_black;
          break;
        case "00AB":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.basiliskv3pro;
          break;
        case "025C":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.BlackWidowV3Pro;
          break;
        case "0271":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.bw3minihyperspeed;
          break;
        case "0904":
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.blank;
          break;
      }
      if (Common.updateInfo.GetPID(1) == "0904")
      {
        this.labelCongratulation.Visible = false;
        this.labelDevFWver.Visible = false;
        this.label1.Visible = false;
        this.labeldevupdatenew.ForeColor = Color.White;
        this.labelupdatefinish.Text = ResourceStr.updatefinish;
        this.labelupdatefinish.ForeColor = Common.greendarktheme;
        this.labeldev2.ForeColor = Color.White;
        this.labeldev1.ForeColor = Color.White;
        if (Common.DongleStatus == 0 && Common.Mousestatus == 0 && Common.KBstatus == 0)
        {
          this.labelupdatefinish.Text = ResourceStr.finished;
          this.labelupdatefinish.ForeColor = Common.greendarktheme;
          this.labeldevupdatenew.Text = ResourceStr.nodevupdated;
          this.labeldevupdatenew.ForeColor = Color.White;
          this.labeldev1.Visible = false;
          this.labeldev2.Visible = false;
          this.labeldev3.Visible = false;
        }
        else if (Common.DongleStatus != 2 && Common.Mousestatus != 2 && Common.KBstatus != 2)
        {
          this.labeldev2.Visible = false;
          this.labeldev1.Visible = false;
          this.labelupdatefinish.Text = ResourceStr.finished;
          this.labelupdatefinish.ForeColor = Common.greendarktheme;
          this.labeldevupdatenew.Text = ResourceStr.nodevupdated;
          this.labeldevupdatenew.ForeColor = Color.White;
        }
        else if (Common.DongleStatus != 2 && Common.Mousestatus != 2 && Common.KBstatus == 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = false;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " Mouse";
          else
            this.labeldev1.Text = ResourceStr.keyboard;
        }
        else if (Common.DongleStatus != 2 && Common.Mousestatus == 2 && Common.KBstatus != 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = false;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dongle";
          else
            this.labeldev1.Text = ResourceStr.mouse;
        }
        else if (Common.DongleStatus == 2 && Common.Mousestatus != 2 && Common.KBstatus != 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = false;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dock";
          else
            this.labeldev1.Text = ResourceStr.wirelessdongle;
        }
        else if (Common.DongleStatus == 2 && Common.Mousestatus == 2 && Common.KBstatus != 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = true;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
          {
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dock";
            this.labeldev2.Text = Common.updateInfo.GetProductName(3) + " dongle";
          }
          else
          {
            this.labeldev1.Text = ResourceStr.wirelessdongle;
            this.labeldev2.Text = ResourceStr.mouse;
          }
        }
        else if (Common.DongleStatus == 2 && Common.Mousestatus != 2 && Common.KBstatus == 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = true;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
          {
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dock";
            this.labeldev2.Text = Common.updateInfo.GetProductName(3) + " mouse";
          }
          else
          {
            this.labeldev1.Text = ResourceStr.wirelessdongle;
            this.labeldev2.Text = ResourceStr.keyboard;
          }
        }
        else if (Common.DongleStatus != 2 && Common.Mousestatus == 2 && Common.KBstatus == 2)
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = true;
          this.labeldev3.Visible = false;
          if (Common.updateInfo.GetPID(3) == "007A")
          {
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dongle";
            this.labeldev2.Text = Common.updateInfo.GetProductName(3) + " mouse";
          }
          else
          {
            this.labeldev1.Text = ResourceStr.mouse;
            this.labeldev2.Text = ResourceStr.keyboard;
          }
        }
        else
        {
          this.labeldevupdatenew.Text = ResourceStr.deviceupdatenewver;
          this.labeldev2.Visible = true;
          this.labeldev3.Visible = true;
          this.labeldev1.Visible = true;
          if (Common.updateInfo.GetPID(3) == "007A")
          {
            this.labeldev1.Text = Common.updateInfo.GetProductName(3) + " dock";
            this.labeldev2.Text = Common.updateInfo.GetProductName(3) + " dongle";
            this.labeldev3.Text = Common.updateInfo.GetProductName(3) + " mouse";
          }
          else
          {
            this.labeldev1.Text = ResourceStr.wirelessdongle;
            this.labeldev2.Text = ResourceStr.mouse;
            this.labeldev3.Text = ResourceStr.keyboard;
          }
        }
        this.myButtonstartagain.Text = ResourceStr.startagain;
        this.myButtonstartagain.ForeColor = Common.btnfontcolor;
        this.buttonClose.Text = ResourceStr.exit;
        this.buttonClose.ForeColor = Common.btnfontcolor;
      }
      else
      {
        this.labelupdatefinish.Visible = false;
        this.labeldevupdatenew.Visible = false;
        this.labeldev1.Visible = false;
        this.labeldev2.Visible = false;
        this.labeldev3.Visible = false;
        this.myButtonstartagain.Visible = false;
        this.buttonClose.ForeColor = Common.btnfontcolor;
        this.buttonClose.Text = ResourceStr.close;
        this.label1.ForeColor = Common.greendarktheme;
        this.labelCongratulation.ForeColor = Common.greendarktheme;
        this.labelCongratulation.Text = ResourceStr.updatesuccessful;
        this.label1.Text = ResourceStr.devicewithnewver;
        this.labelDevFWver.ForeColor = Common.lightgray;
        this.labelDevFWver.Text = ResourceStr.devicever + Common.updateInfo.ActDevFWVer;
      }
    }

    private void FormUpdateEnd_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        if (this.lastPoint.IsEmpty)
        {
          this.lastPoint = new Point(e.X, e.Y);
        }
        else
        {
          this.lastPoint.X = e.X;
          this.lastPoint.Y = e.Y;
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
      }
    }

    private void FormUpdateEnd_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      this.Left += e.X - this.lastPoint.X;
      this.Top += e.Y - this.lastPoint.Y;
    }

    private void buttonClose_MouseEnter(object sender, EventArgs e) => this.buttonClose.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_hover;

    private void buttonClose_MouseLeave(object sender, EventArgs e) => this.buttonClose.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;

    private void buttonClose_Click(object sender, EventArgs e)
    {
      Common.NextPage = PageIndex.Close;
      this.Close();
    }

    private void FormUpdateEnd_Shown(object sender, EventArgs e)
    {
      float num = (float) this.Width / 730f;
      while ((double) this.labelHeader.Width > (double) this.Width - (double) Common.logowidth * (double) num)
        this.labelHeader.Font = new Font(this.labelHeader.Font.FontFamily, this.labelHeader.Font.Size - 1f);
      MyButton buttonClose = this.buttonClose;
      Size size = this.Size;
      int x = size.Width - this.buttonClose.Width - 40;
      size = this.Size;
      int y = size.Height - this.buttonClose.Height - Common.hspacebtnbottom;
      Point point = new Point(x, y);
      buttonClose.Location = point;
      this.myButtonstartagain.Location = new Point(this.buttonClose.Location.X - Common.wspacebutton - this.myButtonstartagain.Width, this.buttonClose.Location.Y);
      Common.NextPage = PageIndex.Close;
    }

    private void myButtonstartagain_Click(object sender, EventArgs e)
    {
      Common.NextPage = PageIndex.FormGuider;
      this.Close();
    }

    private void myButtonstartagain_MouseEnter(object sender, EventArgs e) => this.myButtonstartagain.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_hover;

    private void myButtonstartagain_MouseLeave(object sender, EventArgs e) => this.myButtonstartagain.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (FormUpdateEnd));
      this.labelHeader = new Label();
      this.labeldev1 = new Label();
      this.labeldevupdatenew = new Label();
      this.labelupdatefinish = new Label();
      this.labeldev2 = new Label();
      this.labeldev3 = new Label();
      this.myButtonstartagain = new MyButton();
      this.buttonClose = new MyButton();
      this.label1 = new Label();
      this.labelCongratulation = new Label();
      this.labelDevFWver = new Label();
      this.SuspendLayout();
      this.labelHeader.AutoSize = true;
      this.labelHeader.BackColor = Color.Transparent;
      this.labelHeader.Font = new Font("Microsoft Sans Serif", 13f);
      this.labelHeader.ImageKey = "(none)";
      this.labelHeader.ImeMode = ImeMode.NoControl;
      this.labelHeader.Location = new Point(30, 32);
      this.labelHeader.Name = "labelHeader";
      this.labelHeader.Size = new Size(368, 22);
      this.labelHeader.TabIndex = 2;
      this.labelHeader.Text = "RAZER MANO'WAR FIRMWARE UPDATER";
      this.labelHeader.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labelHeader.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labeldev1.BackColor = Color.Transparent;
      this.labeldev1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labeldev1.Location = new Point(35, 181);
      this.labeldev1.Margin = new Padding(0);
      this.labeldev1.Name = "labeldev1";
      this.labeldev1.Size = new Size(648, 26);
      this.labeldev1.TabIndex = 28;
      this.labeldev1.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labeldev1.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labeldevupdatenew.BackColor = Color.Transparent;
      this.labeldevupdatenew.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labeldevupdatenew.Location = new Point(35, 145);
      this.labeldevupdatenew.Margin = new Padding(0);
      this.labeldevupdatenew.Name = "labeldevupdatenew";
      this.labeldevupdatenew.Size = new Size(694, 26);
      this.labeldevupdatenew.TabIndex = 27;
      this.labeldevupdatenew.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labeldevupdatenew.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labelupdatefinish.BackColor = Color.Transparent;
      this.labelupdatefinish.Font = new Font("Microsoft Sans Serif", 13.875f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelupdatefinish.Location = new Point(35, 104);
      this.labelupdatefinish.Margin = new Padding(0);
      this.labelupdatefinish.Name = "labelupdatefinish";
      this.labelupdatefinish.Size = new Size(648, 26);
      this.labelupdatefinish.TabIndex = 26;
      this.labelupdatefinish.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labelupdatefinish.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labeldev2.BackColor = Color.Transparent;
      this.labeldev2.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labeldev2.Location = new Point(35, 212);
      this.labeldev2.Margin = new Padding(0);
      this.labeldev2.Name = "labeldev2";
      this.labeldev2.Size = new Size(648, 26);
      this.labeldev2.TabIndex = 29;
      this.labeldev2.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labeldev2.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labeldev3.BackColor = Color.Transparent;
      this.labeldev3.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labeldev3.Location = new Point(35, 246);
      this.labeldev3.Margin = new Padding(0);
      this.labeldev3.Name = "labeldev3";
      this.labeldev3.Size = new Size(648, 26);
      this.labeldev3.TabIndex = 31;
      this.labeldev3.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labeldev3.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.myButtonstartagain.AutoSize = true;
      this.myButtonstartagain.BackColor = Color.White;
      this.myButtonstartagain.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
      this.myButtonstartagain.BackgroundImageLayout = ImageLayout.Stretch;
      this.myButtonstartagain.Cursor = Cursors.Default;
      this.myButtonstartagain.EnabledSet = true;
      this.myButtonstartagain.FlatAppearance.BorderSize = 0;
      this.myButtonstartagain.FlatStyle = FlatStyle.Flat;
      this.myButtonstartagain.Font = new Font("Microsoft Sans Serif", 11.25f);
      this.myButtonstartagain.ForeColor = Color.White;
      this.myButtonstartagain.ImeMode = ImeMode.NoControl;
      this.myButtonstartagain.Location = new Point(467, 514);
      this.myButtonstartagain.Margin = new Padding(0);
      this.myButtonstartagain.Name = "myButtonstartagain";
      this.myButtonstartagain.Size = new Size(116, 30);
      this.myButtonstartagain.TabIndex = 32;
      this.myButtonstartagain.TabStop = false;
      this.myButtonstartagain.Text = "START AGAIN";
      this.myButtonstartagain.UseVisualStyleBackColor = false;
      this.myButtonstartagain.Click += new EventHandler(this.myButtonstartagain_Click);
      this.myButtonstartagain.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.myButtonstartagain.MouseEnter += new EventHandler(this.myButtonstartagain_MouseEnter);
      this.myButtonstartagain.MouseLeave += new EventHandler(this.myButtonstartagain_MouseLeave);
      this.myButtonstartagain.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.buttonClose.AutoSize = true;
      this.buttonClose.BackColor = Color.White;
      this.buttonClose.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
      this.buttonClose.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonClose.Cursor = Cursors.Default;
      this.buttonClose.EnabledSet = true;
      this.buttonClose.FlatAppearance.BorderSize = 0;
      this.buttonClose.FlatStyle = FlatStyle.Flat;
      this.buttonClose.Font = new Font("Microsoft Sans Serif", 11.25f);
      this.buttonClose.ForeColor = Color.White;
      this.buttonClose.ImeMode = ImeMode.NoControl;
      this.buttonClose.Location = new Point(598, 514);
      this.buttonClose.Margin = new Padding(0);
      this.buttonClose.Name = "buttonClose";
      this.buttonClose.Size = new Size(90, 30);
      this.buttonClose.TabIndex = 30;
      this.buttonClose.TabStop = false;
      this.buttonClose.Text = "UPDATE";
      this.buttonClose.UseVisualStyleBackColor = false;
      this.buttonClose.Click += new EventHandler(this.buttonClose_Click);
      this.buttonClose.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.buttonClose.MouseEnter += new EventHandler(this.buttonClose_MouseEnter);
      this.buttonClose.MouseLeave += new EventHandler(this.buttonClose_MouseLeave);
      this.buttonClose.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.label1.BackColor = Color.Transparent;
      this.label1.Font = new Font("Microsoft Sans Serif", 12f);
      this.label1.ImeMode = ImeMode.NoControl;
      this.label1.Location = new Point(0, 431);
      this.label1.Name = "label1";
      this.label1.Size = new Size(730, 46);
      this.label1.TabIndex = 34;
      this.label1.Text = "Your device has been updated to the latest firmware.";
      this.label1.TextAlign = ContentAlignment.MiddleCenter;
      this.label1.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.label1.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labelCongratulation.BackColor = Color.Transparent;
      this.labelCongratulation.Font = new Font("Microsoft Sans Serif", 14.25f);
      this.labelCongratulation.ImeMode = ImeMode.NoControl;
      this.labelCongratulation.Location = new Point(0, 389);
      this.labelCongratulation.Name = "labelCongratulation";
      this.labelCongratulation.Size = new Size(730, 46);
      this.labelCongratulation.TabIndex = 33;
      this.labelCongratulation.Text = "CONGRATULATIONS!";
      this.labelCongratulation.TextAlign = ContentAlignment.MiddleCenter;
      this.labelCongratulation.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labelCongratulation.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.labelDevFWver.AutoSize = true;
      this.labelDevFWver.BackColor = Color.Transparent;
      this.labelDevFWver.Font = new Font("Microsoft Sans Serif", 12f);
      this.labelDevFWver.ImeMode = ImeMode.NoControl;
      this.labelDevFWver.Location = new Point(36, 517);
      this.labelDevFWver.Margin = new Padding(0);
      this.labelDevFWver.Name = "labelDevFWver";
      this.labelDevFWver.Size = new Size(119, 20);
      this.labelDevFWver.TabIndex = 35;
      this.labelDevFWver.Text = "Device Version:";
      this.labelDevFWver.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.labelDevFWver.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.viper_ult;
      this.BackgroundImageLayout = ImageLayout.Stretch;
      this.ClientSize = new Size(730, 574);
      this.Controls.Add((Control) this.labelDevFWver);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.labelCongratulation);
      this.Controls.Add((Control) this.myButtonstartagain);
      this.Controls.Add((Control) this.labeldev3);
      this.Controls.Add((Control) this.buttonClose);
      this.Controls.Add((Control) this.labeldev2);
      this.Controls.Add((Control) this.labeldev1);
      this.Controls.Add((Control) this.labeldevupdatenew);
      this.Controls.Add((Control) this.labelupdatefinish);
      this.Controls.Add((Control) this.labelHeader);
      this.DoubleBuffered = true;
      this.ForeColor = Color.Transparent;
      this.FormBorderStyle = FormBorderStyle.None;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Margin = new Padding(1);
      this.Name = nameof (FormUpdateEnd);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = nameof (FormUpdateEnd);
      this.TopMost = true;
      this.Load += new EventHandler(this.FormUpdateEnd_Load);
      this.Shown += new EventHandler(this.FormUpdateEnd_Shown);
      this.MouseDown += new MouseEventHandler(this.FormUpdateEnd_MouseDown);
      this.MouseMove += new MouseEventHandler(this.FormUpdateEnd_MouseMove);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
