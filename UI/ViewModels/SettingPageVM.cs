﻿using Core.Librarys;
using Core.Models.Config;
using Core.Models.Config.Link;
using Core.Servicers.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UI.Controls;
using UI.Models;

namespace UI.ViewModels
{
    public class SettingPageVM : SettingPageModel
    {
        private ConfigModel config;
        private readonly IAppConfig appConfig;
        private readonly MainViewModel mainVM;
        public Command OpenURL { get; set; }
        public Command CheckUpdate { get; set; }

        public SettingPageVM(IAppConfig appConfig, MainViewModel mainVM)
        {
            this.appConfig = appConfig;
            this.mainVM = mainVM;

            OpenURL = new Command(new Action<object>(OnOpenURL));
            CheckUpdate = new Command(new Action<object>(OnCheckUpdate));

            Init();
        }

        private void OnCheckUpdate(object obj)
        {
            try
            {
                CheckUpdateBtnVisibility = System.Windows.Visibility.Collapsed;
                string updaterExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Updater.exe");
                string updaterCacheExePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Update",
                    "Updater.exe");
                string updateDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Update");
                if (!Directory.Exists(updateDirPath))
                {
                    Directory.CreateDirectory(updateDirPath);
                }

                if (!File.Exists(updaterExePath))
                {
                    mainVM.Toast("升级程序似乎已被删除，请手动前往发布页查看新版本", Controls.Base.IconTypes.None);
                    return;
                }
                File.Copy(updaterExePath, updaterCacheExePath, true);

                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Newtonsoft.Json.dll"), Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Update",
                    "Newtonsoft.Json.dll"), true);

                ProcessHelper.Run(updaterCacheExePath, new string[] { Version });
            }
            catch (Exception ex)
            {
                CheckUpdateBtnVisibility = System.Windows.Visibility.Visible;

                Logger.Error(ex.Message);
                mainVM.Toast("无法正确启动检查更新程序", Controls.Base.IconTypes.None);
            }
        }

        private void OnOpenURL(object obj)
        {
            Process.Start(new ProcessStartInfo(obj.ToString()));
        }

        private void Init()
        {
            config = appConfig.GetConfig();

            Data = config.General;

            TabbarData = new System.Collections.ObjectModel.ObservableCollection<string>()
            {
                "常规","关联","行为","关于"
            };

            PropertyChanged += SettingPageVM_PropertyChanged;

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }



        private void SettingPageVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Data))
            {
                if (TabbarSelectedIndex == 0)
                {
                    config.General = Data as GeneralModel;
                }
                else if (TabbarSelectedIndex == 1)
                {
                    if (Data != null)
                    {
                        var newData = new List<LinkModel>();
                        foreach (var item in Data as IEnumerable<object>)
                        {
                            newData.Add(item as LinkModel);
                            Debug.WriteLine(item.ToString());
                        }
                        config.Links = newData;
                    }

                }
                else if (TabbarSelectedIndex == 2)
                {
                    config.Behavior = Data as BehaviorModel;
                }

                appConfig.Save();
            }

            if (e.PropertyName == nameof(TabbarSelectedIndex))
            {
                if (TabbarSelectedIndex == 0)
                {
                    //  常规
                    Data = config.General;
                }
                else if (TabbarSelectedIndex == 1)
                {
                    //  关联
                    Data = config.Links;
                }
                else if (TabbarSelectedIndex == 2)
                {
                    //  行为
                    Data = config.Behavior;
                }
            }
        }
    }
}
