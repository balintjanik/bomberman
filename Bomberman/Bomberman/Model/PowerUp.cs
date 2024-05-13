namespace Bomberman.Model
{
    public class PowerUp
    {
        #region Fields

        private GameModel _model;
        private Player _owner;
        private int _lifeTime;
        private PowerUpType _type;

        #endregion

        #region Properties
        public PowerUpType Type { get { return _type; } }
        public int LifeTime { get { return _lifeTime; } }

        #endregion

        #region Constructor

        public PowerUp(GameModel model, Player owner, PowerUpType type, int lifeTime)
        {
            _model = model;
            _owner = owner;
            _type = type;
            _lifeTime = lifeTime;

            if (_model != null)
            {
                _model.GameAdvanced += new EventHandler(OnTick);
            }
        }

        #endregion

        #region Public methods
        public void LoadModel(GameModel model, Player owner)
        {
            _model = model;
            _model.GameAdvanced += new EventHandler(OnTick);
            _owner = owner;
        }
        public void OnTick(object? sender, EventArgs e)
        {
            _lifeTime--;

            if (_lifeTime <= 0)
            {
                _owner.RemovePowerUp(this);
                _model.PlayerEndedPowerup(_owner.Id, _type);
                DestroyPowerUp();
            }
        }

        public void DestroyPowerUp()
        {
            _model.GameAdvanced -= OnTick;
        }

        #endregion
    }
}
