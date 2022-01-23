using Spaceship.DataAccess.Entities;

namespace Spaceship.DataAccess.Interfaces
{
    public interface IGameRepository
    {
        long CreateGame(Game game);   
    }
}
