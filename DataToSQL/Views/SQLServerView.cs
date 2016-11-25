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
using DevExpress.Xpo;
using System.Data.SqlClient;
using DevExpress.XtraEditors;

namespace DataToSQL
{
    [DBAttribute(Caption = "SQL", IconFile = "SQLServer.png")]
    public partial class SQLServerView : DBViewInterface
    {
        public SQLServerView()
        {
            InitializeComponent();

            _dbInterface1.SetXPCollectionSmart(Global.Default.CollectionWithUnits);

            Global.Default.ThreadMain.InterfaceChanged += new EventHandler(threadMain_InterfaceChanged);

            SqlConnection = new SqlConnection();
            //SqlConnection.ConnectionString = @"Data Source=172.31.106.121\SQL2012;Initial Catalog=KairControl;Persist Security Info=True;User ID=sa;Password=qwe+ASDFG";

            SqlCommand = new SqlCommand();
            SqlCommand.Connection = SqlConnection;

            ValuesCurrentTableName = "_ValuesCurrent";
            DateTimeFormat = "MM/dd/yyyy HH:mm:ss";
        }

        /// <summary>
        /// Объект подключения к БД.
        /// </summary>
        SqlConnection SqlConnection { get; set; }

        string StrCommand;

        SqlCommand SqlCommand { get; set; }

        SqlDataReader SqlDataReader { get; set; }

        /// <summary>
        /// Название таблицы текущих значений.
        /// </summary>
        public string ValuesCurrentTableName { get; set; }

        /// <summary>
        /// Формат времени.
        /// </summary>
        string DateTimeFormat { get; set; }

        void threadMain_InterfaceChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                tableGridControl1.RefreshDataSource();
                if (ceAutoUpdate.Checked)
                {
                    btnUpdate_Click(this, null);
                }
            }
        }

        ItemSql item;

        /// <summary>
        /// Инициализация элементов.
        /// </summary>
        public Collection<ItemSql> InitItems()
        {
            Collection<ItemSql> items = new Collection<ItemSql>();
            try
            {
                SqlConnection.Open();

                StrCommand = string.Format("SELECT DataName, Trend, Description, Unit, FormatValue, MinValue, MaxValue, DataType, DataValue, Quality, SqlTime, DeviceTime, TimeOut FROM {0}",
                                    ValuesCurrentTableName);

                SqlCommand.CommandText = StrCommand;

                SqlDataReader = SqlCommand.ExecuteReader();

                while (SqlDataReader.Read())
                {
                    //ItemSql item = new ItemSql()
                    item = new ItemSql()
                    {
                        DataName = SqlDataReader.GetString(0).TrimEnd(),
                        Trend = SqlDataReader.GetBoolean(1),
                        Description = SqlDataReader.GetString(2).TrimEnd(),
                        Unit = SqlDataReader.GetString(3).TrimEnd(),
                        FormatValue = SqlDataReader.GetString(4).TrimEnd(),
                        MinValue = SqlDataReader.GetDouble(5),
                        MaxValue = SqlDataReader.GetDouble(6),
                        DataType = (short)SqlDataReader.GetByte(7),
                        DataValue = SqlDataReader.GetDouble(8),
                        Quality = (short)SqlDataReader.GetInt32(9),
                        SqlTime = SqlDataReader.GetDateTime(10),
                        DeviceTime = SqlDataReader.GetDateTime(11),
                        TimeOut = SqlDataReader.GetInt32(12)
                    };
                    items.Add(item);
                }
            }
            catch
            {
                XtraMessageBox.Show("Ошибка в чтении данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SqlConnection.Close();
            return items;
        }

        private void SQLServerView_FormUpdate(object sender, EventArgs e)
        {
            LinkXPObject.Transfer<SQLServerReal>(Global.Default.SQLServerCollection, Global.Default.SQLServerRealCollection);

            foreach (SQLServerReal sqlServer in Global.Default.SQLServerRealCollection)
                sqlServer.SendDataToXPObject();

            tableGridControl1.RefreshDataSource();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (_dbInterface1.GetCurrentObject() is SQLServer)
            {
                SQLServer sqlServer = _dbInterface1.GetCurrentObject() as SQLServer;
                SqlConnection.ConnectionString = sqlServer.ConnectionString;
                Collection<ItemSql> items = InitItems();
                gridUpdate.DataSource = null;
                gridUpdate.DataSource = items;
            }
        }

        private void ceAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            btnUpdate.Enabled = !ceAutoUpdate.Checked;
        }
    }
}
