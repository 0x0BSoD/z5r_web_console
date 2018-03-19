using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

using ZP_OPEN_PARAMS = ZPort.ZP_PORT_OPEN_PARAMS;
using ZP_N_EXIST_INFO = ZPort.ZP_DDN_PORT_INFO;
using ZP_N_CHANGE_INFO = ZPort.ZP_DDN_PORT_INFO;
using ZP_N_EXIST_DEVINFO = ZPort.ZP_DDN_DEVICE_INFO;
using ZP_N_CHANGE_DEVINFO = ZPort.ZP_DDN_DEVICE_INFO;
using ZP_NOTIFY_SETTINGS = ZPort.ZP_DD_NOTIFY_SETTINGS;
using ZP_S_DEVICE = ZPort.ZP_USB_DEVICE;
using ZP_DETECTOR_SETTINGS = ZPort.ZP_DD_GLOBAL_SETTINGS;

namespace ZPort
{
    #region Типы
    // Типы считывателей
    public enum ZP_PORT_TYPE
    {
        ZP_PORT_UNDEF = 0,
        ZP_PORT_COM,        // Com-порт
        ZP_PORT_FT,         // Ft-порт (через ftd2xx.dll по с/н USB)
        ZP_PORT_IP,         // IP-порт (конвертер в режиме SERVER)
        ZP_PORT_IPS         // IPS-порт (конвертер в режиме CLIENT)
    }
    // Состояние подключения
    public enum ZP_CONNECTION_STATUS
    {
        ZP_CS_DISCONNECTED = 0, // Отключен
        ZP_CS_CONNECTED,        // Подключен
        ZP_CS_CONNECTING,       // Подключаемся... (при первом подключении)
        ZP_CS_RESTORATION       // Восстанавливаем связь...
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_DEVICEPARSEPROC([In] Byte[] pReply, UInt32 nCount, ref bool nPartially, IntPtr pInfo,
        [In, Out] ZP_PORT_INFO[] pPort, Int32 nArrLen, ref Int32 nPortCount);
    #endregion

