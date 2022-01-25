using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Spaceship.UserAPI.Controllers
{
    [Route("spaceship/user/game")]
    [ApiController]
    public class GameController : ControllerBase
    {
        [HttpPut("{gameId}/fire")]
        public ActionResult Fire(string gameId)
        {
            return null;
        }
    }
}
