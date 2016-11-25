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
    [DBAttribute(Caption = "Статистика", IconFile = "Statistics.png")]
    public partial class StatisticsView : DBViewInterface
    {
        public StatisticsView()
        {
            InitializeComponent();

            _dbInterface1.SetXPCollectionSmart(Global.Default.CollectionWithUnits);

            Global.Default.ThreadMain.InterfaceChanged += ThreadMain_InterfaceChanged;
        }

        void ThreadMain_InterfaceChanged(object sender, EventArgs e)
        {
            if (Visible)
                tableGridControl1.RefreshDataSource();
        }

        private void StatisticsView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<StatisticsReal>(Global.Default.StatisticsCollection, Global.Default.StatisticsRealCollection);

            foreach (StatisticsReal statistics in Global.Default.StatisticsRealCollection)
                statistics.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }
    }
}
