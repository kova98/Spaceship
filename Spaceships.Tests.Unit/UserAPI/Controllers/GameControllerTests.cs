using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.UserAPI.Controllers;
using Spaceship.UserAPI.Models;
using Spaceships.Tests.Unit.Helpers;
using System;
using System.Linq;
using Xunit;

namespace Spaceships.Tests.Unit.UserAPI.Controllers
{
    public class GameControllerTests
    {
        [Fact]
        void Fire_RegistersHitsAndReturnsCorrectShotStatuses()
        {
            const int Ship = 1;
            const int Hit = -1;
            var game = GameHelper.GetGameWithFieldSetTo((0, 1, Ship), (0, 2, Hit), (1, 3, Hit), (0, 4, Ship));
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());
            var fireDto = new FireDTO { Salvo = new string[] { "0x0", "0x1", "0x2", "0x4" } };

            var result = (OkObjectResult)controller.Fire(fireDto, "");
            var responseDto = result.Value as FireResponseDTO;

            responseDto.Should().BeOfType<FireResponseDTO>();
            responseDto.Salvo.Count.Should().Be(4);
            responseDto.Salvo["0x0"].Should().Be("miss", "the field is empty");
            responseDto.Salvo["0x1"].Should().Be("hit", "there is a ship on the field");
            responseDto.Salvo["0x2"].Should().Be("miss", "the field is already hit");
            responseDto.Salvo["0x4"].Should().Be("kill", "it is the last hit field of the ship");
        }

        [Fact]
        void Fire_RespondsWithCorrectPlayerTurn()
        {
            var game = GameHelper.GetGameWithFieldSetTo((0, 1, 1));
            game.PlayerId = "player-1";
            game.OpponentId = "opponent-1";
            game.PlayerTurn = "opponent-1";
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());
            var fireDto = new FireDTO { Salvo = new string[] { "0x0" } };

            var result = (OkObjectResult)controller.Fire(fireDto, "");
            var responseDto = result.Value as FireResponseDTO;

            responseDto.Game.PlayerTurn.Should().Be("player-1");
        }

        [Fact]
        void Status_DoesNotExist_ReturnsNotFound()
        {
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(1)).Returns(It.IsAny<Game>());
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = controller.Status("0");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        void Status_GameExists_ReturnsOkWithPopulatedModel()
        {
            var game = new Game
            {
                Id = 1,
                PlayerId = "player-1",
                OpponentId = "opponent-1",
                PlayerTurn = "player-1"
            };
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = (OkObjectResult)controller.Status("match-1");
            var responseDto = result.Value as StatusResponseDTO;

            responseDto.Should().BeOfType<StatusResponseDTO>();

            responseDto.Self.Should().NotBeNull();
            responseDto.Self.UserId.Should().Be("player-1");
            responseDto.Self.Board.Should().NotBeNull();
            responseDto.Self.Board.Length.Should().Be(16, "the grid height is 16");
            responseDto.Self.Board.First().Length.Should().Be(16, "the grid width is 16");
            responseDto.Self.Board.GroupBy(x => x.Length).Count().Should().Be(1, "all rows should be the same width");

            responseDto.Opponent.Should().NotBeNull();
            responseDto.Opponent.UserId.Should().Be("opponent-1");
            responseDto.Opponent.Board.Should().NotBeNull();
            responseDto.Opponent.Board.Length.Should().Be(16, "the grid height is 16");
            responseDto.Opponent.Board.First().Length.Should().Be(16, "the grid width is 16");
            responseDto.Opponent.Board.GroupBy(x => x.Length).Count().Should().Be(1, "all rows should be the same width");

            responseDto.Game.Should().NotBeNull();
            responseDto.Game.PlayerTurn.Should().Be("player-1");
        }


        [Fact]
        void Status_GameExists_ReturnsCorrectBoards()
        {
            const int Ship = 1;                // *                    
            const int Miss = 0;                // -
            const int Hit = -1;                // X
            const int EmptyOrUnknown = -3;     // .
            var game = GameHelper
                .GetGameWithFieldSetTo((0, 1, Ship), (0, 2, Miss), (0, 3, Hit), (0, 4, EmptyOrUnknown))
                .WithOpponentGridFieldsSetTo((0, 1, EmptyOrUnknown), (0, 2, Hit), (0, 3, Miss), (0, 4, Ship));
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = (OkObjectResult)controller.Status("");
            var responseDto = result.Value as StatusResponseDTO;

            responseDto.Self.Board.First().Should().Be("*-X.............");
            responseDto.Opponent.Board.First().Should().Be(".X-*............");
        }
    }
}
