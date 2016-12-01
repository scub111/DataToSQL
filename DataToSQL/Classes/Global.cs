using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpo;
using RapidInterface;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.XtraEditors;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using DevExpress.Data.Filtering;
using Usable;

namespace DataToSQL
{
    /// <summary>
    /// Основной класс глобальных переменных, которых можно сохранить в XML-файд
    /// </summary>
    public class VarXml
    {
        /// <summary>
        /// Конструктор <see cref="GlobalVarBase"/> class.
        /// </summary>
        public VarXml()
        {
            this.FileXml = "Config.xml";
            Init();
        }

        /// <summary>
        /// Конструктор <see cref="GlobalVarBase"/> class.
        /// </summary>
        public VarXml(string strFileXml)
        {
            FileXml = strFileXml;
            Init();
        }

        void Init()
        {
            FilePath = string.Format("{0}\\{1}", Path.GetDirectoryName(Application.ExecutablePath), FileXml);
            ThreadMainPeriod = 5000;
            ThreadMainDelay = 3000;
            AppName = "ABC";
        }

        /// <summary>
        /// Название файла, куда будет сохняться данные.
        /// </summary>
        [XmlIgnore]
        string FileXml;

        /// <summary>
        /// Путь к файлу.
        /// </summary>
        [XmlIgnore]
        public string FilePath;

        public int ThreadMainPeriod;

        public int ThreadMainDelay;

        public string AppName;

        /// <summary>
        /// Сохранить данные в XML-файл.
        /// </summary>
        public void SaveToXML()
        {
            XmlSerializer xmlSer = new XmlSerializer(typeof(VarXml));
            TextWriter textWriter = new StreamWriter(FilePath);
            xmlSer.Serialize(textWriter, this);
            textWriter.Close();

        }

        /// <summary>
        /// Загрузить данные из XML-файла.
        /// </summary>
        /// <returns></returns>
        public void LoadFromXML()
        {
            if (File.Exists(FilePath))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(VarXml));
                TextReader textReader = new StreamReader(FilePath);
                VarXml obj = (VarXml)deserializer.Deserialize(textReader);
                textReader.Close();

