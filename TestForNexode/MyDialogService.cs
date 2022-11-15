using FolderBrowserEx;
using Microsoft.Win32;
using System;
using System.Windows;

namespace TestForTexode
{
    public class MyDialogService
    {
        public string FilePath { get; set; }

        // метод при котором пользователю нужно выбрать папку с Json файлами в проводнике
        public bool OpenFolderDialog()
        {
            FolderBrowserDialog fbd = new()
            {
                Title = "Выберите папку",
                InitialFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory),
                AllowMultiSelect = false
            };
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FilePath = fbd.SelectedFolder;
                return true;
            }
            return false;
        }

        // метод при котором пользователю нужно выбрать куда сохранить данные из таблицы в проводнике
        public bool SaveFileDialog(string filename, string defExt)
        {
            SaveFileDialog saveFileDialog = new()
            {
                AddExtension = true,
                DefaultExt = defExt,
                Title = $"Сохранить {defExt}",
                FileName = filename
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                return true;
            }
            return false;
        }
    }
}
