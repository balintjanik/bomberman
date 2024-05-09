using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Model
{
    public class Player : Character
    {
        #region Fields
        private GameModel _model;
        private List<PowerUp> _powerUps;
        private int[] _startPosition;

        private int _id;
        private int _maxBombNumber;
        private int _oldMaxBombNumber;
        private int _placedBombs;
        private int _speedCounter;
        private int _bombRange;
        private int _oldRange;
        private int _wins;
        private bool _canStep;
        private bool _active;
        private bool _instantPlacement;

        #endregion

        #region Properties

        public int Id { get { return _id; } }
        public List<PowerUp> PowerUps { get { return _powerUps; } }
        public int OldMaxBombNumber { get { return _oldMaxBombNumber; } }
        public int MaxBombNumber { get { return _maxBombNumber; } }
        public int PlacedBombs { get { return _placedBombs; } }
        public bool CanStep { get { return _canStep; } }
        public int OldRange { get { return _oldRange; } }
        public int BombRange { get { return _bombRange; } }
        public int Wins { get { return _wins; } }
        public bool Active { get { return _active; } }
        public int Speed { get { return _speed; } }
        public int[] StartPosition { get { return _startPosition; } set { _startPosition = value; } }
        public bool InstantPlacement { get { return _instantPlacement; } }
        #endregion

        #region Constructor
        public Player(GameModel model, int id, int wins, List<PowerUp> powerUps = null, int oldRange = 2, int bombRange = 2, int placedBombs = 0, bool active = true, int speed = 1, bool canStep = true, int oldMaxBombNumber = 1, int maxBombNumber = 1, int[] position = null, bool instantPlacement = false)
        {
            _id = id;
            if (powerUps == null)
            {
                _powerUps = new List<PowerUp>();
            }
            else
            {
                _powerUps = powerUps;
            }
            _oldRange = oldRange;
            _bombRange = bombRange;
            _oldMaxBombNumber = oldMaxBombNumber;
            _maxBombNumber = maxBombNumber;
            _placedBombs = placedBombs;
            _active = active;
            _wins = wins;
            _model = model;
            // the higher the speed the slower the player can go
            _speed = speed; // the player can step every tick (every 0.1 seconds)
            _speedCounter = _speed;
            _canStep = true;
            Position = position;
            _instantPlacement = instantPlacement;
           

            if (_model != null)
            {
                _model.GameAdvanced += new EventHandler(OnTick);
            }
        }

        #endregion

        #region Public methods
        public void LoadModel(GameModel model)
        {
            _model = model;
            _model.GameAdvanced += new EventHandler(OnTick);
        }
        public void Die()
        {
            _active = false;
            DestroyPowerups();
        }
        public void DestroyPowerups()
        {
            // ToList copies it to a new list and it will not be modified during
            foreach (var powerUp in _powerUps.ToList())
            {
                powerUp.DestroyPowerUp();
                RemovePowerUp(powerUp);
            }
        }
        public void Revive()
        {
            _active = true;
            _oldMaxBombNumber = 1;
            _maxBombNumber = 1;
            _placedBombs = 0;
            _oldRange = 2;
            _bombRange = 2;
            _speed = 1;
            _instantPlacement = false;
        }
        public void ResetBomb()
        {
            _placedBombs--;
        }
        public void PlaceBomb()
        {
            _placedBombs++;
        }

        public void Stepped()
        {
            _canStep = false;
            _speedCounter = _speed;
        }
        public void Win()
        {
            _wins++;
        }
        public void ResetWins()
        {
            _wins = 0;
        }
        public void PickedPowerup(PowerUpType powerup) {
            bool nochange = false;
            switch(powerup)
            {
                case PowerUpType.PLUSBOMB:
                    // increase old max bomb number if NOBOMB is active
                    if (_maxBombNumber == 0) _oldMaxBombNumber++;
                    // increase max bomb number otherwise
                    else _maxBombNumber++;
                    // output log
                    _model.PlayerPickedPowerupMessage(_id, powerup, nochange);
                    return;
                case PowerUpType.PLUSRANGE:
                    // increase old range if ONERANGE is active
                    if (_bombRange == 1) _oldRange++;
                    // increase bomb range otherwise
                    else _bombRange++;
                    // output log
                    _model.PlayerPickedPowerupMessage(_id, powerup, nochange);
                    return;
                case PowerUpType.MINUSSPEED:
                    // nothing happens if already slowed down
                    if (_speed == 3) nochange = true;
                    // slow down otherwise
                    else _speed = 3;
                    break;
                case PowerUpType.ONERANGE:
                    // nothing happens if already has onerange
                    if (_bombRange == 1) nochange = true;
                    // set onerange otherwise
                    else
                        _oldRange = _bombRange;
                        _bombRange = 1;
                    break;
                case PowerUpType.NOBOMB:
                    // nothing happens if already has nobomb
                    if (_maxBombNumber == 0) nochange = true;
                    // set no placable bombs otherwise
                    else
                        _oldMaxBombNumber = _maxBombNumber;
                        _maxBombNumber = 0;
                    break;
                case PowerUpType.INSTANTPLACEMENT:
                    // nothing happens if already has instantplacement
                    if (_instantPlacement) nochange = true;
                    // set instantplacement otherwise
                    else _instantPlacement = true;
                    break;
                default:
                    break;
            }
            // start countdown for timed powerups
            if (!nochange)
                _powerUps.Add(new PowerUp(_model, this, powerup, 100));

            // output log
            _model.PlayerPickedPowerupMessage(_id, powerup, nochange);
        }

        public void RemovePowerUp(PowerUp powerUp)
        {
            PowerUpType type = powerUp.Type;
            _powerUps.Remove(powerUp);
            switch(type)
            {
                case PowerUpType.MINUSSPEED:
                    _speed = 1;
                    break;
                case PowerUpType.ONERANGE:
                    _bombRange = _oldRange;
                    break;
                case PowerUpType.NOBOMB:
                    _maxBombNumber = _oldMaxBombNumber;
                    break;
                case PowerUpType.INSTANTPLACEMENT:
                    _instantPlacement = false;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Private methods
        private void OnTick(object? sender, EventArgs e)
        {
            if (!_canStep) {
                // decrease counter on tick of model timer
                _speedCounter--;

                // allow step if counter reached 0
                if (_speedCounter <= 0) _canStep = true;
            }

            // instantly place bomb if INSTANTPLACEMENT is active
            if (_instantPlacement) _model.PlaceBomb(Position, _id);
        }

        #endregion
    }
}
