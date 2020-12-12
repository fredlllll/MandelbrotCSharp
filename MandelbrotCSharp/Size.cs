using System;

namespace MandelbrotCSharp
{
    public struct Size : IEquatable<Size>
    {
        public int x, y;

        public Size(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is Size size && Equals(size);
        }

        public bool Equals(Size size)
        {
            return x == size.x && y == size.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}
