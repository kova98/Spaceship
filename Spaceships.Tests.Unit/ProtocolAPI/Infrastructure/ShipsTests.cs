using FluentAssertions;
using Spaceship.ProtocolAPI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Spaceships.Tests.Unit.ProtocolAPI.Infrastructure
{
    public class ShipsTests
    {
        [Theory]
        [InlineData(ShipType.Winger, "1,0,1,1,0,1,0,1,0,1,0,1,1,0,1")]
        [InlineData(ShipType.Angle, "1,0,0,1,0,0,1,0,0,1,1,1,0,0,0")]
        [InlineData(ShipType.AClass, "0,1,0,1,0,1,1,1,1,1,0,1,0,0,0")]
        [InlineData(ShipType.BClass, "1,1,0,1,0,1,1,1,0,1,0,1,1,1,0")]
        [InlineData(ShipType.SClass, "0,1,1,1,0,0,0,1,1,0,0,1,1,1,0")]
        void GetShip_ReturnsCorrectShip(ShipType shipType, string shipAsArray)
        {
            var ship = Ships.GetShip(shipType, ShipOrientation.Top);
            var flattened = ship.Cast<int>().ToArray();
            var shipString = string.Join(',', flattened);

            shipString.Should().Be(shipAsArray);
        }

        //    Top          Left       Bottom         Right  
        //  1, 0, 0   0, 1, 1, 1, 1   0, 0, 0    0, 0, 0, 1, 0 
        //  1, 0, 0   0, 1, 0, 0, 0   1, 1, 1    0, 0, 0, 1, 0     
        //  1, 0, 0   0, 1, 0, 0, 0   0, 0, 1    1, 1, 1, 1, 0   
        //  1, 1, 1                   0, 0, 1
        //  0, 0, 0                   0, 0, 1    

        [Theory]
        [InlineData(ShipOrientation.Top, "1,0,0,1,0,0,1,0,0,1,1,1,0,0,0")]
        [InlineData(ShipOrientation.Right, "0,1,1,1,1,0,1,0,0,0,0,1,0,0,0")]
        [InlineData(ShipOrientation.Bottom, "0,0,0,1,1,1,0,0,1,0,0,1,0,0,1")]
        [InlineData(ShipOrientation.Left, "0,0,0,1,0,0,0,0,1,0,1,1,1,1,0")]
        void GetShip_ReturnsCorrectOrientation(ShipOrientation shipOrientation, string shipAsArray)
        {
            var ship = Ships.GetShip(ShipType.Angle, shipOrientation);
            var flattened = ship.Cast<int>().ToArray();
            var shipString = string.Join(',', flattened);

            shipString.Should().Be(shipAsArray);
        }
    }
}
