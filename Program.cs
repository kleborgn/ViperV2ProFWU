using System;
using System.Threading;
using System.Windows.Forms;

namespace WirelessSetFWU
{
  internal static class Program
  {
    private static Mutex deviceMutex;

    [STAThread]
    private static void Main()
    {
      Logger.getInstance().writeLog("Enter Main...", (short) 1);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      try
      {
        Common.updateInfo = new UpdateInfo();
        Logger.getInstance().writeLog("update info object open success", (short) 1);
        if (!Program.CheckFirstInstance())
        {
          DeviceInterface deviceInterface = new DeviceInterface();
          int num = (int) MessageBox.Show("Another instance is already running.", string.Format("Razer {0} Device Updater", (object) Common.updateInfo.GetProductName(Common.updateInfo.CurDevIndex)));
          return;
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ex.Message);
      }
      Application.Run((ApplicationContext) new appContextDevice());
    }

    private static bool CheckFirstInstance()
    {
      Logger.getInstance().writeLog("CheckFirstInstance start...", (short) 1);
      DeviceInterface deviceInterface = new DeviceInterface();
      Logger.getInstance().writeLog("Common.updateInfo.SupportDevCount is " + (object) Common.updateInfo.SupportDevCount, (short) 1);
      Common.updateInfo.CurDevIndex = 1;
      string productName = Common.updateInfo.GetProductName(Common.updateInfo.CurDevIndex);
      Logger.getInstance().writeLog("deviceName is " + productName, (short) 1);
      bool createdNew;
      Program.deviceMutex = new Mutex(true, string.Format("Razer{0}DeviceUpdater", (object) productName), out createdNew);
      return createdNew;
    }
  }
}
