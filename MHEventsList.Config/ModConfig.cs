using StardewModdingAPI;

namespace MHEventsList.Config;

public sealed class ModConfig
{
	public SButton OpenMenuKey { get; set; } = (SButton)118;

	public string OpenMenuKeyAndroid { get; set; } = "F7";

	public bool UseDarkTheme { get; set; }

	public EventListFormat EventListFormat { get; set; } = EventListFormat.ThreeLines;

	public int MaxIdCharacters { get; set; } = 30;

	public bool ShowModNameInList { get; set; }

	public bool UseManifestFolderForModName { get; set; }

	public bool UseModDisplayName { get; set; }

	public bool Use24HourClock { get; set; } = true;

	public bool HideUnseenLocations { get; set; } = true;

	public int OptionsPanelWidthPercent { get; set; } = 30;

	public int StatusLinesVerticalOffset { get; set; }

	public OptionsPanelLayout OptionsPanelControlLayout { get; set; }

	public bool ShowGoToLocationButton { get; set; } = true;

	public bool ShowDebugButton { get; set; } = true;

	public bool ShowPlayEventButton { get; set; } = true;

	public bool ShowEventActionsButton { get; set; } = true;

	public bool MarkAsSeenWhenPlaying { get; set; }

	public bool ShowPlayInListInsteadOfHide { get; set; }

	public int OverlayScale { get; set; } = 100;

	public int MaxPinnedEvents { get; set; } = 3;

	public string OverlayPosition { get; set; } = "Right";

	public int OverlayOffsetX { get; set; }

	public int OverlayOffsetY { get; set; } = 100;

	public int OverlayRefreshSeconds { get; set; } = 5;

	public bool VerboseLogging { get; set; }

	public bool EvaluateUnloadedLocations { get; set; } = true;

	public ContentPatcherEventMode ContentPatcherEventMode { get; set; }
}
