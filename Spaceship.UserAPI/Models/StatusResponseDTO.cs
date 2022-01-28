namespace Spaceship.UserAPI.Models
{
    public class StatusResponseDTO
    {
        public BoardInfo Self { get; set; }
        public BoardInfo Opponent { get; set; }
        public GameInfo Game { get; set; }
    }

    public class BoardInfo
    {
        public string UserId { get; set; }
        public string[] Board { get; set; }
    }

}
