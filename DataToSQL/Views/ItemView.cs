using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RapidInterface;
using DevExpress.Xpo;
using DevExpress.XtraEditors;

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
    }
}
