using CustomerFirmwareUpdater;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WirelessSetFWU.Resources;

namespace WirelessSetFWU
{
  public class FormGuider : Form
  {
    private Point lastPoint = Point.Empty;
    private DeviceInterface device;
    private IContainer components;
    private Label labelHeader;
    private Label labelguidemessage;
    private Label labelrecommandmessage;
    private MyButton buttonNext;
    private MyButton buttonCancel;
    private BackgroundWorker backgroundWorkerCloseRazerApps;
    private Label labelshutdownrazerapps;
    private Label labellaptoppower;

    public FormGuider(DeviceInterface device)
    {
      this.InitializeComponent();
      this.device = device;
    }

    private void FormGuider_Load(object sender, EventArgs e)
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
      this.labelguidemessage.ForeColor = Color.White;
      this.labelrecommandmessage.ForeColor = Color.White;
      this.labelshutdownrazerapps.ForeColor = Color.White;
      this.labellaptoppower.ForeColor = Color.White;
      this.buttonCancel.Text = ResourceStr.cancel;
      this.buttonNext.Text = ResourceStr.next;
      this.buttonCancel.ForeColor = Common.btnfontcolor;
      this.buttonNext.ForeColor = Common.btnfontcolor;
      Point location;
      if (Common.updateInfo.GetPID(1) == "0904")
      {
        this.labelrecommandmessage.Text = ResourceStr.recommandmessage;
        this.labelguidemessage.Text = ResourceStr.guidemessage;
        this.labellaptoppower.Text = ResourceStr.laptoppower;
        this.labelshutdownrazerapps.Text = ResourceStr.shutdownrazerapps;
        if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CHT")
        {
          Label labelrecommandmessage = this.labelrecommandmessage;
          location = this.labelrecommandmessage.Location;
          int x1 = location.X;
          location = this.labelrecommandmessage.Location;
          int y1 = location.Y + 17;
          Point point1 = new Point(x1, y1);
          labelrecommandmessage.Location = point1;
          Label labelguidemessage = this.labelguidemessage;
          location = this.labelguidemessage.Location;
          int x2 = location.X;
          location = this.labelguidemessage.Location;
          int y2 = location.Y + 30;
          Point point2 = new Point(x2, y2);
          labelguidemessage.Location = point2;
        }
      }
      else
      {
        this.labellaptoppower.Visible = false;
        this.labelshutdownrazerapps.Visible = false;
        Label labelguidemessage = this.labelguidemessage;
        location = this.labelguidemessage.Location;
        int x3 = location.X;
        location = this.labelguidemessage.Location;
        int y3 = location.Y + 80;
        Point point3 = new Point(x3, y3);
        labelguidemessage.Location = point3;
        Label labelrecommandmessage = this.labelrecommandmessage;
        location = this.labelrecommandmessage.Location;
        int x4 = location.X;
        location = this.labelrecommandmessage.Location;
        int y4 = location.Y + 60;
        Point point4 = new Point(x4, y4);
        labelrecommandmessage.Location = point4;
        this.labelrecommandmessage.Text = ResourceStr.shutdownrazerapps;
        if (Common.updateInfo.GetPID(1) == "025C" || Common.updateInfo.GetPID(1) == "0271")
          this.labelguidemessage.Text = ResourceStr.guidemessageforkb;
        else
          this.labelguidemessage.Text = ResourceStr.guidemessageformouse;
      }
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
          this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.mika_turret;
          break;
      }
      this.buttonNext.Location = new Point(this.Size.Width - this.buttonNext.Width - 40, this.Size.Height - this.buttonNext.Height - Common.hspacebtnbottom);
      MyButton buttonCancel = this.buttonCancel;
      location = this.buttonNext.Location;
      int x = location.X - Common.wspacebutton - this.buttonCancel.Width;
      location = this.buttonNext.Location;
      int y = location.Y;
      Point point = new Point(x, y);
      buttonCancel.Location = point;
    }

    private void buttonCancel_MouseEnter(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_hover;

    private void buttonCancel_MouseLeave(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_clicked;
      Common.NextPage = PageIndex.Close;
      this.Close();
    }

    private void buttonNext_MouseEnter(object sender, EventArgs e) => this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_hover;

    private void buttonNext_MouseLeave(object sender, EventArgs e) => this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;

    private void buttonNext_Click(object sender, EventArgs e)
    {
      if (!this.backgroundWorkerCloseRazerApps.IsBusy)
        this.backgroundWorkerCloseRazerApps.RunWorkerAsync();
      this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
      this.buttonNext.Enabled = false;
    }

    private void FormGuider_MouseDown(object sender, MouseEventArgs e)
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

    private void FormGuider_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      this.Left += e.X - this.lastPoint.X;
      this.Top += e.Y - this.lastPoint.Y;
    }

    private void FormGuider_Shown(object sender, EventArgs e)
    {
      Common.NextPage = PageIndex.Close;
      float num = (float) this.Width / 730f;
      while ((double) this.labelHeader.Width > (double) this.Width - (double) Common.logowidth * (double) num)
        this.labelHeader.Font = new Font(this.labelHeader.Font.FontFamily, this.labelHeader.Font.Size - 1f);
    }

    private void backgroundWorkerCloseRazerApps_DoWork(object sender, DoWorkEventArgs e)
    {
      this.device.StopSvc("RzActionSvc");
      this.device.StopSvc("Razer Chroma SDK Server");
      this.device.StopSvc("Razer Chroma SDK Service");
      this.device.StopSvc("Razer Game Manager Service");
      this.device.StopSvc("Razer Synapse Service");
    }

    private void backgroundWorkerCloseRazerApps_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_click;
      Common.NextPage = PageIndex.dev1;
      this.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (FormGuider));
      this.labelHeader = new Label();
      this.labelguidemessage = new Label();
      this.labelrecommandmessage = new Label();
      this.backgroundWorkerCloseRazerApps = new BackgroundWorker();
      this.labelshutdownrazerapps = new Label();
      this.labellaptoppower = new Label();
      this.buttonNext = new MyButton();
      this.buttonCancel = new MyButton();
      this.SuspendLayout();
      this.labelHeader.AutoSize = true;
      this.labelHeader.BackColor = Color.Transparent;
      this.labelHeader.Font = new Font("Microsoft Sans Serif", 13f);
      this.labelHeader.ImageKey = "(none)";
      this.labelHeader.ImeMode = ImeMode.NoControl;
      this.labelHeader.Location = new Point(35, 32);
      this.labelHeader.Name = "labelHeader";
      this.labelHeader.Size = new Size(368, 22);
      this.labelHeader.TabIndex = 3;
      this.labelHeader.Text = "RAZER MANO'WAR FIRMWARE UPDATER";
      this.labelHeader.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.labelHeader.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.labelguidemessage.BackColor = Color.Transparent;
      this.labelguidemessage.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelguidemessage.ImageKey = "(none)";
      this.labelguidemessage.ImeMode = ImeMode.NoControl;
      this.labelguidemessage.Location = new Point(39, 321);
      this.labelguidemessage.Name = "labelguidemessage";
      this.labelguidemessage.Size = new Size(638, 37);
      this.labelguidemessage.TabIndex = 5;
      this.labelguidemessage.Text = "This utility will check the firmware, and update your mouse if necessary.";
      this.labelguidemessage.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.labelguidemessage.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.labelrecommandmessage.BackColor = Color.Transparent;
      this.labelrecommandmessage.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelrecommandmessage.ImageKey = "(none)";
      this.labelrecommandmessage.ImeMode = ImeMode.NoControl;
      this.labelrecommandmessage.Location = new Point(39, 377);
      this.labelrecommandmessage.Name = "labelrecommandmessage";
      this.labelrecommandmessage.Size = new Size(638, 60);
      this.labelrecommandmessage.TabIndex = 6;
      this.labelrecommandmessage.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.labelrecommandmessage.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.backgroundWorkerCloseRazerApps.DoWork += new DoWorkEventHandler(this.backgroundWorkerCloseRazerApps_DoWork);
      this.backgroundWorkerCloseRazerApps.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerCloseRazerApps_RunWorkerCompleted);
      this.labelshutdownrazerapps.BackColor = Color.Transparent;
      this.labelshutdownrazerapps.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelshutdownrazerapps.ImageKey = "(none)";
      this.labelshutdownrazerapps.ImeMode = ImeMode.NoControl;
      this.labelshutdownrazerapps.Location = new Point(39, 434);
      this.labelshutdownrazerapps.Name = "labelshutdownrazerapps";
      this.labelshutdownrazerapps.Size = new Size(638, 30);
      this.labelshutdownrazerapps.TabIndex = 24;
      this.labelshutdownrazerapps.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.labelshutdownrazerapps.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.labellaptoppower.BackColor = Color.Transparent;
      this.labellaptoppower.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labellaptoppower.ImageKey = "(none)";
      this.labellaptoppower.ImeMode = ImeMode.NoControl;
      this.labellaptoppower.Location = new Point(39, 472);
      this.labellaptoppower.Name = "labellaptoppower";
      this.labellaptoppower.Size = new Size(638, 40);
      this.labellaptoppower.TabIndex = 25;
      this.labellaptoppower.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.labellaptoppower.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.buttonNext.AutoSize = true;
      this.buttonNext.BackColor = Color.White;
      this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
      this.buttonNext.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonNext.Cursor = Cursors.Default;
      this.buttonNext.EnabledSet = true;
      this.buttonNext.FlatAppearance.BorderSize = 0;
      this.buttonNext.FlatStyle = FlatStyle.Flat;
      this.buttonNext.Font = new Font("Microsoft Sans Serif", 11.25f);
      this.buttonNext.ForeColor = Color.White;
      this.buttonNext.ImeMode = ImeMode.NoControl;
      this.buttonNext.Location = new Point(587, 519);
      this.buttonNext.Margin = new Padding(0);
      this.buttonNext.Name = "buttonNext";
      this.buttonNext.Size = new Size(90, 30);
      this.buttonNext.TabIndex = 23;
      this.buttonNext.TabStop = false;
      this.buttonNext.Text = "NEXT";
      this.buttonNext.UseVisualStyleBackColor = false;
      this.buttonNext.Click += new EventHandler(this.buttonNext_Click);
      this.buttonNext.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.buttonNext.MouseEnter += new EventHandler(this.buttonNext_MouseEnter);
      this.buttonNext.MouseLeave += new EventHandler(this.buttonNext_MouseLeave);
      this.buttonNext.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.buttonCancel.AutoSize = true;
      this.buttonCancel.BackColor = Color.White;
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
      this.buttonCancel.BackgroundImageLayout = ImageLayout.Stretch;
      this.buttonCancel.Cursor = Cursors.Default;
      this.buttonCancel.EnabledSet = true;
      this.buttonCancel.FlatAppearance.BorderSize = 0;
      this.buttonCancel.FlatStyle = FlatStyle.Flat;
      this.buttonCancel.Font = new Font("Microsoft Sans Serif", 11.25f);
      this.buttonCancel.ForeColor = Color.White;
      this.buttonCancel.ImeMode = ImeMode.NoControl;
      this.buttonCancel.Location = new Point(474, 519);
      this.buttonCancel.Margin = new Padding(0);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(90, 30);
      this.buttonCancel.TabIndex = 22;
      this.buttonCancel.TabStop = false;
      this.buttonCancel.Text = "CANCEL";
      this.buttonCancel.UseVisualStyleBackColor = false;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.buttonCancel.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.buttonCancel.MouseEnter += new EventHandler(this.buttonCancel_MouseEnter);
      this.buttonCancel.MouseLeave += new EventHandler(this.buttonCancel_MouseLeave);
      this.buttonCancel.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.bw3minihyperspeed;
      this.BackgroundImageLayout = ImageLayout.Stretch;
      this.ClientSize = new Size(730, 574);
      this.Controls.Add((Control) this.labellaptoppower);
      this.Controls.Add((Control) this.labelshutdownrazerapps);
      this.Controls.Add((Control) this.buttonNext);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.labelrecommandmessage);
      this.Controls.Add((Control) this.labelguidemessage);
      this.Controls.Add((Control) this.labelHeader);
      this.DoubleBuffered = true;
      this.FormBorderStyle = FormBorderStyle.None;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Name = nameof (FormGuider);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = nameof (FormGuider);
      this.TopMost = true;
      this.Load += new EventHandler(this.FormGuider_Load);
      this.Shown += new EventHandler(this.FormGuider_Shown);
      this.MouseDown += new MouseEventHandler(this.FormGuider_MouseDown);
      this.MouseMove += new MouseEventHandler(this.FormGuider_MouseMove);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
