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
    }
}
