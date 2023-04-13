using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WirelessSetFWU
{
  public class DeviceInterface
  {
    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr OpenDevice(
      uint VID,
      uint PID,
      ushort bcdpid,
      float blver,
      int reporttype,
      int featurelen,
      int inputlen,
      int outputlen);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr GetBootloaderHandle(
      uint VID,
      uint PID,
      ushort bcdpid,
      float blver);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void CloseDevice(IntPtr handle);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort GetDevPIDInBootloader(IntPtr handle, float blver);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetFWVersion(IntPtr handle, byte[] fwver, byte devtype);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int EnterDeviceMode(IntPtr handle, byte mode);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void delay(float duration);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DFUErase(IntPtr handle, uint startaddr, uint endaddr);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DFUProgram(
      IntPtr handle,
      byte datasize,
      uint startaddr,
      int delaytime,
      byte[] data);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DFUVerify(
      IntPtr handle,
      byte datasize,
      uint startaddr,
      int delaytime,
      byte[] data);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int DFUExit(IntPtr handle);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool BackToDefult(IntPtr handle);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SendCmd(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      byte pktmsb,
      byte pktlsb,
      byte paramlen,
      int getrepretry,
      int delay,
      byte[] param,
      byte[] retdata);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool SetFeatureRpt(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      byte pktmsb,
      byte pktlsb,
      byte paramlen,
      int setrepretry,
      int delay,
      byte[] param);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetFeatureRpt(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      int getrepretry,
      int delay,
      byte[] param);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool ControlIn(
      IntPtr handle,
      byte bcommand,
      ushort uvalue,
      uint uindex,
      ushort ulength,
      byte[] rbBuffer);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool ControlOut(
      IntPtr handle,
      byte bcommand,
      ushort uvalue,
      uint uindex,
      ushort ulength,
      byte[] rbBuffer);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr GetPS4FWVerion(IntPtr handle, int featurelen, int protocolver);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void EnterPS4Bootloader(IntPtr handle, int featurelen, int protocolver);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetBLFWVERInBootloader(IntPtr handle, float blver, byte[] fwver);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool RebootSystem();

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool IsWindows10OrGreater();

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool IsWindows8BLUEOrGreater();

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void InstallBLDriver(string path);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int SetActiveProfile(IntPtr handle, byte proile);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool OutData(IntPtr handle, byte outpipe, byte[] buffer, ulong len);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool ReadData(IntPtr handle, byte inpipe, byte[] buffer, int len);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool CheckPipeID(IntPtr handle, ref byte inpipe, ref byte outpipe);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void HaveBT(ref bool havebt, ref bool bton, ref bool supportble);

    [DllImport("FWUpdaterDLL.dll", EntryPoint = "GetEditionID", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool GetEdition(IntPtr handle, ref byte edition);

    [DllImport("FWUpdaterDLL.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
    internal static extern bool DoStopSvc(string servicename);

    [DllImport("HID.dll", CharSet = CharSet.Auto)]
    internal static extern bool HidD_GetFeature(
      IntPtr hndRef,
      [Out] byte[] ReportBuffer,
      int numberOfBytesToRead);

    [DllImport("HID.dll", CharSet = CharSet.Auto)]
    internal static extern bool HidD_SetFeature(
      IntPtr hndRef,
      byte[] ReportBuffer,
      int nNumberOfBytesToWrite);

    public bool HidSetFeature(IntPtr hndRef, byte[] ReportBuffer, int nNumberOfBytesToWrite) => DeviceInterface.HidD_SetFeature(hndRef, ReportBuffer, nNumberOfBytesToWrite);

    public bool HidGetFeature(IntPtr hndRef, [Out] byte[] ReportBuffer, int numberOfBytesToRead) => DeviceInterface.HidD_GetFeature(hndRef, ReportBuffer, numberOfBytesToRead);

    public void haveBT(ref bool havebt, ref bool bton, ref bool supportble) => DeviceInterface.HaveBT(ref havebt, ref bton, ref supportble);

    public bool Is64Bit()
    {
      bool lpSystemInfo;
      DeviceInterface.IsWow64Process(Process.GetCurrentProcess().Handle, out lpSystemInfo);
      return lpSystemInfo;
    }

    public bool IsWin10orGreater() => DeviceInterface.IsWindows10OrGreater();

    public bool IsWin8P1orGreater() => DeviceInterface.IsWindows8BLUEOrGreater();

    public void InstallBLDrv(string path) => DeviceInterface.InstallBLDriver(path);

    public void Delay(float duration) => DeviceInterface.delay(duration);

    public IntPtr OpenDev(
      uint vid,
      uint pid,
      ushort bcdpid,
      float blver,
      int reporttype,
      int featurelen,
      int inputlen,
      int outputlen)
    {
      return DeviceInterface.OpenDevice(vid, pid, bcdpid, blver, reporttype, featurelen, inputlen, outputlen);
    }

    public IntPtr OpenBootloader(uint vid, uint pid, ushort bcdpid, float blver) => DeviceInterface.GetBootloaderHandle(vid, pid, bcdpid, blver);

    public void CloseDev(IntPtr handle) => DeviceInterface.CloseDevice(handle);

    public string GetBCDPID(IntPtr handle, float blver) => string.Format("{0:X4}", (object) DeviceInterface.GetDevPIDInBootloader(handle, blver));

    public int EnterDevMode(IntPtr handle, byte mode)
    {
      if (Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex) != 7)
        return DeviceInterface.EnterDeviceMode(handle, mode);
      if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "1000" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "1100")
        DeviceInterface.EnterPS4Bootloader(handle, 48, 1);
      else
        DeviceInterface.EnterPS4Bootloader(handle, 48, 2);
      return 2;
    }

    public string GetDevFWVer(IntPtr handle, bool nxp)
    {
      if (Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex) == 7)
      {
        int protocolver = Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "1000" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "1100" ? 1 : 2;
        IntPtr ps4FwVerion = DeviceInterface.GetPS4FWVerion(handle, 48, protocolver);
        string stringAnsi = Marshal.PtrToStringAnsi(ps4FwVerion);
        Marshal.FreeHGlobal(ps4FwVerion);
        return stringAnsi;
      }
      byte devtype = Convert.ToByte(Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex));
      byte[] fwver = new byte[9];
      if (2 != DeviceInterface.GetFWVersion(handle, fwver, devtype))
        return "";
      return nxp ? string.Format("{0:D}.{1:D2}.{2:D2}", (object) fwver[0], (object) fwver[1], (object) fwver[2]) : string.Format("{0:D}.{1:D2}.{2:D2}", (object) fwver[4], (object) fwver[5], (object) fwver[6]);
    }

    public bool GetEID(IntPtr handle, ref byte edition) => DeviceInterface.GetEdition(handle, ref edition);

    public int EraseFW(IntPtr handle, uint startaddr, uint endaddr) => DeviceInterface.DFUErase(handle, startaddr, endaddr);

    public int ProgramFW(
      IntPtr handle,
      byte datasize,
      uint startaddr,
      int delaytime,
      byte[] data)
    {
      return DeviceInterface.DFUProgram(handle, datasize, startaddr, delaytime, data);
    }

    public int VerifyFW(IntPtr handle, byte datasize, uint startaddr, int delaytime, byte[] data) => DeviceInterface.DFUVerify(handle, datasize, startaddr, delaytime, data);

    public int ExitBL(IntPtr handle, float blver)
    {
      if ((double) blver == 3.0)
      {
        byte outpipe = 0;
        byte inpipe = 0;
        if (!this.GetPipeID(handle, ref inpipe, ref outpipe))
          return 0;
        byte[] buffer = new byte[1]{ (byte) 7 };
        return DeviceInterface.OutData(handle, outpipe, buffer, 1UL) ? 2 : 0;
      }
      if ((double) blver == 2.0)
        return DeviceInterface.DFUExit(handle);
      return this.WinUSB_ControlOut(handle, (byte) 132, (ushort) 0, 0U, (ushort) 0, (byte[]) null) ? 2 : 0;
    }

    public bool BacktoDefault(IntPtr handle) => DeviceInterface.BackToDefult(handle);

    public int SendCommand(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      byte pktmsb,
      byte pktlsb,
      byte paramlen,
      int getrepretry,
      int delay,
      byte[] param,
      byte[] retdata)
    {
      return DeviceInterface.SendCmd(handle, reportid, cmdcls, cmdid, pktmsb, pktlsb, paramlen, getrepretry, delay, param, retdata);
    }

    public bool WinUSB_ControlOut(
      IntPtr handle,
      byte bcommand,
      ushort uvalue,
      uint uindex,
      ushort ulength,
      byte[] rbBuffer)
    {
      return DeviceInterface.ControlOut(handle, bcommand, uvalue, uindex, ulength, rbBuffer);
    }

    public bool WinUSB_ControlIn(
      IntPtr handle,
      byte bcommand,
      ushort uvalue,
      uint uindex,
      ushort ulength,
      byte[] rbBuffer)
    {
      return DeviceInterface.ControlIn(handle, bcommand, uvalue, uindex, ulength, rbBuffer);
    }

    public string GetBLFWVer(IntPtr handle, float blver)
    {
      byte[] fwver = new byte[5];
      if (2 != DeviceInterface.GetBLFWVERInBootloader(handle, blver, fwver))
        return "";
      return string.Format("{0:D}.{1:D2}.{2:D2}.{3:D2}", (object) fwver[0], (object) fwver[1], (object) fwver[2], (object) fwver[3]);
    }

    public int ActiveProfile(IntPtr handle, byte activeprofile) => DeviceInterface.SetActiveProfile(handle, activeprofile);

    public bool RestartSystem() => DeviceInterface.RebootSystem();

    public bool GetPipeID(IntPtr handle, ref byte inpipe, ref byte outpipe) => DeviceInterface.CheckPipeID(handle, ref inpipe, ref outpipe);

    public bool ReadPipe(IntPtr handle, byte inpipe, byte[] buffer, int len) => DeviceInterface.ReadData(handle, inpipe, buffer, len);

    public bool WritePipe(IntPtr handle, byte outpipe, byte[] buffer, ulong len) => DeviceInterface.OutData(handle, outpipe, buffer, len);

    public bool SetFeatureReport(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      byte pktmsb,
      byte pktlsb,
      byte paramlen,
      int setretry,
      int delay,
      byte[] param)
    {
      return DeviceInterface.SetFeatureRpt(handle, reportid, cmdcls, cmdid, pktmsb, pktlsb, paramlen, setretry, delay, param);
    }

    public int GetFeatureReport(
      IntPtr handle,
      byte reportid,
      byte cmdcls,
      byte cmdid,
      int getretry,
      int delay,
      byte[] param)
    {
      return DeviceInterface.GetFeatureRpt(handle, reportid, cmdcls, cmdid, getretry, delay, param);
    }

    public bool StopSvc(string servicename) => DeviceInterface.DoStopSvc(servicename);
  }
}
