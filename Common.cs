using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WirelessSetFWU
{
  public class Common
  {
    public static bool fordummy = false;
    public static PageIndex NextPage = PageIndex.FormGuider;
    public static string resfile = Path.GetDirectoryName(Application.ExecutablePath) + "\\DeviceUpdater.resources";
    public static bool fExiting = false;
    public static UpdateInfo updateInfo = (UpdateInfo) null;
    public const int FIRMWARE_DELAY = 0;
    public static long filelen = 0;
    public static ushort PACKLEN = 64;
    public static ushort PAGESIZE = 4096;
    public static Color lightgreen = Color.FromArgb(7584512);
    public static Color lightgray = Color.FromArgb(7763574);
    public static uint StartAddr = 0;
    public static uint EndAddr = 0;
    public static bool IsLowBattery = false;
    public static bool LogEnabled = false;
    public static bool DevFWNeedUpdate = false;
    public static bool FlashFWNeedUpdate = false;
    public static Image background = (Image) null;
    public static Color btnfontcolor = Color.FromArgb(2236962);
    public static Color greendarktheme = Color.FromArgb(4511276);
    public static int DongleStatus = 0;
    public static int Mousestatus = 0;
    public static int KBstatus = 0;
    public static int hspacebtnbottom = 33;
    public static int wspacebutton = 15;
    public static int logowidth = 130;
    public static string curdevver = "";
    public static byte RegionID = 0;
    public static byte total = 0;
    public static byte type = 0;
    public static uint regionsize = 0;
    public static int MAX_RETRY = 10;
    public static ushort crc;

    public static byte[] CRC16(byte[] data)
    {
      int length = data.Length;
      if (length <= 0)
        return new byte[2];
      for (int index = 0; index < length; ++index)
      {
        Common.crc = (ushort) ((uint) (byte) ((uint) Common.crc >> 8) | (uint) Common.crc << 8);
        Common.crc ^= (ushort) data[index];
        Common.crc ^= (ushort) ((uint) (byte) ((uint) Common.crc & (uint) byte.MaxValue) >> 4);
        Common.crc ^= (ushort) ((int) Common.crc << 8 << 4);
        Common.crc ^= (ushort) (((int) Common.crc & (int) byte.MaxValue) << 4 << 1);
      }
      return new byte[2]
      {
        (byte) (((int) Common.crc & 65280) >> 8),
        (byte) ((uint) Common.crc & (uint) byte.MaxValue)
      };
    }

    public struct VidPid
    {
      private string strVID;
      private string strPID;
      private bool bconnect;

      public VidPid(string strVID, string strPID, bool bconnect)
      {
        this.strVID = strVID;
        this.strPID = strPID;
        this.bconnect = bconnect;
      }

      public VidPid(string strVID, string strPID)
      {
        this.strVID = strVID;
        this.strPID = strPID;
        this.bconnect = true;
      }

      public string GetVID() => this.strVID;

      public string GetPID() => this.strPID;

      public bool GetDevType() => this.bconnect;

      public void SetDevType(bool bconnect) => this.bconnect = bconnect;

      public static bool operator ==(Common.VidPid vidPid1, Common.VidPid vidPid2) => vidPid1.strVID == vidPid2.strVID && vidPid1.strPID == vidPid2.strPID;

      public static bool operator !=(Common.VidPid vidPid1, Common.VidPid vidPid2) => !(vidPid1 == vidPid2);

      public override bool Equals(object obj) => obj is Common.VidPid vidPid && this == vidPid;

      public override int GetHashCode() => 0;
    }
  }
}
