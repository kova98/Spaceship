namespace Spaceship.ProtocolAPI.Infrastructure
{
    public class Shot
    {
        public ShotStatus Status { get; set; }
        public (int X, int Y) Location { get; set; }
    }

    public enum ShotStatus 
    { 
        Kill = -2, 
        Hit = -1, 
        Miss = 0,
    }
}