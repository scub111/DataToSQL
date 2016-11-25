using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RapidInterface;
using System.Collections.ObjectModel;
using DevExpress.XtraEditors;

namespace DataToSQL
{
    [DBAttribute(Caption = "OPC->Взлет", IconFile = "Vzljot.png")]
    public partial class VzljotView : DBViewInterface
    {
        public VzljotView()
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

        private void _dbInterface1_DataBaseSaved(object sender, EventArgs e)
        {
        }

        private void repOpcServer_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            OPCServer opcServer = tableGridView1.GetFocusedRow() as OPCServer;
            if (opcServer != null)
            {
                OpcServerBrowserForm opcServerBrowser = new OpcServerBrowserForm();
                opcServerBrowser.ShowDialog();
                if (opcServerBrowser.DialogResult == DialogResult.OK)
                {

                    opcServer.Server = opcServerBrowser.Server;
                    opcServer.ProgID = opcServerBrowser.ProgID;

                    ButtonEdit edit = sender as ButtonEdit;
                    edit.Text = opcServerBrowser.ProgID;
                }
            }
        }

        private void VzljotView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<VzljotReal>(Global.Default.VzljotCollection, Global.Default.VzljotRealCollection);

            foreach (VzljotReal vzljot in Global.Default.VzljotRealCollection)
                vzljot.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }
    }
}
