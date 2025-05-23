using UnityEngine;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts
{
    /// <summary>
    /// This behavior is meant to be a 'flag' of sorts, used in conjunction with the <see cref="PointerHelper"/>. Using this script, game objects
    /// can be set to block the pointer casts directly, even if not on a UI layer
    /// </summary>
    public class PointerSelectionBlocker : MonoBehaviour
    {
        [SerializeField]
        private bool _blockPointer = true;

        public bool BlockPointerCast
        {
            get => _blockPointer;
            set => _blockPointer = value;
        }
    }
}