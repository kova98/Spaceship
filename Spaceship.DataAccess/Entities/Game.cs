namespace Spaceship.DataAccess.Entities
{
    public class Game
    {
        public long Id { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string OpponentId { get; set; }
        public string OpponentName { get; set; }
        public string OpponentHostname { get; set; }
        public int OpponentPort { get; set; }
        public string Starting { get; set; }
        public GameStatus Status { get; set; }
        public string PlayerGrid { get; set; }
        public string OpponentGrid { get; set; }
    }

    public enum GameStatus
    {
        InProgress,
        Finished
    }
}
