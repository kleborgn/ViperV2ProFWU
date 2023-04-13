using CustomerFirmwareUpdater;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WirelessSetFWU.Resources;

namespace WirelessSetFWU
{
  public class FormFWUStep1 : Form
  {
    private DeviceInterface device;
    private ushort m_uPageSize = Common.updateInfo.PageSize;
    private ushort m_uDataPacketSize = Common.updateInfo.DataPacketSize;
    private string m_strFormattedData = "";
    private uint m_uAddressBegin;
    private uint m_uAddressEnd;
    private bool m_fAddressBeginSet;
    private string m_LastError = string.Empty;
    private const byte COMMAND_WRITE = 129;
    private const byte COMMAND_ERASE = 130;
    private const byte COMMAND_CHECKSUM = 131;
    private const byte COMMAND_EXITBL = 132;
    private const byte COMMAND_VERIFY = 135;
    private const byte COMMAND_STATUS = 143;
    private const byte COMMAND_BOOTLOADER_DOWNLOAD = 128;
    private const int MAX_RETRY_GET = 50;
    private const int MAX_RETRY_SET = 50;
    private bool stopclosethread;
    private eState curstate;
    private Point lastPoint = Point.Empty;
    private bool devconnect;
    private bool blconnect;
    private bool isGenericDongle;
    private DeviceListener devlistener = new DeviceListener();
    public bool checkingretry;
    private bool devreconnect;
    private Common.VidPid deviceVidPid;
    private Common.VidPid BLVidPid;
    private bool firstconnectbl;
    private const int WINUSB_DATA_SIZE = 8;
    private const byte CONTROL_REQUEST = 131;
    private int devindex;
    private IContainer components;
    private Label labelHeader;
    private Label labelpluginDevice;
    private Label labelPromptMessage;
    private Label labelUpdateprogress;
    private Label labelUpdateInfor;
    private CustomProgressBar.CustomProgressBar progressBarupdate;
    private MyButton buttonUpdate;
    private MyButton buttonCancel;
    private Label labeltargetver;
    private Label labelCurFWver;
    private BackgroundWorker backgroundWorkerProcessFWData;
    private BackgroundWorker backgroundWorkerCheckVer;
    private BackgroundWorker backgroundWorkerEraseFlash;
    private BackgroundWorker backgroundWorkerProgramFW;
    private BackgroundWorker backgroundWorkerVerify;
    private System.Windows.Forms.Timer timerbllistener;
    private System.Windows.Forms.Timer timerblentersuccess;
    private BackgroundWorker backgroundWorkerCloseRestartDialog;
    private BackgroundWorker backgroundWorkercheckbattery;
    private BackgroundWorker backgroundWorkerCheckFlashFWVer;
    private BackgroundWorker backgroundWorkerNordicENTERBL;
    private BackgroundWorker backgroundWorkerGetRegionInfor;
    private BackgroundWorker backgroundWorkerNordicProgram;
    private BackgroundWorker backgroundWorkerVerifyNordicFW;
    private Label labelupdatestatus;
    private Label labeldevstatus1;
    private Label labeldevstatus2;
    private Label labelpoweroff;

    public FormFWUStep1(DeviceInterface device, int deviceindex)
    {
      this.InitializeComponent();
      this.device = device;
      this.devindex = deviceindex;
    }

