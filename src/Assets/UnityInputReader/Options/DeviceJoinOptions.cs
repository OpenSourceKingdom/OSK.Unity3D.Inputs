using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options
{
    /// <summary>
    /// Sets the behaviors for players when they attempt to join the game.
    /// </summary>
    public class DeviceJoinOptions
    {
        public DeviceJoinBehavior DeviceJoinBehavior { get; set; }

        /// <summary>
        /// Determines the maximum number of input controllers that can be associated to a user. If null, there is no limit.
        /// 
        /// <br />
        /// <br />
        /// Notes:
        /// <list type="bullet">
        ///     <item>Devices are added round robin until all are paired. For example, if MaxInputControllersPerPlayer is set to two with a join behavior for <see cref="DeviceJoinBehavior.DeviceActivation"/> then new devices that are activated will be assigned to each player for their input controller until the max local users allowed is reached, before attempting to add the devices to a user with a valid input controller again</item>
        ///     <item>If set to null, this will mean that all devices will be initially added to the same user if join behavior is set to <see cref="DeviceJoinBehavior.Automatic"/></item>
        ///     <item>An Input Controller is a group of <see cref="UnityEngine.InputSystem.InputDevice"/>s that represent a single input source. For example, a keyboard and mouse would act as a single input controller in most use cases, whereas a GamePad can be a single input controller on its own.</item>
        /// </list>
        /// </summary>
        public int? MaxInputControllersPerPlayer { get; set; }
    }
}
