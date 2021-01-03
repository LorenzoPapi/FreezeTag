using System;
using System.Numerics;

namespace Impostor.Api.Innersloth
{
    public class Vent : IComparable
    {
        public Vent(VentLocation id, Vector2 position)
        {
            Id = id;
            Position = position;
        }

        public VentLocation Id { get; }

        public Vector2 Position { get; }

        public int CompareTo(object obj) => Id.CompareTo(obj);
    }
}
