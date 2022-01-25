using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.ProtocolAPI.Infrastructure;
using Spaceship.ProtocolAPI.Models;

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

            var gameManager = new GameManager(gameRepository, game);
            gameManager.InitializeGame();

            var createdGameDto = new GameCreatedDTO
            {
                UserId = config["PlayerId"],
                FullName = config["PlayerName"],
                GameId = $"match-{game.Id}",
                Starting = game.Starting
            };

            var uri = $"{config["UserAPI"]}/match-{game.Id}";

            return Created(uri, createdGameDto);
        }

        [HttpPut("{gameId}")]
        public ActionResult Fire([FromBody] FireDTO fireDto, string gameId)
        {
            var parsedGameId = ParseId(gameId);
            var game = gameRepository.GetGame(parsedGameId);
            var gameManager = new GameManager(gameRepository, game);

            if (game.Status == GameStatus.Finished)
            {
                return NotFoundWithMissedSalvo(gameManager, game, fireDto);
            }

            var shots = gameManager.Fire(fireDto.Salvo);

            var fireResponseDto = new FireResponseDTO
            {
                Salvo = ShotsToDictionary(shots),
                Game = game.Status == GameStatus.Finished 
                    ? new GameInfo { Won = game.Winner } 
                    : null
            };

            return Ok(fireResponseDto);
        }

        private ActionResult NotFoundWithMissedSalvo(GameManager gameManager, Game game, FireDTO fireDto)
        {
            var response = new FireResponseDTO
            {
                Game = new GameInfo { Won = game.Winner },
                Salvo = ShotsToDictionary(gameManager.GetMissedShots(fireDto.Salvo)),
            };

            return NotFound(response);
        }

        private Dictionary<string, string> ShotsToDictionary(List<Shot> shots)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var shot in shots)
            {
                var x = shot.Location.X.ToString("X");
                var y = shot.Location.Y.ToString("X");
                var location = $"{x}x{y}";
                dictionary.Add(location, shot.Status.ToString().ToLower());
            }

            return dictionary;
        }

        private long ParseId(string gameId)
        {
            var idString = gameId.Split('-').LastOrDefault();
            long.TryParse(idString, out long id);
            return id;
        }
    }
}
