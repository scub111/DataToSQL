using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RapidInterface;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DataToSQL
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            //Application.ThreadException += NBug.Handler.ThreadException;

            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Application.ThreadException += Application_ThreadException;

            DBConnection.InitSkin("Office 2007 Black");

            //try
            //{
                Application.Run(new MainForm());
            /*}
            catch
            {
                Global.Default.SQLServerRealCollection.SendDataLogAsync("Ошибка MainForm void.");
                RestartApplication();
            }
             */
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Global.Default.SQLServerRealCollection.SendDataLogAsync("Ошибка UnhandledException.");
            RestartApplication();
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Global.Default.SQLServerRealCollection.SendDataLogAsync("Ошибка ThreadException.");
            RestartApplication();
        }

        /// <summary>
        /// Функция перезагрузки приложения.
        /// </summary>
        static void RestartApplication()
        {
            try
            {
                Process.Start(Application.StartupPath + "\\DataToSQL.exe");
                Process.GetCurrentProcess().Kill();
            }
            catch
            {

            }
        }

        public static void SaveLog(string text)
        {
            try
            {
                //Pass the filepath and filename to the StreamWriter Constructor
                StreamWriter stream = new StreamWriter(Path.GetDirectoryName(Application.ExecutablePath) + "\\" + "Log.txt", true);

                //Write a line of text
                stream.WriteLine(string.Format("{0} - {1}", DateTime.Now, text));

                //Close the file
                stream.Close();
            }
            catch
            {
            }
        }
    }
}
