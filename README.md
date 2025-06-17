# OSK.Unity3D.Inputs
OSK.Inputs integration that uses Unity3D as the underlying input mechanism

The library is designed to utilize the OSK.Inputs library and uses functionality from that to interpret the inputs being read. 

There are 2 primary scripts in the repository:
* `UnityInputManager`: an input manager script that must be set in the scene needing to read input from users. It will handle creating users and pairing devices depending on the device pairing mode set
* `UnityInputSystemReader`: the main implementation for the OSK.Inputs library for Untiy3D. This input reader will read from a variety of devices to retrieve input from players. The following input devices have been tested to varying degrees:
 * Mouse
 * Keyboard
 * Dual Shock Wireless Controller
The following input devices are supported but not fully tested against the library:
 * XControllers

Along with the above primary integration points, this library also contains some helpful game object selection code that can be found in the `PointerHelper` class to retrieve objects given a pointer's location.

An example scene is included in the project to showcase how the input manager script works within a scene