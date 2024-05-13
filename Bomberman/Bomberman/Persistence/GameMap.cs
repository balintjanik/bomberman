namespace Bomberman.Persistence
{
    public class GameMap
    {
        #region Fields

        private int _size;
        private FieldType[,] _map;

        #endregion

        #region Properties
        public int Size { get => _size; }
        public FieldType[,] Map { get => _map; }
        public FieldType this[int x, int y] { get { return GetField(new int[2] { x, y }); } }

        #endregion

        #region Constructors
        public GameMap(int size)
        {
            _size = size;
            _map = new FieldType[size, size];
        }

        public GameMap(FieldType[,] map)
        {
            _size = (int)Math.Sqrt(map.Length); //given that maps are NxN sized 
            _map = map;
        }
        #endregion

        #region Public methodss
        public void SetField(int[] coords, FieldType type)
        {
            _map[coords[0], coords[1]] = type;
        }
        public FieldType GetField(int[] coords)
        {
            return _map[coords[0], coords[1]];
        }
        public int[]? GetPlayerPosition(int playerId)
        {
            FieldType p;
            FieldType bp; //to handle if player stands on bomb
            switch (playerId)
            {
                case 0:
                    p = FieldType.PLAYER1;
                    bp = FieldType.BOMB1;
                    break;
                case 1:
                    p = FieldType.PLAYER2;
                    bp = FieldType.BOMB2;
                    break;
                case 2:
                    p = FieldType.PLAYER3;
                    bp = FieldType.BOMB3;
                    break;
                default:
                    return null;
            }

            for (int i = 0; i < _size; i++)
            {
                for (int j = 0; j < _size; j++)
                {
                    if (_map[i, j] == p || _map[i, j] == bp)
                    {
                        return new int[2] { i, j };
                    }
                }
            }
            return null;
        }

        #endregion

    }
}
