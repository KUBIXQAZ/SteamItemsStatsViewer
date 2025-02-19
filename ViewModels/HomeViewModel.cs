﻿using Newtonsoft.Json;
using SteamItemsStatsViewer.Commands;
using SteamItemsStatsViewer.Models;
using SteamItemsStatsViewer.Stores;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;

namespace SteamItemsStatsViewer.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        public NavigationStore _navigationStore;

        private ObservableCollection<SteamItemNavigationItemModel> _navigationItems = new ObservableCollection<SteamItemNavigationItemModel>();
        public ObservableCollection<SteamItemNavigationItemModel> NavigationItems
        {
            get => _navigationItems;
            set
            {
                _navigationItems = value;
                OnPropertyChanged(nameof(NavigationItems));
            }
        }

        public RelayCommand RefreshListCommand { get; set; }

        public HomeViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            RefreshListCommand = new RelayCommand(execute => LoadSteamItemsNavigationItems());

            LoadSteamItemsNavigationItems();
        }

        public async void LoadSteamItemsNavigationItems()
        {
            NavigationItems.Clear();

            List<ItemDataModel> itemsData = new List<ItemDataModel>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://192.168.31.71:5000/api/ItemsData/");

                var answer = await client.GetAsync("GetItemsData");

                if(answer.IsSuccessStatusCode)
                {
                    string content = await answer.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(content)) return;

                    itemsData = JsonConvert.DeserializeObject<List<ItemDataModel>>(content)!;
                }
            }

            try
            {
                foreach (ItemDataModel item in itemsData)
                {
                    Random rng = new Random();

                    item.IconPath = $"{App.IconFolder}\\{rng.Next(int.MaxValue)}.png";

                    File.WriteAllBytes(item.IconPath, item.Icon);
                }
            }
            catch (Exception) { }

            string favItemsPath = App.MainDataFolder + "/FavouriteItems.json";

            List<string> favItems = new List<string>();

            if (Path.Exists(favItemsPath))
            {
                string f = File.ReadAllText(favItemsPath);
                favItems = JsonConvert.DeserializeObject<List<string>>(f);
                
                ItemDataModel[]? matchedItems = itemsData.Where(x => favItems.Contains(x.Name)).ToArray();
                ItemDataModel[]? otherItems = itemsData.Where(x => !favItems.Contains(x.Name)).ToArray();

                itemsData = matchedItems.Concat(otherItems).ToList();
            }

            foreach (ItemDataModel itemData in itemsData)
            {
                bool isFav = favItems.Contains(itemData.Name);

                SteamItemNavigationItemModel steamItemNavigationItem = new SteamItemNavigationItemModel(itemData, isFav, this);
                NavigationItems.Add(steamItemNavigationItem);
            }
        }
    }
}