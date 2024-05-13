using Bomberman.Model;
using Bomberman.Persistence;
using Newtonsoft.Json;
namespace Bomberman_Test
{
    [TestClass]
    public class GameModelTest
    {
        private GameModel gameModel = null!;
        private GameMap gameMap = null!;

        #region Constructor
        [TestInitialize]
        public void Initialize()
        {
            // Initialize if necessary
            gameModel = new GameModel();
        }

        #endregion

        #region Explosion test
        [TestMethod]
        public void GameModel_InvalidIndexExplosion()
        {
            gameModel.Init(0, 3);
            FieldType[,] map = (FieldType[,])gameModel.Map.Map.Clone();
            Bomb b = new Bomb(gameModel, new int[] { -1, -1 }, 0);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Assert.AreEqual(map[i, j], gameModel.Map.Map[i, j]);
                }
            }
            gameModel.Explosion(b, 0, b.Position);
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Assert.AreEqual(map[i, j], gameModel.Map.Map[i, j]);
                }
            }
        }

        [TestMethod]
        public async Task GameModel_ExplosionPlayerWallBox()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;
            Bomb b = new Bomb(gameModel, new int[] { 1, 3 }, 0);

            Assert.AreEqual(FieldType.BOMB, gameMap[1, 3]);
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.WALL, gameMap[1, 0]);
            Assert.AreEqual(FieldType.WALL, gameMap[0, 3]);
            Assert.AreEqual(FieldType.WALL, gameMap[1, 4]);
            Assert.AreEqual(FieldType.BOX, gameMap[2, 3]);
            Assert.AreEqual(FieldType.BOMB3, gameMap[3, 3]);

            gameModel.Explosion(b, 0, b.Position);

            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 3]); // bomb must be gone
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 2]); // player2 must be dead
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 1]); // player1 must be dead
            Assert.AreEqual(FieldType.WALL, gameMap[1, 0]); // explosion must not spread further than 2 fields
            Assert.AreEqual(FieldType.WALL, gameMap[0, 3]); // wall must stay there
            Assert.AreEqual(FieldType.WALL, gameMap[1, 4]); // wall must stay there
            bool isBoxOrPowerUp = gameMap[2, 3] == FieldType.ROAD || gameMap[2, 3] == FieldType.POWERUP;
            Assert.IsTrue(isBoxOrPowerUp); // box must explode into either powerup or road
            Assert.AreEqual(FieldType.BOMB3, gameMap[3, 3]); // box must stop explosion from spreading
        }


        [TestMethod]
        public async Task GameModel_ExplosionEnemyPowerUp()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;
            Bomb b = new Bomb(gameModel, new int[] { 3, 1 }, 0);
            gameMap.SetField(new int[] { 3, 1 }, FieldType.BOMB);
            gameMap.SetField(new int[] { 3, 2 }, FieldType.POWERUP);
            gameMap.SetField(new int[] { 3, 3 }, FieldType.ROAD);

            Assert.AreEqual(FieldType.BOMB, gameMap[3, 1]);
            Assert.AreEqual(FieldType.ENEMY, gameMap[2, 1]);
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.BOX, gameMap[0, 1]);
            Assert.AreEqual(FieldType.WALL, gameMap[3, 0]);
            Assert.AreEqual(FieldType.WALL, gameMap[4, 1]);
            Assert.AreEqual(FieldType.POWERUP, gameMap[3, 2]);
            Assert.AreEqual(FieldType.ROAD, gameMap[3, 3]);
            Assert.AreEqual(FieldType.WALL, gameMap[3, 4]);

            gameModel.Explosion(b, 0, b.Position);

            await Task.Delay(TimeSpan.FromSeconds(1));
            Assert.AreEqual(FieldType.ROAD, gameMap[3, 1]); // bomb must be gone
            Assert.AreEqual(FieldType.ROAD, gameMap[2, 1]); // enemy must be dead
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 1]); // player1 must be dead
            Assert.AreEqual(FieldType.BOX, gameMap[0, 1]); // explosion must not spread further than 2 fields
            Assert.AreEqual(FieldType.WALL, gameMap[3, 0]); // wall must stay there
            Assert.AreEqual(FieldType.WALL, gameMap[4, 1]); // wall must stay there
            Assert.AreEqual(FieldType.ROAD, gameMap[3, 2]); // powerup must be gone
            Assert.AreEqual(FieldType.ROAD, gameMap[3, 3]); // road must stay there
            Assert.AreEqual(FieldType.WALL, gameMap[3, 4]); // explosion must not spread further than 2 fields
        }
        #endregion

        #region GameModel Step tests
        [TestMethod]
        public void GameModel_StepTest()
        {
            // map 0:
            // w x w w w
            // w 1 2 b w
            // w y e x w
            // w r r b3 w
            // w w w p w

            GameModel_StepIntoWall();

            GameModel_StepIntoBox();

            GameModel_StepIntoPlayer();

            GameModel_StepIntoEnemy();

            GameModel_StepIntoBomb();

            GameModel_StepIntoExplosion();

            GameModel_StepPlayerFromBomb();

            GameModel_StepIntoPowerup();
        }

        private void GameModel_StepIntoWall()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p1 step left into wall: fields must not change
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.WALL, gameMap[1, 0]);
            gameModel.Step(0, 2);
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.WALL, gameMap[1, 0]);
        }

        private void GameModel_StepIntoBox()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p1 step up into box: fields must not change
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.BOX, gameMap[0, 1]);
            gameModel.Step(0, 0);
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.BOX, gameMap[0, 1]);
        }

        private void GameModel_StepIntoPlayer()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p1 step right into p2: fields must not change
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
            gameModel.Step(0, 3);
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
        }

        private void GameModel_StepIntoEnemy()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p1 step down into enemy: p1 must die and enemy must stay there
            Assert.AreEqual(FieldType.PLAYER1, gameMap[1, 1]);
            Assert.AreEqual(FieldType.ENEMY, gameMap[2, 1]);
            gameModel.Step(0, 1);
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 1]);
            Assert.AreEqual(FieldType.ENEMY, gameMap[2, 1]);
        }

        private void GameModel_StepIntoBomb()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p2 step right into bomb: fields must not change
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
            Assert.AreEqual(FieldType.BOMB, gameMap[1, 3]);
            gameModel.Step(1, 3);
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
            Assert.AreEqual(FieldType.BOMB, gameMap[1, 3]);
        }

        private void GameModel_StepIntoExplosion()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p2 step down into explosion: p2 must die and explosion must stay there
            Assert.AreEqual(FieldType.PLAYER2, gameMap[1, 2]);
            Assert.AreEqual(FieldType.EXPLOSION, gameMap[2, 2]);
            gameModel.Step(1, 1);
            Assert.AreEqual(FieldType.ROAD, gameMap[1, 2]);
            Assert.AreEqual(FieldType.EXPLOSION, gameMap[2, 2]);
        }

        private void GameModel_StepPlayerFromBomb()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p3 standing on bomb, step left into road: p3 must be on road and bomb3 must change to bomb
            Assert.AreEqual(FieldType.BOMB3, gameMap[3, 3]);
            Assert.AreEqual(FieldType.ROAD, gameMap[3, 2]);
            gameModel.Step(2, 2);
            Assert.AreEqual(FieldType.BOMB, gameMap[3, 3]);
            Assert.AreEqual(FieldType.PLAYER3, gameMap[3, 2]);
        }

        private void GameModel_StepIntoPowerup()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;

            // check p3 step down into powerup: powerup must be replaced with p3
            Assert.AreEqual(FieldType.BOMB3, gameMap[3, 3]);
            Assert.AreEqual(FieldType.POWERUP, gameMap[4, 3]);
            gameModel.Step(2, 1);
            Assert.AreEqual(FieldType.BOMB, gameMap[3, 3]);
            Assert.AreEqual(FieldType.PLAYER3, gameMap[4, 3]);
        }
        #endregion

        #region GameModel EnemyStep test
        [TestMethod]
        public void GameModel_EnemyStepTest()
        {
            GameModel_EnemyStep();
            GameModel_EnemyNoStep();
        }
        private void GameModel_EnemyStep()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;
            Assert.AreEqual(FieldType.ENEMY, gameModel.Map[2, 1]);
            gameModel.StartGame();
            gameModel.EnemyStep(new int[] { 2, 1 });
            Assert.AreNotEqual(FieldType.ENEMY, gameModel.Map[2, 1]);
            Assert.AreEqual(FieldType.ROAD, gameModel.Map[2, 1]);
        }
        private void GameModel_EnemyNoStep()
        {
            gameModel.Init(0, 3);
            gameMap = gameModel.Map;
            Assert.AreEqual(FieldType.ENEMY, gameModel.Map[2, 1]);
            gameModel.StartGame();
            gameModel.EnemyStep(new int[] { 0, 0 });
            Assert.AreEqual(FieldType.ENEMY, gameModel.Map[2, 1]);
        }
        #endregion

        #region GameModel Initialization tests
        [TestMethod]
        public void GameModel_ConstructorTest()
        {
            gameModel = new GameModel();
            Assert.IsInstanceOfType(gameModel.GameTime, typeof(int));
            Assert.IsInstanceOfType(gameModel.MatchLength, typeof(int));
            Assert.IsInstanceOfType(gameModel.MapId, typeof(int));
            Assert.IsInstanceOfType(gameModel.ShrinkTimeLeft, typeof(int));
            Assert.IsNull(gameModel.Timer);
            Assert.IsNull(gameModel.Map);
        }
        [TestMethod]
        public void GameModel_StartGameTest()
        {
            gameModel.Init(0, 3, 3);
            gameMap = gameModel.Map;
            gameModel.StartGame();
            Assert.IsTrue(gameModel.GameTime < 3);
            Assert.AreNotSame(gameMap, gameModel.Map);
            Assert.AreEqual(3, gameModel.MatchLength);
            Assert.AreEqual(0, gameModel.MapId);
            Assert.IsInstanceOfType(gameModel.Timer, typeof(Timer));
        }
        [TestMethod]
        public void GameModel_InitTest()
        {
            gameModel.Init(0, 3, 3);
            gameMap = gameModel.Map;
            Assert.AreSame(gameMap, gameModel.Map);
            Assert.AreEqual(3, gameModel.MatchLength);
            Assert.AreEqual(0, gameModel.MapId);

            gameModel.Init(0, 3, 5);

            Assert.AreNotSame(gameMap, gameModel.Map);
            Assert.AreEqual(5, gameModel.MatchLength);
            Assert.AreEqual(0, gameModel.MapId);
        }
        [TestMethod]
        public void GameModel_StopGameTest()
        {
            gameModel.Init(0, 3, 3);
            gameMap = gameModel.Map;
            gameModel.StartGame();
            gameModel.StopGame();
            Assert.IsInstanceOfType(gameModel.Timer, typeof(Timer));
            Assert.AreNotSame(gameMap, gameModel.Map);
            Assert.IsInstanceOfType(gameModel.GameTime, typeof(int));
            Assert.AreEqual(3, gameModel.MatchLength);
            Assert.AreEqual(0, gameModel.MapId);
        }

        #endregion

        #region GameModel PLace Bomb tests
        [TestMethod]
        public void GameModel_PlaceBombTest()
        {
            // map 0:
            // w x w w w
            // w 1 2 b w
            // w y e x w
            // w r r b3 w
            // w w w p w
            gameModel = new GameModel();
            gameModel.Init(0, 3);
            gameModel.StartGame();
            Assert.AreEqual(FieldType.PLAYER1, gameModel.Map[1, 1]);
            gameModel.PlaceBomb(new int[2] { 1, 1 }, 0);
            Assert.AreEqual(FieldType.BOMB1, gameModel.Map[1, 1]);

            gameModel.PlaceBomb(new int[2] { 1, 1 }, 1);
            Assert.AreEqual(FieldType.BOMB1, gameModel.Map[1, 1]);

        }
        [TestMethod]
        public async Task GameModel_CantPlaceBombTest()
        {
            gameModel = new GameModel();
            gameModel.Init(4, 2);
            gameModel.StartGame();


            Assert.AreEqual(FieldType.PLAYER1, gameModel.Map[1, 1]);
            gameModel.Step(0, 1);
            await Task.Delay(TimeSpan.FromSeconds(0.2));
            gameModel.PlaceBomb(new int[2] { 2, 1 }, 0);
            Assert.AreEqual(FieldType.BOMB1, gameModel.Map[2, 1]);
            gameModel.Step(0, 1);
            gameModel.PlaceBomb(new int[2] { 3, 1 }, 0);
            Assert.AreEqual(FieldType.PLAYER1, gameModel.Map[3, 1]);
        }
        #endregion

        #region Player class tests
        [TestMethod]
        public void Player_ConstructorTest()
        {
            Player p = new Player(gameModel, 1, 0);
            Assert.AreEqual(1, p.Id);
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(1, p.OldMaxBombNumber);
            Assert.AreEqual(1, p.MaxBombNumber);
            Assert.AreEqual(0, p.PlacedBombs);
            Assert.IsTrue(p.CanStep);
            Assert.AreEqual(2, p.OldRange);
            Assert.AreEqual(2, p.BombRange);
            Assert.AreEqual(0, p.Wins);
            Assert.IsTrue(p.Active);
            Assert.AreEqual(1, p.Speed);
            Assert.IsFalse(p.InstantPlacement);
        }

        [TestMethod]
        public void Player_PowerUpHandlingTest()
        {
            gameModel.Init();
            Player p = new Player(gameModel, 0, 0);

            // PLUSBOMB
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(1, p.MaxBombNumber);
            p.PickedPowerup(PowerUpType.PLUSBOMB);
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(2, p.MaxBombNumber);

            // PLUSRANGE
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(2, p.BombRange);
            p.PickedPowerup(PowerUpType.PLUSRANGE);
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(3, p.BombRange);

            // MINUSSPEED - add and remove
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(1, p.Speed);
            p.PickedPowerup(PowerUpType.MINUSSPEED);
            Assert.AreEqual(1, p.PowerUps.Count);
            Assert.AreEqual(3, p.Speed);

            PowerUp pu = p.PowerUps[0];
            p.RemovePowerUp(pu);
            pu.DestroyPowerUp();
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(1, p.Speed);

            // ONERANGE - add and remove
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(3, p.BombRange);
            p.PickedPowerup(PowerUpType.ONERANGE);
            Assert.AreEqual(1, p.PowerUps.Count);
            Assert.AreEqual(1, p.BombRange);

            pu = p.PowerUps[0];
            p.RemovePowerUp(pu);
            pu.DestroyPowerUp();
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(3, p.BombRange);

            // NOBOMB - add only
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.AreEqual(2, p.MaxBombNumber);
            p.PickedPowerup(PowerUpType.NOBOMB);
            Assert.AreEqual(1, p.PowerUps.Count);
            Assert.AreEqual(0, p.MaxBombNumber);

            // INSTANTPLACEMENT - add only
            Assert.AreEqual(1, p.PowerUps.Count);
            Assert.IsFalse(p.InstantPlacement);
            p.PickedPowerup(PowerUpType.INSTANTPLACEMENT);
            Assert.AreEqual(2, p.PowerUps.Count);
            Assert.IsTrue(p.InstantPlacement);

            // destroy all powerups
            Assert.AreEqual(0, p.MaxBombNumber);
            p.DestroyPowerups();
            Assert.AreEqual(0, p.PowerUps.Count);
            Assert.IsFalse(p.InstantPlacement);
            Assert.AreEqual(2, p.MaxBombNumber);
        }

        [TestMethod]
        public void Player_DieReviveTest()
        {
            gameModel.Init();
            Player p = new Player(gameModel, 0, 0);
            p.PickedPowerup(PowerUpType.PLUSBOMB);
            p.PickedPowerup(PowerUpType.PLUSRANGE);
            p.PickedPowerup(PowerUpType.MINUSSPEED);
            p.PickedPowerup(PowerUpType.ONERANGE);
            p.PickedPowerup(PowerUpType.NOBOMB);
            p.PickedPowerup(PowerUpType.INSTANTPLACEMENT);
            Assert.IsTrue(p.Active);
            Assert.AreEqual(2, p.OldMaxBombNumber);
            Assert.AreEqual(3, p.OldRange);
            Assert.AreEqual(3, p.Speed);
            Assert.AreEqual(1, p.BombRange);
            Assert.AreEqual(0, p.MaxBombNumber);
            Assert.IsTrue(p.InstantPlacement);
            p.Die();
            Assert.IsFalse(p.Active);
            p.Revive();
            Assert.IsTrue(p.Active);
            Assert.AreEqual(1, p.OldMaxBombNumber);
            Assert.AreEqual(2, p.OldRange);
            Assert.AreEqual(1, p.Speed);
            Assert.AreEqual(2, p.BombRange);
            Assert.AreEqual(1, p.MaxBombNumber);
            Assert.IsFalse(p.InstantPlacement);
        }

        [TestMethod]
        public void Player_SetterMethodsTest()
        {
            Player p = new Player(gameModel, 0, 0);

            // PlaceBomb, ResetBomb
            int pb = p.PlacedBombs;
            p.PlaceBomb();
            Assert.AreEqual((pb + 1), p.PlacedBombs);
            p.ResetBomb();
            Assert.AreEqual(pb, p.PlacedBombs);

            // Win, ResetWin
            Assert.AreEqual(0, p.Wins);
            p.Win();
            Assert.AreEqual(1, p.Wins);
            p.ResetWins();
            Assert.AreEqual(0, p.Wins);
        }
        #endregion

        #region GameModel Save tests
        [TestMethod]
        public void GameModel_SaveLoad()
        {
            string _path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Files\GameState.json");

            gameModel.Init(1, 3);
            gameModel.StartGame();

            gameModel.LoadSaves();

            int savesCount = gameModel.Saves.Count;
            int gameTime = gameModel.GameTime;

            gameModel.SaveGameState(1, "Test");

            string jsonString = File.ReadAllText(_path);
            string loadId = JsonConvert.DeserializeObject<List<Save>>(jsonString)!.Where(s => s.Name == "Test").First().Id;

            gameModel = new GameModel();
            gameModel.LoadSaves();
            gameModel.LoadGameState(loadId);

            Assert.AreEqual(1, gameModel.MapId);
            Assert.AreEqual(savesCount + 1, gameModel.Saves.Count);
            Assert.AreEqual(3, gameModel.PlayerNumber);
            Assert.AreEqual(gameTime, gameModel.GameTime);

            GameModel_DeleteSave(_path);
        }

        private void GameModel_DeleteSave(string _path)
        {
            gameModel.LoadSaves();

            int savesCount = gameModel.Saves.Count;

            string jsonString = File.ReadAllText(_path);
            string deleteId = JsonConvert.DeserializeObject<List<Save>>(jsonString)!.Where(s => s.Name == "Test").First().Id;
            gameModel.DeleteGameState(deleteId);


            Assert.AreEqual(savesCount - 1, gameModel.Saves.Count);
        }
        #endregion
    }
}