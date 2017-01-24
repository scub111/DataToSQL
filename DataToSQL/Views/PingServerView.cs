using System;
using System.Drawing;
using RapidInterface;
using DevExpress.XtraGrid.Menu;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System.Collections.Generic;

namespace DataToSQL
{
    [DBAttribute(Caption = "Пинг", IconFile = "PingServer.png")]
    public partial class PingServerView : DBViewInterface
    {
        public PingServerView()
        {
            InitializeComponent();

            _dbInterface1.SetXPCollectionSmart(Global.Default.CollectionWithUnits);

            Global.Default.ThreadMain.InterfaceChanged += new EventHandler(Global_InterfaceChanged);
        }

        void Global_InterfaceChanged(object sender, EventArgs e)
        {
            if (Visible)
                tableGridControl1.RefreshDataSource();
        }

        private void PingServerView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<PingServerReal>(Global.Default.PingServerCollection, Global.Default.PingServerRealCollection);

            foreach (PingServerReal pingServer in Global.Default.PingServerRealCollection)
                pingServer.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }

        //Create a menu item 
        DXMenuCheckItem CreateCheckItem(string caption, GridColumn column, Image image, EventHandler eventHandler)
        {
            DXMenuCheckItem item = new DXMenuCheckItem(caption, false, image, eventHandler);
            item.Tag = column;
            return item;
        }

        private void tableGridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == GridMenuType.Column)
            {
                GridViewColumnMenu menu = e.Menu as GridViewColumnMenu;
                if (menu.Column == null)
                {
                    menu.Items.Add(CreateCheckItem("Заполнить остальные поля", menu.Column, null, FillEmptyField));
                    menu.Items.Add(CreateCheckItem("Найти совпадения адресов", menu.Column, null, FindEqualAddresses));
                    menu.Items.Add(CreateCheckItem("Сгенерировать IP-адреса", menu.Column, null, GenerateIPAddress));
                }
            }
        }

        private void FillEmptyField(object sender, EventArgs e)
        {            
            foreach (PingServer pingServer in Global.Default.PingServerCollection)
            {
                if (string.IsNullOrEmpty(pingServer.Caption))
                    pingServer.Caption = string.Format("Узел сети {0}", pingServer.Address);
                if (string.IsNullOrEmpty(pingServer.Comment))
                    pingServer.Comment = "{" + string.Format("{0};-;1", pingServer.Address) + "}";
                if (string.IsNullOrEmpty(pingServer.SQLPrefix))
                    pingServer.SQLPrefix = string.Format("ping_{0}_", Item.GetSQLTableName(pingServer.Address));
                if (string.IsNullOrEmpty(pingServer.DescriptionPrefix))
                    pingServer.DescriptionPrefix = string.Format("Узел сети {0}. ", pingServer.Address);
            }
        }

        private void FindEqualAddresses(object sender, EventArgs e)
        {
            Dictionary<string, PingServer> pingDict = new Dictionary<string, PingServer>();

            Dictionary<string, PingServer> equalAddresses = new Dictionary<string, PingServer>();

            foreach (PingServer ping in Global.Default.PingServerCollection)
            {
                if (!pingDict.ContainsKey(ping.Address))
                    pingDict.Add(ping.Address, ping);
                else
                    if (!equalAddresses.ContainsKey(ping.Address))
                        equalAddresses.Add(ping.Address, ping);
            }

            string result = "";

            foreach (var address in equalAddresses)
                result += address.Key + "; ";

            EqualAddressForm equalAddressForm = new EqualAddressForm(equalAddresses);
            equalAddressForm.ShowDialog();
        }

        static void GenerateIPAddress(string network, int from, int to, Dictionary<string, PingServer> pingServerDict)
        {
            string address;
            for (int i = from; i <= to; i++)
            {
                address = string.Format("{0}.{1}", network, i);
                if (!pingServerDict.ContainsKey(address))
                {
                    PingServer pingServer = new PingServer(Global.Default.PingServerCollection.Session) { Address = address };
                    Global.Default.PingServerCollection.Add(pingServer);
                }
            }
        }

        private void GenerateIPAddress(object sender, EventArgs e)
        {
            Dictionary<string, PingServer> pingServerDict = new Dictionary<string, PingServer>();

            foreach (PingServer item in Global.Default.PingServerCollection)
                if (!pingServerDict.ContainsKey(item.Address))
                    pingServerDict.Add(item.Address, item);

            // Рудоуправление.
            GenerateIPAddress("172.31.106", 1, 255, pingServerDict);

            // Ангидрит.
            GenerateIPAddress("172.24.92", 1, 255, pingServerDict);

            // Известняки.
            GenerateIPAddress("172.24.43", 193, 255, pingServerDict);

            // Скальный.
            GenerateIPAddress("172.24.88", 65, 94, pingServerDict);

            // Кайерканский.
            GenerateIPAddress("172.24.228", 1, 128, pingServerDict);

            // КУР.
            GenerateIPAddress("172.31.71", 1, 255, pingServerDict);
        }
    }
}
