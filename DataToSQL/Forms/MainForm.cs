using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using RapidInterface;
using System.Threading;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using System.IO;

namespace DataToSQL
{
    public partial class MainForm : XtraForm
    {
        public MainForm()
        {
            InitializeComponent();

            //XpoDefault.DataLayer = new SimpleDataLayer(new InMemoryDataStore());
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Global.Default.Init();
            Global.Default.ThreadMain.InterfaceChanged += new EventHandler(ThreadMain_InterfaceChanged);
            WindowState = FormWindowState.Minimized;
            Global.Default.SQLServerRealCollection.SendDataLogAsync("Запуск программы.");
            Text += string.Format(" {0}", Global.Default.Version);            
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.Default.OPCDisconnect();
            Global.Default.SQLServerRealCollection.SendDataLogAsync("Ручное закрытие.");
        }
        void ThreadMain_InterfaceChanged(object sender, EventArgs e)
        {
            stInfo.Caption = Global.Default.GetStatusInfo();

            //if (Global.Default.ThreadMain.CountWork == 3)
            //    Thread.ResetAbort();

        }
    }
}
