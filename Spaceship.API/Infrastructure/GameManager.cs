using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;

namespace Spaceship.ProtocolAPI.Infrastructure
{
    public class GameManager
    {
        private IGameRepository gameRepository;
        private Game game;

        public GameManager(IGameRepository gameRepository, Game game)
        {
            this.gameRepository = gameRepository;
            this.game = game;
        }

        public void InitializeGame()
        {
            game.OpponentGrid = RandomlyPopulateGridWithShips();
            game.PlayerGrid = RandomlyPopulateGridWithShips();
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

            var grid = new int[16, 16];
            var possibleSpots = new Stack<(int x, int y)>(GetRandomSpots());

            foreach (var ship in ships)
            {
                grid = PlaceShipOnGrid(ship, grid, possibleSpots.Pop());
            }

            var flattened = grid.Cast<int>().ToArray();

            return string.Join(',', flattened);
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
