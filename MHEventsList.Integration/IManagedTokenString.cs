using System.Collections.Generic;

namespace MHEventsList.Integration;

public interface IManagedTokenString
{
	bool IsValid { get; }

	string? ValidationError { get; }

	bool IsReady { get; }

	string? Value { get; }

	IEnumerable<int> UpdateContext();
}
