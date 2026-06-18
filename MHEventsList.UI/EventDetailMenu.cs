using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MHEventsList.Core;
using MHEventsList.Integration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using xTile.Dimensions;

namespace MHEventsList.UI;

public class EventDetailMenu : IClickableMenu
{
	private readonly EventData eventData;

	private readonly IClickableMenu previousMenu;

	private readonly Texture2D letterTexture;

	private ClickableTextureComponent toggleSeenButton;

	private ClickableTextureComponent playEventButton;

	private ClickableTextureComponent debugButton;

	private ClickableTextureComponent goToButton;

	private ClickableTextureComponent hideButton;

	private ClickableTextureComponent legendButton;

	private ClickableTextureComponent actionsButton;

	private ClickableTextureComponent openJsonButton;

	private ClickableTextureComponent pinButton;

	private Dictionary<string, bool> conditionCheckCache;

	private readonly List<Tuple<Rectangle, string>> tooltipAreas = new List<Tuple<Rectangle, string>>();

	private bool showingLegend;

	private Rectangle legendCloseBounds;

	private int legendScrollOffset;

	private Rectangle legendScrollArea;

	private bool showingActions;

	private Rectangle actionsCloseBounds;

	private int actionsScrollOffset;

	private Rectangle actionsScrollArea;

	private int maxActionsScroll;

	private bool showingConditionDetail;

	private string selectedConditionCode;

	private string selectedConditionTranslation;

	private bool selectedConditionMet;

	private Rectangle conditionDetailCloseBounds;

	private bool showingCPConditionsPopup;

	private Rectangle cpConditionsPopupBounds;

	private Rectangle cpConditionsCloseBounds;

	private Rectangle cpConditionsClickArea;

	private readonly List<Tuple<Rectangle, string, string, bool>> conditionRows = new List<Tuple<Rectangle, string, string, bool>>();

	private int contentX;

	private int contentY;

	private int contentWidth;

	private int contentHeight;

	private int tableCheckWidth = 36;

	private int tableCodeWidth = 170;

	private int tableTranslationWidth;

	private int columnGap = 12;

	private Rectangle conditionsArea;

	private int conditionsScrollOffset;

	private int maxConditionsScroll;

	public EventDetailMenu(EventData evt, IClickableMenu previousMenu = null)
		: base((Game1.uiViewport.Width - 1030) / 2, (Game1.uiViewport.Height - 680) / 2, 1030, 680, true)
	{
		eventData = evt;
		this.previousMenu = previousMenu;
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		conditionCheckCache = new Dictionary<string, bool>();
		contentX = base.xPositionOnScreen + 50;
		contentY = base.yPositionOnScreen + 90;
		contentWidth = base.width - 100;
		contentHeight = base.height - 180;
		tableTranslationWidth = contentWidth - tableCodeWidth - tableCheckWidth - 40;
		InitializeButtons();
		UnmarkPendingEvents();
	}

	private void InitializeButtons()
	{
		InitializeButtonsHorizontal();
	}

