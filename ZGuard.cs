using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ZPort;

namespace ZGuard
{
    #region Типы
    // Модель конвертера
    public enum ZG_CVT_TYPE
    {
        ZG_CVT_UNDEF = 0,       // Не определено
        ZG_CVT_Z397,            // Z-397
        ZG_CVT_Z397_GUARD,      // Z-397 Guard
        ZG_CVT_Z397_IP,         // Z-397 IP
        ZG_CVT_Z397_WEB,		// Z-397 Web
        ZG_CVT_Z5R_WEB,         // Z5R Web
        ZG_CVT_MATRIX2WIFI		// Matrix II Wi-Fi
    }
    // Режим конвертера Z397 Guard
    public enum ZG_GUARD_MODE
    {
        ZG_GUARD_UNDEF = 0,     // Не определено
        ZG_GUARD_NORMAL,        // Режим "Normal" (эмуляция обычного конвертера Z397)
        ZG_GUARD_ADVANCED,      // Режим "Advanced"
        ZG_GUARD_TEST,          // Режим "Test" (для специалистов)
        ZG_GUARD_ACCEPT         // Режим "Accept" (для специалистов)
    }
    // Скорость конвертера
    public enum ZG_CVT_SPEED
    {
        ZG_SPEED_19200 = 19200,
        ZG_SPEED_57600 = 57600
    }
    // Тип контроллера
    public enum ZG_CTR_TYPE
    {
        ZG_CTR_UNDEF = 0,       // Не определено
        ZG_CTR_GATE2K,			// Gate 2000
        ZG_CTR_MATRIX2NET,		// Matrix II Net
        ZG_CTR_MATRIX3NET,	    // Matrix III Net	
        ZG_CTR_Z5RNET,			// Z5R Net
        ZG_CTR_Z5RNET8K,		// Z5R Net 8000
        ZG_CTR_GUARDNET,		// Guard Net
        ZG_CTR_Z9,			    // Z-9 EHT Net
        ZG_CTR_EUROLOCK,	    // EuroLock EHT net
        ZG_CTR_Z5RWEB,		    // Z5R Web
        ZG_CTR_MATRIX2WIFI		// Matrix II Wi-Fi
    }
    // Подтип контроллера
    public enum ZG_CTR_SUB_TYPE
    {
        ZG_CS_UNDEF = 0,        // Не определено
        ZG_CS_DOOR,				// Дверь
        ZG_CS_TURNSTILE,		// Турникет
        ZG_CS_GATEWAY,			// Шлюз
        ZG_CS_BARRIER			// Шлакбаум
    }
    // Тип ключа контроллера
    public enum ZG_CTR_KEY_TYPE
    {
        ZG_KEY_UNDEF = 0,       // Не определено
        ZG_KEY_NORMAL,		    // Обычный
        ZG_KEY_BLOCKING,	    // Блокирующий
        ZG_KEY_MASTER		    // Мастер
    }
    // Тип события контроллера
    public enum ZG_CTR_EV_TYPE
    {
        ZG_EV_UNKNOWN = 0,          // Неизвестно
        ZG_EV_BUT_OPEN,				// Открыто кнопкой изнутри
        ZG_EV_KEY_NOT_FOUND,		// Ключ не найден в банке ключей
        ZG_EV_KEY_OPEN,				// Ключ найден, дверь открыта
        ZG_EV_KEY_ACCESS,			// Ключ найден, доступ не разрешен
        ZG_EV_REMOTE_OPEN,			// Открыто оператором по сети
        ZG_EV_KEY_DOOR_BLOCK,		// Ключ найден, дверь заблокирована
        ZG_EV_BUT_DOOR_BLOCK,		// Попытка открыть заблокированную дверь кнопкой
        ZG_EV_NO_OPEN,				// Дверь взломана
        ZG_EV_NO_CLOSE,				// Дверь оставлена открытой (timeout)
        ZG_EV_PASSAGE,				// Проход состоялся
        ZG_EV_SENSOR1,				// Сработал датчик 1
        ZG_EV_SENSOR2,				// Сработал датчик 2
        ZG_EV_REBOOT,				// Перезагрузка контроллера
        ZG_EV_BUT_BLOCK,			// Заблокирована кнопка открывания
        ZG_EV_DBL_PASSAGE,			// Попытка двойного прохода
        ZG_EV_OPEN,					// Дверь открыта штатно
        ZG_EV_CLOSE,				// Дверь закрыта
        ZG_EV_POWEROFF,				// Пропало питание
        ZG_EV_ELECTRO_ON,			// Включение электропитания
        ZG_EV_ELECTRO_OFF,			// Выключение электропитания
        ZG_EV_LOCK_CONNECT,         // Включение замка (триггер)
        ZG_EV_LOCK_DISCONNECT,      // Отключение замка (триггер)
        ZG_EV_MODE_STATE,           // Переключение режимов работы (cм Режим)
        ZG_EV_FIRE_STATE,           // Изменение состояния Пожара
        ZG_EV_SECUR_STATE,          // Изменение состояния Охраны
        ZG_EV_UNKNOWN_KEY,          // Неизвестный ключ
        ZG_EV_GATEWAY_PASS,         // Совершен вход в шлюз
        ZG_EV_GATEWAY_BLOCK,        // Заблокирован вход в шлюз (занят)
        ZG_EV_GATEWAY_ALLOWED,      // Разрешен вход в шлюз
        ZG_EV_ANTIPASSBACK,         // Заблокирован проход (Антипассбек)
        ZG_EV_HOTEL40,
        ZG_EV_HOTEL41
    }
    // Направление прохода контроллера
    public enum ZG_CTR_DIRECT
    {
        ZG_DIRECT_UNDEF = 0,    // Не определено
        ZG_DIRECT_IN,			// Вход
        ZG_DIRECT_OUT			// Выход
    }
    // Условие, вызвавшее событие ElectroControl: ZG_EV_ELECTRO_ON, ZG_EV_ELECTRO_OFF
    public enum ZG_EC_SUB_EV
    {
        ZG_EC_EV_UNDEF = 0,     // Не определено
        ZG_EC_EV_CARD_DELAY,	// Поднесена валидная карта с другой стороны (для входа) запущена задержка
        ZG_EC_EV_RESERVED1,		// (зарезервировано)
        ZG_EC_EV_ON_NET,		// Включено командой по сети
        ZG_EC_EV_OFF_NET,		// Выключено командой по сети
        ZG_EC_EV_ON_SCHED,		// Включено по временной зоне
        ZG_EC_EV_OFF_SHED,		// Выключено по временной зоне
        ZG_EC_EV_CARD,			// Поднесена валидная карта к контрольному устройству
        ZG_EC_EV_RESERVED2,		// (зарезервировано)
        ZG_EC_EV_OFF_TIMEOUT,	// Выключено после отработки таймаута
        ZG_EC_EV_OFF_EXIT		// Выключено по срабатыванию датчика выхода
    }
    // Условие, вызвавшее событие ZG_EV_FIRE_STATE
    public enum ZG_FIRE_SUB_EV
    {
        ZG_FR_EV_UNDEF = 0,     // Не определено
        ZG_FR_EV_OFF_NET,       // выключено по сети
        ZG_FR_EV_ON_NET,        // Включено по сети
        ZG_FR_EV_OFF_INPUT_F,   // Выключено по входу FIRE
        ZG_FR_EV_ON_INPUT_F,    // Включено по входу FIRE
        ZG_FR_EV_OFF_TEMP,      // Выключено по датчику температуры
        ZG_FR_EV_ON_TEMP        // Включено по датчику температуры
    }
    // Условие, вызвавшее событие ZG_EV_SECUR_STATE
    public enum ZG_SECUR_SUB_EV
    {
        ZG_SR_EV_UNDEF = 0,     // Не определено
        ZG_SR_EV_OFF_NET,       // выключено по сети
        ZG_SR_EV_ON_NET,        // Включено по сети
        ZG_SR_EV_OFF_INPUT_A,   // Выключено по входу ALARM
        ZG_SR_EV_ON_INPUT_A,    // Включено по входу ALARM
        ZG_FR_EV_OFF_TAMPERE,   // Выключено по тамперу
        ZG_FR_EV_ON_TAMPERE,    // Включено по тамперу
        ZG_FR_EV_OFF_DOOR,      // Выключено по датчику двери
        ZG_FR_EV_ON_DOOR        // Включено по датчику двери
    }
    // Условие, вызвавшее событие ZG_EV_MODE_STATE
    public enum ZG_MODE_SUB_EV
    {
        ZG_MD_EV_UNDEF = 0,
        ZG_MD_EV_RS485_ALLOW,   // Установка командой по сети 
        ZG_MD_EV_RS485_DENIED,  // Отказано оператору по сети
        ZG_MD_EV_TZ_START,      // Началась временная зона
        ZG_MD_EV_TZ_FINISH,     // Окончилась временная зона
        ZG_MD_EV_CARD_ALLOW,    // Установка картой
        ZG_MD_EV_CARD_DENIED    // Отказано изменению картой
    }
    // Условие, вызвавшее события ZG_EV_HOTEL40, ZG_EV_HOTEL41
    public enum ZG_HOTEL_SUB_EV
    {
        ZG_H_EV_UNDEF = 0,      // Не определено
        ZG_H_EV_FREECARD,       // Карта открытия
        ZG_H_EV_BLOCKCARD,      // Карта блокирующая
        ZG_H_EV_EXFUNC,         // Дополнительная функция
        ZG_H_EV_NEWRCARD,       // создана резервная карта
        ZG_H_EV_NETWORK,
        ZG_H_EV_TIMEZONE,
        ZG_H_EV_COUNTER,        // обновлен счетчик
        ZG_H_EV_CRYPTOKEY,      // обновлен криптоключ
        ZG_H_EV_PULSEZ,         // измененение защелки в течении 2х секунд
        ZG_H_EV_STATE           // состояние защелки -если нажали ручку и отпустили более чем через 2 секунды 
    }
    // Режим Охрана
    public enum ZG_SECUR_MODE
    {
        ZG_SR_M_UNDEF = 0,      // Не определено
        ZG_SR_M_SECUR_OFF,      // Выключить режим охраны
        ZG_SR_M_SECUR_ON,       // Включить режим охраны
        ZG_SR_M_ALARM_OFF,      // Выключить тревогу
        ZG_SR_M_ALARM_ON        // Включить тревогу
    }
    // Режим прохода контроллера
    public enum ZG_CTR_MODE
    {
        ZG_MODE_UNDEF = 0,
        ZG_MODE_NORMAL,			// Обычный режим работы
        ZG_MODE_BLOCK,			// Блокировка (проходить могут только "блокирующие" карты)
        ZG_MODE_FREE,			// Свободный (замок обесточен, при поднесении карты регистрируются)
        ZG_MODE_WAIT			// Ожидание (обычный режим работы, при поднесении допустимой карты переход в режим "Free")
    }
    // Режим HOTEL
    public enum ZG_HOTEL_MODE
    {
        ZG_HMODE_UNDEF = 0,
        ZG_HMODE_NORMAL,		// Норма
        ZG_HMODE_BLOCK,			// Блокирован
        ZG_HMODE_FREE,			// Свободный проход 
        ZG_HMODE_RESERVED		// Резерв
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_PROCESSCALLBACK(int nPos, int nMax, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMCTRSPROC(ref ZG_FIND_CTR_INFO pInfo, int nPos, int nMax, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMCTRTIMEZONESPROC(int nIdx, ref ZG_CTR_TIMEZONE pTz, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMCTRKEYSPROC(int nIdx, ref ZG_CTR_KEY pKey, int nPos, int nMax, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMCTREVENTSPROC(int nIdx, ref ZG_CTR_EVENT pEvent, int nPos, int nMax, IntPtr pUserData);
    #endregion

    #region Устаревшие типы
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMCVTSPROC(ref ZG_ENUM_CVT_INFO pInfo, ref ZP_PORT_INFO pPort, IntPtr pUserData);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool ZG_ENUMIPCVTSPROC(ref ZG_ENUM_IPCVT_INFO pInfo, ref ZP_PORT_INFO pPort, IntPtr pUserData);
    #endregion

    #region Структуры
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_ENUM_CVT_INFO
    {
        public ZP_DEVICE_INFO rBase;
        public ZG_CVT_TYPE nType;               // Тип конвертера
        public ZG_GUARD_MODE nMode;             // Режим работы конвертера Guard
    }
    // Информация об IP-конвертере
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_ENUM_IPCVT_INFO
    {
        public ZP_DEVICE_INFO rBase;
        public ZG_CVT_TYPE nType;                      // Тип IP-конвертера
        public ZG_GUARD_MODE nMode;             // Режим работы конвертера Guard
        public UInt32 nFlags;                          // Флаги: бит 0 - "VCP", бит 1 - "WEB", 0xFF - "All"
    }
    // Информация о конвертере, возвращаемая функциями: ZG_Cvt_Open и ZG_Cvt_GetInformation
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public class ZG_CVT_INFO
    {
        public ZP_DEVICE_INFO rBase;
        public ZG_CVT_TYPE nType;                // Тип конвертера
        public ZG_CVT_SPEED nSpeed;              // Скорость конвертера
        public ZG_GUARD_MODE nMode;              // Режим работы конвертера Guard
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszLinesBuf;               // Буфер для информационных строк
        public int nLinesBufMax;                 // Размер буфера в символах, включая завершающий '\0'
    }
    // Параметры открытия конвертера, используемые функциями: ZG_Cvt_Open и ZG_UpdateCvtFirmware
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZG_CVT_OPEN_PARAMS
    {
        public ZP_PORT_TYPE nPortType;          // Тип порта
        [MarshalAs(UnmanagedType.LPWStr)]
        public string pszName;                  // Имя порта. Если =NULL, то используется hPort
        public IntPtr hPort;                    // Дескриптор порта, полученный функцией ZP_Open
        public ZG_CVT_TYPE nCvtType;            // Тип конвертера. Если =ZG_CVT_UNDEF, то автоопределение
        public ZG_CVT_SPEED nSpeed;             // Скорость конвертера
        public IntPtr pWait;                    // Параметры ожидания. Может быть =NULL.
        public byte nStopBits;
        public int nLicN;                       // Номер лицензии. Если =0, то используется ZG_DEF_CVT_LICN
        [MarshalAs(UnmanagedType.LPStr)]
        public string pActCode;                 // Код активации для режима "Proxy"
        public int nSn;                         // С/н конвертера для режима "Proxy"
        public UInt32 nFlags;                   // Флаги ZG_OF_...
        public ZG_CVT_OPEN_PARAMS(ZP_PORT_TYPE _nType, string _sName, IntPtr _hPort, ZG_CVT_TYPE _nCvtType,
            ZG_CVT_SPEED _nSpeed, 
            IntPtr _pWS = default(IntPtr), 
            byte _nStopBits = 2, 
            int _nLicN = 0,
            string _ActCode = null, 
            int _nSn = 0,
            UInt32 _nFlags = 0)
        {
            nPortType = _nType;
            pszName = _sName;
            hPort = _hPort;
            nCvtType = _nCvtType;
            nSpeed = _nSpeed;
            pWait = _pWS;
            nStopBits = _nStopBits;
            nLicN = _nLicN;
            pActCode = _ActCode;
            nSn = _nSn;
            nFlags = _nFlags;
        }
    }
    // Информация о лицензии конвертера Guard
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CVT_LIC_INFO
    {
        public UInt16 nStatus;                         // Статус лицензии
        UInt16 Reserved;
        public int nMaxCtrs;                           // Максимальное количество контроллеров
        public int nMaxKeys;                           // Максимальное количество ключей
        public UInt16 nMaxYear;                        // Дата: год (= 0xFFFF дата неограничена)
        public UInt16 nMaxMon;                         // Дата: месяц
        public UInt16 nMaxDay;                         // Дата: день
        public UInt16 nDownCountTime;                  // Оставшееся время жизни лицензии в минутах
    }
    // Краткая информация о лицензии конвертера Guard
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CVT_LIC_SINFO
    {
        public int nLicN;                              // Номер лицензии
        public int nMaxCtrs;                           // Максимальное количество контроллеров
        public int nMaxKeys;                           // Максимальное количество ключей
    }
    // Информация о найденном контроллере, возвращаемая функцией ZG_Cvt_FindNextCtr
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct ZG_FIND_CTR_INFO
    {
        public ZG_CTR_TYPE nType;                      // Тип контроллера
        public byte nTypeCode;                         // Код типа контроллера
        public byte nAddr;                             // Сетевой адрес
        public UInt16 nSn;                             // Заводской номер
        public UInt16 nVersion;                        // Версия прошивки
        public int nMaxKeys;                           // Максимум ключей
        public int nMaxEvents;                         // Максимум событий
        public UInt32 nFlags;                          // Флаги контроллера (ZG_CTR_F_...)
        public ZG_CTR_SUB_TYPE nSubType;               // Подтип контроллера
    }
    // Информация о контроллере, возвращаемая функциями: ZG_Ctr_Open и ZG_Ctr_GetInformation
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct ZG_CTR_INFO
    {
        public ZG_CTR_TYPE nType;                      // Тип контроллера
        public byte nTypeCode;                         // Код типа контроллера
        public byte nAddr;                             // Сетевой адрес
        public UInt16 nSn;                             // Заводской номер
        public UInt16 nVersion;                        // Версия прошивки
        public int nInfoLineCount;                     // Количество строк с информацией
        public int nMaxKeys;                           // Максимум ключей
        public int nMaxEvents;                         // Максимум событий
        public UInt32 nFlags;                          // Флаги контроллера (ZG_CTR_F_...)
        UInt16 Reserved;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string pszLinesBuf;                     // Буфер для информационных строк
        public int nLinesBufMax;                       // Размер буфера в символах, включая завершающий '\0'
        public ZG_CTR_SUB_TYPE nSubType;               // Подтип контроллера
        public int nOptReadItems;                      // Количество элементов, которое может быть считано одним запросом контроллеру 
        public int nOptWriteItems;                     // Количество элементов, которое может быть записано одним запросом контроллеру
    }
    // Временная зона контроллера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_TIMEZONE
    {
        public byte nDayOfWeeks;                       // Дни недели
        public byte nBegHour;                          // Начало: час
        public byte nBegMinute;                        // Начало: минута
        public byte nEndHour;                          // Конец: час
        public byte nEndMinute;                        // Конец: минута
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        byte[] Reserved;
        public ZG_CTR_MODE nMode;                       // Режим контроллера (используется только для вр.зон ZG_MODES_TZ0..ZG_MODES_TZ0+1)
        public ZG_CTR_TIMEZONE(byte _nDayOfWeeks, byte _nBegHour, byte _nBegMinute, byte _nEndHour, byte _nEndMinute, ZG_CTR_MODE _nMode)
        {
            nDayOfWeeks = _nDayOfWeeks;
            nBegHour = _nBegHour;
            nBegMinute = _nBegMinute;
            nEndHour = _nEndHour;
            nEndMinute = _nEndMinute;
            Reserved = new byte[3];
            nMode = _nMode;
        }
    }
    // Ключ контроллера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_KEY
    {
        public bool fErased;                           // TRUE, если ключ стерт
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] rNum;                            // Номер ключа
        public ZG_CTR_KEY_TYPE nType;                  // Тип ключа
        public UInt32 nFlags;                          // Флаги ZG_KF_...
        public UInt32 nAccess;                         // Доступ (маска временных зон)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] aData1;                          // Другие данные ключа
        public ZG_CTR_KEY(bool _fErased, [In] byte[] _rNum, ZG_CTR_KEY_TYPE _nType, UInt32 _nFlags, UInt32 _nAccess, [In] byte[] _aData1)
        {
            fErased = _fErased;
            rNum = _rNum;
            nType = _nType;
            nFlags = _nFlags;
            nAccess = _nAccess;
            aData1 = _aData1;
        }
    }
    // Часы контроллера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_CLOCK
    {
        public bool fStopped;                          // TRUE, если часы остановлены
        public UInt16 nYear;                           // Год
        public UInt16 nMonth;                          // Месяц
        public UInt16 nDay;                            // День
        public UInt16 nHour;                           // Час
        public UInt16 nMinute;                         // Минута
        public UInt16 nSecond;                         // Секунда
        public ZG_CTR_CLOCK(bool _fStopped, UInt16 _nYear, UInt16 _nMonth, UInt16 _nDay, UInt16 _nHour, UInt16 _nMinute, UInt16 _nSecond)
        {
            fStopped = _fStopped;
            nYear = _nYear;
            nMonth = _nMonth;
            nDay = _nDay;
            nHour = _nHour;
            nMinute = _nMinute;
            nSecond = _nSecond;
        }
    }
    // Событие контроллера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_EVENT
    {
        public ZG_CTR_EV_TYPE nType;                    // Тип события
        //public byte nEvCode;                            // Код события в контроллере
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        //public byte[] aParams;                          // Параметры события
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] aData;                            // Данные события 
        // (используйте функцию декодирования, соответстующую типу события,
        // ZG_Ctr_DecodePassEvent, ZG_Ctr_DecodeEcEvent, ZG_Ctr_DecodeUnkKeyEvent,
        // ZG_Ctr_DecodeFireEvent, ZG_Ctr_DecodeSecurEvent, ZG_Ctr_DecodeModeEvent)
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_EV_TIME
    {
        public byte nMonth;                             // Месяц
        public byte nDay;                               // День
        public byte nHour;                              // Час
        public byte nMinute;                            // Минута
        public byte nSecond;                            // Секунда
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        byte[] Reserved;
    }
    // Конфигурация управления электропитанием
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_ELECTRO_CONFIG
    {
        public UInt32 nPowerConfig;                    // Конфигурация управления питанием
        public UInt32 nPowerDelay;                     // Время задержки в секундах
        public ZG_CTR_TIMEZONE rTz6;                   // Временная зона №6 (считаем от 0)
        public ZG_CTR_ELECTRO_CONFIG(UInt32 _nPowerConfig, UInt32 _nPowerDelay, ZG_CTR_TIMEZONE _rTz6)
        {
            nPowerConfig = _nPowerConfig;
            nPowerDelay = _nPowerDelay;
            rTz6 = _rTz6;
        }
    }
    // Состояние электропитания
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_CTR_ELECTRO_STATE
    {
        public UInt32 nPowerFlags;                     // Флаги состояния электропитания
        public UInt32 nPowerConfig;                    // Конфигурация управления питанием
        public UInt32 nPowerDelay;                     // Время задержки в секундах
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_N_CTR_CHANGE_INFO
    {
        public UInt32 nChangeMask;                      // Маска изменений (бит0 addr, бит1 version, бит2 proximity)
        public ZG_FIND_CTR_INFO rCtrInfo;               // Измененная информация о контроллере
        public UInt16 nOldVersion;                      // Старое значение версии
        public byte nOldAddr;                           // Старое значение адреса
        byte Reserved;                                  // Зарезервировано для выравнивания структуры
    }
    // Параметры для уведомлений от конвертера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZG_CVT_NOTIFY_SETTINGS
    {
        public UInt32 nNMask;                           // Маска типов уведомлений
        public SafeWaitHandle hEvent;                   // Событие (объект синхронизации)
        public IntPtr hWindow;                          // Окно, принимиющее сообщение nWndMsgId
        public UInt32 nWndMsgId;                        // Сообщение для отправки окну hWnd
        public UInt32 nScanCtrsPeriod;                  // Период сканирования списка контроллеров в мс (=0 использовать значение по умолчанию, 5000, =-1 никогда)
		public int nScanCtrsLastAddr;                   // Последней сканируемый адрес контроллера
        public ZG_CVT_NOTIFY_SETTINGS(UInt32 _nNMask, SafeWaitHandle _hEvent, IntPtr _hWindow, UInt32 _nWndMsgId, UInt32 _nScanCtrsPeriod, int _nScanCtrsLastAddr)
        {
            nNMask = _nNMask;
            hEvent = _hEvent;
            hWindow = _hWindow;
            nWndMsgId = _nWndMsgId;
            nScanCtrsPeriod = _nScanCtrsPeriod;
            nScanCtrsLastAddr = _nScanCtrsLastAddr;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_N_NEW_EVENT_INFO
    {
        public int nNewCount;                           // Количество новых событий
        public int nWriteIdx;                           // Указатель записи
        public int nReadIdx;                            // Указатель чтения
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] rLastNum;                         // Номер последнего поднесенного ключа
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ZG_N_KEY_TOP_INFO
    {
        public int nBankN;                              // Номер банка ключей
        public int nNewTopIdx;                          // Новое значение верхней границы ключей
        public int nOldTopIdx;                          // Старое значение верхней границы ключей
    }
    // Параметры для уведомлений от контроллера
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ZG_CTR_NOTIFY_SETTINGS
    {
        public UInt32 nNMask;                           // Маска типов уведомлений
        public SafeWaitHandle hEvent;                   // Событие (объект синхронизации)
        public IntPtr hWindow;                          // Окно, принимиющее сообщение nWndMsgId
        public UInt32 nWndMsgId;                        // Сообщение для отправки окну hWnd
        public int nReadEvIdx;                          // Указатель чтения событий
        public UInt32 nCheckStatePeriod;                // Период проверки состояния контроллера в мс (=0 использовать значение по умолчанию, 1000): часы, указатели событий, верхняя граница ключей
        public UInt32 nClockOffs;                       // Смещение часов контроллера от часов ПК в секундах
        public ZG_CTR_NOTIFY_SETTINGS(UInt32 _nNMask, SafeWaitHandle _hEvent, IntPtr _hWindow, UInt32 _nWndMsgId, int _nReadEvIdx, UInt32 _nCheckStatePeriod, UInt32 _nClockOffs)
        {
            nNMask = _nNMask;
            hEvent = _hEvent;
            hWindow = _hWindow;
            nWndMsgId = _nWndMsgId;
            nReadEvIdx = _nReadEvIdx;
            nCheckStatePeriod = _nCheckStatePeriod;
            nClockOffs = _nClockOffs;
        }
    }
    #endregion

    class ZGIntf
    {
        #region Совместимая версия SDK
        public const int ZG_SDK_VER_MAJOR = 3;
        public const int ZG_SDK_VER_MINOR = 35;
        #endregion

        #region Коды ошибок
        public static readonly int S_OK = 0;                                                // Операция выполнена успешно
        public static readonly int E_FAIL = unchecked((int)0x80000008);                     // Другая ошибка
        public static readonly int E_OUTOFMEMORY = unchecked((int)0x80000002);              // Недостаточно памяти для обработки команды
        public static readonly int E_INVALIDARG = unchecked((int)0x80000003);               // Неправильный параметр
        public static readonly int E_NOINTERFACE = unchecked((int)0x80000004);              // Функция не поддерживается
        public static readonly int E_HANDLE = unchecked((int)0x80000006);                   // Неправильный дескриптор
        public static readonly int E_ABORT = unchecked((int)0x80000007);                    // Функция прервана (см.описание ZP_WAIT_SETTINGS)

        public static readonly int ZG_E_TOOLARGEMSG = unchecked((int)0x80040301);           // Слишком большое сообщение для отправки
        public static readonly int ZG_E_NOANSWER = unchecked((int)0x80040303);              // Нет ответа
        public static readonly int ZG_E_BADANSWER = unchecked((int)0x80040304);             // Нераспознанный ответ
        public static readonly int ZG_E_WRONGZPORT = unchecked((int)0x80040305);            // Не правильная версия ZPort.dll
        public static readonly int ZG_E_CVTBUSY = unchecked((int)0x80040306);               // Конвертер занят (при открытии конвертера в режиме "Proxy")
        public static readonly int ZG_E_CVTERROR = unchecked((int)0x80040307);              // Другая ошибка конвертера
        public static readonly int ZG_E_LICNOTFOUND = unchecked((int)0x80040308);           // Ошибка конвертера: Нет такой лицензии
        public static readonly int ZG_E_LICEXPIRED = unchecked((int)0x80040309);            // Текущая лицензия истекла
        public static readonly int ZG_E_LICONTROLLERS = unchecked((int)0x8004030A);         // Ошибка конвертера: ограничение лицензии на количество контроллеров
        public static readonly int ZG_E_LICREADKEYS = unchecked((int)0x8004030B);           // Ограничение лицензии на число ключей при чтении
        public static readonly int ZG_E_LICWRITEKEYS = unchecked((int)0x8004030C);          // Ограничение лицензии на число ключей при записи
        public static readonly int ZG_E_LICEXPIRED2 = unchecked((int)0x8004030D);           // Срок лицензии истек (определено при установке даты в контроллере)
        public static readonly int ZG_E_NOCONVERTER = unchecked((int)0x8004030E);           // Конвертер не найден (неверный адрес).
        public static readonly int ZG_E_NOCONTROLLER = unchecked((int)0x8004030F);          // Неверный адрес контроллера (контроллер не найден)
        public static readonly int ZG_E_CTRNACK = unchecked((int)0x80040310);               // Контроллер отказал в выполнении команды
        public static readonly int ZG_E_FWBOOTLOADERNOSTART = unchecked((int)0x80040311);   // Загрузчик не запустился (при прошивке)
        public static readonly int ZG_E_FWFILESIZE = unchecked((int)0x80040312);            // Некорректный размер файла (при прошивке)
        public static readonly int ZG_E_FWNOSTART = unchecked((int)0x80040313);             // Не обнаружен старт прошивки. Попробуйте перезагрузить устройство (при прошивке)
        public static readonly int ZG_E_FWNOCOMPATIBLE = unchecked((int)0x80040314);        // Не подходит для этого устройства (при прошивке)
        public static readonly int ZG_E_FWINVALIDDEVNUM = unchecked((int)0x80040315);       // Не подходит для этого номера устройства (при прошивке)
        public static readonly int ZG_E_FWTOOLARGE = unchecked((int)0x80040316);            // Слишком большой размер данных прошивки (при прошивке)
        public static readonly int ZG_E_FWSEQUENCEDATA = unchecked((int)0x80040317);        // Некорректная последовательность данных (при прошивке)
        public static readonly int ZG_E_FWDATAINTEGRITY = unchecked((int)0x80040318);       // Целостность данных нарушена (при прошивке)
        #endregion

        #region Значения по умолчанию
        public const uint ZG_CVT_SCANCTRSPERIOD = 5000;     // Период сканирования контроллеров (в миллисекундах)
        public const uint ZG_CVT_SCANCTRSLASTADDR = 31;     // Последней сканируемый адрес контроллера (для альтернативного метода сканирования)
        #endregion

        #region Флаги для функции ZG_Initialize
        public const uint ZG_IF_LOG = 0x100;          // Записывать лог
        #endregion

        #region Флаги для функции ZG_Cvt_SetNotification
        public const uint ZG_NF_CVT_CTR_EXIST = 0x01;       // ZG_N_CVT_CTR_INSERT / ZG_N_CVT_CTR_REMOVE
        public const uint ZG_NF_CVT_CTR_CHANGE = 0x02;      // Изменение параметров контроллера ZG_N_CVT_CTR_CHANGE
        public const uint ZG_NF_CVT_ERROR = 0x04;           // ZG_N_CVT_ERROR
        public const uint ZG_NF_CVT_PORTSTATUS = 0x08;      // ZG_N_CVT_PORTSTATUS
        public const uint ZG_NF_CVT_CTR_DBL_CHECK = 0x1000; // Дважды проверять отключение контроллеров
        public const uint ZG_NF_CVT_REASSIGN_ADDRS = 0x2000;// Автоматическое переназначение адресов контроллеров (кроме Guard Advanced) (работает только с ZG_NF_CVT_CTR_EXIST)
        public const uint ZG_NF_CVT_WND_SYNC = 0x4000;      // Синхронизировать с очередью сообщений Windows
        public const uint ZG_NF_CVT_ONLY_NOTIFY = 0x8000;   // Только уведомлять о добавлении новых сообщений в очередь
        public const uint ZG_NF_CVT_RESCAN_CTRS = 0x10000;  // Начать заново сканирование контроллеров (для простого конвертера, не Guard)
        public const uint ZG_NF_CVT_ALT_SCAN = 0x20000;     // Альтернативный метод скарирования
        public const uint ZG_NF_CVT_NOGATE = 0x40000;       // Не сканировать GATE-контроллеры (все, кроме Eurolock)
        public const uint ZG_NF_CVT_NOEUROLOCK = 0x80000;   // Не сканировать Eurolock EHT net

        public const uint ZG_N_CVT_CTR_INSERT = 1;          // Контроллер подключен PZG_FIND_CTR_INFO(MsgParam) - информация о контроллере
        public const uint ZG_N_CVT_CTR_REMOVE = 2;          // Контроллер отключен PZG_FIND_CTR_INFO(MsgParam) - информация о контроллере
        public const uint ZG_N_CVT_CTR_CHANGE = 3;          // Изменены параметры контроллера PZG_N_CTR_CHANGE_INFO(MsgParam)
        public const uint ZG_N_CVT_ERROR = 4;               // Возникла ошибка в нити (HRESULT*)nMsgParam - код ошибки
        public const uint ZG_N_CVT_PORTSTATUS = 5;          // Изменилось состояние подключения
        #endregion

        #region Флаги для функции ZG_Ctr_SetNotification
        public const uint ZG_NF_CTR_NEW_EVENT = 0x01;       // ZG_N_CTR_NEW_EVENT
        public const uint ZG_NF_CTR_CLOCK = 0x02;           // ZG_N_CTR_CLOCK
        public const uint ZG_NF_CTR_KEY_TOP = 0x04;         // ZG_N_CTR_KEY_TOP
        public const uint ZG_NF_CTR_ADDR_CHANGE = 0x08;     // ZG_N_CTR_ADDR_CHANGE
        public const uint ZG_NF_CTR_ERROR = 0x10;           // ZG_N_CTR_ERROR

        public const uint ZG_N_CTR_NEW_EVENT = 1;           // Новые события PZG_N_NEW_EVENT_INFO(MsgParam) - информация
        public const uint ZG_N_CTR_CLOCK = 2;               // Величина рассинхронизации в секундах PINT64(MsgParam)
        public const uint ZG_N_CTR_KEY_TOP = 3;             // Изменилась верхняя граница ключей PZG_N_KEY_TOP_INFO(MsgParam) - информация
        public const uint ZG_N_CTR_ADDR_CHANGE = 4;         // Изменен сетевой адрес контроллера PByte(MsgParam) = новый адрес
        public const uint ZG_N_CTR_ERROR = 5;               // Возникла ошибка в нити PHRESULT(MsgParam) - код ошибки
        #endregion

        #region Флаги для структуры ZG_CTR_KEY
        public const uint ZG_KF_FUNCTIONAL = 0x0002;     // Функциональная
        public const uint ZG_KF_DUAL = 0x0004;           // Двойная карта
        public const uint ZG_KF_SHORTNUM = 0x0020;    // Короткий номер. Если fProximity=False, то контроллер будет проверять только первые 3 байта номера ключа.
        #endregion

        #region Флаги для функции ZG_Ctr_ControlDevices
        public const uint ZG_DEV_RELE1 = 0;     // реле номер 1
        public const uint ZG_DEV_RELE2 = 1;     // реле номер 2
        public const uint ZG_DEV_SW3 = 2;       // силовой ключ SW3 (ОС) Конт.5 колодки К5
        public const uint ZG_DEV_SW4 = 3;       // силовой ключ SW4 (ОС) Конт.5 колодки К6
        public const uint ZG_DEV_SW0 = 4;       // силовой ключ SW0 (ОС) Конт.1 колодки К4
        public const uint ZG_DEV_SW1 = 5;       // силовой ключ SW1 (ОС) Конт.3 колодки К4
        public const uint ZG_DEV_K65 = 6;       // слаботочный ключ (ОК) Конт.6 колодки К5
        public const uint ZG_DEV_K66 = 7;       // слаботочный ключ (ОК) Конт.6 колодки К6
        #endregion

        #region Флаги для структуры ZG_FIND_CTR_INFO и ZG_CTR_INFO
        public const uint ZG_CTR_F_2BANKS = 0x01;           // 2 банка / 1 банк
        public const uint ZG_CTR_F_PROXIMITY = 0x02;        // Proximity (Wiegand) / TouchMemory (Dallas)
        public const uint ZG_CTR_F_JOIN = 0x04;             // Объединение двух банков
        public const uint ZG_CTR_F_X2 = 0x08;               // Удвоение ключей
        public const uint ZG_CTR_F_ELECTRO = 0x10;          // Функция ElectroControl (для Matrix II Net)
        public const uint ZG_CTR_F_MODES = 0x20;            // Поддержка режимов прохода
        public const uint ZG_CTR_F_DUAL_ZONE = 0x40;        // Поддержка двух наборов временных зон
        #endregion

        #region Флаги для функции ZG_Ctr_ReadElectroConfig, ZG_Ctr_GetElectroState, ZG_Ctr_DecodeEcEvent (конфигурация электропитания)
        public const uint ZG_EC_CF_ENABLED = 0x01;          // Задействовать управление питанием
        public const uint ZG_EC_CF_SCHEDULE = 0x02;         // Использовать временную зону 6 для включения питания
        public const uint ZG_EC_CF_EXT_READER = 0x04;       // Контрольный считыватель: «0» Matrix-II Net, «1» внешний считыватель
        public const uint ZG_EC_CF_INVERT = 0x08;           // Инвертировать управляющий выход
        public const uint ZG_EC_CF_EXIT_OFF = 0x10;         // Задействовать датчик двери
        public const uint ZG_EC_CF_CARD_OPEN = 0x20;        // Не блокировать функцию открывания для контрольного считывателя
        #endregion

        #region Флаги для функции ZG_Ctr_GetElectroState, ZG_Ctr_DecodeEcEvent (состояние электропитания)
        public const uint ZG_EC_SF_ENABLED = 0x01;          // состояние питания – 1 вкл/0 выкл
        public const uint ZG_EC_SF_SCHEDULE = 0x02;         // активно включение по временной зоне
        public const uint ZG_EC_SF_REMOTE = 0x04;           // включено по команде по сети
        public const uint ZG_EC_SF_DELAY = 0x08;            // идет отработка задержки
        public const uint ZG_EC_SF_CARD = 0x10;             // карта в поле контрольного считывателя
        #endregion

        #region Флаги для функции ZG_Ctr_GetFireInfo и ZG_Ctr_DecodeFireEvent (состояние режима Пожар)
        public const uint ZG_FR_F_ENABLED = 0x01;           // Состояние пожарного режима – 1 вкл/0 выкл
        public const uint ZG_FR_F_INPUT_F = 0x02;           // Активен пожарный режим по входу FIRE
        public const uint ZG_FR_F_TEMP = 0x04;              // Активен пожарный режим по превышению температуры
        public const uint ZG_FR_F_NET = 0x08;               // Активен пожарный режим по внешней команде
        #endregion

        #region Флаги для функции ZG_Ctr_GetFireInfo (маска разрешенных источников режима Пожар)
        public const uint ZG_FR_SRCF_INPUT_F = 0x01;        // Разрешен пожарный режим по входу FIRE
        public const uint ZG_FR_SRCF_TEMP = 0x02;           // Разрешен пожарный режим по превышению температуры
        #endregion

        #region Флаги для функции ZG_Ctr_GetSecurInfo и ZG_Ctr_DecodeSecurEvent (состояние режима Охрана)
        public const uint ZG_SR_F_ENABLED = 0x01;           // Состояние охранного режима – 1 вкл/0 выкл
        public const uint ZG_SR_F_ALARM = 0x02;             // Состояние тревоги
        public const uint ZG_SR_F_INPUT_A = 0x04;           // Тревога по входу ALARM
        public const uint ZG_SR_F_TAMPERE = 0x08;           // Тревога по тамперу
        public const uint ZG_SR_F_DOOR = 0x10;              // Тревога по датчику двери
        public const uint ZG_SR_F_NET = 0x20;               // Тревога включена по сети
        #endregion

        #region Флаги для функции ZG_Ctr_GetSecurInfo (маска разрешенных источников режима Охрана)
        public const uint ZG_SR_SRCF_INPUT_F = 0x01;        // Разрешена тревога по входу FIRE
        public const uint ZG_SR_SRCF_TAMPERE = 0x02;        // Разрешена тревога по тамперу
        public const uint ZG_SR_SRCF_DOOR = 0x04;           // Разрешена тревога по датчику двери
        #endregion

        #region Флаги для функции ZG_DecodeHotelEvent (состояние HOTEL)
        public const uint ZG_HF_LATCH = 0x01;       // Защёлка
        public const uint ZG_HF_LATCH2 = 0x02;      // Задвижка
        public const uint ZG_HF_KEY = 0x04;         // Ключ
        public const uint ZG_HF_CARD = 0x08;        // Карта
        #endregion

        #region Флаги для функции ZG_Cvt_EnumControllers
        public const uint ZG_F_UPDATE = 1;      // Обновить список сейчас
        public const uint ZG_F_REASSIGN = 2;    // Переназначить конфликтующие адреса
        public const uint ZG_F_NOGATE = 4;      // Не искать GATE-контроллеры
        public const uint ZG_F_NOEUROLOCK = 8;  // Не искать Eurolock EHT net
        #endregion

        #region Флаги для функции ZG_Cvt_Open и структуры ZG_CVT_OPEN_PARAMS
        public const uint ZG_OF_NOCHECKLIC = 1;      // Не проверять/обновлять лицензию
        public const uint ZG_OF_NOSCANCTRS = 2;      // Не сканировать контроллеры
        #endregion

        #region Флаги для функции ZG_Cvt_SearchControllers
        public const uint ZG_CVSF_DETECTOR = 1;     // Использовать готовый список найденных контроллеров детектора (ZG_Cvt_SetNotification)
        public const uint ZG_CVSF_NOGATE = 4;       // Не искать GATE-контроллеры
        public const uint ZG_CVSF_NOEUROLOCK = 8;   // Не искать замки: Eurolock EHT net и Z-9 EHT net
        #endregion

        #region Другие константы
        public const uint ZG_DEVTYPE_GUARD = 1;
        public const uint ZG_DEVTYPE_Z397 = 2;
        public const uint ZG_DEVTYPE_COM = 5;
        public const uint ZG_DEVTYPE_IPGUARD = ZPIntf.ZP_MAX_REG_DEV;
        public const uint ZG_DEVTYPE_CVTS = 0x26;
        public const uint ZG_IPDEVTYPE_CVTS = 1;
        public const byte ZG_DEF_CVT_LICN = 5;              // Номер лицензии конвертера по умолчанию
        public const int ZG_MAX_TIMEZONES = 7;              // Максимум временных зон
        public const int ZG_MAX_LICENSES = 16;              // Максимальное количество лицензий, которое можно установить в конвертер
        public const int ZG_MODES_TZ0 = -2;                 // Номер первой вр.зоны для переключения режима контроллера, всего 2 зоны
        public const int ZG_DUAL_ZONE_TZ0 = -9;             // Номер первой вр.зоны во втором наборе, всего 7 зон
        #endregion

        #region Устаревшие константы
        [Obsolete("use S_OK")]
        public static readonly int ZG_SUCCESS = S_OK;
        [Obsolete("use ZP_S_CANCELLED")]
        public static readonly int ZG_E_CANCELLED = ZPIntf.ZP_S_CANCELLED;
        [Obsolete("use ZP_S_NOTFOUND")]
        public static readonly int ZG_E_NOT_FOUND = ZPIntf.ZP_S_NOTFOUND;
        [Obsolete("use E_INVALIDARG")]
        public static readonly int ZG_E_INVALID_PARAM = E_INVALIDARG;
        [Obsolete("use ZPIntf.ZP_E_OPENNOTEXIST")]
        public static readonly int ZG_E_OPEN_NOT_EXIST = ZPIntf.ZP_E_OPENNOTEXIST;
        [Obsolete("use ZPIntf.ZP_E_OPENACCESS")]
        public static readonly int ZG_E_OPEN_ACCESS = ZPIntf.E_ACCESSDENIED;
        [Obsolete("use ZPIntf.ZP_E_OPENPORT")]
        public static readonly int ZG_E_OPEN_PORT = ZPIntf.ZP_E_OPENPORT;
        [Obsolete("use ZPIntf.ZP_E_PORTIO")]
        public static readonly int ZG_E_PORT_IO_ERROR = ZPIntf.ZP_E_PORTIO;
        [Obsolete("use ZPIntf.ZP_E_PORTSETUP")]
        public static readonly int ZG_E_PORT_SETUP = ZPIntf.ZP_E_PORTSETUP;
        [Obsolete("use ZPIntf.ZP_E_LOADFTD2XX")]
        public static readonly int ZG_E_LOAD_FTD2XX = ZPIntf.ZP_E_LOADFTD2XX;
        [Obsolete("use ZPIntf.ZP_E_SOCKET")]
        public static readonly int ZG_E_INIT_SOCKET = ZPIntf.ZP_E_SOCKET;
        [Obsolete("use ZPIntf.ZP_E_SERVERCLOSE")]
        public static readonly int ZG_E_SERVERCLOSE = ZPIntf.ZP_E_SERVERCLOSE;
        [Obsolete("use E_OUTOFMEMORY")]
        public static readonly int ZG_E_NOT_ENOUGH_MEMORY = E_OUTOFMEMORY;
        [Obsolete("use E_NOINTERFACE")]
        public static readonly int ZG_E_UNSUPPORT = E_NOINTERFACE;
        [Obsolete("use ZPIntf.ZP_E_NOTINITALIZED")]
        public static readonly int ZG_E_NOT_INITALIZED = ZPIntf.ZP_E_NOTINITALIZED;
        [Obsolete("use E_FAIL")]
        public static readonly int ZG_E_CREATE_EVENT = E_FAIL;
        [Obsolete("use ZG_E_TOOLARGEMSG")]
        public static readonly int ZG_E_TOO_LARGE_MSG = ZG_E_TOOLARGEMSG;
        [Obsolete("use ZG_E_INSUFFICIENTBUFFER")]
        public static readonly int ZG_E_INSUFFICIENT_BUFFER = ZPIntf.ZP_E_INSUFFICIENTBUFFER;
        [Obsolete("use ZG_E_NOANSWER")]
        public static readonly int ZG_E_NO_ANSWER = ZG_E_NOANSWER;
        [Obsolete("use ZG_E_BADANSWER")]
        public static readonly int ZG_E_BAD_ANSWER = ZG_E_BADANSWER;
        [Obsolete("use E_NOINTERFACE")]
        public static readonly int ZG_E_ONLY_GUARD = E_NOINTERFACE;
        [Obsolete("use ZG_E_WRONGZPORT")]
        public static readonly int ZG_E_WRONG_ZPORT_VERSION = ZG_E_WRONGZPORT;
        [Obsolete("use ZG_E_CVTBUSY")]
        public static readonly int ZG_E_CVT_BUSY = ZG_E_CVTBUSY;
        [Obsolete("use E_NOINTERFACE")]
        public static readonly int ZG_E_G_ONLY_ADVANCED = E_NOINTERFACE;
        [Obsolete("use ZG_E_CVTERROR")]
        public static readonly int ZG_E_G_OTHER = ZG_E_CVTERROR;
        [Obsolete("use ZG_E_CVTERROR")]
        public static readonly int ZG_E_G_LIC_OTHER = ZG_E_CVTERROR;
        [Obsolete("use ZG_E_LICNOTFOUND")]
        public static readonly int ZG_E_G_LIC_NOT_FOUND = ZG_E_LICNOTFOUND;
        [Obsolete("use ZG_E_LICEXPIRED")]
        public static readonly int ZG_E_G_LIC_EXPIRED = ZG_E_LICEXPIRED;
        [Obsolete("use ZG_E_LICONTROLLERS")]
        public static readonly int ZG_E_G_LIC_CTR_LIM = ZG_E_LICONTROLLERS;
        [Obsolete("use ZG_E_LICREADKEYS")]
        public static readonly int ZG_E_G_LIC_RKEY_LIM = ZG_E_LICREADKEYS;
        [Obsolete("use ZG_E_LICWRITEKEYS")]
        public static readonly int ZG_E_G_LIC_WKEY_LIM = ZG_E_LICWRITEKEYS;
        [Obsolete("use ZG_E_LICEXPIRED2")]
        public static readonly int ZG_E_G_LIC_EXPIRED2 = ZG_E_LICEXPIRED2;
        [Obsolete("use ZG_E_BADCS")]
        public static readonly int ZG_E_G_BAD_CS = ZG_E_CVTERROR;
        [Obsolete("use ZG_E_CTRNOTFOUND")]
        public static readonly int ZG_E_G_CTR_NOT_FOUND = ZG_E_NOCONTROLLER;
        [Obsolete("use ZG_E_CVTERROR")]
        public static readonly int ZG_E_G_CMD_UNSUPPORT = ZG_E_CVTERROR;
        [Obsolete("use ZG_E_CTRNACK")]
        public static readonly int ZG_E_CTR_NACK = ZG_E_CTRNACK;
        [Obsolete("use ZG_E_CTRNOTFOUND")]
        public static readonly int ZG_E_CTR_TRANSFER = ZG_E_NOCONTROLLER;
        [Obsolete("use ZG_E_FWBOOTLOADERNOSTART")]
        public static readonly int ZG_E_BOOTLOADER_NOSTART = ZG_E_FWBOOTLOADERNOSTART;
        [Obsolete("use ZG_E_FWFILESIZE")]
        public static readonly int ZG_E_FIRMWARE_FILESIZE = ZG_E_FWFILESIZE;
        [Obsolete("use ZG_E_FWNOSTART")]
        public static readonly int ZG_E_FIRMWARE_NOSTART = ZG_E_FWNOSTART;
        [Obsolete("use ZG_E_FWNOCOMPATIBLE")]
        public static readonly int ZG_E_FW_NO_COMPATIBLE = ZG_E_FWNOCOMPATIBLE;
        [Obsolete("use ZG_E_FWINVALIDDEVNUM")]
        public static readonly int ZG_E_FW_INVALID_DEV_NUM = ZG_E_FWINVALIDDEVNUM;
        [Obsolete("use ZG_E_FWTOOLARGE")]
        public static readonly int ZG_E_FW_TOOLARGE = ZG_E_FWTOOLARGE;
        [Obsolete("use ZG_E_FWSEQUENCEDATA")]
        public static readonly int ZG_E_FW_SEQUENCE_DATA = ZG_E_FWSEQUENCEDATA;
        [Obsolete("use ZG_E_FWDATAINTEGRITY")]
        public static readonly int ZG_E_FW_DATA_INTEGRITY = ZG_E_FWDATAINTEGRITY;
        [Obsolete("use E_FAIL")]
        public static readonly int ZG_E_FW_OTHER = E_FAIL;
        [Obsolete("use E_FAIL")]
        public static readonly int ZG_E_OTHER = E_FAIL;

        [Obsolete]
        public const uint ZG_NF_CTR_WND_SYNC = 0x4000;
        [Obsolete]
        public const uint ZG_NF_CTR_ONLY_NOTIFY = 0x8000;
        [Obsolete("use ZG_KF_SHORTNUM")]
        public const uint ZG_KF_SHORT_NUM = ZG_KF_SHORTNUM;
        [Obsolete]
        public const uint ZG_KF_ANTIPASSBACK = 2;
        [Obsolete("Use ZG_IF_LOG")]
        public const uint ZG_IF_ERROR_LOG = ZG_IF_LOG;
        #endregion

        public const string ZgDllName = "ZGuard.dll";

        //Функции библиотеки
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_GetVersion")]
        public static extern UInt32 ZG_GetVersion();

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Initialize")]
        public static extern int ZG_Initialize(UInt32 nFlags);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Finalyze")]
        public static extern int ZG_Finalyze();

        public static int ZG_CloseHandle(IntPtr hHandle)
        {
            return ZPIntf.ZP_CloseHandle(hHandle);
        }

        public static int ZG_GetPortInfoList(ref IntPtr pHandle, ref Int32 nCount)
        {
            return ZPIntf.ZP_GetPortInfoList(ref pHandle, ref nCount, ZG_DEVTYPE_CVTS);
        }

        public static int ZG_SearchDevices(ref IntPtr pHandle, ref ZP_SEARCH_PARAMS pParams, bool fSerial=true, bool fIP=true)
        {
            if (fSerial)
                pParams.nDevMask |= ZG_DEVTYPE_CVTS;
            if (fIP)
                pParams.nIpDevMask |= ZG_IPDEVTYPE_CVTS;
            return ZPIntf.ZP_SearchDevices(ref pHandle, ref pParams);
        }
        public static int ZG_FindNextDevice(IntPtr hHandle,
            ref ZG_ENUM_IPCVT_INFO pInfo,
            [In, Out] ZP_PORT_INFO[] pPortArr, Int32 nArrLen, ref Int32 nPortCount,
            UInt32 nTimeout = 0xffffffff)
        {
            int hr;
            pInfo.rBase.cbSize = (UInt32)Marshal.SizeOf(typeof(ZG_ENUM_IPCVT_INFO));
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(pInfo));
            try
            {
                Marshal.StructureToPtr(pInfo, p, false);
                hr = ZPIntf.ZP_FindNextDevice(hHandle, p, pPortArr, nArrLen, ref nPortCount, nTimeout);
                if (hr == S_OK)
                    pInfo = (ZG_ENUM_IPCVT_INFO)Marshal.PtrToStructure(p, typeof(ZG_ENUM_IPCVT_INFO));
            }
            finally
            {
                Marshal.FreeHGlobal(p);
            }
            return hr;
        }

        public static int ZG_SetNotification(ref IntPtr pHandle, ref ZP_DD_NOTIFY_SETTINGS pSettings, bool fSerial=true, bool fIP=true)
        {
            if (fSerial)
                pSettings.nSDevTypes |= ZG_DEVTYPE_CVTS;
            if (fIP)
                pSettings.nIpDevTypes |= ZG_IPDEVTYPE_CVTS;
            return ZPIntf.ZP_DD_SetNotification(ref pHandle, ref pSettings);
        }

        public static int ZG_GetNextMessage(IntPtr hHandle, ref UInt32 nMsg, ref IntPtr nMagParam)
        {
            return ZPIntf.ZP_DD_GetNextMessage(hHandle, ref nMsg, ref nMagParam);
        }

        [DllImport(ZgDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_GetProxyConverters")]
        public static extern int ZG_GetProxyConverters(IntPtr hHandle,
            [In, Out] UInt16[] pSnBuf, int nBufSize, ref int nRCount,
            string pIpAddr, [In] [MarshalAs(UnmanagedType.LPStr)] string pActCode,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null);

        [DllImport(ZgDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_UpdateCvtFirmware")]
        public static extern int ZG_UpdateCvtFirmware(ref ZG_CVT_OPEN_PARAMS pParams, [In] byte[] pData, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_Open")]
        /// <summary>
        /// Открывает конвертер.
        /// </summary>
        /// <param name="pHandle">Возвращаемый дескриптор конвертера.</param>
        /// <param name="pParams">Параметры открытия конвертера.</param>
        /// <param name="pInfo">Информация о конвертере.</param>
        /// <returns></returns>
        public static extern int ZG_Cvt_Open(ref IntPtr pHandle, ref ZG_CVT_OPEN_PARAMS pParams, 
            [In,Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_CVT_INFO pInfo=null);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_DettachPort")]
        public static extern int ZG_Cvt_DettachPort(IntPtr hHandle, ref IntPtr pPortHandle);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetConnectionStatus")]
        public static extern int ZG_Cvt_GetConnectionStatus(IntPtr pHandle, ref ZP_CONNECTION_STATUS nValue);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetWaitSettings")]
        public static extern int ZG_Cvt_GetWaitSettings(IntPtr hHandle, ref ZP_WAIT_SETTINGS pSetting);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetWaitSettings")]
        public static extern int ZG_Cvt_SetWaitSettings(IntPtr hHandle, ref ZP_WAIT_SETTINGS pSetting);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetCapture")]
        public static extern int ZG_Cvt_SetCapture(IntPtr hHandle);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_ReleaseCapture")]
        public static extern int ZG_Cvt_ReleaseCapture(IntPtr hHandle);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_Clear")]
        public static extern int ZG_Cvt_Clear(IntPtr hHandle);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_Send")]
        public static extern int ZG_Cvt_Send(IntPtr hHandle, [In] byte[] pData, int nCount);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_Receive")]
        public static extern int ZG_Cvt_Receive(IntPtr hHandle, [In, Out] byte[] pBuf, int nBufSize, int nMinRead, ref int pCount);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_Exec")]
        public static extern int ZG_Cvt_Exec(IntPtr hHandle, [In] byte[] pData, int nCount, [In, Out] byte[] pBuf, int nBufSize, int nMinRead, ref int pRCount);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_EnumControllers")]
        public static extern int ZG_Cvt_EnumControllers(IntPtr hHandle, ZG_ENUMCTRSPROC pEnumProc, IntPtr pUserData = default(IntPtr),
             UInt32 nFlags = ZG_F_UPDATE);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_FindController")]
        public static extern int ZG_Cvt_FindController(IntPtr hHandle, byte nAddr, ref ZG_FIND_CTR_INFO pInfo,
            UInt32 nFlags /*= ZG_F_UPDATE*/, ref ZP_WAIT_SETTINGS pWait);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SearchControllers")]
        public static extern int ZG_Cvt_SearchControllers(IntPtr hHandle, int nMaxCtrs, UInt32 nFlags = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_FindNextController")]
        public static extern int ZG_Cvt_FindNextController(IntPtr hHandle, 
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_FIND_CTR_INFO pInfo);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetInformation")]
        public static extern int ZG_Cvt_GetInformation(IntPtr hHandle, ref ZG_CVT_INFO pInfo);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetNotification")]
        public static extern int ZG_Cvt_SetNotification(IntPtr hHandle, [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_CVT_NOTIFY_SETTINGS pSettings);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetNextMessage")]
        public static extern int ZG_Cvt_GetNextMessage(IntPtr hHandle, ref UInt32 nMsg, ref IntPtr nMsgParam);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetScanCtrsState")]
        public static extern int ZG_Cvt_GetScanCtrsState(IntPtr hHandle, ref int nNextAddr);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_UpdateFirmware")]
        public static extern int ZG_Cvt_UpdateFirmware(IntPtr hHandle, [In] byte[] pData, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetLicense")]
        public static extern int ZG_Cvt_GetLicense(IntPtr hHandle, byte nLicN, ref ZG_CVT_LIC_INFO pInfo);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetLicenseData")]
        public static extern int ZG_Cvt_SetLicenseData(IntPtr hHandle, byte nLicN, 
            [In] byte[] pData, int nCount, ref UInt16 pLicStatus);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_ClearAllLicenses")]
        public static extern int ZG_Cvt_ClearAllLicenses(IntPtr hHandle);

        /// <summary>
        /// Возвращает информацию о всех лицензиях, установленных в конвертер
        /// </summary>
        /// <param name="hHandle">Дескриптор конвертера</param>
        /// <param name=""></param>
        /// <param name="pBuf">Буфер для информации о лицензиях. Может быть равен NULL.</param>
        /// <param name="nBufSize">Количество элементов в буфере</param>
        /// <param name="pCount">Количество установленных лицензий</param>
        /// <returns></returns>
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetAllLicenses")]
        public static extern int ZG_Cvt_GetAllLicenses(IntPtr hHandle, [In, Out] ZG_CVT_LIC_SINFO[] pBuf, int nBufSize, ref int pCount);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetShortInfo")]
        public static extern int ZG_Cvt_GetShortInfo(IntPtr hHandle, ref UInt16 pSn, ref ZG_GUARD_MODE pMode);

        [DllImport(ZgDllName, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetLongInfo")]
        public static extern int ZG_Cvt_GetLongInfo(IntPtr hHandle, ref UInt16 pSn, ref UInt32 pVersion, 
            ref ZG_GUARD_MODE pMode, ref string pBuf, int nBufSize, ref int pLen);

        [DllImport(ZgDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_UpdateCtrFirmware")]
        public static extern int ZG_Cvt_UpdateCtrFirmware(IntPtr hHandle, UInt16 nSn,
            [In] byte[] pData, int nCount, string pszInfoStr, ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetCtrAddrBySn")]
        public static extern int ZG_Cvt_SetCtrAddrBySn(IntPtr hHandle, UInt16 nSn, byte nNewAddr, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_SetCtrAddr")]
        public static extern int ZG_Cvt_SetCtrAddr(IntPtr hHandle, byte nOldAddr, byte nNewAddr, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetCtrInfoNorm")]
        public static extern int ZG_Cvt_GetCtrInfoNorm(IntPtr hHandle, byte nAddr, ref byte pTypeCode, ref UInt16 pSn, ref UInt16 pVersion, ref int pInfoLines, ref UInt32 pFlags, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetCtrInfoAdv")]
        public static extern int ZG_Cvt_GetCtrInfoAdv(IntPtr hHandle, byte nAddr, ref byte pTypeCode, ref UInt16 pSn, ref UInt16 pVersion, ref UInt32 pFlags, ref int pEvWrIdx, ref int pEvRdIdx);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetCtrInfoBySn")]
        public static extern int ZG_Cvt_GetCtrInfoBySn(IntPtr hHandle, UInt16 nSn, ref byte pTypeCode, ref byte pAddr, ref UInt16 pVersion, ref int pInfoLines, ref UInt32 pFlags, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetCtrInfoLine")]
        public static extern int ZG_Cvt_GetCtrInfoLine(IntPtr hHandle, UInt16 nSn, int nLineN, ref string pBuf, int nBufSize, ref int pLen, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Cvt_GetCtrVersion")]
        public static extern int ZG_Cvt_GetCtrVersion(IntPtr hHandle, byte nAddr, ref byte[] pVerData5, [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_Open")]
        public static extern int ZG_Ctr_Open(ref IntPtr hHandle, IntPtr hCvtHandle, byte nAddr, UInt16 nSn,
            ref ZG_CTR_INFO pInfo, ZG_CTR_TYPE nModel = ZG_CTR_TYPE.ZG_CTR_UNDEF);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetInformation")]
        public static extern int ZG_Ctr_GetInformation(IntPtr hHandle, ref ZG_CTR_INFO pInfo);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetNotification")]
        public static extern int ZG_Ctr_SetNotification(IntPtr hHandle, 
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_CTR_NOTIFY_SETTINGS pSettings);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetNextMessage")]
        public static extern int ZG_Ctr_GetNextMessage(IntPtr hHandle, ref UInt32 nMsg, ref IntPtr nMsgParam);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetNewAddr")]
        public static extern int ZG_Ctr_SetNewAddr(IntPtr hHandle, byte nNewAddr);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_AssignAddr")]
        public static extern int ZG_Ctr_AssignAddr(IntPtr hHandle, byte nAddr);

        [DllImport(ZgDllName, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_UpdateFirmware")]
        public static extern int ZG_Ctr_UpdateFirmware(IntPtr hHandle, [In] byte[] pData, int nCount,
            string pszInfoStr, ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_OpenLock")]
        public static extern int ZG_Ctr_OpenLock(IntPtr hHandle, int nLockN=0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_CloseLock")]
        public static extern int ZG_Ctr_CloseLock(IntPtr hHandle);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_EnableEmergencyUnlocking")]
        public static extern int ZG_Ctr_EnableEmergencyUnlocking(IntPtr hHandle, bool fEnable=true);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_IsEmergencyUnlockingEnabled")]
        public static extern int ZG_Ctr_IsEmergencyUnlockingEnabled(IntPtr hHandle, ref bool pEnabled);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadRegs")]
        public static extern int ZG_Ctr_ReadRegs(IntPtr hHandle, UInt32 nAddr, int nCount, [In, Out] byte[] pBuf);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadPorts")]
        public static extern int ZG_Ctr_ReadPorts(IntPtr hHandle, ref UInt32 pData);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ControlDevices")]
        public static extern int ZG_Ctr_ControlDevices(IntPtr hHandle, UInt32 nDevType, bool fActive, UInt32 nTimeMs=0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadData")]
        public static extern int ZG_Ctr_ReadData(IntPtr hHandle, int nBankN, UInt32 nAddr, int nCount,
            [In, Out] byte[] pBuf, ref int pReaded, 
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteData")]
        public static extern int ZG_Ctr_WriteData(IntPtr hHandle, int nBankN, UInt32 nAddr,
            [In, Out] byte[] pData, int nCount, ref int pWritten, 
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadLockTimes")]
        public static extern int ZG_Ctr_ReadLockTimes(IntPtr hHandle, 
            ref UInt32 pOpenMs, ref UInt32 pLetMs, ref UInt32 pMaxMs, int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteLockTimes")]
        public static extern int ZG_Ctr_WriteLockTimes(IntPtr hHandle, UInt32 nMask,
            UInt32 nOpenMs, UInt32 nLetMs, UInt32 nMaxMs, int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadTimeZones")]
        public static extern int ZG_Ctr_ReadTimeZones(IntPtr hHandle, int nIdx,
            [In, Out] ZG_CTR_TIMEZONE[] pBuf, int nCount, 
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteTimeZones")]
        public static extern int ZG_Ctr_WriteTimeZones(IntPtr hHandle, int nIdx,
            [In, Out] ZG_CTR_TIMEZONE[] pTzs, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteTimeZones")]
        public static extern int ZG_Ctr_WriteTimeZones(IntPtr hHandle, int nIdx,
            ref ZG_CTR_TIMEZONE pTzs, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_EnumTimeZones")]
        public static extern int ZG_Ctr_EnumTimeZones(IntPtr hHandle, int nStart,
            ZG_ENUMCTRTIMEZONESPROC fnEnumProc, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadKeys")]
        public static extern int ZG_Ctr_ReadKeys(IntPtr hHandle, int nIdx,
            [In, Out] ZG_CTR_KEY[] pBuf, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteKeys")]
        public static extern int ZG_Ctr_WriteKeys(IntPtr hHandle, int nIdx,
            [In, Out] ZG_CTR_KEY[] pKeys, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0, bool fUpdateTop=true);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ClearKeys")]
        public static extern int ZG_Ctr_ClearKeys(IntPtr hHandle, int nIdx, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr), int nBankN = 0, bool fUpdateTop = true);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetKeyTopIndex")]
        public static extern int ZG_Ctr_GetKeyTopIndex(IntPtr hHandle, ref int pIdx, int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_EnumKeys")]
        public static extern int ZG_Ctr_EnumKeys(IntPtr hHandle, int nStart,
            ZG_ENUMCTRKEYSPROC fnEnumProc, IntPtr pUserData = default(IntPtr), int nBankN = 0);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetClock")]
        public static extern int ZG_Ctr_GetClock(IntPtr hHandle, ref ZG_CTR_CLOCK pClock);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetClock")]
        public static extern int ZG_Ctr_SetClock(IntPtr hHandle, ref ZG_CTR_CLOCK pClock);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadLastKeyNum")]
        public static extern int ZG_Ctr_ReadLastKeyNum(IntPtr hHandle, [In, Out] byte[] pNum);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadRTCState")]
        public static extern int ZG_Ctr_ReadRTCState(IntPtr hHandle, ref ZG_CTR_CLOCK pClock,
            ref int pWrIdx, ref int pRdIdx, [In, Out] byte[] pNum);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadEventIdxs")]
        public static extern int ZG_Ctr_ReadEventIdxs(IntPtr hHandle, ref int pWrIdx, ref int pRdIdx);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteEventIdxs")]
        public static extern int ZG_Ctr_WriteEventIdxs(IntPtr hHandle, UInt32 nMask, int nWrIdx, int nRdIdx);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadEvents")]
        public static extern int ZG_Ctr_ReadEvents(IntPtr hHandle, int nIdx,
            [In, Out] ZG_CTR_EVENT[] pBuf, int nCount,
            ZG_PROCESSCALLBACK pfnCallback, IntPtr pUserData = default(IntPtr));

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_EnumEvents")]
        public static extern int ZG_Ctr_EnumEvents(IntPtr hHandle, int nStart, int nCount,
            ZG_ENUMCTREVENTSPROC fnEnumProc, IntPtr pUserData = default(IntPtr));
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodePassEvent")]
        /// <summary>
        /// Декодирование событий прохода
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="pDirect">Направление прохода</param>
        /// <param name="pKeyIdx">Индекс ключа в банке ключей</param>
        /// <param name="pKeyBank">Номер банка ключей</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodePassEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_CTR_DIRECT pDirect, ref int pKeyIdx, ref int pKeyBank);
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodeEcEvent")]
        /// <summary>
        /// Декодирование событий ElectoControl: ZG_EV_ELECTRO_ON, ZG_EV_ELECTRO_OFF
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="pSubEvent">Условие, вызвавшее событие</param>
        /// <param name="pPowerFlags">Флаги электропитания</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodeEcEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_EC_SUB_EV pSubEvent, ref UInt32 pPowerFlags);
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodeUnkKeyEvent")]
        /// <summary>
        /// Декодирование события со значением ключа: ZG_EV_KEY_VALUE
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pKeyNum">Номер ключа</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodeUnkKeyEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            [In, Out] byte[] pKeyNum);
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodeFireEvent")]
        /// <summary>
        /// Декодирование события ZG_EV_FIRE_STATE (Изменение состояния Пожара)
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="pSubEvent">Условие, вызвавшее событие</param>
        /// <param name="pFireFlags">Флаги состояния режима Пожар (ZG_FR_F_...)</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodeFireEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_FIRE_SUB_EV pSubEvent, ref UInt32 pFireFlags);
        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodeSecurEvent")]
        /// <summary>
        /// Декодирование события ZG_EV_SECUR_STATE (Изменение состояния Охрана)
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="pSubEvent">Условие, вызвавшее событие</param>
        /// <param name="pSecurFlags">Флаги состояния режима Охрана (ZG_SR_F_...)</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodeSecurEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_SECUR_SUB_EV pSubEvent, ref UInt32 pSecurFlags);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_DecodeModeEvent")]
        /// <summary>
        /// Декодирование события ZG_EV_MODE_STATE (Изменение состояния Режим)
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name=""></param>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="nCurrMode">Текущий режим</param>
        /// <param name="nSubEvent">Условие, вызвавшее событие</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_DecodeModeEvent(IntPtr hHandle, [In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_CTR_MODE nCurrMode, ref ZG_MODE_SUB_EV nSubEvent);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_DecodeHotelEvent")]
        /// <summary>
        /// Декодирование событий HOTEL: ZG_EV_HOTEL40 и ZG_EV_HOTEL41
        /// </summary>
        /// <param name="pData8">Данные события (8 байт)</param>
        /// <param name="pTime">Время события</param>
        /// <param name="nMode">Текущий режим</param>
        /// <param name="nSubEvent">Условие, вызвавшее событие</param>
        /// <param name="nFlags">Флаги состояния (ZG_HF_...)</param>
        /// <returns></returns>
        public static extern int ZG_DecodeHotelEvent([In, Out] byte[] pData8,
            ref ZG_EV_TIME pTime, ref ZG_HOTEL_MODE nMode, ref ZG_HOTEL_SUB_EV nSubEvent, ref UInt32 nFlags);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetFireMode")]
        public static extern int ZG_Ctr_SetFireMode(IntPtr hHandle, bool fOn=true);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetFireInfo")]
        /// <summary>
        /// Запрос состояния пожарного режима
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name="pFireFlags">Флаги состояния ZG_FR_F_...</param>
        /// <param name="pCurrTemp">Текущая температура</param>
        /// <param name="pSrcMask">Маска разрешенных источников ZG_FR_SRCF_...</param>
        /// <param name="pLimitTemp">Пороговая температура</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_GetFireInfo(IntPtr hHandle, ref UInt32 pFireFlags,
            ref UInt32 pCurrTemp, ref UInt32 pSrcMask, ref UInt32 pLimitTemp);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetFireConfig")]
        /// <summary>
        /// Установка параметров пожарного режима
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name="nSrcMask">Маска разрешенных источников ZG_FR_SRCF_...</param>
        /// <param name="nLimitTemp">Пороговая температура (в градусах)</param>
        /// <param name="pFireFlags">Флаги состояния ZG_FR_F_...</param>
        /// <param name="pCurrTemp">Текущая температура (в градусах)</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_SetFireConfig(IntPtr hHandle, UInt32 nSrcMask,
            UInt32 nLimitTemp, [Out] UInt32 nFireFlags, [Out] UInt32 nCurrTemp);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetSecurMode")]
        public static extern int ZG_Ctr_SetSecurMode(IntPtr hHandle, ZG_SECUR_MODE nMode);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetSecurInfo")]
        /// <summary>
        /// Запрос состояния режима Охрана
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name="nSecurFlags">Флаги состояния ZG_SR_F_</param>
        /// <param name="nSrcMask">Маска разрешенных источников ZG_SR_SRCF_...</param>
        /// <param name="nAlarmTime">Время звучания сирены (в секундах)</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_GetSecurInfo(IntPtr hHandle, ref UInt32 nSecurFlags,
            ref UInt32 nSrcMask, ref UInt32 nAlarmTime);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetSecurConfig")]
        /// <summary>
        /// Установка параметров режима Охрана
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name="nSrcMask">Маска разрешенных источников ZG_SR_SRCF_...</param>
        /// <param name="nAlarmTime">Время звучания сирены (в секундах)</param>
        /// <param name="nSecurFlags">Флаги состояния ZG_SR_F_</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_SetSecurConfig(IntPtr hHandle, UInt32 nSrcMask,
            UInt32 nAlarmTime, [Out] UInt32 nSecurFlags);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetCtrMode")]
        public static extern int ZG_Ctr_SetCtrMode(IntPtr hHandle, ZG_CTR_MODE nMode);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetCtrModeInfo")]
        /// <summary>
        /// Запрос состояния режима контроллера
        /// </summary>
        /// <param name="hHandle">Дескриптор контроллера</param>
        /// <param name="nCurrMode">Текущий режим</param>
        /// <param name="nFlags">Флаги</param>
        /// <returns></returns>
        public static extern int ZG_Ctr_GetCtrModeInfo(IntPtr hHandle, ref ZG_CTR_MODE nCurrMode,
            ref UInt32 nFlags);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_ReadElectroConfig")]
        public static extern int ZG_Ctr_ReadElectroConfig(IntPtr hHandle, ref ZG_CTR_ELECTRO_CONFIG pConfig);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_WriteElectroConfig")]
        public static extern int ZG_Ctr_WriteElectroConfig(IntPtr hHandle, ref ZG_CTR_ELECTRO_CONFIG pConfig,
            bool fSetTz=true);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_GetElectroState")]
        public static extern int ZG_Ctr_GetElectroState(IntPtr hHandle, ref ZG_CTR_ELECTRO_STATE pState);

        [DllImport(ZgDllName, CallingConvention = CallingConvention.StdCall, EntryPoint = "ZG_Ctr_SetElectroPower")]
        public static extern int ZG_Ctr_SetElectroPower(IntPtr hHandle, bool fOn=true);


        // Устаревшие функции

        [Obsolete("use ZG_DD_SetNotification")]
        public static int ZG_FindNotification(ref IntPtr pHandle, ref ZP_DD_NOTIFY_SETTINGS pSettings, bool fSerial, bool fIP)
        {
            return ZG_SetNotification(ref pHandle, ref pSettings, fSerial, fIP);
        }
        [Obsolete("use ZG_GetNextMessage")]
        public static int ZG_ProcessMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData)
        {
            return ZG_EnumMessages(hHandle, pEnumProc, pUserData);
        }
        [Obsolete("use ZG_GetNextMessage")]
        public static int ZG_EnumMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData)
        {
            return ZPIntf.ZP_EnumMessages(hHandle, pEnumProc, pUserData);
        }
        [Obsolete("use ZG_Cvt_SetNotification")]
        public static int ZG_Cvt_FindNotification(IntPtr hHandle,
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_CVT_NOTIFY_SETTINGS pSettings)
        {
            return ZG_Cvt_SetNotification(hHandle, pSettings);
        }
        [Obsolete("use ZG_Cvt_GetNextMessage")]
        public static int ZG_Cvt_ProcessMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData = default(IntPtr))
        {
            return ZG_Cvt_EnumMessages(hHandle, pEnumProc, pUserData);
        }
        [Obsolete("use ZG_Cvt_GetNextMessage")]
        public static int ZG_Cvt_EnumMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData = default(IntPtr))
        {
            int hr;
            UInt32 nMsg = 0;
            IntPtr nMsgParam = IntPtr.Zero;
            while ((hr = ZG_Cvt_GetNextMessage(hHandle, ref nMsg, ref nMsgParam)) == S_OK)
                pEnumProc(nMsg, nMsgParam, pUserData);
            if (hr == ZPIntf.ZP_S_NOTFOUND)
                hr = S_OK;
            return hr;
        }
        [Obsolete("use ZG_Ctr_SetNotification")]
        public static int ZG_Ctr_FindNotification(IntPtr hHandle,
            [In, Out] [MarshalAs(UnmanagedType.LPStruct)] ZG_CTR_NOTIFY_SETTINGS pSettings)
        {
            return ZG_Ctr_SetNotification(hHandle, pSettings);
        }
        [Obsolete("use ZG_Ctr_GetNextMessage")]
        public static int ZG_Ctr_ProcessMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData)
        {
            return ZG_Ctr_EnumMessages(hHandle, pEnumProc, pUserData);
        }
        [Obsolete("use ZG_Ctr_GetNextMessage")]
        public static int ZG_Ctr_EnumMessages(IntPtr hHandle, ZP_NOTIFYPROC pEnumProc, IntPtr pUserData)
        {
            int hr;
            UInt32 nMsg = 0;
            IntPtr nMsgParam = IntPtr.Zero;
            while ((hr = ZG_Ctr_GetNextMessage(hHandle, ref nMsg, ref nMsgParam)) == S_OK)
                pEnumProc(nMsg, nMsgParam, pUserData);
            if (hr == ZPIntf.ZP_S_NOTFOUND)
                hr = S_OK;
            return hr;
        }

        [Obsolete("use ZG_GetPortInfoList")]
        public static int ZG_EnumSerialPorts(ZP_ENUMPORTSPROC pEnumProc, IntPtr pUserData)
        {
            int hr;
            int nPortCount = 0;
            IntPtr hList = new IntPtr();
            hr = ZGIntf.ZG_GetPortInfoList(ref hList, ref nPortCount);
            if (hr < 0)
                return hr;
            try
            {
                ZP_PORT_INFO rPI = new ZP_PORT_INFO();
                for (int i = 0; i < nPortCount; i++)
                {
                    ZPIntf.ZP_GetPortInfo(hList, i, ref rPI);
                    if (!pEnumProc(ref rPI, pUserData))
                        return ZPIntf.ZP_S_CANCELLED;
                }
            }
            finally
            {
                ZGIntf.ZG_CloseHandle(hList);
            }
            return hr;
        }
        [Obsolete("use ZG_SearchDevices")]
        public static int ZP_EnumSerialDevices(UInt32 nTypeMask,
            [In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount, ZG_ENUMCVTSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = (ZPIntf.ZP_SF_UPDATE|ZPIntf.ZP_SF_USEVCOM))
        {
            int hr;
            ZP_SEARCH_PARAMS rParams = new ZP_SEARCH_PARAMS();
            ZG_ENUM_CVT_INFO pDI = new ZG_ENUM_CVT_INFO();
            ZP_PORT_INFO[] aPIs = new ZP_PORT_INFO[2];
            ZG_ENUM_CVT_INFO pInfo;
            IntPtr p = IntPtr.Zero;
            int nPortCount;
            IntPtr hSearch = IntPtr.Zero;
            rParams.nDevMask = nTypeMask;
            if ((nFlags & ZPIntf.ZP_SF_USEVCOM) != 0)
                rParams.nFlags |= ZPIntf.ZP_SF_USECOM;
            try
            {
                if (nPCount > 0)
                {
                    rParams.pPorts = Marshal.AllocHGlobal(Marshal.SizeOf(pPorts));
                    Marshal.StructureToPtr(pPorts, rParams.pPorts, true);
                }
                if (pWS != null)
                {
                    rParams.pWait = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ZP_WAIT_SETTINGS)));
                    Marshal.StructureToPtr(pWS, rParams.pWait, true);
                }
                hr = ZPIntf.ZP_SearchDevices(ref hSearch, ref rParams);
                if (hr < 0)
                    return hr;
                pDI.rBase.cbSize = (UInt32)Marshal.SizeOf(pDI);
                p = Marshal.AllocHGlobal(Marshal.SizeOf(pDI));
                Marshal.StructureToPtr(pDI, p, true);
                nPortCount = 0;
                while ((hr = ZPIntf.ZP_FindNextDevice(hSearch, p, aPIs, aPIs.Length, ref nPortCount)) == S_OK)
                {
                    for (int i = 0; i < nPortCount; i++)
                    {
                        pInfo = (ZG_ENUM_CVT_INFO)Marshal.PtrToStructure(p, typeof(ZG_ENUM_CVT_INFO));
                        if (!pEnumProc(ref pInfo, ref aPIs[i], pUserData))
                            return ZPIntf.ZP_S_CANCELLED;
                    }
                    pDI.rBase.cbSize = (UInt32)Marshal.SizeOf(pDI);
                    Marshal.StructureToPtr(pDI, p, true);
                }
            }
            finally
            {
                if (p != IntPtr.Zero)
                    Marshal.FreeHGlobal(p);
                if (rParams.pWait != IntPtr.Zero)
                    Marshal.FreeHGlobal(rParams.pWait);
                if (rParams.pPorts != IntPtr.Zero)
                    Marshal.FreeHGlobal(rParams.pPorts);
                if (hSearch != IntPtr.Zero)
                    ZG_CloseHandle(hSearch);
            }
            return hr;
        }

        [Obsolete("use ZG_SearchDevices")]
        public static int ZG_EnumConverters([In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount, ZG_ENUMCVTSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = (ZPIntf.ZP_SF_UPDATE|ZPIntf.ZP_SF_USEVCOM))
        {
            return ZP_EnumSerialDevices(ZG_DEVTYPE_CVTS, pPorts, nPCount, pEnumProc, pUserData, pWS, nFlags);
        }

        [Obsolete("use ZG_SearchDevices")]
        public static int ZP_EnumIpDevices(UInt32 nTypeMask,
            ZG_ENUMIPCVTSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = ZPIntf.ZP_SF_UPDATE)
        {
            int hr;
            ZP_SEARCH_PARAMS rParams = new ZP_SEARCH_PARAMS();
            ZG_ENUM_IPCVT_INFO pDI = new ZG_ENUM_IPCVT_INFO();
            ZP_PORT_INFO[] aPIs = new ZP_PORT_INFO[2];
            ZG_ENUM_IPCVT_INFO pInfo2;
            IntPtr hSearch = IntPtr.Zero;
            IntPtr p = IntPtr.Zero;
            int nPortCount;
            rParams.nIpDevMask = nTypeMask;
            try
            {
                if (pWS != null)
                {
                    rParams.pWait = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ZP_WAIT_SETTINGS)));
                    Marshal.StructureToPtr(pWS, rParams.pWait, true);
                }
                hr = ZPIntf.ZP_SearchDevices(ref hSearch, ref rParams);
                if (hr < 0)
                    return hr;
                pDI.rBase.cbSize = (UInt32)Marshal.SizeOf(typeof(ZG_ENUM_IPCVT_INFO));
                p = Marshal.AllocHGlobal(Marshal.SizeOf(pDI));
                Marshal.StructureToPtr(pDI, p, true);
                nPortCount = 0;
                while ((hr = ZPIntf.ZP_FindNextDevice(hSearch, p, aPIs, aPIs.Length, ref nPortCount)) == S_OK)
                {
                    for (int i = 0; i < nPortCount; i++)
                    {
                        pInfo2 = (ZG_ENUM_IPCVT_INFO)Marshal.PtrToStructure(p, typeof(ZG_ENUM_IPCVT_INFO));
                        if (!pEnumProc(ref pInfo2, ref aPIs[i], pUserData))
                            return ZPIntf.ZP_S_CANCELLED;
                    }
                    pDI.rBase.cbSize = (UInt32)Marshal.SizeOf(typeof(ZG_ENUM_IPCVT_INFO));
                    Marshal.StructureToPtr(pDI, p, true);
                }
            }
            finally
            {
                if (p != IntPtr.Zero)
                    Marshal.FreeHGlobal(p);
                if (rParams.pWait != IntPtr.Zero)
                    Marshal.FreeHGlobal(rParams.pWait);
                if (hSearch != IntPtr.Zero)
                    ZG_CloseHandle(hSearch);
            }
            return hr;
        }

        [Obsolete("use ZG_SearchDevices")]
        public static int ZG_EnumIpConverters(ZG_ENUMIPCVTSPROC pEnumProc, IntPtr pUserData,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = ZPIntf.ZP_SF_UPDATE)
        {
            return ZP_EnumIpDevices(1, pEnumProc, pUserData, pWS, nFlags);
        }


        [Obsolete("use ZG_SearchDevices")]
        public static int ZG_FindConverter([In] [MarshalAs(UnmanagedType.LPArray)] ZP_PORT_ADDR[] pPorts, int nPCount,
            [MarshalAs(UnmanagedType.LPStruct)] ref ZG_ENUM_CVT_INFO pInfo, ref ZP_PORT_INFO pPort,
            [In] [MarshalAs(UnmanagedType.LPStruct)] ZP_WAIT_SETTINGS pWS = null, UInt32 nFlags = (ZPIntf.ZP_SF_UPDATE|ZPIntf.ZP_SF_USEVCOM))
        {
            int hr;
            ZP_SEARCH_PARAMS rParams = new ZP_SEARCH_PARAMS();
            ZG_ENUM_CVT_INFO pDI = new ZG_ENUM_CVT_INFO();
            ZP_PORT_INFO[] aPIs = new ZP_PORT_INFO[2];
            IntPtr p = IntPtr.Zero;
            int nPortCount;
            IntPtr hSearch = IntPtr.Zero;
            rParams.nDevMask = ZG_DEVTYPE_CVTS;
            if ((nFlags & ZPIntf.ZP_SF_USEVCOM) != 0)
                rParams.nFlags |= ZPIntf.ZP_SF_USECOM;
            try
            {
                if (nPCount > 0)
                {
                    rParams.pPorts = Marshal.AllocHGlobal(Marshal.SizeOf(pPorts));
                    Marshal.StructureToPtr(pPorts, rParams.pPorts, true);
                }
                if (pWS != null)
                {
                    rParams.pWait = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ZP_WAIT_SETTINGS)));
                    Marshal.StructureToPtr(pWS, rParams.pWait, true);
                }
                hr = ZPIntf.ZP_SearchDevices(ref hSearch, ref rParams);
                if (hr < 0)
                    return hr;
                pDI.rBase.cbSize = (UInt32)Marshal.SizeOf(pDI);
                p = Marshal.AllocHGlobal(Marshal.SizeOf(pDI));
                Marshal.StructureToPtr(pDI, p, true);
                nPortCount = 0;
                hr = ZPIntf.ZP_FindNextDevice(hSearch, p, aPIs, aPIs.Length, ref nPortCount);
                if (hr == S_OK)
                {
                    pInfo = (ZG_ENUM_CVT_INFO)Marshal.PtrToStructure(p, typeof(ZG_ENUM_CVT_INFO));
                    pPort = aPIs[0];
                }
            }
            finally
            {
                if (p != IntPtr.Zero)
                    Marshal.FreeHGlobal(p);
                if (rParams.pWait != IntPtr.Zero)
                    Marshal.FreeHGlobal(rParams.pWait);
                if (rParams.pPorts != IntPtr.Zero)
                    Marshal.FreeHGlobal(rParams.pPorts);
                if (hSearch != IntPtr.Zero)
                    ZG_CloseHandle(hSearch);
            }
            return hr;
        }

        // Utils

        // Преобразовать номер ключа в строку
        public static string CardNumToStr(Byte[] aNum, bool fProximity)
        {
            string s;
            if (fProximity)
            {
                s = String.Format("[{0:X2}{1:X2}] {2:D3},{3:D5}", aNum[5], aNum[4], aNum[3], aNum[1] + (aNum[2] << 8));
            }
            else
            {
                s = "";
                for (int i = aNum[0]; i > 0; i--)
                    s = s + aNum[i].ToString("X2");
            }
            return s;
        }
    }
}
