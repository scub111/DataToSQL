using System;
using RapidInterface;
using DevExpress.Xpo;

namespace DataToSQL
{
    #region Item
    /// <summary>
    /// Таблица "Элемент".
    /// </summary>
    [DBAttribute(Caption = "Элемент", IconFile = "Item.png")]
    public class Item : XPObjectEx
    {
        public Item() : base(Session.DefaultSession) { }

        public Item(Session session) : base(session) { }

        /// <summary>
        /// Последний выбранный источник данных.
        /// </summary>
        public static DataSource DataSourceLast { get; set; }

        DataSource _DataSource;
        /// <summary>
        /// Источник.
        /// </summary>
        [DisplayName("Источник")]
        public DataSource DataSource
        {
            get { return _DataSource; }
            set 
            {
                SetPropertyValueEx("DataSource", ref _DataSource, value);

                if (!IsLoading)
                {
                    string strPrefix = "";
                    if (DataSource != null)
                        strPrefix = DataSource.DescriptionPrefix;
                    if (Description == null || Description == "")
                        Description = strPrefix;

                    DataSourceLast = value;
                }
            }
        }

        string _ItemID;
        /// <summary>
        /// ID.
        /// </summary>
        [DisplayName("ID")]
        [Size(SizeAttribute.Unlimited)]
        public string ItemID
        {
            get { return _ItemID; }
            set
            {
                SetPropertyValueEx("ItemID", ref _ItemID, value);

                if (!IsLoading)
                {
                    string sqlPrefix = "";
                    if (DataSource != null)
                        sqlPrefix = DataSource.SQLPrefix;
                    if (SQLTableName == null || SQLTableName == "")
                        SQLTableName = GetSQLTableName(sqlPrefix + ItemID);
                }
            }
        }

        string _SQLTableName;
        /// <summary>
        /// SQL-таблица.
        /// </summary>
        [DisplayName("SQL-таблица")]
        [Size(SizeAttribute.Unlimited)]
        public string SQLTableName
        {
            get { return _SQLTableName; }
            set { SetPropertyValueEx("SQLTableName", ref _SQLTableName, value); }
        }

        string _Description;
        /// <summary>
        /// Описание.
        /// </summary>
        [DisplayName("Описание")]
        [Size(SizeAttribute.Unlimited)]
        public string Description
        {
            get { return _Description; }
            set { SetPropertyValueEx("Description", ref _Description, value); }
        }

        string _Unit;
        /// <summary>
        /// Единица измерения.
        /// </summary>
        [DisplayName("Ед. изм.")]
        [Size(SizeAttribute.Unlimited)]
        public string Unit
        {
            get { return _Unit; }
            set { SetPropertyValueEx("Unit", ref _Unit, value); }
        }

        string _FormatValue;
        /// <summary>
        /// Формат значения.
        /// </summary>
        [DisplayName("Формат изм.")]
        [Size(SizeAttribute.Unlimited)]
        public string FormatValue
        {
            get { return _FormatValue; }
            set { SetPropertyValueEx("FormatValue", ref _FormatValue, value); }
        }

        double _MinValue;
        /// <summary>
        /// Минимальное значение.
        /// </summary>
        [DisplayName("Мин. значение")]
        public double MinValue
        {
            get { return _MinValue; }
            set { SetPropertyValueEx("MinValue", ref _MinValue, value); }
        }

        double _MaxValue;
        /// <summary>
        /// Максимальное значение.
        /// </summary>
        [DisplayName("Макс. значение")]
        public double MaxValue
        {
            get { return _MaxValue; }
            set { SetPropertyValueEx("MaxValue", ref _MaxValue, value); }
        }

        bool _SQLTrend;
        /// <summary>
        /// Тренд.
        /// </summary>
        [DisplayName("Тренд")]
        public bool SQLTrend
        {
            get { return _SQLTrend; }
            set { SetPropertyValueEx("SQLTrend", ref _SQLTrend, value); }
        }

        int _TimeOut;
        /// <summary>
        /// Таймаут, c.
        /// </summary>
        [DisplayName("Таймаут, с")]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { SetPropertyValueEx("TimeOut", ref _TimeOut, value); }
        }

