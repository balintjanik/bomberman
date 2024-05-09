using Bomberman.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman.Persistence
{
    public class Save
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Timestamp { get; set; }
        public Player[] Players { get; set; }
        public List<Bomb> Bombs { get; set; }
        public List<Enemy> Enemies { get; set; }
        public int GameTime { get; set; }
        public int ShrinkTime { get; set; }
        public int ShrinkRound { get; set; }
        public int OriginalShrinkTime { get; set; }
        public string MapId { get; set; }
        public FieldType[][] Data { get; set; }
        public List<int> Order { get; set; }
        public int GameOverTime { get; set; }
        public int MatchLength { get; set; }

    }
}
