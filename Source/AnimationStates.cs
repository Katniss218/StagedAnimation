
namespace StagedAnimation
{
	public enum AnimationStates
	{
		/// <summary>
		/// Indicates that the animation is ready to be triggered.
		/// </summary>
		READY,
		/// <summary>
		/// Indicates that the animation is currently playing.
		/// </summary>
		MOVING,
		/// <summary>
		/// Indicates that the animation has finished playing and it should not be triggered again.
		/// </summary>
		DISABLED
	}
}