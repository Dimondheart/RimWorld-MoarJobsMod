using System;

namespace MoarJobs
{
	public abstract class GroupEntry
	{
		public string name;
		protected string[] entry;

		public GroupEntry(string[] entry)
		{
			this.entry = entry;
		}

		/** Parse the entry data and validate non-string values. */
		public abstract void Finalize(SetupData data);
	}
}
