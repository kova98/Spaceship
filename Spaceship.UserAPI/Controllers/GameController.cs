using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Spaceship.DataAccess.Entities;
using Spaceship.DataAccess.Interfaces;
using Spaceship.Domain;
using Spaceship.UserAPI.Models;

namespace Spaceship.UserAPI.Controllers
{
    [Route("spaceship/user/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private IGameRepository gameRepository;
        private IConfiguration config;

        public GameController(IGameRepository gameRepository, IConfiguration config)
        {
            this.gameRepository = gameRepository;
            this.config = config;
        }

        [HttpPut("{gameId}/fire")]
        public ActionResult Fire(FireDTO fireDto, string gameId)
        {
            var parsedGameId = ParseId(gameId);
            var game = gameRepository.GetGame(parsedGameId);
            var gameManager = new GameManager(gameRepository, game);

            var shots = gameManager.Fire(fireDto.Salvo);

            var fireResponseDto = new FireResponseDTO
            {
                Salvo = ShotsToDictionary(shots),
                Game = new GameInfo { PlayerTurn = game.PlayerTurn }
            };

            return Ok(fireResponseDto);
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
