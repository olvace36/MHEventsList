using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MHEventsList.Config;
using MHEventsList.Core;
using MHEventsList.Integration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using xTile.Dimensions;

namespace MHEventsList.UI;

public class MHEventsMenu : IClickableMenu, IKeyboardSubscriber
{
	private const int MENU_WIDTH = 1280;

	private const int MENU_HEIGHT = 760;

	private const int MAX_ENTRIES_SHOWN = 6;

	private const int ENTRY_HEIGHT = 88;

	private const int BOX_BORDER = 12;

	private readonly TextEntryManager TextEntryManager = new TextEntryManager();

	private readonly List<EventData> allEvents;

	private List<EventData> filteredEvents;

	private readonly List<string> npcList;

	private readonly List<ClickableComponent> eventSlots;

	private readonly List<ClickableTextureComponent> detailButtons;

	private readonly List<ClickableTextureComponent> goToButtons;

	private readonly List<ClickableTextureComponent> hideButtons;

	private readonly Rectangle[] disabledMarkerAreas;

	private ClickableTextureComponent scrollBar;

	private Rectangle scrollBarRunner;

	private bool scrollBarHeld;

	private TextBox searchBox;

	private ClickableTextureComponent clearSearchButton;

	private string lastSearchText = "";

	private bool IsSearchBoxSelectedExplicitly;

	private int filterMode;

	private static int lastFilterMode = 0;

	private static int lastSelectedNpcIndex = -1;

	private int selectedNpcIndex = -1;

	private readonly List<ClickableComponent> npcSlots;

	private int npcScrollOffset;

	private const int MAX_NPC_SHOWN = 8;

	private int eventBoxX;

	private int eventBoxY;

	private int eventBoxWidth;

	private int eventBoxHeight;

	private int optionsPanelX;

	private int optionsPanelWidth;

	private int startIndex;

	private int endIndex;

	private bool isDraggingEventList;

	private bool isDraggingNpcList;

	private int lastDragY;

	private int dragAccumulator;

	private readonly Texture2D letterTexture;

	private HashSet<string> hiddenEventIds;

	private Rectangle clearFilterBounds;

	private Rectangle toggleSeenBounds;

	private Rectangle toggleRelationshipsBounds;

	private bool showOnlyRelationships;

	private Dictionary<string, string> npcNameMapping;

	private string npcSearchText = "";

	private Rectangle npcSearchBounds;

	private bool npcSearchSelected;

	private TextBox npcSearchBox;

	private int sortBy;

	private bool sortAscending = true;

	private Rectangle sortButtonBounds;

	private Rectangle sortOrderBounds;

	private int maxHeartsFilter = 14;

	private Rectangle heartsMinusBounds;

	private Rectangle heartsPlusBounds;

	public bool Selected { get; set; }

	private bool IsAndroid => (int)Constants.TargetPlatform == 0;

	public void RecieveTextInput(char inputChar)
	{
		if (npcSearchSelected && !char.IsControl(inputChar))
		{
			npcSearchText += inputChar;
			npcScrollOffset = 0;
		}
	}

	public void RecieveTextInput(string text)
	{
		if (npcSearchSelected)
		{
			npcSearchText += text;
			npcScrollOffset = 0;
		}
	}

	public void RecieveCommandInput(char command)
	{
		if (npcSearchSelected && command == '\b' && npcSearchText.Length > 0)
		{
			npcSearchText = npcSearchText.Substring(0, npcSearchText.Length - 1);
			npcScrollOffset = 0;
		}
	}

	public void RecieveSpecialInput(Keys key)
	{
	}

	public MHEventsMenu()
		: base((((Rectangle)(ref Game1.uiViewport)).Width - 1280) / 2, (((Rectangle)(ref Game1.uiViewport)).Height - 760) / 2, 1280, 760, true)
	{
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		eventSlots = new List<ClickableComponent>();
		detailButtons = new List<ClickableTextureComponent>();
		goToButtons = new List<ClickableTextureComponent>();
		hideButtons = new List<ClickableTextureComponent>();
		disabledMarkerAreas = (Rectangle[])(object)new Rectangle[6];
		npcSlots = new List<ClickableComponent>();
		hiddenEventIds = LoadHiddenEvents();
		UnmarkPendingEvents();
		EventRegistry.Initialize();
		allEvents = EventRegistry.GetAllEvents().ToList();
		filteredEvents = new List<EventData>();
		npcList = BuildNpcList();
		filterMode = lastFilterMode;
		selectedNpcIndex = lastSelectedNpcIndex;
		if (selectedNpcIndex >= npcList.Count)
		{
			selectedNpcIndex = -1;
		}
		CalculateLayout();
		SetupUIComponents();
		RefreshEventList();
	}

	public void ReloadAndRefresh()
	{
		hiddenEventIds = LoadHiddenEvents();
		startIndex = 0;
		filteredEvents.Clear();
		RefreshEventList();
	}

