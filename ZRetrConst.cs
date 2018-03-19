using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZRetrConst
{
    #region типы
    // Тип устройства
    public enum ZDEV_TYPE
    {
	    ZDT_UNDEF = 0,
	    ZDT_Z397,           // Z-397
	    ZDT_Z397_G_NORM,    // Z-397 Guard в режиме "Normal"
	    ZDT_Z397_G_ADV,     // Z-397 Guard в режиме "Advanced"
	    ZDT_Z2U,            // Z-2 USB
	    ZDT_M3A,            // Matrix III Rd-All
	    ZDT_Z2M,            // Z-2 USB MF
	    ZDT_M3N,            // Matrix III Net
	    ZDT_CPZ2MF,         // CP-Z-2MF
	    ZDT_Z2EHR           // Z-2 EHR
    };
    #endregion

    class ZRetrIntf
    {
        #region Константы
        public const int ZRS_MAX_DEV = 8;
        public const int ZRS_SEARCH_PORT = 9000;
        public const string ZRS_SEARCH_REQ = "SEEK Z397IP";
        #endregion
    }
}
