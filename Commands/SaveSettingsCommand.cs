﻿using SteamItemsStatsViewer.ViewModels;

namespace SteamItemsStatsViewer.Commands
{
    public class SaveSettingsCommand : CommandBase
    {
        private SettingsViewModel _viewModel;

        public SaveSettingsCommand(SettingsViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public override void Execute(object? parameter)
        {
            App.Settings.Currency = _viewModel.SelectedCurrency;

            App.Settings.SaveSettings();
        }
    }
}
