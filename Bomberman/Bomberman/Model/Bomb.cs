using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Model
{
    public class Bomb
    {
        #region Fields

        private GameModel _model;
        private int[] _position;
        private int _playerId;
        private int _lifetime;

        #endregion

        #region Properties
        public int[] Position { get { return _position; } }
        #endregion

        #region Constructor

        public Bomb(GameModel model, int[] position, int playerId) {
            _model = model;
            _position = position;
            _playerId = playerId;
            _lifetime = 20;

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
        public void Explode()
        {
            // handle explosion in model
            _model.Explosion(this, _playerId, _position);

            Destroy();
        }

        public void Destroy()
        {
            // unsubscribe from event to avoid infinite explosion
            // or explosion after stuck in wall (when map shrank on bomb)
            _model.GameAdvanced -= OnTick;

            // reset placable bomb number of player
            _model.ResetPlayerBomb(_playerId);
        }
        #endregion

        #region Private methods
        private void OnTick(object? sender, EventArgs e)
        {
            // decrease lifetime on tick of model timer
            _lifetime--;

            // explode if lifetime is over
            if (_lifetime <= 0)
            {
                Explode();
            }
        }
        #endregion
    }
}
