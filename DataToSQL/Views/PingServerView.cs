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
    }
}
