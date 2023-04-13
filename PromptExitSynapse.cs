using CustomerFirmwareUpdater;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WirelessSetFWU.Resources;

namespace WirelessSetFWU
{
  public class PromptExitSynapse : Form
  {
    private Point lastPoint = Point.Empty;
    private IContainer components;
    private Label labelHeader;
    private Label labelreminder;
    private Label labelexitsynapse;
    private Label labellaptoppower;
    private MyButton buttonNext;
    private MyButton buttonCancel;
    private Label labelrecommandmessage;

    public PromptExitSynapse() => this.InitializeComponent();

    private void PromptExitSynapse_Load(object sender, EventArgs e)
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
      this.labelreminder.ForeColor = Color.White;
      this.labellaptoppower.ForeColor = Color.White;
      this.labelexitsynapse.ForeColor = Color.White;
      this.buttonCancel.Text = ResourceStr.cancel;
      this.buttonNext.Text = ResourceStr.next;
      this.buttonCancel.ForeColor = Common.btnfontcolor;
      this.buttonNext.ForeColor = Common.btnfontcolor;
      this.labelexitsynapse.Text = ResourceStr.exitsynapse;
      this.labelreminder.Text = ResourceStr.reminder;
      this.labellaptoppower.Text = ResourceStr.laptoppower;
      this.buttonNext.Location = new Point(this.Size.Width - this.buttonNext.Width - 40, this.Size.Height - this.buttonNext.Height - Common.hspacebtnbottom);
      this.buttonCancel.Location = new Point(this.buttonNext.Location.X - Common.wspacebutton - this.buttonCancel.Width, this.buttonNext.Location.Y);
      if (Common.updateInfo.GetPID(1) == "0904")
      {
        this.labelrecommandmessage.Visible = false;
      }
      else
      {
        this.labelrecommandmessage.ForeColor = Color.White;
        this.labelrecommandmessage.Text = ResourceStr.recommandmessage;
        this.labelrecommandmessage.Visible = true;
      }
    }

    private void buttonCancel_MouseEnter(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_hover;

    private void buttonCancel_MouseLeave(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;

    private void buttonNext_MouseEnter(object sender, EventArgs e) => this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_hover;

    private void buttonNext_MouseLeave(object sender, EventArgs e) => this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_clicked;
      Common.NextPage = PageIndex.Close;
      this.Close();
    }

    private void buttonNext_Click(object sender, EventArgs e)
    {
      this.buttonNext.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_click;
      Common.NextPage = PageIndex.dev1;
      this.Close();
    }

    private void PromptExitSynapse_MouseDown(object sender, MouseEventArgs e)
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

    private void PromptExitSynapse_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      this.Left += e.X - this.lastPoint.X;
      this.Top += e.Y - this.lastPoint.Y;
    }

