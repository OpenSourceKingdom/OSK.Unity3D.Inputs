using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Events
{
    public class PlayerJoinedEvent
    {
        public Player NewPlayer { get; internal set; }
    }
}
