using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RapidInterface;
using DevExpress.XtraEditors;

namespace DataToSQL
{
    [DBAttribute(Caption = "Настройки", IconFile = "Setting.png")]
    public partial class SettingView : DBViewBase
    {
        public SettingView()
        {
            InitializeComponent();
            varXml = new VarXml("Config.xml");
        }

        private void btnReboot_Click(object sender, EventArgs e)
        {
            Global.Default.Reboot();
        }

        int TidelogCounterLast = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                if (Global.Default.ThreadMain != null)
                {
                    spinThreadMainCountWork.Value = Global.Default.ThreadMain.WorkCount;
                    //spCycleSpan.Text = Global.Default.ThreadMain.CycleSpan.ToString("{0.##}");

                    spinThreadMainFautCount.Value = Global.Default.ThreadMainFautCount;

                    spCycleSpan.Text = Global.Default.ThreadMain.CycleSpan.TotalMilliseconds.ToString();
                    if (Global.Default.ThreadMain.IsBusy)
                        textThreadMainStatus.Text = "Выполняется";
                    else
                        textThreadMainStatus.Text = "Остановлен";                    
                }
                else
                {
                    textThreadMainStatus.Text = "null";
                }

                if (Global.Default.KMAZSServerRealCollection.Count > 0)
                {
                    lblTidelogInfo.Text = string.Format("Tidelog info: {0} ({1}/c)", Global.Default.KMAZSServerRealCollection[0].TidelogCounter, Global.Default.KMAZSServerRealCollection[0].TidelogCounter - TidelogCounterLast);

                    TidelogCounterLast = Global.Default.KMAZSServerRealCollection[0].TidelogCounter;
                }
            }
        }

        private VarXml varXml;

        private void btnThreadMainRun_Click(object sender, EventArgs e)
        {
            Global.Default.ThreadMain.Run();
        }

        private void btnThreadMainStop_Click(object sender, EventArgs e)
        {
            Global.Default.ThreadMain.Stop();
        }

        private void SettingView_Load(object sender, EventArgs e)
        {
            varXml.LoadFromXML();
        }

        private void btnSettingsSave_Click(object sender, EventArgs e)
        {
            varXml.ThreadMainPeriod = int.Parse(spinThreadMainPeriod.Text);
            varXml.ThreadMainDelay = int.Parse(spinThreadMainDelay.Text);
            varXml.AppName = textAppName.Text;
            varXml.SaveToXML();
            XtraMessageBox.Show("Сохранены настройки программы.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SettingView_FormUpdate(object sender, EventArgs e)
        {
            if (Global.Default.ThreadMain != null)
            {
                spinThreadMainPeriod.Text = varXml.ThreadMainPeriod.ToString();
                spinThreadMainDelay.Text = varXml.ThreadMainDelay.ToString();
                textAppName.Text = varXml.AppName;
            }
            else
            {
                btnThreadMainRun.Enabled = false;
                btnThreadMainStop.Enabled = false;
            }
        }

        private void btnUpdateSchema_Click(object sender, EventArgs e)
        {
            DBConnection.UpdateSchema();
        }
    }
}
