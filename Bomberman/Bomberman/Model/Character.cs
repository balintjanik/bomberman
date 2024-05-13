namespace Bomberman.Model
{
    public abstract class Character
    {
        #region Fields

        protected int _speed;
        private int[] _position;

        #endregion

        #region Properties

        public int[] Position
        {
            get { return _position; }
            set { _position = value; }
        }
        #endregion

        #region Constructor

        public Character()
        {
            _position = new int[2];
        }

        #endregion

    }
}
