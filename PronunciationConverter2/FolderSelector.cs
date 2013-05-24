using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PronunciationConverter2
{
    class FolderSelector
    {
        public static String select()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                return dialog.SelectedPath;
            else
                return "";
        }
    }
}
