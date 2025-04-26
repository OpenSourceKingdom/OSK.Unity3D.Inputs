using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.Models.Inputs;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs;

public class UnityCombinationInput : UnityInput
{
    #region Variables

    private readonly UnityInput[] _inputs;

    #endregion

    #region Constructors

    public UnityCombinationInput(IEnumerable<UnityInput> inputs)
    {
        _inputs = inputs.ToArray();
    }

    #endregion

    #region UnityInput Overrides

    public override bool TryGetInputPhase(out InputPhase inputPhase)
    {
        inputPhase = InputPhase.Start;
        if (!_inputs.Any())
        {
            return false;
        }

        foreach (var input in _inputs)
        {
            if (!input.TryGetInputPhase(out InputPhase newPhase))
            {
                return false;
            }

            if ((int)newPhase > (int)inputPhase)
            {
                inputPhase = newPhase;
            }
        }

        return true;
    }

    #endregion
}