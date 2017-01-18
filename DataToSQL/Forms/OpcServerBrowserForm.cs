using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections.ObjectModel;
using OPC.Common;
using DevExpress.XtraSplashScreen;
using RapidInterface;

namespace DataToSQL
{
    public partial class OpcServerBrowserForm : XtraForm
    {
        public class OpcServerDef
        {
            public OpcServerDef(OpcServers opcServers)
            {
                OpcServers = opcServers;
            }

            /// <summary>
            /// OPC-сервер.
            /// </summary>
            OpcServers OpcServers { get; set; }

            /// <summary>
            /// Имя сервера.
            /// </summary>
            public string ServerName { get { return OpcServers.ServerName; } }

            /// <summary>
            /// Имя сервиса.
            /// </summary>
            public string ProgID { get { return OpcServers.ProgID; } }

            /// <summary>
            /// GUID.
            /// </summary>
            public Guid ClsID { get { return OpcServers.ClsID; } }
        }

        public OpcServerBrowserForm()
        {
            InitializeComponent();
            OpcServerDefs = new Collection<OpcServerDef>();
            OpcServerList = new OpcServerList();

            gridProgID.DataSource = OpcServerDefs;
        }

        /// <summary>
        /// Список доступных сервисов.
        /// </summary>
        Collection<OpcServerDef> OpcServerDefs { get; set; }

        /// <summary>
        /// Список OPC-серверов.
        /// </summary>
        OpcServerList OpcServerList { get; set; }

        /// <summary>
        /// Компьютер.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Имя сервиса.
        /// </summary>
        public string ProgID { get; set; }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SplashScreenManager.ShowForm(typeof(WaitFormEx));
            SplashScreenManager.Default.SetWaitFormDescription("Происходит поиск OPC-серверов.");

            OpcServers[] opcServers = new OpcServers[0];

            try
            {
                opcServers = OpcServerList.ListAllEx(textServer.Text);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show(ex.Message, ex.Source);
            }

            OpcServerDefs.Clear();
            for (int i = 0; i < (opcServers.Length); i++)
                OpcServerDefs.Add(new OpcServerDef(opcServers[i]));
           
            gridProgID.RefreshDataSource();
            stlInfo.Caption = string.Format("Всего: {0}", OpcServerDefs.Count);
            SplashScreenManager.CloseForm();
        }

        private void OpcServerBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server = textServer.Text;
            OpcServerDef row = viewProgID.GetFocusedRow() as OpcServerDef;
            if (row != null)
                ProgID = row.ProgID;
        }
    }
}