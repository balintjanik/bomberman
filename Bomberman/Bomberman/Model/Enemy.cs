using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Model
{
    public class Enemy : Character
    {
        #region Fields
        private int _prevStep;
        private int _timeLeft;
        private GameModel _model;

        #endregion

        #region Properties
        public int PrevStep { get { return _prevStep; } set => _prevStep = value; }
        #endregion

        #region Constructor
        public Enemy(GameModel model, int[] position, int dir, int speed) {
            _model = model;
            Position = position;
            PrevStep = dir;
            _speed = speed;
            _timeLeft = 10;

            if (_model != null)
            {
                _model.GameAdvanced += new EventHandler(Move);
            }
        }
        #endregion

        #region Public methods       
        public void Die()
        {
            _model.GameAdvanced -= new EventHandler(Move);
        }
        #endregion

        #region Private methods
        private void Move(object? sender, EventArgs e)
        {
            _timeLeft--;
            if (_timeLeft == 0)
            {
                _model.EnemyStep(Position);
                _timeLeft = _speed;
            }
        }
        #endregion

    }
}
