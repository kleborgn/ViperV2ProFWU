using System;

namespace WirelessSetFWU
{
  public class DeviceListenerEvent : EventArgs
  {
    private Common.VidPid vidPid;
    private bool fConnected;

    public DeviceListenerEvent(Common.VidPid vidPid, bool fConnected)
    {
      this.vidPid = vidPid;
      this.fConnected = fConnected;
    }

    public Common.VidPid GetVidPid() => this.vidPid;

    public bool IsConnected() => this.fConnected;
  }
}
