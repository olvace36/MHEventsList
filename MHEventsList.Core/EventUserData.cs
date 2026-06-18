using System.Collections.Generic;

namespace MHEventsList.Core;

public sealed class EventUserData
{
	public List<string> PinnedEventIds { get; set; } = new List<string>();
}
