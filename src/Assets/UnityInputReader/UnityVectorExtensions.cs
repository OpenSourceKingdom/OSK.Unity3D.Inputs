using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class UnityVectorExtensions
    {
        public static Vector2 ToNumericVector(this UnityEngine.Vector2 vector)
            => new Vector2(vector.x, vector.y);

        public static Vector3 ToNumericVector(this UnityEngine.Vector3 vector)
            => new Vector3(vector.x, vector.y, vector.z);

        public static Vector4 ToNumericVector(this UnityEngine.Vector4 vector)
            => new Vector4(vector.x, vector.y, vector.z, vector.w);
    }
}
