using FSEngine.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FSEngine.Geometry
{

    public struct Direction
    {
        public Direction(int x, int y) { this.x = x; this.y = y; }
        public int x;
        public int y;

        public static bool operator !=(Direction a, Direction b)
        {
            return !(a.x == b.x && a.y == b.y);
        }

        public static bool operator ==(Direction a, Direction b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static Direction operator *(Direction direction, int multiplier)
        {
            return new Direction(direction.x * multiplier, direction.y * multiplier);
        }

        public static Direction operator +(Direction a, Direction b)
        {
            return new Direction(a.x + b.x, a.y + b.y);
        }

        public static Direction MakeDirection(int x, int y) { return new Direction(x, y); }

        public static Direction East() { return MakeDirection(1, 0); }
        public static Direction Northeast() { return MakeDirection(1, 1); }
        public static Direction North() { return MakeDirection(0, 1); }
        public static Direction Northwest() { return MakeDirection(-1, 1); }
        public static Direction West() { return MakeDirection(-1, 0); }
        public static Direction Southwest() { return MakeDirection(-1, -1); }
        public static Direction South() { return MakeDirection(0, -1); }
        public static Direction Southeast() { return MakeDirection(1, -1); }
    }
    public class Result
    {
        public int initialX = -1;
        public int initialY = -1;
        public List<Direction> directions = new List<Direction>();
        public List<Vector2> vertices = new List<Vector2>();
    }
    public static class MarchingSquares
    {
      
        public static bool isSet(int x, int y, int width, int height, byte[] data)
        {
            return x <= 0 || x > width || y <= 0 || y > height
                ? false
                : data[(y - 1) * width + (x - 1)] != 0;
        }

        public static int value(int x, int y, int width, int height, byte[] data)
        {
            int sum = 0;
            if (isSet(x, y, width, height, data)) sum |= 1;
            if (isSet(x + 1, y, width, height, data)) sum |= 2;
            if (isSet(x, y + 1, width, height, data)) sum |= 4;
            if (isSet(x + 1, y + 1, width, height, data)) sum |= 8;
            return sum;
        }
        public static Result FindPerimeter(int initialX, int initialY, int width, int height, byte[] data)
        {
            if (initialX < 0) initialX = 0;
            if (initialX > width) initialX = width;
            if (initialY < 0) initialY = 0;
            if (initialY > height) initialY = height;

            int initialValue = value(initialX, initialY, width, height, data);
            if (initialValue == 0 || initialValue == 15)
            {
                //std::ostringstream error;
                //error << "Supplied initial coordinates (" << initialX << ", " << initialY << ") do not lie on a perimeter.";
                Console.WriteLine($"Supplied initial coordinates ({initialX}, {initialY}) do not lie on a perimeter.");
                //throw std::runtime_error(error.str());
                return new Result();
            }

            Result result = new Result();

            int x = initialX;
            int y = initialY;
            Direction previous = Direction.MakeDirection(0, 0);

            do
            {
                Direction direction;
                switch (value(x, y, width, height, data))
                {
                    case 1: direction = Direction.North(); break;
                    case 2: direction = Direction.East(); break;
                    case 3: direction = Direction.East(); break;
                    case 4: direction = Direction.West(); break;
                    case 5: direction = Direction.North(); break;
                    case 6: direction = previous == Direction.North() ? Direction.West() : Direction.East(); break;
                    case 7: direction = Direction.East(); break;
                    case 8: direction = Direction.South(); break;
                    case 9: direction = previous == Direction.East() ? Direction.North() : Direction.South(); break;
                    case 10: direction = Direction.South(); break;
                    case 11: direction = Direction.South(); break;
                    case 12: direction = Direction.West(); break;
                    case 13: direction = Direction.North(); break;
                    case 14: direction = Direction.West(); break;
                    default: throw new Exception("Illegal state");
                }
                if (direction == previous)
                {
                    // compress
                    result.directions[result.directions.Count - 1] += direction;
                }
                else
                {
                    result.directions.Add(direction);
                    previous = direction;
                }
                x += direction.x;
                y -= direction.y; // accommodate change of basis
                result.vertices.Add(new Vector2(x,y));
            } while (x != initialX || y != initialY);

            result.initialX = initialX;
            result.initialY = initialY;

            return result;
        }
        
       
    }
}
