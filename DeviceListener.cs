using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WirelessSetFWU
{
  public class DeviceListener
  {
    private static Guid rawUsbGUID = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");
    public const int WM_DEVICECHANGE = 537;
    public const int DBT_DEVICEARRIVAL = 32768;
    public const int DBT_CONFIGCHANGECANCELED = 25;
    public const int DBT_CONFIGCHANGED = 24;
    public const int DBT_CUSTOMEVENT = 32774;
    public const int DBT_DEVICEQUERYREMOVE = 32769;
    public const int DBT_DEVICEQUERYREMOVEFAILED = 32770;
    public const int DBT_DEVICEREMOVECOMPLETE = 32772;
    public const int DBT_DEVICEREMOVEPENDING = 32771;
    public const int DBT_DEVICETYPESPECIFIC = 32773;
    public const int DBT_DEVNODES_CHANGED = 7;
    public const int DBT_QUERYCHANGECONFIG = 23;
    public const int DBT_USERDEFINED = 65535;
    public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
    public const int DBT_DEVTYP_HANDLE = 6;
    public const int BROADCAST_QUERY_DENY = 1112363332;
    private List<Common.VidPid> deviceList;
    private static bool fRunning = false;
    private IntPtr hCtrl = IntPtr.Zero;

    public event EventHandler<DeviceListenerEvent> RaiseDeviceEvent;

    public void DoDeviceEvent(Common.VidPid vidPid, bool fConnected) => this.OnRaiseDeviceEvent(new DeviceListenerEvent(vidPid, fConnected));

    private static unsafe string DBHToString(DeviceListener.DEV_BROADCAST_HANDLE* pDBH) => (pDBH->dbch_name0.ToString() + pDBH->dbch_name1.ToString() + pDBH->dbch_name2.ToString() + pDBH->dbch_name3.ToString() + pDBH->dbch_name4.ToString() + pDBH->dbch_name5.ToString() + pDBH->dbch_name6.ToString() + pDBH->dbch_name7.ToString() + pDBH->dbch_name8.ToString() + pDBH->dbch_name9.ToString() + pDBH->dbch_name10.ToString() + pDBH->dbch_name11.ToString() + pDBH->dbch_name12.ToString() + pDBH->dbch_name13.ToString() + pDBH->dbch_name14.ToString() + pDBH->dbch_name15.ToString() + pDBH->dbch_name16.ToString() + pDBH->dbch_name17.ToString() + pDBH->dbch_name18.ToString() + pDBH->dbch_name19.ToString() + pDBH->dbch_name20.ToString() + pDBH->dbch_name21.ToString() + pDBH->dbch_name22.ToString() + pDBH->dbch_name23.ToString() + pDBH->dbch_name24.ToString() + pDBH->dbch_name25.ToString() + pDBH->dbch_name26.ToString() + pDBH->dbch_name27.ToString() + pDBH->dbch_name28.ToString() + pDBH->dbch_name29.ToString() + pDBH->dbch_name30.ToString() + pDBH->dbch_name31.ToString() + pDBH->dbch_name32.ToString() + pDBH->dbch_name33.ToString() + pDBH->dbch_name34.ToString() + pDBH->dbch_name35.ToString() + pDBH->dbch_name36.ToString() + pDBH->dbch_name37.ToString() + pDBH->dbch_name38.ToString() + pDBH->dbch_name39.ToString()).ToUpper();

    public DeviceListener() => this.deviceList = new List<Common.VidPid>();

    ~DeviceListener()
    {
    }

    public void AddDevice(Common.VidPid vidPid) => this.deviceList.Add(vidPid);

    public void RemoveAll() => this.deviceList.Clear();

    public void Start(IntPtr handle)
    {
      Logger.getInstance().writeLog("DeviceListener:Start: RegisterForDeviceChange()", (short) 1);
      DeviceListener.DEV_BROADCAST_HANDLE structure = new DeviceListener.DEV_BROADCAST_HANDLE();
      structure.dbcc_devicetype = 5;
      structure.dbcc_classguid = DeviceListener.rawUsbGUID;
      int cb = Marshal.SizeOf(typeof (DeviceListener.DEV_BROADCAST_HANDLE));
      structure.dbcc_size = cb;
      IntPtr num = Marshal.AllocHGlobal(cb);
      Marshal.StructureToPtr<DeviceListener.DEV_BROADCAST_HANDLE>(structure, num, true);
      if (DeviceListener.Native.RegisterDeviceNotification(handle, num, 0U).Equals((object) null))
        return;
      DeviceListener.fRunning = true;
    }

    public void Stop()
    {
      if (DeviceListener.fRunning)
      {
        int num = (int) DeviceListener.Native.UnregisterDeviceNotification(this.hCtrl);
      }
      DeviceListener.fRunning = false;
    }

    public unsafe void Process(ref Message msg)
    {
      if (!DeviceListener.fRunning || 537 != msg.Msg)
        return;
      int int32 = msg.WParam.ToInt32();
      if (32768 != int32 && 32772 != int32)
        return;
      DeviceListener.DEV_BROADCAST_HANDLE* pointer = (DeviceListener.DEV_BROADCAST_HANDLE*) msg.LParam.ToPointer();
      if (IntPtr.Zero == (IntPtr) pointer)
        return;
      for (int index = 0; index < this.deviceList.Count; ++index)
      {
        Common.VidPid device = this.deviceList[index];
        string str = DeviceListener.DBHToString(pointer);
        Logger.getInstance().writeLog(string.Format("DeviceListener:: VID: {0}, PID: {1}", (object) device.GetVID(), (object) device.GetPID()), (short) 1);
        if (-1 != str.IndexOf(device.GetVID()) && -1 != str.IndexOf(device.GetPID()))
        {
          this.DoDeviceEvent(device, 32768 == int32);
          break;
        }
      }
    }

    private void OnRaiseDeviceEvent(DeviceListenerEvent evt)
    {
      EventHandler<DeviceListenerEvent> raiseDeviceEvent = this.RaiseDeviceEvent;
      if (raiseDeviceEvent == null)
        return;
      raiseDeviceEvent((object) this, evt);
    }

    public struct DEV_BROADCAST_HANDLE
    {
      public int dbcc_size;
      public int dbcc_devicetype;
      public int dbcc_reserved;
      public Guid dbcc_classguid;
      public char dbch_name0;
      public char dbch_name1;
      public char dbch_name2;
      public char dbch_name3;
      public char dbch_name4;
      public char dbch_name5;
      public char dbch_name6;
      public char dbch_name7;
      public char dbch_name8;
      public char dbch_name9;
      public char dbch_name10;
      public char dbch_name11;
      public char dbch_name12;
      public char dbch_name13;
      public char dbch_name14;
      public char dbch_name15;
      public char dbch_name16;
      public char dbch_name17;
      public char dbch_name18;
      public char dbch_name19;
      public char dbch_name20;
      public char dbch_name21;
      public char dbch_name22;
      public char dbch_name23;
      public char dbch_name24;
      public char dbch_name25;
      public char dbch_name26;
      public char dbch_name27;
      public char dbch_name28;
      public char dbch_name29;
      public char dbch_name30;
      public char dbch_name31;
      public char dbch_name32;
      public char dbch_name33;
      public char dbch_name34;
      public char dbch_name35;
      public char dbch_name36;
      public char dbch_name37;
      public char dbch_name38;
      public char dbch_name39;
      public char dbch_name40;
      public char dbch_name41;
      public char dbch_name42;
      public char dbch_name43;
      public char dbch_name44;
      public char dbch_name45;
      public char dbch_name46;
      public char dbch_name47;
      public char dbch_name48;
      public char dbch_name49;
      public char dbch_name50;
    }

    public struct DEV_BROADCAST_HDR
    {
      public int dbch_size;
      public int dbch_devicetype;
      public int dbch_reserved;
    }

    private class Native
    {
      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr RegisterDeviceNotification(
        IntPtr hRecipient,
        IntPtr NotificationFilter,
        uint Flags);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern uint UnregisterDeviceNotification(IntPtr hHandle);
    }
  }
}
