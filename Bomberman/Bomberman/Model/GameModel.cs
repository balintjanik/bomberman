using System.Threading;
using Bomberman.Persistence;
using System.Windows.Input;
using System;
using System.Numerics;
using System.Windows;

namespace Bomberman.Model
{
    public class GameModel
    {
        #region Fields

        private Random _rnd;
        private DataAccess _dataAccess;
        private Timer _timer;
        private GameMap _map;
        private GameMap _starterMap;
        private Player[] _players;
        private List<int> _order;
        private List<Enemy> _enemies;
        private List<Bomb> _bombs;
        private int _matchLength;
        private int _numberOfPlayers;
        private int _gameTime;
        // _originalShrinkTime - (_map.Size / 2) * _shrinkDecrement
        // must be greater than 0 for the "storm" to fully close
        private int _originalShrinkTime;
        private int _shrinkTime;
        private readonly int _shrinkDecrement;
        private int _shrinkRound;
        private int _enemySpeed;
        private int _mapId;
        private int _gameOverTime;
        private bool _isGameOver;
        private bool _matchOver;

        public Dictionary<int, Dictionary<int, Key>> KeyBinds; //int:player id, int:direction/placeBomb, Key:key

        #endregion

        #region Events
        public event EventHandler GameAdvanced;
        public event EventHandler GameOver;
        public event EventHandler MatchOver;
        public event EventHandler MapChanged;
        public event EventHandler ClearMessages;
        public event EventHandler<MessageEventArgs> MessageSent;

        #endregion

        #region Properties
        public List<Save> Saves { get; set; }
        public GameMap Map { get { return _map; } }
        public Timer Timer { get { return _timer; } }
        public int GameTime { get { return _gameTime; } }
        public int ShrinkTimeLeft { get { return _shrinkTime - _gameTime; } }
        public int MatchLength {  get { return _matchLength; } }
        public int PlayerNumber { get { return _numberOfPlayers; } }
        public int MapId { get { return _mapId; } }
        #endregion

        #region Constructor
        public GameModel() {
            _rnd = new Random();
            _dataAccess = new DataAccess();
            _enemySpeed = 4;
            _enemies = new List<Enemy>();
            _matchOver = false;
            // timer ticks at every 0.1s so time values must be 10x the desired seconds
            // e.g. value of 300 means 30s
            _shrinkDecrement = 50; // e.g. if value is 50, shrinkTime will decrement with 5 seconds
        }
        #endregion

        #region Initializations
        public void Init(int mapId = 1, int numberOfPlayers = 3, int matchLength = 3)
        {
            // map
            string path = "Files/map" + mapId.ToString() + ".txt";
            _starterMap = _dataAccess.Load(path);
            _map = CopyMap();
            _mapId = mapId;
            // players
            _numberOfPlayers = numberOfPlayers;
            _players = FindPlayers(numberOfPlayers);
            _matchLength = matchLength;
            _matchOver = false;
            _order = new List<int>();

        }
        public void StartGame(bool loaded = false)
        {
            if (!loaded)
            {
                _map = CopyMap();
                RevivePlayers();
                // enemies
                FindEnemies();
                _originalShrinkTime = (_map.Size / 2) * _shrinkDecrement;
                _shrinkTime = _originalShrinkTime;
                _shrinkRound = 0;
                _bombs = new List<Bomb>();
                _order = new List<int>();
                // other initializations
                _gameTime = 0;
                _gameOverTime = -1;
            }
            if (loaded)
            {
                _originalShrinkTime = (_map.Size / 2) * _shrinkDecrement;
            }
            _isGameOver = false;
            // initial shrinktime will be 10 seconds plus half of mapsize times _shrinkDecrement seconds:
            // this will allow the map to completely shrink and be filled with only walls no matter what size the map is

            //invoke event to refresh view
            if (ClearMessages is not null) ClearMessages.Invoke(this, EventArgs.Empty);
            if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);