        string _Comment;
        /// <summary>
        /// Комментарий.
        /// </summary>
        [DisplayName("Комментарий")]
        [Size(SizeAttribute.Unlimited)]
        public string Comment
        {
            get { return _Comment; }
            set { SetPropertyValueEx("Comment", ref _Comment, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Объект реального времени.
        /// </summary>
        [NonPersistent]
        [DisplayName("Объект")]
        public ItemReal ItemReal { get; set; }

        /// <summary>
        /// Тип переменной.
        /// </summary>
        [DisplayName("Тип переменной")]
        [NonPersistent]
        public string CanonicalDataType 
        {
            get
            {
                if (ItemReal != null)
                    return ItemReal.CanonicalDataType.ToString();
                else
                    return "null";
            }
        }

        /// <summary>
        /// Значение.
        /// </summary>
        [DisplayName("Значение")]
        [NonPersistent]
        public string DataValue
        {
            get
            {
                if (ItemReal != null && 
                    ItemReal.DataValue != null)
                    return ItemReal.DataValue.ToString();
                else
                    return "null";
            }
        }

        /// <summary>
        /// Качество.
        /// </summary>
        [DisplayName("Качество")]
        [NonPersistent]
        public short Quality
        {
            get
            {
                if (ItemReal != null &&
                    ItemReal.Quality != 0)
                    return ItemReal.Quality;
                else
                    return -1;
            }
        }

        /// <summary>
        /// Качество.
        /// </summary>
        [DisplayName("Качество опр.")]
        [NonPersistent]
        public string QualityDef
        {
            get
            {
                if (ItemReal != null &&
                    ItemReal.QualityDef != null)
                    return ItemReal.QualityDef.ToString();
                else
                    return "null";
            }
        }

        /// <summary>
        /// Штамп времени.
        /// </summary>
        [DisplayName("Время устройства")]
        [NonPersistent]
        public DateTime DeviceTime
        {
            get
            {
                if (ItemReal != null)
                    return ItemReal.DeviceTime;
                else
                    return new DateTime(1901, 1, 1);
            }
        }

        /// <summary>
        /// Получение нормального имени SQL-таблицы.
        /// </summary>
        public static string GetSQLTableName(string strIn)
        {
            string strTemp = "";
            char ch;
            for (int i = 0; i < strIn.Length; i++)
            {
                ch = strIn[i];
                if (ch == '.' || ch == ' ' || ch == '[' || ch == ']')
                    ch = '_';
                strTemp += ch;
            }

            return strTemp;
        }

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                DataSource = DataSourceLast;
                SQLTrend = true;
                MinValue = 0;
                MaxValue = 1;
                TimeOut = 20;
                FormatValue = "0.##";
            }
        }
    }
    #endregion

    #region DataSource
    /// <summary>
    /// Таблица "Источник".
    /// </summary>
    [DBAttribute(Caption = "Источник", IconFile = "DataSource.png")]
    public class DataSource : XPObjectEx
    {
        public DataSource() : base(Session.DefaultSession) { }

        public DataSource(Session session) : base(session) { }

        string _Caption;
        /// <summary>
        /// Название.
        /// </summary>
        [DisplayName("Название")]
        [Size(SizeAttribute.Unlimited)]
        public string Caption
        {
            get { return _Caption; }
            set { SetPropertyValueEx("Caption", ref _Caption, value); }
        }

        string _Comment;
        /// <summary>
        /// Комментарий.
        /// </summary>
        [DisplayName("Комментарий")]
        [Size(SizeAttribute.Unlimited)]
        public string Comment
        {
            get { return _Comment; }
            set { SetPropertyValueEx("Comment", ref _Comment, value); }
        }

        string _SQLPrefix;
        /// <summary>
        /// SQL-префикс.
        /// </summary>
        [DisplayName("SQL-префикс")]
        [Size(SizeAttribute.Unlimited)]
        public string SQLPrefix
        {
            get { return _SQLPrefix; }
            set { SetPropertyValueEx("SQLPrefix", ref _SQLPrefix, value); }
        }

        string _DescriptionPrefix;
        /// <summary>
        /// Префикс в описании.
        /// </summary>
        [DisplayName("Префикс в описании")]
        [Size(SizeAttribute.Unlimited)]
        public string DescriptionPrefix
        {
            get { return _DescriptionPrefix; }
            set { SetPropertyValueEx("DescriptionPrefix", ref _DescriptionPrefix, value); }
        }

        /// <summary>
        /// Текст для показа в выпадающем списке.
        /// </summary>
        public override string DisplayMember
        {
            get
            {
                return string.Format("{0}/{1}", Caption, Comment);
            }
        }

