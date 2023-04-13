using System;
using System.Drawing;
using System.Globalization;
using System.Resources;

namespace WirelessSetFWU
{
  public class UpdateInfo
  {
    private ResourceSet rs = new ResourceSet(Common.resfile);
    private bool internalengnieer;
    private ushort uDataPacketSize = 8;
    private ushort uPageSize = 512;
    private bool logfile;
    private int curdevidx = -1;
    private string strActDevFWVer;
    private string strActFlashFWVer;
    private int iSameDevVersion = -1;
    public const uint RETRY_TIMES = 3;
    private int dongleupdateretry;
    private int headsetupdateretry;
    private int pairretry;
    private int iSameFlashVer = -1;
    private int supportdevnum;

    public bool InternalEngnieer
    {
      get => this.internalengnieer;
      set => this.internalengnieer = value;
    }

    public ushort DataPacketSize
    {
      get => this.uDataPacketSize;
      set => this.uDataPacketSize = value;
    }

    public ushort PageSize
    {
      get => this.uPageSize;
      set => this.uPageSize = value;
    }

    public bool LogFile
    {
      get => this.logfile;
      set => this.logfile = value;
    }

    public int CurDevIndex
    {
      get => this.curdevidx;
      set => this.curdevidx = value;
    }

    public string ActDevFWVer
    {
      get => this.strActDevFWVer;
      set => this.strActDevFWVer = value;
    }

    public string ActFlashFWVer
    {
      get => this.strActFlashFWVer;
      set => this.strActFlashFWVer = value;
    }

    public int DevSameVer
    {
      get => this.iSameDevVersion;
      set => this.iSameDevVersion = value;
    }

    public int DongleRetry
    {
      get => this.dongleupdateretry;
      set => this.dongleupdateretry = value;
    }

    public int HeadsetRetry
    {
      get => this.headsetupdateretry;
      set => this.headsetupdateretry = value;
    }

    public int PairRetry
    {
      get => this.pairretry;
      set => this.pairretry = value;
    }

    public int FlashSameVer
    {
      get => this.iSameFlashVer;
      set => this.iSameFlashVer = value;
    }

    public int SupportDevCount => this.supportdevnum;

    public UpdateInfo()
    {
      Common.background = this.rs.GetObject("DevBackground") == null ? (Image) null : (Image) this.rs.GetObject("DevBackground");
      this.supportdevnum = this.rs.GetObject("DeviceNum") != null ? Convert.ToInt32(this.rs.GetObject("DeviceNum")) : 0;
      this.internalengnieer = this.rs.GetObject("Internal") == null && Convert.ToInt32(this.rs.GetObject("Internal")) == 1;
      if (Convert.ToInt32(this.rs.GetObject(nameof (LogFile))) == 1)
        this.logfile = true;
      else
        this.logfile = false;
    }

    ~UpdateInfo()
    {
    }

    public string GetVID(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("VID" + (object) devidx).ToString() : this.rs.GetObject("VID").ToString();
    }

    public string GetPID(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("PID" + (object) devidx).ToString() : this.rs.GetObject("PID").ToString();
    }

    public string GetBLVID(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("BLVID" + (object) devidx).ToString() : this.rs.GetObject("BLVID").ToString();
    }

    public string GetBLPID(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("BLPID" + (object) devidx).ToString() : this.rs.GetObject("BLPID").ToString();
    }

    public string GetBLBCDDevPID(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("BCDPID_BL" + (object) devidx).ToString() : this.rs.GetObject("BCDPID_BL").ToString();
    }

    public int GetDevType(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return -1;
      return this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("DevType" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("DevType"));
    }

    public string GetDeviceName(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return (this.supportdevnum != 0 ? this.rs.GetObject("DevName" + (object) devidx).ToString() : this.rs.GetObject("DevName").ToString()).Replace("\0", "");
    }

    public string GetProductName(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? "" : (this.supportdevnum != 0 ? this.rs.GetObject("ProductName" + (object) devidx).ToString() : this.rs.GetObject("ProductName").ToString());

    public int GetFWType(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("FWType" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("FWType")));

    public int GetDevReportType(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("ReportType" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("ReportType")));

    public int GetFeatureRptLen(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("FeatureReportLen" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("FeatureReportLen")));

    public int GetInputRptLen(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("InputReportLen" + (object) devidx)) : (this.rs.GetObject("InputReportLen") == null || !(this.rs.GetObject("InputReportLen").ToString() != "") ? 0 : Convert.ToInt32(this.rs.GetObject("InputReportLen"))));

