using Spaceship.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spaceships.Tests.Unit.Helpers
{
    internal static class GameHelper
    {
        internal static Game GetGameWithFieldSetTo(params (int x, int y, int value)[] fields)
        {
            var grid = new int[16, 16];

            foreach (var field in fields)
            {
                grid[field.x, field.y] = field.value;
            }

            var flattened = grid.Cast<int>().ToArray();
            var gridString = string.Join(',', flattened);
            var game = new Game { PlayerGrid = gridString };

            return game;
        }
    }
}
