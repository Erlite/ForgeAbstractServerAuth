// This class is meant to be global, one because Sburb is EVERYWHERE, and two, ease of use.
public static class SBURB
{
	public struct Constants
	{
		public const float GRIST_MAX_SIZE = 2f;
		public const float GRIST_MIN_SIZE = 1f;

		/// <summary>
        /// The maximum distance between the server's and client's interpretation of an object before reconciliation.
        /// </summary>
        public const float MAX_RECONCILIATION_DISTANCE = 0.5f;
	}
}