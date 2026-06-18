using System.Collections.Generic;
using StardewModdingAPI;

namespace MHEventsList.Integration;

public interface IContentPatcherAPI
{
	bool IsConditionsApiReady { get; }

	IManagedConditions ParseConditions(IManifest manifest, IDictionary<string, string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null);

	IManagedTokenString ParseTokenString(IManifest manifest, string rawValue, ISemanticVersion formatVersion, string[]? assumeModIds = null);
}
