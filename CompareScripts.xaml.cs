﻿using DBScriptSaver.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DBScriptSaver
{
    /// <summary>
    /// Логика взаимодействия для CompareScripts.xaml
    /// </summary>
    public partial class CompareScripts : Window
    {
        private ProjectDataBase DB;
        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        public CompareScripts()
        {
            InitializeComponent();
            timer.Tick += ApplyTextFilter;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 400);
        }
        private void ApplyTextFilter(object sender, EventArgs e)
        {
            timer.Stop();
            Filtering();
        }
        private void Filtering()
        {
            (gcDBObjects.ItemsSource as ListCollectionView).Filter = new Predicate<object>(Filter);
        }
        private bool Filter(object obj)
        {
            var o = obj as ScriptWrapper;

            if (tbFilter.Text.Length > 0)
            {
                if (!o.FileName.ToUpper().Contains(tbFilter.Text.ToUpper()))
                {
                    return false;
                }
            }
            return true;
        }

        public CompareScripts(ProjectDataBase db, List<(string FileName, string FullPath, string ScriptText)> scripts) : this()
        {
            DataContext = scripts;
            gcDBObjects.ItemsSource = new ListCollectionView(scripts.Select(t => new ScriptWrapper(t)).ToList());
            DB = db;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            var Выбранные_скрипты = ((ListCollectionView)gcDBObjects.ItemsSource).Cast<ScriptWrapper>().Where(w => w.Save).Select(w => w.tuple).ToList();
            DB.UpdateScripts(Выбранные_скрипты);
            DialogResult = true;
        }

        private void ВыбратьВсе_Click(object sender, RoutedEventArgs e)
        {
            ((ListCollectionView)gcDBObjects.ItemsSource).Cast<ScriptWrapper>().ToList().ForEach(w => w.Save = true);
        }

        private void ОтменитьВсе_Click(object sender, RoutedEventArgs e)
        {
            ((ListCollectionView)gcDBObjects.ItemsSource).Cast<ScriptWrapper>().ToList().ForEach(w => w.Save = false);
        }

        private void GcDBObjects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!File.Exists(FileComparer.GetPath()))
            {
                //не указана сравнивалка
                return;
            }
            if (gcDBObjects.SelectedItem is ScriptWrapper s)
            {
                string tempFile = Path.GetTempFileName();

                File.WriteAllText(tempFile, s.ScriptText, new UTF8Encoding(true));

                s.EditedFilePath = tempFile;

                if (!File.Exists(s.FullPath))
                {
                    Process.Start(FileComparer.GetPath(), $@"""{tempFile}"" ""{tempFile}""");
                }
                else
                {
                    Process.Start(FileComparer.GetPath(), $@"""{s.FullPath}"" ""{tempFile}""");
                }
            }
        }

        private ScriptWrapper SelectedObject => gcDBObjects.SelectedItem as ScriptWrapper;

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            DB.RevertObject(SelectedObject);
            ((ListCollectionView)gcDBObjects.ItemsSource).Remove(SelectedObject);
        }

        private void tbFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
