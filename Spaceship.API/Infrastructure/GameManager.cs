using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;

namespace Spaceship.ProtocolAPI.Infrastructure
{
    public class GameManager
    {
        private IGameRepository gameRepository;
        private Game game;

        const int GridSize = 16;

        public GameManager(IGameRepository gameRepository, Game game)
        {
            this.gameRepository = gameRepository;
            this.game = game;
        }

        public void InitializeGame()
        {
            game.OpponentGrid = RandomlyPopulateGridWithShips();
            game.PlayerGrid = RandomlyPopulateGridWithShips();
            game.Starting = game.OpponentId;
            game.Id = gameRepository.CreateGame(game);
        }

        private string RandomlyPopulateGridWithShips()
        {
            var ships = new List<int[,]>();
            var shipTypes = GetShuffledShipTypes();  

            foreach (var shipType in shipTypes)
            {
                var ship = Ships.GetShip(shipType, RandomOrientation());
                ships.Add(ship);
            }

            var grid = new int[GridSize, GridSize];
            var possibleSpots = new Stack<(int x, int y)>(GetRandomSpots());

            for (int i = 0; i < ships.Count; i++)
            {
                SetShipId(ships[i], i + 1);
                grid = PlaceShipOnGrid(ships[i], grid, possibleSpots.Pop());
            }

            var flattened = grid.Cast<int>().ToArray();

            return string.Join(',', flattened);
        }

        private void SetShipId(int[,] ship, int id)
        {
            var width = ship.GetUpperBound(0);
            var height = ship.GetUpperBound(1);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (ship[i,j] == 1)
                    {
                        ship[i,j] = id;
                    }
                }
            }
        }

        private int[,] PlaceShipOnGrid(int[,] ship, int[,] grid, (int x, int y) spot)
        {
            var width = ship.GetUpperBound(0) + 1; // Add 1 because GetUpperBound() returns the index.
            var height = ship.GetUpperBound(1) + 1;

            // Iterate through the grid...
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // and place the ship tiles, taking into consideration the offset of the spot
                    grid[x + spot.x, y + spot.y] = ship[x, y];
                }
            }

            return grid;
        }

        internal void FinishGame()
        {
            game.Status = GameStatus.Finished;
            gameRepository.UpdateGame(game);
        }

        public bool AllShipsDestroyed()
        {
            var gridFields = game.PlayerGrid.Split(',').Select(x => int.Parse(x));
            return gridFields.Any(x => x > 0) == false;
        }

        public List<Shot> Fire(string[] salvo)
        {
            var shots = GetShotsFromSalvo(salvo);
            ProcessShots(shots);
            gameRepository.UpdateGame(game);

            return shots;
        }

        private void ProcessShots(List<Shot> shots)
        {
            var grid = GetGrid();

            foreach (var shot in shots)
            {
                var locationValue = grid[shot.Location.X, shot.Location.Y];

                // If there is no ship or the location had already been hit 
                if (locationValue <= 0)
                {
                    shot.Status = ShotStatus.Miss;
                }
                // if the location is a part of the ship not previously hit
                else if (locationValue > 0)
                {
                    shot.Status = Killed(shot, grid) ? ShotStatus.Kill : ShotStatus.Hit;
                    grid[shot.Location.X, shot.Location.Y] = (int)shot.Status;
                }
            }

            SaveGrid(grid);
        }

        private void SaveGrid(int[,] grid)
        {
            var flattened = grid.Cast<int>().ToArray();
            game.PlayerGrid = string.Join(',', flattened);
        }

        private bool Killed(Shot shot, int[,] grid)
        {
            var shipId = grid[shot.Location.X, shot.Location.Y];

            var intactShipParts = new List<(int, int)>();
            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    if (grid[i, j] == shipId)
                    {
                        intactShipParts.Add((i, j));
                    }
                }
            }

            // If this is the last intact part
            return intactShipParts.Count == 1;
        }

        private int[,] GetGrid()
        {
            var gridFields = game.PlayerGrid.Split(',').Select(x => int.Parse(x));
            var queue = new Queue<int>(gridFields);
            var grid = new int[GridSize, GridSize];

            for (int i = 0; i < GridSize; i++)
            {
                for (int j = 0; j < GridSize; j++)
                {
                    grid[i, j] = queue.Dequeue();
                }
            }

            return grid;
        }

        private List<Shot> GetShotsFromSalvo(string[] salvo)
        {
            var shots = new List<Shot>();
            foreach (var shotString in salvo)
            {
                var split = shotString.Split('x');
                var x = int.Parse(split[0], System.Globalization.NumberStyles.HexNumber);
                var y = int.Parse(split[1], System.Globalization.NumberStyles.HexNumber);

                shots.Add(new Shot { Location = new(x, y) });
            }

            return shots;
        }


        private (int x, int y)[] GetRandomSpots()
        {
            // Preset grid coordinates to keeep it simple
            (int x, int y)[] possibleSpots = new[]{
                (0, 0),  (0, 5),  (0, 10),
                (5, 0),  (5, 5),  (5, 10),
                (10, 0), (10, 5), (10, 10)};

            var rnd = new Random();
            var shuffled = possibleSpots.OrderBy(a => rnd.Next()).ToList();
            var totalShipTypes = Enum.GetValues(typeof(ShipType)).Length;

            return possibleSpots.Take(totalShipTypes).ToArray();
        }

        private ShipType[] GetShuffledShipTypes()
        {
            var rnd = new Random();
            var shipTypes = (ShipType[]) Enum.GetValues(typeof(ShipType));
            var shuffled = shipTypes.OrderBy(a => rnd.Next()).ToArray();
            return shuffled;
        }

        private ShipOrientation RandomOrientation()
        {
            var orientations = (ShipOrientation[])Enum.GetValues(typeof(ShipOrientation));
            var index = new Random().Next(orientations.Length);
            return orientations[index];
        }

    }
}