    public int GetOutputRptLen(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("OutputReportLen" + (object) devidx)) : (this.rs.GetObject("OutputReportLen") == null || !(this.rs.GetObject("OutputReportLen").ToString() != "") ? 0 : Convert.ToInt32(this.rs.GetObject("OutputReportLen"))));

    public int GetDevFWType(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? -1 : (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("FWType" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("FWType")));

    public string GetDevFWVer(int devidx) => devidx <= 0 || devidx > this.supportdevnum ? "" : (this.supportdevnum != 0 ? this.rs.GetObject("DevFWVer" + (object) devidx).ToString() : this.rs.GetObject("DevFWVer").ToString());

    public float GetBLVer(int devidx)
    {
      CultureInfo provider = new CultureInfo("en-US");
      return devidx <= 0 || devidx > this.supportdevnum ? -1f : (this.supportdevnum != 0 ? Convert.ToSingle(this.rs.GetObject("BLVER" + (object) devidx), (IFormatProvider) provider) : Convert.ToSingle(this.rs.GetObject("BLVER"), (IFormatProvider) provider));
    }

    public bool NeedBackToDefault(int devidx)
    {
      bool flag = false;
      return devidx <= 0 || devidx > this.supportdevnum ? flag : (this.supportdevnum != 0 ? this.rs.GetObject("BacktoDefault" + (object) devidx).ToString() == "1" : this.rs.GetObject("BacktoDefault") != null && this.rs.GetObject("BacktoDefault").ToString() != "" && this.rs.GetObject("BacktoDefault").ToString() == "1");
    }

    public bool IsVerifyCheckSum(int devidx) => devidx > 0 && devidx <= this.supportdevnum && (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("VerifyChecksum" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("VerifyChecksum"))) == 1;

    public bool IsUpdateFlashFW(int devidx) => devidx > 0 && devidx <= this.supportdevnum && (this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("FlashFWUpdate" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("FlashFWUpdate"))) == 1;

    public int GetFlashFWType(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return -1;
      return this.supportdevnum != 0 ? Convert.ToInt32(this.rs.GetObject("FlashFWType" + (object) devidx)) : Convert.ToInt32(this.rs.GetObject("FlashFWType"));
    }

    public string GetFlashFWVer(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      return this.supportdevnum != 0 ? this.rs.GetObject("FlashFWVer" + (object) devidx).ToString() : this.rs.GetObject("FlashFWVer").ToString();
    }

    public uint GetDevFWLineNum(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return 0;
      return this.supportdevnum != 0 ? Convert.ToUInt32(this.rs.GetObject("Dev" + (object) devidx + "FWLineNum")) : Convert.ToUInt32(this.rs.GetObject("DevFWLineNum"));
    }

    public string GetDevFWLine(int devidx, int linenum)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      string devFwLine;
      if (this.supportdevnum == 0)
        devFwLine = this.rs.GetObject("DevFWLine" + (object) linenum).ToString();
      else
        devFwLine = this.rs.GetObject("Dev" + (object) devidx + "FWLine" + (object) linenum).ToString();
      return devFwLine;
    }

    public uint GetFlashFWLineNum(int devidx)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return 0;
      return this.supportdevnum != 0 ? Convert.ToUInt32(this.rs.GetObject("Flash" + (object) devidx + "FWLineNum")) : Convert.ToUInt32(this.rs.GetObject("FlashFWLineNum"));
    }

    public string GetFlashFWLine(int devidx, int linenum)
    {
      if (devidx <= 0 || devidx > this.supportdevnum)
        return "";
      string flashFwLine;
      if (this.supportdevnum == 0)
        flashFwLine = this.rs.GetObject("FlashFWLine" + (object) linenum).ToString();
      else
        flashFwLine = this.rs.GetObject("Flash" + (object) devidx + "FWLine" + (object) linenum).ToString();
      return flashFwLine;
    }
  }
}
