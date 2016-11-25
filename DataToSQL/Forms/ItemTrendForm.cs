using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Collections;
using System.Collections.ObjectModel;
using DevExpress.XtraCharts;
using System.Data.SqlClient;
using DevExpress.XtraSplashScreen;
using RapidInterface;
using System.Threading;

namespace DataToSQL
{
    public partial class ItemTrendForm : XtraForm
    {
        public ItemTrendForm(Item item)
        {
            InitializeComponent();
            Item = item;

            DateTimeFormat = "MM/dd/yyyy HH:mm:ss";

            Series = new Series("Series1", ViewType.SwiftPlot);

            MinValue = MinValueInternal = 0;
            MaxValue = MaxValueInternal = 1;

            foreach (ItemReal opcItemReal in Global.Default.ItemRealCollection)
                if (opcItemReal.Oid == item.Oid)
                {
                    OPCItemReal = opcItemReal;
                    break;
                }

            if (OPCItemReal == null)
            {
                XtraMessageBox.Show("Не найден объект OPCItemReal!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
        }

        /// <summary>
        /// Выбранный элемент БД.
        /// </summary>
        public Item Item { get; set; }

        /// <summary>
        /// Объект реального времени.
        /// </summary>
        public ItemReal OPCItemReal { get; set; }

        /// <summary>
        /// Формат времени.
        /// </summary>
        string DateTimeFormat { get; set; }

        /// <summary>
        /// Тренды, которые выводятся на компонент графика.
        /// </summary>
        Series Series { get; set; }

        /// <summary>
        /// Минимальное значение.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Минимальное значение.
        /// </summary>
        public double MinValueInternal { get; set; }

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        public double MaxValueInternal { get; set; }

        /// <summary>
        /// Время обновления.
        /// </summary>
        public TimeSpan UpdateDiff { get; set; }

        /// <summary>
        /// Начальное время.
        /// </summary>
        private DateTime T0 { get; set; }

        private DataTable CreateChartData()
        {
            T0 = DateTime.Now;

            SplashScreenManager.ShowForm(typeof(WaitFormEx));
            SplashScreenManager.Default.SetWaitFormDescription("Происходит сбор данных с SQL-сервера.");

            // Create an empty table.
            DataTable table = new DataTable("Table1");

            //Thread.Sleep(5000);

            // Add two columns to the table.
            table.Columns.Add("Argument", typeof(DateTime));
            table.Columns.Add("Value", typeof(double));

            DataRow row = null;

            SqlConnection sqlConnection = new SqlConnection();
            sqlConnection.ConnectionString = Global.Default.GetActiveConnectionString();
            if (sqlConnection.ConnectionString != "")
            {
                try
                {
                    sqlConnection.Open();
                    string strCommand = string.Format("SELECT SqlTime, DataValue FROM {0} WHERE SqlTime BETWEEN '{1}' AND '{2}'",
                        Item.SQLTableName,
                        ((DateTime)dateBegin.EditValue).ToString(DateTimeFormat),
                        ((DateTime)dateEnd.EditValue).ToString(DateTimeFormat));
                    SqlCommand sqlCommand = new SqlCommand(strCommand, sqlConnection);

                    SqlDataReader reader = sqlCommand.ExecuteReader();

                    int i = 0;

                    while (reader.Read())
                    {
                        row = table.NewRow(); 
                        
                        row["Argument"] = reader.GetDateTime(0);
                        if (OPCItemReal.CanonicalDataTypeSimple == DataToSQL.ItemReal.DataType.Integer)
                            row["Value"] = reader.GetInt32(1);
                        else if (OPCItemReal.CanonicalDataTypeSimple == DataToSQL.ItemReal.DataType.Boolean)
                            row["Value"] = reader.GetBoolean(1);
                        else
                            row["Value"] = reader.GetDouble(1);

                        if (i == 0)
                            MinValueInternal = MaxValueInternal = (double)row["Value"];

                        if ((double)row["Value"] < MinValueInternal)
                            MinValueInternal = (double)row["Value"];

                        if ((double)row["Value"] > MaxValueInternal)
                            MaxValueInternal = (double)row["Value"];

                        table.Rows.Add(row);

                        i++;
                    }

                    reader.Close();

                    string valuesCurrentTableName = Global.Default.GetActiveValuesCurrentTableName();
                    if (valuesCurrentTableName != "")
                    {
                        strCommand = string.Format("SELECT MinValue, MaxValue FROM {0} WHERE DataName = '{1}'",
                            valuesCurrentTableName,
                            Item.SQLTableName);
                        sqlCommand = new SqlCommand(strCommand, sqlConnection);

                        reader = sqlCommand.ExecuteReader();

                        reader.Read();

                        MinValue = reader.GetDouble(0);
                        MaxValue = reader.GetDouble(1);
                    }
                }
                catch
                {
                    XtraMessageBox.Show("Ошибка запроса данных с SQL-сервера.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                }
                sqlConnection.Close();
            }

            SplashScreenManager.CloseForm();

            UpdateDiff = DateTime.Now - T0;

            return table;
        }

        /// <summary>
        /// Обновление графика.
        /// </summary>
        private void UpdateChart()
        {
            Series.DataSource = CreateChartData();
            stlInfo.Caption = string.Format("Всего {0} за {1} мс", Series.Points.Count, UpdateDiff.TotalMilliseconds);
        }

        private void OpcItemBrowserForm_Load(object sender, EventArgs e)
        {
            this.Text = Item.Description;

            textOPCItemID.Text = Item.ItemID;
            textSQLTableName.Text = Item.SQLTableName;
            dateBegin.EditValue = DateTime.Now - new TimeSpan(1, 0, 0);
            dateEnd.EditValue = DateTime.Now;

            chartControl1.Series.Add(Series);

            // Specify data members to bind the series.
            Series.ArgumentScaleType = ScaleType.DateTime;
            Series.ArgumentDataMember = "Argument";
            Series.ValueScaleType = ScaleType.Numerical;
            Series.ValueDataMembers.AddRange(new string[] { "Value" });

            ((SwiftPlotDiagram)chartControl1.Diagram).AxisX.DateTimeMeasureUnit = DateTimeMeasurementUnit.Second;
            ((SwiftPlotDiagram)chartControl1.Diagram).AxisX.DateTimeOptions.Format = DevExpress.XtraCharts.DateTimeFormat.Custom;
            ((SwiftPlotDiagram)chartControl1.Diagram).AxisX.DateTimeOptions.FormatString = "dd-MM \n HH:mm:ss";

            UpdateChart();

            if (Series.Points.Count > 0)
            {
                ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.VisualRange.MaxValue= MaxValue;
                ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.VisualRange.MinValue = MinValue;
            }
            else
            {
                XtraMessageBox.Show("Нет данных", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void OpcItemBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateChart();
        }

        private void cbRangeAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (cbRangeAuto.Checked)
            {
                try
                {
                    if (MinValueInternal != MaxValueInternal)
                    {
                        double range = MaxValueInternal - MinValueInternal;
                        double offset = 0.05 * range;
                        ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MaxValue = MaxValueInternal + offset;
                        ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MinValue = MinValueInternal - offset;
                    }
                    else
                    {
                        ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MaxValue = MaxValueInternal + 0.0001;
                        ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MinValue = MinValueInternal - 0.0001;
                    }
                }
                catch
                {
                    XtraMessageBox.Show(string.Format("MinValueInternal = {0}, MaxValueInternal = {1}", MinValueInternal, MaxValueInternal), "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                //((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.Auto = false;
                ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MinValue = MinValue;
                ((SwiftPlotDiagram)chartControl1.Diagram).AxisY.Range.MaxValue = MaxValue;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            TimeSpan diff = (DateTime)dateEnd.EditValue - (DateTime)dateBegin.EditValue;
            dateEnd.EditValue = dateBegin.EditValue;
            dateBegin.EditValue = (DateTime)dateEnd.EditValue - diff;
            UpdateChart();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            TimeSpan diff = (DateTime)dateEnd.EditValue - (DateTime)dateBegin.EditValue;
            dateBegin.EditValue = dateEnd.EditValue;
            dateEnd.EditValue = (DateTime)dateBegin.EditValue + diff;
            UpdateChart();
        }
    }
}
