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
    [DBAttribute(Caption = "Test", IconFile = "TestView.png")]
    public partial class TestView : DBViewInterface
    {
        public TestView()
        {
            InitializeComponent();
        }
    }
}