    #region Устаревшие типы
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_ENUMPORTSPROC(ref ZP_PORT_INFO pInfo, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_ENUMDEVICEPROC(IntPtr pInfo, ref ZP_PORT_INFO pPort, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZP_NOTIFYPROC(UInt32 nMsg, IntPtr nMsgParam, IntPtr pUserData);
    #endregion

    #region структуры

    // Параметры открытия порта (для функции ZP_Open)
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_PORT_OPEN_PARAMS
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szName;                           // Имя порта
        public ZP_PORT_TYPE nType;                      // Тип порта
        public UInt32 nBaud;                            // Скорость порта
        public SByte nEvChar;                           // Символ-признак конца передачи (если =0, нет символа)
        public Byte nStopBits;                          // Стоповые биты (ONESTOPBIT=0, ONE5STOPBITS=1, TWOSTOPBITS=2)
        public UInt32 nConnectTimeout;                  // Тайм-аут подключения по TCP
        public UInt32 nRestorePeriod;                   // Период с которым будут осуществляться попытки восстановить утерянную TCP-связь
        public UInt32 nFlags;                           // Флаги ZP_PF_...

        public ZP_PORT_OPEN_PARAMS(string _sName, ZP_PORT_TYPE _nType, UInt32 _nBaud, SByte _nEvChar,
            Byte _nStopBits = 2,
            UInt32 _nConnectTimeout = 0,
            UInt32 _nRestorePeriod = 0,
            UInt32 _nFlags = 0)
        {
            szName = _sName;
            nType = _nType;
            nBaud = _nBaud;
            nEvChar = _nEvChar;
            nStopBits = _nStopBits;
            nConnectTimeout = _nConnectTimeout;
            nRestorePeriod = _nRestorePeriod;
            nFlags = _nFlags;
        }
    }
    // Информация о порте
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_PORT_INFO
    {
        public ZP_PORT_TYPE nType;      // Тип порта
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szName;           // Имя порта
        public UInt32 nFlags;           // Флаги порта (ZPIntf.ZP_PF_BUSY,ZP_PF_USER,ZP_PF_BUSY2)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string szFriendly;       // Дружественное имя порта
        public UInt32 nDevTypes;        // Маска типов устройств
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szOwner;          // Владелец порта (для функции ZP_EnumIpDevices)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_PORT_ADDR
    {
        public ZP_PORT_TYPE nType;                      // Тип порта
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pName;                            // Имя порта
        public UInt32 nDevTypes;                        // Маска тивов устройств
    }
    // Настройки ожидания исполнения функций
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZP_WAIT_SETTINGS
    {
        public UInt32 nReplyTimeout;                    // Тайм-аут ожидания ответа на запрос конвертеру
        public Int32 nMaxTries;                         // Количество попыток отправить запрос
        public IntPtr hAbortEvent;                      // Дескриптор стандартного объекта Event для прерывания функции. Если объект установлен в сигнальное состояние, функция возвращает E_ABORT
        public UInt32 nReplyTimeout0;                   // Тайм-аут ожидания первого символа ответа
        public UInt32 nCheckPeriod;                     // Период проверки порта в мс (если =0 или =INFINITE, то по RX-событию)
        public UInt32 nConnectTimeout;                  // Тайм-аут подключения по TCP
        public UInt32 nRestorePeriod;                   // Период с которым будут осуществляться попытки восстановить утерянную TCP-связь

        public ZP_WAIT_SETTINGS(UInt32 _nReplyTimeout, Int32 _nMaxTries, IntPtr _hAbortEvent, UInt32 _nReplyTimeout0, UInt32 _nCheckPeriod,
            UInt32 _nConnectTimeout, UInt32 _nRestorePeriod)
        {
            nReplyTimeout = _nReplyTimeout;
            nMaxTries = _nMaxTries;
            hAbortEvent = _hAbortEvent;
            nReplyTimeout0 = _nReplyTimeout0;
            nCheckPeriod = _nCheckPeriod;
            nConnectTimeout = _nConnectTimeout;
            nRestorePeriod = _nRestorePeriod;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DEVICE_INFO
    {
        public UInt32 cbSize;
        public UInt32 nTypeId;
        public UInt32 nModel;
        public UInt32 nSn;
        public UInt32 nVersion;
    }
    // Информация о порте
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DDN_PORT_INFO
    {
        public ZP_PORT_INFO rPort;
        //[MarshalAs(UnmanagedType.LPArray)]
        //public IntPtr[] aDevs;
        public IntPtr aDevs;
        public Int32 nDevCount;
        public UInt32 nChangeMask;     // Маска изменений
    }
    // Информация о устройстве
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DDN_DEVICE_INFO
    {
        //[MarshalAs(UnmanagedType.LPStruct)]
        //public ZP_DEVICE_INFO pInfo;                    // Информация о устройстве
        public IntPtr pInfo;                    // Информация о устройстве
        //[MarshalAs(UnmanagedType.LPArray)]
        //public ZP_PORT_INFO[] aPorts;
        public IntPtr aPorts;
        public Int32 nPortCount;
        UInt32 nChangeMask;
    }
    // Параметры для уведомлений
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZP_DD_NOTIFY_SETTINGS
    {
        public UInt32 nNMask;                           // Маска типов уведомлений (см. _ZP_NOTIFY_SETTINGS в ZPort.h)
        public SafeWaitHandle hEvent;                   // Событие (объект синхронизации)
        public IntPtr hWindow;                          // Окно, принимиющее сообщение nWndMsgId
        public UInt32 nWndMsgId;                        // Сообщение для отправки окну hWnd
        public UInt32 nSDevTypes;                       // Маска тивов устройств, подключенных к последовательному порту
        public UInt32 nIpDevTypes;                      // Маска тивов Ip-устройств
        //[MarshalAs(UnmanagedType.LPArray)]
        //public UInt16[] aIps;                           // Массив TCP-портов для подключения конвертеров в режиме "CLIENT" (если NULL, то не используется)
        public IntPtr aIps;                             // Массив TCP-портов для подключения конвертеров в режиме "CLIENT" (если NULL, то не используется)
        public Int32 nIpsCount;                         // Количество TCP-портов
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_DEVICE
    {
        public UInt32 nTypeId;                          // Тип устройства
        //[MarshalAs(UnmanagedType.LPArray)]
        //public Byte[] pReqData;                       // Данные запроса (может быть NULL)
        public IntPtr pReqData;                         // Данные запроса (может быть NULL)
        public UInt32 nReqSize;                         // Количество байт в запросе
        public ZP_DEVICEPARSEPROC pfnParse;             // Функция разбора ответа
        public UInt32 nDevInfoSize;                     // Размер структуры ZP_DEVICE_INFO
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_IP_DEVICE
    {
        public ZP_DEVICE rBase;
        public UInt16 nReqPort;                         // Порт для UDP-запроса
        public Int32 nMaxPort;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_USB_DEVICE
    {
        public ZP_DEVICE rBase;
        //[MarshalAs(UnmanagedType.LPArray)]
        //public UInt32[] pVidPids;                     // Vid,Pid USB-устройств MAKELONG(vid, pid)
        public IntPtr pVidPids;                         // Vid,Pid USB-устройств MAKELONG(vid, pid)
        public Int32 nVidPidCount;                      // Количество пар Vid,Pid
        public UInt32 nBaud;                            // Скорость порта
        public SByte chEvent;                           // Символ-признак конца передачи (если =0, нет символа)
        public Byte nStopBits;                          // Стоповые биты (ONESTOPBIT=0, ONE5STOPBITS=1, TWOSTOPBITS=2)
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszBDesc;                         // Описание устройства, предоставленное шиной (DEVPKEY_Device_BusReportedDeviceDesc)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZP_SEARCH_PARAMS
    {
        public UInt32 nDevMask;                         // Маска устройств для сканирования портов (=0 не искать, =0xffffffff искать всё)
        public UInt32 nIpDevMask;                       // Маска IP устройств, сканируемых с помощью UDP-запроса (=0 не искать, =0xffffffff искать всё)
        //[MarshalAs(UnmanagedType.LPArray)]
        //public ZP_PORT_ADDR[] pPorts;                 // Список портов
        public IntPtr pPorts;                           // Список портов
        public Int32 nPCount;                           // Размер списка портов
        public UInt32 nFlags;                           // Флаги ZP_SF_...
        //[MarshalAs(UnmanagedType.LPStruct)]
        //public ZP_WAIT_SETTINGS pWait;                  // Параметры ожидания для сканирования портов. Может быть =NULL.
        public IntPtr pWait;                            // Параметры ожидания для сканирования портов. Может быть =NULL.
        public UInt32 nIpReqTimeout;                    // Тайм-аут ожидания ответа от ip-устройства при опросе по UDP
        public Int32 nIpReqMaxTries;                    // Количество попыток опроса ip-устройства по UDP
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZP_DD_GLOBAL_SETTINGS
    {
        public UInt32 nCheckUsbPeriod;                  // Период проверки состояния USB-портов (в миллисекундах) (=0 по умолчанию 5000)
        public UInt32 nCheckIpPeriod;                   // Период проверки состояния IP-портов (в миллисекундах) (=0 по умолчанию 15000)
        public UInt32 nScanDevPeriod;                   // Период сканирования устройств на USB- и IP-портах (в миллисекундах) (=0 по умолчанию никогда=INFINITE)

        public UInt32 nIpReqTimeout;                    // Тайм-аут ожидания ответа от ip-устройства при опросе по UDP
        public Int32 nIpReqMaxTries;                    // Количество попыток опроса ip-устройства по UDP
        public ZP_WAIT_SETTINGS rScanWS;                // Параметры ожидания при сканировании портов
    }

    #endregion

    class ZPIntf
    {
        #region Версия ZPort API
        public const int ZP_SDK_VER_MAJOR = 1;
        public const int ZP_SDK_VER_MINOR = 18;
        #endregion

        #region Коды ошибок
        public const int S_OK = 0;                                            // Операция выполнена успешно
        public const int E_FAIL = unchecked((int)0x80000008);                 // Другая ошибка
        public const int E_OUTOFMEMORY = unchecked((int)0x80000002);          // Недостаточно памяти для обработки команды
        public const int E_INVALIDARG = unchecked((int)0x80000003);           // Неправильный параметр
        public const int E_NOINTERFACE = unchecked((int)0x80000004);          // Функция не поддерживается
        public const int E_ABORT = unchecked((int)0x80000007);                // Функция прервана (см.описание ZP_WAIT_SETTINGS)
        public const int E_ACCESSDENIED = unchecked((int)0x80000009);         // Ошибка доступа

        public const int ZP_S_CANCELLED = unchecked((int)0x00040201);         // Отменено пользователем
        public const int ZP_S_NOTFOUND = unchecked((int)0x00040202);          // Не найден (для функции ZP_FindSerialDevice)
        public const int ZP_S_TIMEOUT = unchecked((int)0x00040203);
        public const int ZP_E_OPENNOTEXIST = unchecked((int)0x80040203);      // Порт не существует
        public const int ZP_E_OPENPORT = unchecked((int)0x80040205);          // Другая ошибка открытия порта
        public const int ZP_E_PORTIO = unchecked((int)0x80040206);            // Ошибка порта (Конвертор отключен от USB?)
        public const int ZP_E_PORTSETUP = unchecked((int)0x80040207);         // Ошибка настройки порта
        public const int ZP_E_LOADFTD2XX = unchecked((int)0x80040208);        // Неудалось загрузить FTD2XX.DLL
        public const int ZP_E_SOCKET = unchecked((int)0x80040209);            // Не удалось инициализировать сокеты
        public const int ZP_E_SERVERCLOSE = unchecked((int)0x8004020A);       // Дескриптор закрыт со стороны Сервера
        public const int ZP_E_NOTINITALIZED = unchecked((int)0x8004020B);     // Не проинициализировано с помощью ZP_Initialize
        public const int ZP_E_INSUFFICIENTBUFFER = unchecked((int)0x8004020C);// Размер буфера слишком мал
        public const int ZP_E_NOCONNECT = unchecked((int)0x8004020D);
        [Obsolete("use S_OK")]
        public const int ZP_SUCCESS = S_OK;
        [Obsolete("use ZP_S_CANCELLED")]
        public const int ZP_E_CANCELLED = ZP_S_CANCELLED;
        [Obsolete("use ZP_S_NOTFOUND")]
        public const int ZP_E_NOT_FOUND = ZP_S_NOTFOUND;
        [Obsolete("use E_INVALIDARG")]
        public const int ZP_E_INVALID_PARAM = E_INVALIDARG;          
        [Obsolete("use ZP_E_OPENNOTEXIST")]
        public const int ZP_E_OPEN_NOT_EXIST = ZP_E_OPENNOTEXIST;
        [Obsolete("use ZP_E_OPENACCESS")]
        public const int ZP_E_OPEN_ACCESS = ZP_E_OPENACCESS;
        [Obsolete("use ZP_E_OPENPORT")]
        public const int ZP_E_OPEN_PORT = ZP_E_OPENPORT;
        [Obsolete("use ZP_E_PORTIO")]
        public const int ZP_E_PORT_IO_ERROR = ZP_E_PORTIO;
        [Obsolete("use ZP_E_PORTSETUP")]
        public const int ZP_E_PORT_SETUP = ZP_E_PORTSETUP;
        [Obsolete("use ZP_E_LOADFTD2XX")]
        public const int ZP_E_LOAD_FTD2XX = ZP_E_LOADFTD2XX;
        [Obsolete("use ZP_E_INIT_SOCKET")]
        public const int ZP_E_INIT_SOCKET = ZP_E_SOCKET;
        [Obsolete("use E_OUTOFMEMORY")]
        public const int ZP_E_NOT_ENOUGH_MEMORY = E_OUTOFMEMORY;
        [Obsolete("use E_NOINTERFACE")]
        public const int ZP_E_UNSUPPORT = E_NOINTERFACE;              
        [Obsolete("use ZP_E_NOTINITALIZED")]
        public const int ZP_E_NOT_INITALIZED = ZP_E_NOTINITALIZED;         
        [Obsolete("use E_FAIL")]
        public const int ZP_E_CREATE_EVENT = E_FAIL;           // Ошибка функции CreateEvent
        [Obsolete("use E_FAIL")]
        public const int ZP_E_OTHER = E_FAIL;
        [Obsolete("use E_ACCESSDENIED")]
        public const int ZP_E_OPENACCESS = E_ACCESSDENIED;
        #endregion

        #region Константы 
        public const uint ZP_MAX_PORT_NAME = 31;
        public const uint ZP_MAX_REG_DEV = 32;
        #endregion

        #region Константы ZP_Initialize
        // ZP_Initialize Flags
        public const uint ZP_IF_NO_MSG_LOOP = 0x01;     // Приложение не имеет цикла обработки сообщений (Console or Service)
        public const uint ZP_IF_LOG = 0x02;             // Писать лог
        #endregion

        #region Значения параметров по умолчанию
        public const uint ZP_IP_CONNECTTIMEOUT = 4000;      // Тайм-аут подключения по TCP для порта типа ZP_PORT_IP
        public const uint ZP_IP_RESTOREPERIOD = 3000;       // Период восстановления утерянной TCP-связи (для порта типа ZP_PORT_IP)
        public const uint ZP_IPS_CONNECTTIMEOUT = 10000;    // Тайм-аут подключения по TCP для порта типа ZP_PORT_IPS
        public const uint ZP_USB_RESTOREPERIOD = 3000;      // Период восстановления утерянной связи (для портов типов ZP_PORT_COM и ZP_PORT_FT)
        public const uint ZP_DTC_FINDUSBPERIOD = 5000;      // Период поиска USB-устройств (только поиск com-портов) (для детектора устройств)
        public const uint ZP_DTC_FINDIPPERIOD = 15000;      // Период поиска IP-устройства по UDP (для детектора устройств)
        public const uint ZP_DTC_SCANDEVPERIOD = 0xFFFFFFFF;// Период сканирования устройств, опросом портов (для детектора устройств)
        public const uint ZP_SCAN_RCVTIMEOUT0 = 500;        // Тайм-аут ожидания первого байта ответа на запрос при сканировании устройств
        public const uint ZP_SCAN_RCVTIMEOUT = 3000;        // Тайм-аут ожидания ответа на запрос при сканировании устройств
        public const int ZP_SCAN_MAXTRIES = 2;              // Максимум попыток запроса при сканировании устройств
        public const uint ZP_SCAN_CHECKPERIOD = 0xFFFFFFFF; // Период проверки входяших данных порта при сканировании портов
        public const uint ZP_FINDIP_RCVTIMEOUT = 1000;      // Тайм-аут поиска ip-устройств по UDP
        public const int ZP_FINDIP_MAXTRIES = 1;            // Максимум попыток поиска ip-устройств по UDP
        #endregion

        #region Константы (Флаги для ZP_DD_SetNotification и ZP_DD_NOTIFY_SETTINGS.nNMask)
        public const uint ZP_NF_EXIST = 0x01;           // Уведомления о подключении/отключении порта (ZP_N_INSERT / ZP_N_REMOVE)
        public const uint ZP_NF_CHANGE = 0x02;          // Уведомление о изменении параметров порта (ZP_N_CHANGE)
        public const uint ZP_NF_ERROR = 0x08;           // Уведомление об ошибке в ните(thread), сканирующей порты (ZP_N_ERROR)
        public const uint ZP_NF_SDEVICE = 0x10;         // Информация о устройствах, подключенным к последовательным портам
        public const uint ZP_NF_IPDEVICE = 0x20;        // Информация о устройствах, подключенным к IP-портам
        public const uint ZP_NF_IPSDEVICE = 0x80;       // Опрашивать устройства через IPS-порты
        public const uint ZP_NF_COMPLETED = 0x40;       // Уведомления о завершении сканирования
        public const uint ZP_NF_DEVEXIST = 0x04;        // Уведомления о подключении/отключении устройства (ZP_N_DEVINSERT / ZP_N_DEVREMOVE)
        public const uint ZP_NF_DEVCHANGE = 0x100;      // Уведомления о изменении параметров устройства (ZP_N_DEVCHANGE)
        public const uint ZP_NF_UNIDCOM = 0x1000;       // Искать неопознанные com-порты
        public const uint ZP_NF_USECOM = 0x2000;        // По возможности использовать Com-порт

        public const uint ZP_N_INSERT = 1;              // Подключение порта (ZP_N_EXIST_INFO(MsgParam) - инфо о порте)
        public const uint ZP_N_REMOVE = 2;              // Отключение порта (ZP_N_EXIST_INFO(MsgParam) - инфо о порте)
        public const uint ZP_N_CHANGE = 3;              // Изменение состояния порта (ZP_N_CHANGE_STATE(MsgParam) - инфо об изменениях)
        public const uint ZP_N_ERROR = 4;               // Произошла ошибка в ните (PHRESULT(MsgParam) - код ошибки)
        public const uint ZP_N_COMPLETED = 5;           // Сканирование завершено (PINT(MsgParam) - маска: b0-список com-портов, b1-список ip-портов, b2-информация об устройствах, b3-инициализация, b4-команда, <0-ошибка)
        public const uint ZP_N_DEVINSERT = 6;           // Подключение устройства (PZP_N_EXIST_DEVINFO(MsgParam) - инфо о устройстве)
        public const uint ZP_N_DEVREMOVE = 7;           // Отключение устройства (PZP_N_EXIST_DEVINFO(MsgParam) - инфо о устройстве)
        public const uint ZP_N_DEVCHANGE = 8;           // Изменение параметров устройства (PZP_N_CHANGE_DEVINFO(MsgParam) - инфо об изменениях)

        // Флаги для ZP_N_CHANGE_INFO.nChangeMask и ZP_N_CHANGE_DEVINFO.nChangeMask
        public const uint ZP_CIF_BUSY = 4;              // Изменилось состояние "порт занят"
        public const uint ZP_CIF_FRIENDLY = 8;          // Изменилось дружественное имя порта
        public const uint ZP_CIF_OWNER = 0x20;          // Изменился владелец порта (только для IP устройств)
        public const uint ZP_CIF_MODEL = 0x80;          // Изменилась модель устройства
        public const uint ZP_CIF_SN = 0x100;            // Изменился серийный номер устройства
        public const uint ZP_CIF_VERSION = 0x200;       // Изменилась версия прошивки устройства
        public const uint ZP_CIF_DEVPARAMS = 0x400;     // Изменились расширенные параметры устройства
        public const uint ZP_CIF_LIST = 0x800;          // Изменился список портов (для ZP_DDN_DEVICE_INFO) или устройств (для ZP_DDN_PORT_INFO)
        #endregion

        #region Константы (Флаги для ZP_PORT_INFO.nFlags)
        public const uint ZP_PIF_BUSY = 1;   // Порт занят
        public const uint ZP_PIF_USER = 2;   // Порт, указанный пользователем (массив ZP_PORT_ADDR)
        #endregion

        #region Константы (Флаги для ZP_Port_Open и ZP_PORT_OPEN_PARAMS.nFlags)
        public const uint ZP_POF_NO_WAIT_CONNECT = 1;   // Не ждать завершения процедуры подключения
        public const uint ZP_POF_NO_CONNECT_ERR = 2;    // Не возвращать ошибку в случае когда связь прервалась
        public const uint ZP_POF_NO_DETECT_USB = 4;     // Не использовать детектор USB-устройств (для ZP_PORT_FT и ZP_PORT_COM)
        #endregion

        #region Константы (Флаги для ZP_SearchDevices и ZP_SEARCH_PARAMS.nFlags)
        public const uint ZP_SF_USECOM = 1;             // Использовать COM-порт по возможности
        public const uint ZP_SF_DETECTOR = 2;           // Использовать уже готовый список найденных устройств Детектора (создается функцией ZP_FindNotification)
        public const uint ZP_SF_IPS = 4;                // Включить в список найденные IP-конвертеры в режиме CLIENT
        public const uint ZP_SF_UNID = 8;               // Включить в список неопознанные устройства
        public const uint ZP_SF_UNIDCOM = 0x10;         // Опрашивать неопознанные com-порты
        #endregion

        #region Константы (Флаги для ZP_Port_SetNotification и ZP_Port_EnumMessages)
        public const uint ZP_PNF_RXEVENT = 1;           // Пришли новые данные от устройства
        public const uint ZP_PNF_STATUS = 2;            // Изменилось состояние подключения порта
        #endregion

        #region Устаревшие константы
        [Obsolete("use ZP_NF_CHANGE")]
        public const uint ZP_NF_BUSY = ZP_NF_CHANGE;
        [Obsolete("use ZP_NF_CHANGE")]
        public const uint ZP_NF_FRIENDLY = ZP_NF_CHANGE;
        [Obsolete("use ZP_NF_USECOM")]
        public const uint ZP_NF_USEVCOM = ZP_NF_USECOM;
        [Obsolete("use ZP_NF_WNDSYNC")]
        public const uint ZP_NF_WND_SYNC = 0x4000;
        [Obsolete("use ZP_NF_ONLYNOTIFY")]
        public const uint ZP_NF_ONLY_NOTIFY = 0x8000;
        [Obsolete("use ZP_N_CHANGE")]
        public const uint ZP_N_STATE_CHANGED = ZP_N_CHANGE;
        [Obsolete("use ZP_PIF_BUSY")]
        public const uint ZP_PF_BUSY = ZP_PIF_BUSY;
        [Obsolete]
        public const uint ZP_PF_BUSY2 = 4;
        [Obsolete("use ZP_PIF_USER")]
        public const uint ZP_PF_USER = ZP_PIF_USER;
        // Флаги для функций ZP_EnumSerialDevices, ZP_FindSerialDevice и ZP_EnumIpDevices
        [Obsolete("use ZP_SearchDevices")]
        public const uint ZP_SF_UPDATE = 1;
        [Obsolete("use ZP_SearchDevices")]
        public const uint ZP_SF_USEVCOM = 2;
        [Obsolete("use ZP_IF_LOG")]
        public const uint ZP_IF_ERROR_LOG = ZP_IF_LOG;
        [Obsolete("use ZP_CIF_LIST")]
        public const uint ZP_CIF_PORTS = 0x800;
        [Obsolete("use ZP_CIF_LIST")]
        public const uint ZP_CIF_DEVICES = 0x800;
        [Obsolete("use ZP_POF_NO_WAIT_CONNECT")]
        public const uint ZP_PF_NOWAITCONNECT = ZP_POF_NO_WAIT_CONNECT;
        [Obsolete("use ZP_POF_NO_CONNECT_ERR")]
        public const uint ZP_PF_NOCONNECTERR = ZP_POF_NO_CONNECT_ERR;
        [Obsolete("use ZP_POF_NO_DETECT_USB")]
        public const uint ZP_PF_NOUSBDETECT = ZP_POF_NO_DETECT_USB;
        #endregion

        //public const string ZpDllName = "ZPort.dll";
        //public const string ZpDllName = "ZReader.dll";
        public const string ZpDllName = "ZGuard.dll";

        //Функции библиотеки
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetVersion")]
        public static extern UInt32 ZP_GetVersion();

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Initialize")]
        public static extern int ZP_Initialize([In,Out] IntPtr pObj, UInt32 nFlags);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Finalyze")]
        public static extern int ZP_Finalyze();

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_CloseHandle")]
        public static extern int ZP_CloseHandle(IntPtr hHandle);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SearchDevices")]
        public static extern int ZP_SearchDevices(ref IntPtr pHandle, ref ZP_SEARCH_PARAMS pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DD_SetNotification")]
        public static extern int ZP_DD_SetNotification(ref IntPtr pHandle, ref ZP_DD_NOTIFY_SETTINGS pSettings);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DD_GetNextMessage")]
        public static extern int ZP_DD_GetNextMessage(IntPtr hHandle, ref UInt32 nMsg, ref IntPtr nMsgParam);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DD_SetGlobalSettings")]
        public static extern int ZP_DD_SetGlobalSettings(ref ZP_DD_GLOBAL_SETTINGS pSettings);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DD_GetGlobalSettings")]
        public static extern int ZP_DD_GetGlobalSettings(ref ZP_DD_GLOBAL_SETTINGS pSettings);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DD_Refresh")]
        public static extern int ZP_DD_Refresh(UInt32 nWaitMs=0);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SetServiceCtrlHandle")]
        public static extern int ZP_SetServiceCtrlHandle(IntPtr hSvc);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_DeviceEventNotify")]
        public static extern void ZP_DeviceEventNotify(UInt32 nEvType, IntPtr pEvData);

        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_Open")]
        public static extern int ZP_Port_Open(ref IntPtr pHandle, ref ZP_PORT_OPEN_PARAMS pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_GetBaudAndEvChar")]
        public static extern int ZP_Port_GetBaudAndEvChar(IntPtr pHandle, ref UInt32 nBaud, ref char nEvChar);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_GetConnectionStatus")]
        public static extern int ZP_Port_GetConnectionStatus(IntPtr pHandle, ref ZP_CONNECTION_STATUS nValue);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_SetNotification")]
        public static extern int ZP_Port_SetNotification(IntPtr pHandle, IntPtr hEvent, IntPtr hWnd, UInt32 nMsgId, UInt32 nMsgMask);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_EnumMessages")]
        public static extern int ZP_Port_EnumMessages(IntPtr pHandle, ref UInt32 nMsgs);
        
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_Clear")]
        public static extern int ZP_Port_Clear(IntPtr pHandle, bool fIn, bool fOut);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_SetDtr")]
        public static extern int ZP_Port_SetDtr(IntPtr hHandle, bool fState);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_SetRts")]
        public static extern int ZP_Port_SetRts(IntPtr hHandle, bool fState);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_RegSerialDevice")]
        public static extern int ZP_RegSerialDevice(ref ZP_S_DEVICE pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_RegIpDevice")]
        public static extern int ZP_RegIpDevice(ref ZP_IP_DEVICE pParams);

        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetPortInfoList")]
        public static extern int ZP_GetPortInfoList(ref IntPtr pHandle, ref Int32 nCount, UInt32 nSerDevs = 0, UInt32 nFlags = 0);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetPortInfo")]
        public static extern int ZP_GetPortInfo(IntPtr hHandle, Int32 nIdx, ref ZP_PORT_INFO rInfo);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_FindNextDevice")]
        public static extern int ZP_FindNextDevice(IntPtr hHandle,
            ref ZP_DEVICE_INFO pInfo,
            [In, Out] ZP_PORT_INFO[] pPortArr, Int32 nArrLen, ref Int32 nPortCount,
            UInt32 nTimeout = 0xffffffff);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_FindNextDevice")]
        public static extern int ZP_FindNextDevice(IntPtr hHandle,
            IntPtr pInfo,
            [In, Out] ZP_PORT_INFO[] pPortArr, Int32 nArrLen, ref Int32 nPortCount,
            UInt32 nTimeout = 0xffffffff);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_Write")]
        public static extern int ZP_Port_Write(IntPtr hHandle, [In] Byte[] pData, UInt32 nCount);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_Read")]
        public static extern int ZP_Port_Read(IntPtr hHandle, [Out] Byte[] pData, UInt32 nCount, ref UInt32 nRead);
        [DllImport(ZpDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_Port_GetInCount")]
        public static extern int ZP_Port_GetInCount(IntPtr hHandle, ref UInt32 nCount);

#if ZP_LOG
        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_SetLog")]
        public static extern int ZP_SetLog(string sSvrAddr, string sFileName, UInt32 nMsgMask);

#if WIN64
        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetLog")]
        public static extern int ZP_GetLog(ref string sSvrAddrBuf, UInt64 nSABufSize, ref string sFileNameBuf, UInt64 nFNBufSize, ref UInt32 nMsgMask);
#else
        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_GetLog")]
        public static extern int ZP_GetLog(ref string sSvrAddrBuf, UInt32 nSABufSize, ref string sFileNameBuf, UInt32 nFNBufSize, ref UInt32 nMsgMask);
#endif

        [DllImport(ZpDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZP_AddLog")]
        public static extern int ZP_AddLog(char chSrc, int nMsgType; string sText);
#endif // ZP_LOG

        [Obsolete("use ZP_GetPortInfoList")]
        public static int ZP_EnumSerialPorts(UInt32 nDevTypes, ZP_ENUMPORTSPROC pEnumProc, IntPtr pUserData)
        {
            IntPtr hList = IntPtr.Zero;
            int nPortCount = 0;
            int hr = ZP_GetPortInfoList(ref hList, ref nPortCount, nDevTypes);
            if (hr < 0)
                return hr;
            try
            {
                ZP_PORT_INFO rPI = new ZP_PORT_INFO();
                for (int i = 0; i < nPortCount; i++)
                {
                    hr = ZP_GetPortInfo(hList, i, ref rPI);
                    if (hr < 0)
                        break;
                    if (!pEnumProc(ref rPI, pUserData))
                        return ZP_S_CANCELLED;
                }
            }
            finally
            {
                ZP_CloseHandle(hList);
            }
            return hr;
        }
        [Obsolete("use ZP_GetNextMessage")]
        public static int ZP_EnumMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData)
        {
            int hr;
            UInt32 nMsg = 0;
            IntPtr nMsgParam = IntPtr.Zero;
            while ((hr = ZP_GetNextMessage(hHandle, ref nMsg, ref nMsgParam)) == S_OK)
                pEnumProc(nMsg, nMsgParam, pUserData);
            if (hr == ZP_S_NOTFOUND)
                hr = S_OK;
            return hr;
        }

        [Obsolete("use ZP_CloseHandle")]
        public static int ZP_CloseNotification(IntPtr hHandle)
        {
            return ZP_CloseHandle(hHandle);
        }

        [Obsolete("use ZP_CloseHandle")]
        public static int ZP_Close(IntPtr hHandle)
        {
            return ZP_CloseHandle(hHandle);
        }

        [Obsolete("use ZP_DD_SetNotification")]
        public static int ZP_SetNotification(ref IntPtr pHandle, ref ZP_DD_NOTIFY_SETTINGS pSettings)
        {
            return ZP_DD_SetNotification(ref pHandle, ref pSettings);
        }
        [Obsolete("use ZP_DD_GetNextMessage")]
        public static int ZP_GetNextMessage(IntPtr hHandle, ref UInt32 nMsg, ref IntPtr nMsgParam)
        {
            return ZP_DD_GetNextMessage(hHandle, ref nMsg, ref nMsgParam);
        }
        [Obsolete("use ZP_DD_SetGlobalSettings")]
        public static int ZP_SetDetectorSettings(ref ZP_DD_GLOBAL_SETTINGS pSettings)
        {
            return ZP_DD_SetGlobalSettings(ref pSettings);
        }
        [Obsolete("use ZP_DD_GetGlobalSettings")]
        public static int ZP_GetDetectorSettings(ref ZP_DD_GLOBAL_SETTINGS pSettings)
        {
            return ZP_DD_GetGlobalSettings(ref pSettings);
        }
        [Obsolete("use ZP_DD_Refresh")]
        public static int ZP_UpdateDetector(UInt32 nWaitMs=0)
        {
            return ZP_DD_Refresh(nWaitMs);
        }
    }
}
