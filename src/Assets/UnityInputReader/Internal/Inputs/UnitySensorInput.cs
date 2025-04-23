using OSK.Inputs.Models.Inputs;
using OSK.Inputs.Models.Runtime;
using OSK.Inputs.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Internal.Inputs
{
    public class UnitySensorInput: UnityInput<SensorInput, Sensor>
    {
        #region Variables

        private bool _sensorInputStarted;

        #endregion

        #region Consstructors

        public UnitySensorInput(SensorInput sensorInput, Sensor sensor)
            : base(sensorInput, sensor)
        {
            
        }

        #endregion

        #region UnityInput Overrides

        public override bool TryGetInputPhase(out InputPhase inputPhase)
        {
            if (InputControl.CheckStateIsAtDefaultIgnoringNoise())
            {
                inputPhase = InputControl.wasUpdatedThisFrame
                    ? InputPhase.End
                    : InputPhase.Start;
                _sensorInputStarted = false;

                return inputPhase == InputPhase.End;
            }

            inputPhase = _sensorInputStarted
                ? InputPhase.Active
                : InputPhase.Start;
            _sensorInputStarted = true;
            return true;
        }

        #endregion
    }
}
