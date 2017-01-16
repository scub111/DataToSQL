using System;
using System.Drawing;
using RapidInterface;
using DevExpress.XtraGrid.Menu;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;

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
                    menu.Items.Add(CreateCheckItem("Заполнить остальные поля", menu.Column, null, new EventHandler(FillEmptyField)));
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
    }
}
