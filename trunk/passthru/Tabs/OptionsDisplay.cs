using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using FM;

namespace PassThru
{
    /// <summary>
    /// Object used for options interfacing
    /// </summary>
    public partial class OptionsDisplay : UserControl
    {
        public OptionsDisplay()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event for when displayTrayLogs changes state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void displayTrayLogs_CheckedChanged(object sender, EventArgs e)
        {
            TrayIcon.displayTrayLogs = displayTrayLogs.Checked;
        }

        private void OptionsDisplay_Load(object sender, EventArgs e)
        {
            switch (LanguageConfig.GetCurrentLanguage())
            {
                case LanguageConfig.Language.NONE:
                case LanguageConfig.Language.ENGLISH:
                    displayTrayLogs.Text = "Display Icon Popups";
                    break;
                case LanguageConfig.Language.CHINESE:
                    displayTrayLogs.Text = "显示图标弹出窗口";
                    break;
                case LanguageConfig.Language.GERMAN:
                    displayTrayLogs.Text = "Anzeige Icon Popups";
                    break;
                case LanguageConfig.Language.RUSSIAN:
                    displayTrayLogs.Text = "Показать Иконка всплывающие окна";
                    break;
                case LanguageConfig.Language.SPANISH:
                    displayTrayLogs.Text = "Mostrar ventanas emergentes Icono";
                    break;
            }
        }
    }
}
