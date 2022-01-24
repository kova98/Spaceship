﻿using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Spaceship.API.Controllers;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.ProtocolAPI.Infrastructure;
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
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(new Game { Status = GameStatus.InProgress });
            var controller = new GameController(Mock.Of<IGameRepository>(), Mock.Of<IConfiguration>());

            var result = controller.Fire(It.IsAny<FireDTO>(), "");

            result.Should().BeOfType<OkResult>();
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
        void Fire_CallsFireOnGameManager()
        {
            var gameRepoMock = new Mock<IGameRepository>();
            gameRepoMock.Setup(x => x.GetGame(It.IsAny<long>())).Returns(new Game { Status = GameStatus.InProgress });
            //var controller = new GameController(gameRepoMock.Object, Mock.Of<IConfiguration>());

            //controller.Fire(new FireDTO { Salvo = new string[]{ "5xA" } });

            //gameManagerMock.Verify(x => x.CreateGame(game), Times.Once());
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