    private void PromptExitSynapse_Shown(object sender, EventArgs e)
    {
      Common.NextPage = PageIndex.Close;
      float num = (float) this.Width / 730f;
      while ((double) this.labelHeader.Width > (double) this.Width - (double) Common.logowidth * (double) num)
        this.labelHeader.Font = new Font(this.labelHeader.Font.FontFamily, this.labelHeader.Font.Size - 1f);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (PromptExitSynapse));
      this.labelHeader = new Label();
      this.labelreminder = new Label();
      this.labelexitsynapse = new Label();
      this.labellaptoppower = new Label();
      this.buttonNext = new MyButton();
      this.buttonCancel = new MyButton();
      this.labelrecommandmessage = new Label();
      this.SuspendLayout();
      this.labelHeader.AutoSize = true;
      this.labelHeader.BackColor = Color.Transparent;
      this.labelHeader.Font = new Font("Microsoft Sans Serif", 13f);
      this.labelHeader.ImageKey = "(none)";
      this.labelHeader.ImeMode = ImeMode.NoControl;
      this.labelHeader.Location = new Point(35, 32);
      this.labelHeader.Name = "labelHeader";
      this.labelHeader.Size = new Size(368, 22);
      this.labelHeader.TabIndex = 2;
      this.labelHeader.Text = "RAZER MANO'WAR FIRMWARE UPDATER";
      this.labelHeader.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.labelHeader.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
      this.labelreminder.AutoSize = true;
      this.labelreminder.BackColor = Color.Transparent;
      this.labelreminder.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelreminder.ImageKey = "(none)";
      this.labelreminder.ImeMode = ImeMode.NoControl;
      this.labelreminder.Location = new Point(45, 80);
      this.labelreminder.Margin = new Padding(0);
      this.labelreminder.Name = "labelreminder";
      this.labelreminder.Size = new Size(96, 20);
      this.labelreminder.TabIndex = 3;
      this.labelreminder.Text = "REMINDER";
      this.labelreminder.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.labelreminder.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
      this.labelexitsynapse.BackColor = Color.Transparent;
      this.labelexitsynapse.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelexitsynapse.ImageKey = "(none)";
      this.labelexitsynapse.ImeMode = ImeMode.NoControl;
      this.labelexitsynapse.Location = new Point(45, 112);
      this.labelexitsynapse.Margin = new Padding(0);
      this.labelexitsynapse.Name = "labelexitsynapse";
      this.labelexitsynapse.Size = new Size(638, 48);
      this.labelexitsynapse.TabIndex = 4;
      this.labelexitsynapse.Text = "Please close all applications under Razer Central before proceeding. Right mouse click on the Razer icon in the Systray and choose ‘Exit All Apps’.";
      this.labelexitsynapse.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.labelexitsynapse.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
      this.labellaptoppower.BackColor = Color.Transparent;
      this.labellaptoppower.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labellaptoppower.ImageKey = "(none)";
      this.labellaptoppower.ImeMode = ImeMode.NoControl;
      this.labellaptoppower.Location = new Point(45, 411);
      this.labellaptoppower.Name = "labellaptoppower";
      this.labellaptoppower.Size = new Size(638, 26);
      this.labellaptoppower.TabIndex = 5;
      this.labellaptoppower.Text = "If using a laptop, ensure that it is plugged into a power outlet.";
      this.labellaptoppower.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.labellaptoppower.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
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
      this.buttonNext.Location = new Point(604, 505);
      this.buttonNext.Margin = new Padding(0);
      this.buttonNext.Name = "buttonNext";
      this.buttonNext.Size = new Size(90, 30);
      this.buttonNext.TabIndex = 21;
      this.buttonNext.TabStop = false;
      this.buttonNext.Text = "NEXT";
      this.buttonNext.UseVisualStyleBackColor = false;
      this.buttonNext.Click += new EventHandler(this.buttonNext_Click);
      this.buttonNext.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.buttonNext.MouseEnter += new EventHandler(this.buttonNext_MouseEnter);
      this.buttonNext.MouseLeave += new EventHandler(this.buttonNext_MouseLeave);
      this.buttonNext.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
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
      this.buttonCancel.Location = new Point(491, 505);
      this.buttonCancel.Margin = new Padding(0);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new Size(90, 30);
      this.buttonCancel.TabIndex = 20;
      this.buttonCancel.TabStop = false;
      this.buttonCancel.Text = "CANCEL";
      this.buttonCancel.UseVisualStyleBackColor = false;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.buttonCancel.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.buttonCancel.MouseEnter += new EventHandler(this.buttonCancel_MouseEnter);
      this.buttonCancel.MouseLeave += new EventHandler(this.buttonCancel_MouseLeave);
      this.buttonCancel.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
      this.labelrecommandmessage.BackColor = Color.Transparent;
      this.labelrecommandmessage.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.labelrecommandmessage.ImageKey = "(none)";
      this.labelrecommandmessage.ImeMode = ImeMode.NoControl;
      this.labelrecommandmessage.Location = new Point(45, 439);
      this.labelrecommandmessage.Name = "labelrecommandmessage";
      this.labelrecommandmessage.Size = new Size(638, 60);
      this.labelrecommandmessage.TabIndex = 22;
      this.AutoScaleDimensions = new SizeF(96f, 96f);
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.exitsynapse;
      this.BackgroundImageLayout = ImageLayout.Stretch;
      this.ClientSize = new Size(730, 574);
      this.Controls.Add((Control) this.labelrecommandmessage);
      this.Controls.Add((Control) this.buttonNext);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.labellaptoppower);
      this.Controls.Add((Control) this.labelexitsynapse);
      this.Controls.Add((Control) this.labelreminder);
      this.Controls.Add((Control) this.labelHeader);
      this.FormBorderStyle = FormBorderStyle.None;
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Name = nameof (PromptExitSynapse);
      this.StartPosition = FormStartPosition.CenterScreen;
      this.TopMost = true;
      this.Load += new EventHandler(this.PromptExitSynapse_Load);
      this.Shown += new EventHandler(this.PromptExitSynapse_Shown);
      this.MouseDown += new MouseEventHandler(this.PromptExitSynapse_MouseDown);
      this.MouseMove += new MouseEventHandler(this.PromptExitSynapse_MouseMove);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
