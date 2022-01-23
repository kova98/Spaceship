using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Spaceship.API.Controllers;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.ProtocolAPI.Models;
using System;
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
            var newGameDto = new GameNewDTO { SpaceshipProtocol = new SpaceshipProtocol { } };

            var result = (CreatedResult)controller.NewGame(newGameDto);
            var dto = result.Value as GameCreatedDTO;
            
            dto.Should().BeOfType<GameCreatedDTO>();
            dto.Should().NotBeNull();
            dto.UserId.Should().Be(playerId);
            dto.FullName.Should().Be(playerName);
            dto.GameId.Should().Be($"match-{gameId}");
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
