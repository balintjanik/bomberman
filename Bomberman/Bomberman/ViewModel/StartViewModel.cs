using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.ViewModel
{
    public class StartViewModel : ViewModelBase
    {
        #region Commands
        public DelegateCommand StartGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand SettingsCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        #endregion

        #region Events
        public event EventHandler? StartGame;
        public event EventHandler? LoadGame;
        public event EventHandler? Settings;
        public event EventHandler? Exit;
        #endregion

        #region Constructor
        public StartViewModel() {
            StartGameCommand = new DelegateCommand(param => OnStartGame());
            LoadGameCommand = new DelegateCommand(param => OnLoadGame());
            SettingsCommand = new DelegateCommand(param => OnSettings());
            ExitCommand = new DelegateCommand(param => OnExit());
        }
        #endregion

        #region Private methods
        private void OnStartGame()
        {
            StartGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoadGame()
        {
            LoadGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnSettings()
        {
            Settings?.Invoke(this, EventArgs.Empty);
        }

        private void OnExit()
        {
            Exit?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