        bool _CursorDeactivation;
        /// <summary>
        /// Деактивация считывания данных при изменении положения курсора (ДК).
        /// </summary>
        [DisplayName("Курсор-деактивация")]
        public bool CursorDeactivation
        {
            get { return _CursorDeactivation; }
            set { SetPropertyValueEx("CursorDeactivation", ref _CursorDeactivation, value); }
        }

        int _CursorDeactivationLimit;
        /// <summary>
        /// Предел ДК.
        /// </summary>
        [DisplayName("Предел ДК")]
        public int CursorDeactivationLimit
        {
            get { return _CursorDeactivationLimit; }
            set { SetPropertyValueEx("CursorDeactivationLimit", ref _CursorDeactivationLimit, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Объект реального времени.
        /// </summary>
        [NonPersistent]
        [DisplayName("Объект")]
        public DataSourceReal SourceReal { get; set; }

        /// <summary>
        /// Если соединение с сервером.
        /// </summary>
        [NonPersistent]
        [DisplayName("Соединен")]
        public bool IsConnected
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.IsConnected;
                else
                    return false;
            }
        }
 
        /// <summary>
        /// Количество ОРС-элементов.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во элементов")]
        public int ItemsCount
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.Items.Count;
                else return 0;
            }
        }

        /// <summary>
        /// Количество попыток подключения.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во удач. подкл.")]
        public int ConnectSuccessCount
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.ConnectSuccessCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Количество попыток отключения.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во ошиб. покл.")]
        public int ConnectFaultCount
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.ConnectFaultCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Количество полученных запросов.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во получ.")]
        public int ReceiveSuccessCount
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.ReceiveSuccessCount;
                else
                    return 0;
            }
        }
        
        /// <summary>
        /// Количество неполученных запросов.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во неполуч.")]
        public int ReceiveFaultCount
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.ReceiveFaultCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Последнее обновление данных.
        /// </summary>
        [NonPersistent]
        [DisplayName("Последнее обновление")]
        public DateTime UpdateTime
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.UpdateTime;
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Время чтения данных с ОРС-сервера.
        /// </summary>
        [NonPersistent]
        [DisplayName("Время чтения, мс")]
        public double ReadTimeSpan
        {
            get
            {
                if (SourceReal != null) return
                    SourceReal.ReadTimeSpan.TotalMilliseconds;
                else return 0;
            }
        }

        /// <summary>
        /// Оставшиеся циклы до активации.
        /// </summary>
        [NonPersistent]
        [DisplayName("Осталось до активации")]
        public int CursorDeactivationLost
        {
            get
            {
                if (SourceReal != null)
                    return SourceReal.CursorDeactivationLost;
                else
                    return -1;
            }
        }

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                CursorDeactivationLimit = 20;
            }
        }
    }
    #endregion    

    #region Statistics
    /// <summary>
    /// Таблица "COM-порт".
    /// </summary>
    [DBAttribute(Caption = "Статистика", IconFile = "Statistics.png")]
    public class Statistics : DataSource
    {
        public Statistics() : base(Session.DefaultSession) { }

        public Statistics(Session session) : base(session) { }
    }
    #endregion
   
    #region OPCServer
    /// <summary>
    /// Таблица "OPC-сервер".
    /// </summary>
    [DBAttribute(Caption = "OPC", IconFile = "OPCServer.png")]
    public class OPCServer : DataSource
    {
        public OPCServer() : base(Session.DefaultSession) { }

        public OPCServer(Session session) : base(session) { }

        string _Server;
        /// <summary>
        /// Сервер.
        /// </summary>
        [DisplayName("Сервер")]
        [Size(SizeAttribute.Unlimited)]
        public string Server
        {
            get { return _Server; }
            set { SetPropertyValueEx("Server", ref _Server, value); }
        }

        string _ProgID;
        /// <summary>
        /// Служба.
        /// </summary>
        [DisplayName("Служба")]
        [Size(SizeAttribute.Unlimited)]
        public string ProgID
        {
            get { return _ProgID; }
            set { SetPropertyValueEx("ProgID", ref _ProgID, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
    #endregion

    #region Vzljot
    /// <summary>
    /// Таблица "Взлет".
    /// </summary>
    [DBAttribute(Caption = "Взлет", IconFile = "Vzljot.png")]
    public class Vzljot : OPCServer
    {
        public Vzljot() : base(Session.DefaultSession) { }

        public Vzljot(Session session) : base(session) { }

        string _Config;
        /// <summary>
        /// Кофигурация.
        /// </summary>
        [DisplayName("Кофигурация")]
        [Size(SizeAttribute.Unlimited)]
        public string Config
        {
            get { return _Config; }
            set { SetPropertyValueEx("Config", ref _Config, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }
    #endregion

    #region DDEServer
    /// <summary>
    /// Таблица "DDE-сервер".
    /// </summary>
    [DBAttribute(Caption = "DDE", IconFile = "DDEServer.png")]
    public class DDEServer : DataSource
    {
        public DDEServer() : base(Session.DefaultSession) { }

        public DDEServer(Session session) : base(session) { }

        string _Server;
        /// <summary>
        /// Сервер.
        /// </summary>
        [DisplayName("Сервер")]
        [Size(SizeAttribute.Unlimited)]
        public string Server
        {
            get { return _Server; }
            set { SetPropertyValueEx("Server", ref _Server, value); }
        }

        string _Topic;
        /// <summary>
        /// Служба.
        /// </summary>
        [DisplayName("Служба")]
        [Size(SizeAttribute.Unlimited)]
        public string Topic
        {
            get { return _Topic; }
            set { SetPropertyValueEx("Topic", ref _Topic, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                Server = "View";
                Topic = "Tagname";
                Comment = "InTouch DDE-server";
            }
        }
    }
    #endregion

    #region ComPort
    /// <summary>
    /// Таблица "COM-порт".
    /// </summary>
    [DBAttribute(Caption = "COM-порт", IconFile = "ComPort.png")]
    public class ComPort : DataSource
    {
        public ComPort() : base(Session.DefaultSession) { }

        public ComPort(Session session) : base(session) { }

        string _PortName;
        /// <summary>
        /// COM-порт.
        /// </summary>
        [DisplayName("COM-порт")]
        [Size(SizeAttribute.Unlimited)]
        public string PortName
        {
            get { return _PortName; }
            set { SetPropertyValueEx("PortName", ref _PortName, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                PortName = "COM1";
            }
        }
    }
    #endregion

    #region Technograph
    /// <summary>
    /// Таблица "Технограф".
    /// </summary>
    [DBAttribute(Caption = "Технограф", IconFile = "Technograph.png")]
    public class Technograph : ComPort
    {
        public Technograph() : base(Session.DefaultSession) { }

        public Technograph(Session session) : base(session) { }

        bool _ReplacePoint;        
        /// <summary>
        /// Заменять точку на запятую или нет
        /// </summary>
        [DisplayName("Замена точки на запятую")]
        public bool ReplacePoint
        {
            get { return _ReplacePoint; }
            set { SetPropertyValueEx("ReplacePoint", ref _ReplacePoint, value); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Сообщение с COM-порта.
        /// </summary>
        [NonPersistent]
        [DisplayName("Сообщение")]
        public string Message
        {
            get
            {
                if (SourceReal != null)
                    return ((TechnographReal)SourceReal).Message;
                else
                    return "[null]";
            }
        }
    }
    #endregion

    #region PingServer
    /// <summary>
    /// Таблица "Пинг-сервер".
    /// </summary>
    [DBAttribute(Caption = "Пинг", IconFile = "PingServer.png")]
    public class PingServer : DataSource
    {
        public PingServer() : base(Session.DefaultSession) { }

        public PingServer(Session session) : base(session) { }

        /// <summary>
        /// Последний выбранный адрес.
        /// </summary>
        public static string AddressLast { get; set; }

        string _Address;
        /// <summary>
        /// Адрес.
        /// </summary>
        [DisplayName("Адрес")]
        [Size(SizeAttribute.Unlimited)]
        public string Address
        {
            get { return _Address; }
            set 
            { 
                SetPropertyValueEx("Address", ref _Address, value);
                /*
                if (!IsLoading)
                {
                    if (SQLPrefix == null || SQLPrefix == "")
                        SQLPrefix = "ping_" + Item.GetSQLTableName(Address) + "_";
                }
                */

                AddressLast = value;
            }
        }

        int _TimeOut;
        /// <summary>
        /// Таймаут, мс.
        /// </summary>
        [DisplayName("Таймаут, мс")]
        public int TimeOut
        {
            get { return _TimeOut; }
            set { SetPropertyValueEx("TimeOut", ref _TimeOut, value); }
        }

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                //Address = "172.31.";
                Address = AddressLast;
                TimeOut = 3000;
            }
        }
    }
    #endregion

    #region KMAZSServer
    /// <summary>
    /// Таблица "Пинг-сервер".
    /// </summary>
    [DBAttribute(Caption = "КМАЗС", IconFile = "KMAZSServer.png")]
    public class KMAZSServer : DataSource
    {
        public KMAZSServer() : base(Session.DefaultSession) { }

        public KMAZSServer(Session session) : base(session) { }

        string _ConnectionString;
        /// <summary>
        /// Строка подключения.
        /// </summary>
        [DisplayName("Строка подключения")]
        [Size(SizeAttribute.Unlimited)]
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { SetPropertyValueEx("ConnectionString", ref _ConnectionString, value); }
        }

        int _TANKCONFIGTimeOut;
        /// <summary>
        /// Таймаут для таблицы "TANKCONFIG", с.
        /// </summary>
        [DisplayName("Таймаут TANKCONFIG, с ")]
        public int TANKCONFIGTimeOut
        {
        	get	{ return _TANKCONFIGTimeOut; }
        	set	{ SetPropertyValueEx("TANKCONFIGTimeOut", ref _TANKCONFIGTimeOut, value); }
        }

        bool _TIDELOGSync;
        /// <summary>
        /// Синхронизация TIDELOG.
        /// </summary>
        [DisplayName("Синхронизация TIDELOG")]
        public bool TIDELOGSync
        {
            get { return _TIDELOGSync; }
            set { SetPropertyValueEx("TIDELOGSync", ref _TIDELOGSync, value); }
        }

        string _TIDELOGTableName;
        /// <summary>
        /// Название TIDELOG.
        /// </summary>
        [DisplayName("Название TIDELOG")]
        [Size(SizeAttribute.Unlimited)]
        public string TIDELOGTableName
        {
            get { return _TIDELOGTableName; }
            set { SetPropertyValueEx("TIDELOGTableName", ref _TIDELOGTableName, value); }
        }

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                ConnectionString = @"DataSource=172.24.92.120;User=SYSDBA;Password=masterkey;Database=C:\KMAZS\DataBase\KMAZSBASE.FDB";
                TANKCONFIGTimeOut = 3600;
                TIDELOGSync = true;
                TIDELOGTableName = "Angidrit_TIDELOG";
            }
        }
    }
    #endregion

    #region SQLServer
    /// <summary>
    /// Таблица "SQL-сервер".
    /// </summary>
    [DBAttribute(Caption = "SQL", IconFile = "SQLServer.png")]
    public class SQLServer : XPObjectEx
    {
        public SQLServer() : base(Session.DefaultSession) { }

        public SQLServer(Session session) : base(session) { }

        string _SQLName;
        /// <summary>
        /// Имя сервера.
        /// </summary>
        [DisplayName("Имя сервера")]
        [Size(SizeAttribute.Unlimited)]
        public string SQLName
        {
            get { return _SQLName; }
            set { SetPropertyValueEx("SQLName", ref _SQLName, value); }
        }

        string _ConnectionString;
        /// <summary>
        /// Строка подключения.
        /// </summary>
        [DisplayName("Строка подключения")]
        [Size(SizeAttribute.Unlimited)]
        public string ConnectionString
        {
            get { return _ConnectionString; }
            set { SetPropertyValueEx("ConnectionString", ref _ConnectionString, value); }
        }

        string _Comment;
        /// <summary>
        /// Комментарий.
        /// </summary>
        [DisplayName("Комментарий")]
        [Size(SizeAttribute.Unlimited)]
        public string Comment
        {
            get { return _Comment; }
            set { SetPropertyValueEx("Comment", ref _Comment, value); }
        }

        string _DateTimeFormat;
        /// <summary>
        /// Формат времени.
        /// </summary>
        [DisplayName("Формат времени")]
        [Size(SizeAttribute.Unlimited)]
        public string DateTimeFormat
        {
            get { return _DateTimeFormat; }
            set { SetPropertyValueEx("DateTimeFormat", ref _DateTimeFormat, value); }
        }

        bool _SendAll;
        /// <summary>
        /// Активность.
        /// </summary>
        [DisplayName("Отправить все")]
        public bool SendAll
        {
            get { return _SendAll; }
            set { SetPropertyValueEx("Active", ref _SendAll, value); }
        }

        int _ThreadCount;
        /// <summary>
        /// Кол-во потоков.
        /// </summary>
        [DisplayName("Кол-во потоков")]
        public int ThreadCount
        {
            get { return _ThreadCount; }
            set { SetPropertyValueEx("ThreadCount", ref _ThreadCount, value); }
        }

        /// <summary>
        /// Таблица-коллекция "Исключительные элементы".
        /// </summary>
        [DisplayName("Отправляемые элементы")]
        [Association("SQLServer-SQLServerItemForceCollection"), Aggregated]
        public XPCollection<SQLServerItemForceCollection> SQLServerItemForceCollection
        {
            get { return GetCollection<SQLServerItemForceCollection>("SQLServerItemForceCollection"); }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Объект реального времени.
        /// </summary>
        [NonPersistent]
        [DisplayName("Объект")]
        public SQLServerReal SQLServerReal { get; set; }

        /// <summary>
        /// Есть ли соединение с сервером.
        /// </summary>
        [NonPersistent]
        [DisplayName("Соединен")]
        public bool IsConnected
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.IsSending;
                else
                    return false;
            }
        }

        /// <summary>
        /// Инициализация таблиц.
        /// </summary>
        [NonPersistent]
        [DisplayName("Инициализация")]
        public bool TableInitiated
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.TableInitiated;
                else
                    return false;
            }
        }

        /// <summary>
        /// Есть ли отправка данных в сервер.
        /// </summary>
        [NonPersistent]
        [DisplayName("Отправляемый")]
        public bool IsSending
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.IsSending;
                else
                    return false;
            }
        }   

        /// <summary>
        /// Количество успешно отправленных комманд.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во отправ.")]
        public int SendSuccessCount
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.SendSuccessCount;
                else
                    return 0;
            }
        }        

        /// <summary>
        /// Количество неуспешно отправленных комманд.
        /// </summary>
        [NonPersistent]
        [DisplayName("Кол-во неотправ.")]
        public int SendFaultCount
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.SendFaultCount;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Время записи данных в SQL-сервер.
        /// </summary>
        [NonPersistent]
        [DisplayName("Время записи, мс")]
        public double WriteTimeSpan
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.WriteTimeSpan.TotalMilliseconds;
                else 
                    return 0;
            }
        }

        /// <summary>
        /// Последняя транзакция.
        /// </summary>
        [NonPersistent]
        [DisplayName("Последняя транзакция")]
        public DateTime TransactionLast
        {
            get
            {
                if (SQLServerReal != null)
                    return SQLServerReal.TransactionLast;
                else
                    return new DateTime(1900, 1, 1);
            }
        }

        public override void GetData<Type>(Type source)
        {
            base.GetData<Type>(source);

            if (typeof(Type) == typeof(SQLServer))
            {
                SQLServer obj = source as SQLServer;

                SQLName = obj.SQLName;
                ConnectionString = obj.ConnectionString;
            }
        }

        public override void Init()
        {
            base.Init();
            if (!IsLoading)
            {
                DateTimeFormat = "yyyy/dd/MM HH:mm:ss";
                SendAll = true;
                ThreadCount = 2;
            }
        }
    }
    #endregion

    #region SQLServerItemForceCollection
    /// <summary>
    /// Таблица-коллекция "Исключительные элементы".
    /// </summary>
    [DBAttribute(Caption = "Отправляемые элементы", IconFile = "SQLServerItemForceCollection.png")]
    public class SQLServerItemForceCollection : XPObjectEx
    {
        public SQLServerItemForceCollection() : base(Session.DefaultSession) { }

        public SQLServerItemForceCollection(Session session) : base(session) { }

        SQLServer _SQLServerOwner;
        /// <summary>
        /// Владелец "SQL-сервер".
        /// </summary>
        [DisplayName("Владелец \"SQL-сервер\"")]
        [Association("SQLServer-SQLServerItemForceCollection")]
        public SQLServer SQLServerOwner
        {
            get { return _SQLServerOwner; }
            set { SetPropertyValueEx("SQLServerOwner", ref _SQLServerOwner, value); }
        }

        Item _ItemID;
        /// <summary>
        /// Элемент.
        /// </summary>
        [DisplayName("Элемент")]
        public Item ItemID
        {
            get { return _ItemID; }
            set { SetPropertyValueEx("ItemID", ref _ItemID, value); }
        }

        string _Comment;
        /// <summary>
        /// Комментарий.
        /// </summary>
        [DisplayName("Комментарий")]
        [Size(SizeAttribute.Unlimited)]
        public string Comment
        {
            get { return _Comment; }
            set { SetPropertyValueEx("Comment", ref _Comment, value); }
        }

        protected override void OnSaving()
        {
            if (this.SQLServerOwner == null)
                Delete();
            base.OnSaving();
        }
    }
    #endregion

}
