using System;
using MHEventsList.UI;
using StardewModdingAPI;

namespace MHEventsList.Config;

public static class ConfigMenuHelper
{
	public static void Register(IModHelper helper, IMonitor monitor, ModConfig config, IManifest manifest)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		IGenericModConfigMenuApi api = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		if (api == null)
		{
			return;
		}
		api.Register(manifest, delegate
		{
			MHEventsListMod.Config = new ModConfig();
		}, delegate
		{
			helper.WriteConfig<ModConfig>(config);
		});
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.keybindings")));
		if ((int)Constants.TargetPlatform == 0)
		{
			string[] allowedValues = new string[52]
			{
				"F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10",
				"F11", "F12", "A", "B", "C", "D", "E", "F", "G", "H",
				"I", "J", "K", "L", "M", "N", "O", "P", "Q", "R",
				"S", "T", "U", "V", "W", "X", "Y", "Z", "LeftTrigger", "RightTrigger",
				"LeftShoulder", "RightShoulder", "LeftStick", "RightStick", "DPadUp", "DPadDown", "DPadLeft", "DPadRight", "ControllerA", "ControllerB",
				"ControllerX", "ControllerY"
			};
			api.AddTextOption(manifest, () => config.OpenMenuKeyAndroid, delegate(string value)
			{
				config.OpenMenuKeyAndroid = value;
			}, () => Translation.op_Implicit(helper.Translation.Get("config.openMenuKey.name")), () => Translation.op_Implicit(helper.Translation.Get("config.openMenuKey.tooltip")), allowedValues);
		}
		else
		{
			api.AddKeybind(manifest, () => config.OpenMenuKey, delegate(SButton value)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				config.OpenMenuKey = value;
			}, () => Translation.op_Implicit(helper.Translation.Get("config.openMenuKey.name")), () => Translation.op_Implicit(helper.Translation.Get("config.openMenuKey.tooltip")));
		}
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.display")));
		api.AddBoolOption(manifest, () => config.UseDarkTheme, delegate(bool value)
		{
			config.UseDarkTheme = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.useDarkTheme.name")), () => Translation.op_Implicit(helper.Translation.Get("config.useDarkTheme.tooltip")));
		api.AddTextOption(manifest, () => config.EventListFormat.ToString(), delegate(string value)
		{
			config.EventListFormat = Enum.Parse<EventListFormat>(value);
		}, () => Translation.op_Implicit(helper.Translation.Get("config.eventListFormat.name")), () => Translation.op_Implicit(helper.Translation.Get("config.eventListFormat.tooltip")), new string[3] { "OneLine", "TwoLines", "ThreeLines" }, (string value) => value switch
		{
			"OneLine" => Translation.op_Implicit(helper.Translation.Get("config.eventListFormat.oneLine")), 
			"TwoLines" => Translation.op_Implicit(helper.Translation.Get("config.eventListFormat.twoLines")), 
			"ThreeLines" => Translation.op_Implicit(helper.Translation.Get("config.eventListFormat.threeLines")), 
			_ => value, 
		});
		api.AddNumberOption(manifest, () => config.MaxIdCharacters, delegate(int value)
		{
			config.MaxIdCharacters = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.maxIdCharacters.name")), () => Translation.op_Implicit(helper.Translation.Get("config.maxIdCharacters.tooltip")), 10, 100);
		api.AddBoolOption(manifest, () => config.ShowModNameInList, delegate(bool value)
		{
			config.ShowModNameInList = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showModNameInList.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showModNameInList.tooltip")));
		api.AddBoolOption(manifest, () => config.UseModDisplayName, delegate(bool value)
		{
			config.UseModDisplayName = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.useModDisplayName.name")), () => Translation.op_Implicit(helper.Translation.Get("config.useModDisplayName.tooltip")));
		api.AddBoolOption(manifest, () => config.UseManifestFolderForModName, delegate(bool value)
		{
			config.UseManifestFolderForModName = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.useManifestFolderForModName.name")), () => Translation.op_Implicit(helper.Translation.Get("config.useManifestFolderForModName.tooltip")));
		api.AddBoolOption(manifest, () => config.Use24HourClock, delegate(bool value)
		{
			config.Use24HourClock = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.use24HourClock.name")), () => Translation.op_Implicit(helper.Translation.Get("config.use24HourClock.tooltip")));
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.filters")));
		api.AddBoolOption(manifest, () => config.HideUnseenLocations, delegate(bool value)
		{
			config.HideUnseenLocations = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.hideUnseenLocations.name")), () => Translation.op_Implicit(helper.Translation.Get("config.hideUnseenLocations.tooltip")));
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.layout")));
		api.AddNumberOption(manifest, () => config.OptionsPanelWidthPercent, delegate(int value)
		{
			config.OptionsPanelWidthPercent = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.optionsPanelWidth.name")), () => Translation.op_Implicit(helper.Translation.Get("config.optionsPanelWidth.tooltip")), 20, 40);
		api.AddNumberOption(manifest, () => config.StatusLinesVerticalOffset, delegate(int value)
		{
			config.StatusLinesVerticalOffset = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.statusLinesVerticalOffset.name")), () => Translation.op_Implicit(helper.Translation.Get("config.statusLinesVerticalOffset.tooltip")), -20, 20);
		api.AddTextOption(manifest, () => config.OptionsPanelControlLayout.ToString(), delegate(string value)
		{
			config.OptionsPanelControlLayout = Enum.Parse<OptionsPanelLayout>(value);
		}, () => Translation.op_Implicit(helper.Translation.Get("config.optionsPanelControlLayout.name")), () => Translation.op_Implicit(helper.Translation.Get("config.optionsPanelControlLayout.tooltip")), new string[2] { "Original", "Alternative" }, delegate(string value)
		{
			if (value == "Original")
			{
				return Translation.op_Implicit(helper.Translation.Get("config.optionsPanelControlLayout.original"));
			}
			return (value == "Alternative") ? Translation.op_Implicit(helper.Translation.Get("config.optionsPanelControlLayout.alternative")) : value;
		});
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.buttons")));
		api.AddBoolOption(manifest, () => config.ShowGoToLocationButton, delegate(bool value)
		{
			config.ShowGoToLocationButton = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showGoToLocationButton.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showGoToLocationButton.tooltip")));
		api.AddBoolOption(manifest, () => config.ShowDebugButton, delegate(bool value)
		{
			config.ShowDebugButton = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showDebugButton.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showDebugButton.tooltip")));
		api.AddBoolOption(manifest, () => config.ShowPlayEventButton, delegate(bool value)
		{
			config.ShowPlayEventButton = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showPlayEventButton.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showPlayEventButton.tooltip")));
		api.AddBoolOption(manifest, () => config.ShowEventActionsButton, delegate(bool value)
		{
			config.ShowEventActionsButton = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showEventActionsButton.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showEventActionsButton.tooltip")));
		api.AddBoolOption(manifest, () => config.MarkAsSeenWhenPlaying, delegate(bool value)
		{
			config.MarkAsSeenWhenPlaying = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.markAsSeenWhenPlaying.name")), () => Translation.op_Implicit(helper.Translation.Get("config.markAsSeenWhenPlaying.tooltip")));
		api.AddBoolOption(manifest, () => config.ShowPlayInListInsteadOfHide, delegate(bool value)
		{
			config.ShowPlayInListInsteadOfHide = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.showPlayInListInsteadOfHide.name")), () => Translation.op_Implicit(helper.Translation.Get("config.showPlayInListInsteadOfHide.tooltip")));
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.overlay")));
		api.AddNumberOption(manifest, () => config.OverlayScale, delegate(int value)
		{
			config.OverlayScale = value;
			MHEventsListMod.Overlay?.UpdatePosition();
		}, () => Translation.op_Implicit(helper.Translation.Get("config.overlayScale.name")), () => Translation.op_Implicit(helper.Translation.Get("config.overlayScale.tooltip")), 50, 200, 10);
		api.AddNumberOption(manifest, () => config.MaxPinnedEvents, delegate(int value)
		{
			config.MaxPinnedEvents = value;
			EventOverlay.TrimPinnedEvents(value);
			MHEventsListMod.Overlay?.RefreshPinnedEvents();
		}, () => Translation.op_Implicit(helper.Translation.Get("config.maxPinnedEvents.name")), () => Translation.op_Implicit(helper.Translation.Get("config.maxPinnedEvents.tooltip")), 1, 10);
		api.AddTextOption(manifest, () => config.OverlayPosition, delegate(string value)
		{
			config.OverlayPosition = value;
			MHEventsListMod.Overlay?.UpdatePosition();
		}, () => Translation.op_Implicit(helper.Translation.Get("config.overlayPosition.name")), () => Translation.op_Implicit(helper.Translation.Get("config.overlayPosition.tooltip")), new string[2] { "Right", "Left" }, delegate(string value)
		{
			if (value == "Right")
			{
				return Translation.op_Implicit(helper.Translation.Get("config.overlayPosition.right"));
			}
			return (value == "Left") ? Translation.op_Implicit(helper.Translation.Get("config.overlayPosition.left")) : value;
		});
		api.AddNumberOption(manifest, () => config.OverlayOffsetX, delegate(int value)
		{
			config.OverlayOffsetX = value;
			MHEventsListMod.Overlay?.UpdatePosition();
		}, () => Translation.op_Implicit(helper.Translation.Get("config.overlayOffsetX.name")), () => Translation.op_Implicit(helper.Translation.Get("config.overlayOffsetX.tooltip")), -1000, 1000);
		api.AddNumberOption(manifest, () => config.OverlayOffsetY, delegate(int value)
		{
			config.OverlayOffsetY = value;
			MHEventsListMod.Overlay?.UpdatePosition();
		}, () => Translation.op_Implicit(helper.Translation.Get("config.overlayOffsetY.name")), () => Translation.op_Implicit(helper.Translation.Get("config.overlayOffsetY.tooltip")), 0, 800);
		api.AddNumberOption(manifest, () => config.OverlayRefreshSeconds, delegate(int value)
		{
			config.OverlayRefreshSeconds = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.overlayRefreshSeconds.name")), () => Translation.op_Implicit(helper.Translation.Get("config.overlayRefreshSeconds.tooltip")), 1, 60);
		api.AddSectionTitle(manifest, () => Translation.op_Implicit(helper.Translation.Get("config.section.advanced")));
		api.AddBoolOption(manifest, () => config.VerboseLogging, delegate(bool value)
		{
			config.VerboseLogging = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.verboseLogging.name")), () => Translation.op_Implicit(helper.Translation.Get("config.verboseLogging.tooltip")));
		api.AddBoolOption(manifest, () => config.EvaluateUnloadedLocations, delegate(bool value)
		{
			config.EvaluateUnloadedLocations = value;
		}, () => Translation.op_Implicit(helper.Translation.Get("config.evaluateUnloadedLocations.name")), () => Translation.op_Implicit(helper.Translation.Get("config.evaluateUnloadedLocations.tooltip")));
		api.AddTextOption(manifest, () => config.ContentPatcherEventMode.ToString(), delegate(string value)
		{
			config.ContentPatcherEventMode = Enum.Parse<ContentPatcherEventMode>(value);
		}, () => Translation.op_Implicit(helper.Translation.Get("config.contentPatcherEventMode.name")), () => Translation.op_Implicit(helper.Translation.Get("config.contentPatcherEventMode.tooltip")), new string[3] { "LoadedOnly", "All", "AllWithMarker" }, (string value) => value switch
		{
			"LoadedOnly" => Translation.op_Implicit(helper.Translation.Get("config.contentPatcherEventMode.loadedOnly")), 
			"All" => Translation.op_Implicit(helper.Translation.Get("config.contentPatcherEventMode.all")), 
			"AllWithMarker" => Translation.op_Implicit(helper.Translation.Get("config.contentPatcherEventMode.allWithMarker")), 
			_ => value, 
		});
	}
}
