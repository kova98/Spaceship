using FluentAssertions;
using Moq;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Spaceships.Tests.Unit.ProtocolAPI.Infrastructure
{
    public class GameManagerTests
    {
        [Fact]
        void InitializeGame_InitializesGrids()
        {
            // 16 columns + 16 commas per column except the last one, which has 15
            var gridStringLength = (16 * 16) + (15 * 16) + 15; 
            var game = new Game();
            var gameManager = new GameManager(Mock.Of<IGameRepository>(), game);

            gameManager.InitializeGame();

            game.PlayerGrid.Should().NotBeNullOrEmpty();
            game.PlayerGrid.Length.Should().Be(gridStringLength);
            game.OpponentGrid.Should().NotBeNullOrEmpty();
            game.OpponentGrid.Length.Should().Be(gridStringLength);
        }

        [Fact]
        void InitializeGame_CreatesGame()
        {
            var game = new Game();
            var gameRepoMock = new Mock<IGameRepository>();
            var gameManager = new GameManager(gameRepoMock.Object, game);

            gameManager.InitializeGame();

            gameRepoMock.Verify(x => x.CreateGame(game), Times.Once());
        }
    }
}