    private void HandleDeviceListenerEvent(object sender, DeviceListenerEvent e)
    {
      if (this.deviceVidPid == e.GetVidPid())
      {
        if (!e.IsConnected())
        {
          this.devconnect = false;
          if (this.curstate == eState.STATE_EXIT_NordicBL || this.curstate == eState.STATE_ENTER_BL || this.curstate == eState.STATE_FAILED)
            return;
          this.progressBarupdate.Visible = false;
          this.labelCurFWver.Visible = false;
          this.labelPromptMessage.Visible = false;
          this.labeldevstatus1.Visible = false;
          this.labeldevstatus2.Visible = false;
          this.labeltargetver.Visible = false;
          this.labelUpdateInfor.Visible = false;
          this.labelUpdateprogress.Visible = false;
          this.labelupdatestatus.Visible = false;
          this.labelpluginDevice.ForeColor = Common.greendarktheme;
          if (this.curstate == eState.STATE_FLASHFW_AFTER || this.curstate == eState.STATE_EXIT_BL)
          {
            this.buttonUpdate.Enabled = true;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
          }
          else
          {
            this.buttonCancel.Enabled = true;
            this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.Text = ResourceStr.update;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
          }
        }
        else
        {
          if (this.checkingretry)
            this.devreconnect = true;
          this.devconnect = true;
          if (this.curstate == eState.STATE_EXIT_NordicBL)
            this.curstate = eState.STATE_FLASHFW_AFTER;
          else
            this.labelpluginDevice.ForeColor = Color.White;
          if (this.checkingretry || this.backgroundWorkerCheckVer.IsBusy)
            return;
          this.backgroundWorkerCheckVer.RunWorkerAsync();
        }
      }
      else
      {
        if (!(this.BLVidPid == e.GetVidPid()))
          return;
        if (!e.IsConnected())
        {
          this.blconnect = false;
          this.stopclosethread = true;
          if (this.curstate == eState.STATE_NULL)
          {
            this.labelupdatestatus.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelCurFWver.Visible = false;
            this.labelPromptMessage.Visible = false;
            this.labeltargetver.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            this.buttonUpdate.Enabled = false;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
          }
          else
          {
            if (this.curstate != eState.STATE_EXIT_BL || Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
              return;
            this.progressBarupdate.Visible = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
          }
        }
        else
        {
          Thread.Sleep(1000);
          IntPtr zero = IntPtr.Zero;
          IntPtr handle = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : this.CheckGenericDongle(this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
          if (!(handle != IntPtr.Zero))
            return;
          this.device.CloseDev(handle);
          zero = IntPtr.Zero;
          this.blconnect = true;
          if (this.curstate == eState.STATE_NULL)
            this.firstconnectbl = true;
          Common.DevFWNeedUpdate = true;
          if (this.checkingretry)
            this.devreconnect = true;
          if (this.checkingretry)
            return;
          if (this.curstate == eState.STATE_ENTER_BL)
          {
            this.timerblentersuccess.Stop();
            this.timerblentersuccess.Enabled = false;
            this.timerbllistener.Stop();
            this.timerbllistener.Enabled = false;
            if (!this.backgroundWorkerCloseRestartDialog.IsBusy)
              this.backgroundWorkerCloseRestartDialog.RunWorkerAsync();
            if (this.backgroundWorkerProcessFWData.IsBusy)
              return;
            this.backgroundWorkerProcessFWData.RunWorkerAsync();
          }
          else
          {
            this.labelupdatestatus.Visible = false;
            this.labelPromptMessage.Visible = true;
            this.labelPromptMessage.Text = ResourceStr.Nounplug;
            this.labelpluginDevice.ForeColor = Color.White;
            if (this.curstate == eState.STATE_NULL)
            {
              this.labelCurFWver.Text = ResourceStr.devicever;
              this.labeltargetver.Text = ResourceStr.newver + Common.updateInfo.GetDevFWVer(Common.updateInfo.CurDevIndex);
            }
            this.labelCurFWver.ForeColor = Common.lightgray;
            this.labeltargetver.ForeColor = Common.greendarktheme;
            this.labeltargetver.Visible = true;
            this.labelCurFWver.Visible = true;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_click;
            this.buttonUpdate.Enabled = false;
            this.buttonCancel.Enabled = false;
            this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_disabled;
            this.labelUpdateInfor.Visible = true;
            this.labelUpdateprogress.Visible = true;
            this.progressBarupdate.Visible = true;
            if (this.backgroundWorkerProcessFWData.IsBusy)
              return;
            this.backgroundWorkerProcessFWData.RunWorkerAsync();
          }
        }
      }
    }

    protected override void WndProc(ref Message m)
    {
      this.devlistener.Process(ref m);
      base.WndProc(ref m);
    }

    private void FormFWUStep1_Load(object sender, EventArgs e)
    {
      this.DoubleBuffered = true;
      this.SetStyle(ControlStyles.UserPaint, true);
      this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      this.SetStyle(ControlStyles.DoubleBuffer, true);
      Common.updateInfo.CurDevIndex = this.devindex;
      try
      {
        File.Delete(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin");
      }
      catch (Exception ex)
      {
      }
      if (Common.updateInfo.GetPID(1) == "0904")
        this.buttonCancel.Text = ResourceStr.skip;
      else
        this.buttonCancel.Text = ResourceStr.cancel;
      this.buttonUpdate.Text = ResourceStr.update;
      this.devlistener.RaiseDeviceEvent += new EventHandler<DeviceListenerEvent>(this.HandleDeviceListenerEvent);
      this.buttonCancel.Enabled = true;
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
      this.labelCurFWver.Text = ResourceStr.devicever;
      this.labeltargetver.Text = ResourceStr.newver;
      this.deviceVidPid = new Common.VidPid(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), false);
      this.BLVidPid = new Common.VidPid(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), false);
      this.devlistener.AddDevice(this.deviceVidPid);
      this.devlistener.AddDevice(this.BLVidPid);
      this.devlistener.Start(this.Handle);
      IntPtr zero1 = IntPtr.Zero;
      IntPtr handle1 = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : this.CheckGenericDongle(this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
      if (handle1 != IntPtr.Zero)
      {
        this.blconnect = true;
        this.firstconnectbl = true;
        this.device.GetBLFWVer(handle1, Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex));
        this.device.CloseDev(handle1);
        zero1 = IntPtr.Zero;
      }
      else
      {
        IntPtr handle2 = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
        if (handle2 != IntPtr.Zero)
        {
          this.devconnect = true;
          this.device.CloseDev(handle2);
          IntPtr zero2 = IntPtr.Zero;
        }
      }
      if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0068")
      {
        this.labelHeader.Text = string.Format(ResourceStr.Title.ToUpper(), (object) "MAMBA + FIREFLY HYPERFLUX");
        this.labelHeader.ForeColor = Common.greendarktheme;
        this.Text = this.labelHeader.Text;
      }
      else
      {
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
      }
      if (Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex) == 4)
      {
        if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007B" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0088")
          this.labelpluginDevice.Text = ResourceStr.conndongleviadock;
        else
          this.labelpluginDevice.Text = ResourceStr.plugdongle;
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.plugdongle;
      }
      else if (Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex) == 1)
      {
        this.labelpluginDevice.Text = ResourceStr.conntestmouse;
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.fwupdaterBackground;
      }
      else if (Common.updateInfo.GetDevType(Common.updateInfo.CurDevIndex) == 2)
        this.labelpluginDevice.Text = ResourceStr.conntestKB;
      bool flag = false;
      if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0904")
      {
        this.labelpluginDevice.Text = ResourceStr.plugdongle;
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.mika_fw;
      }
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0075")
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.mika_mouse_fw;
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "023E")
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.mika_kb_fw;
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007E")
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.fwupdaterBackground;
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "025A")
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.BWV3ProConn;
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0258")
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.BWV3miniConn;
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "008F")
      {
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.NagaProCFU;
        flag = true;
      }
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007C")
      {
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.DAV2ProCFU;
        flag = true;
      }
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007A")
      {
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.ViperUltCFU;
        flag = true;
      }
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "00A5")
      {
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.viperv2proTrunOff;
        flag = true;
      }
      else if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "00AA")
      {
        this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.basiliskv3proTurnOff;
        flag = true;
      }
      if (flag)
      {
        this.labelpoweroff.Visible = true;
        this.labelpoweroff.Text = this.labelpluginDevice.Text;
        this.labelpoweroff.ForeColor = Color.White;
        this.labelpluginDevice.Text = ResourceStr.switchmsoff;
        this.labelPromptMessage.Location = new Point(this.labelPromptMessage.Location.X, this.labelPromptMessage.Location.Y + 70);
        Label labeldevstatus1 = this.labeldevstatus1;
        Point location = this.labeldevstatus1.Location;
        int x1 = location.X;
        location = this.labeldevstatus1.Location;
        int y1 = location.Y + 70;
        Point point1 = new Point(x1, y1);
        labeldevstatus1.Location = point1;
        Label labeldevstatus2 = this.labeldevstatus2;
        location = this.labeldevstatus2.Location;
        int x2 = location.X;
        location = this.labeldevstatus2.Location;
        int y2 = location.Y + 70;
        Point point2 = new Point(x2, y2);
        labeldevstatus2.Location = point2;
        Label labelUpdateInfor = this.labelUpdateInfor;
        location = this.labelUpdateInfor.Location;
        int x3 = location.X;
        location = this.labelUpdateInfor.Location;
        int y3 = location.Y + 70;
        Point point3 = new Point(x3, y3);
        labelUpdateInfor.Location = point3;
        Label labelUpdateprogress = this.labelUpdateprogress;
        location = this.labelUpdateprogress.Location;
        int x4 = location.X;
        location = this.labelUpdateprogress.Location;
        int y4 = location.Y + 70;
        Point point4 = new Point(x4, y4);
        labelUpdateprogress.Location = point4;
        Label labelupdatestatus = this.labelupdatestatus;
        location = this.labelupdatestatus.Location;
        int x5 = location.X;
        location = this.labelupdatestatus.Location;
        int y5 = location.Y + 70;
        Point point5 = new Point(x5, y5);
        labelupdatestatus.Location = point5;
        CustomProgressBar.CustomProgressBar progressBarupdate = this.progressBarupdate;
        location = this.progressBarupdate.Location;
        int x6 = location.X;
        location = this.progressBarupdate.Location;
        int y6 = location.Y + 70;
        Point point6 = new Point(x6, y6);
        progressBarupdate.Location = point6;
      }
      if (!this.blconnect && !this.devconnect)
      {
        this.labelpluginDevice.ForeColor = Common.greendarktheme;
      }
      else
      {
        this.labelpluginDevice.ForeColor = Color.White;
        if (this.blconnect)
        {
          Common.DevFWNeedUpdate = true;
          this.labelCurFWver.Text = ResourceStr.devicever;
          this.labelCurFWver.Visible = true;
          this.labeltargetver.Visible = true;
          this.labeltargetver.Text = ResourceStr.newver + Common.updateInfo.GetDevFWVer(Common.updateInfo.CurDevIndex);
          this.labelCurFWver.ForeColor = Common.lightgray;
          this.labeltargetver.ForeColor = Common.greendarktheme;
          this.labelPromptMessage.Visible = false;
          this.labelUpdateInfor.Visible = true;
          this.labelUpdateprogress.Visible = false;
          this.progressBarupdate.Visible = false;
          this.buttonUpdate.Enabled = true;
          this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
          this.labelupdatestatus.ForeColor = Common.greendarktheme;
          this.labelupdatestatus.Visible = true;
          this.labelupdatestatus.Text = ResourceStr.anupdaterequired;
        }
        else
        {
          if (!this.devconnect)
            return;
          this.backgroundWorkerCheckVer.RunWorkerAsync();
        }
      }
    }

    private IntPtr CheckGenericDongle(IntPtr blhandle)
    {
      if (blhandle == IntPtr.Zero)
      {
        blhandle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16("00FD", 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0);
        if (blhandle != IntPtr.Zero)
          this.isGenericDongle = true;
      }
      return blhandle;
    }

    private static string Mid(string strSource, int iStart, int iLength)
    {
      int startIndex = iStart > strSource.Length ? strSource.Length : iStart;
      return strSource.Substring(startIndex, startIndex + iLength > strSource.Length ? strSource.Length - startIndex : iLength);
    }

    private static void DataDefault(byte[] data)
    {
      for (int index = 0; index < data.Length; ++index)
        data[index] = byte.MaxValue;
    }

    private void backgroundWorkerProcessFWData_DoWork(object sender, DoWorkEventArgs e)
    {
      int devFwType = Common.updateInfo.GetDevFWType(Common.updateInfo.CurDevIndex);
      double blVer = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex);
      this.m_fAddressBeginSet = false;
      uint devFwLineNum = Common.updateInfo.GetDevFWLineNum(Common.updateInfo.CurDevIndex);
      this.curstate = eState.STATE_PROCESSING_DATA;
      this.backgroundWorkerProcessFWData.ReportProgress(0, (object) eState.STATE_PROCESSING_DATA);
      double num1 = 1.0 / (double) devFwLineNum;
      if (blVer != 1.0)
      {
        if (devFwType != 2)
          return;
        FileStream output = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Create);
        BinaryWriter binaryWriter = new BinaryWriter((Stream) output);
        byte[] numArray = new byte[512];
        FormFWUStep1.DataDefault(numArray);
        uint num2 = 0;
        ushort num3 = 0;
        int count = 0;
        long linenum = 0;
        while (linenum < (long) devFwLineNum)
        {
          string devFwLine = Common.updateInfo.GetDevFWLine(Common.updateInfo.CurDevIndex, (int) linenum);
          this.backgroundWorkerProcessFWData.ReportProgress((int) (linenum++ * 100L / (long) devFwLineNum));
          if (devFwLine.ElementAt<char>(0) != ':')
          {
            int num4 = (int) MessageBox.Show("Hex File was corrupt!");
            this.backgroundWorkerProcessFWData.ReportProgress(0, (object) eState.STATE_PROCESSINGDATA_FAIL);
            e.Result = (object) eState.STATE_PROCESSINGDATA_FAIL;
            return;
          }
          ushort uint16 = Convert.ToUInt16(devFwLine.Substring(3, 4), 16);
          switch (Convert.ToInt32(devFwLine.Substring(7, 2), 16))
          {
            case 0:
              uint num5 = (uint) ((int) num3 * 64 * 1024) + (uint) uint16;
              if (!this.m_fAddressBeginSet)
              {
                this.m_fAddressBeginSet = true;
                Common.StartAddr = num5;
              }
              if (num2 != 0U)
              {
                if (num5 - num2 > 0U)
                {
                  if ((long) count + (long) num5 - (long) num2 >= 512L)
                  {
                    binaryWriter.Write(numArray);
                    FormFWUStep1.DataDefault(numArray);
                    uint num6 = (uint) ((ulong) (num2 + 512U) - (ulong) count);
                    int num7 = (int) num5 - (int) num6;
                    for (int index = 0; index < num7 / 512; ++index)
                      binaryWriter.Write(numArray);
                    count = num7 % 512;
                  }
                  else
                    count = (int) ((long) count + (long) num5 - (long) num2);
                }
              }
              int int32 = Convert.ToInt32(devFwLine.Substring(1, 2), 16);
              for (int index1 = 0; index1 < int32; ++index1)
              {
                if (count == 512)
                {
                  binaryWriter.Write(numArray);
                  FormFWUStep1.DataDefault(numArray);
                  int index2 = 0;
                  numArray[index2] = Convert.ToByte(devFwLine.Substring(9 + index1 * 2, 2), 16);
                  count = index2 + 1;
                  ++num5;
                }
                else
                {
                  ++num5;
                  numArray[count] = Convert.ToByte(devFwLine.Substring(9 + index1 * 2, 2), 16);
                  ++count;
                }
              }
              num2 = num5;
              Common.EndAddr = num5;
              continue;
            case 4:
              string str = devFwLine.Substring(1, 2);
              num3 = Convert.ToUInt16(devFwLine.Substring(9, Convert.ToInt32(str, 16) * 2), 16);
              continue;
            default:
              continue;
          }
        }
        if (count > 0)
        {
          binaryWriter.Write(numArray, 0, count);
          FormFWUStep1.DataDefault(numArray);
        }
        Common.filelen = output.Length;
        this.backgroundWorkerProcessFWData.ReportProgress(100);
        binaryWriter.Close();
        output.Close();
        e.Result = (object) eState.STATE_PROCESSING_DATA_PASS;
      }
      else if (devFwLineNum == 0U)
      {
        e.Result = (object) eState.STATE_PROCESSINGDATA_FAIL;
      }
      else
      {
        uint num8 = 0;
        uint num9 = 0;
        string str1 = "";
        this.m_strFormattedData = "";
        this.m_uAddressBegin = 0U;
        this.m_uAddressEnd = 0U;
        try
        {
          for (int linenum = 0; (long) linenum < (long) devFwLineNum; ++linenum)
          {
            this.backgroundWorkerProcessFWData.ReportProgress((int) ((long) ((linenum + 1) * 100) / (long) devFwLineNum));
            str1 = "";
            string devFwLine = Common.updateInfo.GetDevFWLine(Common.updateInfo.CurDevIndex, linenum);
            uint num10 = 0;
            string strSource = "";
            int num11 = 0;
            if ('\n' == devFwLine[devFwLine.Length - 1])
              ++num11;
            switch (devFwType)
            {
              case 1:
                if ('S' == devFwLine[0])
                {
                  if ('1' == devFwLine[1])
                  {
                    num8 = Convert.ToUInt32(FormFWUStep1.Mid(devFwLine, 4, 2), 16);
                    num9 = Convert.ToUInt32(FormFWUStep1.Mid(devFwLine, 6, 2), 16);
                    strSource = FormFWUStep1.Mid(devFwLine, 8, devFwLine.Length - (10 + num11));
                    goto default;
                  }
                  else
                    break;
                }
                else
                {
                  int num12 = (int) MessageBox.Show("S19 FW corrupt!");
                  goto default;
                }
              case 2:
                if (devFwLine.ElementAt<char>(0) != ':')
                {
                  int num13 = (int) MessageBox.Show("Hex File was corrupt!");
                  return;
                }
                if ('0' == devFwLine[7] && '0' == devFwLine[8])
                {
                  num8 = Convert.ToUInt32(FormFWUStep1.Mid(devFwLine, 3, 2), 16);
                  num9 = Convert.ToUInt32(FormFWUStep1.Mid(devFwLine, 5, 2), 16);
                  strSource = FormFWUStep1.Mid(devFwLine, 9, devFwLine.Length - (11 + num11));
                  goto default;
                }
                else
                  break;
              default:
                uint num14 = num8 * 256U + num9;
                uint num15 = num14;
                if (!this.m_fAddressBeginSet)
                {
                  this.m_uAddressBegin = num14;
                  this.m_fAddressBeginSet = true;
                }
                uint num16 = 0;
                string str2 = "";
                while (strSource.Length > 0)
                {
                  str2 += FormFWUStep1.Mid(strSource, 0, 2);
                  strSource = FormFWUStep1.Mid(strSource, 2, strSource.Length);
                  ++num15;
                  ++num16;
                  if (num16 >= (uint) this.m_uDataPacketSize)
                  {
                    num8 = num14 / 256U;
                    num9 = num14 % 256U;
                    uint num17 = (num15 - 1U) / 256U;
                    uint num18 = (num15 - 1U) % 256U;
                    this.m_strFormattedData += string.Format("{0:X2}", (object) num16);
                    this.m_strFormattedData += string.Format("{0:X2}", (object) num8);
                    this.m_strFormattedData += string.Format("{0:X2}", (object) num9);
                    this.m_strFormattedData += string.Format("{0:X2}", (object) num17);
                    this.m_strFormattedData += string.Format("{0:X2}", (object) num18);
                    this.m_strFormattedData = this.m_strFormattedData.ToUpper();
                    this.m_strFormattedData = this.m_strFormattedData + str2 + " ";
                    num14 = num15;
                    this.m_uAddressEnd = num15 - 1U;
                    num16 = 0U;
                    str2 = "";
                  }
                }
                if (num16 > 0U)
                {
                  num8 = num14 / 256U;
                  num9 = num14 % 256U;
                  uint num19 = (num15 - 1U) / 256U;
                  uint num20 = (num15 - 1U) % 256U;
                  this.m_strFormattedData += string.Format("{0:X2}", (object) num16);
                  this.m_strFormattedData += string.Format("{0:X2}", (object) num8);
                  this.m_strFormattedData += string.Format("{0:X2}", (object) num9);
                  this.m_strFormattedData += string.Format("{0:X2}", (object) num19);
                  this.m_strFormattedData += string.Format("{0:X2}", (object) num20);
                  this.m_strFormattedData = this.m_strFormattedData.ToUpper();
                  this.m_strFormattedData = this.m_strFormattedData + str2 + " ";
                  num10 = num15;
                  this.m_uAddressEnd = num15 - 1U;
                  break;
                }
                break;
            }
          }
          e.Result = (object) eState.STATE_PROCESSING_DATA_PASS;
        }
        catch (Exception ex)
        {
          int num21 = (int) MessageBox.Show(ex.Message);
          e.Result = (object) eState.STATE_PROCESSINGDATA_FAIL;
        }
      }
    }

    private void backgroundWorkerProcessFWData_ProgressChanged(
      object sender,
      ProgressChangedEventArgs e)
    {
      this.labelUpdateInfor.ForeColor = Common.lightgray;
      this.labelUpdateprogress.ForeColor = Common.lightgray;
      this.labelPromptMessage.ForeColor = Common.greendarktheme;
      if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
      {
        this.progressBarupdate.Value = !Common.FlashFWNeedUpdate ? (int) ((double) e.ProgressPercentage * 0.05 * 2.5) : (int) ((double) e.ProgressPercentage * 0.05);
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
        this.labelUpdateInfor.Text = ResourceStr.updatefw;
        this.labelUpdateInfor.ForeColor = Common.lightgray;
        this.labelPromptMessage.Text = ResourceStr.Nounplug;
      }
      if (e.UserState == null || (eState) e.UserState != eState.STATE_PROCESSING_DATA)
        return;
      this.labelUpdateInfor.ForeColor = Common.lightgray;
      this.labelUpdateInfor.Text = ResourceStr.updatefw;
    }

    private void backgroundWorkerProcessFWData_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if ((eState) e.Result == eState.STATE_PROCESSING_DATA_PASS)
      {
        this.buttonCancel.Enabled = false;
        this.buttonUpdate.Enabled = false;
        this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_disabled;
        if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 3.0)
        {
          if (this.backgroundWorkerProgramFW.IsBusy)
            return;
          this.backgroundWorkerProgramFW.RunWorkerAsync();
        }
        else
        {
          if (this.backgroundWorkerEraseFlash.IsBusy)
            return;
          this.backgroundWorkerEraseFlash.RunWorkerAsync();
        }
      }
      else
      {
        if ((eState) e.Result != eState.STATE_PROCESSINGDATA_FAIL)
          return;
        int num = (int) MessageBox.Show("FW Data Corrupt!");
        this.buttonCancel.Enabled = true;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
      }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_clicked;
      if (this.buttonCancel.Text == ResourceStr.cancel)
      {
        Common.NextPage = PageIndex.Close;
      }
      else
      {
        if (!this.devconnect)
        {
          if (Common.updateInfo.CurDevIndex == 1)
            Common.DongleStatus = 0;
          else if (Common.updateInfo.CurDevIndex == 2)
            Common.Mousestatus = 0;
          else
            Common.KBstatus = 0;
        }
        else if (Common.updateInfo.DevSameVer >= 0 && Common.updateInfo.FlashSameVer >= 0)
        {
          if (Common.updateInfo.CurDevIndex == 1)
            Common.DongleStatus = 1;
          else if (Common.updateInfo.CurDevIndex == 2)
            Common.Mousestatus = 1;
          else
            Common.KBstatus = 1;
        }
        Common.NextPage = Common.updateInfo.CurDevIndex != 1 ? (Common.updateInfo.CurDevIndex != 2 ? PageIndex.FormUpdateEnd : PageIndex.dev3) : PageIndex.dev2;
      }
      this.Close();
    }

    private void buttonCancel_MouseEnter(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_hover;

    private void buttonCancel_MouseLeave(object sender, EventArgs e) => this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;

    private void labelpluginDevice_MouseDown(object sender, MouseEventArgs e)
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

    private void labelpluginDevice_MouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left)
        return;
      this.Left += e.X - this.lastPoint.X;
      this.Top += e.Y - this.lastPoint.Y;
    }

    private void backgroundWorkerCheckVer_DoWork(object sender, DoWorkEventArgs e)
    {
      int num = 0;
      IntPtr handle = IntPtr.Zero;
      while (this.devconnect)
      {
        this.device.Delay(1000f);
        ++num;
        handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
        if (handle == IntPtr.Zero)
        {
          if (num == 10)
          {
            e.Result = (object) eState.STATE_WAITING_DEVICE;
            return;
          }
        }
        else
          break;
      }
      if (!this.devconnect || handle == IntPtr.Zero)
      {
        e.Result = (object) eState.STATE_WAITING_DEVICE;
      }
      else
      {
        string strA1 = "";
        if (!(handle != IntPtr.Zero))
          return;
        for (int index = 0; (strA1 == "" || strA1 == "0.00.00" || strA1 == "0") && index < 5; ++index)
        {
          if (this.devconnect)
          {
            strA1 = this.device.GetDevFWVer(handle, true);
            if (strA1.Substring(0, 1) == "3" && Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "025A" || strA1.Substring(0, 1) == "2" && Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0258")
              ++Common.updateInfo.CurDevIndex;
            if (strA1 != "" && strA1 != "0.00.00" && strA1 != "0")
            {
              Common.updateInfo.ActDevFWVer = strA1;
              Common.updateInfo.DevSameVer = string.Compare(strA1, Common.updateInfo.GetDevFWVer(Common.updateInfo.CurDevIndex));
              if (this.firstconnectbl || this.devreconnect)
                Common.DevFWNeedUpdate = Common.updateInfo.DevSameVer < 0;
              else if (this.curstate != eState.STATE_EXIT_BL)
                Common.DevFWNeedUpdate = Common.updateInfo.DevSameVer < 0;
            }
            if ((Common.updateInfo.ActDevFWVer == "" || Common.updateInfo.ActDevFWVer == null) && index == 4)
              Common.DevFWNeedUpdate = true;
            if (this.devconnect)
            {
              if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
              {
                this.device.Delay(10f);
                strA1 = this.device.GetDevFWVer(handle, false);
                if (strA1 != "" && strA1 != "0.00.00" && strA1 != "0")
                {
                  if (Convert.ToByte(strA1.Substring(0, strA1.IndexOf("."))) > (byte) 10)
                  {
                    string strA2 = "";
                    Common.updateInfo.ActFlashFWVer = strA2;
                    Common.updateInfo.FlashSameVer = string.Compare(strA2, Common.updateInfo.GetFlashFWVer(Common.updateInfo.CurDevIndex));
                    Common.FlashFWNeedUpdate = true;
                    break;
                  }
                  Common.updateInfo.ActFlashFWVer = strA1;
                  Common.updateInfo.FlashSameVer = string.Compare(strA1, Common.updateInfo.GetFlashFWVer(Common.updateInfo.CurDevIndex));
                  Common.FlashFWNeedUpdate = Common.updateInfo.FlashSameVer < 0;
                  break;
                }
              }
              if ((Common.updateInfo.ActFlashFWVer == "" || Common.updateInfo.ActFlashFWVer == null) && index == 4)
                Common.FlashFWNeedUpdate = true;
              this.device.Delay(500f);
            }
            else
            {
              e.Result = (object) eState.STATE_WAITING_DEVICE;
              break;
            }
          }
          else
          {
            e.Result = (object) eState.STATE_WAITING_DEVICE;
            break;
          }
        }
        this.device.CloseDev(handle);
      }
    }

    private bool callWinusbChecksumControl(IntPtr handle, int mode, ref int checksumMCU)
    {
      this.curstate = eState.STATE_VERIFYING_FIRMWARE;
      byte[] rbBuffer = new byte[8];
      this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE);
      this.device.Delay(1000f);
      if (this.device.WinUSB_ControlOut(handle, (byte) 131, (ushort) mode, 0U, (ushort) 0, rbBuffer))
      {
        this.backgroundWorkerVerify.ReportProgress(50);
        this.device.Delay(100f);
        this.device.WinUSB_ControlIn(handle, (byte) 131, (ushort) 0, 0U, (ushort) 2, rbBuffer);
        checksumMCU = (int) rbBuffer[0] * 256 + (int) rbBuffer[1];
        this.backgroundWorkerVerify.ReportProgress(100);
        return true;
      }
      this.backgroundWorkerVerify.ReportProgress(50);
      return false;
    }

    private void backgroundWorkerCheckVer_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (e.Result != null)
      {
        if ((Common.updateInfo.CurDevIndex != 1 || Common.NextPage != PageIndex.dev1) && (Common.updateInfo.CurDevIndex != 2 || Common.NextPage != PageIndex.dev2) && (Common.updateInfo.CurDevIndex != 3 || Common.NextPage != PageIndex.dev3))
          return;
        this.devreconnect = false;
        this.checkingretry = true;
        if (MessageBox.Show(ResourceStr.checkverfail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          this.firstconnectbl = true;
          this.checkingretry = false;
          if (!this.devconnect || this.backgroundWorkerCheckVer.IsBusy)
            return;
          this.backgroundWorkerCheckVer.RunWorkerAsync();
        }
        else
        {
          this.labelUpdateInfor.Text = ResourceStr.checkverfail;
          this.labelUpdateInfor.ForeColor = Color.Red;
          this.buttonCancel.Enabled = true;
          this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        }
      }
      else
      {
        if (this.checkingretry)
          this.devreconnect = true;
        int devSameVer = Common.updateInfo.DevSameVer;
        this.labeltargetver.Text = ResourceStr.newver + Common.updateInfo.GetDevFWVer(Common.updateInfo.CurDevIndex);
        this.labeltargetver.Visible = true;
        this.labelCurFWver.ForeColor = Common.lightgray;
        if (devSameVer < 0)
        {
          if (this.curstate == eState.STATE_NULL)
            this.labelCurFWver.Text = ResourceStr.devicever + Common.updateInfo.ActDevFWVer;
          this.labelCurFWver.Visible = true;
          this.labeltargetver.ForeColor = Common.greendarktheme;
          this.labelUpdateInfor.Visible = false;
          this.labelUpdateprogress.Visible = false;
          this.progressBarupdate.Visible = false;
          this.labelupdatestatus.Visible = true;
          this.labeldevstatus1.Visible = false;
          this.labelupdatestatus.Text = ResourceStr.anupdaterequired;
          this.labelupdatestatus.ForeColor = Common.greendarktheme;
          this.buttonUpdate.Enabled = true;
          this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
        }
        else if (this.curstate == eState.STATE_EXIT_BL || this.curstate == eState.STATE_FLASHFW_AFTER)
        {
          if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex) && Common.FlashFWNeedUpdate)
          {
            this.labelCurFWver.Visible = true;
            this.labeltargetver.ForeColor = Common.greendarktheme;
            this.labelPromptMessage.Visible = true;
            this.labelUpdateInfor.Visible = true;
            this.labelUpdateprogress.Visible = true;
            this.progressBarupdate.Value = !Common.DevFWNeedUpdate ? 0 : 40;
            this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
            this.progressBarupdate.Visible = true;
            if (this.backgroundWorkerNordicENTERBL.IsBusy)
              return;
            this.backgroundWorkerNordicENTERBL.RunWorkerAsync();
          }
          else
          {
            this.labelCurFWver.Text = ResourceStr.devicever + Common.updateInfo.ActDevFWVer;
            this.labeltargetver.Text = ResourceStr.devicever + Common.updateInfo.ActDevFWVer;
            this.labeltargetver.ForeColor = Common.lightgray;
            if (Common.updateInfo.NeedBackToDefault(Common.updateInfo.CurDevIndex))
            {
              int num = 0;
              IntPtr handle;
              for (handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex)); num < 5 && (!(handle != IntPtr.Zero) || !this.device.BacktoDefault(handle)); ++num)
                Thread.Sleep(50);
              this.device.CloseDev(handle);
            }
            this.labelupdatestatus.Visible = true;
            this.labelupdatestatus.Text = ResourceStr.updatesuccessful;
            this.labelupdatestatus.ForeColor = Common.greendarktheme;
            if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0904" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0075" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "023E")
            {
              if (Common.updateInfo.CurDevIndex == 1)
              {
                this.labeldevstatus1.ForeColor = Color.White;
                this.labeldevstatus1.Visible = true;
                if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
                  this.labeldevstatus1.Text = string.Format(ResourceStr.checkturretmouse, (object) (" " + Common.updateInfo.GetDeviceName(2).ToUpper()));
                else
                  this.labeldevstatus1.Text = ResourceStr.checkturretmouse;
              }
              else if (Common.updateInfo.CurDevIndex == 2)
              {
                this.labeldevstatus1.ForeColor = Color.White;
                this.labeldevstatus1.Visible = true;
                if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
                  this.labeldevstatus1.Text = string.Format(ResourceStr.checkturretkb, (object) (" " + Common.updateInfo.GetDeviceName(2).ToUpper()));
                else
                  this.labeldevstatus1.Text = ResourceStr.checkturretkb;
              }
              else
              {
                this.labeldevstatus1.Visible = false;
                this.labeldevstatus1.Text = "";
              }
            }
            else
              this.labeldevstatus1.Visible = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelCurFWver.Visible = false;
            this.labeltargetver.Visible = true;
            this.buttonUpdate.Text = ResourceStr.next;
            this.buttonUpdate.Enabled = true;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
            if (Common.updateInfo.CurDevIndex == 1)
              Common.DongleStatus = 2;
            else if (Common.updateInfo.CurDevIndex == 2)
            {
              Common.Mousestatus = 2;
            }
            else
            {
              if (Common.updateInfo.CurDevIndex != 3)
                return;
              Common.KBstatus = 2;
            }
          }
        }
        else if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex) && Common.FlashFWNeedUpdate)
        {
          if (this.curstate == eState.STATE_NULL)
            this.labelCurFWver.Text = ResourceStr.devicever + Common.updateInfo.ActFlashFWVer;
          this.labelCurFWver.Visible = true;
          this.labeltargetver.ForeColor = Common.greendarktheme;
          this.labelPromptMessage.Visible = false;
          this.labelUpdateInfor.Visible = false;
          this.labelUpdateprogress.Visible = false;
          this.progressBarupdate.Visible = false;
          this.buttonUpdate.Enabled = true;
          this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
          this.labelupdatestatus.Visible = true;
          this.labelupdatestatus.ForeColor = Common.greendarktheme;
          this.labelupdatestatus.Text = ResourceStr.anupdaterequired;
        }
        else
        {
          this.labelCurFWver.Visible = false;
          this.labelPromptMessage.Visible = false;
          this.labelUpdateInfor.Visible = false;
          this.labelUpdateprogress.Visible = false;
          this.progressBarupdate.Visible = false;
          this.buttonCancel.Enabled = false;
          this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_disabled;
          this.labeltargetver.ForeColor = Common.lightgray;
          this.labeltargetver.Text = ResourceStr.devicever + Common.updateInfo.ActDevFWVer;
          this.labelupdatestatus.Visible = true;
          this.labelupdatestatus.Text = ResourceStr.docknoupdate;
          this.labelupdatestatus.ForeColor = Common.greendarktheme;
          this.labeldevstatus1.Visible = true;
          this.labeldevstatus1.Text = ResourceStr.havelatestfw;
          this.labeldevstatus1.ForeColor = Color.White;
          if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0904" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0075" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "023E")
          {
            if (Common.updateInfo.CurDevIndex == 1)
            {
              this.labeldevstatus2.ForeColor = Color.White;
              this.labeldevstatus2.Visible = true;
              this.labeldevstatus2.Text = ResourceStr.checkturretmouse;
              if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
                this.labeldevstatus2.Text = string.Format(ResourceStr.checkturretmouse, (object) (" " + Common.updateInfo.GetDeviceName(2).ToUpper()));
              else
                this.labeldevstatus2.Text = ResourceStr.checkturretmouse;
            }
            else if (Common.updateInfo.CurDevIndex == 2)
            {
              this.labeldevstatus2.ForeColor = Color.White;
              this.labeldevstatus2.Visible = true;
              if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
                this.labeldevstatus1.Text = string.Format(ResourceStr.checkturretkb, (object) (" " + Common.updateInfo.GetDeviceName(2).ToUpper()));
              else
                this.labeldevstatus1.Text = ResourceStr.checkturretkb;
            }
            else
            {
              this.labeldevstatus2.Visible = false;
              this.labeldevstatus2.Text = "";
            }
          }
          else
            this.labeldevstatus2.Visible = false;
          this.buttonUpdate.Enabled = true;
          this.buttonUpdate.Text = ResourceStr.next;
          this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
          if (Common.updateInfo.CurDevIndex == 1)
            Common.DongleStatus = 1;
          else if (Common.updateInfo.CurDevIndex == 2)
          {
            Common.Mousestatus = 1;
          }
          else
          {
            if (Common.updateInfo.CurDevIndex != 3)
              return;
            Common.KBstatus = 1;
          }
        }
      }
    }

    private void buttonUpdate_MouseEnter(object sender, EventArgs e) => this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_hover;

    private void buttonUpdate_MouseLeave(object sender, EventArgs e) => this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
      this.devreconnect = false;
      if (this.buttonUpdate.Text == ResourceStr.next)
      {
        Common.NextPage = Common.updateInfo.CurDevIndex != 1 ? (Common.updateInfo.CurDevIndex != 2 ? PageIndex.FormUpdateEnd : (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "025A" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0258" ? PageIndex.FormUpdateEnd : PageIndex.dev3)) : PageIndex.dev2;
        this.Close();
      }
      else
      {
        this.labelupdatestatus.Visible = false;
        this.labelPromptMessage.Visible = true;
        this.labelPromptMessage.ForeColor = Common.greendarktheme;
        this.labelPromptMessage.Text = ResourceStr.Nounplug;
        Application.DoEvents();
        this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_click;
        this.buttonUpdate.Enabled = false;
        this.buttonCancel.Enabled = false;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_disabled;
        this.labelUpdateInfor.Visible = true;
        this.labelUpdateprogress.Visible = true;
        this.progressBarupdate.Visible = true;
        this.progressBarupdate.Value = 0;
        this.labelUpdateprogress.Text = "0%";
        this.labelUpdateprogress.ForeColor = Common.lightgray;
        Application.DoEvents();
        this.labelCurFWver.Visible = true;
        this.labeltargetver.Visible = true;
        if (Common.DevFWNeedUpdate)
        {
          if (this.blconnect)
          {
            if (this.backgroundWorkerProcessFWData.IsBusy)
              return;
            this.backgroundWorkerProcessFWData.RunWorkerAsync();
          }
          else
          {
            this.curstate = eState.STATE_ENTER_BL;
            IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
            if (handle == IntPtr.Zero)
              return;
            this.device.EnterDevMode(handle, (byte) 1);
            this.timerbllistener.Enabled = true;
            this.timerbllistener.Start();
            this.timerblentersuccess.Enabled = true;
            this.timerblentersuccess.Start();
            this.labelUpdateInfor.Text = ResourceStr.updatefw;
            this.labelUpdateInfor.ForeColor = Common.lightgray;
            this.labelUpdateprogress.ForeColor = Common.lightgray;
          }
        }
        else
        {
          if (!Common.FlashFWNeedUpdate)
            return;
          this.labelUpdateInfor.ForeColor = Common.lightgray;
          this.labelUpdateprogress.ForeColor = Common.lightgray;
          this.labelUpdateInfor.Text = ResourceStr.updatefw;
          if (this.backgroundWorkerNordicENTERBL.IsBusy)
            return;
          this.backgroundWorkerNordicENTERBL.RunWorkerAsync();
        }
      }
    }

    private void backgroundWorkerEraseFlash_DoWork(object sender, DoWorkEventArgs e)
    {
      IntPtr zero = IntPtr.Zero;
      IntPtr handle = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : (this.isGenericDongle ? this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16("00FD", 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0) : this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
      if (handle == IntPtr.Zero)
        e.Result = (object) eState.STATE_WAITING_BOOTLOADER;
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 2.0)
      {
        this.backgroundWorkerEraseFlash.ReportProgress(0, (object) eState.STATE_ERASING_FLASH_PASS);
        e.Result = this.device.EraseFW(handle, Common.StartAddr, Common.EndAddr) != 2 ? (object) eState.STATE_ERASING_FLASH_FAIL : (object) eState.STATE_ERASING_FLASH_PASS;
        this.device.CloseDev(handle);
      }
      else
      {
        int uAddressEnd = (int) this.m_uAddressEnd;
        int uPageSize = (int) this.m_uPageSize;
        float num1 = 1f / (float) (this.m_uAddressEnd - this.m_uAddressBegin);
        try
        {
          this.curstate = eState.STATE_ERASING_FLASH;
          this.backgroundWorkerEraseFlash.ReportProgress(0, (object) eState.STATE_ERASING_FLASH);
        }
        catch (Exception ex)
        {
          int num2 = (int) MessageBox.Show(ex.Message);
        }
        if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 1.1000000238418579)
        {
          byte[] rbBuffer = new byte[8];
          long num3 = 0;
          float num4 = 1f / (float) (Common.filelen - 1L - num3);
          rbBuffer[0] = byte.MaxValue;
          for (long index = 0; index < Common.filelen; index += (long) Common.PAGESIZE)
          {
            this.backgroundWorkerEraseFlash.ReportProgress((int) (100.0 * (double) ((float) (index - num3) * num4)));
            if (!this.device.WinUSB_ControlIn(handle, (byte) 130, (ushort) ((ulong) Common.StartAddr + (ulong) index), (uint) (ushort) ((ulong) ((long) Common.StartAddr + index >> 16) & (ulong) ushort.MaxValue), (ushort) 0, (byte[]) null))
            {
              e.Result = (object) eState.STATE_ERASING_FLASH_FAIL;
              this.device.CloseDev(handle);
              return;
            }
            if (!this.device.WinUSB_ControlIn(handle, (byte) 143, (ushort) 0, 0U, (ushort) 1, rbBuffer) || rbBuffer[0] != (byte) 1)
            {
              e.Result = (object) eState.STATE_ERASING_FLASH_FAIL;
              this.device.CloseDev(handle);
              return;
            }
          }
          this.backgroundWorkerEraseFlash.ReportProgress(100);
          e.Result = (object) eState.STATE_ERASING_FLASH_PASS;
        }
        else
        {
          uint uAddressBegin;
          for (uAddressBegin = this.m_uAddressBegin; uAddressBegin <= this.m_uAddressEnd + 1U; uAddressBegin += (uint) this.m_uPageSize)
          {
            ushort uvalue = (ushort) uAddressBegin;
            ushort uindex = (ushort) ((ulong) uAddressBegin + (ulong) ((int) this.m_uPageSize - 1));
            bool flag = false;
            byte[] rbBuffer = new byte[8];
            this.backgroundWorkerEraseFlash.ReportProgress((int) (100.0 * (double) ((float) (uAddressBegin - this.m_uAddressBegin) * num1)));
            for (int index1 = 0; index1 < 3 && !flag; ++index1)
            {
              if (this.device.WinUSB_ControlIn(handle, (byte) 130, uvalue, (uint) uindex, (ushort) 0, rbBuffer))
              {
                this.device.Delay(2f);
                for (int index2 = 0; index2 < 3 && !flag; ++index2)
                {
                  rbBuffer[0] = byte.MaxValue;
                  if (this.device.WinUSB_ControlIn(handle, (byte) 143, (ushort) 0, 0U, (ushort) 1, rbBuffer) && byte.MaxValue != rbBuffer[0])
                  {
                    flag = true;
                    break;
                  }
                  this.device.Delay(100f);
                }
                if (!flag)
                  goto label_30;
                else
                  break;
              }
              else
              {
                this.device.Delay(100f);
                if (index1 == 2)
                  goto label_30;
              }
            }
          }
label_30:
          if (uAddressBegin >= this.m_uAddressEnd)
          {
            this.backgroundWorkerEraseFlash.ReportProgress(100);
            e.Result = (object) eState.STATE_ERASING_FLASH_PASS;
          }
          else
            e.Result = (object) eState.STATE_ERASING_FLASH_FAIL;
        }
        this.device.CloseDev(handle);
      }
    }

    private void backgroundWorkerEraseFlash_ProgressChanged(
      object sender,
      ProgressChangedEventArgs e)
    {
      if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
      {
        this.progressBarupdate.Value = !Common.FlashFWNeedUpdate ? (int) ((double) e.ProgressPercentage * 0.05 * 2.5 + 12.5) : (int) ((double) e.ProgressPercentage * 0.05) + 5;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
      }
      else
      {
        if (e.UserState == null || (eState) e.UserState != eState.STATE_ERASING_FLASH)
          return;
        this.labelUpdateInfor.ForeColor = Common.lightgray;
        this.labelUpdateInfor.Text = ResourceStr.updatefw;
      }
    }

    private void backgroundWorkerEraseFlash_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if ((eState) e.Result == eState.STATE_ERASING_FLASH_PASS)
        this.backgroundWorkerProgramFW.RunWorkerAsync();
      else if ((eState) e.Result == eState.STATE_ERASING_FLASH_FAIL)
      {
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
      else
      {
        if ((eState) e.Result != eState.STATE_WAITING_BOOTLOADER)
          return;
        if (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex);
          this.labelPromptMessage.Text = ResourceStr.reconnectdev;
          this.labelPromptMessage.ForeColor = Common.greendarktheme;
        }
        else
        {
          this.labelUpdateInfor.Text = ResourceStr.updatefail;
          this.labelUpdateInfor.ForeColor = Color.Red;
          this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
          this.buttonCancel.Visible = true;
          this.buttonCancel.Enabled = true;
          this.checkingretry = false;
        }
      }
    }

    private bool ProgramFW(BackgroundWorker bk, IntPtr handle, byte bCommand)
    {
      bool flag1 = true;
      ushort uAddressBegin = (ushort) this.m_uAddressBegin;
      float num1 = 1f / (float) ((int) (ushort) this.m_uAddressEnd - 1 - (int) uAddressBegin);
      string strSource1 = string.Copy(this.m_strFormattedData);
      byte[] rbBuffer1 = new byte[(int) this.m_uDataPacketSize];
      byte[] rbBuffer2 = new byte[(int) this.m_uDataPacketSize];
      if ((byte) 135 == bCommand)
      {
        this.curstate = eState.STATE_VERIFYING_FIRMWARE;
        bk.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE);
      }
      else
      {
        this.curstate = eState.STATE_DOWNLOADING_DATA;
        bk.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA);
      }
      while (strSource1.Length > 2)
      {
        string strSource2 = "";
        int iLength = strSource1.IndexOf(" ");
        if (iLength != 0)
        {
          strSource2 = FormFWUStep1.Mid(strSource1, 0, iLength);
          strSource1 = FormFWUStep1.Mid(strSource1, iLength + 1, strSource1.Length);
        }
        uint uint32 = Convert.ToUInt32(FormFWUStep1.Mid(strSource2, 0, 2), 16);
        uint uvalue = Convert.ToUInt32(FormFWUStep1.Mid(strSource2, 2, 2), 16) * 256U;
        uvalue += Convert.ToUInt32(FormFWUStep1.Mid(strSource2, 4, 2), 16);
        uint uindex = Convert.ToUInt32(FormFWUStep1.Mid(strSource2, 6, 2), 16) * 256U + Convert.ToUInt32(FormFWUStep1.Mid(strSource2, 8, 2), 16);
        float num2 = (float) (uvalue - (uint) uAddressBegin) * num1;
        bk.ReportProgress((int) (100.0 * (double) num2));
        int length = rbBuffer2.Length;
        for (int index = 0; index < length; ++index)
          rbBuffer2[index] = byte.MaxValue;
        for (int index = 0; (long) index < (long) uint32; ++index)
        {
          string s = FormFWUStep1.Mid(strSource2, index * 2 + 10, 2);
          if ("" != s)
            rbBuffer2[index] = byte.Parse(s, NumberStyles.AllowHexSpecifier);
        }
        flag1 = true;
        int num3 = 0;
        while (num3 < 3)
        {
          this.device.Delay(0.0f);
          bool flag2 = false;
          if (this.device.WinUSB_ControlOut(handle, bCommand, (ushort) uvalue, (uint) (ushort) uindex, (ushort) uint32, rbBuffer2))
          {
            for (int index = 0; index < 3; ++index)
            {
              if ((byte) 129 == bCommand)
                this.device.Delay(2f);
              rbBuffer1[0] = byte.MaxValue;
              flag1 = this.device.WinUSB_ControlIn(handle, (byte) 143, (ushort) 0, 0U, (ushort) 1, rbBuffer1);
              if (flag1 && byte.MaxValue != rbBuffer1[0])
              {
                flag2 = true;
                break;
              }
              this.device.Delay(100f);
            }
            if (!flag2)
            {
              string msg = string.Format("Address: {1} to {2}, WinUsbReturnValue: {3}, ReturnedData: {4}, PacketSize: {5}", (object) uvalue.ToString("X"), (object) uindex.ToString("X"), (object) flag1, (object) rbBuffer1[0].ToString("X"), (object) this.m_uDataPacketSize);
              Logger.getInstance().writeLog(msg, (short) 2);
              flag1 = false;
              break;
            }
            flag1 = true;
            break;
          }
          ++num3;
          this.device.Delay(100f);
          if (num3 >= 2)
          {
            flag1 = false;
            break;
          }
          if (num3 != 1)
            break;
        }
        if (!flag1)
          break;
      }
      if (!flag1 || strSource1.Length >= 2)
        return false;
      bk.ReportProgress(100);
      return true;
    }

    private void backgroundWorkerProgramFW_DoWork(object sender, DoWorkEventArgs e)
    {
      IntPtr zero = IntPtr.Zero;
      IntPtr handle = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : (this.isGenericDongle ? this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16("00FD", 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0) : this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
      if (handle == IntPtr.Zero)
        e.Result = (object) eState.STATE_WAITING_BOOTLOADER;
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 3.0)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        input.Position = 0L;
        byte[] buffer1 = new byte[2];
        byte[] buffer2 = new byte[1];
        byte[] numArray1 = new byte[64];
        this.curstate = eState.STATE_DOWNLOADING_DATA;
        this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA);
        float num1 = 1f / 59f;
        byte outpipe = 0;
        byte inpipe = 0;
        if (!this.device.GetPipeID(handle, ref inpipe, ref outpipe))
        {
          e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
          this.device.CloseDev(handle);
          input.Close();
          binaryReader.Close();
        }
        else
        {
          for (byte index1 = 1; index1 < (byte) 60; ++index1)
          {
            input.Position = (long) ((int) index1 * 512);
            this.backgroundWorkerProgramFW.ReportProgress((int) ((double) ((float) index1 * num1) * 100.0));
            buffer1[0] = (byte) 2;
            buffer1[1] = index1;
            this.device.WritePipe(handle, outpipe, buffer1, 2UL);
            this.device.ReadPipe(handle, inpipe, buffer2, 1);
            if (buffer2[0] != (byte) 0)
            {
              this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA_FAIL);
              e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
              int num2 = (int) MessageBox.Show("Error ret code is not 00");
              this.device.CloseDev(handle);
              input.Close();
              binaryReader.Close();
              return;
            }
            for (byte index2 = 0; index2 < (byte) 8; ++index2)
            {
              byte[] numArray2;
              if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
              {
                numArray2 = binaryReader.ReadBytes(64);
                if (numArray2.Length < 64)
                {
                  byte[] numArray3 = new byte[numArray2.Length];
                  numArray2.CopyTo((Array) numArray3, 0);
                  numArray2 = new byte[64];
                  FormFWUStep1.DataDefault(numArray2);
                  numArray3.CopyTo((Array) numArray2, 0);
                }
              }
              else
              {
                numArray2 = new byte[64];
                FormFWUStep1.DataDefault(numArray2);
              }
              this.device.WritePipe(handle, outpipe, numArray2, 64UL);
              this.device.ReadPipe(handle, inpipe, buffer2, 1);
              if (buffer2[0] != (byte) 0)
              {
                this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA_FAIL);
                e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
          }
          this.backgroundWorkerProgramFW.ReportProgress(100);
          e.Result = (object) eState.STATE_DOWNLOADING_DATA_PASS;
          this.device.CloseDev(handle);
          input.Close();
          binaryReader.Close();
        }
      }
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 2.0)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA);
        input.Position = 0L;
        byte[] numArray = new byte[8];
        long num3 = 0;
        float num4 = 1f / (float) (input.Length - 1L - num3);
        numArray[0] = byte.MaxValue;
        byte[] data;
        for (uint index = 0; (long) index < input.Length; index = (uint) ((ulong) index + (ulong) data.Length))
        {
          this.backgroundWorkerProgramFW.ReportProgress((int) ((double) ((float) ((long) index - num3) * num4) * 100.0));
          data = binaryReader.ReadBytes((int) Common.PACKLEN);
          int num5 = 0;
          while (num5 < Common.MAX_RETRY)
          {
            int num6 = !(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007E") ? this.device.ProgramFW(handle, (byte) data.Length, Common.StartAddr + index, 2, data) : this.device.ProgramFW(handle, (byte) data.Length, Common.StartAddr + index, 10, data);
            ++num5;
            if (num6 != 2)
            {
              if (num5 < Common.MAX_RETRY)
              {
                this.device.Delay(1f);
              }
              else
              {
                this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_PROCESSINGDATA_FAIL);
                e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
            else
              break;
          }
        }
        this.backgroundWorkerProgramFW.ReportProgress(100);
        e.Result = (object) eState.STATE_DOWNLOADING_DATA_PASS;
        this.device.CloseDev(handle);
        input.Close();
        binaryReader.Close();
      }
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 1.1000000238418579)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        this.curstate = eState.STATE_DOWNLOADING_DATA;
        this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA);
        input.Position = 0L;
        byte[] rbBuffer1 = new byte[8];
        long num7 = 0;
        float num8 = 1f / (float) (input.Length - 1L - num7);
        rbBuffer1[0] = byte.MaxValue;
        long uvalue = 0;
        while (uvalue < input.Length)
        {
          this.device.Delay(1f);
          this.backgroundWorkerProgramFW.ReportProgress((int) ((double) ((float) (uvalue - num7) * num8) * 100.0));
          byte[] rbBuffer2 = binaryReader.ReadBytes((int) Common.PACKLEN);
          if (rbBuffer2.Length < (int) Common.PACKLEN)
          {
            int num9 = 0;
            while (num9 < Common.MAX_RETRY)
            {
              int num10 = this.device.WinUSB_ControlOut(handle, (byte) 129, (ushort) ((ulong) Common.StartAddr + (ulong) uvalue), (uint) (ushort) ((ulong) ((long) Common.StartAddr + uvalue >> 16) & (ulong) ushort.MaxValue), (ushort) rbBuffer2.Length, rbBuffer2) ? 1 : 0;
              ++num9;
              if (num10 == 0)
              {
                if (num9 < Common.MAX_RETRY)
                {
                  this.device.Delay(1f);
                }
                else
                {
                  e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
                  this.device.CloseDev(handle);
                  input.Close();
                  binaryReader.Close();
                  return;
                }
              }
              else
                break;
            }
            uvalue += (long) rbBuffer2.Length;
          }
          else
          {
            int num11 = 0;
            while (num11 < Common.MAX_RETRY)
            {
              int num12 = this.device.WinUSB_ControlOut(handle, (byte) 129, (ushort) uvalue, (uint) (ushort) ((ulong) (uvalue >> 16) & (ulong) ushort.MaxValue), Common.PACKLEN, rbBuffer2) ? 1 : 0;
              ++num11;
              if (num12 == 0)
              {
                if (num11 < Common.MAX_RETRY)
                {
                  this.device.Delay(1f);
                }
                else
                {
                  e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
                  this.device.CloseDev(handle);
                  input.Close();
                  binaryReader.Close();
                  return;
                }
              }
              else
                break;
            }
            uvalue += (long) Common.PACKLEN;
          }
          this.device.Delay(1f);
          int num13 = 0;
          while (num13 < Common.MAX_RETRY)
          {
            int num14 = this.device.WinUSB_ControlIn(handle, (byte) 143, (ushort) 0, 0U, (ushort) 1, rbBuffer1) ? 1 : 0;
            ++num13;
            if (num14 == 0 || rbBuffer1[0] != (byte) 1)
            {
              if (num13 < Common.MAX_RETRY)
              {
                this.device.Delay(1f);
              }
              else
              {
                e.Result = (object) eState.STATE_DOWNLOADING_DATA_FAIL;
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
            else
              break;
          }
        }
        this.backgroundWorkerProgramFW.ReportProgress(100);
        e.Result = (object) eState.STATE_DOWNLOADING_DATA_PASS;
        this.device.CloseDev(handle);
        input.Close();
        binaryReader.Close();
      }
      else
      {
        this.curstate = eState.STATE_DOWNLOADING_DATA;
        this.backgroundWorkerProgramFW.ReportProgress(0, (object) eState.STATE_DOWNLOADING_DATA);
        e.Result = !this.ProgramFW(this.backgroundWorkerProgramFW, handle, (byte) 129) ? (object) eState.STATE_DOWNLOADING_DATA_FAIL : (object) eState.STATE_DOWNLOADING_DATA_PASS;
        this.device.CloseDev(handle);
      }
    }

    private void backgroundWorkerProgramFW_ProgressChanged(
      object sender,
      ProgressChangedEventArgs e)
    {
      if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
      {
        this.progressBarupdate.Value = !Common.FlashFWNeedUpdate ? (int) ((double) e.ProgressPercentage * 0.15 * 2.5 + 25.0) : (int) ((double) e.ProgressPercentage * 0.15) + 10;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
      }
      else
      {
        this.progressBarupdate.Value = e.ProgressPercentage / 2;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
        if (e.UserState != null && (eState) e.UserState == eState.STATE_DOWNLOADING_DATA)
        {
          this.labelUpdateInfor.ForeColor = Common.lightgray;
          this.labelUpdateInfor.Text = ResourceStr.updatefw;
          this.labelPromptMessage.Text = ResourceStr.Nounplug;
          this.labelPromptMessage.ForeColor = Common.greendarktheme;
        }
        Application.DoEvents();
      }
    }

    private void backgroundWorkerProgramFW_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if ((eState) e.Result == eState.STATE_DOWNLOADING_DATA_PASS)
        this.backgroundWorkerVerify.RunWorkerAsync();
      else if ((eState) e.Result == eState.STATE_DOWNLOADING_DATA_FAIL)
      {
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.Text = ResourceStr.cancel;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
      else
      {
        if ((eState) e.Result != eState.STATE_WAITING_BOOTLOADER)
          return;
        Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex);
        this.labelPromptMessage.Text = ResourceStr.reconnectdev;
        this.labelPromptMessage.ForeColor = Common.greendarktheme;
      }
    }

    private bool ArrayCompare(byte[] array1, byte[] array2)
    {
      if (array1.Length != array2.Length)
        return false;
      for (int index = 0; index < array1.Length; ++index)
      {
        if ((int) array1[index] != (int) array2[index])
          return false;
      }
      return true;
    }

    private void backgroundWorkerVerify_DoWork(object sender, DoWorkEventArgs e)
    {
      IntPtr zero = IntPtr.Zero;
      IntPtr handle = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : (this.isGenericDongle ? this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16("00FD", 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0) : this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
      if (handle == IntPtr.Zero)
        e.Result = (object) eState.STATE_WAITING_BOOTLOADER;
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 3.0)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        input.Position = 0L;
        byte[] buffer1 = new byte[2];
        byte[] buffer2 = new byte[1];
        byte[] numArray1 = new byte[64];
        byte outpipe = 0;
        byte inpipe = 0;
        if (!this.device.GetPipeID(handle, ref inpipe, ref outpipe))
        {
          e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
          this.device.CloseDev(handle);
          input.Close();
          binaryReader.Close();
        }
        else
        {
          this.curstate = eState.STATE_VERIFYING_FIRMWARE;
          float num1 = 1f / 472f;
          this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE);
          for (ushort index = 0; index < (ushort) 480; ++index)
          {
            if ((long) ((int) index * 64) < binaryReader.BaseStream.Length)
              input.Position = (long) ((int) index * 64);
            else
              input.Position = binaryReader.BaseStream.Length;
            if (index >= (ushort) 8)
            {
              byte[] bytes = BitConverter.GetBytes(index);
              byte[] buffer3 = new byte[2]
              {
                (byte) 6,
                bytes[1]
              };
              this.device.WritePipe(handle, outpipe, buffer3, 2UL);
              this.device.ReadPipe(handle, inpipe, buffer2, 1);
              if (buffer2[0] != (byte) 0)
              {
                this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_FAILED);
                e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                int num2 = (int) MessageBox.Show("Error ret code is not 00");
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
              byte[] buffer4 = new byte[2]
              {
                (byte) 3,
                bytes[0]
              };
              byte[] buffer5 = new byte[64];
              this.device.WritePipe(handle, outpipe, buffer4, 2UL);
              this.device.ReadPipe(handle, inpipe, buffer5, 64);
              if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
              {
                byte[] numArray2 = binaryReader.ReadBytes(64);
                if (numArray2.Length < 64)
                {
                  byte[] numArray3 = new byte[numArray2.Length];
                  numArray2.CopyTo((Array) numArray3, 0);
                  byte[] data = new byte[64];
                  FormFWUStep1.DataDefault(data);
                  numArray3.CopyTo((Array) data, 0);
                }
              }
              else
                FormFWUStep1.DataDefault(new byte[64]);
              this.backgroundWorkerVerify.ReportProgress((int) ((double) ((float) ((int) index - 7) * num1) * 100.0));
            }
          }
          buffer1[0] = (byte) 2;
          buffer1[1] = (byte) 0;
          this.device.WritePipe(handle, outpipe, buffer1, 2UL);
          this.device.ReadPipe(handle, inpipe, buffer2, 1);
          if (buffer2[0] != (byte) 0)
          {
            this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_FAILED);
            e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
            int num3 = (int) MessageBox.Show("Error ret code is not 00");
            this.device.CloseDev(handle);
            input.Close();
            binaryReader.Close();
          }
          else
          {
            input.Position = 0L;
            for (byte index = 0; index < (byte) 8; ++index)
            {
              byte[] buffer6 = binaryReader.ReadBytes(64);
              this.device.WritePipe(handle, outpipe, buffer6, 64UL);
              this.device.ReadPipe(handle, inpipe, buffer2, 1);
              if (buffer2[0] != (byte) 0)
              {
                this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_FAILED);
                e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                int num4 = (int) MessageBox.Show("Error ret code is not 00");
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
            input.Position = 0L;
            for (ushort index = 0; index < (ushort) 8; ++index)
            {
              byte[] bytes = BitConverter.GetBytes(index);
              byte[] buffer7 = new byte[2]
              {
                (byte) 6,
                bytes[1]
              };
              this.device.WritePipe(handle, outpipe, buffer7, 2UL);
              this.device.ReadPipe(handle, inpipe, buffer2, 1);
              if (buffer2[0] != (byte) 0)
              {
                this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_FAILED);
                e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                int num5 = (int) MessageBox.Show("Error ret code is not 00");
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
              byte[] buffer8 = new byte[2]
              {
                (byte) 3,
                bytes[0]
              };
              byte[] numArray4 = new byte[64];
              this.device.WritePipe(handle, outpipe, buffer8, 2UL);
              this.device.ReadPipe(handle, inpipe, numArray4, 64);
              if (!this.ArrayCompare(numArray4, binaryReader.ReadBytes(64)))
              {
                this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_FAILED);
                e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                int num6 = (int) MessageBox.Show(string.Format("offset: {0} fw data verifier fail", (object) index));
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
            this.backgroundWorkerVerify.ReportProgress(100);
            e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_PASS;
            this.device.CloseDev(handle);
            input.Close();
            binaryReader.Close();
          }
        }
      }
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 2.0)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE);
        input.Position = 0L;
        byte[] numArray5 = new byte[8];
        long num7 = 0;
        float num8 = 1f / (float) (input.Length - 1L - num7);
        numArray5[0] = byte.MaxValue;
        byte[] numArray6;
        for (uint index1 = 0; (long) index1 < input.Length; index1 = (uint) ((ulong) index1 + (ulong) numArray6.Length))
        {
          this.device.Delay(1f);
          this.backgroundWorkerVerify.ReportProgress((int) ((double) ((float) ((long) index1 - num7) * num8) * 100.0));
          numArray6 = binaryReader.ReadBytes((int) Common.PACKLEN);
          byte[] data = new byte[(int) (byte) numArray6.Length];
          int num9 = 0;
          while (num9 < Common.MAX_RETRY)
          {
            int num10 = !(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "007E") ? this.device.VerifyFW(handle, (byte) numArray6.Length, Common.StartAddr + index1, 2, data) : this.device.VerifyFW(handle, (byte) numArray6.Length, Common.StartAddr + index1, 5, data);
            ++num9;
            if (num10 == 2)
            {
              for (int index2 = 0; index2 < numArray6.Length; ++index2)
              {
                if ((int) numArray6[index2] != (int) data[index2])
                {
                  this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE_FAIL);
                  e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                  this.device.CloseDev(handle);
                  input.Close();
                  binaryReader.Close();
                  return;
                }
              }
              break;
            }
            if (num9 < Common.MAX_RETRY)
            {
              this.device.Delay(1f);
            }
            else
            {
              this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE_FAIL);
              e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
              this.device.CloseDev(handle);
              input.Close();
              binaryReader.Close();
              return;
            }
          }
        }
        this.backgroundWorkerVerify.ReportProgress(100);
        e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_PASS;
        this.device.CloseDev(handle);
        input.Close();
        binaryReader.Close();
      }
      else if ((double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) == 1.1000000238418579)
      {
        FileStream input = new FileStream(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin", FileMode.Open);
        BinaryReader binaryReader = new BinaryReader((Stream) input);
        this.curstate = eState.STATE_VERIFYING_FIRMWARE;
        this.backgroundWorkerVerify.ReportProgress(0, (object) eState.STATE_VERIFYING_FIRMWARE);
        input.Position = 0L;
        byte[] rbBuffer1 = new byte[8];
        long num11 = 0;
        float num12 = 1f / (float) (input.Length - 1L - num11);
        rbBuffer1[0] = byte.MaxValue;
        long uvalue = 0;
        while (uvalue < input.Length)
        {
          this.device.Delay(1f);
          this.backgroundWorkerVerify.ReportProgress((int) ((double) ((float) (uvalue - num11) * num12) * 100.0));
          byte[] rbBuffer2 = binaryReader.ReadBytes((int) Common.PACKLEN);
          if (rbBuffer2.Length < (int) Common.PACKLEN)
          {
            int num13 = 0;
            while (num13 < Common.MAX_RETRY)
            {
              int num14 = this.device.WinUSB_ControlOut(handle, (byte) 135, (ushort) ((ulong) Common.StartAddr + (ulong) uvalue), (uint) (ushort) ((ulong) ((long) Common.StartAddr + uvalue >> 16) & (ulong) ushort.MaxValue), (ushort) rbBuffer2.Length, rbBuffer2) ? 1 : 0;
              ++num13;
              if (num14 == 0)
              {
                if (num13 < Common.MAX_RETRY)
                {
                  this.device.Delay(1f);
                }
                else
                {
                  e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                  this.device.CloseDev(handle);
                  input.Close();
                  binaryReader.Close();
                  return;
                }
              }
              else
                break;
            }
            uvalue += (long) rbBuffer2.Length;
          }
          else
          {
            int num15 = 0;
            while (num15 < Common.MAX_RETRY)
            {
              int num16 = this.device.WinUSB_ControlOut(handle, (byte) 135, (ushort) uvalue, (uint) (ushort) ((ulong) (uvalue >> 16) & (ulong) ushort.MaxValue), Common.PACKLEN, rbBuffer2) ? 1 : 0;
              ++num15;
              if (num16 == 0)
              {
                if (num15 < Common.MAX_RETRY)
                {
                  this.device.Delay(1f);
                }
                else
                {
                  e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                  this.device.CloseDev(handle);
                  input.Close();
                  binaryReader.Close();
                  return;
                }
              }
              else
                break;
            }
            uvalue += (long) Common.PACKLEN;
          }
          this.device.Delay(1f);
          int num17 = 0;
          while (num17 < Common.MAX_RETRY)
          {
            int num18 = this.device.WinUSB_ControlIn(handle, (byte) 143, (ushort) 0, 0U, (ushort) 1, rbBuffer1) ? 1 : 0;
            ++num17;
            if (num18 == 0 || rbBuffer1[0] != (byte) 1)
            {
              if (num17 < Common.MAX_RETRY)
              {
                this.device.Delay(1f);
              }
              else
              {
                e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_FAIL;
                this.device.CloseDev(handle);
                input.Close();
                binaryReader.Close();
                return;
              }
            }
            else
              break;
          }
        }
        this.backgroundWorkerVerify.ReportProgress(100);
        e.Result = (object) eState.STATE_VERIFYING_FIRMWARE_PASS;
        this.device.CloseDev(handle);
        input.Close();
        binaryReader.Close();
      }
      else
      {
        int maxValue = (int) byte.MaxValue;
        e.Result = !Common.updateInfo.IsVerifyCheckSum(Common.updateInfo.CurDevIndex) ? (!this.ProgramFW(this.backgroundWorkerVerify, handle, (byte) 135) ? (object) eState.STATE_VERIFYING_FIRMWARE_FAIL : (object) eState.STATE_VERIFYING_FIRMWARE_PASS) : (!this.callWinusbChecksumControl(handle, 1, ref maxValue) ? (object) eState.STATE_VERIFYING_FIRMWARE_FAIL : (maxValue != 0 ? (object) eState.STATE_VERIFYING_FIRMWARE_FAIL : (object) eState.STATE_VERIFYING_FIRMWARE_PASS));
        this.device.CloseDev(handle);
      }
    }

    private void backgroundWorkerVerify_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
      {
        this.progressBarupdate.Value = !Common.FlashFWNeedUpdate ? (int) ((double) e.ProgressPercentage * 0.15 * 2.5 + 62.5) : (int) ((double) e.ProgressPercentage * 0.15) + 25;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
      }
      else
      {
        this.progressBarupdate.Value = 50 + e.ProgressPercentage / 2;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
        if (e.UserState == null || (eState) e.UserState != eState.STATE_VERIFYING_FIRMWARE)
          return;
        this.labelUpdateInfor.ForeColor = Common.lightgray;
        this.labelUpdateInfor.Text = ResourceStr.updatefw;
      }
    }

    private void backgroundWorkerVerify_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if ((eState) e.Result == eState.STATE_VERIFYING_FIRMWARE_PASS)
      {
        IntPtr zero = IntPtr.Zero;
        IntPtr handle = (double) Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex) != 2.0 ? this.device.OpenBootloader(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex)) : (this.isGenericDongle ? this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16("00FD", 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0) : this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetBLVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetBLPID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt16(Common.updateInfo.GetBLBCDDevPID(Common.updateInfo.CurDevIndex), 16), Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex), 12, 91, 0, 0));
        if (handle == IntPtr.Zero)
          return;
        this.device.ExitBL(handle, Common.updateInfo.GetBLVer(Common.updateInfo.CurDevIndex));
        this.device.CloseDev(handle);
        this.curstate = eState.STATE_EXIT_BL;
      }
      else if ((eState) e.Result == eState.STATE_VERIFYING_FIRMWARE_FAIL)
      {
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
      else
      {
        if ((eState) e.Result != eState.STATE_WAITING_BOOTLOADER)
          return;
        Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex);
        this.labelPromptMessage.Text = ResourceStr.reconnectdev;
        this.labelPromptMessage.ForeColor = Common.greendarktheme;
      }
    }

    private void timerbllistener_Tick(object sender, EventArgs e)
    {
      this.timerbllistener.Stop();
      this.timerbllistener.Enabled = false;
      if (this.blconnect)
        return;
      if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0205" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "020F" || Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0220")
      {
        this.labelPromptMessage.Text = ResourceStr.BLfailretry;
        if (MessageBox.Show(this.labelPromptMessage.Text, "Please restart system", MessageBoxButtons.OKCancel) == DialogResult.OK)
        {
          this.device.RestartSystem();
        }
        else
        {
          Thread.Sleep(10000);
          Application.Exit();
        }
      }
      else
      {
        int num = (int) MessageBox.Show(ResourceStr.reconnectdev);
      }
    }

    private void timerblentersuccess_Tick(object sender, EventArgs e)
    {
      if (!(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0517") && this.devconnect)
      {
        this.timerbllistener.Stop();
        this.timerbllistener.Enabled = false;
        this.timerblentersuccess.Stop();
        this.timerblentersuccess.Enabled = false;
        int num = (int) MessageBox.Show(ResourceStr.closesynapseandotherapp);
        this.curstate = eState.STATE_ENTER_BL;
        IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
        if (handle == IntPtr.Zero)
          return;
        this.device.EnterDevMode(handle, (byte) 1);
        this.timerbllistener.Enabled = true;
        this.timerbllistener.Start();
        this.timerblentersuccess.Enabled = true;
        this.timerblentersuccess.Start();
        this.timerblentersuccess.Interval = 2000;
        this.labelUpdateInfor.Text = ResourceStr.updatefw;
        this.labelUpdateInfor.ForeColor = Common.lightgray;
      }
      else
      {
        this.timerblentersuccess.Stop();
        this.timerblentersuccess.Enabled = false;
      }
    }

    private void backgroundWorkerCloseRestartDialog_DoWork(object sender, DoWorkEventArgs e)
    {
      while (!this.stopclosethread)
      {
        Process[] processesByName1 = Process.GetProcessesByName("taskhostw");
        IntPtr mainWindowHandle;
        for (int index = 0; index < processesByName1.Length; ++index)
        {
          mainWindowHandle = processesByName1[index].MainWindowHandle;
          if (mainWindowHandle.ToInt32() != 0)
            processesByName1[index].CloseMainWindow();
        }
        Process[] processesByName2 = Process.GetProcessesByName("taskhost");
        for (int index = 0; index < processesByName2.Length; ++index)
        {
          mainWindowHandle = processesByName2[index].MainWindowHandle;
          if (mainWindowHandle.ToInt32() != 0)
            processesByName2[index].CloseMainWindow();
        }
      }
    }

    private void backgroundWorkerCloseRestartDialog_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
    }

    private void FormFWUStep1_Shown(object sender, EventArgs e)
    {
      float num = (float) this.Width / 730f;
      while ((double) this.labelHeader.Width > (double) this.Width - (double) Common.logowidth * (double) num)
        this.labelHeader.Font = new Font(this.labelHeader.Font.FontFamily, this.labelHeader.Font.Size - 1f);
      if (Common.updateInfo.CurDevIndex >= 0)
      {
        int curDevIndex = Common.updateInfo.CurDevIndex;
      }
      MyButton buttonUpdate = this.buttonUpdate;
      Size size = this.Size;
      int x = size.Width - this.buttonUpdate.Width - 40;
      size = this.Size;
      int y = size.Height - this.buttonUpdate.Height - Common.hspacebtnbottom;
      Point point = new Point(x, y);
      buttonUpdate.Location = point;
      this.buttonCancel.Location = new Point(this.buttonUpdate.Location.X - Common.wspacebutton - this.buttonCancel.Width, this.buttonUpdate.Location.Y);
      Common.NextPage = PageIndex.Close;
    }

    private void FormFWUStep1_FormClosed(object sender, FormClosedEventArgs e)
    {
      try
      {
        File.Delete(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "FW.bin");
      }
      catch (Exception ex)
      {
      }
    }

    private void backgroundWorkerCheckFlashFWVer_DoWork(object sender, DoWorkEventArgs e)
    {
      int num = 0;
      IntPtr zero = IntPtr.Zero;
      IntPtr handle;
      while (true)
      {
        ++num;
        handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
        if (handle == IntPtr.Zero)
        {
          if (num != 10)
            this.device.Delay(1000f);
          else
            break;
        }
        else
          goto label_5;
      }
      e.Result = (object) eState.STATE_WAITING_DEVICE;
      return;
label_5:
      string strA = "";
      if (!(handle != IntPtr.Zero))
        return;
      for (int index = 0; strA == "" && index < 15; ++index)
      {
        strA = this.device.GetDevFWVer(handle, false);
        if (!(strA != ""))
          this.device.Delay(1000f);
        else
          break;
      }
      this.device.CloseDev(handle);
      Common.curdevver = strA;
      Common.updateInfo.ActFlashFWVer = strA;
      Common.updateInfo.FlashSameVer = string.Compare(strA, Common.updateInfo.GetFlashFWVer(Common.updateInfo.CurDevIndex));
    }

    private void backgroundWorkerCheckFlashFWVer_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (e.Result != null)
      {
        this.curstate = eState.STATE_NULL;
        this.labelUpdateInfor.Text = "Get FW Version Fail!";
        this.labelUpdateInfor.ForeColor = Color.Red;
      }
      else
      {
        int flashSameVer = Common.updateInfo.FlashSameVer;
        this.labeltargetver.Text = ResourceStr.newver + Common.updateInfo.GetFlashFWVer(Common.updateInfo.CurDevIndex);
        this.labeltargetver.Visible = true;
        this.labelCurFWver.ForeColor = Color.FromArgb(7763574);
        if (flashSameVer < 0)
        {
          if (Common.FlashFWNeedUpdate)
          {
            if (this.backgroundWorkerNordicENTERBL.IsBusy)
              return;
            this.backgroundWorkerNordicENTERBL.RunWorkerAsync();
          }
          else
          {
            this.labelCurFWver.Text = ResourceStr.devicever + Common.updateInfo.ActFlashFWVer;
            this.labelCurFWver.Visible = true;
            this.labeltargetver.ForeColor = Color.FromArgb(7584512);
            this.labelPromptMessage.Visible = true;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.buttonUpdate.Enabled = true;
            this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_normal;
            this.labelPromptMessage.ForeColor = Common.greendarktheme;
            this.labelPromptMessage.Text = ResourceStr.anupdaterequired;
          }
        }
        else if (this.curstate == eState.STATE_FLASHFW_AFTER)
        {
          this.curstate = eState.STATE_NULL;
          this.labelCurFWver.Text = ResourceStr.devicever + Common.updateInfo.ActFlashFWVer;
          this.labeltargetver.ForeColor = Color.FromArgb(7763574);
          this.labelCurFWver.Visible = true;
          this.labeltargetver.Visible = true;
          Application.DoEvents();
          this.device.Delay(1000f);
          this.Close();
        }
        else
        {
          this.labelCurFWver.Visible = false;
          this.labelPromptMessage.Visible = true;
          this.labelUpdateInfor.Visible = false;
          this.labelUpdateprogress.Visible = false;
          this.progressBarupdate.Visible = false;
          this.buttonUpdate.Enabled = false;
          this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
          this.buttonCancel.Enabled = true;
          this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
          this.labelPromptMessage.ForeColor = Common.greendarktheme;
          this.labelPromptMessage.Text = ResourceStr.noupdaterequired;
          this.labeltargetver.ForeColor = Common.lightgray;
          this.labeltargetver.Text = ResourceStr.devicever + Common.updateInfo.GetFlashFWVer(Common.updateInfo.CurDevIndex);
        }
      }
    }

    private void backgroundWorkerNordicENTERBL_DoWork(object sender, DoWorkEventArgs e)
    {
      IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
      if (handle != IntPtr.Zero)
      {
        byte[] retdata = new byte[80];
        int num = this.device.SendCommand(handle, (byte) 0, (byte) 10, (byte) 0, (byte) 0, (byte) 0, (byte) 0, 5, 100, (byte[]) null, retdata);
        e.Result = (object) num;
        this.device.CloseDev(handle);
      }
      else
        e.Result = (object) 0;
    }

    private void backgroundWorkerNordicENTERBL_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (Convert.ToInt32(e.Result.ToString()) == 2)
      {
        for (int index = 0; index < 5; ++index)
        {
          this.device.Delay(1000f);
          IntPtr zero = IntPtr.Zero;
          IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
          if (handle != IntPtr.Zero)
          {
            if (this.device.GetDevFWVer(handle, false).StartsWith("177."))
            {
              this.device.CloseDev(handle);
              break;
            }
            this.device.CloseDev(handle);
          }
        }
        this.device.Delay(1000f);
        if (this.backgroundWorkerGetRegionInfor.IsBusy)
          return;
        this.backgroundWorkerGetRegionInfor.RunWorkerAsync();
      }
      else
      {
        this.curstate = eState.STATE_FAILED;
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.Text = ResourceStr.cancel;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
    }

    private void backgroundWorkerGetRegionInfor_DoWork(object sender, DoWorkEventArgs e)
    {
      IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
      if (handle != IntPtr.Zero)
      {
        byte[] retdata = new byte[80];
        byte[] numArray = new byte[80];
        Array.Clear((Array) numArray, 0, 80);
        int num1 = this.device.SendCommand(handle, (byte) 0, (byte) 10, (byte) 128, (byte) 0, (byte) 0, (byte) 80, 5, 100, numArray, retdata);
        if (num1 != 2)
        {
          e.Result = (object) 0;
          int num2 = (int) MessageBox.Show("Get Region Information Fail!");
        }
        else
        {
          Common.total = retdata[0];
          Common.RegionID = retdata[1];
          Common.type = retdata[2];
          Common.regionsize = (uint) ((int) retdata[4] * 256 * 256 * 256 + (int) retdata[5] * 256 * 256 + (int) retdata[6] * 256) + (uint) retdata[7];
        }
        e.Result = (object) num1;
        this.device.CloseDev(handle);
      }
      else
        e.Result = (object) 0;
    }

    private void backgroundWorkerGetRegionInfor_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (Convert.ToInt32(e.Result.ToString()) == 2)
      {
        if (Common.total > (byte) 0)
        {
          if (Common.type == (byte) 2 || Common.type == (byte) 3)
          {
            if (this.backgroundWorkerNordicProgram.IsBusy)
              return;
            this.backgroundWorkerNordicProgram.RunWorkerAsync();
          }
          else
          {
            int num1 = (int) MessageBox.Show("Your Region can't write!");
          }
        }
        else
        {
          int num2 = (int) MessageBox.Show("Sorry, your device have no region id!");
        }
      }
      else
      {
        this.curstate = eState.STATE_FAILED;
        this.curstate = eState.STATE_FAILED;
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.Text = ResourceStr.cancel;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
    }

    private void backgroundWorkerNordicProgram_DoWork(object sender, DoWorkEventArgs e)
    {
      Common.crc = ushort.MaxValue;
      e.Result = (object) 1;
      IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
      if (handle != IntPtr.Zero)
      {
        Thread.Sleep(1000);
        string str = "";
        byte num1 = 0;
        byte[] numArray = new byte[80];
        byte[] retdata = new byte[80];
        this.backgroundWorkerNordicProgram.ReportProgress(0);
        int num2 = 0;
        uint flashFwLineNum = Common.updateInfo.GetFlashFWLineNum(Common.updateInfo.CurDevIndex);
        while ((long) num2 < (long) flashFwLineNum)
        {
          Array.Clear((Array) numArray, 0, 80);
          Array.Clear((Array) retdata, 0, 80);
          num1 = (byte) 0;
          str = "";
          string source = Common.updateInfo.GetFlashFWLine(Common.updateInfo.CurDevIndex, num2++).Trim('\r').Trim('\n');
          byte[] data = new byte[92];
          Array.Clear((Array) data, 0, 92);
          for (int index = 0; index < source.Length; ++index)
            data[index] = (byte) source.ElementAt<char>(index);
          Common.CRC16(data);
          byte length = (byte) source.Length;
          numArray[0] = Common.RegionID;
          numArray[1] = (byte) 0;
          numArray[2] = (byte) 0;
          numArray[3] = (byte) 0;
          numArray[4] = (byte) 0;
          numArray[5] = (byte) 0;
          numArray[6] = length;
          for (int index = 0; index < source.Length; ++index)
            numArray[index + 7] = (byte) source.ElementAt<char>(index);
          if (this.device.SendCommand(handle, (byte) 0, (byte) 10, (byte) 2, byte.MaxValue, byte.MaxValue, (byte) ((uint) length + 7U), 5, 2, numArray, retdata) == 2 && retdata[5] == (byte) 0)
          {
            this.backgroundWorkerNordicProgram.ReportProgress((int) ((long) (num2 * 100) / (long) flashFwLineNum));
            Thread.Sleep(12);
          }
          else
          {
            e.Result = (object) 0;
            break;
          }
        }
        this.device.CloseDev(handle);
        if (Convert.ToInt32(e.Result) != 1)
          return;
        this.backgroundWorkerNordicProgram.ReportProgress(100);
      }
      else
        e.Result = (object) 0;
    }

    private void backgroundWorkerNordicProgram_ProgressChanged(
      object sender,
      ProgressChangedEventArgs e)
    {
      if (Common.updateInfo.IsUpdateFlashFW(Common.updateInfo.CurDevIndex))
      {
        if (Common.DevFWNeedUpdate)
        {
          this.progressBarupdate.Value = (int) ((double) e.ProgressPercentage * 0.6) + 40;
        }
        else
        {
          this.progressBarupdate.Value = e.ProgressPercentage;
          this.labelUpdateInfor.Text = ResourceStr.updatefw;
          this.labelUpdateInfor.ForeColor = Common.lightgray;
          this.labelPromptMessage.Text = ResourceStr.Nounplug;
        }
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
      }
      else
      {
        this.progressBarupdate.Value = e.ProgressPercentage;
        this.labelUpdateprogress.Text = this.progressBarupdate.Value.ToString() + "%";
      }
    }

    private void backgroundWorkerNordicProgram_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (Convert.ToInt32(e.Result) == 0)
      {
        this.curstate = eState.STATE_FAILED;
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.Text = ResourceStr.cancel;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
      else
      {
        if (this.backgroundWorkerVerifyNordicFW.IsBusy)
          return;
        this.backgroundWorkerVerifyNordicFW.RunWorkerAsync();
      }
    }

    private void backgroundWorkerVerifyNordicFW_DoWork(object sender, DoWorkEventArgs e)
    {
      Thread.Sleep(100);
      e.Result = (object) 1;
      IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
      if (handle != IntPtr.Zero)
      {
        int flashFwLineNum = (int) Common.updateInfo.GetFlashFWLineNum(Common.updateInfo.CurDevIndex);
        byte[] numArray = new byte[80];
        byte[] retdata = new byte[80];
        byte num = 3;
        numArray[0] = Common.RegionID;
        numArray[1] = (byte) 0;
        numArray[2] = (byte) 0;
        numArray[3] = (byte) 0;
        numArray[4] = (byte) 0;
        numArray[5] = (byte) 0;
        numArray[6] = num;
        numArray[7] = (byte) 60;
        if (Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex) == "0059")
        {
          numArray[8] = (byte) 0;
          numArray[9] = (byte) 0;
        }
        else
        {
          byte[] bytes = BitConverter.GetBytes(Common.crc);
          numArray[8] = bytes[1];
          numArray[9] = bytes[0];
        }
        e.Result = this.device.SendCommand(handle, (byte) 0, (byte) 10, (byte) 2, (byte) 0, (byte) 0, (byte) ((uint) num + 7U), 5, 2, numArray, retdata) != 2 || retdata[5] != (byte) 0 ? (object) 0 : (object) 1;
        this.device.CloseDev(handle);
      }
      else
        e.Result = (object) 0;
    }

    private void backgroundWorkerVerifyNordicFW_RunWorkerCompleted(
      object sender,
      RunWorkerCompletedEventArgs e)
    {
      if (Convert.ToInt32(e.Result.ToString()) == 0)
      {
        this.curstate = eState.STATE_FAILED;
        this.devreconnect = false;
        this.checkingretry = true;
        while (MessageBox.Show(ResourceStr.updatefail, this.labelHeader.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Retry)
        {
          if (this.devreconnect)
          {
            this.checkingretry = false;
            this.buttonUpdate_Click((object) null, (EventArgs) null);
            return;
          }
          if (!this.devconnect || !this.blconnect)
          {
            this.checkingretry = false;
            this.labelPromptMessage.Visible = false;
            this.labelUpdateInfor.Visible = false;
            this.labelUpdateprogress.Visible = false;
            this.progressBarupdate.Visible = false;
            this.labelpluginDevice.ForeColor = Common.greendarktheme;
            return;
          }
        }
        this.labelUpdateInfor.Text = ResourceStr.updatefail;
        this.labelUpdateInfor.ForeColor = Color.Red;
        this.buttonCancel.Text = ResourceStr.cancel;
        this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
        this.buttonCancel.Visible = true;
        this.buttonCancel.Enabled = true;
        this.checkingretry = false;
      }
      else
      {
        Thread.Sleep(500);
        this.curstate = eState.STATE_EXIT_NordicBL;
        IntPtr handle = this.device.OpenDev(Convert.ToUInt32(Common.updateInfo.GetVID(Common.updateInfo.CurDevIndex), 16), Convert.ToUInt32(Common.updateInfo.GetPID(Common.updateInfo.CurDevIndex), 16), (ushort) 0, 0.0f, Common.updateInfo.GetDevReportType(Common.updateInfo.CurDevIndex), Common.updateInfo.GetFeatureRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetInputRptLen(Common.updateInfo.CurDevIndex), Common.updateInfo.GetOutputRptLen(Common.updateInfo.CurDevIndex));
        if (!(handle != IntPtr.Zero))
          return;
        for (int index = 0; index < 10; ++index)
        {
          byte[] retdata = new byte[80];
          if (this.device.SendCommand(handle, (byte) 0, (byte) 12, (byte) 0, (byte) 0, (byte) 0, (byte) 0, 5, 10, (byte[]) null, retdata) == 2)
            break;
        }
        Thread.Sleep(1000);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.components = (IContainer) new System.ComponentModel.Container();
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (FormFWUStep1));
      this.labelHeader = new Label();
      this.labelpluginDevice = new Label();
      this.labelPromptMessage = new Label();
      this.labelUpdateprogress = new Label();
      this.labelUpdateInfor = new Label();
      this.labeltargetver = new Label();
      this.labelCurFWver = new Label();
      this.backgroundWorkerProcessFWData = new BackgroundWorker();
      this.backgroundWorkerCheckVer = new BackgroundWorker();
      this.backgroundWorkerEraseFlash = new BackgroundWorker();
      this.backgroundWorkerProgramFW = new BackgroundWorker();
      this.backgroundWorkerVerify = new BackgroundWorker();
      this.timerbllistener = new System.Windows.Forms.Timer(this.components);
      this.timerblentersuccess = new System.Windows.Forms.Timer(this.components);
      this.backgroundWorkerCloseRestartDialog = new BackgroundWorker();
      this.backgroundWorkercheckbattery = new BackgroundWorker();
      this.backgroundWorkerCheckFlashFWVer = new BackgroundWorker();
      this.backgroundWorkerNordicENTERBL = new BackgroundWorker();
      this.backgroundWorkerGetRegionInfor = new BackgroundWorker();
      this.backgroundWorkerNordicProgram = new BackgroundWorker();
      this.backgroundWorkerVerifyNordicFW = new BackgroundWorker();
      this.progressBarupdate = new CustomProgressBar.CustomProgressBar();
      this.labelupdatestatus = new Label();
      this.labeldevstatus1 = new Label();
      this.labeldevstatus2 = new Label();
      this.buttonUpdate = new MyButton();
      this.buttonCancel = new MyButton();
      this.labelpoweroff = new Label();
      this.SuspendLayout();
      componentResourceManager.ApplyResources((object) this.labelHeader, "labelHeader");
      this.labelHeader.BackColor = Color.Transparent;
      this.labelHeader.Name = "labelHeader";
      this.labelHeader.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelHeader.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelpluginDevice.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelpluginDevice, "labelpluginDevice");
      this.labelpluginDevice.Name = "labelpluginDevice";
      this.labelpluginDevice.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelpluginDevice.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelPromptMessage.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelPromptMessage, "labelPromptMessage");
      this.labelPromptMessage.Name = "labelPromptMessage";
      this.labelPromptMessage.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelPromptMessage.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelUpdateprogress.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelUpdateprogress, "labelUpdateprogress");
      this.labelUpdateprogress.Name = "labelUpdateprogress";
      this.labelUpdateprogress.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelUpdateprogress.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelUpdateInfor.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelUpdateInfor, "labelUpdateInfor");
      this.labelUpdateInfor.Name = "labelUpdateInfor";
      this.labelUpdateInfor.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelUpdateInfor.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      componentResourceManager.ApplyResources((object) this.labeltargetver, "labeltargetver");
      this.labeltargetver.BackColor = Color.Transparent;
      this.labeltargetver.Name = "labeltargetver";
      this.labeltargetver.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labeltargetver.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      componentResourceManager.ApplyResources((object) this.labelCurFWver, "labelCurFWver");
      this.labelCurFWver.BackColor = Color.Transparent;
      this.labelCurFWver.Name = "labelCurFWver";
      this.labelCurFWver.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelCurFWver.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.backgroundWorkerProcessFWData.WorkerReportsProgress = true;
      this.backgroundWorkerProcessFWData.WorkerSupportsCancellation = true;
      this.backgroundWorkerProcessFWData.DoWork += new DoWorkEventHandler(this.backgroundWorkerProcessFWData_DoWork);
      this.backgroundWorkerProcessFWData.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerProcessFWData_ProgressChanged);
      this.backgroundWorkerProcessFWData.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerProcessFWData_RunWorkerCompleted);
      this.backgroundWorkerCheckVer.DoWork += new DoWorkEventHandler(this.backgroundWorkerCheckVer_DoWork);
      this.backgroundWorkerCheckVer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerCheckVer_RunWorkerCompleted);
      this.backgroundWorkerEraseFlash.WorkerReportsProgress = true;
      this.backgroundWorkerEraseFlash.DoWork += new DoWorkEventHandler(this.backgroundWorkerEraseFlash_DoWork);
      this.backgroundWorkerEraseFlash.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerEraseFlash_ProgressChanged);
      this.backgroundWorkerEraseFlash.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerEraseFlash_RunWorkerCompleted);
      this.backgroundWorkerProgramFW.WorkerReportsProgress = true;
      this.backgroundWorkerProgramFW.DoWork += new DoWorkEventHandler(this.backgroundWorkerProgramFW_DoWork);
      this.backgroundWorkerProgramFW.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerProgramFW_ProgressChanged);
      this.backgroundWorkerProgramFW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerProgramFW_RunWorkerCompleted);
      this.backgroundWorkerVerify.WorkerReportsProgress = true;
      this.backgroundWorkerVerify.DoWork += new DoWorkEventHandler(this.backgroundWorkerVerify_DoWork);
      this.backgroundWorkerVerify.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerVerify_ProgressChanged);
      this.backgroundWorkerVerify.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerVerify_RunWorkerCompleted);
      this.timerbllistener.Interval = 35000;
      this.timerbllistener.Tick += new EventHandler(this.timerbllistener_Tick);
      this.timerblentersuccess.Interval = 20000;
      this.timerblentersuccess.Tick += new EventHandler(this.timerblentersuccess_Tick);
      this.backgroundWorkerCloseRestartDialog.DoWork += new DoWorkEventHandler(this.backgroundWorkerCloseRestartDialog_DoWork);
      this.backgroundWorkerCloseRestartDialog.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerCloseRestartDialog_RunWorkerCompleted);
      this.backgroundWorkerCheckFlashFWVer.DoWork += new DoWorkEventHandler(this.backgroundWorkerCheckFlashFWVer_DoWork);
      this.backgroundWorkerCheckFlashFWVer.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerCheckFlashFWVer_RunWorkerCompleted);
      this.backgroundWorkerNordicENTERBL.DoWork += new DoWorkEventHandler(this.backgroundWorkerNordicENTERBL_DoWork);
      this.backgroundWorkerNordicENTERBL.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerNordicENTERBL_RunWorkerCompleted);
      this.backgroundWorkerGetRegionInfor.DoWork += new DoWorkEventHandler(this.backgroundWorkerGetRegionInfor_DoWork);
      this.backgroundWorkerGetRegionInfor.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerGetRegionInfor_RunWorkerCompleted);
      this.backgroundWorkerNordicProgram.WorkerReportsProgress = true;
      this.backgroundWorkerNordicProgram.DoWork += new DoWorkEventHandler(this.backgroundWorkerNordicProgram_DoWork);
      this.backgroundWorkerNordicProgram.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorkerNordicProgram_ProgressChanged);
      this.backgroundWorkerNordicProgram.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerNordicProgram_RunWorkerCompleted);
      this.backgroundWorkerVerifyNordicFW.WorkerReportsProgress = true;
      this.backgroundWorkerVerifyNordicFW.DoWork += new DoWorkEventHandler(this.backgroundWorkerVerifyNordicFW_DoWork);
      this.backgroundWorkerVerifyNordicFW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorkerVerifyNordicFW_RunWorkerCompleted);
      this.progressBarupdate.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.progressBarupdate, "progressBarupdate");
      this.progressBarupdate.Maximum = 100;
      this.progressBarupdate.Minimum = 0;
      this.progressBarupdate.Name = "progressBarupdate";
      this.progressBarupdate.PercentageVisible = false;
      this.progressBarupdate.ProgressBarBorderColor = Color.Transparent;
      this.progressBarupdate.ProgressBarColor = Color.FromArgb(68, 214, 44);
      this.progressBarupdate.ProgressFont = new Font("Arial", 10f);
      this.progressBarupdate.Value = 0;
      this.progressBarupdate.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.progressBarupdate.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelupdatestatus.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelupdatestatus, "labelupdatestatus");
      this.labelupdatestatus.Name = "labelupdatestatus";
      this.labelupdatestatus.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labelupdatestatus.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labeldevstatus1.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labeldevstatus1, "labeldevstatus1");
      this.labeldevstatus1.Name = "labeldevstatus1";
      this.labeldevstatus1.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labeldevstatus1.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labeldevstatus2.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labeldevstatus2, "labeldevstatus2");
      this.labeldevstatus2.Name = "labeldevstatus2";
      this.labeldevstatus2.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.labeldevstatus2.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      componentResourceManager.ApplyResources((object) this.buttonUpdate, "buttonUpdate");
      this.buttonUpdate.BackColor = Color.White;
      this.buttonUpdate.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_update_disabled;
      this.buttonUpdate.Cursor = Cursors.Default;
      this.buttonUpdate.EnabledSet = true;
      this.buttonUpdate.FlatAppearance.BorderSize = 0;
      this.buttonUpdate.ForeColor = Color.White;
      this.buttonUpdate.Name = "buttonUpdate";
      this.buttonUpdate.TabStop = false;
      this.buttonUpdate.UseVisualStyleBackColor = false;
      this.buttonUpdate.Click += new EventHandler(this.buttonUpdate_Click);
      this.buttonUpdate.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.buttonUpdate.MouseEnter += new EventHandler(this.buttonUpdate_MouseEnter);
      this.buttonUpdate.MouseLeave += new EventHandler(this.buttonUpdate_MouseLeave);
      this.buttonUpdate.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      componentResourceManager.ApplyResources((object) this.buttonCancel, "buttonCancel");
      this.buttonCancel.BackColor = Color.White;
      this.buttonCancel.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.button_cancel_normal;
      this.buttonCancel.Cursor = Cursors.Default;
      this.buttonCancel.EnabledSet = true;
      this.buttonCancel.FlatAppearance.BorderSize = 0;
      this.buttonCancel.ForeColor = Color.White;
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.TabStop = false;
      this.buttonCancel.UseVisualStyleBackColor = false;
      this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
      this.buttonCancel.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.buttonCancel.MouseEnter += new EventHandler(this.buttonCancel_MouseEnter);
      this.buttonCancel.MouseLeave += new EventHandler(this.buttonCancel_MouseLeave);
      this.buttonCancel.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.labelpoweroff.BackColor = Color.Transparent;
      componentResourceManager.ApplyResources((object) this.labelpoweroff, "labelpoweroff");
      this.labelpoweroff.Name = "labelpoweroff";
      componentResourceManager.ApplyResources((object) this, "$this");
      this.AutoScaleMode = AutoScaleMode.Dpi;
      this.AutoValidate = AutoValidate.Disable;
      this.BackgroundImage = (Image) WirelessSetFWU.Properties.Resources.BWV3miniConn;
      this.Controls.Add((Control) this.labelpoweroff);
      this.Controls.Add((Control) this.labeldevstatus2);
      this.Controls.Add((Control) this.labeldevstatus1);
      this.Controls.Add((Control) this.labelupdatestatus);
      this.Controls.Add((Control) this.labeltargetver);
      this.Controls.Add((Control) this.labelCurFWver);
      this.Controls.Add((Control) this.buttonUpdate);
      this.Controls.Add((Control) this.buttonCancel);
      this.Controls.Add((Control) this.progressBarupdate);
      this.Controls.Add((Control) this.labelPromptMessage);
      this.Controls.Add((Control) this.labelUpdateprogress);
      this.Controls.Add((Control) this.labelUpdateInfor);
      this.Controls.Add((Control) this.labelpluginDevice);
      this.Controls.Add((Control) this.labelHeader);
      this.FormBorderStyle = FormBorderStyle.None;
      this.MaximizeBox = false;
      this.Name = nameof (FormFWUStep1);
      this.FormClosed += new FormClosedEventHandler(this.FormFWUStep1_FormClosed);
      this.Load += new EventHandler(this.FormFWUStep1_Load);
      this.Shown += new EventHandler(this.FormFWUStep1_Shown);
      this.MouseDown += new MouseEventHandler(this.labelpluginDevice_MouseDown);
      this.MouseMove += new MouseEventHandler(this.labelpluginDevice_MouseMove);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