	private List<string> BuildNpcList()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		// วน loop จาก events ทั้งหมดแทน getAllVillagers()
		// เพื่อให้ NPC ที่ยังไม่ได้ spawn ใน world ก็ขึ้นในลิสต์ได้
		foreach (EventData evt in allEvents)
		{
			if (evt.HeartRequirements == null) continue;
			foreach (CharacterHeartRequirement req in evt.HeartRequirements)
			{
				if (string.IsNullOrWhiteSpace(req.NpcName)) continue;
				string internalName = req.NpcName;
				// ลอง resolve displayName จาก game ถ้าทำได้
				NPC npc = Game1.getCharacterFromName(internalName, false, false);
				string displayName = npc?.displayName ?? internalName;
				displayName = EventDetailMenu.SanitizeDisplayName(displayName);
				if (!string.IsNullOrWhiteSpace(displayName) &&
					EventDetailMenu.IsValidNpcForDisplay(displayName) &&
					!dictionary.ContainsKey(displayName))
				{
					dictionary[displayName] = internalName;
				}
			}
		}
		npcNameMapping = dictionary;
		return dictionary.Keys.OrderBy((string n) => n).ToList();
	}

	private void CalculateLayout()
	{
		int num = Math.Clamp(MHEventsListMod.Config.OptionsPanelWidthPercent, 15, 35);
		optionsPanelX = base.xPositionOnScreen + 40;
		optionsPanelWidth = (int)((float)((base.width - 80) * num) / 100f);
		eventBoxX = optionsPanelX + optionsPanelWidth + 40;
		eventBoxY = base.yPositionOnScreen + 180;
		eventBoxWidth = base.width - optionsPanelWidth - 140;
		eventBoxHeight = 533;
	}

	private void SetupUIComponents()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Expected O, but got Unknown
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Expected O, but got Unknown
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Expected O, but got Unknown
		searchBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
		{
			X = eventBoxX,
			Y = base.yPositionOnScreen + 115,
			Width = eventBoxWidth - 60,
			Height = 36,
			Text = ""
		};
		clearSearchButton = new ClickableTextureComponent(new Rectangle(eventBoxX + eventBoxWidth - 50, base.yPositionOnScreen + 115, 36, 36), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 2.8f, false);
		npcSearchBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), (Texture2D)null, Game1.smallFont, Game1.textColor)
		{
			X = 0,
			Y = 0,
			Width = 200,
			Height = 36,
			Text = ""
		};
		for (int i = 0; i < 6; i++)
		{
			eventSlots.Add(new ClickableComponent(new Rectangle(eventBoxX + 12, eventBoxY + i * 88 + 12, eventBoxWidth - 180, 80), $"slot_{i}"));
			detailButtons.Add(new ClickableTextureComponent(new Rectangle(eventBoxX + eventBoxWidth - 160, eventBoxY + i * 88 + 20, 40, 40), Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 2.5f, false));
			goToButtons.Add(new ClickableTextureComponent(new Rectangle(eventBoxX + eventBoxWidth - 110, eventBoxY + i * 88 + 20, 40, 40), Game1.mouseCursors, new Rectangle(0, 192, 64, 64), 0.6f, false));
			Rectangle val = (MHEventsListMod.Config.ShowPlayInListInsteadOfHide ? new Rectangle(310, 392, 16, 16) : new Rectangle(322, 498, 12, 12));
			float num = (MHEventsListMod.Config.ShowPlayInListInsteadOfHide ? 2.5f : 2.8f);
			hideButtons.Add(new ClickableTextureComponent(new Rectangle(eventBoxX + eventBoxWidth - 60, eventBoxY + i * 88 + 20, 40, 40), Game1.mouseCursors, val, num, false));
		}
		scrollBarRunner = new Rectangle(eventBoxX + eventBoxWidth + 8, eventBoxY, 24, eventBoxHeight);
		scrollBar = new ClickableTextureComponent(new Rectangle(scrollBarRunner.X, scrollBarRunner.Y, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f, false);
		int num2 = base.yPositionOnScreen + 130;
		for (int j = 0; j < 8; j++)
		{
			npcSlots.Add(new ClickableComponent(new Rectangle(optionsPanelX, num2 + j * 32, optionsPanelWidth - 10, 28), $"npc_{j}"));
		}
	}

	private void RefreshEventList()
	{
		filteredEvents.Clear();
		foreach (EventData allEvent in allEvents)
		{
			bool flag = hiddenEventIds.Contains(allEvent.Id);
			bool flag2 = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(allEvent.Id);
			switch (filterMode)
			{
			case 0:
				if (flag || flag2 || allEvent.HasInvalidScript || (MHEventsListMod.Config.HideUnseenLocations && !((NetHashSet<string>)(object)Game1.player.locationsVisited).Contains(allEvent.LocationName)) || !allEvent.AreConditionsMet())
				{
					continue;
				}
				break;
			case 1:
				if (!flag)
				{
					continue;
				}
				break;
			case 3:
				if (!flag2)
				{
					continue;
				}
				break;
			case 4:
				if (!allEvent.IsFromContentPatcher)
				{
					continue;
				}
				break;
			}
			if (selectedNpcIndex >= 0 && selectedNpcIndex < npcList.Count)
			{
				string selectedDisplayName = npcList[selectedNpcIndex];
				string internalName = null;
				if (npcNameMapping != null && npcNameMapping.TryGetValue(selectedDisplayName, out var value))
				{
					internalName = value;
				}
				List<string> requiredNpcs = allEvent.RequiredNpcs;
				if (requiredNpcs == null || !requiredNpcs.Any((string n) => n.Equals(internalName, StringComparison.OrdinalIgnoreCase) || n.Equals(selectedDisplayName, StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}
				if (allEvent.HeartRequirements != null)
				{
					CharacterHeartRequirement characterHeartRequirement = allEvent.HeartRequirements.FirstOrDefault((CharacterHeartRequirement h) => h.NpcName.Equals(internalName, StringComparison.OrdinalIgnoreCase) || h.NpcName.Equals(selectedDisplayName, StringComparison.OrdinalIgnoreCase));
					if (characterHeartRequirement != null && characterHeartRequirement.Hearts > maxHeartsFilter)
					{
						continue;
					}
				}
			}
			if (!showOnlyRelationships || (allEvent.HeartRequirements != null && allEvent.HeartRequirements.Count > 0))
			{
				filteredEvents.Add(allEvent);
			}
		}
		ApplySearch();
		ApplySorting();
	}

	private void ApplySearch()
	{
		TextBox obj = searchBox;
		string search = ((obj == null) ? null : obj.Text?.ToLower()?.Trim()) ?? "";
		if (!string.IsNullOrEmpty(search))
		{
			filteredEvents = filteredEvents.Where((EventData evt) => evt.Id.ToLower().Contains(search) || evt.GetTranslatedLocation().ToLower().Contains(search) || (evt.RequiredNpcs?.Any((string n) => n.ToLower().Contains(search)) ?? false)).ToList();
		}
		startIndex = 0;
		endIndex = Math.Min(6, filteredEvents.Count);
		UpdateScrollBar();
	}

	private void ApplySorting()
	{
		if (sortBy == 0)
		{
			return;
		}
		IOrderedEnumerable<EventData> orderedEnumerable = null;
		switch (sortBy)
		{
		case 1:
			orderedEnumerable = (sortAscending ? filteredEvents.OrderBy((EventData e) => e.Id) : filteredEvents.OrderByDescending((EventData e) => e.Id));
			break;
		case 2:
			orderedEnumerable = (sortAscending ? filteredEvents.OrderBy((EventData e) => e.GetTranslatedLocation()) : filteredEvents.OrderByDescending((EventData e) => e.GetTranslatedLocation()));
			break;
		case 3:
			orderedEnumerable = (sortAscending ? filteredEvents.OrderBy((EventData e) => e.HeartRequirements?.Max((CharacterHeartRequirement h) => h.Hearts) ?? 0) : filteredEvents.OrderByDescending((EventData e) => e.HeartRequirements?.Max((CharacterHeartRequirement h) => h.Hearts) ?? 0));
			break;
		case 4:
			orderedEnumerable = (sortAscending ? filteredEvents.OrderBy<EventData, string>((EventData e) => e.ModName ?? "Vanilla", StringComparer.OrdinalIgnoreCase) : filteredEvents.OrderByDescending<EventData, string>((EventData e) => e.ModName ?? "Vanilla", StringComparer.OrdinalIgnoreCase));
			break;
		}
		if (orderedEnumerable != null)
		{
			filteredEvents = orderedEnumerable.ToList();
		}
	}

	private void UpdateScrollBar()
	{
		if (filteredEvents.Count <= 6)
		{
			((ClickableComponent)scrollBar).bounds.Y = scrollBarRunner.Y;
			return;
		}
		int num = filteredEvents.Count - 6;
		float num2 = (float)startIndex / (float)num;
		((ClickableComponent)scrollBar).bounds.Y = (int)((float)(scrollBarRunner.Height - 40) * num2) + scrollBarRunner.Y;
	}

	private void Scroll(bool up)
	{
		if (filteredEvents.Count > 6)
		{
			if (up && startIndex > 0)
			{
				startIndex--;
				endIndex--;
			}
			else if (!up && endIndex < filteredEvents.Count)
			{
				startIndex++;
				endIndex++;
			}
			UpdateScrollBar();
		}
	}

	private HashSet<string> LoadHiddenEvents()
	{
		try
		{
			List<string> list = MHEventsListMod.Helper.Data.ReadSaveData<List<string>>("HiddenEvents");
			return (list != null) ? new HashSet<string>(list) : new HashSet<string>();
		}
		catch
		{
			return new HashSet<string>();
		}
	}

	private void SaveHiddenEvents()
	{
		try
		{
			MHEventsListMod.Helper.Data.WriteSaveData<List<string>>("HiddenEvents", hiddenEventIds.ToList());
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error saving hidden events: " + ex.Message, (LogLevel)4);
		}
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, ((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height), Color.Black * 0.75f);
		Color val = (Color)(useDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, val, 1f, true, -1f);
		string text = Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.title"));
		if (useDarkTheme)
		{
			Vector2 val2 = Game1.dialogueFont.MeasureString(text);
			Color val3 = default(Color);
			((Color)(ref val3))._002Ector(220, 180, 120);
			b.DrawString(Game1.dialogueFont, text, new Vector2((float)base.xPositionOnScreen + ((float)base.width - val2.X) / 2f, (float)(base.yPositionOnScreen + 12)), val3);
		}
		else
		{
			SpriteText.drawStringHorizontallyCenteredAt(b, text, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 12, 999999, -1, 999999, 1f, 0.88f, false, (Color?)null, 99999);
		}
		Color val4 = (useDarkTheme ? Color.White : Color.Black);
		int num = Math.Clamp(MHEventsListMod.Config.StatusLinesVerticalOffset, -20, 20);
		int num2 = base.yPositionOnScreen + 22 + num;
		string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view"));
		string text3 = text2 + " " + filterMode switch
		{
			0 => Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view.available")), 
			1 => Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view.hidden")), 
			2 => Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view.all")), 
			3 => Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view.seen")), 
			4 => "Content Patcher", 
			_ => Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.view.all")), 
		};
		b.DrawString(Game1.smallFont, text3, new Vector2((float)optionsPanelX, (float)num2), val4);
		string text4 = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.filters"));
		string text5 = Translation.op_Implicit(showOnlyRelationships ? MHEventsListMod.I18n.Get("menu.filters.relationships") : MHEventsListMod.I18n.Get("menu.filters.all"));
		string text6 = text4 + " " + text5;
		b.DrawString(Game1.smallFont, text6, new Vector2((float)optionsPanelX, (float)(num2 + 20)), val4);
		if (selectedNpcIndex >= 0 && selectedNpcIndex < npcList.Count)
		{
			string text7 = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.npc"));
			string text8 = npcList[selectedNpcIndex];
			string text9 = text7 + " " + text8;
			b.DrawString(Game1.smallFont, text9, new Vector2((float)optionsPanelX, (float)(num2 + 40)), val4);
		}
		int num3 = base.yPositionOnScreen + 78 + num;
		string text10 = Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.eventCount", (object)new
		{
			count = filteredEvents.Count
		}));
		b.DrawString(Game1.smallFont, text10, new Vector2((float)optionsPanelX, (float)num3), val4);
		Rectangle val5 = default(Rectangle);
		((Rectangle)(ref val5))._002Ector(searchBox.X, searchBox.Y, searchBox.Width, searchBox.Height);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), val5.X, val5.Y, val5.Width, val5.Height, searchBox.Selected ? Color.Wheat : Color.White, 3f, false, -1f);
		string text11 = (string.IsNullOrEmpty(searchBox.Text) ? Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.search")) : searchBox.Text);
		Color black = Color.Black;
		float y = Game1.smallFont.MeasureString(text11).Y;
		int num4 = (int)(((float)val5.Height - y) / 2f);
		b.DrawString(Game1.smallFont, text11, new Vector2((float)(val5.X + 10), (float)(val5.Y + num4)), black);
		if (!string.IsNullOrEmpty(searchBox.Text))
		{
			clearSearchButton.draw(b);
		}
		DrawOptionsPanel(b);
		if (useDarkTheme)
		{
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), eventBoxX, eventBoxY, eventBoxWidth, eventBoxHeight, new Color(50, 50, 60), 1f, true, -1f);
		}
		else
		{
			Game1.DrawBox(eventBoxX, eventBoxY, eventBoxWidth, eventBoxHeight, (Color?)null);
		}
		DrawEvents(b);
		if (filteredEvents.Count > 6)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, false, -1f);
			scrollBar.draw(b);
		}
		((IClickableMenu)this).draw(b);
		DrawTooltips(b);
		((IClickableMenu)this).drawMouse(b, false, -1);
	}

	private void DrawOptionsPanel(SpriteBatch b)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		Color textColor = (useDarkTheme ? Color.White : Color.Black);
		int num = base.yPositionOnScreen + 160;
		int num2 = eventBoxHeight + 30;
		if (useDarkTheme)
		{
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), optionsPanelX - 10, num - 10, optionsPanelWidth + 10, num2, new Color(50, 50, 60), 1f, true, -1f);
		}
		else
		{
			Game1.DrawBox(optionsPanelX - 10, num - 10, optionsPanelWidth + 10, num2, (Color?)null);
		}
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		if (MHEventsListMod.Config.OptionsPanelControlLayout == OptionsPanelLayout.Alternative)
		{
			DrawAlternativeLayout(b, num, num2, mouseX, mouseY, textColor);
		}
		else
		{
			DrawOriginalLayout(b, num, num2, mouseX, mouseY, textColor);
		}
	}

	private void DrawNPCList(SpriteBatch b, int startY, int mouseX, int mouseY, Color textColor, bool darkMode)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		string text = Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.characters"));
		b.DrawString(Game1.smallFont, text, new Vector2((float)optionsPanelX, (float)startY), textColor);
		int num = startY + 29;
		npcSearchBounds = new Rectangle(optionsPanelX + 4, num, optionsPanelWidth - 28, 38);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), npcSearchBounds.X, npcSearchBounds.Y, npcSearchBounds.Width, npcSearchBounds.Height, npcSearchSelected ? Color.Wheat : Color.White, 3f, false, -1f);
		string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.searchNpc"));
		string text3 = (string.IsNullOrEmpty(npcSearchText) ? text2 : npcSearchText);
		float y = Game1.smallFont.MeasureString(text3).Y;
		int num2 = (int)(((float)npcSearchBounds.Height - y) / 2f);
		b.DrawString(Game1.smallFont, text3, new Vector2((float)(npcSearchBounds.X + 10), (float)(npcSearchBounds.Y + num2)), Color.Black);
		List<string> list = (string.IsNullOrEmpty(npcSearchText) ? npcList : npcList.Where((string n) => n.ToLower().Contains(npcSearchText.ToLower())).ToList());
		int num3 = num + 48;
		int num4 = Math.Min(7, list.Count - npcScrollOffset);
		Rectangle val = default(Rectangle);
		for (int num5 = 0; num5 < num4; num5++)
		{
			int num6 = num5 + npcScrollOffset;
			if (num6 >= list.Count)
			{
				break;
			}
			string text4 = list[num6];
			int num7 = npcList.IndexOf(text4);
			((Rectangle)(ref val))._002Ector(optionsPanelX, num3 + num5 * 28, optionsPanelWidth - 20, 26);
			if (num5 < npcSlots.Count)
			{
				npcSlots[num5].bounds = val;
			}
			bool flag = num7 == selectedNpcIndex;
			bool flag2 = ((Rectangle)(ref val)).Contains(mouseX, mouseY);
			if (flag)
			{
				b.Draw(Game1.staminaRect, val, new Color(86, 22, 12) * 0.6f);
			}
			else if (flag2)
			{
				b.Draw(Game1.staminaRect, val, (darkMode ? Color.Gray : Color.White) * 0.25f);
			}
			string text5 = text4;
			if (Game1.smallFont.MeasureString(text5).X > (float)(optionsPanelWidth - 30))
			{
				while (Game1.smallFont.MeasureString(text5 + "...").X > (float)(optionsPanelWidth - 30) && text5.Length > 3)
				{
					text5 = text5.Substring(0, text5.Length - 1);
				}
				text5 += "...";
			}
			Color val2 = (flag ? Color.White : textColor);
			b.DrawString(Game1.smallFont, text5, new Vector2((float)(optionsPanelX + 5), (float)(num3 + num5 * 28 + 3)), val2);
		}
	}

	private void DrawAlternativeLayout(SpriteBatch b, int panelY, int panelHeight, int mouseX, int mouseY, Color textColor)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		int num = panelY;
		int num2 = 40;
		int num3 = 32;
		int num4 = 6;
		int num5 = (optionsPanelWidth - 30) / 2;
		int num6 = optionsPanelWidth - 20;
		sortButtonBounds = new Rectangle(optionsPanelX, num, num5, num3);
		string text = sortBy switch
		{
			0 => ((object)MHEventsListMod.I18n.Get("menu.sort.none")).ToString(), 
			1 => ((object)MHEventsListMod.I18n.Get("menu.sort.id")).ToString(), 
			2 => ((object)MHEventsListMod.I18n.Get("menu.sort.location")).ToString(), 
			3 => ((object)MHEventsListMod.I18n.Get("menu.sort.hearts")).ToString(), 
			4 => ((object)MHEventsListMod.I18n.Get("menu.sort.mod")).ToString(), 
			_ => ((object)MHEventsListMod.I18n.Get("menu.sort.none")).ToString(), 
		};
		DrawSmallButton(b, sortButtonBounds, text, mouseX, mouseY);
		sortOrderBounds = new Rectangle(optionsPanelX + num5 + 10, num, num5, num3);
		string text2 = Translation.op_Implicit(sortAscending ? MHEventsListMod.I18n.Get("menu.sort.asc") : MHEventsListMod.I18n.Get("menu.sort.desc"));
		DrawSmallButton(b, sortOrderBounds, text2, mouseX, mouseY, sortBy > 0);
		num += num3 + num4;
		string text3 = filterMode switch
		{
			0 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAll")), 
			1 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showSeen")), 
			2 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showHidden")), 
			3 => "Content Patcher", 
			4 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAvailable")), 
			_ => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAll")), 
		};
		toggleSeenBounds = new Rectangle(optionsPanelX, num, num6, num2);
		DrawBigButton(b, toggleSeenBounds, text3, mouseX, mouseY, filterMode == 4);
		num += num2 + num4;
		string text4 = Translation.op_Implicit(showOnlyRelationships ? MHEventsListMod.Helper.Translation.Get("menu.showRelationships") : MHEventsListMod.Helper.Translation.Get("menu.showAllEvents"));
		toggleRelationshipsBounds = new Rectangle(optionsPanelX, num, num6, num2);
		DrawBigButton(b, toggleRelationshipsBounds, text4, mouseX, mouseY, showOnlyRelationships);
		num += num2 + 10;
		DrawNPCList(b, num, mouseX, mouseY, textColor, useDarkTheme);
		int num7 = 307;
		num += num7 + 10;
		int num8 = 32;
		int num9 = optionsPanelWidth - num8 * 2 - 30;
		bool isClickable = selectedNpcIndex >= 0;
		heartsMinusBounds = new Rectangle(optionsPanelX, num, num8, num8);
		DrawSmallButton(b, heartsMinusBounds, "-", mouseX, mouseY, isActive: false, isClickable);
		Rectangle bounds = default(Rectangle);
		((Rectangle)(ref bounds))._002Ector(optionsPanelX + num8 + 5, num, num9, num8);
		string text5 = ((maxHeartsFilter >= 14) ? Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.allHearts")) : ($"{maxHeartsFilter} " + Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.hearts"))));
		DrawSmallButton(b, bounds, text5, mouseX, mouseY, isActive: false, isClickable);
		heartsPlusBounds = new Rectangle(optionsPanelX + num8 + num9 + 10, num, num8, num8);
		DrawSmallButton(b, heartsPlusBounds, "+", mouseX, mouseY, isActive: false, isClickable);
		int num10 = panelY + panelHeight - num2 - 15;
		string text6 = Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.clearFilter"));
		clearFilterBounds = new Rectangle(optionsPanelX, num10, num6, num2);
		DrawBigButton(b, clearFilterBounds, text6, mouseX, mouseY);
	}

	private void DrawOriginalLayout(SpriteBatch b, int panelY, int panelHeight, int mouseX, int mouseY, Color textColor)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		DrawNPCList(b, panelY, mouseX, mouseY, textColor, useDarkTheme);
		int num = 40;
		int num2 = 6;
		int num3 = 32;
		int num4 = 32;
		int num5 = num3 + num4 + 16;
		int num6 = panelY + panelHeight - num * 3 - num2 * 2 - num5 - 20;
		int num7 = optionsPanelWidth - 20;
		string text = Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.clearFilter"));
		clearFilterBounds = new Rectangle(optionsPanelX, num6, num7, num);
		DrawBigButton(b, clearFilterBounds, text, mouseX, mouseY);
		string text2 = filterMode switch
		{
			0 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAll")), 
			1 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showSeen")), 
			2 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showHidden")), 
			3 => "Content Patcher", 
			4 => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAvailable")), 
			_ => Translation.op_Implicit(MHEventsListMod.Helper.Translation.Get("menu.showAll")), 
		};
		toggleSeenBounds = new Rectangle(optionsPanelX, num6 + num + num2, num7, num);
		DrawBigButton(b, toggleSeenBounds, text2, mouseX, mouseY, filterMode == 4);
		string text3 = Translation.op_Implicit(showOnlyRelationships ? MHEventsListMod.Helper.Translation.Get("menu.showRelationships") : MHEventsListMod.Helper.Translation.Get("menu.showAllEvents"));
		toggleRelationshipsBounds = new Rectangle(optionsPanelX, num6 + (num + num2) * 2, num7, num);
		DrawBigButton(b, toggleRelationshipsBounds, text3, mouseX, mouseY, showOnlyRelationships);
		int num8 = (optionsPanelWidth - 30) / 2;
		int num9 = num6 + num * 3 + num2 * 2 + 10;
		sortButtonBounds = new Rectangle(optionsPanelX, num9, num8, num3);
		string text4 = sortBy switch
		{
			0 => ((object)MHEventsListMod.I18n.Get("menu.sort.none")).ToString(), 
			1 => ((object)MHEventsListMod.I18n.Get("menu.sort.id")).ToString(), 
			2 => ((object)MHEventsListMod.I18n.Get("menu.sort.location")).ToString(), 
			3 => ((object)MHEventsListMod.I18n.Get("menu.sort.hearts")).ToString(), 
			4 => ((object)MHEventsListMod.I18n.Get("menu.sort.mod")).ToString(), 
			_ => ((object)MHEventsListMod.I18n.Get("menu.sort.none")).ToString(), 
		};
		DrawSmallButton(b, sortButtonBounds, text4, mouseX, mouseY);
		sortOrderBounds = new Rectangle(optionsPanelX + num8 + 10, num9, num8, num3);
		string text5 = Translation.op_Implicit(sortAscending ? MHEventsListMod.I18n.Get("menu.sort.asc") : MHEventsListMod.I18n.Get("menu.sort.desc"));
		DrawSmallButton(b, sortOrderBounds, text5, mouseX, mouseY, sortBy > 0);
		int num10 = num9 + num3 + 8;
		int num11 = 32;
		int num12 = optionsPanelWidth - num11 * 2 - 30;
		bool isClickable = selectedNpcIndex >= 0;
		heartsMinusBounds = new Rectangle(optionsPanelX, num10, num11, num11);
		DrawSmallButton(b, heartsMinusBounds, "-", mouseX, mouseY, isActive: false, isClickable);
		Rectangle bounds = default(Rectangle);
		((Rectangle)(ref bounds))._002Ector(optionsPanelX + num11 + 5, num10, num12, num11);
		string text6 = ((maxHeartsFilter >= 14) ? Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.allHearts")) : ($"{maxHeartsFilter} " + Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.hearts"))));
		DrawSmallButton(b, bounds, text6, mouseX, mouseY, isActive: false, isClickable);
		heartsPlusBounds = new Rectangle(optionsPanelX + num11 + num12 + 10, num10, num11, num11);
		DrawSmallButton(b, heartsPlusBounds, "+", mouseX, mouseY, isActive: false, isClickable);
	}

	private void DrawBigButton(SpriteBatch b, Rectangle bounds, string text, int mouseX, int mouseY, bool isActive = false)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((Rectangle)(ref bounds)).Contains(mouseX, mouseY);
		Color val;
		Color val2;
		if (MHEventsListMod.Config.UseDarkTheme)
		{
			val = (isActive ? new Color(60, 100, 60) : (flag ? new Color(70, 70, 80) : new Color(50, 50, 60)));
			val2 = Color.White;
		}
		else
		{
			val = (Color)(isActive ? new Color(200, 230, 200) : (flag ? Color.Wheat : Color.White));
			val2 = Color.Black;
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), bounds.X, bounds.Y, bounds.Width, bounds.Height, val, 4f, false, -1f);
		Vector2 val3 = Game1.smallFont.MeasureString(text);
		b.DrawString(Game1.smallFont, text, new Vector2((float)bounds.X + ((float)bounds.Width - val3.X) / 2f, (float)bounds.Y + ((float)bounds.Height - val3.Y) / 2f), val2);
	}

	private void DrawSmallButton(SpriteBatch b, Rectangle bounds, string text, int mouseX, int mouseY, bool isActive = false, bool isClickable = true, float textScale = 0.9f)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		bool flag = isClickable && ((Rectangle)(ref bounds)).Contains(mouseX, mouseY);
		Color val;
		Color val2;
		if (MHEventsListMod.Config.UseDarkTheme)
		{
			val = (isActive ? new Color(60, 100, 60) : (flag ? new Color(70, 70, 80) : new Color(50, 50, 60)));
			val2 = (Color)(isClickable ? Color.White : new Color(120, 120, 120));
		}
		else
		{
			val = (Color)(isActive ? new Color(200, 230, 200) : (flag ? Color.Wheat : Color.White));
			val2 = Color.Black;
			if (!isClickable)
			{
				val = Color.LightGray;
			}
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), bounds.X, bounds.Y, bounds.Width, bounds.Height, val, 3f, false, -1f);
		Vector2 val3 = Game1.smallFont.MeasureString(text) * textScale;
		b.DrawString(Game1.smallFont, text, new Vector2((float)bounds.X + ((float)bounds.Width - val3.X) / 2f, (float)bounds.Y + ((float)bounds.Height - val3.Y) / 2f), val2, 0f, Vector2.Zero, textScale, (SpriteEffects)0, 1f);
	}

	private void DrawEvents(SpriteBatch b)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		if (filteredEvents.Count == 0)
		{
			Color val = (MHEventsListMod.Config.UseDarkTheme ? Color.LightGray : Color.Gray) * 0.7f;
			string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.noResults"));
			string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("menu.noResultsHint"));
			Vector2 val2 = Game1.smallFont.MeasureString(text);
			Vector2 val3 = Game1.smallFont.MeasureString(text2);
			int num = eventBoxX + eventBoxWidth / 2;
			int num2 = eventBoxY + eventBoxHeight / 2;
			b.DrawString(Game1.smallFont, text, new Vector2((float)num - val2.X / 2f, (float)(num2 - 20)), val);
			b.DrawString(Game1.smallFont, text2, new Vector2((float)num - val3.X / 2f, (float)(num2 + 10)), val * 0.8f);
			return;
		}
		for (int i = 0; i < 6; i++)
		{
			int num3 = startIndex + i;
			if (num3 < filteredEvents.Count)
			{
				EventData evt = filteredEvents[num3];
				int num4 = eventBoxY + i * 88 + 12;
				if (eventSlots[i].containsPoint(mouseX, mouseY))
				{
					b.Draw(Game1.staminaRect, new Rectangle(eventBoxX + 4, num4 - 4, eventBoxWidth - 8, 84), Color.White * 0.2f);
				}
				DrawEventEntry(b, evt, num4, i);
				if (i < 5 && num3 < filteredEvents.Count - 1)
				{
					b.Draw(Game1.menuTexture, new Rectangle(eventBoxX + 8, num4 + 88 - 8, eventBoxWidth - 16, 4), (Rectangle?)Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25, -1, -1), Color.White * 0.5f);
				}
				continue;
			}
			break;
		}
	}

	private void DrawEventEntry(SpriteBatch b, EventData evt, int yPos, int buttonIndex)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_0709: Unknown result type (might be due to invalid IL or missing references)
		//IL_070e: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_074f: Unknown result type (might be due to invalid IL or missing references)
		//IL_078e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0790: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_069e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_077a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Unknown result type (might be due to invalid IL or missing references)
		//IL_077f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		Color val = (useDarkTheme ? Color.White : Color.Black);
		Color val2 = (useDarkTheme ? new Color(200, 150, 100) : new Color(86, 22, 12));
		Color val3 = (useDarkTheme ? Color.LightGray : Color.DimGray);
		int num = eventBoxX + 12 + 8;
		int num2 = eventBoxWidth - 180;
		int num3 = 22;
		int num4 = yPos + 4;
		EventListFormat eventListFormat = MHEventsListMod.Config.EventListFormat;
		bool flag = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(evt.Id);
		if (eventListFormat == EventListFormat.ThreeLines)
		{
			string text = "ID: " + evt.Id;
			bool showModNameInList = MHEventsListMod.Config.ShowModNameInList;
			if (showModNameInList)
			{
				string text2 = ((!string.IsNullOrEmpty(evt.ModName)) ? evt.ModName : Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.modVanilla")));
				text = text + " - " + text2;
			}
			bool flag2 = evt.IsDisabledByCP && MHEventsListMod.Config.ContentPatcherEventMode == ContentPatcherEventMode.AllWithMarker;
			if (flag2)
			{
				text += " [D]";
			}
			int num5 = (showModNameInList ? num2 : (num2 - 80));
			while (Game1.smallFont.MeasureString(text).X > (float)num5 && text.Length > 10)
			{
				if (flag2 && text.EndsWith(" [D]"))
				{
					text = text.Substring(0, text.Length - 5);
					text = text.Substring(0, text.Length - 1) + "... [D]";
					break;
				}
				text = text.Substring(0, text.Length - 1);
			}
			if (!text.EndsWith("...") && !flag2 && Game1.smallFont.MeasureString("ID: " + evt.Id + (showModNameInList ? " - " : "")).X > (float)num5)
			{
				text += "...";
			}
			string text3 = (flag2 ? text.Replace(" [D]", "") : text);
			b.DrawString(Game1.smallFont, text3, new Vector2((float)num, (float)num4), val);
			if (flag2)
			{
				float x = Game1.smallFont.MeasureString(text3).X;
				Vector2 val4 = default(Vector2);
				((Vector2)(ref val4))._002Ector((float)num + x, (float)num4);
				Vector2 val5 = Game1.smallFont.MeasureString(" [D]");
				b.DrawString(Game1.smallFont, " [D]", val4, Color.Red);
				disabledMarkerAreas[buttonIndex] = new Rectangle((int)val4.X, (int)val4.Y, (int)val5.X, (int)val5.Y);
			}
			else
			{
				disabledMarkerAreas[buttonIndex] = Rectangle.Empty;
			}
			if (!showModNameInList)
			{
				if (flag)
				{
					string text4 = "[" + Translation.op_Implicit(MHEventsListMod.I18n.Get("list.seen")) + "]";
					int num6 = eventBoxX + eventBoxWidth - 240;
					b.DrawString(Game1.smallFont, text4, new Vector2((float)num6, (float)num4), Color.ForestGreen);
				}
				if (evt.HasInvalidScript)
				{
					string text5 = Translation.op_Implicit(MHEventsListMod.I18n.Get("list.nullScript"));
					int num7 = eventBoxX + eventBoxWidth - (flag ? 120 : 240);
					b.DrawString(Game1.smallFont, text5, new Vector2((float)num7, (float)num4), Color.OrangeRed);
				}
			}
			num4 += num3 + 2;
		}
		string text6 = evt.GetTranslatedLocation();
		while (Game1.smallFont.MeasureString(text6).X > (float)(num2 - 100) && text6.Length > 5)
		{
			text6 = text6.Substring(0, text6.Length - 1);
		}
		if (text6.Length < evt.GetTranslatedLocation()?.Length)
		{
			text6 += "...";
		}
		b.DrawString(Game1.smallFont, text6, new Vector2((float)num, (float)num4), val2);
		if (eventListFormat != EventListFormat.ThreeLines || MHEventsListMod.Config.ShowModNameInList)
		{
			if (flag)
			{
				string text7 = "[" + Translation.op_Implicit(MHEventsListMod.I18n.Get("list.seen")) + "]";
				int num8 = eventBoxX + eventBoxWidth - 240;
				b.DrawString(Game1.smallFont, text7, new Vector2((float)num8, (float)num4), Color.ForestGreen);
			}
			if (evt.HasInvalidScript)
			{
				string text8 = Translation.op_Implicit(MHEventsListMod.I18n.Get("list.nullScript"));
				int num9 = eventBoxX + eventBoxWidth - (flag ? 120 : 240);
				b.DrawString(Game1.smallFont, text8, new Vector2((float)num9, (float)num4), Color.OrangeRed);
			}
		}
		num4 += num3 + 2;
		if (eventListFormat == EventListFormat.TwoLines || eventListFormat == EventListFormat.ThreeLines)
		{
			string text9 = evt.GetConditionsSummary();
			if (!string.IsNullOrEmpty(text9))
			{
				while (Game1.smallFont.MeasureString(text9).X > (float)num2 && text9.Length > 3)
				{
					text9 = text9.Substring(0, text9.Length - 1);
				}
				if (text9.Length < evt.GetConditionsSummary()?.Length)
				{
					text9 += "...";
				}
				b.DrawString(Game1.smallFont, text9, new Vector2((float)num, (float)num4), val3);
			}
		}
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		int y = yPos + 24 + 10;
		bool flag3 = ((ClickableComponent)detailButtons[buttonIndex]).containsPoint(mouseX, mouseY);
		((ClickableComponent)detailButtons[buttonIndex]).bounds.Y = y;
		detailButtons[buttonIndex].draw(b, Color.White * (flag3 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		if (MHEventsListMod.Config.ShowGoToLocationButton)
		{
			bool flag4 = ((ClickableComponent)goToButtons[buttonIndex]).containsPoint(mouseX, mouseY);
			((ClickableComponent)goToButtons[buttonIndex]).bounds.Y = y;
			goToButtons[buttonIndex].draw(b, Color.White * (flag4 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		}
		bool flag5 = ((ClickableComponent)hideButtons[buttonIndex]).containsPoint(mouseX, mouseY);
		((ClickableComponent)hideButtons[buttonIndex]).bounds.Y = y;
		Rectangle sourceRect = default(Rectangle);
		float baseScale;
		Color val6;
		if (MHEventsListMod.Config.ShowPlayInListInsteadOfHide)
		{
			((Rectangle)(ref sourceRect))._002Ector(310, 392, 16, 16);
			baseScale = 2.5f;
			val6 = Color.Gold;
		}
		else
		{
			bool flag6 = hiddenEventIds.Contains(evt.Id);
			if (filterMode == 1)
			{
				((Rectangle)(ref sourceRect))._002Ector(310, 392, 16, 16);
				baseScale = 2.5f;
				val6 = Color.LightGreen;
			}
			else
			{
				((Rectangle)(ref sourceRect))._002Ector(322, 498, 12, 12);
				baseScale = 2.8f;
				val6 = (flag6 ? Color.Orange : Color.White);
			}
		}
		hideButtons[buttonIndex].sourceRect = sourceRect;
		hideButtons[buttonIndex].baseScale = baseScale;
		hideButtons[buttonIndex].draw(b, val6 * (flag5 ? 0.7f : 1f), 0.88f, 0, 0, 0);
	}

	private void DrawTooltips(SpriteBatch b)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		for (int i = 0; i < 6; i++)
		{
			int num = startIndex + i;
			if (num < filteredEvents.Count)
			{
				if (((ClickableComponent)detailButtons[i]).containsPoint(mouseX, mouseY))
				{
					IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("list.tip.detail")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
					break;
				}
				if (MHEventsListMod.Config.ShowGoToLocationButton && ((ClickableComponent)goToButtons[i]).containsPoint(mouseX, mouseY))
				{
					IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("list.tip.goTo")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
					break;
				}
				if (((ClickableComponent)hideButtons[i]).containsPoint(mouseX, mouseY))
				{
					string text = ((!MHEventsListMod.Config.ShowPlayInListInsteadOfHide) ? Translation.op_Implicit((filterMode == 1) ? MHEventsListMod.I18n.Get("list.tip.show") : MHEventsListMod.I18n.Get("list.tip.hide")) : Translation.op_Implicit(MHEventsListMod.I18n.Get("list.tip.play")));
					IClickableMenu.drawToolTip(b, text, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
					break;
				}
				if (disabledMarkerAreas[i] != Rectangle.Empty && ((Rectangle)(ref disabledMarkerAreas[i])).Contains(mouseX, mouseY))
				{
					IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("list.disabledByCP.tooltip")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
					break;
				}
				continue;
			}
			break;
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		((IClickableMenu)this).receiveLeftClick(x, y, playSound);
		Rectangle val = default(Rectangle);
		((Rectangle)(ref val))._002Ector(searchBox.X, searchBox.Y, searchBox.Width, searchBox.Height);
		if (((Rectangle)(ref val)).Contains(x, y))
		{
			SelectSearchBox(explicitly: true);
			return;
		}
		DeselectSearchBox();
		if (!string.IsNullOrEmpty(searchBox.Text) && ((ClickableComponent)clearSearchButton).containsPoint(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			searchBox.Text = "";
			ApplySearch();
			return;
		}
		if (((Rectangle)(ref npcSearchBounds)).Contains(x, y))
		{
			SelectNpcSearchBox();
			return;
		}
		DeselectNpcSearchBox();
		if (((ClickableComponent)scrollBar).containsPoint(x, y) || ((Rectangle)(ref scrollBarRunner)).Contains(x, y))
		{
			scrollBarHeld = true;
			((IClickableMenu)this).leftClickHeld(x, y);
			return;
		}
		if (IsAndroid)
		{
			Rectangle val2 = default(Rectangle);
			((Rectangle)(ref val2))._002Ector(eventBoxX, eventBoxY, eventBoxWidth, eventBoxHeight);
			if (((Rectangle)(ref val2)).Contains(x, y))
			{
				bool flag = false;
				for (int i = 0; i < 6; i++)
				{
					if (((ClickableComponent)detailButtons[i]).containsPoint(x, y) || ((ClickableComponent)goToButtons[i]).containsPoint(x, y) || ((ClickableComponent)hideButtons[i]).containsPoint(x, y))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					isDraggingEventList = true;
					lastDragY = y;
					dragAccumulator = 0;
				}
			}
			if (x >= optionsPanelX && x <= optionsPanelX + optionsPanelWidth && y >= base.yPositionOnScreen + 130 && y <= base.yPositionOnScreen + 130 + 256 && !((Rectangle)(ref npcSearchBounds)).Contains(x, y))
			{
				bool flag2 = false;
				for (int j = 0; j < npcSlots.Count; j++)
				{
					if (npcSlots[j].containsPoint(x, y))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					isDraggingNpcList = true;
					lastDragY = y;
					dragAccumulator = 0;
				}
			}
		}
		List<string> list = (string.IsNullOrEmpty(npcSearchText) ? npcList : npcList.Where((string n) => n.ToLower().Contains(npcSearchText.ToLower())).ToList());
		for (int num = 0; num < 7; num++)
		{
			int num2 = num + npcScrollOffset;
			if (num2 >= list.Count)
			{
				break;
			}
			if (num < npcSlots.Count && npcSlots[num].containsPoint(x, y))
			{
				Game1.playSound("smallSelect", (int?)null);
				string item = list[num2];
				int num3 = npcList.IndexOf(item);
				selectedNpcIndex = ((selectedNpcIndex == num3) ? (-1) : num3);
				lastSelectedNpcIndex = selectedNpcIndex;
				RefreshEventList();
				return;
			}
		}
		if (((Rectangle)(ref sortButtonBounds)).Contains(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			int num4 = (MHEventsListMod.Config.ShowModNameInList ? 5 : 4);
			sortBy = (sortBy + 1) % num4;
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref sortOrderBounds)).Contains(x, y) && sortBy > 0)
		{
			Game1.playSound("drumkit6", (int?)null);
			sortAscending = !sortAscending;
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref heartsMinusBounds)).Contains(x, y) && selectedNpcIndex >= 0)
		{
			Game1.playSound("drumkit6", (int?)null);
			maxHeartsFilter = Math.Max(0, maxHeartsFilter - 1);
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref heartsPlusBounds)).Contains(x, y) && selectedNpcIndex >= 0)
		{
			Game1.playSound("drumkit6", (int?)null);
			maxHeartsFilter = Math.Min(14, maxHeartsFilter + 1);
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref clearFilterBounds)).Contains(x, y))
		{
			Game1.playSound("bigDeSelect", (int?)null);
			selectedNpcIndex = -1;
			lastSelectedNpcIndex = -1;
			maxHeartsFilter = 14;
			searchBox.Text = "";
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref toggleSeenBounds)).Contains(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			if (filterMode == 0)
			{
				filterMode = 2;
			}
			else if (filterMode == 2)
			{
				filterMode = 1;
			}
			else if (filterMode == 1)
			{
				filterMode = 3;
			}
			else if (filterMode == 3)
			{
				filterMode = 4;
			}
			else
			{
				filterMode = 0;
			}
			lastFilterMode = filterMode;
			RefreshEventList();
			return;
		}
		if (((Rectangle)(ref toggleRelationshipsBounds)).Contains(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			showOnlyRelationships = !showOnlyRelationships;
			RefreshEventList();
			return;
		}
		for (int num5 = 0; num5 < 6; num5++)
		{
			int num6 = startIndex + num5;
			if (num6 >= filteredEvents.Count)
			{
				break;
			}
			EventData eventData = filteredEvents[num6];
			if (((ClickableComponent)hideButtons[num5]).containsPoint(x, y))
			{
				if (MHEventsListMod.Config.ShowPlayInListInsteadOfHide)
				{
					Game1.playSound("newArtifact", (int?)null);
					TryPlayEventFromList(eventData);
					break;
				}
				Game1.playSound("drumkit6", (int?)null);
				if (filterMode == 1)
				{
					hiddenEventIds.Remove(eventData.Id);
				}
				else
				{
					hiddenEventIds.Add(eventData.Id);
				}
				SaveHiddenEvents();
				startIndex = 0;
				filteredEvents.Clear();
				RefreshEventList();
				break;
			}
			if (((ClickableComponent)detailButtons[num5]).containsPoint(x, y))
			{
				Game1.playSound("bigSelect", (int?)null);
				Game1.activeClickableMenu = (IClickableMenu)(object)new EventDetailMenu(eventData, (IClickableMenu)(object)this);
				break;
			}
			if (MHEventsListMod.Config.ShowGoToLocationButton && ((ClickableComponent)goToButtons[num5]).containsPoint(x, y))
			{
				Game1.playSound("drumkit6", (int?)null);
				WarpToEvent(eventData);
				break;
			}
		}
	}

	private void WarpToEvent(EventData evt)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		try
		{
			GameLocation locationFromName = Game1.getLocationFromName(evt.LocationName);
			if (locationFromName != null && ((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps).Count > 0)
			{
				((IClickableMenu)this).exitThisMenu(true);
				Game1.warpFarmer(evt.LocationName, ((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps)[0].X, ((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps)[0].Y, false);
				Game1.addHUDMessage(new HUDMessage("Warped to " + evt.GetTranslatedLocation(), 2));
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage("Location not found", 3));
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Warp error: " + ex.Message, (LogLevel)4);
			Game1.addHUDMessage(new HUDMessage("Warp failed", 3));
		}
	}

	public override void leftClickHeld(int x, int y)
	{
		((IClickableMenu)this).leftClickHeld(x, y);
		if (scrollBarHeld && filteredEvents.Count > 6)
		{
			int value = y - scrollBarRunner.Y;
			value = Math.Clamp(value, 0, scrollBarRunner.Height);
			int num = filteredEvents.Count - 6;
			float num2 = (float)value / (float)scrollBarRunner.Height;
			startIndex = (int)(num2 * (float)num);
			endIndex = startIndex + 6;
			UpdateScrollBar();
		}
		if (!IsAndroid)
		{
			return;
		}
		int num3 = lastDragY - y;
		lastDragY = y;
		if (isDraggingEventList && filteredEvents.Count > 6)
		{
			dragAccumulator += num3;
			int num4 = 44;
			while (dragAccumulator >= num4)
			{
				Scroll(up: false);
				dragAccumulator -= num4;
			}
			while (dragAccumulator <= -num4)
			{
				Scroll(up: true);
				dragAccumulator += num4;
			}
		}
		if (!isDraggingNpcList)
		{
			return;
		}
		dragAccumulator += num3;
		int num5 = 16;
		List<string> list = (string.IsNullOrEmpty(npcSearchText) ? npcList : npcList.Where((string n) => n.ToLower().Contains(npcSearchText.ToLower())).ToList());
		while (dragAccumulator >= num5)
		{
			if (npcScrollOffset < list.Count - 8 + 2)
			{
				npcScrollOffset++;
			}
			dragAccumulator -= num5;
		}
		while (dragAccumulator <= -num5)
		{
			if (npcScrollOffset > 0)
			{
				npcScrollOffset--;
			}
			dragAccumulator += num5;
		}
	}

	public override void releaseLeftClick(int x, int y)
	{
		((IClickableMenu)this).releaseLeftClick(x, y);
		scrollBarHeld = false;
		isDraggingEventList = false;
		isDraggingNpcList = false;
		dragAccumulator = 0;
	}

	public override void receiveScrollWheelAction(int direction)
	{
		((IClickableMenu)this).receiveScrollWheelAction(direction);
		int mouseX = Game1.getMouseX();
		if (mouseX < eventBoxX)
		{
			if (direction > 0 && npcScrollOffset > 0)
			{
				npcScrollOffset--;
			}
			else if (direction < 0 && npcScrollOffset < npcList.Count - 8 + 2)
			{
				npcScrollOffset++;
			}
		}
		else
		{
			Scroll(direction > 0);
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		if (npcSearchSelected)
		{
			if ((int)key == 27)
			{
				npcSearchSelected = false;
			}
		}
		else if (searchBox.Selected)
		{
			if ((int)key == 27)
			{
				searchBox.Selected = false;
			}
		}
		else
		{
			((IClickableMenu)this).receiveKeyPress(key);
		}
	}

	public override void update(GameTime time)
	{
		((IClickableMenu)this).update(time);
		TextEntryManager.Update();
		if (TextEntryManager.JustClosed())
		{
			if (searchBox.Selected)
			{
				DeselectSearchBox();
			}
			if (npcSearchSelected)
			{
				DeselectNpcSearchBox();
			}
		}
		if (IsSearchBoxSelectedExplicitly && !searchBox.Selected)
		{
			DeselectSearchBox();
		}
		if (searchBox.Text != lastSearchText)
		{
			lastSearchText = searchBox.Text;
			RefreshEventList();
		}
		if (IsAndroid && npcSearchBox != null && npcSearchBox.Text != npcSearchText)
		{
			npcSearchText = npcSearchBox.Text;
			npcScrollOffset = 0;
		}
	}

	private void SelectSearchBox(bool explicitly)
	{
		searchBox.Selected = true;
		IsSearchBoxSelectedExplicitly = explicitly;
		if (npcSearchSelected)
		{
			DeselectNpcSearchBox();
		}
		if (IsAndroid)
		{
			typeof(TextBox).GetMethod("ShowAndroidKeyboard", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(searchBox, null);
		}
		else
		{
			Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)(object)searchBox;
		}
	}

	private void DeselectSearchBox()
	{
		Game1.closeTextEntry();
		searchBox.Selected = false;
		IsSearchBoxSelectedExplicitly = false;
		if (IsAndroid)
		{
			typeof(TextBox).GetMethod("HideStatusBar", BindingFlags.Instance | BindingFlags.Public)?.Invoke(searchBox, null);
		}
		else if ((object)Game1.keyboardDispatcher.Subscriber == searchBox)
		{
			Game1.keyboardDispatcher.Subscriber = null;
		}
	}

	private void SelectNpcSearchBox()
	{
		if (searchBox.Selected)
		{
			DeselectSearchBox();
		}
		npcSearchSelected = true;
		if (IsAndroid)
		{
			npcSearchBox.Text = npcSearchText;
			typeof(TextBox).GetMethod("ShowAndroidKeyboard", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(npcSearchBox, null);
		}
		else
		{
			Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)(object)this;
		}
	}

	private void DeselectNpcSearchBox()
	{
		Game1.closeTextEntry();
		npcSearchSelected = false;
		if ((object)Game1.keyboardDispatcher.Subscriber == this)
		{
			Game1.keyboardDispatcher.Subscriber = null;
		}
		if (IsAndroid)
		{
			typeof(TextBox).GetMethod("HideStatusBar", BindingFlags.Instance | BindingFlags.Public)?.Invoke(npcSearchBox, null);
		}
	}

	protected override void cleanupBeforeExit()
	{
		if (!IsAndroid && ((object)Game1.keyboardDispatcher.Subscriber == this || (object)Game1.keyboardDispatcher.Subscriber == searchBox))
		{
			Game1.keyboardDispatcher.Subscriber = null;
		}
		((IClickableMenu)this).cleanupBeforeExit();
	}

	private void TryPlayEventFromList(EventData evt)
	{
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Expected O, but got Unknown
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Expected O, but got Unknown
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Expected O, but got Unknown
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!MHEventsListMod.Config.MarkAsSeenWhenPlaying)
			{
				MHEventsListMod.EventsToUnmark.Add(evt.Id);
			}
			if (((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(evt.Id))
			{
				((NetHashSet<string>)(object)Game1.player.eventsSeen).Remove(evt.Id);
			}
			string text = null;
			GameLocation locationFromName = Game1.getLocationFromName(evt.LocationName);
			string text2 = default(string);
			Dictionary<string, string> dictionary = default(Dictionary<string, string>);
			if (locationFromName != null && locationFromName.TryGetLocationEvents(ref text2, ref dictionary))
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					if (item.Key == evt.Id || item.Key.StartsWith(evt.Id + "/"))
					{
						text = item.Value;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = evt.GetEventScript();
				if (!string.IsNullOrEmpty(text) && text.Contains("{{") && text.Contains("}}"))
				{
					if (!string.IsNullOrEmpty(evt.ModFolderPath))
					{
						text = I18nResolver.ResolveI18nTokens(text, evt.ModFolderPath);
					}
					if (text.Contains("{{") && text.Contains("}}"))
					{
						ContentPatcherIntegration contentPatcher = MHEventsListMod.ContentPatcher;
						if (contentPatcher != null && contentPatcher.IsReady)
						{
							text = contentPatcher.ResolveTokens(text, evt.ModUniqueId);
						}
						if (text.Contains("{{") && text.Contains("}}"))
						{
							text = I18nResolver.CleanupUnresolvedTokens(text);
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				((IClickableMenu)this).exitThisMenuNoSound();
				Game1.activeClickableMenu = null;
				string text3 = evt.LocationName;
				GameLocation val = Game1.getLocationFromName(text3);
				if (val == null)
				{
					GameLocation currentLocation = Game1.currentLocation;
					text3 = ((currentLocation != null) ? currentLocation.Name : null) ?? "Farm";
					val = Game1.currentLocation ?? Game1.getLocationFromName("Farm");
				}
				GameLocation currentLocation2 = Game1.currentLocation;
				if (((currentLocation2 != null) ? currentLocation2.Name : null) != text3 && val != null)
				{
					int x;
					int num;
					if (((NetList<Warp, NetRef<Warp>>)(object)val.warps).Count > 0)
					{
						x = ((NetList<Warp, NetRef<Warp>>)(object)val.warps)[0].X;
						num = ((NetList<Warp, NetRef<Warp>>)(object)val.warps)[0].Y - 1;
					}
					else
					{
						x = ((Character)Game1.player).TilePoint.X;
						num = ((Character)Game1.player).TilePoint.Y;
					}
					Game1.warpFarmer(text3, x, num, false);
				}
				string finalScript = text;
				Game1.delayedActions.Add(new DelayedAction(300, (Action)delegate
				{
					//IL_001e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Expected O, but got Unknown
					try
					{
						GameLocation currentLocation3 = Game1.currentLocation;
						if (currentLocation3 != null)
						{
							currentLocation3.startEvent(new Event(finalScript, (string)null, evt.Id, (Farmer)null));
						}
					}
					catch (Exception ex2)
					{
						MHEventsListMod.Monitor.Log("Event play error: " + ex2.Message, (LogLevel)4);
					}
				}));
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventNotFound")), 3));
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Play event error: " + ex.Message, (LogLevel)4);
			Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventError")), 3));
		}
	}

	private void UnmarkPendingEvents()
	{
		if (MHEventsListMod.EventsToUnmark.Count <= 0)
		{
			return;
		}
		foreach (string item in MHEventsListMod.EventsToUnmark)
		{
			if (((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(item))
			{
				((NetHashSet<string>)(object)Game1.player.eventsSeen).Remove(item);
				if (MHEventsListMod.Config.VerboseLogging)
				{
					MHEventsListMod.Monitor.Log("Unmarked event " + item + " as seen (MarkAsSeenWhenPlaying = false)", (LogLevel)1);
				}
			}
		}
		MHEventsListMod.EventsToUnmark.Clear();
	}
}
