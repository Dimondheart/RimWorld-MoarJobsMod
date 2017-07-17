using System;

namespace MoarJobs
{
	public static class Utils
	{
		/** Shorthand for calling array.FirstIndexOf(predictate function) when looking for
		 * an exact string match.
		 */
		public static int StringFirstIndexInArray(string lookingFor, string[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == lookingFor)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
