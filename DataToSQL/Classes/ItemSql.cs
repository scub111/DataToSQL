using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataToSQL
{
    #region ItemSqlTrend
    /// <summary>
    /// Класс трендов.
    /// </summary>
    public class ItemSqlTrend
    {
        public ItemSqlTrend()
        {
        }

        /// <summary>
        /// Значение.
        /// </summary>
        public double DataValue { get; set; }

        /// <summary>
        /// Время записи на SQL-сервере.
        /// </summary>
        public DateTime SqlTime { get; set; }
    }
    #endregion

    #region ItemSqlSimple
    /// <summary>
    /// Класс обновления значений.
    /// </summary>
    public class ItemSqlSimple : ItemSqlTrend
    {
        public ItemSqlSimple()
        {
        }

        /// <summary>
        /// Качество.
        /// </summary>
        public short Quality { get; set; }

        /// <summary>
        /// Штамп времени.
        /// </summary>
        public DateTime DeviceTime { get; set; }
    }
    #endregion

    #region ItemSql
    /// <summary>
    /// Класс элементов реального времени.
    /// </summary>
    public class ItemSql : ItemSqlSimple
    {
        public ItemSql()
        {
        }
        /// <summary>
        /// Имя SQL-таблицы.
        /// </summary>
        public string DataName { get; set; }

        /// <summary>
        /// Тренд.
        /// </summary>
        public bool Trend { get; set; }

        /// <summary>
        /// Описание.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Единица измерения.
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// Формат значения.
        /// </summary>
        public string FormatValue { get; set; }

        /// <summary>
        /// Минимальное значение.
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// Максимальное значение.
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// Упрощенный тип переменной.
        /// </summary>
        public short DataType { get; set; }

        /// <summary>
        /// Таймаут, с.
        /// </summary>
        public int TimeOut { get; set; }
    }
    #endregion
}
