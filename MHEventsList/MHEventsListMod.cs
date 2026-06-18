using System;
using System.Collections.Generic;
using System.Linq;
using MHEventsList.Config;
using MHEventsList.Core;
using MHEventsList.Integration;
using MHEventsList.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MHEventsList;

public sealed class MHEventsListMod : Mod
{
	internal static readonly HashSet<string> EventsToUnmark = new HashSet<string>();

	private int ticksSinceGameLaunched = -1;

	private bool isOverlayDragging;

	private int overlayRefreshCounter;

	internal static ModConfig Config { get; set; }

	internal static IModHelper Helper { get; private set; }

	internal static IMonitor Monitor { get; private set; }

	internal static ITranslationHelper I18n => Helper.Translation;

	internal static ContentPatcherIntegration ContentPatcher { get; private set; }

	internal static EventOverlay Overlay { get; private set; }

	internal static EventUserDataManager UserData { get; private set; }

	public override void Entry(IModHelper helper)
	{
		Helper = helper;
		Monitor = ((Mod)this).Monitor;
		Config = helper.ReadConfig<ModConfig>();
		UserData = new EventUserDataManager(helper);
		helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
		helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
		helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
		helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
		helper.Events.GameLoop.Saving += OnSaving;
		helper.Events.Input.ButtonsChanged += OnButtonsChanged;
		helper.Events.Input.ButtonPressed += OnButtonPressed;
		helper.Events.Input.ButtonReleased += OnButtonReleased;
		helper.Events.Input.CursorMoved += OnCursorMoved;
		Monitor.Log("MH Events List initialized", (LogLevel)2);
	}

	private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
	{
		ConfigMenuHelper.Register(Helper, Monitor, Config, ((Mod)this).ModManifest);
		ContentPatcher = new ContentPatcherIntegration(Helper.ModRegistry, ((Mod)this).ModManifest);
		ticksSinceGameLaunched = 0;
	}

	private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
	{
		EventRegistry.Initialize();
		UserData?.Load();
		Overlay = new EventOverlay();
		if (!Game1.onScreenMenus.Contains((IClickableMenu)(object)Overlay))
		{
			Game1.onScreenMenus.Add((IClickableMenu)(object)Overlay);
		}
		if (Config.VerboseLogging)
		{
			Monitor.Log($"Loaded {EventRegistry.GetAllEvents().Count} events", (LogLevel)1);
			Monitor.Log($"Found {EventRegistry.GetNpcsWithEvents().Count()} NPCs with events", (LogLevel)1);
		}
	}

	private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
	{
		EventRegistry.Clear();
		EventsToUnmark.Clear();
		if (Overlay != null && Game1.onScreenMenus.Contains((IClickableMenu)(object)Overlay))
		{
			Game1.onScreenMenus.Remove((IClickableMenu)(object)Overlay);
		}
		Overlay = null;
		EventOverlay.ClearPins();
	}

	private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
	{
		if (ticksSinceGameLaunched >= 0 && ticksSinceGameLaunched < 3)
		{
			ticksSinceGameLaunched++;
			if (ticksSinceGameLaunched >= 2 && ContentPatcher != null && ContentPatcher.IsReady)
			{
				Monitor.Log("Content Patcher API is now ready for condition evaluation.", (LogLevel)1);
				ticksSinceGameLaunched = 100;
			}
		}
	}

	private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
	{
		if (!Context.IsWorldReady)
		{
			return;
		}
		if (EventOverlay.PinnedCount > 0)
		{
			overlayRefreshCounter++;
			if (overlayRefreshCounter >= Config.OverlayRefreshSeconds)
			{
				overlayRefreshCounter = 0;
				Overlay?.RefreshPinnedEvents();
			}
		}
		else
		{
			overlayRefreshCounter = 0;
		}
	}

	private void OnSaving(object sender, SavingEventArgs e)
	{
		UserData?.Save();
	}

	private unsafe void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (((int)Constants.TargetPlatform != 0) ? e.Pressed.Contains(Config.OpenMenuKey) : e.Pressed.Any((SButton button) => ((object)(*(SButton*)(&button))/*cast due to constrained. prefix*/).ToString().Equals(Config.OpenMenuKeyAndroid, StringComparison.OrdinalIgnoreCase)))
		{
			if (Game1.activeClickableMenu == null && Context.IsPlayerFree)
			{
				OpenMenu();
			}
			else if (Game1.activeClickableMenu is MHEventsMenu)
			{
				Game1.activeClickableMenu.exitThisMenu(true);
			}
		}
	}

	private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!Context.IsWorldReady || Game1.activeClickableMenu != null || (int)e.Button != 1000)
		{
			return;
		}
		Point uIMousePosition = GetUIMousePosition();
		int x = uIMousePosition.X;
		int y = uIMousePosition.Y;
		if (Overlay != null && Overlay.IsHeaderClick(x, y))
		{
			isOverlayDragging = true;
			Overlay.StartDrag(x, y);
			return;
		}
		EventOverlay overlay = Overlay;
		if (overlay != null)
		{
			((IClickableMenu)overlay).receiveLeftClick(x, y, true);
		}
	}

	private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (Context.IsWorldReady && (int)e.Button == 1000 && isOverlayDragging)
		{
			isOverlayDragging = false;
			Point uIMousePosition = GetUIMousePosition();
			EventOverlay overlay = Overlay;
			if (overlay != null)
			{
				((IClickableMenu)overlay).releaseLeftClick(uIMousePosition.X, uIMousePosition.Y);
			}
		}
	}

	private void OnCursorMoved(object sender, CursorMovedEventArgs e)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (Context.IsWorldReady && Game1.activeClickableMenu == null && isOverlayDragging)
		{
			Point uIMousePosition = GetUIMousePosition();
			EventOverlay overlay = Overlay;
			if (overlay != null)
			{
				((IClickableMenu)overlay).leftClickHeld(uIMousePosition.X, uIMousePosition.Y);
			}
		}
	}

	private static Point GetUIMousePosition()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		MouseState state = Mouse.GetState();
		float uiScale = Game1.options.uiScale;
		int num = (int)((float)((MouseState)(ref state)).X / uiScale);
		int num2 = (int)((float)((MouseState)(ref state)).Y / uiScale);
		return new Point(num, num2);
	}

	private void OpenMenu()
	{
		if (!Context.IsWorldReady)
		{
			Monitor.Log("Cannot open menu: world not ready", (LogLevel)3);
		}
		else
		{
			Game1.activeClickableMenu = (IClickableMenu)(object)new MHEventsMenu();
		}
	}
}
