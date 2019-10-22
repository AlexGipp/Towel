﻿using Towel.Mathematics;
using static Towel.Syntax;

namespace Towel.Physics
{
    public class Material<T>
    {
        private T _density = Constant<T>.One;
        private T _kineticFriction = Division(Convert<int, T>(3), Convert<int, T>(10));
        private T _staticFriction = Division(Convert<int, T>(6), Convert<int, T>(10));
        private T _restitution = Constant<T>.Zero;

        public Material(
            T density,
            T kineticFriction,
            T staticFriction,
            T restitution)
        {
            this._density = density;
            this._kineticFriction = kineticFriction;
            this._staticFriction = staticFriction;
            this._restitution = restitution;
        }

        public T Density { get { return _density; } set { _density = value; } }
        public T Restitution { get { return _restitution; } set { _restitution = value; } }
        public T StaticFriction { get { return _staticFriction; } set { _staticFriction = value; } }
        public T KineticFriction { get { return _kineticFriction; } set { _kineticFriction = value; } }
    }
}
