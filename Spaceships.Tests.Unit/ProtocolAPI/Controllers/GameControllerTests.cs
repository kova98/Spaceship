using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Spaceship.API.Controllers;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.ProtocolAPI.Infrastructure;
using Spaceship.ProtocolAPI.Models;
using System;
using System.Linq;
using Xunit;

namespace Spaceships.Tests.Unit.ProtocolAPI.Controllers
{
    public class GameControllerTests
    {
        [Fact]
        void NewGame_ReturnsCreatedResult()
        {
            var controller = new GameController(Mock.Of<IGameRepository>(), Mock.Of<IConfiguration>());
            var newGameDto = new GameNewDTO{ SpaceshipProtocol = new SpaceshipProtocol{}};

            var result = controller.NewGame(newGameDto) as CreatedResult;

            result.Should().BeOfType<CreatedResult>();
        }

        [Theory]
        [InlineData("player", "Assessment Player", 123)]
        [InlineData("a", "b", 0)]
        void NewGame_ReturnsCorrectDto(string playerId, string playerName, long gameId)
        {
            var mockConfig = GetMockConfig(playerId, playerName);
            var mockGameRepo = GetMockGameRepo(gameId);
            var controller = new GameController(mockGameRepo, mockConfig);
            var newGameDto = new GameNewDTO { SpaceshipProtocol = new SpaceshipProtocol { }, UserId = "opponent-1" };

            var result = (CreatedResult)controller.NewGame(newGameDto);
            var dto = result.Value as GameCreatedDTO;
            
            dto.Should().BeOfType<GameCreatedDTO>();
            dto.Should().NotBeNull();
            dto.UserId.Should().Be(playerId);
            dto.FullName.Should().Be(playerName);
            dto.GameId.Should().Be($"match-{gameId}");
            dto.Starting.Should().Be($"opponent-1");
        }

        [Fact]
        void Fire_GameInProgress_ReturnsOk()
        {
            var game = GetGameWithFieldSetTo((0, 0, 0));
            game.Status = GameStatus.InProgress;
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = controller.Fire(new FireDTO { Salvo = new string[] { } }, "") as OkObjectResult;
            var responseDto = result.Value as FireResponseDTO;

            result.Should().BeOfType<OkObjectResult>();
            responseDto.Game.Should().BeNull("the game is still in progress");
        }

        [Fact]
        void Fire_GameFinished_ReturnsNotFound()
        {
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(new Game { Status = GameStatus.Finished });
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = controller.Fire(It.IsAny<FireDTO>(), "");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        void Fire_ProcessesShots()
        {
            const int Ship = 1;
            var game = GetGameWithFieldSetTo((0, 1, Ship), (0, 2, Ship));
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());
            var fireDto = new FireDTO { Salvo = new string[] { "0x0", "8x4", "DxA"} };

            var result = (OkObjectResult)controller.Fire(fireDto, "");
            var responseDto = result.Value as FireResponseDTO;

            responseDto.Should().BeOfType<FireResponseDTO>();
            responseDto.Salvo.Count.Should().Be(3);
            responseDto.Salvo["0x0"].Should().NotBeNull();
            responseDto.Salvo["8x4"].Should().NotBeNull();
            responseDto.Salvo["DxA"].Should().NotBeNull();
        }

        [Fact]
        void Fire_RegistersHitsAndReturnsCorrectShotStatuses()
        {                                                    
            const int Ship = 1;                              
            const int Hit = -1;
            var game = GetGameWithFieldSetTo((0, 1, Ship), (0, 2, Hit), (1, 3, Hit), (0, 4, Ship));
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
        void Fire_UpdatesGridAndGame()
        {
            var game = GetGameWithFieldSetTo((0, 0, 1));
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            controller.Fire(new FireDTO { Salvo = new string[] { "0x0" } }, "");

            game.PlayerGrid.Should().StartWith("-2", "the first field was hit and destroyed");
            gameRepoMock.Verify(x => x.UpdateGame(game));
        }

        [Fact]
        void Fire_LastShipDestroyed_ReturnsWonWithOpponentIdAndFinishesGame()
        {
            var game = GetGameWithFieldSetTo((0, 0, 1));
            game.OpponentId = "opponent-1";
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(game);
            var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            var result = (OkObjectResult)controller.Fire(new FireDTO { Salvo = new string[] { "0x0" } }, "");
            var responseDto = result.Value as FireResponseDTO;

            responseDto.Game.Won.Should().Be("opponent-1");
            game.Status.Should().Be(GameStatus.Finished);
            gameRepoMock.Verify(x => x.UpdateGame(game));
        }

        private Game GetGameWithFieldSetTo(params (int x, int y, int value)[] fields)
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

        private IGameRepository GetMockGameRepo(long gameId)
        {
            var mockRepo = new Mock<IGameRepository>();
            mockRepo.Setup(x => x.CreateGame(It.IsAny<Game>())).Returns(gameId);

            return mockRepo.Object;
        }

        private IConfiguration GetMockConfig(string playerId = "", string playerName = "", string userApi = "")
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.SetupGet(x => x[It.Is<string>(s => s == "PlayerId")]).Returns(playerId);
            mockConfig.SetupGet(x => x[It.Is<string>(s => s == "PlayerName")]).Returns(playerName);
            mockConfig.SetupGet(x => x[It.Is<string>(s => s == "userAPi")]).Returns(userApi);

            return mockConfig.Object;
        }
    }
}
