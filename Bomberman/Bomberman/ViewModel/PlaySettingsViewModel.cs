namespace Bomberman.ViewModel
{
    public class PlaySettingsViewModel : ViewModelBase
    {
        #region Fields
        private bool _is2Player;
        private bool _is3Player;
        private bool _isMap1;
        private bool _isMap2;
        private bool _isMap3;
        private bool _isFT3;
        private bool _isFT5;
        #endregion

        #region Properties
        public bool Is2Player
        {
            get { return _is2Player; }
            set { _is2Player = value; }
        }
        public bool Is3Player
        {
            get { return _is3Player; }
            set { _is3Player = value; }
        }

        public bool IsMap1
        {
            get { return _isMap1; }
            set { _isMap1 = value; }
        }
        public bool IsMap2
        {
            get { return _isMap2; }
            set { _isMap2 = value; }
        }
        public bool IsMap3
        {
            get { return _isMap3; }
            set { _isMap3 = value; }
        }
        public bool IsFT3
        {
            get { return _isFT3; }
            set { _isFT3 = value; }
        }
        public bool IsFT5
        {
            get { return _isFT5; }
            set { _isFT5 = value; }
        }
        public int PlayerNumber
        {
            get
            {
                if (_is2Player) return 2;
                return 3;
            }
        }
        public int MapId
        {
            get
            {
                if (_isMap1) return 1;
                else if (_isMap2) return 2;
                return 3;
            }
        }
        public int MatchLength
        {
            get
            {
                if (_isFT3) return 3;
                return 5;
            }
        }
        #endregion

        #region Commands
        public DelegateCommand StartGameCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        #endregion

        #region Events
        public event EventHandler? StartGame;
        public event EventHandler? Cancel;
        #endregion

        #region Constructor
        public PlaySettingsViewModel()
        {
            StartGameCommand = new DelegateCommand(param => OnStartGame());
            CancelCommand = new DelegateCommand(param => OnCancel());
            _is2Player = true;
            _is3Player = false;
            _isMap1 = true;
            _isMap2 = false;
            _isMap3 = false;
            _isFT3 = true;
            _isFT5 = false;
        }
        #endregion

        #region Private methods
        private void OnStartGame()
        {
            StartGame!.Invoke(this, EventArgs.Empty);
        }
        private void OnCancel()
        {
            Cancel!.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
