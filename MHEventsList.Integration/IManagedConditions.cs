using System.Collections.Generic;

namespace MHEventsList.Integration;

public interface IManagedConditions
{
	bool IsValid { get; }

	string? ValidationError { get; }

	bool IsReady { get; }

	bool IsMatch { get; }

	bool IsMutable { get; }

	IEnumerable<int> UpdateContext();

	string? GetReasonNotMatched();
}
