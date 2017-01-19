using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Data.SqlClient;
using DevExpress.Xpo;
using System.Collections.ObjectModel;
using RapidInterface;
using OPC.Data;
using System.Runtime.InteropServices;
using System.IO.Ports;
using System.Windows.Forms;
using NDde.Client;
using System.Net.NetworkInformation;
using FirebirdSql.Data.FirebirdClient;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataToSQL
{
    #region CollectionEx
    public class CollectionEx<T> : Collection<T>
    {
        /// <summary>
        /// Количество подключенных элементов.
        /// </summary>
        public int ConnectedCount { get; set; }

        /// <summary>
        /// Асинхронное чтение данных.
        /// </summary>
        public void ReadDataAsync()
        {
            if (typeof(T) == typeof(DataSourceReal) || TypeEx.IsSubclassOf(typeof(T), typeof(DataSourceReal)))
            {
                ConnectedCount = 0;
                foreach (T item in this)
                {
                    DataSourceReal dataSource = item as DataSourceReal;
                    dataSource.ReadDataAsync();
                    dataSource.SendDataToXPObject();
                    if (dataSource.IsConnected)
                        ConnectedCount++;
                }
            }
        }

        /// <summary>
        /// Асинхронная отправка данных.
        /// </summary>
        public void SendDataAsync()
        {
            if (typeof(T) == typeof(SQLServerReal) || TypeEx.IsSubclassOf(typeof(T), typeof(SQLServerReal)))
            {
                ConnectedCount = 0;
                foreach (T item in this)
                {
                    SQLServerReal sqlServer = item as SQLServerReal;
                    sqlServer.SendDataAsync();
                    sqlServer.SendDataToXPObject();
                    if (sqlServer.IsSending)
                        ConnectedCount++;
                }
            }
        }

        /// <summary>
        /// Асинхронная отправка лога.
        /// </summary>
        public void SendDataLogAsync(string text)
        {
            if (typeof(T) == typeof(SQLServerReal) || TypeEx.IsSubclassOf(typeof(T), typeof(SQLServerReal)))
            {
                foreach (T item in this)
                {
                    SQLServerReal sqlServer = item as SQLServerReal;
                    sqlServer.LogText = text;
                    sqlServer.SendDataLogAsync();
                }
            }
        }
    }
    #endregion

    #region LinkXPObject
    public class LinkXPObject
    {
        public LinkXPObject(XPObject xpObject)
        {
            XPObject = xpObject;
            Oid = xpObject.Oid;
        }

        public object XPObject { get; set; }
        public int Oid { get; set; }

        /// <summary>
        /// Поиск объекта сервера.
        /// </summary>
        public object FindXPOject(Collection<XPObject> collection)
        {
            foreach (XPObject record in collection)
                if (record.Oid == Oid)
                {
                    XPObject = record;
                    return XPObject;
                }
            return null;
        }

        /// <summary>
        /// Отправка данных.
        /// </summary>
        public virtual void SendDataToXPObject() {}

        public static void Transfer<TypeReal>(XPCollection xpCollection, Collection<TypeReal> collectionReal)
        {
            if (TypeEx.IsSubclassOf(typeof(TypeReal), typeof(LinkXPObject)))
            {
                Collection<XPObject> collection = new Collection<XPObject>();
                foreach (XPObject server in xpCollection)
                    collection.Add(server);

                foreach (TypeReal serverReal in collectionReal)
                {
                    LinkXPObject link = serverReal as LinkXPObject;
                    XPObject server = link.FindXPOject(collection) as XPObject;
                    if (server != null)
                        collection.Remove(server);
                }
            }
        }
    }
    #endregion

    #region LinkXPObjects
    public class LinkXPObjects : Collection<LinkXPObject>
    {
        public void SendData()
        {
            foreach (LinkXPObject item in this)
                item.SendDataToXPObject();
        }
    }
    #endregion

    #region ItemReal
    /// <summary>
    /// Таблица "Элемент".
    /// </summary>
    public class ItemReal : LinkXPObject
    {
        public ItemReal(Item item)
            : base(item)
        {
            DataSourceOid = -1;
            Ready = false;
            IsService = false;
            DeviceTime = new DateTime(1900, 1, 1);
            GetData(item);
        }

        /// <summary>
        /// Тип переменной.
        /// </summary>
        public enum DataType
        {
            Boolean = 1,            // булевый
            Integer = 2,            // целочисленный
            Real = 3                // вещественный
        };

        /// <summary>
        /// OPC-сервер №1.
        /// </summary>
        public int DataSourceOid { get; set; }

        /// <summary>
        /// Имя OPC-переменной.
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// Имя SQL-таблицы.
        /// </summary>
        public string SQLTableName { get; set; }

        /// <summary>
        /// Тренд.
        /// </summary>
        public bool SQLTrend { get; set; }

        /// <summary>
        /// Сервисный ли это элемент.
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Тип поля "DataValue" для SQL.
        /// </summary>
        public string SQLTableDataValueType
        {
            get
            {
                if (CanonicalDataTypeSimple == DataType.Boolean)
                    return "bit";
                else if (CanonicalDataTypeSimple == DataType.Integer)
                    return "int";
                return "float";
            }
        }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Единица измерения.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Формат значения.
        /// </summary>
        public string FormatValue { get; set; }

        /// <summary>
        /// Тип переменной.
        /// </summary>
        public VarEnum CanonicalDataType { get; set; }

        /// <summary>
        /// Упрощенный тип переменной.
        /// </summary>
        public DataType CanonicalDataTypeSimple { get; set; }

        /// <summary>
        /// Значение.
        /// </summary>
        public object DataValue { get; set; }

        /// <summary>
        /// Последние значение при хорошем качестве.
        /// </summary>
        public object DataValueLastGood { get; set; }

        /// <summary>
        /// Минимальное значение.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Качество.
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Определение качества.
        /// </summary>
        public string QualityDef { get; set; }

        /// <summary>
        /// Время устройства.
        /// </summary>
        public DateTime DeviceTime { get; set; }

        /// <summary>
        /// Таймаут, с.
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Готовность к работе.
        /// </summary>
        public bool Ready { get; set; }

        /// <summary>
        /// Показатель того, что можно отправлять данные.
        /// </summary>
        public bool GoodToSend
        {
            get
            {
                if (Ready && SQLTableName != null && DataValue != null)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Получение данных.
        /// </summary>
        public void GetData(Item item)
        {
            Description = item.Description;
            Unit = item.Unit;
            FormatValue = item.FormatValue;
            MinValue = item.MinValue;
            MaxValue = item.MaxValue;
            SQLTableName = item.SQLTableName;
            if (item.DataSource != null)
                DataSourceOid = item.DataSource.Oid;
            ItemID = item.ItemID;
            SQLTrend = item.SQLTrend;
            TimeOut = item.TimeOut;
            Comment = item.Comment;
        }

        public override void SendDataToXPObject()
        {
            base.SendDataToXPObject();
            if (XPObject != null)
                ((Item)XPObject).ItemReal = this;
        }

        /// <summary>
        /// Установление готовности переменной.
        /// </summary>
        public void SetReady(VarEnum varEnum)
        {
            CanonicalDataType = varEnum;

            if (CanonicalDataType == VarEnum.VT_BOOL)
            {
                CanonicalDataTypeSimple = ItemReal.DataType.Boolean;
                DataValue = DataValueLastGood = false;
            }
            else if (CanonicalDataType == VarEnum.VT_UI1 ||
                CanonicalDataType == VarEnum.VT_UI2 ||
                CanonicalDataType == VarEnum.VT_UI4 ||
                CanonicalDataType == VarEnum.VT_UI8 ||
                CanonicalDataType == VarEnum.VT_UINT ||
                CanonicalDataType == VarEnum.VT_I1 ||
                CanonicalDataType == VarEnum.VT_I2 ||
                CanonicalDataType == VarEnum.VT_I4 ||
                CanonicalDataType == VarEnum.VT_I8 ||
                CanonicalDataType == VarEnum.VT_INT)
            {
                CanonicalDataTypeSimple = ItemReal.DataType.Integer;
                DataValue = DataValueLastGood = 0;
            }
            else
            {
                CanonicalDataTypeSimple = DataType.Real;
                DataValue = DataValueLastGood = 0.0;
            }

            Ready = true;
        }
    }
    #endregion

    #region DataSourceReal
    public class DataSourceReal : LinkXPObject
    {
        public DataSourceReal(DataSource dataSource, Collection<ItemReal> itemRealCollection)
            : base(dataSource)
        {
            GetData(dataSource);
            Items = new Collection<ItemReal>();
            RequiredItems = new Collection<ItemReal>();
            RequiredIndexes = new Collection<int>();
            ReadTimeT0 = DateTime.Now;
            GetItems(itemRealCollection);
            IsConnected = false;
            IsReading = false;
            UpdateTime = new DateTime();
            IsBusy = false;

            ReadDataThread = new Thread(() => TryReadDataEx());

            ReadDataTimeOut = 10000;
            TriggerPostionX = new TriggerT<int>();
            TriggerPostionY = new TriggerT<int>();
            CursorDeactivationLost = 0;
            InitItems();
        }

        /// <summary>
        /// Название.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Комментарий.
        /// </summary>
        public string Comment { get; set; }

        bool _IsConnected;
        /// <summary>
        /// Состояние соединения.
        /// </summary>
        public bool IsConnected 
        { 
            get {return _IsConnected;}
            set
            {
                if (_IsConnected == value) return;
                _IsConnected = value;
                if (IsConnectedChanged != null)
                    IsConnectedChanged(this, EventArgs.Empty);
            }
        }

        int _ReceiveSuccessCount;
        /// <summary>
        /// Количество полученных запросов.
        /// </summary>
        public int ReceiveSuccessCount
        {
            get
            {
                return _ReceiveSuccessCount;
            }
            set
            {
                _ReceiveSuccessCount = value;
                IsReading = true;
                UpdateTime = DateTime.Now;
            }
        }

        int _ReceiveFaultCount;
        /// <summary>
        /// Количество неполученных запросов.
        /// </summary>
        public int ReceiveFaultCount
        {
            get
            {
                return _ReceiveFaultCount;
            }
            set
            {
                _ReceiveFaultCount = value;
                IsReading = false;
            }
        }

        int _ConnectSuccessCount;
        /// <summary>
        /// Количество удачных попыток подключения.
        /// </summary>
        public int ConnectSuccessCount
        {
            get
            {
                return _ConnectSuccessCount;
            }
            set
            {
                _ConnectSuccessCount = value;
            }
        }

        int _ConnectFaultCount;
        /// <summary>
        /// Количество неудачных попыток поключения.
        /// </summary>
        public int ConnectFaultCount
        {
            get
            {
                return _ConnectFaultCount;
            }
            set
            {
                _ConnectFaultCount = value;
                IsReading = false;
            }
        }

        /// <summary>
        /// Состояние чтения.
        /// </summary>
        public bool IsReading{ get; set; }

        /// <summary>
        /// Последнее время обновления данных.
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// Начальное время.
        /// </summary>
        public DateTime ReadTimeT0 { get; set; }

        /// <summary>
        /// Время чтения.
        /// </summary>
        public TimeSpan ReadTimeSpan { get; set; }

        /// <summary>
        /// Поток для чтения данных.
        /// </summary>
        Thread ReadDataThread { get; set; }

        /// <summary>
        /// Коллекция всех элементов.
        /// </summary>
        public Collection<ItemReal> Items { get; set; }

        /// <summary>
        /// Коллекция свободных элементов.
        /// </summary>
        public Collection<ItemReal> RequiredItems { get; set; }

        /// <summary>
        /// Коллекция индексов свободных элементов.
        /// </summary>
        public Collection<int> RequiredIndexes { get; set; }

        /// <summary>
        /// Тайм-аут на чтения данных, мс.
        /// </summary>
        public int ReadDataTimeOut { get; set; }

        /// <summary>
        /// Деактивация считывания данных при изменении положения курсора (ДК).
        /// </summary>
        public bool CursorDeactivation { get; set; }

        /// <summary>
        /// Предел ДК.
        /// </summary>
        public int CursorDeactivationLimit { get; set; }

        /// <summary>
        /// Оставшиеся циклы до активации.
        /// </summary>
        public int CursorDeactivationLost { get; set; }

        /// <summary>
        /// Занятость потока.
        /// </summary>
        public bool IsBusy { get; set; }
        
        /// <summary>
        /// Триггер на изменение X-позиции.
        /// </summary>
        TriggerT<int> TriggerPostionX { get; set; }

        /// <summary>
        /// Триггер на изменение Y-позиции.
        /// </summary>
        TriggerT<int> TriggerPostionY { get; set; }

        /// <summary>
        /// Событие на изменение состояния подключения.
        /// </summary>
        public event EventHandler IsConnectedChanged;

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(DataSource dataSource)
        {
            Caption = dataSource.Caption;
            Comment = dataSource.Comment;
            CursorDeactivation = dataSource.CursorDeactivation;
            CursorDeactivationLimit = dataSource.CursorDeactivationLimit;
        }

        public override void SendDataToXPObject()
        {
            base.SendDataToXPObject();
            if (XPObject != null)
                ((DataSource)XPObject).SourceReal = this;
        }

        /// <summary>
        /// Получение элемента, удовлетворяющего условию.
        /// </summary>
        /// <param name="items"></param>
        void GetItems(Collection<ItemReal> items)
        {
            foreach (ItemReal item in items)
            {
                if (item.DataSourceOid == Oid)
                    Items.Add(item);
            }
        }

        /// <summary>
        /// Асинхронное обновление данных.
        /// </summary>
        public void ReadDataAsync()
        {
            if (!IsBusy)
            {
                IsBusy = true;
                Thread ReadDataThread = new Thread(() => TryReadDataEx());
                ReadDataThread.Start();
            }
        }

        /// <summary>
        /// Установка готовности всех переменных в принудительном режиме.
        /// </summary>
        public void SetItemsReadyForce(VarEnum varEnum = VarEnum.VT_R8)
        {
            foreach (ItemReal item in Items)
                item.SetReady(varEnum);
        }

        /// <summary>
        /// Установка готовности всех переменных за исключением уже готовых
        /// </summary>
        public void SetItemReadyExeptOther(VarEnum varEnum = VarEnum.VT_R8)
        {
            foreach (ItemReal item in Items)
            {
                if (!item.Ready)
                {
                    item.Quality = 192;
                    item.QualityDef = "Connected";
                    item.SetReady(varEnum);
                }
            }
        }

        /// <summary>
        /// Установка показателей качества всех переменных.
        /// </summary>
        public void SetItemsQuality(short quality, string qualityDef)
        {
            foreach (ItemReal itemReal in Items)
            {
                itemReal.Quality = quality;
                itemReal.QualityDef = qualityDef;
            }
        }

        /// <summary>
        /// Установка показателей качества для определенных переменных.
        /// </summary>
        public void SetRequiredItemsQuality(short quality, string qualityDef)
        {
            for (int i = 0; i < RequiredIndexes.Count; i++)
            {
                Items[RequiredIndexes[i]].Quality = quality;
                Items[RequiredIndexes[i]].QualityDef = qualityDef;
            }
        }

        /// <summary>
        /// Попытка выполнить задачу обновления данных в заданный период времени.
        /// </summary>
        /// <returns></returns>
        public void TryReadDataEx()
        {
            ReadTimeT0 = DateTime.Now;

            // На случай включения функции ДК.
            if (CursorDeactivation)
            {
                // Определение изменения положения курсора на экране.
                if (TriggerPostionX.CalculateRet(Cursor.Position.X) ||
                    TriggerPostionY.CalculateRet(Cursor.Position.Y))
                    CursorDeactivationLost = CursorDeactivationLimit;

                CursorDeactivationLost--;

                if (CursorDeactivationLost < 0)
                    CursorDeactivationLost = 0;
            }


            if (CursorDeactivationLost == 0)
            {
                //result = ThreadEx.CallTimedOutMethodSync(ReadData, ReadDataTimeOut);
                try
                {
                    ReadData();
                }
                catch
                {
                    ReceiveFaultCount++;
                }
            }
            else
            {
                SetRequiredItemsQuality(14, "Frozen");
                ReceiveFaultCount++;
            }

            ReadTimeSpan = DateTime.Now - ReadTimeT0;
            IsBusy = false;
        }

        /// <summary>
        /// Виртуальная функция чтения данных.
        /// </summary>
        public virtual void ReadData()
        {
            foreach (ItemReal item in Items)
            {
                if (item.Quality == 192)
                    item.DataValueLastGood = item.DataValue;

                // Обновление информации дианостических переменных.
                if (item.IsService)
                    switch (item.ItemID)
                    {
                        case "_ServiceInfo_IsConnected":
                            item.DataValue = IsConnected;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_IsReading":
                            item.DataValue = IsReading;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_ConnectSuccessCount":
                            item.DataValue = ConnectSuccessCount;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_ConnectFaultCount":
                            item.DataValue = ConnectFaultCount;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_ReceiveSuccessCount":
                            item.DataValue = ReceiveSuccessCount;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_ReceiveFaultCount":
                            item.DataValue = ReceiveFaultCount;
                            item.DeviceTime = DateTime.Now;
                            break;

                        case "_ServiceInfo_ReadTimeSpanMs":
                            item.DataValue = ReadTimeSpan.TotalMilliseconds;
                            item.DeviceTime = DateTime.Now;
                            break;
                        default:
                            break;
                    }
            }
        }

        /// <summary>
        /// Инициализация значений.
        /// </summary>
        public virtual void InitItems()
        {
            //SetItemsReadyForce();

            // Обновление типа данных дианостических переменных.
            for (int i = 0; i < Items.Count; i++)
                if (Items[i].ItemID == "_ServiceInfo_IsConnected")
                {
                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_BOOL);
                }
                else if (Items[i].ItemID == "_ServiceInfo_IsReading")
                {
                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_BOOL);
                }
                else if (Items[i].ItemID == "_ServiceInfo_ConnectSuccessCount")
                {

                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_I4);
                }
                else if (Items[i].ItemID == "_ServiceInfo_ConnectFaultCount")
                {

                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_I4);
                }
                else if (Items[i].ItemID == "_ServiceInfo_ReceiveSuccessCount")
                {

                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_I4);
                }
                else if (Items[i].ItemID == "_ServiceInfo_ReceiveFaultCount")
                {

                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_I4);
                }
                else if (Items[i].ItemID == "_ServiceInfo_ReadTimeSpanMs")
                {

                    Items[i].IsService = true;
                    Items[i].Quality = 192;
                    Items[i].QualityDef = "_ServiceInfo";
                    Items[i].SetReady(VarEnum.VT_R8);
                }
                else
                {
                    RequiredIndexes.Add(i);
                    RequiredItems.Add(Items[i]);
                }
        }

        /// <summary>
        /// Обработчик события потока чтения данных.
        /// </summary>
        void ReadDataWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            TryReadDataEx();
        }
    }
    #endregion

    #region StatisticsReal
    public class StatisticsReal : DataSourceReal
    {
        public StatisticsReal(Statistics statistics, Collection<ItemReal> itemRealCollection)
            : base(statistics, itemRealCollection)
        {
            ConnectSuccessCount = 1;
            IsConnected = true;
        }

        public override void InitItems()
        {
            base.InitItems();

            foreach (ItemReal item in Items)
            {
                if (!item.Ready)
                {
                    item.Quality = 192;
                    item.QualityDef = "_Statistics";

                    switch (item.ItemID)
                    {
                        case "ActivityDays":
                            item.SetReady(VarEnum.VT_R8);
                            break;

                        case "ThreadMainCountWork":
                            item.SetReady(VarEnum.VT_I4);
                            break;

                        default:
                            item.Quality = 13;
                            item.QualityDef = "Not found";
                            break;
                    }
                }
            }
        }

        public override void ReadData()
        {
            ReadTimeT0 = DateTime.Now;

            try
            {
                foreach (ItemReal itemReal in Items)
                {
                    switch (itemReal.ItemID)
                    {
                        case "ActivityDays":
                            itemReal.DataValue = Global.Default.WorkTimeSpanDays.TotalDays;
                            break;

                        case "ThreadMainCountWork":
                            itemReal.DataValue = Global.Default.ThreadMain.WorkCount;
                            break;

                    }
                    itemReal.DeviceTime = DateTime.Now;
                }
                UpdateTime = DateTime.Now;
                ReceiveSuccessCount++;
            }
            catch
            {
                ReceiveFaultCount++;
            }

            ReadTimeSpan = DateTime.Now - ReadTimeT0;

            base.ReadData();
        }
    }
    #endregion

    #region OPCServerReal
    /// <summary>
    /// Класс "OPC-сервер" для реального времени.
    /// </summary>
    public class OPCServerReal : DataSourceReal
    {
        public OPCServerReal(OPCServer server, Collection<ItemReal> itemRealCollection)
            : base(server, itemRealCollection)
        {
            GetData(server);
            OpcServer = new OpcServer();
            DisconnectFaultCount_last = -1;
        }

        /// <summary>
        /// Компьютер.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Имя сервиса.
        /// </summary>
        public string ProgID { get; set; }

        /// <summary>
        /// OPC-сервер.
        /// </summary>
        public OpcServer OpcServer { get; set; }

        /// <summary>
        /// ОРС-группа.
        /// </summary>
        public OpcGroup OpcGroup { get; set; }


        public int DisconnectFaultCount_last { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(OPCServer server)
        {
            Server = server.Server;
            ProgID = server.ProgID;
        }

        public override void ReadData()
        {
            if (!IsConnected)
            {
                try
                {
                    Connect();
                    if (OpcServer.IsConnected)
                    {
                        OpcGroup = OpcServer.AddGroup(string.Format("Group{0}", ReceiveFaultCount), false, 1000);

                        int count = RequiredIndexes.Count;

                        if (count > 0)
                        {
                            OPCItemDef[] items = new OPCItemDef[count];

                            // Передача списка ОРС-переменных.
                            for (int i = 0; i < count; i++)
                                items[i] = new OPCItemDef(RequiredItems[i].ItemID);
                            OpcGroup.AddItems(items);

                            // Возврат необходимой начальной информации о переменных.
                            for (int i = 0; i < count; i++)
                                Items[RequiredIndexes[i]].SetReady(items[i].CanonicalDataType);
                        }
                        IsConnected = true;
                        ConnectSuccessCount++;
                    }
                    else
                    {
                        IsConnected = false;
                        ConnectFaultCount++;
                    }
                }
                catch
                {
                    IsConnected = false;
                    ConnectFaultCount++;
                }
            }

            if (IsConnected)
            {
                if (OpcGroup != null)
                {
                    try
                    {
                        OpcGroup.ReadSync();

                        for (int i = 0; i < RequiredIndexes.Count; i++)
                        {
                            Items[RequiredIndexes[i]].DataValue = OpcGroup.OPCItems[i].DataValue;
                            if (OpcGroup.OPCItems[i].TimeStamp != 0)
                                Items[RequiredIndexes[i]].DeviceTime = DateTime.FromFileTime(OpcGroup.OPCItems[i].TimeStamp);
                            else
                                Items[RequiredIndexes[i]].DeviceTime = new DateTime(1900, 1, 1);
                            Items[RequiredIndexes[i]].Quality = OpcGroup.OPCItems[i].Quality;
                            Items[RequiredIndexes[i]].QualityDef = OpcGroup.QualityToString(OpcGroup.OPCItems[i].Quality);
                        }

                        ReceiveSuccessCount++;
                    }
                    catch
                    {
                        IsConnected = false;
                        ReceiveFaultCount++;
                        for (int i = 0; i < RequiredIndexes.Count; i++)
                        {
                            Items[RequiredIndexes[i]].Quality = 0;
                            Items[RequiredIndexes[i]].QualityDef = "Disconnected";
                        }
                    }
                }
            }
            else
            {
                Disconnect();
            }

            base.ReadData();
        }

        /// <summary>
        /// Подключение к ОРС-серверу.
        /// </summary>
        void Connect()
        {
            try
            {
                //if (DisconnectFaultCount != DisconnectFaultCount_last)
                //{
                    OpcServer = null;
                    OpcGroup = null;
                    OpcServer = new OpcServer();
                  //  DisconnectFaultCount_last = DisconnectFaultCount;
                //}

                OpcServer.Connect(ProgID, Server);
            }
            catch
            {
                ConnectSuccessCount++;
            }
        }

        /// <summary>
        /// Попытка отключиться с тайм-аутом.
        /// </summary>
        public bool TryDisconnect(int timeOut = 500)
        {
            if (ThreadEx.CallTimedOutMethodSync(new Action(Disconnect), timeOut))
            {
                return true;
            }
            else
            {
                ConnectFaultCount++;
                return false;
            }
        }

        /// <summary>
        /// Закрытие подключения и удаление группы.
        /// </summary>
        void Disconnect()
        {
            try
            {   
                /*
                if (OpcGroup != null)
                {
                    OpcGroup.RemoveItems();
                    OpcGroup.Remove(false);
                }
                if (OpcServer.IsConnected)
                    OpcServer.Disconnect();
                 */
            }
            catch
            {
                ConnectFaultCount++;
            }
        }
    }
    #endregion

    #region VzljotReal
    public class VzljotReal : OPCServerReal
    {
        public VzljotReal(Vzljot vzljot, Collection<ItemReal> itemRealCollection)
            : base(vzljot, itemRealCollection)
        {
            GetData(vzljot);
            IsConnectedChanged += VzljotReal_IsConnectedChanged;
        }

        /// <summary>
        /// Кофигурация.
        /// </summary>
        public string Config { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(Vzljot vzljot)
        {
            Config = vzljot.Config;
        }

        void VzljotReal_IsConnectedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
                try
                {
                    if (Config != "")
                    {
                        OpcGroup groupTemp = OpcServer.AddGroup("GroupTemp", false, 1000);
                        OPCItemDef[] opcItems = new OPCItemDef[1];
                        opcItems[0] = new OPCItemDef(Config);
                        groupTemp.AddItems(opcItems);
                        groupTemp.ReadSync();

                        bool temp = (bool)groupTemp.OPCItems[0].DataValue;
                        if (!temp)
                        {
                            groupTemp.OPCItems[0].DataValue = true;
                            groupTemp.WriteSync();
                        }

                        groupTemp.RemoveItems();
                        groupTemp.Remove(false);
                    }
                }
                catch
                {

                }
        }       
    }
    #endregion

    #region DDEServerReal
    /// <summary>
    /// Класс "DDE-сервер" для реального времени.
    /// </summary>
    public class DDEServerReal : DataSourceReal
    {
        public DDEServerReal(DDEServer server, Collection<ItemReal> itemRealCollection)
            : base(server, itemRealCollection)
        {
            GetData(server);
        }

        /// <summary>
        /// Компьютер.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Имя сервиса.
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Объект DDE-клиента.
        /// </summary>
        public DdeClient DdeClient { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(DDEServer server)
        {
            Server = server.Server;
            Topic = server.Topic;
        }

        public override void InitItems()
        {
            base.InitItems();
            SetItemReadyExeptOther();
        }

        public override void ReadData()
        {
            ReadTimeT0 = DateTime.Now;

            if (!IsConnected)
            {
                try
                {
                    if (DdeClient == null)
                        DdeClient = new DdeClient(Server, Topic);

                    DdeClient.Connect();
                    ConnectSuccessCount++;
                    IsConnected = true;
                }
                catch
                {
                    Disconnect();
                    ConnectFaultCount++;
                }
            }

            if (IsConnected)
            {
                try
                {
                    for (int i = 0; i < RequiredIndexes.Count; i++)
                    {
                        Items[RequiredIndexes[i]].DataValue = double.Parse(DdeClient.Request(Items[RequiredIndexes[i]].ItemID, 100));
                        Items[RequiredIndexes[i]].DeviceTime = DateTime.Now;
                        Items[RequiredIndexes[i]].Quality = 192;
                        Items[RequiredIndexes[i]].QualityDef = "Connected";
                    }
                    ReceiveSuccessCount++;
                }
                catch
                {
                    Disconnect();
                    for (int i = 0; i < RequiredIndexes.Count; i++)
                    {
                        Items[RequiredIndexes[i]].Quality = 0;
                        Items[RequiredIndexes[i]].QualityDef = "Disconnected";
                    }
                    ReceiveFaultCount++;
                }
            }

            ReadTimeSpan = DateTime.Now - ReadTimeT0;

            base.ReadData();
        }

        /// <summary>
        /// Закрытие подключения и удаление группы.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                IsConnected = false;
                DdeClient.Dispose();
                DdeClient = null;
            }
            catch
            {
                ConnectFaultCount++;
            }
        }
    }
    #endregion

    #region ComPortReal
    public class ComPortReal : DataSourceReal
    {
        public ComPortReal(ComPort comPort, Collection<ItemReal> itemRealCollection)
            : base(comPort, itemRealCollection)
        {
            ComPort = new System.IO.Ports.SerialPort();
        }

        /// <summary>
        /// COM-порт.
        /// </summary>
        public SerialPort ComPort { get; set; }

        /// <summary>
        /// Название порта.
        /// </summary>
        public string PortName
        {
            get { return ComPort.PortName; }
            set { ComPort.PortName = value; }
        }
    }
    #endregion

    #region Technograph
    public class TechnographReal : ComPortReal
    {
        public TechnographReal(Technograph technograph, Collection<ItemReal> itemRealCollection)
            : base(technograph, itemRealCollection)
        {
            ComPort.BaudRate = 2400;
            ComPort.Parity = Parity.None;
            ComPort.StopBits = StopBits.One;
            ComPort.DataBits = 8;
            ComPort.Handshake = Handshake.None;
            ReplacePoint = true;

            GetData(technograph);

            SendDelay = 1000;

            ThisType = GetType();
        }

        /// <summary>
        /// Сообщение с COM-порта.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Переменная K1 Технографа.
        /// </summary>
        public double K1 { get; set; }

        /// <summary>
        /// Переменная K2 Технографа.
        /// </summary>
        public double K2 { get; set; }

        /// <summary>
        /// Переменная K3 Технографа.
        /// </summary>
        public double K3 { get; set; }

        /// <summary>
        /// Переменная K4 Технографа.
        /// </summary>
        public double K4 { get; set; }

        /// <summary>
        /// Переменная K5 Технографа.
        /// </summary>
        public double K5 { get; set; }

        /// <summary>
        /// Переменная K6 Технографа.
        /// </summary>
        public double K6 { get; set; }

        /// <summary>
        /// Переменная K7 Технографа.
        /// </summary>
        public double K7 { get; set; }

        /// <summary>
        /// Переменная K8 Технографа.
        /// </summary>
        public double K8 { get; set; }

        /// <summary>
        /// Переменная K9 Технографа.
        /// </summary>
        public double K9 { get; set; }

        /// <summary>
        /// Переменная K10 Технографа.
        /// </summary>
        public double K10 { get; set; }

        /// <summary>
        /// Переменная K11 Технографа.
        /// </summary>
        public double K11 { get; set; }

        /// <summary>
        /// Переменная K12 Технографа.
        /// </summary>
        public double K12 { get; set; }

        /// <summary>
        /// Читабельность сообщений.
        /// </summary>
        public bool IsReadable { get; set; }

        /// <summary>
        /// Заменять точку на запятую или нет
        /// </summary>
        public bool ReplacePoint { get; set; }

        /// <summary>
        /// Тип этого класса.
        /// </summary>
        public Type ThisType { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        public void GetData(Technograph technograph)
        {
            PortName = technograph.PortName;
            ReplacePoint = technograph.ReplacePoint;
        }

        public override void InitItems()
        {
            base.InitItems();
            SetItemReadyExeptOther();
        }

        public override void ReadData()
        {
            ReadTimeT0 = DateTime.Now;
            IsReadable = false;

            if (!IsConnected)
                OpenPort();

            if (IsConnected)
            {
                Message = ReadMessage();

                IsReadable = Parse(Message);
                //IsReadable = Parse(" 16:03 08.06.15 K1 057.9 K2 027.2 K3 050.5 K4 050.0 K5 19.76 K6-02.50 K7 07.14 K8 02.90 KA 16.77  ");
            }

            UpdateItemsValue();

            ReadTimeSpan = DateTime.Now - ReadTimeT0;

            base.ReadData();


    }

        /// <summary>
        /// Задержка в посылке данных.
        /// </summary>
        public int SendDelay { get; set; }

        void OpenPort()
        {
            try
            {
                ComPort.Open();
                ConnectSuccessCount++;
            }
            catch
            {
                ComPort.Close();
                IsConnected = false;
                ConnectFaultCount++;
            }
            IsConnected = true;
        }

        string ReadMessage()
        {
            try
            {
                ComPort.WriteLine("?");
                Thread.Sleep(SendDelay);
                return ComPort.ReadExisting();
            }
            catch
            {
                ComPort.Close();
                IsConnected = false;
                return "";
            }
        }

        /// <summary>
        /// Расшифровка сообщения.
        /// </summary>
        public bool Parse(string message)
        {
            try
            {
                /*
                temp = message.Split(new string[] { "K1", "K2", "K3", "K4", "K5", "K6", "K7", "K8", "K9", "KA", "Kb", "KC" }, StringSplitOptions.None);

                if (temp.Length == 1)
                    throw null;

                for (int i = 1; i <= 12; i++)
                    if (temp.Length > i)
                        if (ReplacePoint)
                            ThisType.GetProperty(string.Format("K{0}", i)).SetValue(this, double.Parse(temp[i].Replace('.', ',')), null);
                        else
                            ThisType.GetProperty(string.Format("K{0}", i)).SetValue(this, double.Parse(temp[i]), null);
                */

                string digit = "";
                bool collect = false;
                int Kindex = 0;
                for (int i = 0; i < message.Length - 1; i++)
                {
                    if (message[i] == 'K' || message[i] == 'k' || i == message.Length - 2)
                    {
                        if (Kindex != 0)
                        {
                            if (ReplacePoint)
                                ThisType.GetProperty(string.Format("K{0}", Kindex)).SetValue(this, double.Parse(digit.Replace('.', ',')), null);
                            else
                                ThisType.GetProperty(string.Format("K{0}", Kindex)).SetValue(this, double.Parse(digit), null);
                        }

                        collect = true;
                        if (message[i + 1] == '1')
                            Kindex = 1;
                        else if (message[i + 1] == '2')
                            Kindex = 2;
                        else if (message[i + 1] == '3')
                            Kindex = 3;
                        else if (message[i + 1] == '4')
                            Kindex = 4;
                        else if (message[i + 1] == '5')
                            Kindex = 5;
                        else if (message[i + 1] == '6')
                            Kindex = 6;
                        else if (message[i + 1] == '7')
                            Kindex = 7;
                        else if (message[i + 1] == '8')
                            Kindex = 8;
                        else if (message[i + 1] == '9')
                            Kindex = 9;
                        else if (message[i + 1] == 'A' || message[i + 1] == 'a')
                            Kindex = 10;
                        else if (message[i + 1] == 'B' || message[i + 1] == 'b')
                            Kindex = 11;
                        else if (message[i + 1] == 'C' || message[i + 1] == 'c')
                            Kindex = 12;

                        i++;
                        digit = "";
                    }
                    else
                    {
                        if (collect)
                        {
                            digit += message[i];
                        }
                    }
                }
                ReceiveSuccessCount++;
            }
            catch
            {
                ReceiveFaultCount++;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Обновление значений переменных.
        /// </summary>
        public void UpdateItemsValue()
        {
            for (int i = 0; i < RequiredIndexes.Count; i++)
            {

                if (IsReadable)
                {
                    Items[RequiredIndexes[i]].Quality = 192;
                    Items[RequiredIndexes[i]].QualityDef = "Connected";
                }
                else
                {
                    Items[RequiredIndexes[i]].Quality = 24;
                    Items[RequiredIndexes[i]].QualityDef = "Disconnected";
                }

                switch (Items[RequiredIndexes[i]].ItemID)
                {
                    case "K1":
                        Items[RequiredIndexes[i]].DataValue = K1;
                        break;
                    case "K2":
                        Items[RequiredIndexes[i]].DataValue = K2;
                        break;
                    case "K3":
                        Items[RequiredIndexes[i]].DataValue = K3;
                        break;
                    case "K4":
                        Items[RequiredIndexes[i]].DataValue = K4;
                        break;
                    case "K5":
                        Items[RequiredIndexes[i]].DataValue = K5;
                        break;
                    case "K6":
                        Items[RequiredIndexes[i]].DataValue = K6;
                        break;
                    case "K7":
                        Items[RequiredIndexes[i]].DataValue = K7;
                        break;
                    case "K8":
                        Items[RequiredIndexes[i]].DataValue = K8;
                        break;
                    case "K9":
                        Items[RequiredIndexes[i]].DataValue = K9;
                        break;
                    case "K10":
                        Items[RequiredIndexes[i]].DataValue = K10;
                        break;
                    case "K11":
                        Items[RequiredIndexes[i]].DataValue = K11;
                        break;
                    case "K12":
                        Items[RequiredIndexes[i]].DataValue = K12;
                        break;
                    default:
                        Items[RequiredIndexes[i]].Quality = 13;
                        Items[RequiredIndexes[i]].QualityDef = "Not found";
                        break;
                }
                Items[RequiredIndexes[i]].DeviceTime = DateTime.Now;
            }
        }

        public string ToString
        {
            get
            {
                return string.Format("{0}, K1 = {1}, K2 = {2}, K3 = {3}, K4 = {4}, K5 = {5}, K6 = {6}, K7 = {7}, K8 = {8}, K9 = {9}, KA = {10}, KB = {11}, KC = {12}",
                                UpdateTime,
                                K1,
                                K2,
                                K3,
                                K4,
                                K5,
                                K6,
                                K7,
                                K8,
                                K9,
                                K10,
                                K11,
                                K12);
            }
        }
    }
    #endregion

    #region PingServerReal
    /// <summary>
    /// Класс "Пинг-сервер" для реального времени.
    /// </summary>
    public class PingServerReal : DataSourceReal
    {
        public PingServerReal(PingServer server, Collection<ItemReal> itemRealCollection)
            : base(server, itemRealCollection)
        {
            GetData(server);
            //SetItemsReadyForce();
            PingSender = new Ping();
            Options = new PingOptions();
            buffer = Encoding.ASCII.GetBytes("a");
            ConnectSuccessCount = 1;
            IsConnected = true;
        }

        /// <summary>
        /// Адрес.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Таймаут, мс.
        /// </summary
        public int TimeOut { get; set; }

        /// <summary>
        /// Объект работы с со службой Ping.
        /// </summary>
        Ping PingSender { get; set; }

        /// <summary>
        /// Опции пинга.
        /// </summary>
        PingOptions Options { get; set; }

        byte[] buffer { get; set; }

        PingReply reply { get; set; }

        bool Status {get; set;}

        long ReplyTime { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(PingServer server)
        {
            Address = server.Address;
            TimeOut = server.TimeOut;
        }

        public override void InitItems()
        {
            base.InitItems();

            foreach (ItemReal item in Items)
            {
                if (!item.IsService)
                {
                    item.Quality = 192;
                    item.QualityDef = "Connected";

                    switch (item.ItemID)
                    {
                        case "Status":
                            item.SetReady(VarEnum.VT_BOOL);
                            break;
                        case "ReplyTime":
                            item.SetReady(VarEnum.VT_I4);
                            break;
                        default:
                            item.Quality = 13;
                            item.QualityDef = "Not found";
                            break;
                    }
                }
            }
        }

        public override void ReadData()
        {
            ReadTimeT0 = DateTime.Now;

            try
            {
                try
                {
                    reply = PingSender.Send(Address, TimeOut, buffer, Options);
                }
                catch
                {
                    reply = null;
                    Status = false;
                    ReplyTime = -1;
                }

                if (reply != null)
                    if (reply.Status == IPStatus.Success)
                    {
                        Status = true;
                        ReplyTime = reply.RoundtripTime;
                    }
                    else
                    {
                        Status = false;
                        ReplyTime = -1;
                    }

                foreach (ItemReal item in RequiredItems)
                {
                    switch (item.ItemID)
                    {
                        case "Status":
                            item.DataValue = Status;
                            break;
                        case "ReplyTime":
                            item.DataValue = ReplyTime;
                            break;
                        default:
                            break;
                    }
                    item.DeviceTime = DateTime.Now;
                }
                ReceiveSuccessCount++;
            }
            catch
            {
                ReceiveFaultCount++;
            }

            ReadTimeSpan = DateTime.Now - ReadTimeT0;

            base.ReadData();
        }
    }
    #endregion

    #region KMAZSServerReal
    /// <summary>
    /// Класс "Пинг-сервер" для реального времени.
    /// </summary>
    public class KMAZSServerReal : DataSourceReal
    {
        /// <summary>
        /// Класс для таблицы "TANKCONFIG"
        /// </summary>
        public class TANKCONFIG
        {
            /// <summary>
            /// КМАЗС.
            /// </summary>
            public int KM_ID { get; set; }

            /// <summary>
            /// Время.
            /// </summary>
            public DateTime TC_ST_TIME { get; set; }

            /// <summary>
            /// Уровень.
            /// </summary>
            public int TC_ST_LEVEL { get; set; }

            /// <summary>
            /// Объем.
            /// </summary>
            public int TC_ST_VOLUME { get; set; }

            /// <summary>
            /// Плотность.
            /// </summary>
            public int TC_ST_PLOTN { get; set; }

            /// <summary>
            /// Температура.
            /// </summary>
            public int TC_ST_TEMPER { get; set; }
        }

        /// <summary>
        /// Класс для таблицы "TIDELOG"
        /// </summary>
        public class TIDELOGEx
        {
            public TIDELOGEx()
            {
                TDID = -1;
                TankTime = new DateTime(1900, 1, 1);
                TankLevel = -1;
                TankVolume = -1;
                TankDensity = -1;
                TankTemperature = -1;
            }

            /// <summary>
            /// ID таблицы Firebird.
            /// </summary>
            public int TDID { get; set; }

            /// <summary>
            /// КМАЗС.
            /// </summary>
            public string KMAZS { get; set; }

            /// <summary>
            /// Оператор.
            /// </summary>
            public string Operator { get; set; }

            /// <summary>
            /// Машина.
            /// </summary>
            public string Car { get; set; }

            /// <summary>
            /// Время.
            /// </summary>
            public DateTime TDTime { get; set; }

            /// <summary>
            /// Литры.
            /// </summary>
            public double Liters { get; set; }

            /// <summary>
            /// Ближайнее время резервуара.
            /// </summary>
            public DateTime TankTime { get; set; }

            /// <summary>
            /// Уровень резервуара.
            /// </summary>
            public double TankLevel { get; set; }

            /// <summary>
            /// Объем резервуара.
            /// </summary>
            public double TankVolume { get; set; }

            /// <summary>
            /// Температура резервуара.
            /// </summary>
            public double TankTemperature { get; set; }

            /// <summary>
            /// Плотность резервуара.
            /// </summary>
            public double TankDensity { get; set; }
        }

        public KMAZSServerReal(KMAZSServer server, Collection<ItemReal> itemRealCollection)
            : base(server, itemRealCollection)
        {
            GetData(server);
            Inited = false;
            KMDict = new Dictionary<int, string>();
            OperatorDict = new Dictionary<int, string>();
            CarDict = new Dictionary<int, string>();

            TankLevelFactor = 0.1;
            TankVolumeFactor = 1;
            TankDensityFactor = 0.0001;
            TankTemperatureFactor = 0.5;

            ConnectSuccessCount = 1;

            TidelogCounter = 0;
        }

        /// <summary>
        /// Строка подключения.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Таймаут для актуальности значений с таблицы TANKCONFIG, с.
        /// </summary>
        public int TANKCONFIGTimeOut { get; set; }

        /// <summary>
        /// Синхронизация TIDELOG.
        /// </summary>
        public bool TIDELOGSync { get; set; }

        /// <summary>
        /// Название TIDELOG.
        /// </summary>
        public string TIDELOGTableName { get; set; }

        /// <summary>
        /// Инициализация.
        /// </summary>
        public bool Inited { get; set; }

        /// <summary>
        /// Словарь "КМАЗС".
        /// </summary>
        Dictionary<int, string> KMDict { get; set; }

        /// <summary>
        /// Словарь "Водитель".
        /// </summary>
        Dictionary<int, string> OperatorDict { get; set; }

        /// <summary>
        /// Словарь "Машина".
        /// </summary>
        Dictionary<int, string> CarDict { get; set; }

        /// <summary>
        /// Множитель для уровня.
        /// </summary>
        double TankLevelFactor { get; set; }

        /// <summary>
        /// Множитель для объема.
        /// </summary>
        double TankVolumeFactor { get; set; }

        /// <summary>
        /// Множитель для плотности.
        /// </summary>
        double TankDensityFactor { get; set; }

        /// <summary>
        /// Множитель для температуры.
        /// </summary>
        double TankTemperatureFactor { get; set; }

        /// <summary>
        /// Счетчик выполнение запроса данных.
        /// </summary>
        public int TidelogCounter { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        void GetData(KMAZSServer server)
        {
            ConnectionString = server.ConnectionString;
            TANKCONFIGTimeOut = server.TANKCONFIGTimeOut;
            TIDELOGSync = server.TIDELOGSync;
            TIDELOGTableName = server.TIDELOGTableName;
        }

        /// <summary>
        /// Получение коллекции таблицы "TANKCONFIG".
        /// </summary>
        public Collection<TANKCONFIG> GetTANKCONFIGs()
        {
            Collection<TANKCONFIG> collection = new Collection<TANKCONFIG>();
            FbConnection connection = new FbConnection(ConnectionString);
            connection.Open();
            FbCommand command = new FbCommand(@"SELECT KM_ID, TC_ST_TIME, TC_ST_LEVEL, TC_ST_VOLUME, TC_ST_PLOTN, TC_ST_TEMPER
                                               FROM TANKCONFIG",
                                               connection);

            FbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TANKCONFIG item = new TANKCONFIG()
                {
                    KM_ID = reader.GetInt32(0),
                    TC_ST_TIME = reader.GetDateTime(1),
                    TC_ST_LEVEL = reader.GetInt32(2),
                    TC_ST_VOLUME = reader.GetInt32(3),
                    TC_ST_PLOTN = reader.GetInt32(4),
                    TC_ST_TEMPER = reader.GetInt32(5)
                };
                collection.Add(item);
            }
            connection.Close();
            return collection;
        }

        /// <summary>
        /// Заполнение словаря.
        /// </summary>
        void FillDictionary(Dictionary<int, string> dictionary, string statement)
        {
            FbConnection connection = new FbConnection(ConnectionString);
            connection.Open();
            FbCommand command = new FbCommand(statement, connection);
            FbDataReader reader = command.ExecuteReader();
            while (reader.Read())
                dictionary.Add(reader.GetInt32(0), reader.GetString(1));
            connection.Close();
        }

        public Collection<TIDELOGEx> GetTIDELOGs(DateTime dateBegin, DateTime dateEnd)
        {
            Collection<TIDELOGEx> collection = new Collection<TIDELOGEx>();
            FbConnection connection = new FbConnection(ConnectionString);
            connection.Open();
            FbCommand command = new FbCommand(string.Format(@"SELECT TD_KMRECID, KM_ID, OP_ID, CAR_ID, TD_TIME, TD_LITERS 
                                                            FROM TIDELOG
                                                            WHERE TD_TIME > '{0}' AND  TD_TIME <= '{1}' ORDER BY TD_TIME",
                                                            /*0*/dateBegin,
                                                            /*1*/dateEnd),
                                                            connection);

            FbDataReader reader = command.ExecuteReader();

            TidelogCounter = 0;

            while (reader.Read())
            {
                TIDELOGEx item = new TIDELOGEx() { TDID = reader.GetInt32(0) };

                if (reader.GetValue(1) is int)
                    item.KMAZS = KMDict[reader.GetInt32(1)];

                if (reader.GetValue(2) is int)
                    item.Operator = OperatorDict[reader.GetInt32(2)];

                if (reader.GetValue(3) is int)
                    item.Car = CarDict[reader.GetInt32(3)];

                item.TDTime = reader.GetDateTime(4);
                item.Liters = reader.GetFloat(5);

                FbCommand commandInner = new FbCommand(string.Format(@"SELECT TH_ST_TIME, TH_ST_LEVEL, TH_ST_VOLUME, TH_ST_PLOTN, TH_ST_TEMPER 
                                                                    FROM TANKHISTORY
                                                                    WHERE KM_ID = {0} AND TH_ST_TIME BETWEEN '{1}' AND '{2}' ORDER BY TH_ST_TIME",
                                                                    /*0*/reader.GetInt32(1),
                                                                    /*1*/reader.GetDateTime(4),
                                                                    /*2*/reader.GetDateTime(4) + new TimeSpan(48, 0, 0)),
                                                                    connection);
                FbDataReader readerInner = commandInner.ExecuteReader();
                try
                {
                    readerInner.Read();
                    item.TankTime = readerInner.GetDateTime(0);
                    item.TankLevel = readerInner.GetInt32(1) * TankLevelFactor;
                    item.TankVolume = readerInner.GetInt32(2) * TankVolumeFactor;
                    item.TankDensity = readerInner.GetInt32(3) * TankDensityFactor;
                    item.TankTemperature = readerInner.GetInt32(4) * TankTemperatureFactor;
                }
                catch
                {
                }

                collection.Add(item);

                TidelogCounter++;
            }
            connection.Close();
            return collection;
        }

        /// <summary>
        /// Получение текущего времени с сервера.
        /// </summary>
        public DateTime GetCurrentTime()
        {
            DateTime time = DateTime.MinValue;
            FbConnection connection = new FbConnection(ConnectionString);
            connection.Open();
            FbCommand fbCommand = new FbCommand(@"SELECT CURRENT_TIMESTAMP from rdb$database", connection);
            time = (DateTime)fbCommand.ExecuteScalar();
            connection.Close();
            return time;
        }

        /// <summary>
        /// Получение времени последней записи в таблице "TIDELOG".
        /// </summary>
        public DateTime GetTIDELOGLastTime()
        {
            DateTime time;
            FbConnection connection = new FbConnection(ConnectionString);
            connection.Open();
            FbCommand fbCommand = new FbCommand(@"SELECT MAX(TD_TIME) FROM TIDELOG", connection);
            object result = fbCommand.ExecuteScalar();
            if (result is DateTime)
                time = (DateTime)result;
            else
                time = new DateTime(1900, 1, 1);
            connection.Close();
            return time;
        }

        /// <summary>
        /// Получение значения по ID.
        /// </summary>
        static TANKCONFIG GetTank(Collection<TANKCONFIG> collection, int id)
        {
            foreach (TANKCONFIG item in collection)
                if (item.KM_ID == id)
                    return item;

            return null;
        }

        /// <summary>
        /// Расшифровка значений.
        /// </summary>
        bool ParseItem(ItemReal itemReal, Collection<TANKCONFIG> tanks, DateTime currentTime, string regular, string wordEnd, string propertyName, double factor = 1)
        {
            if (!string.IsNullOrEmpty(itemReal.ItemID))
            {
                Regex RegLevel = new Regex(regular, RegexOptions.IgnoreCase);
                Match matches = RegLevel.Match(itemReal.ItemID);

                if (!string.IsNullOrEmpty(matches.Value))
                {
                    string[] temp = matches.Value.Split(new string[] { "KM_ID_", wordEnd }, StringSplitOptions.None);

                    int id = int.Parse(temp[1]);

                    TANKCONFIG tank = GetTank(tanks, id);
                    if (tank != null)
                    {
                        itemReal.Quality = 192;
                        itemReal.QualityDef = "Connected";

                        itemReal.DataValue = (int)tank.GetType().GetProperty(propertyName).GetValue(tank, null) * factor;
                        itemReal.DeviceTime = tank.TC_ST_TIME;

                        if ((currentTime - itemReal.DeviceTime).TotalSeconds > TANKCONFIGTimeOut)
                        {
                            itemReal.Quality = 15;
                            itemReal.QualityDef = "Old";
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        public override void InitItems()
        {
            base.InitItems();
            SetItemReadyExeptOther();
        }

        public override void ReadData()
        {
            base.ReadData();

            ReadTimeT0 = DateTime.Now;

            try
            {
                if (!Inited)
                {
                    FillDictionary(KMDict, "SELECT KM_ID, KM_NAME FROM KMAZS");
                    FillDictionary(OperatorDict, "SELECT OP_ID, OP_FULLNAME FROM OPERATORS");
                    FillDictionary(CarDict, "SELECT CAR_ID, CAR_NUMBER FROM CARS");
                    Inited = true;
                }

                Collection<TANKCONFIG> tanks = GetTANKCONFIGs();

                DateTime currentTime = GetCurrentTime();

                for (int i = 0; i < RequiredIndexes.Count; i++)
                {
                    if (!(ParseItem(Items[RequiredIndexes[i]], tanks, currentTime, "KM_ID_([0-9]*)_Level", "_Level", "TC_ST_LEVEL", TankLevelFactor) ||
                            ParseItem(Items[RequiredIndexes[i]], tanks, currentTime, "KM_ID_([0-9]*)_Volume", "_Volume", "TC_ST_VOLUME", TankVolumeFactor) ||
                            ParseItem(Items[RequiredIndexes[i]], tanks, currentTime, "KM_ID_([0-9]*)_Plotn", "_Plotn", "TC_ST_PLOTN", TankDensityFactor) ||
                            ParseItem(Items[RequiredIndexes[i]], tanks, currentTime, "KM_ID_([0-9]*)_Temper", "_Temper", "TC_ST_TEMPER", TankTemperatureFactor)))
                    {
                        Items[RequiredIndexes[i]].Quality = 13;
                        Items[RequiredIndexes[i]].QualityDef = "Not found";
                        Items[RequiredIndexes[i]].DeviceTime = DateTime.Now;
                    }
                }

                IsConnected = true;
                ReceiveSuccessCount++;
            }
            catch
            {
                IsConnected = false;
                ReceiveFaultCount++;
                SetItemsQuality(0, "Disconnected");

                // Очистка всех пулов соедениения. Без этой команды не будет работать восстановление соедения.
                FbConnection.ClearAllPools();
            }

            ReadTimeSpan = DateTime.Now - ReadTimeT0;
        }
    }
    #endregion

    #region SQLServerReal
    /// <summary>
    /// Класс "SQL-сервер" для реального времени.
    /// </summary>
    public class SQLServerReal : LinkXPObject
    {
        public SQLServerReal(XPObject xpObject)
            : base(xpObject)
        {
        }

        public SQLServerReal(SQLServer server, Collection<ItemReal> itemRealCollection)
            : base(server)
        {
            InitIsBusy = false;
            SendIsBusy = false;
            LogIsBusy = false;

            TableInitiated = false;
            IsSending = false;
            GetData(server);
            ItemRealCollection = itemRealCollection;
            ItemForceRealCollection = new Collection<ItemReal>();
            WriteTimeT0 = DateTime.Now;
            InitTimeT0 = DateTime.Now;
            Count10s = Count1m = Count10m = Count1h = Count6h = Count1d = 1;
            Enter10s = Enter1m = Ent10m = Enter1h = Enter6h = Enter1d = false;
            ValuesCurrentTableName = "_ValuesCurrent";
            LogsTableName = "_Logs";
            CliensTableName = "_Cliens";
            TransactionLast = DateTime.Now;
        }


        public string SQLName { get; set; }

        public string ConnectionString { get; set; }

        public int OPCTrendCycle1 { get; set; }

        public bool SendAll { get; set; }

        public int ThreadCount { get; set; }

        /// <summary>
        /// Инициализация таблиц.
        /// </summary>
        public bool TableInitiated { get; set; }

        int _SendSuccessCount;
        /// <summary>
        /// Количество отправленных комманд.
        /// </summary>
        public int SendSuccessCount
        {
            get
            {
                return _SendSuccessCount;
            }
            set
            {
                _SendSuccessCount = value;
                IsSending = true;
                WriteTimeSpan = DateTime.Now - TransactionLast;
                TransactionLast = DateTime.Now;
            }
        }

        int _SendFaultCount;
        /// <summary>
        /// Количество неуспешно отправленных комманд.
        /// </summary>
        public int SendFaultCount
        {
            get
            {
                return _SendFaultCount;
            }
            set
            {
                _SendFaultCount = value;
                IsSending = false;
            }
        }

        /// <summary>
        /// Есть ли отправка данных в сервер.
        /// </summary>
        public bool IsSending { get; set; }

        /// <summary>
        /// Занят ли поток инициализации элементов в БД.
        /// </summary>
        bool InitIsBusy { get; set; }

        /// <summary>
        /// Занят ли поток отправки элементов в БД.
        /// </summary>
        bool SendIsBusy { get; set; }

        /// <summary>
        /// Занят ли поток логирования.
        /// </summary>
        bool LogIsBusy { get; set; }        

        /// <summary>
        /// Коллекция элементов, которые будут логироваться в БД.
        /// </summary>
        public Collection<ItemReal> ItemRealCollection { get; set; }

        /// <summary>
        /// Коллекция исключительных элементов, которые будут логироваться в БД.
        /// </summary>
        public Collection<ItemReal> ItemForceRealCollection { get; set; }

        /// <summary>
        /// Название таблицы текущих значений.
        /// </summary>
        public string ValuesCurrentTableName { get; set; }

        /// <summary>
        /// Название таблицы лога.
        /// </summary>
        public string LogsTableName { get; set; }

        /// <summary>
        /// Название таблицы клиентов.
        /// </summary>
        public string CliensTableName { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string LogText { get; set; }

        /// <summary>
        /// Последняя транзакция.
        /// </summary>
        public DateTime TransactionLast { get; set; }

        /// <summary>
        /// Начальное время записи.
        /// </summary>
        DateTime WriteTimeT0 { get; set; }

        /// <summary>
        /// Начальное время записи.
        /// </summary>
        DateTime InitTimeT0 { get; set; }

        /// <summary>
        /// Разница во времени.
        /// </summary>
        TimeSpan TimePeriodDiff { get; set; }

        /// <summary>
        /// Счетчик для 10 секунд.
        /// </summary>
        int Count10s { get; set; }

        /// <summary>
        /// Вхождение в 10 секунд.
        /// </summary>
        bool Enter10s { get; set; }

        /// <summary>
        /// Счетчик для 60 секунд.
        /// </summary>
        int Count1m { get; set; }

        /// <summary>
        /// Вхождение в 60 секунд.
        /// </summary>
        bool Enter1m { get; set; }

        /// <summary>
        /// Счетчик для 600 секунд.
        /// </summary>
        int Count10m { get; set; }

        /// <summary>
        /// Вхождение в 600 секунд.
        /// </summary>
        bool Ent10m { get; set; }

        /// <summary>
        /// Счетчик для 3600 секунд.
        /// </summary>
        int Count1h { get; set; }

        /// <summary>
        /// Вхождение в 3600 секунд.
        /// </summary>
        bool Enter1h { get; set; }

        /// <summary>
        /// Счетчик для 21600 секунд.
        /// </summary>
        int Count6h { get; set; }

        /// <summary>
        /// Вхождение в 21600 секунд.
        /// </summary>
        bool Enter6h { get; set; }

        /// <summary>
        /// Счетчик для 86400 секунд.
        /// </summary>
        int Count1d { get; set; }

        /// <summary>
        /// Вхождение в 21600 секунд.
        /// </summary>
        bool Enter1d { get; set; }

        /// <summary>
        /// Время чтения.
        /// </summary>
        public TimeSpan WriteTimeSpan { get; set; }

        /// <summary>
        /// Формат времени.
        /// </summary>
        string DateTimeFormat { get; set; }

        /// <summary>
        /// Получение данных.
        /// </summary>
        public void GetData(SQLServer server)
        {
            SQLName = server.SQLName;
            ConnectionString = server.ConnectionString;
            DateTimeFormat = server.DateTimeFormat;
            SendAll = server.SendAll;
            ThreadCount = server.ThreadCount;
        }

        public override void SendDataToXPObject()
        {
            base.SendDataToXPObject();

            if (XPObject != null)
                ((SQLServer)XPObject).SQLServerReal = this;
        }

        /// <summary>
        /// Асинхронная инициализация таблицы в БД. 
        /// </summary>
        public void TableInitAsync()
        {
            if (!InitIsBusy)
            {
                InitIsBusy = true;
                Thread thread = new Thread(InitItems);
                thread.Start();
            }
        }

        /// <summary>
        /// Конвертирует булевое знание в нужный формат
        /// </summary>
        static string ConvertToStr(bool value)
        {
            if (value)
                return "1";
            else
                return "0";
        }

        /// <summary>
        /// Получение времени последней записи в таблице "TIDELOG".
        /// </summary>
        public DateTime GetTIDELOGLastTime(string tableName)
        {
            DateTime time = new DateTime(1900, 1, 1);
            SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(string.Format("SELECT MAX(TDTime) FROM {0}", tableName), connection);
                object result = command.ExecuteScalar();
                if (result is DateTime)
                    time = (DateTime)result;
                connection.Close();
            }
            catch
            {
            }
            finally
            {
                connection.Close();
            }
            return time;
        }

        /// <summary>
        /// Получение значения периода времени для трендовых таблиц.
        /// </summary>
        short GetTimePeriod()
        {
            TimePeriodDiff = DateTime.Now - InitTimeT0;

            Enter10s = Enter1m = Ent10m = Enter1h = Enter6h = Enter1d = false;

            if (TimePeriodDiff.TotalSeconds >= Count10s * 10)
            {
                Count10s++;
                Enter10s = true;
            }
            if (TimePeriodDiff.TotalSeconds >= Count1m * 60)
            {
                Count1m++;
                Enter1m = true;
            }
            if (TimePeriodDiff.TotalSeconds >= Count10m * 600)
            {
                Count10m++;
                Ent10m = true;
            }
            if (TimePeriodDiff.TotalSeconds >= Count1h * 3600)
            {
                Count1h++;
                Enter1h = true;
            }
            if (TimePeriodDiff.TotalSeconds >= Count6h * 21600)
            {
                Count6h++;
                Enter6h = true;
            }
            if (TimePeriodDiff.TotalSeconds >= Count1d * 86400)
            {
                Count1d++;
                Enter1d = true;
            }

            if (Enter1d)
                return 6;
            if (Enter6h)
                return 5;
            else if (Enter1h)
                return 4;
            else if (Ent10m)
                return 3;
            else if (Enter1m)
                return 2;
            else if (Enter10s)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// Формирование SQL-запроса для инициализации элементов в БД.
        /// </summary>
        string SQLInitItems(Collection<ItemReal> itemRealCollection)
        {
            string statement = "";
            foreach (ItemReal item in ItemRealCollection)
            {
                if (item.Ready &&
                    item.SQLTableName != null)
                {
                    // Вставка записи в текущую таблицу.
                    statement += string.Format("IF NOT EXISTS (SELECT * FROM {0} WHERE DataName = '{1}') INSERT INTO {0} (DataName) VALUES ('{1}'); ",
                        /*0*/ValuesCurrentTableName,
                        /*1*/item.SQLTableName);

                    // Обновление всех полей записи в текущей таблице.
                    statement += string.Format(@"UPDATE {0} SET 
                            Trend = {2}, 
                            Description = '{3}', 
                            Unit = '{4}', 
                            FormatValue = '{5}', 
                            MinValue = {6}, 
                            MaxValue = {7}, 
                            DataType = {8}, 
                            TimeOut = {9}, 
                            Comment = '{10}'
                            WHERE DataName = '{1}'; ",
                        /*0*/ValuesCurrentTableName,
                        /*1*/item.SQLTableName,
                        /*2*/ConvertToStr(item.SQLTrend),
                        /*3*/item.Description,
                        /*4*/item.Unit,
                        /*5*/item.FormatValue,
                        /*6*/item.MinValue,
                        /*7*/item.MaxValue,
                        /*8*/(short)item.CanonicalDataTypeSimple,
                        /*9*/item.TimeOut,
                        /*10*/item.Comment);

                    // Создание таблицы тренда.
                    if (item.SQLTrend)
                        statement += string.Format(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}') CREATE TABLE {0} (
                                DataValue {1}, 
                                Quality int, 
                                SqlTime datetime DEFAULT GETDATE(), 
                                DeviceTime datetime, 
                                TimePeriod tinyint DEFAULT 0); ",
                            /*0*/item.SQLTableName,
                            /*1*/item.SQLTableDataValueType);
                }
            }
            return statement;
        }

        /// <summary>
        /// Функция, в которой будет выполняться в потоке инициализации таблиц в БД.
        /// </summary>
        void InitItems()
        {
            if (SendAll || ItemForceRealCollection.Count > 0)
            {
                SqlConnection connection = new SqlConnection(ConnectionString);
                try
                {
                    connection.Open();

                    // Создание таблицы текущих значений.
                    string statement = string.Format(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}') CREATE TABLE {0} (
                        DataName char(100), 
                        Trend bit, 
                        Description char(500) DEFAULT '', 
                        Unit char(50) DEFAULT '', 
                        FormatValue char(50) DEFAULT '', 
                        MinValue float, 
                        MaxValue float, 
                        DataType tinyint, 
                        DataValue float DEFAULT 0, 
                        Quality int DEFAULT 0, 
                        SqlTime datetime DEFAULT GETDATE(), 
                        DeviceTime datetime DEFAULT '{1}', 
                        TimeOut int,
                        Comment char(500) DEFAULT ''); ",
                        /*0*/ValuesCurrentTableName,
                        /*1*/(new DateTime(1900, 1, 1)).ToString(DateTimeFormat));

                    // Создание таблицы логов.
                    statement += string.Format(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}') CREATE TABLE {0} (
                        SqlTime datetime DEFAULT GETDATE(), 
                        Source char(100), 
                        Version char(100), 
                        Text char(1000)); ",
                            LogsTableName);

                    // Создание таблицы клиентов.
                    statement += string.Format(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}') CREATE TABLE {0} (
                        SqlTime datetime DEFAULT GETDATE(), 
                        GUID char(100), 
                        IPAddress char(100), 
                        Version char(100), 
                        ClientTime datetime DEFAULT '{1}', 
                        BrowserInformation char(200)); ",
                        /*0*/CliensTableName,
                        /*1*/(new DateTime(1900, 1, 1)).ToString(DateTimeFormat));

                    // Инициализация записей элементов.
                    if (SendAll)
                    {
                        statement += SQLInitItems(ItemRealCollection);

                        foreach (KMAZSServerReal kmazsServerReal in Global.Default.KMAZSServerRealCollection)
                        {
                            if (kmazsServerReal.TIDELOGSync)
                                statement += string.Format(@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}') CREATE TABLE {0} (
                                ID int PRIMARY KEY IDENTITY(1, 1) NOT NULL,
                                TDID int NOT NULL,
                                SqlTime datetime DEFAULT GETDATE(),
                                TDTime datetime,
                                KMAZS char(50), 
                                Operator char(100), 
                                Car char(100), 
                                Liters float,
                                TankTime datetime,
                                TankLevel float,
                                TankVolume float,
                                TankDensity float,
                                TankTemperature float); ",
                                /*0*/kmazsServerReal.TIDELOGTableName);
                        }
                    }
                    else
                        statement += SQLInitItems(ItemForceRealCollection);

                    SqlCommand command = new SqlCommand(statement, connection);
                    command.ExecuteNonQuery();
                    TableInitiated = true;
                }
                catch
                {
                    TableInitiated = false;
                }
                finally
                {
                    connection.Close();
                }
            }
            InitIsBusy = false;
        }

        /// <summary>
        /// Асинхронная отправка данных в БД. 
        /// </summary>
        public void SendDataAsync()
        {
            if (!SendIsBusy)
            {
                SendIsBusy = true;
                Thread thread = new Thread(SendItems);
                thread.Start();
            }
        }

        /// <summary>
        /// Формирование SQL-запроса для отправки элементов в БД.
        /// </summary>
        string CreateSQLItemsStatement(Collection<ItemReal> itemRealCollection)
        {
            string statement = "";

            object dataValueObject = null;
            string dataValueString = "";

            int timePeriod = GetTimePeriod();

            foreach (ItemReal item in itemRealCollection)
                if (item.Ready && !string.IsNullOrEmpty(item.SQLTableName))
                {
                    if (item.DataValue != null)
                        dataValueObject = item.DataValue;
                    else
                        dataValueObject = item.DataValueLastGood;

                    if (item.CanonicalDataTypeSimple == ItemReal.DataType.Real || item.CanonicalDataTypeSimple == ItemReal.DataType.Integer)
                        dataValueString = dataValueObject.ToString().Replace(",", ".");
                    else if (item.CanonicalDataTypeSimple == ItemReal.DataType.Boolean)
                        dataValueString = ConvertToStr((bool)dataValueObject);

                    // Обновление значений в текущей таблице.
                    statement += string.Format(@"UPDATE {0} SET 
                            DataValue = {2}, 
                            Quality = {3}, 
                            SqlTime = GETDATE(), 
                            DeviceTime = '{4}' 
                            WHERE DataName = '{1}'; ",
                        /*0*/ValuesCurrentTableName,
                        /*1*/item.SQLTableName,
                        /*2*/dataValueString,
                        /*3*/item.Quality,
                        /*4*/item.DeviceTime.ToString(DateTimeFormat));

                    // Вставка записи в тренд.
                    if (item.SQLTrend)
                        statement += string.Format(@"INSERT INTO {0} (
                                DataValue, 
                                Quality, 
                                DeviceTime,
                                TimePeriod) 
                                VALUES ({1}, {2}, '{3}', {4}); ",
                            /*0*/item.SQLTableName,
                            /*1*/dataValueString,
                            /*2*/item.Quality,
                            /*3*/item.DeviceTime.ToString(DateTimeFormat),
                            /*4*/timePeriod);
                }
            return statement;
        }

        void SendSQLItemsEx()
        {
            List<Task> SendSQLTaskList = new List<Task>();

            double itemPerThread;
            int from, to;
            for (int i = 0; i < ThreadCount; i++)
            {
                itemPerThread = ItemRealCollection.Count / (double)ThreadCount;

                Collection<ItemReal> itemRealCollection = new Collection<ItemReal>();

                from = (int)(i * itemPerThread);
                to = (int)((i + 1) * itemPerThread);

                for (int j = from; j < to; j++)
                    itemRealCollection.Add(ItemRealCollection[j]);

                SendSQLTaskList.Add(Task.Factory.StartNew(() => SendSQLItems(itemRealCollection)));
            }
            //Task task = Task.Factory.StartNew(() => SendSQLItems(ItemRealCollection));
            //task.Wait();
            Task.WaitAll(SendSQLTaskList.ToArray());
        }

        /// <summary>
        /// Отправка данных в БД, разделенная на потоки.
        /// </summary>
        void SendSQLItems(Collection<ItemReal> itemRealCollection)
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand(CreateSQLItemsStatement(itemRealCollection), connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                SendFaultCount++;
                Program.SaveLog(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        /// <summary>
        /// Функция, в которой будет выполняться в потоке отправки данных в БД.
        /// </summary>
        void SendItems()
        {
            if (SendAll || ItemForceRealCollection.Count > 0)
            {
                try
                {
                    // Отправка записей элементов.
                    if (SendAll)
                    {
                        //statement += CreateSQLItemsStatement(ItemRealCollection);
                        SendSQLItemsEx();
                        string statement = "";

                        foreach (KMAZSServerReal kmazsServerReal in Global.Default.KMAZSServerRealCollection)
                        {
                            if (kmazsServerReal.Inited && kmazsServerReal.TIDELOGSync)
                            {
                                DateTime sqlTIDELOGLastTime = GetTIDELOGLastTime(kmazsServerReal.TIDELOGTableName);
                                DateTime firebirdTIDELOGLastTime = kmazsServerReal.GetTIDELOGLastTime();

                                if (firebirdTIDELOGLastTime != sqlTIDELOGLastTime)
                                {
                                    Collection<KMAZSServerReal.TIDELOGEx> TIDELOGs = kmazsServerReal.GetTIDELOGs(sqlTIDELOGLastTime, firebirdTIDELOGLastTime);
                                    //Collection<KMAZSServerReal.TIDELOGEx> TIDELOGs = kmazsServerReal.GetTIDELOGs(new DateTime(2015, 2, 12), firebirdTIDELOGLastTime);

                                    foreach (KMAZSServerReal.TIDELOGEx tidelog in TIDELOGs)
                                    {
                                        statement += string.Format(@"INSERT INTO {0} (
                                                                TDID,
                                                                TDTime,
                                                                KMAZS,
                                                                Operator,
                                                                Car,
                                                                Liters,
                                                                TankTime, 
                                                                TankLevel,
                                                                TankVolume,
                                                                TankDensity,
                                                                TankTemperature)
                                                                VALUES ({1}, '{2}', '{3}', '{4}', '{5}', {6}, '{7}', {8}, {9}, {10}, {11}); ",
                                            /*0*/kmazsServerReal.TIDELOGTableName,
                                            /*1*/tidelog.TDID,
                                            /*2*/tidelog.TDTime.ToString(DateTimeFormat),
                                            /*3*/tidelog.KMAZS,
                                            /*4*/tidelog.Operator,
                                            /*5*/tidelog.Car,
                                            /*6*/tidelog.Liters.ToString().Replace(",", "."),
                                            /*7*/tidelog.TankTime.ToString(DateTimeFormat),
                                            /*8*/tidelog.TankLevel.ToString().Replace(",", "."),
                                            /*9*/tidelog.TankVolume.ToString().Replace(",", "."),
                                            /*10*/tidelog.TankDensity.ToString().Replace(",", "."),
                                            /*11*/tidelog.TankTemperature.ToString().Replace(",", "."));
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(statement))
                        {
                            SqlConnection connection = new SqlConnection(ConnectionString);
                            connection.Open();
                            SqlCommand command = new SqlCommand(statement, connection);
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                    }
                    else
                        //statement += CreateSQLItemsStatement(ItemForceRealCollection);
                        SendSQLItemsEx();

                    SendSuccessCount++;
                }
                catch (Exception ex)
                {
                    SendFaultCount++;
                    Program.SaveLog(ex.Message);
                }
            }
            SendIsBusy = false;
        }

        /// <summary>
        /// Асинхронная отправка данных в БД. 
        /// </summary>
        public void SendDataLogAsync()
        {
            if (!LogIsBusy)
            {
                LogIsBusy = true;
                Thread thread = new Thread(LogEvents);
                thread.Start();
            }
        }

        /// <summary>
        /// Функция, в которой будет выполняться в потоке инициализации таблиц в БД.
        /// </summary>
        void LogEvents()
        {
            if (SendAll)
            {
                SqlConnection connection = new SqlConnection();
                try
                {
                    connection.ConnectionString = ConnectionString;
                    connection.Open();

                    string statement = string.Format("INSERT INTO {0} (Source, Version, Text) VALUES ('{1}', '{2}', '{3}'); ",
                                            LogsTableName,
                                            Global.Default.AppName,
                                            Global.Default.Version,
                                            LogText);

                    SqlCommand command = new SqlCommand(statement, connection);
                    command.ExecuteNonQuery();
                }
                catch
                {
                }
                finally
                {
                    connection.Close();
                }
            }
            LogIsBusy = false;
        }
    }
    #endregion
}
