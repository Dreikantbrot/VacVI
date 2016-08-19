using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoVI.classes.math
{
    public class Vector3D
    {
        #region Variables
        private double _x;
        protected double _y;
        protected double _z;
        #endregion


        #region Properties
        /// <summary> Returns or sets the x value.
        /// </summary>
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }


        /// <summary> Returns or sets the y value.
        /// </summary>
        public double Y
        {
            get { return _x; }
            set { _x = value; }
        }


        /// <summary> Returns or sets the z value.
        /// </summary>
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }


        /// <summary> Returns the vector's pitch.
        /// </summary>
        public double Pitch
        {
            get { return Math.Atan2(_z, _x); }
        }


        /// <summary> Returns the vector's azimuth.
        /// </summary>
        public double Azimuth
        {
            get { return Math.Atan2(_y, _x); }
        }
        #endregion

        #region Constructors
        public Vector3D(double pX, double pY, double pZ)
        {
            this._x = pX;
            this._y = pY;
            this._z = pZ;
        }

        public Vector3D() : this(0, 0, 0) { }
        #endregion


        #region Functions
        /// <summary> Returns the vector's length.
        /// </summary>
        /// <returns>The vector's length.</returns>
        public double GetLength()
        {
            if ((_x == 0) && (_y == 0) && (_z == 0)) { return Math.Sqrt(Math.Pow(_x, 2) + Math.Pow(_y, 2) + Math.Pow(_z, 2)); }

            return 0;
        }


        /// <summary> Sets a vector's length.
        /// </summary>
        /// <param name="length">The desired new length.</param>
        public void SetLength(double length)
        {
            double ratio = length / GetLength();

            _x *= ratio;
            _y *= ratio;
            _z *= ratio;
        }


        /// <summary> Normalizes the vector.
        /// </summary>
        public void Normalize()
        {
            this.SetLength(1);
        }


        /// <summary> Returns the cross product to another vector.
        /// </summary>
        /// <param name="vect2">The second vector.</param>
        /// <returns></returns>
        public double Dot(Vector3D vect2)
        {
            return ((this.X * vect2.X) + (this.Y * vect2.Y) + (this.Z * vect2.Z));
        }


        /// <summary> Returns the distance to another vector.
        /// </summary>
        /// <param name="vect2">The destination vector.</param>
        /// <returns>The distance to the destination vector.</returns>
        public double GetDistance(Vector3D destVect)
        {
            double deltaX = destVect.X - this.X;
            double deltaY = destVect.Y - this.Y;
            double deltaZ = destVect.Z - this.Z;

            if ((deltaX == 0) && (deltaY == 0) && (deltaZ == 0))
            {
                return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
            }

            return 0;
        }
        #endregion
    }
}
