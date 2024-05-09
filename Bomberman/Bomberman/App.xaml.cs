using Bomberman.Model;
using Bomberman.ViewModel;
using Bomberman.Persistence;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Bomberman.View;
using Microsoft.Win32;

namespace Bomberman
{
    public partial class App : Application
    {
        #region Fields

        private GameModel _model = null!;
        private ViewModel.ViewModel _viewModel = null!;
        private StartViewModel _startViewModel = null!;
        private PlaySettingsViewModel _playSettingsViewModel = null!;
        private SettingsViewModel _settingsViewModel = null!;
        private SavedGamesViewModel _savedGamesViewModel = null!;
        private MainWindow _gameView = null!;
        private StartWindow _startView = null!;
        private PlaySettingsWindow _playSettingsView = null!;
        private SavedGamesWindow _savedGamesView = null!;
        private SettingsWindow _settingsView = null!;
        private Key[] keys;
        #endregion

        #region Init
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object? sender, StartupEventArgs e)
        {
            _model = new GameModel();

            keys = SettingsAccess.LoadSettings();

            

            _startViewModel = new StartViewModel();
            _startViewModel.StartGame += new EventHandler(StartView_PlaySettings);
            _startViewModel.LoadGame += new EventHandler(StartView_LoadGame);
            _startViewModel.Settings += new EventHandler(StartView_Settings);
            _startViewModel.Exit += new EventHandler(StartView_Exit);

            _savedGamesView = new SavedGamesWindow();

            _model.LoadSaves();

            _savedGamesViewModel = new SavedGamesViewModel(_model);
            _savedGamesViewModel.Cancel += new EventHandler(SavedGamesView_Cancel);
            _savedGamesViewModel.Load += SavedGamesView_Load;
            _savedGamesViewModel.Delete += SavedGamesView_Delete;


            _startView = new StartWindow();
            _startView.DataContext = _startViewModel;
            _startView.Closing += View_Closing;
            _startView.Show();
        }
        private void Keydown(object sender, KeyEventArgs e)
        {
            Key k = e.Key;
            foreach (var player in _model.KeyBinds)
            {
                foreach (var bind in player.Value)
                {
                    if (bind.Key == 4 && k == bind.Value)
                    {
                        if (_model.Map.GetPlayerPosition(player.Key) == null)
                        {
                            return;
                        }
                        _model.PlaceBomb(_model.Map.GetPlayerPosition(player.Key), player.Key); //TODO null reference problem
                    }
                    else //first 4 are the direction
                    {
                        if (k == bind.Value)
                        {
                            _model.Step(player.Key, bind.Key);
                            break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Save and Load
        private void ViewModel_SaveGameView(object? sender, EventArgs e)
        {
            _model.LoadSaves();

            _savedGamesView = new SavedGamesWindow();
            _savedGamesView.DataContext = _savedGamesViewModel;            
            _savedGamesViewModel.isSave = true;

            _savedGamesViewModel.Save -= ViewModel_SaveGame;
            _savedGamesViewModel.Save += ViewModel_SaveGame;
            _savedGamesViewModel.Load -= SavedGamesView_Load;
            _savedGamesViewModel.Load += SavedGamesView_Load;
            
            _model.StopGame();
            _savedGamesView.lb_Saves.SelectedIndex = -1;
            _savedGamesView.Show();
            _gameView.Hide();
        }
        private void ViewModel_SaveGame(object? sender, EventArgs e)
        {
            _model.SaveGameState(_model.MapId, _savedGamesViewModel.CurrentSave);
            _model.DestroyPowerUps();
            MessageBox.Show("Game saved successfully!", "Bomberman", MessageBoxButton.OK);

            _startView.Show();
            _savedGamesView.Close();
            _viewModel.DestroyViewModel();
            _savedGamesViewModel.Save -= ViewModel_SaveGame;

            _model = new GameModel();
            _model.LoadSaves();
            _savedGamesViewModel = new SavedGamesViewModel(_model);
            _savedGamesViewModel.Cancel += new EventHandler(SavedGamesView_Cancel);
            _savedGamesViewModel.Load -= SavedGamesView_Load;
            _savedGamesViewModel.Load += SavedGamesView_Load;
            _savedGamesViewModel.Delete += SavedGamesView_Delete;
        }
        private void SavedGamesView_Load(object? sender, EventArgs e)
        {
            _model.LoadGameState(_savedGamesViewModel.SelectedId);

            _savedGamesView.lb_Saves.SelectedIndex = -1;
            _savedGamesView.Hide();

            _model.InitKeys(keys);

            _viewModel = new ViewModel.ViewModel(_model, Application.Current.Dispatcher);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.SaveGame -= new EventHandler(ViewModel_SaveGameView);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGameView);


            _gameView = new MainWindow();
            _gameView.DataContext = _viewModel;
            _gameView.Closing += View_Closing;
            _gameView.KeyDown += new KeyEventHandler(Keydown);

            _model.StartGame(true);
            _gameView.Show();
        }
        private void SavedGamesView_Delete(object? sender, EventArgs e)
        {
            _model.DeleteGameState(_savedGamesViewModel.SelectedId);
            MessageBox.Show("Game deleted successfully!", "Bomberman", MessageBoxButton.OK);

        }
        private void SavedGamesView_Cancel(object? sender, EventArgs e)
        {
            if (_savedGamesViewModel.isSave)
            {
                _gameView.Show();
                _model.StartGame();
            }
            else
            {
                _startView.Show();
            }
            _savedGamesView.Hide();
        }
        private void StartView_LoadGame(object? sender, EventArgs e)
        {
            _model.LoadSaves();

            _savedGamesView = new SavedGamesWindow();
            _savedGamesViewModel.isSave = false;
            _savedGamesView.DataContext = _savedGamesViewModel;

            _savedGamesView.Show();

            _startView.Hide();
        }

        #endregion

        #region Settings
        private void PlaySettingsView_Start(object? sender, EventArgs e)
        {
            _playSettingsView.Hide();
            int playerNumber = _playSettingsViewModel.PlayerNumber;
            int mapId = _playSettingsViewModel.MapId;
            int matchLength = _playSettingsViewModel.MatchLength;

            _model.Init(mapId, playerNumber, matchLength);
            _model.InitKeys(keys);

            _viewModel = new ViewModel.ViewModel(_model, Application.Current.Dispatcher);
            _viewModel.ExitGame += new EventHandler(ViewModel_ExitGame);
            _viewModel.SaveGame -= new EventHandler(ViewModel_SaveGameView);
            _viewModel.SaveGame += new EventHandler(ViewModel_SaveGameView);


            _gameView = new MainWindow();
            _gameView.DataContext = _viewModel;
            _gameView.Closing += View_Closing;
            _gameView.KeyDown += new KeyEventHandler(Keydown);

            _model.StartGame();
            _gameView.Show();
        }
        private void PlaySettingsView_Cancel(object? sender, EventArgs e) {
            _startView.Show();
            _playSettingsView.Close();
        }
        private void SettingsView_Save(object? sender, EventArgs e)
        {
            _settingsView.KeyDown -= _settingsViewModel.OnKeyDown;
            _startView.Show();
            keys = SettingsAccess.LoadSettings();
            _settingsView.Close();
        }
        private void SettingsView_Cancel(object? sender, EventArgs e)
        {
            _settingsView.KeyDown -= _settingsViewModel.OnKeyDown;
            _startView.Show();
            _settingsView.Close();
        }
        private void StartView_PlaySettings(object? sender, EventArgs e) {
            _playSettingsViewModel = new PlaySettingsViewModel();
            _playSettingsViewModel.StartGame += new EventHandler(PlaySettingsView_Start);
            _playSettingsViewModel.Cancel += new EventHandler(PlaySettingsView_Cancel);

            _playSettingsView = new PlaySettingsWindow();
            _playSettingsView.DataContext = _playSettingsViewModel;
            _playSettingsView.Show();

            _startView.Hide();
        }
        private void StartView_Settings(object? sender, EventArgs e)
        {
            _settingsViewModel = new SettingsViewModel();
            _settingsViewModel.Save += new EventHandler(SettingsView_Save);
            _settingsViewModel.Cancel += new EventHandler(SettingsView_Cancel);

            _settingsView = new SettingsWindow();
            _settingsView.DataContext = _settingsViewModel;
            _settingsView.PreviewKeyDown += new KeyEventHandler(_settingsViewModel.OnKeyDown);
            _settingsView.Show();

            _startView.Hide();
        }
        #endregion

        #region Closing
        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            _startView.Show();
            _gameView.Close();
            _model.StopGame();
            _model.DestroyPowerUps();
            _viewModel.DestroyViewModel();
        }
        private void View_Closing(object? sender, CancelEventArgs e)
        {
            // if (MessageBox.Show("Are you sure you want to exit?", "Bomberman", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            // {
            //     e.Cancel = true;
            // }
        }
        private void StartView_Exit(object? sender, EventArgs e) {
            Application.Current.Shutdown();
        }
        #endregion
        





      
    }
}
