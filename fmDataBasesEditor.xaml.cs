﻿using DBScriptSaver.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ude;

namespace DBScriptSaver
{
    /// <summary>
    /// Логика взаимодействия для fmDataBasesEditor.xaml
    /// </summary>
    public partial class fmDataBasesEditor : Window
    {
        private Project project => DataContext as Project;
        public fmDataBasesEditor(Project proj)
        {
            InitializeComponent();

            DataContext = proj;
        }

        private ProjectDataBase SelectedBase => gcDataBases.SelectedItem as ProjectDataBase;

        private void EditDBObjects_Click(object sender, RoutedEventArgs e)
        {
            var DB = SelectedBase;

            if (DB == null)
            {
                return;
            }

            try
            {
                var fmEditor = new DBObjectsFiltering(DB) { Owner = this };
                fmEditor.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"Фильтр объектов");
            }
        }

        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            var DB = SelectedBase;
            DB.UpdateScripts();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            project.DataBases.Add(new ProjectDataBase(project));
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            project.vm.SaveProjects();
            DialogResult = true;
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            var DB = SelectedBase;
            if (DB == null)
            {
                MessageBox.Show("Не выбрана база данных");
                return;
            }
            project.DataBases.Remove(SelectedBase);
        }
    }
}
