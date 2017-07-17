using System;

namespace MoarJobs
{
	public class GroupEntry_String : GroupEntry
	{
		public GroupEntry_String(string[] entry) : base(entry)
		{
		}

		public override void Finalize(SetupData data)
		{
			name = entry[0];
		}
	}
}