            // timer
            TimerCallback timerCallback = OnTick;
            _timer = new Timer(timerCallback, null, 0, 100);
        }
        public void InitLoadedGame(Save load)
        {
            _starterMap = _dataAccess.Load("Files/" + load.MapId + ".txt");

            Player[] players = new Player[load.Players.Count()];
            for (int i = 0; i < load.Players.Count(); i++)
            {
                players[i] = new Player(this, load.Players[i].Id, load.Players[i].Wins, load.Players[i].PowerUps, load.Players[i].OldRange, load.Players[i].BombRange, load.Players[i].PlacedBombs, load.Players[i].Active, load.Players[i].Speed, load.Players[i].CanStep, load.Players[i].OldMaxBombNumber, load.Players[i].MaxBombNumber, load.Players[i].Position, load.Players[i].InstantPlacement);
                players[i].StartPosition = _starterMap.GetPlayerPosition(i);
            }
            _players = players;
            _gameTime = load.GameTime;
            _numberOfPlayers = _players.Count();
            _shrinkTime = load.ShrinkTime;
            _bombs = load.Bombs;
            _shrinkRound = load.ShrinkRound;
            _order = load.Order;
            _gameOverTime = load.GameOverTime;
            _matchLength = load.MatchLength;
            _originalShrinkTime = load.OriginalShrinkTime;
            _mapId = int.Parse(load.MapId.Split("map")[1].ToString());

            foreach (var e in load.Enemies)
            {
                Enemy enemy = new Enemy(this, e.Position, 0, _enemySpeed);
                _enemies.Add(enemy);
            }

            _bombs.ForEach(b => b.LoadModel(this));

            _players.ToList().ForEach(p => p.LoadModel(this));
            _players.ToList().ForEach(p => p.PowerUps.ToList().ForEach(pu => pu.LoadModel(this, p)));

            for (int i = _numberOfPlayers; i < 3; i++)
            {
                int[] p = _starterMap.GetPlayerPosition(i);
                _starterMap.SetField(p, FieldType.ROAD);
            }
        }
        public void StopGame()
        {
            // timer
            _timer.Dispose();
            foreach (var bomb in _bombs)
            {
                bomb.Destroy();
            }
        }
        public void DestroyPowerUps()
        {
            foreach (var player in _players)
            {
                player.DestroyPowerups();
            }
        }
        public void InitKeys(Key[] keys)
        {
            KeyBinds = new Dictionary<int, Dictionary<int, Key>>();
            for (int i = 0; i < _numberOfPlayers; i++)
            {
                KeyBinds.Add(i, new Dictionary<int, Key>());
            }
            int j = 0;
            for (int i = 0; i < _numberOfPlayers * 5; i++)
            {
                KeyBinds[j][i % 5] = keys[i];
                if ((i + 1) % 5 == 0)
                {
                    j++;
                }
            }
        }
        private GameMap CopyMap()
        {
            GameMap newmap = new GameMap(_starterMap.Size);
            for (int i = 0; i < _starterMap.Size; i++)
            {
                for (int j = 0; j < _starterMap.Size; j++)
                {
                    int[] p = new int[2] { i, j };
                    newmap.SetField(p, _starterMap.GetField(p));
                }
            }
            return newmap;
        }
        private void RevivePlayers() //to revive players when new game starts
        {
            for (int i = 0; i < _numberOfPlayers; i++)
            {
                _players[i].Revive();
                _players[i].Position = _players[i].StartPosition;
            }

        }
        private Player[] FindPlayers(int n) {
            // initialize players
            Player[] players = new Player[n];
            for (int i = 0; i < n; i++)
            {
                players[i] = new Player(this, i, 0);
                players[i].Position = _starterMap.GetPlayerPosition(i);
                players[i].StartPosition = _starterMap.GetPlayerPosition(i);
            }
            // the rest of the players are removed from the map
            for (int i = n; i < 3; i++)
            {
                int[] p = _starterMap.GetPlayerPosition(i);
                _starterMap.SetField(p, FieldType.ROAD);
            }
            return players;
        }
        private void FindEnemies()
        {
            if (_enemies.Count > 0)
            {
                foreach (var enemy in _enemies)
                {
                    enemy.Die();
                }
                _enemies.Clear();
            }

            _enemies = new List<Enemy>();
            for (int i = 0; i < Map.Size; i++)
            {
                for (int j = 0; j < Map.Size; j++)
                {
                    if (Map[i, j] == FieldType.ENEMY)
                    {
                        _enemies.Add(new Enemy(this, new int[2] { i, j }, _rnd.Next(4), _enemySpeed));
                    }
                }
            }
        }

