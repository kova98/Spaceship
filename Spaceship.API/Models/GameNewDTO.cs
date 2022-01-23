namespace Spaceship.API.Models
{
    public class GameNewDTO
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public SpaceshipProtocol SpaceshipProtocol { get; set; }
    }

    public class SpaceshipProtocol
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
    }
}
