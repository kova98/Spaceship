using Spaceship.DataAccess.Entities;

namespace Spaceship.DataAccess.Interfaces
{
    public interface IGameRepository
    {
        long CreateGame(Game game);   
        Game GetGame(long gameId);
        void UpdateGame(Game game);
    }
}
