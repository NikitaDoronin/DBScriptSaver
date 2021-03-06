﻿using DBScriptSaver.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DBScriptSaver.ViewModels
{
    public class DBScriptViewModel: IDataErrorInfo
    {
        public ObservableCollection<Project> Projects { get; }

        string comparer;
        //Путь к инструменту развёртывания
        public string Comparer
        {
            get
            {
                return comparer;
            }
            set
            {
                comparer = value;
                FileComparer.SetPath(comparer);
                SaveAppSettings();
            }
        }

        public ListCollectionView EditProjects
        {
            get
            {
                return new ListCollectionView(Projects);
            }
        }

        string IDataErrorInfo.Error => throw new NotImplementedException();

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "Comparer":
                        if (!File.Exists(Comparer))
                        {
                            error = "Не найден файл инструмента сравнения!";
                        }
                        break;
                }
                return error;
            }
        }

        const string ProjectSettingsFileName = @"Settings.cfg";
        const string AppSettingsFileName = @"AppSettings.cfg";

        private string Settings = string.Empty;
        private string AppSettings = string.Empty;

        public DBScriptViewModel()
        {
            string SettingsDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}{Path.DirectorySeparatorChar}DBScriptHelper";
            
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
            }

            AppSettings = Path.Combine(SettingsDirectory, AppSettingsFileName);

            string AppSettingsData = "";

            using (FileStream fs = File.Open(AppSettings, FileMode.OpenOrCreate))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);
                AppSettingsData = Encoding.UTF8.GetString(bytes);
            }

            if (!string.IsNullOrEmpty(AppSettingsData))
            {
                Dictionary<string, string> AppSet = JsonConvert.DeserializeObject<Dictionary<string, string>>(AppSettingsData);
                if (AppSet.ContainsKey("Comparer"))
                {
                    Comparer = AppSet["Comparer"];
                }
            }

            Settings = Path.Combine(SettingsDirectory, ProjectSettingsFileName);

            using (FileStream fs = File.Open(Settings, FileMode.OpenOrCreate))
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, (int)fs.Length);

                string ProjectsData = Encoding.UTF8.GetString(bytes);

                if (!string.IsNullOrEmpty(ProjectsData))
                {
                    Projects = new ObservableCollection<Project>(JsonConvert.DeserializeObject<List<Project>>(ProjectsData));
                    Projects.ToList().ForEach(i => (i as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged);
                    Projects.ToList().ForEach(p => p.vm = this);
                    Projects.ToList().SelectMany(i => i.DataBases).ToList().ForEach(i => (i as INotifyPropertyChanged).PropertyChanged += Item_PropertyChanged);
                }
                else
                {
                    Projects = new ObservableCollection<Project>(new List<Project>());
                }
            }

            Projects.CollectionChanged += ContentCollectionChanged;
        }

        private void ContentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    item.PropertyChanged += Item_PropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (Project proj in e.NewItems)
                {
                    proj.vm = this;
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == @"DBPassword")
            {
                return;
            }
            SaveProjects();
        }
        public void SaveProjects()
        {
            File.WriteAllText(Settings, JsonConvert.SerializeObject(Projects));
            TaskbarIconHelper.UpdateContextMenu();
        }
        public void SaveAppSettings()
        {
            Dictionary<string, string> AppSet = new Dictionary<string, string>();

            AppSet.Add("Comparer", Comparer);

            File.WriteAllText(AppSettings, JsonConvert.SerializeObject(AppSet));
        }

        public void AddProject()
        {
            Projects.Add(new Project(this) { Name = Resources.НовыйПроект });
        }
    }
}
