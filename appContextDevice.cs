using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WirelessSetFWU
{
  internal class appContextDevice : ApplicationContext
  {
    protected DeviceInterface device;
    protected List<Form> deviceForms;
    protected Form currForm;
    private string strPID;

    public appContextDevice() => this.InitializeDevice();

    public void InstallBL()
    {
      bool flag1 = this.device.Is64Bit();
      bool flag2 = this.device.IsWin10orGreater();
      string currentDirectory = Environment.CurrentDirectory;
      string path;
      if (flag1 & flag2)
        path = currentDirectory.Trim('\\') + "\\BootLoader\\Win10\\amd64\\DPInst_amd64.exe";
      else if (!flag1 & flag2)
        path = currentDirectory.Trim('\\') + "\\BootLoader\\Win10\\i386\\DPInst_x86.exe";
      else if (!flag1 && !flag2)
        path = currentDirectory.Trim('\\') + "\\BootLoader\\Win81below\\i386\\DPInst_x86.exe";
      else
        path = currentDirectory.Trim('\\') + "\\BootLoader\\Win81below\\amd64\\DPInst_amd64.exe";
      this.device.InstallBLDrv(path);
    }

    public virtual void InitializeDevice()
    {
      try
      {
        Thread.CurrentThread.CurrentUICulture = CLocalize.getInstance().getCulture();
        this.device = new DeviceInterface();
        for (int devidx = 1; devidx <= Common.updateInfo.SupportDevCount; ++devidx)
        {
          if ((double) Common.updateInfo.GetBLVer(devidx) != 2.0)
            this.InstallBL();
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
        return;
      }
      this.deviceForms = new List<Form>();
      this.deviceForms.Add((Form) new FormGuider(this.device));
      this.deviceForms.Add((Form) new PromptExitSynapse());
      for (int index = 0; index < Common.updateInfo.SupportDevCount; ++index)
        this.deviceForms.Add((Form) new FormFWUStep1(this.device, index + 1));
      this.deviceForms.Add((Form) new FormUpdateEnd());
      Common.NextPage = PageIndex.FormGuider;
      this.currForm = this.deviceForms.ElementAt<Form>((int) Common.NextPage);
      this.currForm.Closed += new EventHandler(this.OnFormClosed);
      this.currForm.Show();
    }

    protected void OnFormClosed(object sender, EventArgs e)
    {
      Point location = this.currForm.Location;
      this.currForm.Dispose();
      if (Common.NextPage < PageIndex.Close)
      {
        this.currForm = this.deviceForms.ElementAt<Form>((int) Common.NextPage);
        if (this.currForm == null || this.currForm.IsDisposed)
        {
          switch (Common.NextPage)
          {
            case PageIndex.FormGuider:
              this.currForm = (Form) new FormGuider(this.device);
              break;
            case PageIndex.PromptExitSynapse:
              this.currForm = (Form) new PromptExitSynapse();
              break;
            case PageIndex.dev1:
              this.currForm = (Form) new FormFWUStep1(this.device, 1);
              break;
            case PageIndex.dev2:
              this.currForm = (Form) new FormFWUStep1(this.device, 2);
              break;
            case PageIndex.dev3:
              this.currForm = (Form) new FormFWUStep1(this.device, 3);
              break;
            case PageIndex.FormUpdateEnd:
              this.currForm = (Form) new FormUpdateEnd();
              break;
          }
        }
        if (this.currForm != null && !this.currForm.IsDisposed)
        {
          this.currForm.StartPosition = FormStartPosition.Manual;
          this.currForm.Closed += new EventHandler(this.OnFormClosed);
          this.currForm.Location = location;
          this.currForm.Show();
          return;
        }
        this.ExitThread();
      }
      this.ExitThread();
    }
  }
}
