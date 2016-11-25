using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RapidInterface;

namespace DataToSQL
{
    [DBAttribute(Caption = "DDE", IconFile = "DDEServer.png")]
    public partial class DDEServerView : DBViewInterface
    {
        public DDEServerView()
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

        private void DDEServerView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<DDEServerReal>(Global.Default.DDEServerCollection, Global.Default.DDEServerRealCollection);

            foreach (DDEServerReal ddeServer in Global.Default.DDEServerRealCollection)
                ddeServer.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }
    }
}
