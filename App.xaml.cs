﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SteamItemsStatsViewer.DTOs;
using SteamItemsStatsViewer.Models;
using SteamItemsStatsViewer.ViewModels;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Windows;

namespace SteamItemsStatsViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ViewModelBase ViewModel { get; set; }

        public static string MainDataFolder;
        public static string TempDataFolder;

        public static SettingsModel Settings {  get; set; }

        public static Dictionary<string, double> ExchangeRates = new Dictionary<string,double>();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ViewModel = new HomeViewModel();

            CreateAppDataFolders();
            GetExchangeRates();
            LoadExchangeRates();
            LoadSettings();

            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel(ViewModel);
            mainWindow.Show();
        }

        private void CreateAppDataFolders()
        {
            MainDataFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KUBIXQAZ\\SteamItemsStatsViewer\\";
            TempDataFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\KUBIXQAZ\\SteamItemsStatsViewer\\Temp";

            if(!Directory.Exists(MainDataFolder)) Directory.CreateDirectory(MainDataFolder);
            if(!Directory.Exists(TempDataFolder)) Directory.CreateDirectory(TempDataFolder);
        }

        private void GetExchangeRates()
        {
            string fileName = "ExchangeRatesFile.json";
            string filePath = $"{TempDataFolder}\\{fileName}";

            if (File.Exists(filePath))
            {
                string file = File.ReadAllText(filePath);
                ExchangeRatesModel exchangeRates = JsonConvert.DeserializeObject<ExchangeRatesModel>(file);
                if (DateTime.Now.Date == exchangeRates.Date.Date) return;
            }

            HttpClient httpClient = new HttpClient();

            string data = httpClient.GetStringAsync("https://api.exchangerate-api.com/v4/latest/usd").GetAwaiter().GetResult();

            File.WriteAllText(filePath, data);
        }

        private void LoadExchangeRates()
        {
            string fileName = "ExchangeRatesFile.json";
            string filePath = $"{TempDataFolder}\\{fileName}";

            if (File.Exists(filePath))
            {
                string file = File.ReadAllText(filePath);
                ExchangeRatesModel exchangeRates = JsonConvert.DeserializeObject<ExchangeRatesModel>(file);
                if (exchangeRates == null) throw new Exception("exchangeRates is null");
                ExchangeRates = exchangeRates.Rates;
            }
            else
            {
                throw new Exception("ExchangeRatesFile.json file does not exist");
            }
        }

        private void LoadSettings()
        {
            string fileName = "Settings.json";
            string filePath = $"{MainDataFolder}\\{fileName}";

            if (File.Exists(filePath))
            {
                string file = File.ReadAllText(filePath);
                if (file == null) throw new Exception("Settings.json file is empty");
                Settings = JsonConvert.DeserializeObject<SettingsModel>(file);
            } else
            {
                SettingsModel settings = new SettingsModel("USD");
                settings.SaveSettings();
            }
        }
    }
}
