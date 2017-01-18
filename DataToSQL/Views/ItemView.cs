using System;
using System.Windows.Forms;
using RapidInterface;
using DevExpress.XtraEditors;
using DevExpress.Utils.Menu;
using DevExpress.XtraGrid.Columns;
using System.Drawing;
using DevExpress.XtraGrid.Menu;
using DevExpress.XtraGrid.Views.Grid;
using System.Collections.Generic;
using DevExpress.Xpo;

namespace DataToSQL
{
    [DBAttribute(Caption = "Элемент", IconFile = "Item.png")]
    public partial class ItemView : DBViewInterface
    {
        public ItemView()
        {
            InitializeComponent();
            _dbInterface1.SetXPCollectionSmart(Global.Default.CollectionWithUnits);
            Global.Default.ThreadMain.InterfaceChanged += new EventHandler(threadMain_InterfaceChanged);
        }

        void threadMain_InterfaceChanged(object sender, EventArgs e)
        {
            if (Visible)
                tableGridControl1.RefreshDataSource();
        }

        private void repItem_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            Item opcItem = tableGridView1.GetFocusedRow() as Item;
            if (opcItem != null &&
                opcItem.DataSource != null)
            {

                ItemBrowserForm opcItemBrowser = new ItemBrowserForm(opcItem);
                opcItemBrowser.ShowDialog();
                if (opcItemBrowser.DialogResult == DialogResult.OK)
                {
                    opcItem.ItemID = opcItemBrowser.ItemID;

                    ButtonEdit edit = sender as ButtonEdit;
                    edit.Text = opcItemBrowser.ItemID;
                }
            }
        }

        private void repSQLTrend_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            Item item = tableGridView1.GetFocusedRow() as Item;
            if (item != null)
            {
                ItemTrendForm opcItemTrendForm = new ItemTrendForm(item);
                opcItemTrendForm.Show();
            }
        }

        private void OPCItemView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<ItemReal>(Global.Default.ItemCollection, Global.Default.ItemRealCollection);

            foreach (ItemReal opcItem in Global.Default.ItemRealCollection)
                opcItem.SendDataToXPObject();

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
                    menu.Items.Add(CreateCheckItem("Добавить недостающие записи Пинг-серверов", menu.Column, null, new EventHandler(AddPingServers)));
                }
            }
        }

        private void AddPingServers(object sender, EventArgs e)
        {
            Dictionary<string, Item> itemDict = new Dictionary<string, Item>();

            foreach (Item item in Global.Default.ItemCollection)
                if (!itemDict.ContainsKey(item.SQLTableName))
                    itemDict.Add(item.SQLTableName, item);

            foreach (PingServer pingServer in new XPCollection<PingServer>(Global.Default.ItemCollection.Session))
            {
                if (!itemDict.ContainsKey(string.Format("{0}Status", pingServer.SQLPrefix)))
                {
                    Item itemNew = new Item(Global.Default.ItemCollection.Session);
                    itemNew.DataSource = pingServer;
                    itemNew.ItemID = "Status";
                    Global.Default.ItemCollection.Add(itemNew);
                }

                if (!itemDict.ContainsKey(string.Format("{0}ReplyTime", pingServer.SQLPrefix)))
                {
                    Item itemNew = new Item(Global.Default.ItemCollection.Session);
                    itemNew.DataSource = pingServer;
                    itemNew.ItemID = "ReplyTime";
                    Global.Default.ItemCollection.Add(itemNew);
                }
            }

            foreach (Item item in Global.Default.ItemCollection)
            {
                if (item.DataSource is PingServer)
                {
                    if (item.ItemID == "Status")
                    {
                        item.Description = string.Format("{0}Статус", item.DataSource.DescriptionPrefix);
                        item.Unit = "";
                        item.FormatValue = "";
                    }
                    else if (item.ItemID == "ReplyTime")
                    {
                        item.Description = string.Format("{0}Время отлика", item.DataSource.DescriptionPrefix);
                        item.Unit = "мс";
                        item.FormatValue = "0";
                    }
                    item.Comment = item.DataSource.Comment;
                }
            }
        }
    }
}
