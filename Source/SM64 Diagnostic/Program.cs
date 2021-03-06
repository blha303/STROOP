﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SM64_Diagnostic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoadingHandler.ShowLoadingForm();

            var mainForm = new StroopMainForm();
            mainForm.LoadConfig(LoadingHandler.LoadingForm);

            LoadingHandler.CloseForm();
            Application.Run(mainForm);
        }
    }
}
