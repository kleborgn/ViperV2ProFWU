using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace WirelessSetFWU
{
  internal class CLocalize
  {
    private static readonly IList<CLocalize.structLocale> LocaleArray = (IList<CLocalize.structLocale>) new ReadOnlyCollection<CLocalize.structLocale>((IList<CLocalize.structLocale>) new CLocalize.structLocale[10]
    {
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_DEFAULT, "English", "", "Arial", (short) 1033),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_CHINESESIMPLIFIED, "ChineseSimplified", "zh-CN", "Simhei", (short) 2052),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_CHINESETRADITIONAL, "ChineseTraditional", "zh-CHT", "Simhei", (short) 1028),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_FRENCH, "French", "fr-FR", "Arial", (short) 1036),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_GERMAN, "German", "de-DE", "Arial", (short) 1031),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_KOREAN, "Korean", "ko-KR", "", (short) 1042),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_JAPANESE, "Japanese", "ja-JP", "", (short) 1041),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_RUSSIAN, "Russian", "ru-RU", "", (short) 1049),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_SPANISH, "Spanish", "es-ES", "", (short) 1034),
      new CLocalize.structLocale(CLocalize.enumLocale.LOCALE_PORTUGUESE, "Portuguese", "pt-BR", "", (short) 1046)
    });
    private const short LANGUAGE_DEFAULT = 1033;
    private static CLocalize instance = (CLocalize) null;
    private static CultureInfo m_Culture = (CultureInfo) null;
    private static CLocalize.structLocale m_currentLocale = new CLocalize.structLocale();

    [DllImport("kernel32.dll")]
    public static extern short GetSystemDefaultLangID();

    private CLocalize()
    {
      CLocalize.structLocale localeInfo = this.getLocaleInfo(CLocalize.GetSystemDefaultLangID());
      CLocalize.m_currentLocale = localeInfo.LanguageKey != (short) 0 ? localeInfo : CLocalize.LocaleArray[0];
      this.setCulture(CLocalize.m_currentLocale.Resource);
    }

    private bool setCulture(string userCulture)
    {
      try
      {
        CLocalize.m_Culture = new CultureInfo(userCulture);
      }
      catch (Exception ex)
      {
        return false;
      }
      return true;
    }

    public CultureInfo getCulture() => CLocalize.m_Culture;

    public static CLocalize getInstance()
    {
      if (CLocalize.instance == null)
        CLocalize.instance = new CLocalize();
      return CLocalize.instance;
    }

    private CLocalize.structLocale getLocaleInfo(short language)
    {
      CLocalize.structLocale localeInfo = new CLocalize.structLocale();
      byte[] bytes1 = BitConverter.GetBytes(language);
      int num = CLocalize.LocaleArray.Count<CLocalize.structLocale>();
      for (int index = 0; index < num; ++index)
      {
        byte[] bytes2 = BitConverter.GetBytes(CLocalize.LocaleArray[index].LanguageKey);
        if ((int) bytes1[0] == (int) bytes2[0])
          localeInfo = bytes1[0] != (byte) 4 ? CLocalize.LocaleArray[index] : (bytes1[1] == (byte) 4 || bytes1[1] == (byte) 12 ? CLocalize.LocaleArray[2] : CLocalize.LocaleArray[1]);
      }
      return localeInfo;
    }

    public enum enumLocale
    {
      LOCALE_DEFAULT = 1,
      LOCALE_ENGLISH = 1,
      LOCALE_CHINESESIMPLIFIED = 2,
      LOCALE_CHINESETRADITIONAL = 3,
      LOCALE_FRENCH = 4,
      LOCALE_KOREAN = 5,
      LOCALE_JAPANESE = 6,
      LOCALE_GERMAN = 7,
      LOCALE_RUSSIAN = 8,
      LOCALE_SPANISH = 9,
      LOCALE_PORTUGUESE = 10, // 0x0000000A
    }

    public struct structLocale
    {
      private CLocalize.enumLocale id;
      private string sLanguage;
      private string sResource;
      private string sFont;
      private short slanguagekey;

      public structLocale(
        CLocalize.enumLocale id,
        string sLanguage,
        string sResource,
        string sFont,
        short slanguagekey)
      {
        this.id = id;
        this.sLanguage = sLanguage;
        this.sResource = sResource;
        this.sFont = sFont;
        this.slanguagekey = slanguagekey;
      }

      public CLocalize.enumLocale Id => this.id;

      public string Language => this.sLanguage;

      public string Resource => this.sResource;

      public string Font => this.sFont;

      public short LanguageKey => this.slanguagekey;
    }
  }
}