	private void InitializeButtonsHorizontal()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Expected O, but got Unknown
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Expected O, but got Unknown
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Expected O, but got Unknown
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Expected O, but got Unknown
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Expected O, but got Unknown
		int num = base.yPositionOnScreen + base.height - 65;
		int num2 = 70;
		int num3 = 5;
		if (MHEventsListMod.Config.ShowPlayEventButton)
		{
			num3++;
		}
		if (MHEventsListMod.Config.ShowGoToLocationButton)
		{
			num3++;
		}
		if (MHEventsListMod.Config.ShowDebugButton)
		{
			num3++;
		}
		if (MHEventsListMod.Config.ShowEventActionsButton)
		{
			num3++;
		}
		int num4 = num2 * (num3 - 1);
		int num5 = base.xPositionOnScreen + (base.width - num4) / 2;
		int num6 = num5;
		toggleSeenButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 0.75f, false);
		num6 += num2;
		if (MHEventsListMod.Config.ShowPlayEventButton)
		{
			playEventButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(310, 392, 16, 16), 3f, false);
			num6 += num2;
		}
		if (MHEventsListMod.Config.ShowGoToLocationButton)
		{
			goToButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(0, 192, 64, 64), 0.75f, false);
			num6 += num2;
		}
		if (MHEventsListMod.Config.ShowDebugButton)
		{
			debugButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 3f, false);
			num6 += num2;
		}
		if (MHEventsListMod.Config.ShowEventActionsButton)
		{
			actionsButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(0, 428, 10, 10), 4.8f, false);
			num6 += num2;
		}
		pinButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(434, 475, 9, 9), 5.3f, false);
		num6 += num2;
		hideButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3.5f, false);
		num6 += num2;
		legendButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 3f, false);
		num6 += num2;
		openJsonButton = new ClickableTextureComponent(new Rectangle(num6, num, 48, 48), Game1.mouseCursors, new Rectangle(127, 412, 10, 11), 4f, false);
	}

	private void InitializeButtonsVertical()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		int num = base.xPositionOnScreen + base.width - 70;
		int num2 = base.yPositionOnScreen + 90;
		int num3 = 58;
		int num4 = num2;
		toggleSeenButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 0.75f, false);
		num4 += num3;
		if (MHEventsListMod.Config.ShowPlayEventButton)
		{
			playEventButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(310, 392, 16, 16), 3f, false);
			num4 += num3;
		}
		if (MHEventsListMod.Config.ShowGoToLocationButton)
		{
			goToButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(0, 192, 64, 64), 0.75f, false);
			num4 += num3;
		}
		if (MHEventsListMod.Config.ShowDebugButton)
		{
			debugButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 3f, false);
			num4 += num3;
		}
		if (MHEventsListMod.Config.ShowEventActionsButton)
		{
			actionsButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(0, 428, 10, 10), 4.8f, false);
			num4 += num3;
		}
		hideButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3.5f, false);
		num4 += num3;
		legendButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(208, 320, 16, 16), 3f, false);
		num4 += num3;
		openJsonButton = new ClickableTextureComponent(new Rectangle(num, num4, 48, 48), Game1.mouseCursors, new Rectangle(127, 412, 10, 11), 4f, false);
	}

	public override void draw(SpriteBatch b)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		tooltipAreas.Clear();
		conditionRows.Clear();
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.75f);
		Color val = (Color)(MHEventsListMod.Config.UseDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, val, 1f, true, -1f);
		string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.title"));
		SpriteText.drawStringHorizontallyCenteredAt(b, text, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 15, 999999, -1, 999999, 1f, 0.88f, false, (Color?)null, 99999);
		DrawEventDetails(b);
		DrawButtons(b);
		((IClickableMenu)this).draw(b);
		if (showingLegend)
		{
			DrawLegendPopup(b);
		}
		if (showingCPConditionsPopup)
		{
			DrawCPConditionsPopup(b);
		}
		if (showingActions)
		{
			DrawActionsPopup(b);
		}
		if (showingConditionDetail)
		{
			DrawConditionDetailPopup(b);
		}
		DrawTooltips(b);
		((IClickableMenu)this).drawMouse(b, false, -1);
	}

	private void DrawEventDetails(SpriteBatch b)
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
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_072a: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_0743: Unknown result type (might be due to invalid IL or missing references)
		//IL_0778: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0801: Unknown result type (might be due to invalid IL or missing references)
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_082c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0831: Unknown result type (might be due to invalid IL or missing references)
		//IL_0852: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0528: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0561: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a11: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b09: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		Color val = (useDarkTheme ? Color.White : Color.Black);
		Color val2 = (useDarkTheme ? new Color(200, 150, 100) : new Color(86, 22, 12));
		Color val3 = (useDarkTheme ? Color.LightGray : Color.Gray);
		int num = contentY;
		int num2 = 32;
		int num3 = contentX + 120;
		if (!string.IsNullOrEmpty(eventData.DisplayName))
		{
			b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.logname")), new Vector2((float)contentX, (float)num), val2);
			b.DrawString(Game1.smallFont, eventData.DisplayName, new Vector2((float)num3, (float)num), val);
			num += num2;
		}
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.id")), new Vector2((float)contentX, (float)num), val2);
		b.DrawString(Game1.smallFont, eventData.Id, new Vector2((float)num3, (float)num), val);
		num += num2;
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.location")), new Vector2((float)contentX, (float)num), val2);
		string translatedLocation = eventData.GetTranslatedLocation();
		b.DrawString(Game1.smallFont, translatedLocation, new Vector2((float)num3, (float)num), val);
		num += num2;
		string text = ((!string.IsNullOrEmpty(eventData.ModName)) ? eventData.ModName : Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.modVanilla")));
		Color val4 = ((!string.IsNullOrEmpty(eventData.ModName)) ? new Color(100, 149, 237) : new Color(144, 238, 144));
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.mod")), new Vector2((float)contentX, (float)num), val2);
		b.DrawString(Game1.smallFont, text, new Vector2((float)num3, (float)num), val4);
		num += num2;
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.seen")), new Vector2((float)contentX, (float)num), val2);
		bool flag = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id);
		string text2 = (flag ? "Sí" : "No");
		Color val5 = (flag ? Color.ForestGreen : (useDarkTheme ? Color.Salmon : Color.DarkRed));
		b.DrawString(Game1.smallFont, text2, new Vector2((float)num3, (float)num), val5);
		num += num2;
		List<string> requiredNpcs = eventData.RequiredNpcs;
		if (requiredNpcs != null && requiredNpcs.Count > 0)
		{
			b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.npcsInvolved")), new Vector2((float)contentX, (float)num), val2);
			List<string> list = new List<string>();
			foreach (string item3 in requiredNpcs)
			{
				if (!IsValidNpcForDisplay(item3))
				{
					continue;
				}
				NPC characterFromName = Game1.getCharacterFromName(item3, false, false);
				string name = ((characterFromName != null) ? ((Character)characterFromName).displayName : null) ?? ((characterFromName != null) ? ((Character)characterFromName).Name : null) ?? item3;
				if (IsValidNpcForDisplay(name))
				{
					name = SanitizeDisplayName(name);
					if (!string.IsNullOrWhiteSpace(name))
					{
						list.Add(name);
					}
				}
			}
			if (list.Count > 0)
			{
				string text3 = string.Join(", ", list);
				int num4 = contentWidth - 140;
				while (Game1.smallFont.MeasureString(text3).X > (float)num4 && text3.Length > 10)
				{
					text3 = text3.Substring(0, text3.Length - 1);
				}
				if (text3.Length < string.Join(", ", list).Length)
				{
					text3 += "...";
				}
				b.DrawString(Game1.smallFont, text3, new Vector2((float)num3, (float)num), val);
				num += num2;
			}
		}
		if (eventData.IsFromContentPatcher)
		{
			string text4 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.cpConditions"));
			b.DrawString(Game1.smallFont, text4, new Vector2((float)contentX, (float)num), val2);
			string text5;
			Color val6;
			if (eventData.HasWhenConditions)
			{
				bool flag2 = eventData.AreWhenConditionsMet();
				text5 = Translation.op_Implicit(flag2 ? MHEventsListMod.I18n.Get("detail.cpConditions.met") : MHEventsListMod.I18n.Get("detail.cpConditions.notMet"));
				val6 = (flag2 ? Color.ForestGreen : (useDarkTheme ? Color.Salmon : Color.DarkRed));
				Vector2 val7 = Game1.smallFont.MeasureString(text4);
				Vector2 val8 = Game1.smallFont.MeasureString(text5);
				cpConditionsClickArea = new Rectangle(contentX, num, (int)((float)(num3 - contentX) + val8.X), num2);
				b.Draw(Game1.staminaRect, new Rectangle(num3, num + num2 - 4, (int)val8.X, 1), val6 * 0.5f);
			}
			else
			{
				text5 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.cpConditions.none"));
				val6 = val;
				cpConditionsClickArea = Rectangle.Empty;
			}
			b.DrawString(Game1.smallFont, text5, new Vector2((float)num3, (float)num), val6);
			num += num2;
		}
		else
		{
			cpConditionsClickArea = Rectangle.Empty;
		}
		if (eventData.HasInvalidScript)
		{
			string text6 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.invalidScript"));
			b.DrawString(Game1.smallFont, text6, new Vector2((float)contentX, (float)num), Color.OrangeRed);
			num += num2;
			string text7 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.nullScriptMessage"));
			int num5 = contentWidth - 40;
			string[] array = text7.Split(' ');
			string text8 = "";
			string[] array2 = array;
			foreach (string text9 in array2)
			{
				string text10 = (string.IsNullOrEmpty(text8) ? text9 : (text8 + " " + text9));
				if (Game1.smallFont.MeasureString(text10).X > (float)num5 && !string.IsNullOrEmpty(text8))
				{
					b.DrawString(Game1.smallFont, text8, new Vector2((float)contentX, (float)num), val);
					num += num2;
					text8 = text9;
				}
				else
				{
					text8 = text10;
				}
			}
			if (!string.IsNullOrEmpty(text8))
			{
				b.DrawString(Game1.smallFont, text8, new Vector2((float)contentX, (float)num), val);
				num += num2;
			}
		}
		num += 10;
		b.Draw(Game1.staminaRect, new Rectangle(contentX, num, contentWidth - 20, 2), (useDarkTheme ? Color.Gray : Color.Brown) * 0.4f);
		num += 10;
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.conditions")), new Vector2((float)contentX, (float)num), val2);
		num += 24;
		int num6 = contentX;
		int num7 = num6 + tableCheckWidth + columnGap;
		int num8 = num7 + tableCodeWidth + columnGap;
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableOk")), new Vector2((float)num6, (float)num), val3);
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableCode")), new Vector2((float)num7, (float)num), val3);
		b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableDescription")), new Vector2((float)num8, (float)num), val3);
		num += 26;
		b.Draw(Game1.staminaRect, new Rectangle(num6, num, contentWidth - 20, 1), (useDarkTheme ? Color.Gray : Color.Brown) * 0.3f);
		num += 4;
		int num9 = num;
		conditionsArea = new Rectangle(num6, num9, contentWidth - 20, contentHeight - (num9 - contentY) - 10);
		if (!string.IsNullOrEmpty(eventData.RawPreconditions))
		{
			string[] array3 = eventData.RawPreconditions.Split('/');
			int num10 = 34;
			int num11 = 0;
			for (int j = 0; j < array3.Length; j++)
			{
				if (!string.IsNullOrWhiteSpace(array3[j]))
				{
					num11++;
				}
			}
			int num12 = num11 * num10;
			maxConditionsScroll = Math.Max(0, num12 - conditionsArea.Height);
			int num13 = num - conditionsScrollOffset;
			int num14 = 0;
			Rectangle item = default(Rectangle);
			for (int k = 0; k < array3.Length; k++)
			{
				string text11 = array3[k].Trim();
				if (string.IsNullOrEmpty(text11))
				{
					continue;
				}
				if (num13 + num10 < num9 || num13 > num9 + conditionsArea.Height)
				{
					num13 += num10;
					num14++;
					continue;
				}
				bool flag3 = CheckCondition(text11);
				if (num14 % 2 == 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle(num6, num13, contentWidth - 20, num10 - 2), (useDarkTheme ? Color.Gray : Color.Brown) * 0.08f);
				}
				DrawCheckIcon(b, num6 + 4, num13 + 4, flag3);
				string text12 = ((text11.Length > 12) ? (text11.Substring(0, 12) + "...") : text11);
				b.DrawString(Game1.smallFont, text12, new Vector2((float)num7, (float)(num13 + 4)), val);
				string text13 = ConditionTranslator.Translate(text11);
				int num15 = contentWidth - tableCheckWidth - tableCodeWidth - columnGap * 2 - 30;
				string text14 = text13;
				while (Game1.smallFont.MeasureString(text14).X > (float)num15 && text14.Length > 3)
				{
					text14 = text14.Substring(0, text14.Length - 1);
				}
				if (text14 != text13)
				{
					text14 += "...";
				}
				b.DrawString(Game1.smallFont, text14, new Vector2((float)num8, (float)(num13 + 4)), val);
				if (num13 >= num9 && num13 <= num9 + conditionsArea.Height)
				{
					item._002Ector(num6, num13, contentWidth - 20, num10 - 2);
					string item2 = text11 + "\n\n" + text13;
					tooltipAreas.Add(new Tuple<Rectangle, string>(item, item2));
					conditionRows.Add(new Tuple<Rectangle, string, string, bool>(item, text11, text13, flag3));
				}
				num13 += num10;
				num14++;
			}
			if (maxConditionsScroll > 0)
			{
				int num16 = Math.Max(20, conditionsArea.Height * conditionsArea.Height / num12);
				int num17 = num9 + (int)((float)conditionsScrollOffset / (float)maxConditionsScroll * (float)(conditionsArea.Height - num16));
				b.Draw(Game1.staminaRect, new Rectangle(contentX + contentWidth - 12, num17, 8, num16), (useDarkTheme ? Color.LightGray : Color.Brown) * 0.5f);
			}
		}
		else
		{
			b.DrawString(Game1.smallFont, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.noConditions")), new Vector2((float)(contentX + 10), (float)num), val3);
		}
	}

	private void DrawCheckIcon(SpriteBatch b, int x, int y, bool isChecked)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Rectangle value = (isChecked ? new Rectangle(128, 256, 64, 64) : new Rectangle(192, 256, 64, 64));
		b.Draw(Game1.mouseCursors, new Rectangle(x, y, 26, 26), (Rectangle?)value, Color.White);
	}

	private void DrawLegendPopup(SpriteBatch b)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0944: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a24: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Unknown result type (might be due to invalid IL or missing references)
		//IL_075c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_077d: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0919: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8c: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		int num = 1000;
		int num2 = 700;
		int num3 = (Game1.uiViewport.Width - num) / 2;
		int num4 = (Game1.uiViewport.Height - num2) / 2;
		Color val = (Color)(useDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), num3, num4, num, num2, val, 1f, true, -1f);
		Rectangle val2 = default(Rectangle);
		val2._002Ector(num3 + num - 48, num4 + 8, 36, 36);
		b.Draw(Game1.mouseCursors, val2, (Rectangle?)new Rectangle(337, 494, 12, 12), Color.White);
		legendCloseBounds = val2;
		string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("legend.title"));
		Vector2 val3 = Game1.dialogueFont.MeasureString(text);
		Color val4 = (useDarkTheme ? new Color(220, 180, 120) : new Color(86, 22, 12));
		b.DrawString(Game1.dialogueFont, text, new Vector2((float)num3 + ((float)num - val3.X) / 2f, (float)(num4 + 15)), val4);
		int num5 = num4 + 65;
		int num6 = num2 - 75;
		legendScrollArea = new Rectangle(num3 + 10, num5, num - 20, num6);
		Rectangle scissorRectangle = default(Rectangle);
		scissorRectangle._002Ector(legendScrollArea.X, legendScrollArea.Y, legendScrollArea.Width, legendScrollArea.Height);
		Rectangle scissorRectangle2 = ((GraphicsResource)b).GraphicsDevice.ScissorRectangle;
		RasterizerState rasterizerState = ((GraphicsResource)b).GraphicsDevice.RasterizerState;
		RasterizerState val5 = new RasterizerState
		{
			ScissorTestEnable = true
		};
		b.End();
		b.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, val5, (Effect)null, (Matrix?)null);
		((GraphicsResource)b).GraphicsDevice.ScissorRectangle = scissorRectangle;
		int num7 = num5 - legendScrollOffset;
		int num8 = num3 + 30;
		int num9 = num3 + num / 2 + 15;
		int num10 = 26;
		Color val6 = (useDarkTheme ? Color.LightSkyBlue : Color.DarkBlue);
		Color val7 = (useDarkTheme ? Color.White : Color.Black);
		List<(Rectangle, string)> list = new List<(Rectangle, string)>();
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		(string, string)[] array = new(string, string)[46]
		{
			("a", "code_a"),
			("A", "code_a_up"),
			("b", "code_b"),
			("B", "code_b_up"),
			("c", "code_c"),
			("C", "code_c_up"),
			("d", "code_d"),
			("D", "code_d_up"),
			("e", "code_e"),
			("f", "code_f"),
			("F", "code_f_up"),
			("g", "code_g"),
			("G", "code_g_up"),
			("h", "code_h"),
			("H", "code_h_up"),
			("i", "code_i"),
			("j", "code_j"),
			("J", "code_j_up"),
			("k", "code_k"),
			("l", "code_l"),
			("L", "code_l_up"),
			("m", "code_m"),
			("M", "code_m_up"),
			("n", "code_n"),
			("N", "code_n_up"),
			("o", "code_o"),
			("O", "code_o_up"),
			("p", "code_p"),
			("q", "code_q"),
			("r", "code_r"),
			("R", "code_r_up"),
			("s", "code_s"),
			("S", "code_s_up"),
			("t", "code_t"),
			("u", "code_u"),
			("U", "code_u_up"),
			("v", "code_v"),
			("w", "code_w"),
			("x", "code_x"),
			("X", "code_x_up"),
			("y", "code_y"),
			("z", "code_z"),
			("*", "code_star"),
			("Hn", "code_hn"),
			("Hl", "code_hl"),
			("Rf", "code_rf")
		};
		int num11 = (array.Length + 1) / 2;
		int num12 = num / 2 - 80;
		Rectangle item3 = default(Rectangle);
		for (int i = 0; i < array.Length; i++)
		{
			(string, string) tuple = array[i];
			string item = tuple.Item1;
			string item2 = tuple.Item2;
			string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("legend." + item2));
			string text3 = text2;
			while (Game1.smallFont.MeasureString(text3).X > (float)num12 && text3.Length > 5)
			{
				text3 = text3.Substring(0, text3.Length - 1);
			}
			if (text3 != text2)
			{
				text3 += "...";
			}
			int num13 = ((i < num11) ? num8 : num9);
			int num14 = ((i < num11) ? i : (i - num11));
			int num15 = num7 + num14 * num10;
			b.DrawString(Game1.smallFont, item, new Vector2((float)num13, (float)num15), val6);
			b.DrawString(Game1.smallFont, ": " + text3, new Vector2((float)(num13 + 20), (float)num15), val7);
			item3._002Ector(num13, num15, num / 2 - 40, num10);
			list.Add((item3, item + ": " + text2));
		}
		int num16 = num7 + num11 * num10 + 20;
		Color val8 = (useDarkTheme ? Color.Yellow : Color.DarkRed);
		string text4 = Translation.op_Implicit(MHEventsListMod.I18n.Get("legend.extended_title"));
		b.DrawString(Game1.smallFont, text4, new Vector2((float)num8, (float)num16), val8);
		string[] array2 = new string[11]
		{
			"ext_festivalday", "ext_skill", "ext_minutesplayed", "ext_isgreenrain", "ext_isfestival", "ext_hasrecipe", "ext_hasprofession", "ext_skillevel", "ext_notactiveevt", "ext_notcommcenter",
			"ext_negations"
		};
		num16 += num10 + 5;
		Rectangle item4 = default(Rectangle);
		for (int j = 0; j < array2.Length; j++)
		{
			string text5 = Translation.op_Implicit(MHEventsListMod.I18n.Get("legend." + array2[j]));
			string text6 = text5;
			int num17 = num - 80;
			while (Game1.smallFont.MeasureString(text6).X > (float)num17 && text6.Length > 5)
			{
				text6 = text6.Substring(0, text6.Length - 1);
			}
			if (text6 != text5)
			{
				text6 += "...";
			}
			int num18 = num16 + j * num10;
			b.DrawString(Game1.smallFont, text6, new Vector2((float)num8, (float)num18), val7);
			item4._002Ector(num8, num18, num - 80, num10);
			list.Add((item4, text5));
		}
		b.End();
		((GraphicsResource)b).GraphicsDevice.ScissorRectangle = scissorRectangle2;
		b.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, rasterizerState, (Effect)null, (Matrix?)null);
		int num19 = num16 + array2.Length * num10 - num5 + 50;
		if (num19 > num6)
		{
			int num20 = num3 + num - 28;
			int num21 = num5;
			int num22 = num6;
			int num23 = 24;
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(403, 383, 6, 6), num20, num21, num23, num22, Color.White, 1f, false, -1f);
			float num24 = (float)legendScrollOffset / (float)(num19 - num6);
			int num25 = Math.Max(40, (int)((float)num22 / (float)num19 * (float)num22));
			int num26 = num21 + (int)((float)(num22 - num25) * num24);
			b.Draw(Game1.menuTexture, new Rectangle(num20 + 4, num26, num23 - 8, num25), (Rectangle?)new Rectangle(403, 383, 6, 6), Color.White);
		}
		if (!legendScrollArea.Contains(mouseX, mouseY))
		{
			return;
		}
		foreach (var item5 in list)
		{
			var (val9, text7) = item5;
			if (val9.Contains(mouseX, mouseY) && val9.Y >= num5 && val9.Y + val9.Height <= num5 + num6)
			{
				IClickableMenu.drawToolTip(b, text7, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
				break;
			}
		}
	}

	private void DrawActionsPopup(SpriteBatch b)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		int num = 800;
		int num2 = 600;
		int num3 = (Game1.uiViewport.Width - num) / 2;
		int num4 = (Game1.uiViewport.Height - num2) / 2;
		Color val = (Color)(useDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), num3, num4, num, num2, val, 1f, true, -1f);
		Rectangle val2 = default(Rectangle);
		val2._002Ector(num3 + num - 48, num4 + 8, 36, 36);
		b.Draw(Game1.mouseCursors, val2, (Rectangle?)new Rectangle(337, 494, 12, 12), Color.White);
		actionsCloseBounds = val2;
		string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.actionsPopup.title"));
		Vector2 val3 = Game1.dialogueFont.MeasureString(text);
		Color val4 = (useDarkTheme ? new Color(220, 180, 120) : new Color(86, 22, 12));
		b.DrawString(Game1.dialogueFont, text, new Vector2((float)num3 + ((float)num - val3.X) / 2f, (float)(num4 + 15)), val4);
		List<string> eventActionsSummary = eventData.GetEventActionsSummary();
		int num5 = num4 + 65;
		int num6 = num2 - 75;
		actionsScrollArea = new Rectangle(num3 + 10, num5, num - 20, num6);
		Rectangle scissorRectangle = default(Rectangle);
		scissorRectangle._002Ector(actionsScrollArea.X, actionsScrollArea.Y, actionsScrollArea.Width, actionsScrollArea.Height);
		Rectangle scissorRectangle2 = ((GraphicsResource)b).GraphicsDevice.ScissorRectangle;
		RasterizerState rasterizerState = ((GraphicsResource)b).GraphicsDevice.RasterizerState;
		RasterizerState val5 = new RasterizerState
		{
			ScissorTestEnable = true
		};
		b.End();
		b.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, val5, (Effect)null, (Matrix?)null);
		((GraphicsResource)b).GraphicsDevice.ScissorRectangle = scissorRectangle;
		int num7 = num5 - actionsScrollOffset;
		int num8 = num7;
		int num9 = 28;
		int num10 = num3 + 40;
		Color val6 = (Color)(useDarkTheme ? new Color(200, 220, 255) : Color.DarkBlue);
		if (eventActionsSummary == null || eventActionsSummary.Count == 0)
		{
			string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.actionsPopup.noActions"));
			b.DrawString(Game1.smallFont, text2, new Vector2((float)num10, (float)num8), useDarkTheme ? Color.Gray : Color.DarkGray);
		}
		else
		{
			foreach (string item in eventActionsSummary)
			{
				b.DrawString(Game1.smallFont, item, new Vector2((float)num10, (float)num8), val6);
				num8 += num9;
			}
		}
		b.End();
		((GraphicsResource)b).GraphicsDevice.ScissorRectangle = scissorRectangle2;
		b.Begin((SpriteSortMode)0, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, rasterizerState, (Effect)null, (Matrix?)null);
		int num11 = ((eventActionsSummary != null && eventActionsSummary.Count > 0) ? (eventActionsSummary.Count * num9 + 20) : 50);
		maxActionsScroll = Math.Max(0, num11 - num6);
		if (num11 > num6)
		{
			int num12 = num3 + num - 28;
			int num13 = num5;
			int num14 = num6;
			int num15 = 24;
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(403, 383, 6, 6), num12, num13, num15, num14, Color.White, 1f, false, -1f);
			if (maxActionsScroll > 0)
			{
				float num16 = (float)actionsScrollOffset / (float)maxActionsScroll;
				int num17 = Math.Max(40, (int)((float)num14 / (float)num11 * (float)num14));
				int num18 = num13 + (int)((float)(num14 - num17) * num16);
				b.Draw(Game1.menuTexture, new Rectangle(num12 + 4, num18, num15 - 8, num17), (Rectangle?)new Rectangle(403, 383, 6, 6), Color.White);
			}
		}
	}

	private void DrawCPConditionsPopup(SpriteBatch b)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		List<Dictionary<string, string>> whenConditionsVariants = eventData.WhenConditionsVariants;
		if (whenConditionsVariants == null || whenConditionsVariants.Count == 0)
		{
			return;
		}
		int num = 28;
		int num2 = 20;
		int num3 = 50;
		int num4 = 0;
		foreach (Dictionary<string, string> item in whenConditionsVariants)
		{
			num4 += item.Count;
			if (whenConditionsVariants.Count > 1)
			{
				num4++;
			}
		}
		int num5 = 1000;
		int num6 = Math.Min(600, num3 + num2 * 2 + num4 * num + 40);
		int num7 = (Game1.uiViewport.Width - num5) / 2;
		int num8 = (Game1.uiViewport.Height - num6) / 2;
		cpConditionsPopupBounds = new Rectangle(num7, num8, num5, num6);
		Color val = (Color)(useDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), num7, num8, num5, num6, val, 1f, true, -1f);
		Rectangle val2 = default(Rectangle);
		val2._002Ector(num7 + num5 - 48, num8 + 8, 36, 36);
		b.Draw(Game1.mouseCursors, val2, (Rectangle?)new Rectangle(337, 494, 12, 12), Color.White);
		cpConditionsCloseBounds = val2;
		string text = "Content Patcher Conditions";
		Color val3 = (useDarkTheme ? new Color(220, 180, 120) : new Color(86, 22, 12));
		Vector2 val4 = Game1.dialogueFont.MeasureString(text);
		b.DrawString(Game1.dialogueFont, text, new Vector2((float)num7 + ((float)num5 - val4.X) / 2f, (float)(num8 + 12)), val3);
		int num9 = num8 + num3 + num2;
		Color val5 = (useDarkTheme ? Color.White : Color.Black);
		int num10 = 0;
		foreach (Dictionary<string, string> item2 in whenConditionsVariants)
		{
			num10++;
			if (whenConditionsVariants.Count > 1)
			{
				string text2 = $"— Variante {num10} —";
				Vector2 val6 = Game1.smallFont.MeasureString(text2);
				b.DrawString(Game1.smallFont, text2, new Vector2((float)num7 + ((float)num5 - val6.X) / 2f, (float)num9), useDarkTheme ? Color.LightGray : Color.DarkGray);
				num9 += num;
			}
			foreach (KeyValuePair<string, string> item3 in item2)
			{
				string text3 = item3.Key + ": " + item3.Value;
				int num11 = 100;
				if (text3.Length > num11)
				{
					text3 = text3.Substring(0, num11 - 3) + "...";
				}
				b.DrawString(Game1.smallFont, text3, new Vector2((float)(num7 + num2), (float)num9), val5);
				num9 += num;
				if (num9 > num8 + num6 - num2)
				{
					break;
				}
			}
			if (num9 > num8 + num6 - num2)
			{
				break;
			}
		}
	}

	private void DrawConditionDetailPopup(SpriteBatch b)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		bool useDarkTheme = MHEventsListMod.Config.UseDarkTheme;
		int num = 700;
		int num2 = 280;
		int num3 = (Game1.uiViewport.Width - num) / 2;
		int num4 = (Game1.uiViewport.Height - num2) / 2;
		Color val = (Color)(useDarkTheme ? new Color(40, 40, 50) : Color.White);
		IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), num3, num4, num, num2, val, 1f, true, -1f);
		Rectangle val2 = default(Rectangle);
		val2._002Ector(num3 + num - 48, num4 + 8, 36, 36);
		b.Draw(Game1.mouseCursors, val2, (Rectangle?)new Rectangle(337, 494, 12, 12), Color.White);
		conditionDetailCloseBounds = val2;
		string text = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.conditionDetail.title"));
		Vector2 val3 = Game1.dialogueFont.MeasureString(text);
		Color val4 = (useDarkTheme ? new Color(220, 180, 120) : new Color(86, 22, 12));
		b.DrawString(Game1.dialogueFont, text, new Vector2((float)num3 + ((float)num - val3.X) / 2f, (float)(num4 + 15)), val4);
		int num5 = num3 + 40;
		int num6 = num4 + 70;
		int num7 = 40;
		int num8 = 150;
		Color val5 = (useDarkTheme ? new Color(200, 150, 100) : new Color(86, 22, 12));
		Color val6 = (useDarkTheme ? Color.White : Color.Black);
		string text2 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableOk"));
		b.DrawString(Game1.smallFont, text2, new Vector2((float)num5, (float)num6), val5);
		DrawCheckIcon(b, num5 + num8, num6 - 2, selectedConditionMet);
		num6 += num7;
		string text3 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableCode"));
		b.DrawString(Game1.smallFont, text3, new Vector2((float)num5, (float)num6), val5);
		string text4 = selectedConditionCode ?? "";
		int maxWidth = num - num8 - 80;
		List<string> list = WrapText(text4, maxWidth);
		foreach (string item in list)
		{
			b.DrawString(Game1.smallFont, item, new Vector2((float)(num5 + num8), (float)num6), val6);
			num6 += 24;
		}
		if (list.Count == 0)
		{
			num6 += 24;
		}
		num6 += 8;
		string text5 = Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tableDescription"));
		b.DrawString(Game1.smallFont, text5, new Vector2((float)num5, (float)num6), val5);
		string text6 = selectedConditionTranslation ?? "";
		int maxWidth2 = num - num8 - 80;
		List<string> list2 = WrapText(text6, maxWidth2);
		foreach (string item2 in list2)
		{
			b.DrawString(Game1.smallFont, item2, new Vector2((float)(num5 + num8), (float)num6), val6);
			num6 += 24;
		}
	}

	private List<string> WrapText(string text, int maxWidth)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		if (string.IsNullOrEmpty(text))
		{
			return list;
		}
		string[] array = text.Split(' ');
		string text2 = "";
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			string text4 = (string.IsNullOrEmpty(text2) ? text3 : (text2 + " " + text3));
			if (Game1.smallFont.MeasureString(text4).X > (float)maxWidth && !string.IsNullOrEmpty(text2))
			{
				list.Add(text2);
				text2 = text3;
			}
			else
			{
				text2 = text4;
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			list.Add(text2);
		}
		return list;
	}

	private (bool canEvaluate, bool isMet) EvaluateWhenCondition(string key, string expectedValue)
	{
		try
		{
			ContentPatcherIntegration contentPatcher = MHEventsListMod.ContentPatcher;
			if (contentPatcher != null && contentPatcher.IsAvailable && contentPatcher.IsReady && !key.StartsWith("Query:", StringComparison.OrdinalIgnoreCase) && !key.StartsWith("Query ", StringComparison.OrdinalIgnoreCase))
			{
				string value = expectedValue;
				if (expectedValue.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					value = "true";
				}
				var (flag, item) = contentPatcher.EvaluateSingleCondition(key, value);
				if (flag)
				{
					return (canEvaluate: true, isMet: item);
				}
			}
			if (key.Contains("{{") && key.Contains("}}"))
			{
				return (canEvaluate: false, isMet: false);
			}
			string text = key;
			string text2 = "";
			bool flag2 = false;
			if (key.Contains("|contains="))
			{
				int num = key.IndexOf("|contains=");
				text = key.Substring(0, num).Trim();
				text2 = key.Substring(num + 10).Trim();
				flag2 = true;
			}
			else if (key.Contains("|contains"))
			{
				text = key[..key.IndexOf("|contains")].Trim();
				flag2 = true;
				text2 = expectedValue;
			}
			else if (key.Contains(":"))
			{
				int num2 = key.IndexOf(':');
				string text3 = key.Substring(0, num2).Trim();
				if (text3 == "Query")
				{
					string expression = key.Substring(num2 + 1).Trim();
					return EvaluateQueryExpression(expression, expectedValue);
				}
				if (text3 == "Hearts")
				{
					string npcName = key.Substring(num2 + 1).Trim();
					return EvaluateHeartsRange(npcName, expectedValue);
				}
			}
			bool flag3 = expectedValue.Equals("true", StringComparison.OrdinalIgnoreCase) || expectedValue == "true";
			bool flag4 = false;
			switch (text)
			{
			case "HasSeenEvent":
				flag4 = ((!flag2 || string.IsNullOrEmpty(text2)) ? ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(expectedValue) : ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(text2));
				break;
			case "HasFlag":
			case "HasReadLetter":
				flag4 = ((!flag2 || string.IsNullOrEmpty(text2)) ? ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(expectedValue) : ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(text2));
				break;
			case "HasActiveQuest":
			{
				string text4 = (flag2 ? text2 : expectedValue);
				flag4 = Game1.player.hasQuest(text4);
				break;
			}
			case "PlayerGender":
			{
				string value2 = (Game1.player.IsMale ? "Male" : "Female");
				flag4 = expectedValue.Equals(value2, StringComparison.OrdinalIgnoreCase);
				break;
			}
			case "Season":
				flag4 = Game1.currentSeason.Equals(expectedValue, StringComparison.OrdinalIgnoreCase);
				break;
			case "Weather":
			{
				string value3 = (Game1.isRaining ? "rainy" : (Game1.isSnowing ? "snowy" : "sunny"));
				flag4 = expectedValue.Equals(value3, StringComparison.OrdinalIgnoreCase) || (expectedValue.Equals("Sun", StringComparison.OrdinalIgnoreCase) && !Game1.isRaining && !Game1.isSnowing);
				break;
			}
			case "DayOfWeek":
			{
				string value4 = Game1.Date.DayOfWeek.ToString();
				if (flag2)
				{
					string text6 = ((!string.IsNullOrEmpty(text2)) ? text2 : expectedValue);
					flag4 = text6.Contains(value4, StringComparison.OrdinalIgnoreCase);
				}
				else
				{
					flag4 = expectedValue.Equals(value4, StringComparison.OrdinalIgnoreCase);
				}
				break;
			}
			case "Query":
			{
				string text5 = key;
				if (text5.StartsWith("Query:", StringComparison.OrdinalIgnoreCase))
				{
					text5 = text5.Substring(6).Trim();
				}
				else if (text5.StartsWith("Query ", StringComparison.OrdinalIgnoreCase))
				{
					text5 = text5.Substring(6).Trim();
				}
				return EvaluateQueryExpression(text5, expectedValue);
			}
			default:
				if (flag3)
				{
					flag4 = ((NetHashSet<string>)(object)Game1.player.mailReceived).Contains(text);
					break;
				}
				return (canEvaluate: false, isMet: false);
			}
			bool item2 = (flag3 ? flag4 : (!flag4));
			return (canEvaluate: true, isMet: item2);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private bool CheckWhenCondition(string key, string value)
	{
		var (flag, flag2) = EvaluateWhenCondition(key, value);
		return flag && flag2;
	}

	private (bool canEvaluate, bool isMet) EvaluateHeartsRange(string npcName, string rangeValue)
	{
		try
		{
			NPC characterFromName = Game1.getCharacterFromName(npcName, true, false);
			if (characterFromName == null)
			{
				return (canEvaluate: false, isMet: false);
			}
			int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(((Character)characterFromName).Name);
			List<int> list = new List<int>();
			string[] array = rangeValue.Split(',');
			foreach (string text in array)
			{
				string s = text.Trim();
				if (int.TryParse(s, out var result))
				{
					list.Add(result);
				}
			}
			if (list.Count > 0)
			{
				return (canEvaluate: true, isMet: list.Contains(friendshipHeartLevelForNPC));
			}
			return (canEvaluate: false, isMet: false);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateQueryExpression(string expression, string expectedValue)
	{
		try
		{
			bool flag = expectedValue.Equals("true", StringComparison.OrdinalIgnoreCase);
			if (expression.Contains(" AND ", StringComparison.OrdinalIgnoreCase))
			{
				string[] array = expression.Split(new string[1] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string text in array2)
				{
					var (flag2, flag3) = EvaluateSingleQueryExpression(text.Trim());
					if (!flag2)
					{
						return (canEvaluate: false, isMet: false);
					}
					if (!flag3)
					{
						return (canEvaluate: true, isMet: !flag);
					}
				}
				return (canEvaluate: true, isMet: flag);
			}
			if (expression.Contains(" OR ", StringComparison.OrdinalIgnoreCase))
			{
				string[] array3 = expression.Split(new string[1] { " OR " }, StringSplitOptions.RemoveEmptyEntries);
				string[] array4 = array3;
				foreach (string text2 in array4)
				{
					var (flag4, flag5) = EvaluateSingleQueryExpression(text2.Trim());
					if (flag4 && flag5)
					{
						return (canEvaluate: true, isMet: flag);
					}
				}
				return (canEvaluate: true, isMet: !flag);
			}
			(bool, bool) tuple3 = EvaluateSingleQueryExpression(expression);
			if (!tuple3.Item1)
			{
				return (canEvaluate: false, isMet: false);
			}
			return (canEvaluate: true, isMet: flag ? tuple3.Item2 : (!tuple3.Item2));
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateSingleQueryExpression(string expression)
	{
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			expression = expression.Trim();
			if (expression.Contains("{{Hearts:"))
			{
				int num = expression.IndexOf("{{Hearts:") + 9;
				int num2 = expression.IndexOf("}}", num);
				if (num2 > num)
				{
					string text = expression.Substring(num, num2 - num);
					string comparisonExpr = expression.Substring(num2 + 2).Trim();
					int friendshipHeartLevelForNPC = Game1.player.getFriendshipHeartLevelForNPC(text);
					return EvaluateComparison(friendshipHeartLevelForNPC, comparisonExpr);
				}
			}
			if (expression.Contains("{{Time}}"))
			{
				int timeOfDay = Game1.timeOfDay;
				string comparisonExpr2 = expression.Replace("{{Time}}", "").Trim();
				return EvaluateComparison(timeOfDay, comparisonExpr2);
			}
			if (expression.Contains("{{DayOfWeek}}") && expression.Contains(" IN "))
			{
				string currentDay = Game1.Date.DayOfWeek.ToString();
				int num3 = expression.IndexOf(" IN ", StringComparison.OrdinalIgnoreCase);
				if (num3 > 0)
				{
					string text2 = expression.Substring(num3 + 4).Trim().Trim('(', ')', ' ');
					List<string> source = (from d in text2.Split(',')
						select d.Trim().Trim('\'', '"', ' ')).ToList();
					bool item = source.Any((string d) => d.Equals(currentDay, StringComparison.OrdinalIgnoreCase));
					return (canEvaluate: true, isMet: item);
				}
			}
			if (expression.Contains("{{Season}}"))
			{
				string currentSeason = Game1.currentSeason;
				if (expression.Contains("=="))
				{
					string value = expression.Split(new string[1] { "==" }, StringSplitOptions.None)[1].Trim().Trim('"', '\'', ' ');
					return (canEvaluate: true, isMet: currentSeason.Equals(value, StringComparison.OrdinalIgnoreCase));
				}
			}
			if (expression.Contains("{{DaysPlayed}}"))
			{
				int daysPlayed = (int)Game1.stats.DaysPlayed;
				string comparisonExpr3 = expression.Replace("{{DaysPlayed}}", "").Trim();
				return EvaluateComparison(daysPlayed, comparisonExpr3);
			}
			if (expression.Contains("{{Year}}"))
			{
				int year = Game1.Date.Year;
				string comparisonExpr4 = expression.Replace("{{Year}}", "").Trim();
				return EvaluateComparison(year, comparisonExpr4);
			}
			if (expression.Contains("{{SpouseGender}}"))
			{
				string text3 = "undefined";
				if (Game1.player.isMarriedOrRoommates())
				{
					NPC spouse = Game1.player.getSpouse();
					if (spouse != null)
					{
						text3 = (((int)((Character)spouse).Gender == 0) ? "male" : "female");
					}
				}
				if (expression.Contains("="))
				{
					string[] array = expression.Split('=');
					if (array.Length >= 2)
					{
						string value2 = array[^1].Trim().Trim('\'', '"', ' ');
						return (canEvaluate: true, isMet: text3.Equals(value2, StringComparison.OrdinalIgnoreCase));
					}
				}
			}
			expression.Contains("{{Random");
			return (canEvaluate: false, isMet: false);
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private (bool canEvaluate, bool isMet) EvaluateComparison(int actualValue, string comparisonExpr)
	{
		try
		{
			comparisonExpr = comparisonExpr.Trim();
			string text = "";
			string text2 = "";
			if (comparisonExpr.StartsWith(">="))
			{
				text = ">=";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else if (comparisonExpr.StartsWith("<="))
			{
				text = "<=";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else if (comparisonExpr.StartsWith(">"))
			{
				text = ">";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			else if (comparisonExpr.StartsWith("<"))
			{
				text = "<";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			else if (comparisonExpr.StartsWith("=="))
			{
				text = "==";
				text2 = comparisonExpr.Substring(2).Trim();
			}
			else
			{
				if (!comparisonExpr.StartsWith("="))
				{
					return (canEvaluate: false, isMet: false);
				}
				text = "=";
				text2 = comparisonExpr.Substring(1).Trim();
			}
			if (!int.TryParse(text2, out var result))
			{
				return (canEvaluate: false, isMet: false);
			}
			return (canEvaluate: true, isMet: text switch
			{
				">=" => actualValue >= result, 
				"<=" => actualValue <= result, 
				">" => actualValue > result, 
				"<" => actualValue < result, 
				"==" => actualValue == result, 
				"=" => actualValue == result, 
				_ => false, 
			});
		}
		catch
		{
			return (canEvaluate: false, isMet: false);
		}
	}

	private bool CheckCondition(string condition)
	{
		try
		{
			if (string.IsNullOrEmpty(condition))
			{
				return true;
			}
			if (condition.StartsWith("x "))
			{
				return true;
			}
			if (conditionCheckCache != null && conditionCheckCache.TryGetValue(condition, out var value))
			{
				return value;
			}
			if (condition.Contains("{{") && condition.Contains("}}"))
			{
				conditionCheckCache?.TryAdd(condition, value: false);
				return false;
			}
			GameLocation locationFromName = Game1.getLocationFromName(eventData.LocationName);
			if (locationFromName == null)
			{
				conditionCheckCache?.TryAdd(condition, value: false);
				return false;
			}
			string text = locationFromName.checkEventPrecondition("-8888888/" + condition, false);
			bool flag = text != "-1";
			conditionCheckCache?.TryAdd(condition, flag);
			return flag;
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error checking condition '" + condition + "': " + ex.Message, (LogLevel)1);
			conditionCheckCache?.TryAdd(condition, value: false);
			return false;
		}
	}

	private void DrawButtons(SpriteBatch b)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		bool flag = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id);
		toggleSeenButton.sourceRect = (flag ? new Rectangle(192, 256, 64, 64) : new Rectangle(128, 256, 64, 64));
		bool flag2 = ((ClickableComponent)toggleSeenButton).containsPoint(mouseX, mouseY);
		toggleSeenButton.draw(b, Color.White * (flag2 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		if (MHEventsListMod.Config.ShowPlayEventButton && playEventButton != null)
		{
			bool flag3 = ((ClickableComponent)playEventButton).containsPoint(mouseX, mouseY);
			playEventButton.draw(b, Color.Gold * (flag3 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		}
		if (MHEventsListMod.Config.ShowGoToLocationButton && goToButton != null)
		{
			bool flag4 = ((ClickableComponent)goToButton).containsPoint(mouseX, mouseY);
			goToButton.draw(b, Color.White * (flag4 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		}
		if (MHEventsListMod.Config.ShowDebugButton && debugButton != null)
		{
			bool flag5 = ((ClickableComponent)debugButton).containsPoint(mouseX, mouseY);
			debugButton.draw(b, Color.White * (flag5 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		}
		if (MHEventsListMod.Config.ShowEventActionsButton && actionsButton != null)
		{
			bool flag6 = ((ClickableComponent)actionsButton).containsPoint(mouseX, mouseY);
			actionsButton.draw(b, Color.LightGreen * (flag6 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		}
		Color val = (EventOverlay.IsEventPinned(eventData.Id) ? Color.Yellow : Color.White);
		bool flag7 = ((ClickableComponent)pinButton).containsPoint(mouseX, mouseY);
		pinButton.draw(b, val * (flag7 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		bool flag8 = IsEventHidden();
		hideButton.sourceRect = (flag8 ? new Rectangle(310, 392, 16, 16) : new Rectangle(322, 498, 12, 12));
		((ClickableComponent)hideButton).scale = (flag8 ? 2.5f : 3.5f);
		Color val2 = (flag8 ? Color.LightGreen : Color.White);
		bool flag9 = ((ClickableComponent)hideButton).containsPoint(mouseX, mouseY);
		hideButton.draw(b, val2 * (flag9 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		bool flag10 = ((ClickableComponent)legendButton).containsPoint(mouseX, mouseY);
		legendButton.draw(b, Color.White * (flag10 ? 0.7f : 1f), 0.88f, 0, 0, 0);
		bool flag11 = ((ClickableComponent)openJsonButton).containsPoint(mouseX, mouseY);
		openJsonButton.draw(b, Color.White * (flag11 ? 0.7f : 1f), 0.88f, 0, 0, 0);
	}

	private void DrawTooltips(SpriteBatch b)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (showingLegend || showingActions || showingConditionDetail || showingCPConditionsPopup)
		{
			return;
		}
		int mouseX = Game1.getMouseX();
		int mouseY = Game1.getMouseY();
		foreach (Tuple<Rectangle, string> tooltipArea in tooltipAreas)
		{
			Rectangle item = tooltipArea.Item1;
			if (item.Contains(mouseX, mouseY))
			{
				IClickableMenu.drawToolTip(b, tooltipArea.Item2, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
				return;
			}
		}
		bool flag = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id);
		if (((ClickableComponent)legendButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.legend")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (((ClickableComponent)openJsonButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.openJson")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (((ClickableComponent)pinButton).containsPoint(mouseX, mouseY))
		{
			string text = Translation.op_Implicit(EventOverlay.IsEventPinned(eventData.Id) ? MHEventsListMod.I18n.Get("detail.tip.unpin") : MHEventsListMod.I18n.Get("detail.tip.pin"));
			IClickableMenu.drawToolTip(b, text, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (((ClickableComponent)hideButton).containsPoint(mouseX, mouseY))
		{
			string text2 = Translation.op_Implicit(IsEventHidden() ? MHEventsListMod.I18n.Get("detail.tip.show") : MHEventsListMod.I18n.Get("detail.tip.hide"));
			IClickableMenu.drawToolTip(b, text2, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (((ClickableComponent)toggleSeenButton).containsPoint(mouseX, mouseY))
		{
			string text3 = Translation.op_Implicit(flag ? MHEventsListMod.I18n.Get("detail.tip.markUnseen") : MHEventsListMod.I18n.Get("detail.tip.markSeen"));
			IClickableMenu.drawToolTip(b, text3, "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (MHEventsListMod.Config.ShowPlayEventButton && playEventButton != null && ((ClickableComponent)playEventButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.play")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (MHEventsListMod.Config.ShowGoToLocationButton && goToButton != null && ((ClickableComponent)goToButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.goTo")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (MHEventsListMod.Config.ShowDebugButton && debugButton != null && ((ClickableComponent)debugButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.debug")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
		else if (MHEventsListMod.Config.ShowEventActionsButton && actionsButton != null && ((ClickableComponent)actionsButton).containsPoint(mouseX, mouseY))
		{
			IClickableMenu.drawToolTip(b, Translation.op_Implicit(MHEventsListMod.I18n.Get("detail.tip.actions")), "", (Item)null, false, -1, 0, (string)null, -1, (CraftingRecipe)null, -1, (IList<Item>)null);
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if (showingConditionDetail)
		{
			if (conditionDetailCloseBounds.Contains(x, y))
			{
				Game1.playSound("bigDeSelect", (int?)null);
				showingConditionDetail = false;
			}
			return;
		}
		if (showingCPConditionsPopup)
		{
			if (cpConditionsCloseBounds.Contains(x, y) || !cpConditionsPopupBounds.Contains(x, y))
			{
				Game1.playSound("bigDeSelect", (int?)null);
				showingCPConditionsPopup = false;
			}
			return;
		}
		if (showingActions)
		{
			Game1.playSound("bigDeSelect", (int?)null);
			showingActions = false;
			return;
		}
		if (showingLegend)
		{
			Game1.playSound("bigDeSelect", (int?)null);
			showingLegend = false;
			return;
		}
		if (cpConditionsClickArea != Rectangle.Empty && cpConditionsClickArea.Contains(x, y) && eventData.HasWhenConditions)
		{
			Game1.playSound("bigSelect", (int?)null);
			showingCPConditionsPopup = true;
			return;
		}
		foreach (Tuple<Rectangle, string, string, bool> conditionRow in conditionRows)
		{
			Rectangle item = conditionRow.Item1;
			if (item.Contains(x, y))
			{
				Game1.playSound("bigSelect", (int?)null);
				selectedConditionCode = conditionRow.Item2;
				selectedConditionTranslation = conditionRow.Item3;
				selectedConditionMet = conditionRow.Item4;
				showingConditionDetail = true;
				return;
			}
		}
		if (base.upperRightCloseButton != null && ((ClickableComponent)base.upperRightCloseButton).containsPoint(x, y) && previousMenu != null)
		{
			Game1.playSound("bigDeSelect", (int?)null);
			Game1.activeClickableMenu = previousMenu;
			return;
		}
		((IClickableMenu)this).receiveLeftClick(x, y, playSound);
		if (MHEventsListMod.Config.ShowEventActionsButton && actionsButton != null && ((ClickableComponent)actionsButton).containsPoint(x, y))
		{
			Game1.playSound("bigSelect", (int?)null);
			showingActions = true;
			actionsScrollOffset = 0;
		}
		else if (((ClickableComponent)legendButton).containsPoint(x, y))
		{
			Game1.playSound("bigSelect", (int?)null);
			showingLegend = true;
		}
		else if (((ClickableComponent)openJsonButton).containsPoint(x, y))
		{
			Game1.playSound("bigSelect", (int?)null);
			OpenEventJson();
		}
		else if (((ClickableComponent)pinButton).containsPoint(x, y))
		{
			if (EventOverlay.IsEventPinned(eventData.Id))
			{
				EventOverlay.UnpinEvent(eventData.Id);
			}
			else if (!EventOverlay.PinEvent(eventData.Id))
			{
				if (string.IsNullOrEmpty(eventData.RawPreconditions) && eventData.HasWhenConditions)
				{
					Game1.showRedMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("overlay.onlyWhenConditions")), true);
				}
				else if (EventOverlay.PinnedCount >= MHEventsListMod.Config.MaxPinnedEvents)
				{
					Game1.showRedMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("overlay.maxReached")), true);
				}
				else
				{
					Game1.showRedMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("overlay.noConditions")), true);
				}
			}
			if (MHEventsListMod.Overlay != null)
			{
				MHEventsListMod.Overlay.RefreshPinnedEvents();
			}
		}
		else if (((ClickableComponent)hideButton).containsPoint(x, y))
		{
			Game1.playSound("trashcan", (int?)null);
			ToggleHideEvent();
		}
		else if (((ClickableComponent)toggleSeenButton).containsPoint(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			if (((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id))
			{
				((NetHashSet<string>)(object)Game1.player.eventsSeen).Remove(eventData.Id);
			}
			else
			{
				((NetHashSet<string>)(object)Game1.player.eventsSeen).Add(eventData.Id);
			}
		}
		else if (MHEventsListMod.Config.ShowPlayEventButton && playEventButton != null && ((ClickableComponent)playEventButton).containsPoint(x, y))
		{
			Game1.playSound("newArtifact", (int?)null);
			TryPlayEvent();
		}
		else if (MHEventsListMod.Config.ShowGoToLocationButton && goToButton != null && ((ClickableComponent)goToButton).containsPoint(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			WarpToLocation();
		}
		else if (MHEventsListMod.Config.ShowDebugButton && debugButton != null && ((ClickableComponent)debugButton).containsPoint(x, y))
		{
			Game1.playSound("drumkit6", (int?)null);
			LogDebugInfo();
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (showingConditionDetail)
		{
			showingConditionDetail = false;
		}
		else if (showingCPConditionsPopup)
		{
			showingCPConditionsPopup = false;
		}
		else if (showingActions)
		{
			showingActions = false;
		}
		else if (showingLegend)
		{
			showingLegend = false;
		}
		else if ((int)key == 27 && previousMenu != null)
		{
			Game1.playSound("bigDeSelect", (int?)null);
			Game1.activeClickableMenu = previousMenu;
		}
		else
		{
			((IClickableMenu)this).receiveKeyPress(key);
		}
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (showingActions)
		{
			int num = ((direction > 0) ? (-50) : 50);
			actionsScrollOffset = Math.Max(0, Math.Min(maxActionsScroll, actionsScrollOffset + num));
		}
		else if (showingLegend)
		{
			int height = legendScrollArea.Height;
			int num2 = 1200;
			int num3 = Math.Max(0, num2 - height);
			if (direction > 0 && legendScrollOffset > 0)
			{
				legendScrollOffset = Math.Max(0, legendScrollOffset - 40);
			}
			else if (direction < 0 && legendScrollOffset < num3)
			{
				legendScrollOffset = Math.Min(num3, legendScrollOffset + 40);
			}
		}
		else
		{
			((IClickableMenu)this).receiveScrollWheelAction(direction);
			if (direction > 0 && conditionsScrollOffset > 0)
			{
				conditionsScrollOffset -= 30;
			}
			else if (direction < 0 && conditionsScrollOffset < maxConditionsScroll)
			{
				conditionsScrollOffset += 30;
			}
			conditionsScrollOffset = Math.Clamp(conditionsScrollOffset, 0, Math.Max(0, maxConditionsScroll));
		}
	}

	private void TryPlayEvent()
	{
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Expected O, but got Unknown
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Expected O, but got Unknown
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Expected O, but got Unknown
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!MHEventsListMod.Config.MarkAsSeenWhenPlaying)
			{
				MHEventsListMod.EventsToUnmark.Add(eventData.Id);
			}
			if (((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id))
			{
				((NetHashSet<string>)(object)Game1.player.eventsSeen).Remove(eventData.Id);
			}
			string text = null;
			if (eventData.IsFromContentPatcher)
			{
				try
				{
					MHEventsListMod.Helper.GameContent.InvalidateCache("Data/Events/" + eventData.LocationName);
					MHEventsListMod.Monitor.Log("Invalidated cache for Data/Events/" + eventData.LocationName, (LogLevel)1);
				}
				catch (Exception ex)
				{
					MHEventsListMod.Monitor.Log("Cache invalidation failed: " + ex.Message, (LogLevel)0);
				}
			}
			GameLocation locationFromName = Game1.getLocationFromName(eventData.LocationName);
			string text2 = default(string);
			Dictionary<string, string> dictionary = default(Dictionary<string, string>);
			if (locationFromName != null && locationFromName.TryGetLocationEvents(ref text2, ref dictionary))
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					if (item.Key.StartsWith(eventData.Id + "/") || item.Key == eventData.Id)
					{
						text = item.Value;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = eventData.GetEventScript();
				if (!string.IsNullOrEmpty(text) && text.Contains("{{") && text.Contains("}}"))
				{
					if (!string.IsNullOrEmpty(eventData.ModFolderPath))
					{
						text = I18nResolver.ResolveI18nTokens(text, eventData.ModFolderPath);
					}
					if (text.Contains("{{") && text.Contains("}}"))
					{
						ContentPatcherIntegration contentPatcher = MHEventsListMod.ContentPatcher;
						if (contentPatcher != null && contentPatcher.IsReady)
						{
							text = contentPatcher.ResolveTokens(text, eventData.ModUniqueId);
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
				string text3 = eventData.LocationName;
				GameLocation val = Game1.getLocationFromName(text3);
				if (val == null)
				{
					MHEventsListMod.Monitor.Log("Location '" + text3 + "' not found, using current location", (LogLevel)1);
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
					//IL_005c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0066: Expected O, but got Unknown
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					//IL_002d: Expected O, but got Unknown
					try
					{
						GameLocation currentLocation3 = Game1.currentLocation;
						if (currentLocation3 != null)
						{
							currentLocation3.startEvent(new Event(finalScript, (string)null, eventData.Id, (Farmer)null));
						}
					}
					catch (Exception ex3)
					{
						MHEventsListMod.Monitor.Log("Event play error: " + ex3.Message, (LogLevel)4);
						Game1.addHUDMessage(new HUDMessage("Error: " + ex3.Message, 3));
					}
				}));
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventNotFound")), 3));
			}
		}
		catch (Exception ex2)
		{
			MHEventsListMod.Monitor.Log("Play event error: " + ex2.Message, (LogLevel)4);
			Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventError")), 3));
		}
	}

	private void WarpToLocation()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		try
		{
			GameLocation locationFromName = Game1.getLocationFromName(eventData.LocationName);
			if (locationFromName != null)
			{
				int num;
				int num2;
				if (((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps).Count > 0)
				{
					num = ((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps)[0].X;
					num2 = ((NetList<Warp, NetRef<Warp>>)(object)locationFromName.warps)[0].Y;
				}
				else
				{
					num = locationFromName.Map.DisplayWidth / 128;
					num2 = locationFromName.Map.DisplayHeight / 128;
				}
				((IClickableMenu)this).exitThisMenu(true);
				Game1.warpFarmer(eventData.LocationName, num, num2, false);
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.warpedTo", (object)new
				{
					location = eventData.GetTranslatedLocation()
				})), 2));
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.locationNotFound")), 3));
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Warp error: " + ex.Message, (LogLevel)4);
			Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.warpFailed")), 3));
		}
	}

	public static string SanitizeDisplayName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return name;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in name)
		{
			if (c != '·' && c != '•' && c != '·' && c != '・' && c != '･' && (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '\''))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString().Trim();
	}

	public static bool IsValidNpcForDisplay(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		if (name.Contains("{{") || name.Contains("}}") || name.Contains("{") || name.Contains("["))
		{
			return false;
		}
		if (name.Contains('·') || name.Contains('•') || name.Contains('·') || name.Contains('・') || name.Contains('･'))
		{
			return false;
		}
		if (name.Length < 2)
		{
			return false;
		}
		if (!name.Any(char.IsLetter))
		{
			return false;
		}
		if (name.All(char.IsDigit))
		{
			return false;
		}
		if (name.StartsWith("??"))
		{
			return false;
		}
		if (name.Any((char c) => !char.IsLetterOrDigit(c) && c != ' ' && c != '-' && c != '\''))
		{
			return false;
		}
		return true;
	}

	private void LogDebugInfo()
	{
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Expected O, but got Unknown
		ITranslationHelper i18n = MHEventsListMod.I18n;
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.separator")), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.eventId", (object)new
		{
			id = eventData.Id
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.location", (object)new
		{
			location = eventData.LocationName
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.seen", (object)new
		{
			seen = ((NetHashSet<string>)(object)Game1.player.eventsSeen).Contains(eventData.Id)
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.startTime", (object)new
		{
			time = eventData.StartTime
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.endTime", (object)new
		{
			time = eventData.EndTime
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.freeSlots", (object)new
		{
			count = eventData.RequiredFreeSlots
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.requiredItems", (object)new
		{
			count = (eventData.RequiredItems?.Count ?? 0)
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.npcsInLocation", (object)new
		{
			count = (eventData.RequiredNpcsPresent?.Count ?? 0)
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.npcsWithHearts", (object)new
		{
			count = (eventData.HeartRequirements?.Count ?? 0)
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.preconditions", (object)new
		{
			conditions = eventData.RawPreconditions
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.modifiedPreconditions", (object)new
		{
			conditions = eventData.GetModifiedPreconditions()
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.isFromCP", (object)new
		{
			value = eventData.IsFromContentPatcher
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.hasWhenConditions", (object)new
		{
			value = eventData.HasWhenConditions
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.modFolderPath", (object)new
		{
			path = eventData.ModFolderPath
		})), (LogLevel)2);
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.checkingConditions")), (LogLevel)2);
		eventData.WriteAllConditions();
		MHEventsListMod.Monitor.Log(Translation.op_Implicit(i18n.Get("debug.separator")), (LogLevel)2);
		Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(i18n.Get("msg.debugLogged")), 2));
	}

	private void ToggleHideEvent()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		try
		{
			List<string> collection = MHEventsListMod.Helper.Data.ReadSaveData<List<string>>("HiddenEvents") ?? new List<string>();
			HashSet<string> hashSet = new HashSet<string>(collection);
			if (hashSet.Contains(eventData.Id))
			{
				hashSet.Remove(eventData.Id);
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventShown")), 1));
			}
			else
			{
				hashSet.Add(eventData.Id);
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.eventHidden")), 1));
			}
			MHEventsListMod.Helper.Data.WriteSaveData<List<string>>("HiddenEvents", hashSet.ToList());
			if (previousMenu is MHEventsMenu mHEventsMenu)
			{
				mHEventsMenu.ReloadAndRefresh();
			}
			if (previousMenu != null)
			{
				Game1.activeClickableMenu = previousMenu;
			}
			else
			{
				((IClickableMenu)this).exitThisMenu(true);
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Toggle hide error: " + ex.Message, (LogLevel)4);
		}
	}

	private bool IsEventHidden()
	{
		try
		{
			List<string> list = MHEventsListMod.Helper.Data.ReadSaveData<List<string>>("HiddenEvents") ?? new List<string>();
			return list.Contains(eventData.Id);
		}
		catch
		{
			return false;
		}
	}

	private void OpenEventJson()
	{
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Expected O, but got Unknown
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Expected O, but got Unknown
		try
		{
			string path = Path.Combine(Path.GetDirectoryName(((ContentManager)Game1.content).RootDirectory), "Mods");
			if (!Directory.Exists(path))
			{
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.jsonNotFound")), 3));
				return;
			}
			string value = "\"" + eventData.Id + "/";
			string text = null;
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string path2 in array)
			{
				string[] files = Directory.GetFiles(path2, "*.json", SearchOption.AllDirectories);
				string[] array2 = files;
				foreach (string text2 in array2)
				{
					try
					{
						string text3 = text2.ToLower();
						if (!text3.Contains(Path.DirectorySeparatorChar + "i18n" + Path.DirectorySeparatorChar) && !text3.Contains(Path.AltDirectorySeparatorChar + "i18n" + Path.AltDirectorySeparatorChar) && !text3.EndsWith(Path.DirectorySeparatorChar + "i18n") && !Path.GetDirectoryName(text2).EndsWith("i18n", StringComparison.OrdinalIgnoreCase))
						{
							string text4 = File.ReadAllText(text2);
							if (text4.Contains(value))
							{
								Process.Start(new ProcessStartInfo
								{
									FileName = text2,
									UseShellExecute = true
								});
								string fileName = Path.GetFileName(path2);
								string fileName2 = Path.GetFileName(text2);
								Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.jsonOpened", (object)new
								{
									file = fileName + "/" + fileName2
								})), 1));
								return;
							}
							if (text == null && text4.Contains(eventData.Id) && (text3.Contains("data") || text3.Contains("event")))
							{
								text = text2;
							}
						}
					}
					catch
					{
					}
				}
			}
			if (text != null)
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = text,
					UseShellExecute = true
				});
				string fileName3 = Path.GetFileName(text);
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.jsonOpened", (object)new
				{
					file = fileName3
				})), 1));
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.jsonNotFound")), 3));
			}
		}
		catch (Exception ex)
		{
			MHEventsListMod.Monitor.Log("Error opening JSON: " + ex.Message, (LogLevel)4);
			Game1.addHUDMessage(new HUDMessage(Translation.op_Implicit(MHEventsListMod.I18n.Get("msg.jsonError")), 3));
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
