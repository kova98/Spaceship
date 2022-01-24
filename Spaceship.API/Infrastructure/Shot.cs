namespace Spaceship.ProtocolAPI.Infrastructure
{
    public class Shot
    {
        public ShotStatus Status { get; set; }
        public (int X, int Y) Location { get; set; }
    }

    public enum ShotStatus { Hit, Kill, Miss }
}