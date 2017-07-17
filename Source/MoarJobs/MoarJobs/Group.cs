using System;
using System.Collections.Generic;

namespace MoarJobs
{
	public class Group
	{
		/** The name of this group within its parent group. */
		public readonly string localName;
		/** The parent group of this group. Is null if this group does not
		 * have a parent.
		 */
		public readonly Group parent;
		private readonly List<GroupEntry> entries;
		private IList<GroupEntry> entriesAsReadOnly;

		/** The full name of this group. */
		public string FullName
		{
			get
			{
				if (IsBaseGroup)
				{
					return localName;
				}
				return parent.FullName + "." + localName;
			}
		}

		public IList<GroupEntry> Entries
		{
			get
			{
				if (entriesAsReadOnly == null)
				{
					entriesAsReadOnly = entries.AsReadOnly();
				}
				return entriesAsReadOnly;
			}
		}

		public bool IsBaseGroup
		{
			get
			{
				return parent == null;
			}
		}

		public Group(string name) : this(name, null)
		{
		}

		public Group(string localName, Group parent)
		{
			this.localName = localName;
			this.parent = parent;
			entries = new List<GroupEntry>();
		}

		public bool AddEntry(GroupEntry entry)
		{
			if (entries.Contains(entry))
			{
				return false;
			}
			entries.Add(entry);
			parent.AddEntry(entry);
			return true;
		}
	}
}