        #endregion

        #region Step
        private int[] ValidPosition(int[] position, int direction) //returns {0,0} if invalid, returns new position if valid
        {
            // get indices and update them according to direction
            int i = position[0]; // row
            int j = position[1]; // col
            switch (direction)
            {
                case 0: // go up
                    i = i - 1;
                    break;
                case 1: // go down
                    i = i + 1;
                    break;
                case 2: // go left
                    j = j - 1;
                    break;
                case 3: // go right
                    j = j + 1;
                    break;
                default:
                    break;
            }

            // check for invalid indices
            if (!InsideBounds(i, j)) return new int[] { 0, 0 };

            // check for invalid fields
            if (_map[i, j] == FieldType.WALL || _map[i, j] == FieldType.BOX
                || _map[i, j] == FieldType.BOMB || _map[i, j] == FieldType.BOMB1 || _map[i, j] == FieldType.BOMB2 || _map[i, j] == FieldType.BOMB3) return new int[] { 0, 0 };

            // step is valid if no problems were found
            return new int[] { i, j };
        }
         
        public void Step(int playerId, int direction) {
            if (!_players.Where(p => p.Id == playerId).First().Active || !_players[playerId].CanStep || _isGameOver)
            {
                return;
            }
            // get position of player
            int[] position = _players[playerId].Position;

            int[] newPosition = ValidPosition(position, direction);
            //if invalid, return without action
            if (newPosition[0] == 0 && newPosition[1] == 0) return;
            // if not invalid field, check if players are there
            if (_map[newPosition[0], newPosition[1]] == FieldType.PLAYER1 || _map[newPosition[0], newPosition[1]] == FieldType.PLAYER2 || _map[newPosition[0], newPosition[1]] == FieldType.PLAYER3) return;
            
            // save field type and then overwrite it to ROAD (or in specific cases BOMB)
            FieldType playerField = _map.GetField(position);
            if (playerField == FieldType.BOMB1 || playerField == FieldType.BOMB2 || playerField == FieldType.BOMB3)
            {
                _map.SetField(position, FieldType.BOMB);
            }
            else
            {
                _map.SetField(position, FieldType.ROAD);
            }

            // kill player if stepped on explosion or enemy
            if (_map.GetField(newPosition) == FieldType.EXPLOSION || _map.GetField(newPosition) == FieldType.ENEMY) {
                _players[playerId].Position = newPosition;
                PlayerDied(newPosition);
                return;
            }

            // check if powerup was on field for player to pick up
            if (_map.GetField(newPosition) == FieldType.POWERUP) {
                PlayerPickedPowerup(playerId);
            }

            FieldType player;
            switch (playerField)
            {
                case FieldType.BOMB1:
                    player = FieldType.PLAYER1;
                    break;
                case FieldType.BOMB2:
                    player = FieldType.PLAYER2;
                    break;
                case FieldType.BOMB3:
                    player = FieldType.PLAYER3;
                    break;
                default:
                    player = playerField; 
                    break;
            }
            _map.SetField(newPosition, player);

            // update player's position
            _players[playerId].Position = newPosition;

            // invoke step event
            if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);

