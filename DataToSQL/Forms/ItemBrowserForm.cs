using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections;
using System.Collections.ObjectModel;

namespace DataToSQL
{
    public partial class ItemBrowserForm : XtraForm
    {

        public class ItemDef
        {
            public ItemDef(string itemID)
            {
                ItemID = itemID;
            }

            /// <summary>
            /// Имя OPC-переменной.
            /// </summary>
            public string ItemID { get; set; }
        }

        /// <summary>
        /// Заполнение данных реальных объектов с БД.
        /// </summary>
        public static DataSourceReal FindSource<TReal>(Item item, CollectionEx<TReal> realCollection)
        {
            foreach (TReal server in realCollection)
                if (item.DataSource != null &&
                    (server as DataSourceReal).Oid == item.DataSource.Oid)
                    return server as DataSourceReal;

            return null;
        }

        public ItemBrowserForm(Item item)
        {
            InitializeComponent();

            Item = item;
            
            if (item.DataSource is Statistics)
                DataSourceReal = FindSource(item, Global.Default.StatisticsRealCollection);      
            else if (item.DataSource is OPCServer)
                DataSourceReal = FindSource(item, Global.Default.OPCServerRealCollection);
            else if (item.DataSource is DDEServer)
                DataSourceReal = FindSource(item, Global.Default.DDEServerRealCollection);
            else if (item.DataSource is Technograph)
                DataSourceReal = FindSource(item, Global.Default.TechnographRealCollection);
            else if (item.DataSource is Vzljot)
                DataSourceReal = FindSource(item, Global.Default.VzljotRealCollection);
            else if (item.DataSource is PingServer)
                DataSourceReal = FindSource(item, Global.Default.PingServerRealCollection);             
            else if (item.DataSource is KMAZSServer)
                DataSourceReal = FindSource(item, Global.Default.KMAZSServerRealCollection);             
            ItemDefs = new Collection<ItemDef>();
        }

        /// <summary>
        /// Объект реального времени.
        /// </summary>
        public DataSourceReal DataSourceReal { get; set; }

        /// <summary>
        /// Выбранный элемент БД.
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// Имя переменной.
        /// </summary>
        public string ItemID { get; set; }

        /// <summary>
        /// Список всех ОРС-переменных.
        /// </summary>
        public Collection<ItemDef> ItemDefs { get; set; }

        private void OpcItemBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ItemDef row = viewOpcItem.GetFocusedRow() as ItemDef;
            if (row != null)
                ItemID = row.ItemID;
        }

        private void ItemBrowserForm_Load(object sender, EventArgs e)
        {            
            if (DataSourceReal == null)
            {
                XtraMessageBox.Show("Не найден объект DataSourceReal!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            
            txtCaption.Text = DataSourceReal.Caption;
            txtComment.Text = DataSourceReal.Comment;
            /*
            if (DataSourceReal is PingServerReal)
            {
                ItemDefs.Add(new ItemDef("Status"));
                ItemDefs.Add(new ItemDef("ReplyTime"));
            }
             * */

            ItemDefs.Add(new ItemDef("_ServiceInfo_IsConnected"));
            ItemDefs.Add(new ItemDef("_ServiceInfo_IsReading"));
            ItemDefs.Add(new ItemDef("_ServiceInfo_ConnectSuccessCount"));
            ItemDefs.Add(new ItemDef("_ServiceInfo_ConnectFaultCount"));
            ItemDefs.Add(new ItemDef("_ServiceInfo_ReceiveSuccessCount"));
            ItemDefs.Add(new ItemDef("_ServiceInfo_ReceiveFaultCount"));            
            ItemDefs.Add(new ItemDef("_ServiceInfo_ReadTimeSpanMs"));

            if (DataSourceReal is StatisticsReal)
            {
                ItemDefs.Add(new ItemDef("ActivityDays"));
                ItemDefs.Add(new ItemDef("ThreadMainCountWork"));
            }
            else if (DataSourceReal is OPCServerReal)
            {
                OPCServerReal opcServerReal = (OPCServerReal)DataSourceReal;

                if (opcServerReal.OpcServer != null &&
                    opcServerReal.OpcServer.IsConnected)
                {
                    ArrayList list = opcServerReal.OpcServer.Browse();

                    int index = -1;
                    string itemID;
                    for (int i = 0; i < list.Count; i++)
                    {
                        itemID = list[i].ToString();
                        ItemDefs.Add(new ItemDef(itemID));
                        if (itemID == Item.ItemID)
                            index = i;
                    }

                    if (index != -1)
                    {
                        int rowHandel = viewOpcItem.GetRowHandle(index);
                        viewOpcItem.FocusedRowHandle = rowHandel;
                        viewOpcItem.TopRowIndex = rowHandel;
                    }
                }
            }
            else if (DataSourceReal is TechnographReal)
            {
                for (int i = 0; i < 12; i++)
                    ItemDefs.Add(new ItemDef(string.Format("K{0}", i + 1)));
            }
            else if (DataSourceReal is PingServerReal)
            {
                ItemDefs.Add(new ItemDef("Status"));
                ItemDefs.Add(new ItemDef("ReplyTime"));
            }
            else if (DataSourceReal is KMAZSServerReal)
            {
                KMAZSServerReal kmazsServerReal = (KMAZSServerReal)DataSourceReal;

                Collection<DataToSQL.KMAZSServerReal.TANKCONFIG> collection = kmazsServerReal.GetTANKCONFIGs();

                foreach (DataToSQL.KMAZSServerReal.TANKCONFIG item in collection)
                {
                    ItemDefs.Add(new ItemDef(string.Format("KM_ID_{0}_Level", item.KM_ID)));
                    ItemDefs.Add(new ItemDef(string.Format("KM_ID_{0}_Volume", item.KM_ID)));
                    ItemDefs.Add(new ItemDef(string.Format("KM_ID_{0}_Plotn", item.KM_ID)));
                    ItemDefs.Add(new ItemDef(string.Format("KM_ID_{0}_Temper", item.KM_ID)));
                }
            }
            
            gridItem.DataSource = ItemDefs;
            stlInfo.Caption = String.Format("Всего: {0}", ItemDefs.Count);
             
        }
    }
}
