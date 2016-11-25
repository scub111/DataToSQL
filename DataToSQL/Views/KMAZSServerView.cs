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
    [DBAttribute(Caption = "КМАЗС", IconFile = "KMAZSServer.png")]
    public partial class KMAZSServerView : DBViewInterface
    {
        public KMAZSServerView()
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

        private void KMAZSServerView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<KMAZSServerReal>(Global.Default.KMAZSServerCollection, Global.Default.KMAZSServerRealCollection);

            foreach (KMAZSServerReal kmazsServer in Global.Default.KMAZSServerRealCollection)
                kmazsServer.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }
    }
}
