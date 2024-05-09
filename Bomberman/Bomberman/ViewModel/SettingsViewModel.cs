using Bomberman.Persistence;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Bomberman.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Fields
        private Key[] _keys;
        private bool _canChange;
        private int _row;
        private int _col;
        #endregion

        #region Properties
        public ObservableCollection<BindButton> BindButtons { get; set; }
        #endregion

        #region Commands
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }
        #endregion

        #region Events
        public event EventHandler? Save;
        public event EventHandler? Cancel;
        #endregion

        #region Constructor
        public SettingsViewModel()
        {
            _keys = SettingsAccess.LoadSettings();
            BindButtons = new ObservableCollection<BindButton>();

            _canChange = false;

            SaveCommand = new DelegateCommand(param => OnSave());
            CancelCommand = new DelegateCommand(param => OnCancel());

            for (int i = 1; i <= 5; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    var bindButton = new BindButton {
                        Row = j,
                        Col = i,
                        Content = _keys[(j - 1) * 5 + (i - 1)].ToString(),
                        Command = new DelegateCommand(param => OnBindClick(param)),
                        Parameter = j + "," + i
                    };
                    BindButtons.Add(bindButton);
                }
            }
        }
        #endregion

        #region Public methods
        public void OnKeyDown(object sender, KeyEventArgs e) {
            if (_canChange)
            {
                _keys[(_col - 1) * 5 + (_row - 1)] = e.Key;
                _canChange = false;
                BindButtons[(_row - 1) * 3 + (_col - 1)].Content = _keys[(_col - 1) * 5 + (_row - 1)].ToString();
            }
        }
        #endregion

        #region Private methods
        private void OnSave()
        {
            SettingsAccess.SaveSettings(_keys);
            Save?.Invoke(this, EventArgs.Empty);
        }

        private async void OnBindClick(object? data)
        {
            string[] strings = (data as string).Split(',');
            _col = Convert.ToInt32(strings[0]);
            _row = Convert.ToInt32(strings[1]);
            _canChange = true;
        }

        private void OnCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
