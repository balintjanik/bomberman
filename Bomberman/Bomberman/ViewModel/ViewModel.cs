using Bomberman.Model;
using Bomberman.Persistence;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Bomberman.ViewModel
{
    public class ViewModel : ViewModelBase
    {
        #region Fields
        private GameModel _model;
        private Dispatcher _dispatcher;
        #endregion

        #region Properties
        public ColumnDefinitionCollection GameTableColumns() { return null!; }
        public RowDefinitionCollection GameTableRows() { return null!; }
        public ObservableCollection<Field> Fields { get; set; } = null!;
        public String GameTime { get { return TimeSpan.FromSeconds(_model.GameTime / 10).ToString("g"); } }
        public String ShrinkTimeLeft { get { return TimeSpan.FromSeconds(_model.ShrinkTimeLeft / 10).ToString("g"); } }
        public int MapSize { get; set; }
        public int MapDisplaySize { get { return Convert.ToInt32(SystemParameters.PrimaryScreenHeight); } }
        public int Player1Wins { get { return _model.GetPlayerWins(0); } }
        public int Player2Wins { get { return _model.GetPlayerWins(1); } }
        public int Player3Wins { get { return _model.GetPlayerWins(2); } }
        public Visibility Player3Stat { get { return (_model.PlayerNumber == 3 ? Visibility.Visible : Visibility.Hidden); } }
        public ObservableCollection<MessageType> Messages { get; set; }
        #endregion

        #region Commands
        public DelegateCommand SaveGameCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }
        #endregion

        #region Event handlers
        public event EventHandler? SaveGame;
        public event EventHandler? ExitGame;
        #endregion

        #region Constructor
        public ViewModel(GameModel model, Dispatcher dispatcher)
        {
            _model = model;
            GenerateMap();
            _dispatcher = dispatcher;

            SaveGameCommand = new DelegateCommand(param => OnSaveGame());
            ExitGameCommand = new DelegateCommand(param => OnExitGame());

            _model.GameAdvanced += new EventHandler(Model_GameAdvanced);
            _model.MapChanged += new EventHandler(Model_MapChanged);
            _model.GameOver += new EventHandler(Model_GameOver);
            _model.MatchOver += new EventHandler(Model_MatchOver);
            _model.MessageSent += new EventHandler<MessageEventArgs>(Model_MessageSent);
            _model.ClearMessages += new EventHandler(Model_ClearMessages);

            Messages = new ObservableCollection<MessageType>();
        }
        #endregion

        #region Private methods
        private Uri SetFieldType(FieldType fieldType)
        {
            Uri path;
            switch (fieldType)
            {
                case FieldType.WALL:
                    path = new Uri("/Textures/wall.png", UriKind.Relative);
                    break;
                case FieldType.ROAD:
                    path = new Uri("/Textures/grass.png", UriKind.Relative);
                    break;
                case FieldType.BOX:
                    path = new Uri("/Textures/box.png", UriKind.Relative);
                    break;
                case FieldType.BOMB:
                    path = new Uri("/Textures/bomb.gif", UriKind.Relative);
                    break;
                case FieldType.EXPLOSION:
                    path = new Uri("/Textures/explosion.gif", UriKind.Relative);
                    break;
                case FieldType.ENEMY:
                    path = new Uri("/Textures/enemy_l.png", UriKind.Relative);
                    break;
                case FieldType.PLAYER1:
                    path = new Uri("/Textures/player1.gif", UriKind.Relative);
                    break;
                case FieldType.PLAYER2:
                    path = new Uri("/Textures/player2.gif", UriKind.Relative);
                    break;
                case FieldType.PLAYER3:
                    path = new Uri("/Textures/player3.gif", UriKind.Relative);
                    break;
                case FieldType.POWERUP:
                    path = new Uri("/Textures/powerup.gif", UriKind.Relative);
                    break;
                case FieldType.BOMB1:
                    path = new Uri("/Textures/player1bomb.gif", UriKind.Relative);
                    break;
                case FieldType.BOMB2:
                    path = new Uri("/Textures/player2bomb.gif", UriKind.Relative);
                    break;
                case FieldType.BOMB3:
                    path = new Uri("/Textures/player3bomb.gif", UriKind.Relative);
                    break;
                default:
                    path = new Uri("/Textures/wall.png", UriKind.Relative);
                    break;
            }
            return path;
        }
        private void GenerateMap()
        {
            MapSize = _model.Map.Size;
            Fields = new ObservableCollection<Field>();
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    FieldType fieldType = _model.Map.GetField(new int[] { i, j });
                    Uri path = SetFieldType(fieldType);
                    ImageSource imageSource = new BitmapImage(path);

                    Fields.Add(new Field
                    {
                        ImageSource = imageSource,
                        Path = path,
                        X = i,
                        Y = j,
                    });
                }
            }
        }

        private void RefreshMap()
        {
            foreach (Field field in Fields)
            {
                FieldType fieldType = _model.Map.GetField(new int[] { field.X, field.Y });
                Uri path = SetFieldType(fieldType);
                ImageSource imageSource = new BitmapImage(path);
                if (!Equals(path, field.Path))
                {
                    field.ImageSource = imageSource;
                    field.Path = path;
                }
            }
        }

        private void OnSaveGame()
        {
            SaveGame?.Invoke(this, EventArgs.Empty);
        }
        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        private void Model_MapChanged(object? sender, EventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                RefreshMap();
            });
        }
        private void Model_ClearMessages(object? sender, EventArgs e)
        {
            Messages = new ObservableCollection<MessageType>();
            OnPropertyChanged(nameof(Messages));
        }
        private void Model_GameOver(object sender, EventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(Player1Wins));
                OnPropertyChanged(nameof(Player2Wins));
                OnPropertyChanged(nameof(Player3Wins));
            });

        }
        private void Model_MatchOver(object sender, EventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                OnExitGame();
            });
        }
        private void Model_GameAdvanced(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(GameTime));
            OnPropertyChanged(nameof(ShrinkTimeLeft));
        }

        private void Model_MessageSent(object? sender, MessageEventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                MessageType msg = new MessageType(e.Sender, e.Message);
                Messages.Add(msg);
                OnPropertyChanged(nameof(Messages));
            });
        }
        #endregion

        #region Public methods
        public void DestroyViewModel()
        {
            _model.GameAdvanced -= Model_GameAdvanced;
            _model.MapChanged -= Model_MapChanged;
        }
        #endregion
    }
}
