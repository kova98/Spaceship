using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spaceship.API.Models;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;

namespace Spaceship.API.Controllers
{
    [Route("/spaceship/protocol/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameRepository gameRepository;
        private readonly IConfiguration config;

        public GameController(IGameRepository gameRepository, IConfiguration config)
        {
            this.gameRepository = gameRepository;
            this.config = config;
        }

        [HttpPost("new")]
        public ActionResult NewGame(GameNewDTO gameNewDto)
        {
            var game = new Game
            {
                PlayerId = config["PlayerId"],
                PlayerName = config["PlayerName"],
                OpponentId = gameNewDto.UserId,
                OpponentName = gameNewDto.FullName,
                OpponentHostname = gameNewDto.SpaceshipProtocol.Hostname,
                OpponentPort = gameNewDto.SpaceshipProtocol.Port
            };

            var gameId = gameRepository.CreateGame(game);

            var createdGame = new GameCreatedDTO
            {
                UserId = config["PlayerId"],
                FullName = config["PlayerName"],
                GameId = $"match-{gameId}",
                Starting = game.Starting
            };

            var uri = $"{config["UserAPI"]}/match-{gameId}";

            return Created(uri, createdGame);
        }
    }
}
