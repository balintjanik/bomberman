using Bomberman.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Bomberman.Persistence
{
    public class DataAccess
    {
        private string _path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Files\GameState.json");
        public DataAccess() { }
        public GameMap Load(string path)
        {
            GameMap map;
            try
            {
                StreamReader sr = new(path);
                int size = int.Parse(sr.ReadLine()!);
                map = new GameMap(size);
                for (int i = 0; i < size; i++)
                {
                    string line = sr.ReadLine()!;
                    string[] data = line.Split(' ');
                    for (int j = 0; j < size; j++)
                    {
                        int[] coords = { i, j };
                        // WALL - w,
                        // ROAD - r,
                        // BOX - x,
                        // BOMB - b,
                        // EXPLOSION - e,
                        // ENEMY - y,
                        // PLAYER1 - 1,
                        // PLAYER2 - 2,
                        // PLAYER3 - 3,
                        // BOMB1 - b1,
                        // BOMB2 - b2,
                        // BOMB3 - b3,
                        // POWERUP - p
                        
                        switch (data[j])
                        {
                            case "w":
                                map.SetField(coords, FieldType.WALL);
                                break;
                            case "r":
                                map.SetField(coords, FieldType.ROAD);
                                break;
                            case "x":
                                map.SetField(coords, FieldType.BOX);
                                break;
                            case "b":
                                map.SetField(coords, FieldType.BOMB);
                                break;
                            case "e":
                                map.SetField(coords, FieldType.EXPLOSION);
                                break;
                            case "y":
                                map.SetField(coords, FieldType.ENEMY);
                                break;
                            case "1":
                                map.SetField(coords, FieldType.PLAYER1);
                                break;
                            case "2":
                                map.SetField(coords, FieldType.PLAYER2);
                                break;
                            case "3":
                                map.SetField(coords, FieldType.PLAYER3);
                                break;
                            case "b1":
                                map.SetField(coords, FieldType.BOMB1);
                                break;
                            case "b2":
                                map.SetField(coords, FieldType.BOMB2);
                                break;
                            case "b3":
                                map.SetField(coords, FieldType.BOMB3);
                                break;
                            case "p":
                                map.SetField(coords, FieldType.POWERUP);
                                break;
                            default:
                                break;
                        }
                    }
                }
                sr.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return map;
        }

        public (GameMap,Save) LoadSavedGame(string id)
        {
            Save load;
            int numOfPlayers;
            Player[] players;
            List<Enemy> enemies;

            string jsonString = File.ReadAllText(_path);            
            load = JsonConvert.DeserializeObject<List<Save>>(jsonString)!.Where(s => s.Id == id).First();
            numOfPlayers = load.Players.Count();
            players = load.Players;
            enemies = load.Enemies;

            GameMap map;
            try
            {
                FieldType[][] jaggedArray = load.Data;
                int size = jaggedArray.Length;

                FieldType[,] fields = new FieldType[size, size];
                for (int i = 0; i < jaggedArray[0].Length; i++)
                {
                    for (int j = 0; j < jaggedArray[0].Length; j++)
                    {
                        fields[i, j] = jaggedArray[i][j];
                    }
                }
                map = new GameMap(fields);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return (map, load);
        }

        public List<Save> Save(GameMap map, Player[] players, int mapId, string name, List<Bomb> bombs, List<Enemy> enemies, int gameTime, int shrinkTime, int shrinkRound, List<int> order, int gameOverTime, int matchLength, int originalShrinkTime)
        {
            try
            {
                string now = DateTime.Now.ToString("yyyy.MM.dd HH:mm");

                FieldType[][] jaggedArray = new FieldType[map.Size][];
                for (int i = 0; i < map.Size; i++)
                {
                    jaggedArray[i] = new FieldType[map.Size];
                    for (int j = 0; j < map.Size; j++)
                    {
                        jaggedArray[i][j] = map[i, j];
                    }
                }


                var save = new Save
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name,
                    Timestamp = now,
                    Players = players,
                    MapId = "map"+mapId.ToString(),
                    Data = jaggedArray,
                    Bombs = bombs,
                    Enemies = enemies,
                    GameTime = gameTime,
                    ShrinkRound = shrinkRound,
                    ShrinkTime = shrinkTime,
                    Order = order,
                    GameOverTime = gameOverTime,
                    MatchLength = matchLength,
                    OriginalShrinkTime = originalShrinkTime
                };

                List<Save> jsonArray = LoadSaves();
                jsonArray.Add(save);
                
                File.WriteAllText(_path, JsonConvert.SerializeObject(jsonArray));
                return jsonArray;
            }
            catch (Exception e)
            {

                throw new Exception(e.Message);
            }
           

            
        }
        public void DeleteSavedGame(string id)
        {
            List<Save> jsonArray = new List<Save>();
            string jsonStringOriginal = File.ReadAllText(_path);
            if (jsonStringOriginal != "")
            {
                jsonArray = JsonConvert.DeserializeObject<List<Save>>(jsonStringOriginal);
            }
            Save delete = jsonArray.Where(d => d.Id == id).FirstOrDefault()!;
            jsonArray.Remove(delete);
            File.WriteAllText(_path, JsonConvert.SerializeObject(jsonArray));

        }
        public List<Save> LoadSaves()
        {
            List<Save> jsonArray = new List<Save>();
            string jsonStringOriginal = File.ReadAllText(_path);
            if (jsonStringOriginal != "")
            {
                jsonArray = JsonConvert.DeserializeObject<List<Save>>(jsonStringOriginal);
            }
            return jsonArray;
        }
       
    }
}
