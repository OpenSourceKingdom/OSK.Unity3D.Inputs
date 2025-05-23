using UnityEngine.EventSystems;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options
{
	public class UISelectionOptions
	{
		#region Variables

		/// <summary>
		/// The preferred event system to use for selecting UI elements
		/// </summary>
		public EventSystem EventSystem { get; set; }

		/// <summary>
		/// The layer masks to use for the UI selection. This is used to determine which layers should be considered for UI selection.
		/// </summary>
		public int[] LayerFilter { get; set; }

		#endregion
	}
}