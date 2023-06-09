﻿namespace WirelessSetFWU
{
  public enum eState
  {
    STATE_NULL = 0,
    STATE_WAITING_DEVICE = 1,
    STATE_WAITING_BOOTLOADER = 2,
    STATE_PROCESSING_DATA = 3,
    STATE_PROCESSING_DATA_PASS = 4,
    STATE_PROCESSINGDATA_FAIL = 5,
    STATE_ERASING_FLASH = 6,
    STATE_ERASING_FLASH_PASS = 7,
    STATE_ERASING_FLASH_FAIL = 8,
    STATE_DOWNLOADING_DATA = 9,
    STATE_DOWNLOADING_DATA_PASS = 10, // 0x0000000A
    STATE_DOWNLOADING_DATA_FAIL = 11, // 0x0000000B
    STATE_VERIFYING_FIRMWARE = 12, // 0x0000000C
    STATE_VERIFYING_FIRMWARE_PASS = 13, // 0x0000000D
    STATE_VERIFYING_FIRMWARE_FAIL = 14, // 0x0000000E
    STATE_ENTER_BL = 15, // 0x0000000F
    STATE_EXIT_BL = 16, // 0x00000010
    STATE_EXIT_NordicBL = 17, // 0x00000011
    STATE_FLASHFW_BEFORE = 18, // 0x00000012
    STATE_FLASHFW_AFTER = 19, // 0x00000013
    STATE_SUCCESS = 20, // 0x00000014
    STATE_FAILED = 21, // 0x00000015
    STATE_LAST = 21, // 0x00000015
    NUM_STATES = 22, // 0x00000016
  }
}