            // disable stepping for a while
            _players[playerId].Stepped();
        }

        public void EnemyStep(int[] position)
        {
            Enemy? enemy = _enemies.Where(e => e.Position[0] == position[0] && e.Position[1] == position[1]).FirstOrDefault();
            if (enemy == null)
            {
                return;
            }
            int[] newPosition = NewPosition(enemy);
            FieldType newField = _map.GetField(newPosition);
            if (newField == FieldType.PLAYER1 || newField == FieldType.PLAYER2 || newField == FieldType.PLAYER3)
            {
                PlayerDied(newPosition);
            }
            if (newField == FieldType.ENEMY)
            {
                newPosition = NewPosition(enemy);
            }
            if (newField == FieldType.EXPLOSION)
            {
                EnemyDied(newPosition);
            }

            _map.SetField(position, FieldType.ROAD);
            _map.SetField(newPosition, FieldType.ENEMY);

            // update enemy's position
            enemy.Position = newPosition;

            // invoke step event
            if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
        }
        private int[] NewPosition(Enemy enemy)
        {
            int[] newPosition;
            int direction;
            int r;
            do
            {
                r = _rnd.Next(12);
                if (r < 4)
                {
                    direction = r;
                    enemy.PrevStep = r; //not sure of this
                }
                else
                {
                    direction = enemy.PrevStep;
                }
                newPosition = ValidPosition(enemy.Position, direction);
            } while (newPosition[0] == 0 && newPosition[1] == 0 || Map.GetField(newPosition) == FieldType.ENEMY);
            enemy.PrevStep = direction;
            return newPosition;
        }

    #endregion

        #region Bomb
        public void PlaceBomb(int[] position, int playerId)
        {
            if (!_players[playerId].Active || _isGameOver || _players[playerId].MaxBombNumber == 0)
            {
                return;
            }
            FieldType f = Map.GetField(position);
            if (_players[playerId].MaxBombNumber == _players[playerId].PlacedBombs  //placed all available bombs
                || f == FieldType.BOMB1
                || f == FieldType.BOMB2
                || f == FieldType.BOMB3) //currently stands on placed bomb
            {
                return;
            }
            Bomb b = new Bomb(this, position, playerId);
            FieldType type;
            switch (playerId)
            {
                case 0:
                    type = FieldType.BOMB1;
                    break;
                case 1:
                    type = FieldType.BOMB2;
                    break;
                default:
                    type = FieldType.BOMB3;
                    break;
            }

            _map.SetField(position, type);
            _bombs.Add(b);
            _players[playerId].PlaceBomb();

            //invoke event for viewmodel
            if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
        }
        public void ResetPlayerBomb(int playerId)
        {
            _players[playerId].ResetBomb();
        }

        #endregion

        #region Explosion
        public void Explosion(Bomb bomb, int playerId, int[] position){
            // return if indexes are invalid
            if (position[0] < 0 || position[0] >= _map.Size || position[1] < 0 || position[1] >= _map.Size) return;

            // initialize variables
            int bombRange = _players[playerId].BombRange;
            int r = position[0];
            int c = position[1];
            bool up = true;
            bool right = true;
            bool down = true;
            bool left = true;
            float t = 0.55f + bombRange * 0.1f;

            // kill player if standing on bomb
            if (_map.GetField(position) == FieldType.PLAYER1 || _map.GetField(position) == FieldType.BOMB1 ||
                _map.GetField(position) == FieldType.PLAYER2 || _map.GetField(position) == FieldType.BOMB2 ||
                _map.GetField(position) == FieldType.PLAYER3 || _map.GetField(position) == FieldType.BOMB3)
            {
                PlayerDied(position);
            }

            // explode field of bomb (we can assume the bomb itself is in a valid place because it is checked when placing a bomb)
            _map.SetField(position, FieldType.EXPLOSION);
            if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
            Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { if (_map.GetField(position) == FieldType.EXPLOSION) _map.SetField(position, FieldType.ROAD); if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty); });

            // spread explosion
            for (int i = 1; i <= bombRange; i++)
            {
                // explosion spread up
                Explosion_CheckUp(r - i, c, ref up, i, t);

                // explosion spread to the right
                Explosion_CheckRight(r, c + i, ref right, i, t);

                // explosion spread down
                Explosion_CheckDown(r + i, c, ref down, i, t);

                // explosion spread to the left
                Explosion_CheckLeft(r, c - i, ref left, i, t);
            }

            // refresh view after all fields exploded
            Task.Delay(TimeSpan.FromSeconds(t + 0.01 + 0.1 * bombRange)).ContinueWith(_ => { if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty); });

            // update placable bombs' number for player happens in bomb destroy

            // remove bomb from list
            try {
                _bombs.Remove(bomb);
            } catch (Exception) {
            
            }
        }

        private void SafeSpread(Bomb b, int r, int c) {
            if (_map[r, c] != FieldType.WALL)
            {
                b.Explode();
                if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
            }
            else {
                b.Destroy();
            }
        }

        private void SafeExplode(int r, int c) {
            if (_map[r, c] != FieldType.WALL)
            {
                if (_map[r, c] == FieldType.ENEMY) EnemyDied(new int[2] { r, c });
                _map.SetField(new int[2] { r, c }, FieldType.EXPLOSION);
                if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void SafeResetExplosion(int r, int c) {
            if (_map[r, c] == FieldType.EXPLOSION)
            {
                _map.SetField(new int[2] { r, c }, FieldType.ROAD);
                if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
            }
        }

        private void Explosion_CheckUp(int r, int c, ref bool up, int i, float t)
        {
            // if indices are inside bounds
            if (InsideBounds(r, c))
            {
                FieldType upField = _map[r, c];
                if (up && upField != FieldType.WALL)
                {
                    // handle bomb, box, enemy or player explosion
                    if (upField == FieldType.BOX)
                    {
                        BoxExploded(r, c, t);
                        up = false;
                    }
                    if (upField == FieldType.BOMB || upField == FieldType.BOMB1 || upField == FieldType.BOMB2 || upField == FieldType.BOMB3)
                    {
                        Bomb? b = GetBomb(r, c);
                        if (b is not null) Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeSpread(b, r, c); });
                        up = false;
                        return;
                    }
                    if (upField == FieldType.ENEMY) EnemyDied(new int[2] { r, c });
                    if (upField == FieldType.PLAYER1 || upField == FieldType.PLAYER2 || upField == FieldType.PLAYER3) PlayerDied(new int[2] { r, c });
                    // set and delay unsetting of fieldtype
                    Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeExplode(r, c); });
                    Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafeResetExplosion(r, c); });
                }
                // if up was true, but we ran into a wall, set up to false to stop explosion from spreading
                else if (up)
                {
                    up = false;
                }
            }
        }
        private void Explosion_CheckRight(int r, int c, ref bool right, int i, float t)
        {
            // if indices are inside bounds
            if (InsideBounds(r, c))
            {
                FieldType rightField = _map[r, c];
                if (right && rightField != FieldType.WALL)
                {
                    // handle bomb, box, enemy or player explosion
                    if (rightField == FieldType.BOX)
                    {
                        BoxExploded(r, c, t);
                        right = false;
                    }
                    if (rightField == FieldType.BOMB || rightField == FieldType.BOMB1 || rightField == FieldType.BOMB2 || rightField == FieldType.BOMB3)
                    {
                        Bomb? b = GetBomb(r, c);
                        if (b is not null) Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeSpread(b, r, c); });
                        right = false;
                        return;
                    }
                    if (rightField == FieldType.ENEMY) EnemyDied(new int[2] { r, c });
                    if (rightField == FieldType.PLAYER1 || rightField == FieldType.PLAYER2 || rightField == FieldType.PLAYER3) PlayerDied(new int[2] { r, c });
                    // set and delay unsetting of fieldtype
                    Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeExplode(r, c); });
                    Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafeResetExplosion(r, c); });
                }
                // if right was true, but we ran into a wall, set right to false to stop explosion from spreading
                else if (right)
                {
                    right = false;
                }
            }
        }
        private void Explosion_CheckDown(int r, int c, ref bool down, int i, float t)
        {
            // if indices are inside bounds
            if (InsideBounds(r, c))
            {
                FieldType downField = _map[r, c];
                if (down && downField != FieldType.WALL)
                {
                    // handle bomb, box, enemy or player explosion
                    if (downField == FieldType.BOX)
                    {
                        BoxExploded(r, c, t);
                        down = false;
                    }
                    if (downField == FieldType.BOMB || downField == FieldType.BOMB1 || downField == FieldType.BOMB2 || downField == FieldType.BOMB3)
                    {
                        Bomb? b = GetBomb(r, c);
                        if (b is not null) Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeSpread(b, r, c); });
                        down = false;
                        return;
                    }
                    if (downField == FieldType.ENEMY) EnemyDied(new int[2] { r, c });
                    if (downField == FieldType.PLAYER1 || downField == FieldType.PLAYER2 || downField == FieldType.PLAYER3) PlayerDied(new int[2] { r, c });
                    // set and delay unsetting of fieldtype
                    Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeExplode(r, c); });
                    Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafeResetExplosion(r, c); });
                }
                // if down was true, but we ran into a wall, set down to false to stop explosion from spreading
                else if (down)
                {
                    down = false;
                }
            }
        }
        private void Explosion_CheckLeft(int r, int c, ref bool left, int i, float t)
        {
            // if indices are inside bounds
            if (InsideBounds(r, c))
            {
                FieldType leftField = _map[r, c];
                if (left && leftField != FieldType.WALL)
                {
                    // handle bomb, box, enemy or player explosion
                    if (leftField == FieldType.BOX)
                    {
                        BoxExploded(r, c, t);
                        left = false;
                    }
                    if (leftField == FieldType.BOMB || leftField == FieldType.BOMB1 || leftField == FieldType.BOMB2 || leftField == FieldType.BOMB3)
                    {
                        Bomb? b = GetBomb(r, c);
                        if (b is not null) Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeSpread(b, r, c); });
                        left = false;
                        return;
                    }
                    if (leftField == FieldType.ENEMY) EnemyDied(new int[2] { r, c });
                    if (leftField == FieldType.PLAYER1 || leftField == FieldType.PLAYER2 || leftField == FieldType.PLAYER3) PlayerDied(new int[2] { r, c });
                    // set and delay unsetting of fieldtype
                    Task.Delay(TimeSpan.FromSeconds(0.1 * i)).ContinueWith(_ => { SafeExplode(r, c); });
                    Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafeResetExplosion(r, c); });
                }
                // if left was true, but we ran into a wall, set left to false to stop explosion from spreading
                else if (left)
                {
                    left = false;
                }
            }
        }

       
        private bool InsideBounds(int i, int j)
        {
            // check if indices are inside the bounds of the map
            if (i < 0 || i >= _map.Size) return false;
            if (j < 0 || j >= _map.Size) return false;
            return true;
        }
        private void BoxExploded(int r, int c, float t)
        {
            // replace box with powerup with a chance of 0.4
            if (_rnd.NextDouble() < 0.4)
            {
                Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafePowerUpSpawn(r, c, FieldType.POWERUP); });
            }
            // otherwise just remove box
            else
            {
                Task.Delay(TimeSpan.FromSeconds(t)).ContinueWith(_ => { SafePowerUpSpawn(r, c, FieldType.ROAD); });
            }
        }
        private void SafePowerUpSpawn(int r, int c, FieldType t) {
            if (_map[r, c] != FieldType.WALL) {
                _map.SetField(new int[2] { r, c }, t);
            }
        }
        private Bomb? GetBomb(int r, int c)
        {
            // get bomb present at the specified coordinates
            int i = 0;
            while (!(_bombs[i].Position[0] == r && _bombs[i].Position[1] == c))
            {
                i++;
            }
            if (i < _bombs.Count)
            {
                return _bombs[i];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Saves
        public void LoadSaves()
        {
            Saves = new List<Save>();

            Saves = _dataAccess.LoadSaves();
        }
        public void SaveGameState(int mapId, string name)
        {
            Saves = _dataAccess.Save(_map, _players, mapId, name, _bombs, _enemies, _gameTime, _shrinkTime, _shrinkRound, _order, _gameOverTime, _matchLength, _originalShrinkTime);            
        }
        public void LoadGameState(string loadId)
        {
            Save load;
            (_map, load) = _dataAccess.LoadSavedGame(loadId);
            InitLoadedGame(load);
            
        }
        public void DeleteGameState(string deleteId)
        {
            _dataAccess.DeleteSavedGame(deleteId);
            LoadSaves();
        }

        #endregion

        #region GameOver
        private void IsGameOver()
        {
            if (_order.Count == _numberOfPlayers)
            {
                Finish();
            }
            else if (_order.Count == _numberOfPlayers - 1)
            {
                if (_gameOverTime == -1)
                {
                    _gameOverTime = 50;
                }
            }
        }
        private void Finish()
        {
            if (_isGameOver)
            {
                return;
            }

            foreach (var bomb in _bombs)
            {
                bomb.Destroy();
            }
            _gameOverTime = 40;

            // get winner and decide game result and send messages
            Player? winner = _players.Where(p => p.Active).FirstOrDefault();
            if (winner != null)
            {
                winner.Win();
                _isGameOver = true;
                string sender = GetSenderFromId(winner.Id);
                MessageSent?.Invoke(this, new MessageEventArgs("INFO", sender + " wins."));

            }
            else
            {
                _isGameOver = true;
                string s = GetSenderFromId(_order[_order.Count - 1]);
                s += " and " + GetSenderFromId(_order[_order.Count - 2]);
                MessageSent?.Invoke(this, new MessageEventArgs("INFO", s + " tied the game."));

            }

            // destroy alive powerups
            foreach (Player p in _players) p.DestroyPowerups();

            // end game if necessary
            if (_players.Any(p => p.Wins == _matchLength))
            {
                FullGameOver();
            }
            // otherwise start new round
            else
            {
                if (GameOver is not null) GameOver.Invoke(this, EventArgs.Empty);
            }
        }
        private void FullGameOver()
        {
            _matchOver = true;

            GameOver.Invoke(this, EventArgs.Empty);
            string sender = "PLAYER" + (_players.Where(p => p.Wins >= _matchLength).First().Id + 1).ToString();
            foreach (var p in _players)
            {
                p.ResetWins();
            }
            MessageSent?.Invoke(this, new MessageEventArgs(sender, sender + " won the game."));
            for (int i = 1; i < 6; i++)
            {
                string s = "Returning to menu in " + (6 - i) + " seconds";
                MessageSent?.Invoke(this, new MessageEventArgs("INFO", s));
                Thread.Sleep(1000);
            }
            MatchOver.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Battle Royale
        private void DestroyPosition(int x, int y)
        {
            // kill player
            if (_map[x, y] == FieldType.PLAYER1 ||
                    _map[x, y] == FieldType.PLAYER2 ||
                    _map[x, y] == FieldType.PLAYER3)
            {
                PlayerDied(new int[2] { x, y });
            }
            // kill enemy
            else if (_map[x, y] == FieldType.ENEMY)
            {
                EnemyDied(new int[2] { x, y });
            }
            // destroy bomb to avoid explosion in the wall
            else if (_map[x, y] == FieldType.BOMB)
            {
                Bomb b = GetBomb(x, y);
                if (b is not null)
                {
                    _bombs.Remove(b);
                    b.Destroy();
                }
            }
            // destroy bomb and kill player if both on the field
            else if (_map[x, y] == FieldType.BOMB1 ||
                _map[x, y] == FieldType.BOMB2 ||
                _map[x, y] == FieldType.BOMB3)
            {
                Bomb b = GetBomb(x, y);
                if (b is not null)
                {
                    _bombs.Remove(b);
                    b.Destroy();
                }
                PlayerDied(new int[2] { x, y });
            }

            // set field to wall
            _map.SetField(new int[2] { x, y }, FieldType.WALL);
        }
        private void ShrinkMap()
        {
            for (int i = 1 + _shrinkRound; i < _map.Size - 1 - _shrinkRound; i++)
            {
                DestroyPosition(1 + _shrinkRound, i);
                DestroyPosition(_map.Size - 2 - _shrinkRound, i);
                DestroyPosition(i, 1 + _shrinkRound);
                DestroyPosition(i, _map.Size - 2 - _shrinkRound);
            }

            // update shrinktime
            _shrinkRound++;
            _shrinkTime += _originalShrinkTime - _shrinkRound * _shrinkDecrement;

            // invoke step event
            MapChanged.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Deaths
        private void EnemyDied(int[] position)
        {
            Map.SetField(position, FieldType.ROAD);
            Enemy? enemy = _enemies.Where(e => e.Position[0] == position[0] && e.Position[1] == position[1]).FirstOrDefault();
            if (enemy != null)
            {
                enemy.Die();
                _enemies.Remove(enemy);
            }
        }

        private void PlayerDied(int[] position)
        {
            Player? player = _players.Where(p => p.Position[0] == position[0] && p.Position[1] == position[1] && p.Active).FirstOrDefault();
            if (player != null)
            {
                player.Die();
                if (MapChanged is not null) MapChanged.Invoke(this, EventArgs.Empty);
                if (!_order.Contains(player.Id))
                {
                    _order.Add(player.Id);
                }
                string sender = GetSenderFromId(player.Id);
                MessageSent?.Invoke(this, new MessageEventArgs(sender, sender + " died."));
                IsGameOver();
            }
        }
        #endregion

        #region Getters 
        public int GetPlayerWins(int playerId)
        {
            if (playerId >= _players.Length) return 0;
            return _players[playerId].Wins;
        }

        private string GetSenderFromId(int playerId)
        {
            string sender;
            switch (playerId)
            {
                case 0:
                    sender = "PLAYER1";
                    break;
                case 1:
                    sender = "PLAYER2";
                    break;
                default:
                    sender = "PLAYER3";
                    break;
            }
            return sender;
        }
        #endregion

        #region Powerup 
        private void PlayerPickedPowerup(int playerId) {
            // get a random powerup and pass it to the player
            PowerUpType[] powerups = (PowerUpType[])Enum.GetValues(typeof(PowerUpType));
            int powerupNumber = powerups.Length;
            int ind = _rnd.Next(powerupNumber);
            PowerUpType powerup = powerups[ind];
            _players[playerId].PickedPowerup(powerup);
        }

        public void PlayerPickedPowerupMessage(int playerId, PowerUpType powerup, bool nochange) {
            // trigger message
            string sender = GetSenderFromId(playerId);
            string message;
            switch (powerup)
            {
                case PowerUpType.PLUSBOMB:
                    if (_players[playerId].MaxBombNumber != 0)
                        message = "Number of placable bombs increased to " + _players[playerId].MaxBombNumber + "!";
                    else
                        message = "Original number of placable bombs increased to " + _players[playerId].OldMaxBombNumber + "!";
                    break;
                case PowerUpType.PLUSRANGE:
                    if (_players[playerId].BombRange != 1)
                        message = "Range of explosions increased to " + _players[playerId].BombRange + "!";
                    else
                        message = "Original range of explosions increased to " + _players[playerId].OldRange + "!";
                    break;
                case PowerUpType.MINUSSPEED:
                    message = "Speed decreased for 10 seconds!";
                    break;
                case PowerUpType.ONERANGE:
                    message = "Bombs range decreased to 1 for 10 seconds!";
                    break;
                case PowerUpType.NOBOMB:
                    message = "Can't place bombs for 10 seconds!";
                    break;
                case PowerUpType.INSTANTPLACEMENT:
                    message = "Instantly places bombs for 10 seconds!";
                    break;
                default:
                    message = "Unknown powerup!";
                    break;
            }
            if (nochange)
                message = "Already has this powerup!";
            MessageSent?.Invoke(this, new MessageEventArgs(sender, message));
        }

        public void PlayerEndedPowerup(int playerId, PowerUpType powerup) {
            // trigger message
            string sender = GetSenderFromId(playerId);
            string message;
            switch (powerup)
            {
                case PowerUpType.MINUSSPEED:
                    message = "Decreased speed expired"; // TODO: How many seconds?
                    break;
                case PowerUpType.ONERANGE:
                    message = "Bombs range set back to " + _players[playerId].BombRange + "!"; // TODO: How many seconds?
                    break;
                case PowerUpType.NOBOMB:
                    message = "Can place bombs again!"; // TODO: How many seconds?
                    break;
                case PowerUpType.INSTANTPLACEMENT:
                    message = "Instant bomb placement expired!"; // TODO: How many seconds?
                    break;
                default:
                    message = "Unknown powerup expired!";
                    break;
            }
            MessageSent?.Invoke(this, new MessageEventArgs(sender, message));
        }
        #endregion

        #region Timer Tick 
        private void OnTick(object state) {
            if (_isGameOver) {
                _gameOverTime--;
                if (_gameOverTime <= 0) {
                    _timer.Dispose();
                    StartGame();
                }
                else if (_gameOverTime % 10 == 0 && !_matchOver)
                {
                    string s = "Next round starts in " + (_gameOverTime / 10) + " seconds";
                    MessageSent?.Invoke(this, new MessageEventArgs("INFO", s));
                }
                return;
            }

            // increase gametime
            _gameTime++;

            // send message 5 secs before shrink
            if (_gameTime + 50 == _shrinkTime) { 
                MessageSent?.Invoke(this, new MessageEventArgs("WALL", "Shrink in 5 seconds!"));
            }

            // shrink map if gametime reached the given threshold
            if (_gameTime == _shrinkTime) {
                ShrinkMap();
                MessageSent?.Invoke(this, new MessageEventArgs("WALL", "The wall has shrank! Next shrink in " + (_shrinkTime - _gameTime)/10 + " seconds."));
            }

            // gameover time handling
            if (_gameOverTime == 0)
            {
                Finish();
            }
            else if (_gameOverTime > 0)
            {
                _gameOverTime--;
            }

            // invoke event for view (only every whole second)
            GameAdvanced?.Invoke(this, EventArgs.Empty);
        }
        #endregion

    }
}
