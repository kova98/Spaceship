namespace Spaceship.UserAPI.Models
{
    public class FireResponseDTO
    {
        public Dictionary<string, string> Salvo { get; set; }
        public GameInfo Game { get; set; }
    }

    public class GameInfo
    {
        public string PlayerTurn { get; set; }
    }
}
