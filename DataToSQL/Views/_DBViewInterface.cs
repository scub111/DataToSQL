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
    [DBAttribute(Caption = "_DBViewInterface", IconFile = "_DBViewInterface.png")]
    public partial class _DBViewInterface : DBViewInterface
    {
        public _DBViewInterface()
        {
            InitializeComponent();
        }
    }
}
