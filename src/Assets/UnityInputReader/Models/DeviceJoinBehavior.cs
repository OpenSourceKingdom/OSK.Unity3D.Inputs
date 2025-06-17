namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Models
{
    /// <summary>
    /// Determines when input devices are joined to players in the input system.
    /// </summary>
    public enum DeviceJoinBehavior
    {
        /// <summary>
        /// Will auto initialize all devices that are connected based on the controllers provided to the input system.
        /// 
        /// <br/>
        /// <br/>
        /// Notes:
        /// <list type="bullet">
        ///     <item>
        ///         An attempt will be made to join new devices to current users if it is possible, i.e. a keyboard is pressed and joined to a user and later a mouse is pressed and the input system
        ///         is configured to have a controller with keyboard and mouse, the mouse will be joined to the user that has the keyboard.
        ///     </item>
        ///     <item>
        ///         If the maximum number of users allowed to the input system has been reached, the device will not be added to the input system, and the user will not be created.
        ///     </item>
        /// </list>        
        /// </summary>
        Automatic,

        /// <summary>
        /// Players are created based on device activation. This means that when a device is activated (e.g., a gamepad button is pressed), a new player will be created for that device. 
        /// 
        /// <br/>
        /// <br/>
        /// Notes:
        /// <list type="bullet">
        ///     <item>
        ///         An attempt will be made to join new devices to current users if it is possible, i.e. a keyboard is pressed and joined to a user and later a mouse is pressed and the input system
        ///         is configured to have a controller with keyboard and mouse, the mouse will be joined to the user that has the keyboard.
        ///     </item>
        ///     <item>
        ///         If the maximum number of users allowed to the input system has been reached, the device will not be added to the input system, and the user will not be created.
        ///     </item>
        /// </list>
        /// </summary>
        DeviceActivation,

        /// <summary>
        /// The integrating application is expected to handle player joining manually. This means that the application will need to explicitly handle device pairing to users.
        /// </summary>
        Manual
    }
}
