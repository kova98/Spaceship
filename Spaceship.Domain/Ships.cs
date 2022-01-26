namespace Spaceship.Domain
{
    public static class Ships
    {
        public static int[,] GetShip(ShipType type, ShipOrientation orientation)
        {
            var ship = type switch
            {
                ShipType.Winger => Winger,
                ShipType.Angle => Angle,
                ShipType.AClass => AClass,
                ShipType.BClass => BClass,
                ShipType.SClass => SClass,
                _ => throw new ArgumentException("Invalid ShipType", nameof(type)),
            };

            for (int i = 0; i < (int)orientation; i++)
            {
                ship = RotateClockwise(ship);
            }

            return ship;
        }

        private static int[,] Winger => new int[,] {
            { 1, 0, 1 },
            { 1, 0, 1 },
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 0, 1 }};

        private static int[,] Angle => new int[,] { 
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 0, 0 },
            { 1, 1, 1 },
            { 0, 0, 0 }};

        private static int[,] AClass => new int[,] {
            { 0, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 1 },
            { 1, 0, 1 },
            { 0, 0, 0 }};

        private static int[,] BClass => new int[,] {
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 0 },
            { 1, 0, 1 },
            { 1, 1, 0 }};

        private static int[,] SClass => new int[,] {
            { 0, 1, 1 },
            { 1, 0, 0 },
            { 0, 1, 1 },
            { 0, 0, 1 },
            { 1, 1, 0 }};

        private static int[,] RotateClockwise(int[,] src)
        {
            int width;
            int height;
            int[,] dst;

            width = src.GetUpperBound(0) + 1;
            height = src.GetUpperBound(1) + 1;
            dst = new int[height, width];

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int newRow;
                    int newCol;

                    newRow = col;
                    newCol = height - (row + 1);

                    dst[newCol, newRow] = src[col, row];
                }
            }

            return dst;
        }
    }

    public enum ShipType { Winger, Angle, AClass, BClass, SClass }
    public enum ShipOrientation { Top, Left, Bottom, Right }
}
