using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RapidInterface;
using System.IO.Ports;
using System.Threading;
using System.Collections.ObjectModel;

namespace DataToSQL
{

    [DBAttribute(Caption = "Технограф", IconFile = "Technograph.png")]
    public partial class TechnographView : DBViewInterface
    {
        public TechnographView()
        {
            InitializeComponent();

            _dbInterface1.SetXPCollectionSmart(Global.Default.CollectionWithUnits);

            Global.Default.ThreadMain.InterfaceChanged += new EventHandler(Global_InterfaceChanged);
        }

        void Global_InterfaceChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                tableGridControl1.RefreshDataSource();
            }
        }

        private void TechnographView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<TechnographReal>(Global.Default.TechnographCollection, Global.Default.TechnographRealCollection);

            foreach (TechnographReal technograph in Global.Default.TechnographRealCollection)
                technograph.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }

        TechnographReal tech { get; set; }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            tech = new TechnographReal(new Technograph(), new Collection<ItemReal>());

            tech.Parse(memoEdit1.Text);
        }
    }
}
