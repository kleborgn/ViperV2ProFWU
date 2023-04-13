using System;
using System.IO;
using System.Windows.Forms;

namespace WirelessSetFWU
{
  internal class Logger
  {
    public const short LOG_NULL = 0;
    public const short LOG_INFO = 1;
    public const short LOG_ERROR = 2;
    public const short LOG_WARNING = 3;
    private static Logger instance = (Logger) null;
    private static string logFile = (string) null;
    private static string logFileName = string.Format("DeviceUpdater_{0}.log", (object) DateTime.Now.ToShortDateString().Replace("/", "_"));

    private Logger() => Logger.logFile = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Logger.logFileName);

    public static Logger getInstance()
    {
      if (Logger.instance == null)
      {
        Logger.instance = new Logger();
        if (Common.LogEnabled)
        {
          try
          {
            using (StreamWriter text = File.CreateText(Logger.logFile))
              text.WriteLine(string.Format("{0} : ------------ Starting {1} Update ------------", (object) DateTime.Now.ToLongTimeString(), (object) Common.updateInfo.GetProductName(Common.updateInfo.CurDevIndex)));
          }
          catch
          {
            try
            {
              string str = "D:\\temp\\log";
              Directory.CreateDirectory(str);
              Logger.logFile = Path.Combine(str, Logger.logFileName);
              using (StreamWriter text = File.CreateText(Logger.logFile))
                text.WriteLine(DateTime.Now.ToLongTimeString() + " : ------------ Starting Update ------------");
            }
            catch
            {
            }
          }
        }
      }
      return Logger.instance;
    }

    public void setLogFile(string fileName) => Logger.logFile = fileName;

    public void writeLog(string msg, short msgType)
    {
      if (!Common.LogEnabled)
        return;
      string longTimeString = DateTime.Now.ToLongTimeString();
      using (StreamWriter streamWriter = File.AppendText(Logger.logFile))
      {
        switch (msgType)
        {
          case 1:
            streamWriter.WriteLine(longTimeString + " : Info >> " + msg);
            break;
          case 2:
            streamWriter.WriteLine(longTimeString + " : ERROR >> " + msg);
            break;
          case 3:
            streamWriter.WriteLine(longTimeString + " : Warning >> " + msg);
            break;
          default:
            streamWriter.WriteLine(msg);
            break;
        }
        streamWriter.Close();
      }
    }
  }
}