                ThreadMainPeriod = obj.ThreadMainPeriod;
                ThreadMainDelay = obj.ThreadMainDelay;
                AppName = obj.AppName;
            }
        }
    }

    public class GlobalDefault
    {
        /// <summary>
        /// Версия программы.
        /// </summary>
        public string Version;

        /// <summary>
        /// Переменные из файла настроек.
        /// </summary>
        public VarXml varXml;

        /// <summary>
        /// Коллекция данных DataSource.
        /// </summary>
        public XPCollection DataSourceCollection { get; set; }
        /// <summary>
        /// Коллекция данных OPCServers.
        /// </summary>
        public XPCollection StatisticsCollection { get; set; }
        /// <summary>
        /// Коллекция данных OPCServers.
        /// </summary>
        public XPCollection OPCServerCollection { get; set; }
        /// <summary>
        /// Коллекция данных Vzljot.
        /// </summary>
        public XPCollection VzljotCollection { get; set; }
        /// <summary>
        /// Коллекция данных DDEServer.
        /// </summary>
        public XPCollection DDEServerCollection { get; set; }
        /// <summary>
        /// Коллекция данных Technograph.
        /// </summary>
        public XPCollection TechnographCollection { get; set; }
        /// <summary>
        /// Коллекция данных PingServer.
        /// </summary>
        public XPCollection PingServerCollection { get; set; }
        /// <summary>
        /// Коллекция данных KMAZSServer.
        /// </summary>
        public XPCollection KMAZSServerCollection { get; set; }

        /// <summary>
        /// Коллекция данных OPCItem.
        /// </summary>
        public XPCollection ItemCollection { get; set; }

        /// <summary>
        /// Коллекция данных SQLServer.
        /// </summary>
        public XPCollection SQLServerCollection { get; set; }

        /// <summary>
        /// Коллекция данных SQLServerItemForceCollection.
        /// </summary>
        public XPCollection SQLServerItemForceCollection { get; set; }

        public XPCollectionWithUnits CollectionWithUnits { get; set; }

        /// <summary>
        /// Основной поток обработки данных.
        /// </summary>
        public ThreadTimer ThreadMain { get; set; }

        public CollectionEx<DataSourceReal> DataSourceRealCollection { get; set; }
        public CollectionEx<StatisticsReal> StatisticsRealCollection { get; set; }
        public CollectionEx<OPCServerReal> OPCServerRealCollection { get; set; }
        public CollectionEx<VzljotReal> VzljotRealCollection { get; set; }
        public CollectionEx<DDEServerReal> DDEServerRealCollection { get; set; }
        public CollectionEx<TechnographReal> TechnographRealCollection { get; set; }
        public CollectionEx<PingServerReal> PingServerRealCollection { get; set; }
        public CollectionEx<KMAZSServerReal> KMAZSServerRealCollection { get; set; }
        public Collection<ItemReal> ItemRealCollection { get; set; }
        public CollectionEx<SQLServerReal> SQLServerRealCollection { get; set; }

        public BackgroundWorker WorkerReboot { get; set; }

        /// <summary>
        /// Задержка между получением данных с ОРС-серверов и отправкой их SQL-серверы.
        /// </summary>
        public int ThreadMainDelay { get; set; }

        /// <summary>
        /// Название приложения.
        /// </summary>
        public string AppName { get; set; }        

        /// <summary>
        /// Время инициализации системы.
        /// </summary>
        public DateTime InitTime { get; set; }

        /// <summary>
        /// Время работы системы.
        /// </summary>
        public TimeSpan WorkTimeSpanDays { get; set; }

        /// <summary>
        /// Инициализация переменных.
        /// </summary>
        public void Init()
        {
            Version = "v1.20.31";

            InitTime = DateTime.Now;

            varXml = new VarXml("Config.xml");
            varXml.LoadFromXML();

            CollectionWithUnits = new XPCollectionWithUnits();
            DataSourceCollection = (CollectionWithUnits.Add(typeof(DataSource))).Collection;
            StatisticsCollection = (CollectionWithUnits.Add(typeof(Statistics))).Collection;
            OPCServerCollection = (CollectionWithUnits.Add(typeof(OPCServer))).Collection;
            VzljotCollection = (CollectionWithUnits.Add(typeof(Vzljot))).Collection;
            DDEServerCollection = (CollectionWithUnits.Add(typeof(DDEServer))).Collection;
            TechnographCollection = (CollectionWithUnits.Add(typeof(Technograph))).Collection;
            PingServerCollection = (CollectionWithUnits.Add(typeof(PingServer))).Collection;
            KMAZSServerCollection = (CollectionWithUnits.Add(typeof(KMAZSServer))).Collection;
            ItemCollection = (CollectionWithUnits.Add(typeof(Item))).Collection;
            SQLServerCollection = (CollectionWithUnits.Add(typeof(SQLServer))).Collection;
            SQLServerItemForceCollection = (CollectionWithUnits.Add(typeof(SQLServerItemForceCollection))).Collection;

            DataSourceRealCollection = new CollectionEx<DataSourceReal>();
            StatisticsRealCollection = new CollectionEx<StatisticsReal>();
            OPCServerRealCollection = new CollectionEx<OPCServerReal>();
            VzljotRealCollection = new CollectionEx<VzljotReal>();
            DDEServerRealCollection = new CollectionEx<DDEServerReal>();
            TechnographRealCollection = new CollectionEx<TechnographReal>();
            PingServerRealCollection = new CollectionEx<PingServerReal>();
            KMAZSServerRealCollection = new CollectionEx<KMAZSServerReal>();
            ItemRealCollection = new Collection<ItemReal>();
            SQLServerRealCollection = new CollectionEx<SQLServerReal>();

            CopyDataToReal();

            ThreadMain = new ThreadTimer() { Period = varXml.ThreadMainPeriod };
            ThreadMain.WorkChanged += ThreadMain_WorkChanged;
            ThreadMain.Run();

            ThreadMainDelay = varXml.ThreadMainDelay;
            AppName = varXml.AppName;

            WorkerReboot = new BackgroundWorker();
            WorkerReboot.DoWork += WorkerReboot_DoWork;
        }

        /// <summary>
        /// Заполнение данных реальных объектов с БД.
        /// </summary>
        public void FillData<T, TReal>(XPCollection xpCollection, CollectionEx<TReal> realCollection)
        {
            realCollection.Clear();
            foreach (T server in xpCollection)
                realCollection.Add((TReal)Activator.CreateInstance(typeof(TReal), server, ItemRealCollection));
        }
        
        /// <summary>
        /// Копирование данных из файла в коллекцию.
        /// </summary>
        public void CopyDataToReal()
        {
            foreach (Item item in ItemCollection)
                ItemRealCollection.Add(new ItemReal(item));

            FillData<DataSource, DataSourceReal>(DataSourceCollection, DataSourceRealCollection);
            FillData<Statistics, StatisticsReal>(StatisticsCollection, StatisticsRealCollection);
            FillData<OPCServer, OPCServerReal>(OPCServerCollection, OPCServerRealCollection);
            FillData<Vzljot, VzljotReal>(VzljotCollection, VzljotRealCollection);
            FillData<DDEServer, DDEServerReal>(DDEServerCollection, DDEServerRealCollection);
            FillData<Technograph, TechnographReal>(TechnographCollection, TechnographRealCollection);
            FillData<PingServer, PingServerReal>(PingServerCollection, PingServerRealCollection);
            FillData<KMAZSServer, KMAZSServerReal>(KMAZSServerCollection, KMAZSServerRealCollection);
            FillData<SQLServer, SQLServerReal>(SQLServerCollection, SQLServerRealCollection);    

            // Инициализация исключительных записей.
            foreach (SQLServerReal sqlServerReal in SQLServerRealCollection)
            {
                SQLServerItemForceCollection.Filter = CriteriaOperator.Parse("[SQLServerOwner].[Oid] == ?", sqlServerReal.Oid);

                foreach (SQLServerItemForceCollection sqlServerItemForce in SQLServerItemForceCollection)
                {
                    foreach (ItemReal itemReal in ItemRealCollection)
                    {
                        if (itemReal.Oid == sqlServerItemForce.ItemID.Oid)
                            sqlServerReal.ItemForceRealCollection.Add(itemReal);
                    }
                }
            }
        }

        /// <summary>
        /// Инициализации определенной коллекции данных.
        /// </summary>
        public static void InitXPCollection(XPCollection collection, Type type, Session session)
        {
            collection = new XPCollection();
            XPCollectionContainer.InitXPCollection(collection, type, session);
        }

        /// <summary>
        /// Закрытие подключений.
        /// </summary>
        public void OPCDisconnect()
        {
            if (ThreadMain != null)
                ThreadMain.Stop();

            if (OPCServerRealCollection != null)
                foreach (OPCServerReal opcServer in OPCServerRealCollection)
                    opcServer.TryDisconnect();
        }

        /// <summary>
        /// Перезагрузка потока и его инициализация.
        /// </summary>
        public void Reboot()
        {
            if (!WorkerReboot.IsBusy)
                WorkerReboot.RunWorkerAsync();
        }

        void WorkerReboot_DoWork(object sender, DoWorkEventArgs e)
        {
            OPCDisconnect();
            Thread.Sleep(1000);
            CopyDataToReal();
            ThreadMain.Run();
            XtraMessageBox.Show("Выполнена перезагрузка потока.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Асинхронное чтение данных.
        /// </summary>
        static void ReadDataAsync(CollectionEx<DataSourceReal> realCollection)
        {
            realCollection.ConnectedCount = 0;
            foreach (DataSourceReal opcServer in realCollection)
            {
                opcServer.ReadDataAsync();
                opcServer.SendDataToXPObject();
                if (opcServer.IsConnected)
                    realCollection.ConnectedCount++;
            }
        }

        /// <summary>
        /// Основная функция, которая выполняется в главном потоке.
        /// </summary>
        void ThreadMain_WorkChanged(object sender, EventArgs e)
        {
            // Инициализация таблиц в БД.
            if (ThreadMain.WorkCount > 2)
                foreach (SQLServerReal sqlServer in SQLServerRealCollection)
                    if (!sqlServer.TableInitiated)
                        sqlServer.TableInitAsync();

            // Чтение статистических данных.
            StatisticsRealCollection.ReadDataAsync();

            // Чтение данных с OPC-серверов.
            OPCServerRealCollection.ReadDataAsync();

            // Чтение данных со Взлетов.
            VzljotRealCollection.ReadDataAsync();

            // Чтение данных со DDE-серверов.
            DDEServerRealCollection.ReadDataAsync();

            // Чтение данных с Технографов.
            TechnographRealCollection.ReadDataAsync();

            // Чтение данных с Ping-серверов.
            PingServerRealCollection.ReadDataAsync();

            // Чтение данных с КМАЗС.
            KMAZSServerRealCollection.ReadDataAsync();

            Thread.Sleep(ThreadMainDelay);

            // Отправка значений на интерфейс.
            foreach (ItemReal item in ItemRealCollection)
                item.SendDataToXPObject();

            WorkTimeSpanDays = DateTime.Now - InitTime;

            // Отправка данных в SQL-сервера.
            SQLServerRealCollection.SendDataAsync();
        }

        /// <summary>
        /// Возврат сктроки подключения к БД, которая активна.
        /// </summary>
        public string GetActiveConnectionString()
        {
            foreach (SQLServerReal sqlServer in SQLServerRealCollection)
                if (sqlServer.IsSending)
                    return sqlServer.ConnectionString;
            return "";
        }

        /// <summary>
        /// Возврат сктроки подключения к БД, которая активна.
        /// </summary>
        public string GetActiveValuesCurrentTableName()
        {
            foreach (SQLServerReal sqlServer in SQLServerRealCollection)
                if (sqlServer.IsSending)
                    return sqlServer.ValuesCurrentTableName;
            return "";
        }

        /// <summary>
        /// Получение статуса работы главного потока.
        /// </summary>
        public string GetStatusInfo()
        {
            string strOut = "";

            if (OPCServerRealCollection.Count > 0)
            {
                strOut += string.Format("OPC: {0}/{1} в работе ",
                OPCServerRealCollection.ConnectedCount,
                OPCServerRealCollection.Count);
            }

            if (VzljotRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("Взлет: {0}/{1} в работе",
                VzljotRealCollection.ConnectedCount,
                VzljotRealCollection.Count);
            }

            if (DDEServerRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("DDE: {0}/{1} в работе",
                DDEServerRealCollection.ConnectedCount,
                DDEServerRealCollection.Count);
            }

            if (TechnographRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("Технограф: {0}/{1} в работе",
                TechnographRealCollection.ConnectedCount,
                TechnographRealCollection.Count);
            }

            if (PingServerRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("Пинг: {0}/{1} в работе",
                PingServerRealCollection.ConnectedCount,
                PingServerRealCollection.Count);
            }

            if (KMAZSServerRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("КМАЗС: {0}/{1} в работе",
                KMAZSServerRealCollection.ConnectedCount,
                KMAZSServerRealCollection.Count);
            }

            if (SQLServerRealCollection.Count > 0)
            {
                if (strOut != "") strOut += ", ";
                strOut += string.Format("SQL: {0}/{1} в работе",
                SQLServerRealCollection.ConnectedCount,
                SQLServerRealCollection.Count);
            }

            return strOut;
        }
    }

    public class Global
    {
        private readonly static GlobalDefault defaultInstance = new GlobalDefault();
        public static GlobalDefault Default { get { return defaultInstance; } }
    }
}