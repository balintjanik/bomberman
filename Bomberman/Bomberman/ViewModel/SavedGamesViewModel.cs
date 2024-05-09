using Bomberman.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Bomberman.ViewModel
{
    public class SavedGamesViewModel : ViewModelBase
    {
        #region Fields
        private GameModel _model;
        #endregion

        #region Properties
        public ObservableCollection<Saves> Saves { get; set; }
        public string CurrentSave { get; set; }   
        public string SelectedId { get; set; }
        public bool isSave { get; set; } = false;

        #endregion

        #region Commands
        public DelegateCommand CancelCommand { get; private set; }
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand LoadGameCommand { get; private set; }
        public DelegateCommand DeleteGameCommand { get; private set; }

        #endregion

        #region Events

        public event EventHandler? Cancel;
        public event EventHandler? Save;
        public event EventHandler? Load;
        public event EventHandler? Delete;

        #endregion

        #region Constructor
        public SavedGamesViewModel(GameModel model)
        {
            _model = model;

            CancelCommand = new DelegateCommand(param => OnCancel());
            SaveGameCommand = new DelegateCommand(param => OnSave());
            LoadGameCommand = new DelegateCommand(param => OnLoad());
            DeleteGameCommand = new DelegateCommand(param => OnDelete());

            CurrentSave = "Enter save name here";

            //load data
            LoadSaves();

            OnPropertyChanged(nameof(Saves));
        }

        #endregion

        #region Private methods
        private void LoadSaves()
        {
            CurrentSave = "Enter save name here";
            Saves = new ObservableCollection<Saves>();
            if (_model.Saves != null)
            {
                for (int i = 0; i < _model.Saves.Count; i++)
                {
                    Saves.Add(new Saves
                    {
                        Id = _model.Saves[i].Id,
                        Date = _model.Saves[i].Timestamp,
                        Map = _model.Saves[i].MapId,
                        Name = _model.Saves[i].Name,
                        Players = _model.Saves[i].Players.Count().ToString(),
                        IsSelected = false
                    }) ;
                }
                OnPropertyChanged(nameof(Saves));
                
            }
        }
        private void OnCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
            LoadSaves();

        }
        private void OnSave()
        {
            Save?.Invoke(this, EventArgs.Empty);
            LoadSaves();
        }
        private void OnLoad()
        {
            SelectedId = Saves.Where(s => s.IsSelected).FirstOrDefault()!.Id;
            Load?.Invoke(this, EventArgs.Empty);
        }
        private void OnDelete()
        {
            SelectedId = Saves.Where(s => s.IsSelected).FirstOrDefault()!.Id;
            Delete?.Invoke(this, EventArgs.Empty);
            LoadSaves();
        }

        #endregion
    }
}
