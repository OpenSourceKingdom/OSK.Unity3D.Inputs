using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Shared
{
    [Serializable]
    public struct UnityNullable<T>
        where T : struct
    {
        #region Variables

        [SerializeField]
        private T _value;
        [SerializeField]
        private bool _hasValue;

        #endregion

        #region Nullable

        private UnityNullable(T value, bool hasValue)
        {
            _value = value;
            _hasValue = hasValue;
        }

        public T Value => _value;
        public bool HasValue => _hasValue;
        public Type UnderlyingType => typeof(T);

        public T GetValueOrDefault(T defaultValue = default) => _hasValue ? _value : defaultValue;

        public static implicit operator UnityNullable<T>(T value) => new UnityNullable<T>(value, true);

        public static implicit operator T?(UnityNullable<T> value) => value.HasValue ? (T?)value.Value : null;

        public static implicit operator UnityNullable<T>(T? value) => new UnityNullable<T>(value.GetValueOrDefault(), value.HasValue);

        #endregion
    }
}
