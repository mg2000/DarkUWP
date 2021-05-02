﻿using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DarkUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class GamePage : Page
	{
		private int mMapWidth = 0;
		private int mMapHeight = 0;

		private SpriteSheet mWizardEyeTile;
		private SpriteSheet mMapTiles;
		private SpriteSheet mCharacterTiles;
		private byte[] mMapLayer = null;
		private readonly object mapLock = new object();

		private int mXWide; // 시야 범위
		private int mYWide; // 시야 범위

		bool ClampToSourceRect = true;

		private LorePlayer mParty;
		private List<Lore> mPlayerList;
		private Lore mAssistPlayer = null;

		private PositionType mPosition;
		private int mFace = 0;
		private int mEncounter = 0;
		private int mMaxEnemy = 0;

		private bool mTriggeredDownEvent = false;
		private int mTalkX = 0;
		private int mTalkY = 0;

		private List<TextBlock> mPlayerNameList = new List<TextBlock>();
		private List<TextBlock> mPlayerHPList = new List<TextBlock>();
		private List<TextBlock> mPlayerSPList = new List<TextBlock>();
		private List<TextBlock> mPlayerConditionList = new List<TextBlock>();
		private List<TextBlock> mEnemyTextList = new List<TextBlock>();
		private List<Border> mEnemyBlockList = new List<Border>();

		private List<HealthTextBlock> mHealthTextList = new List<HealthTextBlock>();

		private List<TextBlock> mMenuList = new List<TextBlock>();
		private MenuMode mMenuMode = MenuMode.None;
		private int mMenuCount = 0;
		private int mMenuFocusID = 0;

		private SpinnerType mSpinnerType = SpinnerType.None;
		private Tuple<string, int>[] mSpinnerItems;
		private int mSpinnerID;

		private Lore mMagicPlayer = null;
		private Lore mMagicWhomPlayer = null;

		private int mOrderFromPlayerID = -1;

		private bool mWeaponShopEnd = false;
		private int mBuyWeaponID = -1;

		private Lore mCurePlayer = null;

		private int mTeleportationDirection = 0;

		private Lore mTrainPlayer;
		private static readonly List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		private List<Tuple<int, int>> mTrainSkillList = list;
		private readonly List<string> mChangableClassList = new List<string>();
		private readonly List<int> mChangableClassIDList = new List<int>();

		private readonly string[] mItems = new string[] { "화살", "소환 문서", "대형 횃불", "수정 구슬", "비행 부츠", "이동 구슬" };
		private readonly int[] mItemPrices = new int[] { 500, 4_000, 300, 500, 1_000, 5_000 };
		private int mBuyItemID;

		private readonly string[] mMedicines = new string[] { "체력 회복약", "마법 회복약", "해독의 약초", "의식의 약초", "부활의 약초" };
		private readonly int[] mMedicinePrices = new int[] { 2_000, 3_000, 1_000, 5_000, 10_000 };
		private int mBuyMedicineID;

		private int mUseItemID;
		private Lore mItemUsePlayer;
		private readonly List<int> mUsableItemIDList = new List<int>();

		private int mWeaponTypeID;

		private int mTrainTime;

		private SpecialEventType mSpecialEvent = SpecialEventType.None;

		private BattleEvent mBattleEvent = BattleEvent.None;

		private volatile AnimationType mAnimationEvent = AnimationType.None;
		private int mAnimationFrame = 0;

		private bool mCureBattle = false;

		private Random mRand = new Random();

		private List<EnemyData> mEnemyDataList = null;
		private List<BattleEnemyData> mEncounterEnemyList = new List<BattleEnemyData>();
		private int mBattlePlayerID = 0;
		private int mBattleFriendID = 0;
		private int mBattleCommandID = 0;
		private int mBattleToolID = 0;
		private int mEnemyFocusID = 0;
		private Queue<BattleCommand> mBattleCommandQueue = new Queue<BattleCommand>();
		private Queue<BattleEnemyData> mBatteEnemyQueue = new Queue<BattleEnemyData>();
		private BattleTurn mBattleTurn = BattleTurn.None;

		private bool mLoading = true;

		private Dictionary<EnterType, string> mEnterTypeMap = new Dictionary<EnterType, string>();
		private EnterType mTryEnterType = EnterType.None;

		private Lore mReserveMember = null;
		private int mMemberX = -1;
		private int mMemberY = -1;
		private byte mMemberLeftTile = 0;

		private int mTelescopePeriod = 0;
		private int mTelescopeXCount = 0;
		private int mTelescopeYCount = 0;

		// 동굴 질문 번호
		private int mQuestionID = 0;

		// 블랙 나이트 등장 위치
		private int xEnemyOffset = 0;
		private int yEnemyOffset = -1;

		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyDownEvent = null;
		private TypedEventHandler<CoreWindow, KeyEventArgs> gamePageKeyUpEvent = null;

		private const int DIALOG_MAX_LINES = 13;

		private readonly List<string> mCureResult = new List<string>();
		private readonly List<string> mRemainDialog = new List<string>();

		private WizardEye mWizardEye = new WizardEye();
		private volatile bool mWizardEyePosBlink = false;
		private DispatcherTimer mWizardEyeTimer = new DispatcherTimer();
		private int mWizardEyePosX;
		private int mWizardEyePosY;

		private int mExchangeCategory;
		private Lore mExchangePlayer;

		private Lore mUnequipPlayer;

		private int mPrevX;
		private int mPrevY;

		public GamePage()
		{
			var rootFrame = Window.Current.Content as Frame;

			this.InitializeComponent();

			mWizardEyeTimer.Interval = TimeSpan.FromMilliseconds(100);
			mWizardEyeTimer.Tick += (sender, e) =>
			{
				if (mWizardEyePosBlink)
					mWizardEyePosBlink = false;
				else
					mWizardEyePosBlink = true;
			};

			DialogText.Tag = 0;

			mPlayerNameList.Add(PlayerName0);
			mPlayerNameList.Add(PlayerName1);
			mPlayerNameList.Add(PlayerName2);
			mPlayerNameList.Add(PlayerName3);
			mPlayerNameList.Add(PlayerName4);
			mPlayerNameList.Add(PlayerName5);

			mPlayerHPList.Add(PlayerHP0);
			mPlayerHPList.Add(PlayerHP1);
			mPlayerHPList.Add(PlayerHP2);
			mPlayerHPList.Add(PlayerHP3);
			mPlayerHPList.Add(PlayerHP4);
			mPlayerHPList.Add(PlayerHP5);

			mPlayerSPList.Add(PlayerSP0);
			mPlayerSPList.Add(PlayerSP1);
			mPlayerSPList.Add(PlayerSP2);
			mPlayerSPList.Add(PlayerSP3);
			mPlayerSPList.Add(PlayerSP4);
			mPlayerSPList.Add(PlayerSP5);

			mPlayerConditionList.Add(PlayerCondition0);
			mPlayerConditionList.Add(PlayerCondition1);
			mPlayerConditionList.Add(PlayerCondition2);
			mPlayerConditionList.Add(PlayerCondition3);
			mPlayerConditionList.Add(PlayerCondition4);
			mPlayerConditionList.Add(PlayerCondition5);

			mMenuList.Add(GameMenuText0);
			mMenuList.Add(GameMenuText1);
			mMenuList.Add(GameMenuText2);
			mMenuList.Add(GameMenuText3);
			mMenuList.Add(GameMenuText4);
			mMenuList.Add(GameMenuText5);
			mMenuList.Add(GameMenuText6);
			mMenuList.Add(GameMenuText7);
			mMenuList.Add(GameMenuText8);
			mMenuList.Add(GameMenuText9);

			mEnemyBlockList.Add(EnemyBlock0);
			mEnemyBlockList.Add(EnemyBlock1);
			mEnemyBlockList.Add(EnemyBlock2);
			mEnemyBlockList.Add(EnemyBlock3);
			mEnemyBlockList.Add(EnemyBlock4);
			mEnemyBlockList.Add(EnemyBlock5);
			mEnemyBlockList.Add(EnemyBlock6);
			mEnemyBlockList.Add(EnemyBlock7);

			mEnemyTextList.Add(EnemyText0);
			mEnemyTextList.Add(EnemyText1);
			mEnemyTextList.Add(EnemyText2);
			mEnemyTextList.Add(EnemyText3);
			mEnemyTextList.Add(EnemyText4);
			mEnemyTextList.Add(EnemyText5);
			mEnemyTextList.Add(EnemyText6);
			mEnemyTextList.Add(EnemyText7);

			mEnterTypeMap[EnterType.CastleLore] = "로어 성";
			mEnterTypeMap[EnterType.CastleLastDitch] = "라스트디치 성";
			mEnterTypeMap[EnterType.Menace] = "메너스";
			mEnterTypeMap[EnterType.UnknownPyramid] = "알수없는 피라미드";
			mEnterTypeMap[EnterType.ProofOfInfortune] = "흉성의 증거";
			mEnterTypeMap[EnterType.ClueOfInfortune] = "흉성의 단서";
			mEnterTypeMap[EnterType.RoofOfLight] = "빛의 지붕";
			mEnterTypeMap[EnterType.TempleOfLight] = "빛의 사원";
			mEnterTypeMap[EnterType.SurvivalOfPerishment] = "필멸의 생존";
			mEnterTypeMap[EnterType.CaveOfBerial] = "베리알의 동굴";
			mEnterTypeMap[EnterType.CaveOfMolok] = "몰록의 동굴";
			mEnterTypeMap[EnterType.TeleportationGate1] = "공간 이동 게이트";
			mEnterTypeMap[EnterType.TeleportationGate2] = "공간 이동 게이트";
			mEnterTypeMap[EnterType.TeleportationGate3] = "공간 이동 게이트";
			mEnterTypeMap[EnterType.CaveOfAsmodeus1] = "아스모데우스의 동굴";
			mEnterTypeMap[EnterType.CaveOfAsmodeus2] = "아스모데우스의 동굴";
			mEnterTypeMap[EnterType.FortressOfMephistopheles] = "메피스토펠레스의 요새";
			mEnterTypeMap[EnterType.CabinOfRegulus] = "레굴루스의 오두막";

			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName1, HealthPoison1, HealthUnconscious1, HealthDead1));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName2, HealthPoison2, HealthUnconscious2, HealthDead2));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName3, HealthPoison3, HealthUnconscious3, HealthDead3));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName4, HealthPoison4, HealthUnconscious4, HealthDead4));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName5, HealthPoison5, HealthUnconscious5, HealthDead5));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName6, HealthPoison6, HealthUnconscious6, HealthDead6));

			gamePageKeyDownEvent = async (sender, args) =>
			{
				if (mLoading || mSpecialEvent > 0 || mAnimationEvent != AnimationType.None || ContinueText.Visibility == Visibility.Visible || mTriggeredDownEvent)
					return;

				if (mMenuMode == MenuMode.None && mSpinnerType == SpinnerType.None && (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.Right ||
				 args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight ||
				 args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadDPadLeft || args.VirtualKey == VirtualKey.GamepadDPadRight))
				{
					var x = mParty.XAxis;
					var y = mParty.YAxis;

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
					{
						y--;
						if (mPosition == PositionType.Town)
							mFace = 1;
						else
							mFace = 5;
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
					{
						y++;
						if (mPosition == PositionType.Town)
							mFace = 0;
						else
							mFace = 4;
					}
					else if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						x--;
						if (mPosition == PositionType.Town)
							mFace = 3;
						else
							mFace = 7;
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						x++;
						if (mPosition == PositionType.Town)
							mFace = 2;
						else
							mFace = 6;
					}

					if (mParty.Map == 26)
						mFace += 4;

					if (x > 3 && x < mMapWidth - 4 && y > 4 && y < mMapHeight - 5)
					{
						void EnterMap()
						{
							if (mParty.Map == 1)
							{
								if (x == 19 && y == 10)
									ShowEnterMenu(EnterType.CastleLore);
								else if (x == 75 && y == 56)
									ShowEnterMenu(EnterType.CastleLastDitch);
								else if (x == 16 && y == 88)
									ShowEnterMenu(EnterType.Menace);
								else if (x == 83 && y == 85)
									ShowEnterMenu(EnterType.UnknownPyramid);
							}
							else if (mParty.Map == 2)
							{
								if (x == 99 && y == 99)
									ShowExitMenu();
								else if (x == 15 && y == 15)
									ShowEnterMenu(EnterType.ProofOfInfortune);
								else if (x == 148 && y == 64)
									ShowEnterMenu(EnterType.ClueOfInfortune);
							}
							else if (mParty.Map == 3)
							{
								if (x == 65 && y == 77)
									ShowEnterMenu(EnterType.RoofOfLight);
								else if (x == 88 && y == 93)
									ShowEnterMenu(EnterType.TempleOfLight);
								else if (x == 32 && y == 48)
									ShowEnterMenu(EnterType.SurvivalOfPerishment);
								else if (x == 35 && y == 15)
									ShowEnterMenu(EnterType.CaveOfBerial);
								else if (x == 92 && y == 5)
									ShowEnterMenu(EnterType.CaveOfMolok);
							}
							else if (mParty.Map == 4)
							{
								if (x == 41 && y == 75)
									ShowEnterMenu(EnterType.TeleportationGate1);
								else if (x == 12 && y == 70)
									ShowEnterMenu(EnterType.TeleportationGate2);
								else if (x == 40 && y == 53)
									ShowEnterMenu(EnterType.TeleportationGate3);
								else if (x == 13 && y == 42)
									ShowEnterMenu(EnterType.CaveOfAsmodeus1);
								else if (x == 8 && y == 20)
									ShowEnterMenu(EnterType.CaveOfAsmodeus2);
								else if (x == 26 && y == 8)
									ShowEnterMenu(EnterType.FortressOfMephistopheles);
							}
							else if (mParty.Map == 7)
							{
								AppendText(new string[] {
									" 당신이 동굴 입구에 들어가려 할때 어떤 글을 보았다.",
									"",
									"",
									"",
									$"[color={RGB.White}]   여기는 한때 피라미드라고 불리우는 악마의 동굴이었지만 지금은 폐쇄되어 아무도 들어갈 수가 없습니다.[/color]"
								});
							}
							else if (mParty.Map == 11)
							{
								if ((x == 24 && y == 6) || (x == 25 && y == 6))
								{
									AppendText(" 당신이 입구에 들어가려 했지만  이미 입구는 함몰 되어 들어 갈 수가 없었다.");
									if (mPlayerList.Count > 1 && (mParty.Etc[30] & (1 << 7)) == 0)
									{
										mSpecialEvent = SpecialEventType.InvestigationCave;
										ContinueText.Visibility = Visibility.Visible;
									}
								}
							}
							else if (mParty.Map == 12)
							{
								if ((x == 24 && y == 27) || (x == 25 && y == 27))
								{
									AppendText(" 당신은 이 동굴에 들어 가려고 했지만 동굴의 입구는  어떠한 강한 힘에 의해 무너져 있었고" +
									" 일행들의 힘으로는 도저히 들어갈 방도가 없었다. 결국에 일행은 들어가기를 포기했다.");

									UpdateTileInfo(24, 24, 52);
									UpdateTileInfo(25, 24, 52);
								}
							}
						}

						if (mPosition == PositionType.Town)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 19)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;
							}
							else if (1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 18 || GetTileInfo(x, y) == 20 || GetTileInfo(x, y) == 21)
							{
								// Don't Move
							}
							else if (GetTileInfo(x, y) == 22)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 23)
							{
								ShowSign(x, y);
							}
							else if (GetTileInfo(x, y) == 24)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 25)
							{
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 26)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (27 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
						else if (mPosition == PositionType.Ground)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 20)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									mTriggeredDownEvent = true;
							}
							else if (1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 19)
							{
								// Don't Move
							}
							else if (GetTileInfo(x, y) == 22)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 23 || GetTileInfo(x, y) == 49)
							{
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (24 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
						}
						else if (mPosition == PositionType.Den)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 52)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								if (await InvokeSpecialEvent(oriX, oriY))
									 mTriggeredDownEvent = true;
							}
							else if ((1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 40) || GetTileInfo(x, y) == 51)
							{

							}
							else if (GetTileInfo(x, y) == 53)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 49)
							{
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 54)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (41 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
						else if (mPosition == PositionType.Keep)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 52)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);

								mTriggeredDownEvent = true;
							}
							else if ((1 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 39) || GetTileInfo(x, y) == 51)
							{

							}
							else if (GetTileInfo(x, y) == 53)
							{
								ShowSign(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 48)
							{
								if (EnterWater())
									MovePlayer(x, y);
								mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 49)
							{
								EnterSwamp();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								EnterLava();
								MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 54)
							{
								EnterMap();
								mTriggeredDownEvent = true;
							}
							else if (40 <= GetTileInfo(x, y) && GetTileInfo(x, y) <= 47)
							{
								// Move Move
								MovePlayer(x, y);
							}
							else
							{
								TalkMode(x, y);
								mTriggeredDownEvent = true;
							}
						}
					}
				}
			};

			gamePageKeyUpEvent = async (sender, args) =>
			{
				var swordEnableClass = new int[,] {
						{  50,  50,  50,  50,  50,  50 },
						{  60,  60,  50,   0,  60,  50 },
						{ 100,   0,   0,   0,  30,  30 },
						{   0,  50,  50, 100,   0,  60 },
						{   0,   0,   0,   0,   0, 100 },
						{  80,   0,  60,  80,   0,  70 },
						{  60,  50,  50,  30,  70,  50 }
					};

				var magicEnableClass = new int[,] {
						{  50,  30,  30,   0,  50,   0 },
						{  20,  50,  30,  10,  50,  30 },
						{  20,  20,  50,  10,  50,  50 },
						{ 100,  60,  60,  50, 100,   0 },
						{  60,  70, 100, 100, 100, 100 },
						{  60, 100,  70, 100, 100,  50 },
						{  70, 100,  70,  50, 100, 100 }
					};

				var weaponPrice = new int[,] {
					{ 500, 3_000, 5_000,  7_000, 12_000, 40_000,  70_000 },
					{ 500, 3_000, 5_000, 10_000, 30_000, 60_000, 100_000 },
					{ 100, 1_000, 1_500,  4_000,  8_000, 35_000,  50_000 },
					{ 200,   300,   800,  2_000,  5_000, 10_000,  30_000 }
				};

				var shieldPrice = new int[] { 3_000, 15_000, 45_000, 80_000, 150_000 };
				var armorPrice = new int[] { 2_000, 5_000, 22_000, 45_000, 75_000, 100_000, 140_000, 200_000, 350_000, 500_000 };

				void ShowTrainSkillMenu(int defaultMenuID)
				{
					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

					var trainSkillMenuList = new List<string>();
					mTrainSkillList.Clear();

					if (swordEnableClass[mTrainPlayer.Class - 1, 0] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  베는 무기  기술치  :\t{mTrainPlayer.SwordSkill,5}[/color]", true);
						trainSkillMenuList.Add("  베는  무기  기술치");
						mTrainSkillList.Add(new Tuple<int, int>(0, swordEnableClass[mTrainPlayer.Class - 1, 0]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 1] > 0)
					{
						if (mTrainPlayer.Class != 7)
						{
							AppendText($"[color={RGB.LightCyan}]  찍는 무기  기술치  :\t{mTrainPlayer.AxeSkill}[/color]", true);
							trainSkillMenuList.Add("  찍는  무기  기술치");
						}
						else
						{
							AppendText($"[color={RGB.LightCyan}]  치료 마법  능력치  :\t{mTrainPlayer.AxeSkill}[/color]", true);
							trainSkillMenuList.Add("  치료  마법  능력치");
						}

						mTrainSkillList.Add(new Tuple<int, int>(1, swordEnableClass[mTrainPlayer.Class - 1, 1]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 2] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  찌르는 무기 기술치 :\t{mTrainPlayer.SpearSkill}[/color]", true);
						trainSkillMenuList.Add("  찌르는 무기 기술치");
						mTrainSkillList.Add(new Tuple<int, int>(2, swordEnableClass[mTrainPlayer.Class - 1, 2]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 3] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  쏘는 무기  기술치  :\t{mTrainPlayer.BowSkill}[/color]", true);
						trainSkillMenuList.Add("  쏘는  무기  기술치");
						mTrainSkillList.Add(new Tuple<int, int>(3, swordEnableClass[mTrainPlayer.Class - 1, 3]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 4] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  방패 사용  기술치  :\t{mTrainPlayer.ShieldSkill}[/color]", true);
						trainSkillMenuList.Add("  방패  사용  능력치");
						mTrainSkillList.Add(new Tuple<int, int>(4, swordEnableClass[mTrainPlayer.Class - 1, 4]));
					}

					if (swordEnableClass[mTrainPlayer.Class - 1, 5] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  맨손 사용  기술치  :\t{mTrainPlayer.FistSkill}[/color]", true);
						trainSkillMenuList.Add("  맨손  사용  기술치");
						mTrainSkillList.Add(new Tuple<int, int>(5, swordEnableClass[mTrainPlayer.Class - 1, 5]));
					}

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 수련 하고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainSkill, trainSkillMenuList.ToArray(), defaultMenuID);
				}

				void ShowTrainMagicMenu(int defaultMenuID)
				{
					AppendText($"[color={RGB.White}]{mTrainPlayer.Name}의 현재 능력치[/color]");

					var trainSkillMenuList = new List<string>();
					mTrainSkillList.Clear();

					if (magicEnableClass[mTrainPlayer.Class - 1, 0] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  공격 마법 능력치 :\t{mTrainPlayer.AttackMagic, 5}[/color]", true);
						trainSkillMenuList.Add("  공격 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(0, magicEnableClass[mTrainPlayer.Class - 1, 0]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 1] > 0) {
						AppendText($"[color={RGB.LightCyan}]  변화 마법 능력치 :\t{mTrainPlayer.PhenoMagic}[/color]", true);
						trainSkillMenuList.Add("  변화 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(1, magicEnableClass[mTrainPlayer.Class - 1, 1]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 2] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  치료 마법 능력치 :\t{mTrainPlayer.CureMagic}[/color]", true);
						trainSkillMenuList.Add("  치료 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(2, magicEnableClass[mTrainPlayer.Class - 1, 2]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 3] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  특수 마법 능력치 :\t{mTrainPlayer.SpecialMagic}[/color]", true);
						trainSkillMenuList.Add("  특수 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(3, magicEnableClass[mTrainPlayer.Class - 1, 3]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 4] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  초 자연력 능력치 :\t{mTrainPlayer.ESPMagic}[/color]", true);
						trainSkillMenuList.Add("  초 자연력 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(4, magicEnableClass[mTrainPlayer.Class - 1, 4]));
					}

					if (magicEnableClass[mTrainPlayer.Class - 1, 5] > 0)
					{
						AppendText($"[color={RGB.LightCyan}]  소환 마법 능력치 :\t{mTrainPlayer.SummonMagic}[/color]", true);
						trainSkillMenuList.Add("  소환 마법 능력치");
						mTrainSkillList.Add(new Tuple<int, int>(5, magicEnableClass[mTrainPlayer.Class - 1, 5]));
					}

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience.ToString("#,#0")}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 배우고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainMagic, trainSkillMenuList.ToArray(), defaultMenuID);
				}

				void ShowChooseTrainSkillMemberMenu()
				{
					AppendText($"[color={RGB.White}]누가 훈련을 받겠습니까?[/color]");
					ShowCharacterMenu(MenuMode.ChooseTrainSkillMember);
				}

				void ShowChooseTrainMagicMemberMenu()
				{
					AppendText($"[color={RGB.White}]누가 가르침을 받겠습니까?[/color]");
					ShowCharacterMenu(MenuMode.ChooseTrainMagicMember);
				}

				bool EnoughMoneyToChangeJob() {
					if (mParty.Gold < 10000)
					{
						Talk(" 그러나 일행에게는  직업을 바꿀때 드는 비용인 금 10000 개가 없습니다.");
						return false;
					}
					else
						return true;
				}

				void ShowChooseChangeSwordMemberMenu()
				{
					if (EnoughMoneyToChangeJob())
					{
						AppendText($"[color={RGB.White}]누가 전투사 계열의 직업을 바꾸겠습니까?[/color]");
						ShowCharacterMenu(MenuMode.ChooseChangeSwordMember);
					}
				}

				void ShowChooseChangeMagicMemberMenu()
				{
					if (EnoughMoneyToChangeJob())
					{

						AppendText($"[color={RGB.White}]누가 마법사 계열의 직업을 바꾸겠습니까?[/color]");
						ShowCharacterMenu(MenuMode.ChooseChangeMagicMember);
					}
				}

				bool IsUsableWeapon(Lore player, int weapon) {
					if (player.ClassType == ClassCategory.Magic)
						return false;
					else {
						if ((player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 6 || player.Class == 7) && 1 <= weapon && weapon <= 7)
							return true;
						else if ((player.Class == 1 || player.Class == 2 || player.Class == 4) && 8 <= weapon && weapon <= 14)
							return true;
						else if ((player.Class == 1 || player.Class == 2 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 15 <= weapon && weapon <= 21)
							return true;
						else if ((player.Class == 1 || player.Class == 4 || player.Class == 6 || player.Class == 7) && 22 <= weapon && weapon <= 28)
							return true;
						else
							return false;
					}
				}

				bool IsUsableShield(Lore player)
				{
					if (player.ClassType == ClassCategory.Magic)
						return false;
					else
					{
						if (player.Class == 1 || player.Class == 2 || player.Class == 3 || player.Class == 7)
							return true;
						else
							return false;
					}
				}

				bool IsUsableArmor(Lore player, int armor)
				{
					if (player.ClassType == ClassCategory.Magic && armor == 1)
						return true;
					else if (player.ClassType == ClassCategory.Sword && ((1 <= armor && armor <= 10) || armor == 255))
						return true;
					else
						return false;
				}

				void UpdateItem(Lore player)
				{
					var weaponData = new int[,,] {
						{
							{ 15, 15, 15, 15, 15, 25, 15 },
							{ 30, 30, 25, 25, 25, 25, 30 },
							{ 35, 40, 35, 35, 35, 35, 40 },
							{ 45, 48, 50, 40, 40, 40, 40 },
							{ 50, 55, 60, 50, 50, 50, 55 },
							{ 60, 70, 70, 60, 60, 60, 65 },
							{ 70, 70, 80, 70, 70, 70, 70 }
						},
						{
							{ 15, 15, 15, 15, 15, 15, 15 },
							{ 35, 30, 30, 37, 30, 30, 30 },
							{ 35, 40, 35, 35, 35, 35, 35 },
							{ 52, 45, 45, 45, 45, 45, 45 },
							{ 60, 60, 55, 55, 55, 55, 55 },
							{ 75, 70, 70, 70, 70, 70, 70 },
							{ 80, 85, 80, 80, 80, 80, 80 }
						},
						{
							{ 10, 10, 10, 25, 10, 20, 10 },
							{ 35, 40, 35, 35, 35, 35, 40 },
							{ 35, 30, 30, 35, 30, 30, 30 },
							{ 40, 40, 40, 45, 40, 40, 40 },
							{ 60, 60, 60, 60, 60, 60, 60 },
							{ 80, 80, 80, 80, 80, 80, 80 },
							{ 90, 90, 90, 90, 90, 90, 90 }
						},
						{
							{ 10, 10, 10, 15, 10, 15, 10 },
							{ 10, 10, 10, 10, 10, 20, 10 },
							{ 20, 20, 20, 27, 20, 20, 20 },
							{ 35, 35, 35, 40, 35, 38, 35 },
							{ 45, 45, 45, 55, 45, 45, 45 },
							{ 55, 55, 55, 65, 55, 55, 55 },
							{ 70, 70, 70, 85, 70, 70, 70 }
						}
					};

					if (IsUsableWeapon(player, player.Weapon))
					{
						if (player.Weapon > 0)
						{
							int sort = (player.Weapon - 1) / 7;
							int order = player.Weapon % 7;
							player.WeaPower = weaponData[sort, order, player.Class - 1];
						}
						else
							player.WeaPower = 5;
					}

					if (IsUsableShield(player))
						player.ShiPower = player.Shield;
					else
						player.ShiPower = 0;

					if (IsUsableArmor(player, player.Armor))
					{
						player.ArmPower = player.Armor;
						if (player.Armor == 255)
							player.ArmPower = 20;
					}
					else
						player.ArmPower = 0;

					player.AC = player.PotentialAC + player.ArmPower;
				}

				void ShowHealType()
				{
					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까?[/color]" });

					ShowMenu(MenuMode.HealType, new string[]
					{
						"상처를 치료",
						"독을 제거",
						"의식의 회복",
						"부활"
					});
				}

				//				async Task ExitCastleLore()
				//				{
				//					mParty.XAxis = 19;
				//					mParty.YAxis = 11;
				//					mParty.Map = 1;

				//					await RefreshGame();
				//				}

				async Task EndBattle()
				{
					var battleEvent = mBattleEvent;
					mBattleEvent = BattleEvent.None;

					if (mBattleTurn == BattleTurn.Win)
					{
						mBattleCommandQueue.Clear();
						mBatteEnemyQueue.Clear();

						var endMessage = "";

						if (mParty.Etc[5] == 2)
							endMessage = "";
						else
						{
#if DEBUG
							var goldPlus = 10_000;
#else
							var goldPlus = 0;
							foreach (var enemy in mEncounterEnemyList)
							{
								var enemyInfo = mEnemyDataList[enemy.ENumber];
								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
								var plus = enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= enemyInfo.Level;
								plus *= point;
								goldPlus += plus;
							}
#endif

							mParty.Gold += goldPlus;

							endMessage = $"[color={RGB.White}]일행은 {goldPlus.ToString("#,#0")}개의 금을 얻었다.[/color]";

							AppendText(new string[] { endMessage, "" });
						}

						if (battleEvent == BattleEvent.MenaceMurder)
						{
							StartBattleEvent(BattleEvent.MenaceMurder);
							return;
						}
						else if (battleEvent == BattleEvent.GuardOfObsidianArmor)
						{
							AppendText(" 당신은 보물 파수꾼들을 물리쳤다.");
							mParty.Etc[44] |= 1 << 3;
						}
						else if (battleEvent == BattleEvent.Slaim)
							mParty.Etc[44] |= 1 << 4;
						else if (battleEvent == BattleEvent.CaveEntrance)
							mParty.Etc[44] |= 1 << 5;
						else if (battleEvent == BattleEvent.CaveOfBerialEntrance)
						{
							mParty.Map = 15;
							mParty.XAxis = 24;
							mParty.YAxis = 43;

							await RefreshGame();
						}
						else if (battleEvent == BattleEvent.CaveOfAsmodeusEntrance)
						{
							Lore slowestPlayer = null;
							foreach (var player in mPlayerList)
							{
								if (player.HP > 0)
								{
									if (slowestPlayer == null || player.Agility <= slowestPlayer.Agility)
										slowestPlayer = player;
								}
							}

							if (mAssistPlayer != null && mAssistPlayer.Agility <= slowestPlayer.Agility)
								slowestPlayer = mAssistPlayer;

							AppendText(new string[] {
								$"[color={RGB.LightMagenta}] 우욱... 하지만 나는 죽더라도 한사람은 지옥으로 보내 주겠다.[/color]",
								"",
								" 가디안 레프트는  죽기 직전에 일행의 뒤에서 거대한 마법 독화살을 쏘았다.",
								"",
								$"[color={RGB.LightRed}] 가디안 레프트의 마법 독화살은 일행 중 가장 민첩성이 낮은 {slowestPlayer.Name}에게 명중 했다.",
								$"[color={RGB.LightRed}] 그리고, {slowestPlayer.NameSubjectJosa} 즉사 했다."
							});

							slowestPlayer.HP = 0;
							slowestPlayer.Poison = 1;
							slowestPlayer.Unconscious = slowestPlayer.Endurance * slowestPlayer.Level * 10 - 2;
							if (slowestPlayer.Unconscious < 1)
								slowestPlayer.Unconscious = 1;
							slowestPlayer.Dead = 1;

							UpdatePlayersStat();

							mParty.Map = 17;
							mParty.XAxis = 24;
							mParty.YAxis = 43;

							mParty.Etc[42] = 0;
							mParty.Etc[40] |= 1 << 6;
						}

						mEncounterEnemyList.Clear();
						mBattleEvent = 0;

						ShowMap();

					}
					else if (mBattleTurn == BattleTurn.RunAway)
					{
						AppendText(new string[] { "" });

						mBattlePlayerID = 0;
						while (!mPlayerList[mBattlePlayerID].IsAvailable && mBattlePlayerID < mPlayerList.Count)
							mBattlePlayerID++;

						if (battleEvent == BattleEvent.MenaceMurder)
						{
							ShowMap();
							Talk(" 하지만 너무 많은 적들에게 포위되어 도망 갈수가 없었다.");

							mBattleEvent = BattleEvent.MenaceMurder;
							mSpecialEvent = SpecialEventType.BackToBattleMode;
							return;
						}
						else if (battleEvent == BattleEvent.GuardOfObsidianArmor)
							mParty.YAxis++;
						else if (battleEvent == BattleEvent.Slaim)
							mParty.YAxis--;
						else if (battleEvent == BattleEvent.CaveEntrance)
							mParty.YAxis++;

						mEncounterEnemyList.Clear();
						ShowMap();
					}
					else if (mBattleTurn == BattleTurn.Lose)
					{
						if (battleEvent == BattleEvent.MenaceMurder)
						{
							Talk(new string[] {
								$"[color={RGB.LightMagenta}] 당신은 악마 사냥꾼에게 기습을 받아서  거의 다 죽게 되었을때  갑자기 낯익은 목소리가 먼곳에서 들려 왔다.[/color]",
								$"[color={RGB.LightMagenta}] {mPlayerList[0].Name}. 나는 레드 안타레스일세. 당신도 실력이 많이 줄었군. 이런 조무래기들에게 당하다니." +
								"  그럼 약간의 도움을 주도록하지. 잘 보게나.[/color]"
							});

							mSpecialEvent = SpecialEventType.HelpRedAntares;
						}
						else
						{
							ShowGameOver(new string[] {
								$"[color={RGB.LightMagenta}]일행은 모두 전투에서 패했다!![/color]",
								$"[color={RGB.LightGreen}]    어떻게 하시겠습니까?[/color]"
							});
						}
					}

					mBattleTurn = BattleTurn.None;
				}

				void AddBattleCommand(bool skip = false)
				{
					if (!skip)
					{
						mBattleCommandQueue.Enqueue(new BattleCommand()
						{
							Player = mPlayerList[mBattlePlayerID],
							FriendID = mBattleFriendID,
							Method = mBattleCommandID,
							Tool = mBattleToolID,
							EnemyID = mEnemyFocusID
						});

						if (mEnemyFocusID >= 0)
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);
					}

					do
					{
						mBattlePlayerID++;
					} while (mBattlePlayerID < mPlayerList.Count && !mPlayerList[mBattlePlayerID].IsAvailable);

					if (mBattlePlayerID < mPlayerList.Count)
						BattleMode();
					else
					{
						DialogText.TextHighlighters.Clear();
						DialogText.Blocks.Clear();

						mBattleTurn = BattleTurn.Player;

						ExecuteBattle();
					}
				}

				void ShowCureResult()
				{
					var resultPart = new List<string>();
					if (mCureResult.Count > DIALOG_MAX_LINES)
					{
						for (var i = 0; i < DIALOG_MAX_LINES; i++)
						{
							resultPart.Add(mCureResult[0]);
							mCureResult.RemoveAt(0);
						}
					}
					else
					{
						resultPart.AddRange(mCureResult);
						mCureResult.Clear();
					}

					Talk(resultPart.ToArray());
				}

				void ShowWeaponTypeMenu(int weaponCategory) {
					mWeaponTypeID = weaponCategory;

					if (0 <= weaponCategory && weaponCategory <= 3)
					{
						AppendText($"[color={RGB.White}]어떤 무기를 원하십니까?[/color]");

						var weaponNameArr = new string[7];
						for (var i = 1; i <= 7; i++)
						{
							if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 3)
								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
							else if (Common.GetWeaponName(mWeaponTypeID * 7 + i).Length < 5)
								weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
							else
								
							weaponNameArr[i - 1] = $"{Common.GetWeaponName(mWeaponTypeID * 7 + i)}\t금 {weaponPrice[mWeaponTypeID, i - 1].ToString("#,#0")} 개";
						}

						ShowMenu(MenuMode.BuyWeapon, weaponNameArr);
					}
					else if (weaponCategory == 4) {
						AppendText($"[color={RGB.White}]어떤 방패를 원하십니까?[/color]");

						var shieldNameArr = new string[5];
						for (var i = 1; i <= 5; i++){
							if (Common.GetShieldName(i).Length <= 5)
								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t\t금 {shieldPrice[i - 1].ToString("#,#0")} 개";
							else
								shieldNameArr[i - 1] = $"{Common.GetShieldName(i)}\t금 {shieldPrice[i - 1].ToString("#,#0")} 개";
						}

						ShowMenu(MenuMode.BuyShield, shieldNameArr);
					}
					else if (weaponCategory == 5) {
						AppendText($"[color={RGB.White}]어떤 갑옷을 원하십니까?[/color]");

						var armorNameArr = new string[10];
						for (var i = 1; i <= 10; i++)
						{
							if (Common.GetArmorName(i).Length <= 5)
								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
							else
								armorNameArr[i - 1] = $"{Common.GetArmorName(i)}\t금 {armorPrice[i - 1].ToString("#,#0")} 개";
						}

						ShowMenu(MenuMode.BuyArmor, armorNameArr);
					}
				}

				if (mLoading || (mAnimationEvent != AnimationType.None && ContinueText.Visibility != Visibility.Visible && mMenuMode == MenuMode.None) || mTriggeredDownEvent)
				{
					mTriggeredDownEvent = false;
					return;
				}
				else if (ContinueText.Visibility == Visibility.Visible)
				{
					async Task InvokeSpecialEventLaterPart()
					{
						var specialEvent = mSpecialEvent;
						mSpecialEvent = SpecialEventType.None;

						if (specialEvent == SpecialEventType.CantTrain)
							ShowTrainSkillMenu(mMenuFocusID);
						else if (specialEvent == SpecialEventType.CantTrainMagic)
							ShowTrainMagicMenu(mMenuFocusID);
						else if (specialEvent == SpecialEventType.TrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (specialEvent == SpecialEventType.TrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForSword)
							ShowChooseChangeSwordMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForMagic)
							ShowChooseChangeMagicMemberMenu();
						else if (specialEvent == SpecialEventType.CureComplete)
						{
							AppendText($"[color={RGB.White}]누가 치료를 받겠습니까?[/color]");

							ShowCharacterMenu(MenuMode.Hospital);
						}
						else if (specialEvent == SpecialEventType.NotCured)
						{
							ShowHealType();
						}
						else if (specialEvent == SpecialEventType.LeaveSoldier)
							InvokeAnimation(AnimationType.LeaveSoldier);
						else if (specialEvent == SpecialEventType.ViewGeniusKieLetter)
						{
							Talk(new string[] {
								$"{mPlayerList[0].Name}에게.",
								"",
								"",
								" 자네라면 여기에 올 거라고 믿고  이 글을 쓴다네. 나는 지금 은신 중이라  내가 있는 곳을 밝힐순 없지만 나는 지금 무사하다네." +
								"  자네와의 기나 긴 모험을 끝마치고 돌아 오던중 나는 나름대로의 뜻이 있어 로어성에 돌아오지 않았다네." +
								" 내 자신의 의지였다면 영영 로어성에 돌아오지 않았겠지만  곧 발생할  새로운 위협을 나는 느끼고 있기 때문에  자네에게 이런 쪽지를 보내게 되었네."
							});

							mSpecialEvent = SpecialEventType.ViewGeniusKieLetter2;
						}
						else if (specialEvent == SpecialEventType.ViewGeniusKieLetter2)
						{
							AppendText(new string[] {
								$" 로어성까지는 소문이 미치지 않았는지는 모르겠지만 로어 대륙의 남동쪽에는 알 수 없는 피라미드가 땅 속으로 부터 솟아 올랐다네." +
								"  나와 로어 헌터 둘이서 그 곳을 탐험 했었지. 그 곳에는 다시 두개의 동굴이 있었고 그 두곳은 지하 세계와 연결 되어 있었다네." +
								"  그 곳에 대해서는 지금 이 메모의 여백이 좁아서 말하기 어렵다네." +
								" 나는 지금 이 곳,저 곳 떠돌아 다니지만  북동쪽 해안의 오두막에 살고 있는 전투승 레굴루스에게 물어 보면  내가 있는 곳을 자세히 알 수 있을 걸세.",
								"",
								"",
								"",
								"                          지니어스 기로부터"
							});
						}
						else if (specialEvent == SpecialEventType.TalkPrisoner)
						{
							AppendText(new string[] {
							" 하지만 더욱 이상한것은 로드 안 자신도 그에 대한 사실을 인정하면서도  왜 우리에게는 그를 배격하도록만 교육시키는 가를  알고 싶을뿐입니다." +
							" 로드 안께서는 나를 이해한다고 하셨지만 사회 혼란을 방지하기 위해 나를 이렇게 밖에 할수 없다고 말씀하시더군요." +
							" 그리고 이것은 선을 대표하는 자기로서는 이 방법 밖에는 없다고 하시더군요.",
							" 하지만 로드 안의 마음은 사실 이렇지 않다는걸 알수 있었습니다.  에인션트 이블의 말로는 사실 서로가 매우 절친한 관계임을 알수가 있었기 때문입니다."
							});
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn)
						{
							Talk(" 또 자네의 힘을 빌릴 때가 온것 같네. 새로운 마력이 온통 이 세계를 휘감고 있다네." +
							$" 그것을 내가 깨닭았을때는  이미 그 새로운 존재가 민심을 선동하고 있었다네." +
							"  주민들은 [color={RGB.LightCyan}]다크 메이지[/color]가 있어서 곧 여기를 침공할 거라고 하며 나에게 방어를 요청했다네." +
							"  하지만 다크 메이지란 존재는 내가 알기로도 존재하지 않으며  나와 연대 관계에 있는 에인션트 이블도 역시 그런 존재를 모르고 있었다네." +
							"  하지만 주민들은 어떻게 알았는지 그 존재를 말하고 있다네. 그럼 이번에 해야할 임무를 말해 주겠네.");

							mSpecialEvent = SpecialEventType.MeetLordAhn2;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn2)
						{
							AppendText(new string[] { " 자네는 메너스란 동굴을 기억할 걸세. 그곳은 네크로만서가 사라진후 파괴되었고  지금은 다시 원래대로 광산이 되었다네." +
							"  하지만 언제부턴가 그곳은 잇달은 의문의 살인 때문에  지금은 거의 폐광이 되다시피 한 곳이라네." +
							" 주민들은 그 살인이  모두 다크 메이지의 짓이라고들 하고있네. 나도 다크 메이지의 존재를 믿고 싶지는 않지만  일이 이렇게 되었으니 어쩔수 없다네.",
							" 지금 즉시 메너스로 가서  진상을 밝혀 주게.",
							" 그리고 다크 메이지에 대한 정보도  알아 오도록 하게.  무기는 무기고에서 가져가도록 허락하지. 부탁하네.",
							$"[color={RGB.LightCyan}] [[ 경험치 + 10000 ] [[ 황금 + 1000 ][/color]"});

							mPlayerList[0].Experience += 10000;
							mParty.Gold += 1000;
							mParty.Etc[9]++;

							mSpecialEvent = SpecialEventType.None;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn3)
						{
							Talk(new string[] {
								" 그리고 내가 다크 메이지의 기원을 여러 방면으로 알아 보던 중에 이 책을 찾아 내었네.",
								$" 이 책은 당신도 알다시피  [color={RGB.LightCyan}]알비레오[/color]라고 하는 타임 워커가 이 대륙에 남기고 간 예언서이지." +
								" 중요한 부분만 해석하면 다음과 같네."
							});

							mSpecialEvent = SpecialEventType.MeetLordAhn4;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn4)
						{
							Talk(new string[] {
								$"[color={RGB.White}]첫번째 흉성이 나타나는 날, 평화는 로어의 신과 함께 대지로 추락할 것이며  공간이 어긋나고 대륙이 진동하며  새로운 존재가 나타난다.[/color]",
								"",
								" 이 글은 저번에 네크로만서가 나타날 때의 그 광경을 묘사한 구절이란것을 금방 알 수 있네.",
								"",
								$"[color={RGB.White}]두번째 흉성이 나타나는 날, 그는 용암의 대륙으로부터 세상을 뒤흔들게 되며 그는 네크로만서라 불리어 진다.[/color]",
								"",
								" 이 글 또한 몇년 전의 일과 일치 하네.",
								"",
								$"[color={RGB.White}]세번째 흉성이 나타나는 날, 네크로만서를  이기는 자는 아무도 없게 된다.[/color]",
								"",
								" 하지만 이 글은 사실과 다르다네.  자네가 네크로만서를 물리쳤기 때문에 말일세.",
								"",
								$"[color={RGB.White}]네번째 흉성이 나타나는 날, 메너스의 달이 붉게 물들때 어둠의 영혼이 나타나  세계의 종말을 예고한다.[/color]",
								"",
								" 이 글이 가장 중요한 요지라네. '메너스의 달이 붉게 물들때' 란 메너스에서 일어난 지금까지의 살인 사건을 말하는 것이고," +
								" '영혼' 이란 옛 부터 전승되는 구전에 의하면  영혼의 힘을 이용할 수 있는 사람,  즉 마법사를  지칭하는 말이 된다네." +
								"  그러므로 '어둠의 영혼'은 바로 어둠의 마법사를 뜻하는 말이 되네." +
								" 다시 풀이하면 그 뜻은 '메너스에서 살인 사건이 일어날때 다크 메이지가 세계의 종말을 예고 한다'라는 말이 된다네.",
								"",
								$"[color={RGB.White}]다섯번째 흉성이 나타나는 날, 내가 본 다섯번의 흉성 중에 하나가  나타나지 않았음을 알아낸다.[/color]",
								"",
								" 위에서의 예언을 보면 하나가 틀려 있지.  바로 세번째 흉성이 떨어질 때의 일 말일세.  그것이 틀렸다는 그 말일세." +
								" 알비레오 그 자신도 자네가 네크로만서를 물리치리라고는 생각하지 못했는데 자네 해 내었기 때문에 이런 말을 적었던것 같네."
							});

							mSpecialEvent = SpecialEventType.MeetLordAhn5;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn5)
						{
							AppendText(new string[] {
								" 이 예언에 의하면 반드시 다크 메이지가 세상을  멸망 시키게 된다네.  알비레오 그 자신도 그 것을 알려 주려 했던 것이고." +
								"  그렇다면 우리는 이번에도 저번의 네크로만서때 처럼 스스로의 운명을 바꾸기 위해  도전 보아야 한다는 결론을 얻을 수 있게 되네.",
								" 그럼 자네에게 또 하나의 할 일을 주겠네. 자네는 지금 즉시 라스트 디치성에 가도록 하게." +
								" 그곳에서도  우리와 같은 위기를  느끼고 있을것이고  이 보다 더 많은 정보가 있을 수도 있을 테니 한시 바삐 그 곳으로 가보도록 하게."
							});

							mParty.Etc[9]++;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn6)
						{
							Talk(new string[] {
								" 이 석판의 내용을 읽어 보겠네.",
								$"[color={RGB.White}] 어둠은 달이며 달은 여성이라." +
								$"  이 세계가 아르테미스를 3 번 범할때 어둠은 깨어나고 다크 메이지는[/color] [color={RGB.Yellow}]실리안 카미너스( Cyllian Cominus )[/color][color={RGB.White}]라고 불리운다." +
								" 그리고 그녀는 누구도 당해낼 수 없는 마력으로 세계를 종말로 이끄노니....[/color]",
								"",
								" 이글의 해석은 다음과 같다네.",
								" \"어둠의 마법사\"는 여자 마법사를 뜻하며, \"세계가 아르테미스를 범한다\"라는 구절은  지구가 달(아르테미스)을 가리는 현상," +
								" 즉 월식을 말하는 것이며 그 월식이 3번 일어난 후 \"어둠이 깨어난다\" 즉 실리안 카미너스라는 존재가 생겨난다는 것이네.  그리고는 그녀가 이 세계를 멸망시킨다는 것이네."
							});

							specialEvent = SpecialEventType.MeetLordAhn7;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn7)
						{
							Talk(new string[] {
								" 이 글이 정말이라면 알비레오의 예언은 실현되고 반드시 세계는 멸망하게 될 걸세. 하지만 자네는 언제나 운명에 대항에 왔으니 이런 운명적인 것에는 익숙하겠지 ?" +
								" 네크로만서와 싸울때도 그랬으니까.",
								$" 그리고 [color={RGB.White}]흉성의 단서[/color]에서 가져온  이 석판은 도저히 나의 힘으로는 해독이 안되는군." +
								"  이 세계의 운명이 달린 일이니  지금 당장 에인션트 이블과 함께 상의 해봐야 겠네.",
								" 그럼 내일 아침에 보도록하세."
							});

							specialEvent = SpecialEventType.SleepLoreCastle;
							mParty.Etc[9]++;
						}
						else if (specialEvent == SpecialEventType.SleepLoreCastle)
						{
							InvokeAnimation(AnimationType.SleepLoreCastle);

							specialEvent = SpecialEventType.None;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn8)
						{
							InvokeAnimation(AnimationType.TalkLordAhn);

							specialEvent = SpecialEventType.None;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn9)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							var eclipseDay = mParty.Day + 15;
							var eclipseYear = mParty.Year;

							if (eclipseDay > 360)
							{
								eclipseYear++;
								eclipseDay %= 360;
							}

							AppendText(new string[] {
								" 자네 말을 들어 보니 정말 큰일이군.  이러다간 정말 알비레오의 예언처럼 되어 버리겠는데." +
								" 자네가 지하 세계에 내려간 후 이 곳에도 많은 변화가 있었네.  갑자기 달의 운행이  빨라졌다네." +
								" 원래는 월식이 일어날 수 있는 보름달이 29.5일에 한번이었는데. 이제는 훨씬 더 빨라졌다네." +
								"  달의 운행이 빨라져서 달은 원심력이 증가했고  달을 붙잡아 두기 위해서 지구의 중력장이 증가 했다네." +
								" 지금은 거의 문제가 안될 정도이지만 이 상황이 점점 악화 된다면 지구는 백색 왜성이나 블랙홀처럼 스스로의 중력에 의해 파괴될 지도 모른다네.",
								$" 그리고 다음 월식이 일어날 날짜가 계산 되었다네. 날짜는 15일 뒤인 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일로 예정 되어 있다네." +
								" 그때까지 스스로를 단련 시키게. 그때 역시 잘 부탁하네."
							});

							mParty.Etc[36] = eclipseDay / 256;
							mParty.Etc[35] = eclipseDay % 256;
							mParty.Etc[38] = eclipseYear / 256;
							mParty.Etc[37] = eclipseYear % 256;

							mParty.Etc[9]++;
						}
						else if (specialEvent == SpecialEventType.MeetLordAhn10)
							InvokeAnimation(AnimationType.TalkLordAhn2);
						else if (specialEvent == SpecialEventType.MeetLordAhn11)
						{
							mAnimationEvent = AnimationType.None;
							mAnimationFrame = 0;

							var eclipseDay = mParty.Day + 5;
							var eclipseYear = mParty.Year;

							if (eclipseDay > 360)
							{
								eclipseYear++;
								eclipseDay %= 360;
							}

							AppendText(new string[] {
								" 음... 자네 말을 들어보니  정말 고생이 많았었군. 그리고 악의 추종자 두명을 처단한 일도 정말 수고했네." +
								" 하지만 벌써 마지막 세번째 월식 날짜가 임박했네. 사실 걱정은 바로 이것이네." +
								" 알비레오의 예언에 나오는 다크 메이지 실리안 카미너스가  부활하게 되는 시간은  바로 이번 월식이 일어나는 때라네. 아마 이번이 마지막 파견인것 같군. 그럼 날짜를 알려주지.",
								$" 마지막 세번째 월식은  바로 5 일 뒤인 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일 이라네." +
								" 자네가 만약 성공한다면 다시 로어의 세계는 평화가 올 것이지만 만약 실패한다면 우리가 이렇게 만나는 것도 마지막이 된다네."
							});

							mParty.Etc[36] = eclipseDay / 256;
							mParty.Etc[35] = eclipseDay % 256;
							mParty.Etc[38] = eclipseYear / 256;
							mParty.Etc[37] = eclipseYear % 256;

							mParty.Etc[9]++;
						}
						else if (specialEvent == SpecialEventType.DestructCastleLore)
						{
							Talk(new string[] {
								" 다크 메이지는 가공할 힘으로 전 대륙에 결계를 형성하기 시작했고 결계속의 물체들은 서서히 형체를 잃어가기 시작했다." +
								"  이제는 실리안 카미너스를 제어할 수있는 메피스토펠레스마저 사라져 버려서  그녀는 의지의 중심을 잃고 한없이 그녀의 힘을 방출하기 시작했다.",
								" 이제는 누구도 그녀의 기하 급수적인 힘의 폭주를 막을 수가 없었고 이미 당신의 의식도 흐려져 갔다."
							});

							mSpecialEvent = SpecialEventType.DestructCastleLore2;
						}
						else if (specialEvent == SpecialEventType.DestructCastleLore2)
						{
							Talk(new string[] {
								" 하지만 이때, 실리안 카미너스의 마법 결계를 깨트리며 희미한 의지가 스며들어왔다." +
								" 그것은 레드 안타레스의 마지막 의지였고 그는 그녀의 결계를 버티며 마지막 말을 한다.",
								"",
								$"[color={RGB.LightMagenta}] 다크 메이지의 힘은 영혼인 나조차도 소멸 시킬 힘이 있다는 걸 알았네." +
								"  지금은 나의 마법력으로 나 혼자 정도를 보호할 수 있는 결계를 구성했지만 곧 나의 힘도 다하게 될 걸세.  나는 마지막으로 한가지의 묘안을 생각해 내었네." +
								" 자네는 알비레오의 예언을 기억 하겠지.  그중 마지막 예언을 외워보게.  거기에는 대단한 모순이 있네." +
								" 지금은 그의 세번째 예언인 \"네크로만서에게 이긴자는 아무도 없었다.\"라는 구절이 실행되지 않은 예언이라고 생각되고 있지.  하지만 반대로 생각해보게." +
								"  자네가 만약 지금이라도 네크로만서에게 패한다면 네번째의 예언이 실현 되지 않는 예언이 된다네." +
								" 그렇게만 된다면  다크 메이지는 처음부터 없었던 존재가 되어 버리고  로어 대륙은 원래의 모습대로 시간을 진행해 나가게 된다네." +
								" 나의 결계가 점점 약해져 가는군. 나의 소멸을 헛되게 하지는말게.  그리고 자신보다 로어의 운명을 먼저 생각하도록 하게.... 그..그럼... 다..다음 세상에서......[/color]"
							});

							mSpecialEvent = SpecialEventType.DestructCastleLore3;
						}
						else if (specialEvent == SpecialEventType.DestructCastleLore3)
						{
							AppendText(new string[] {
								" 그의 의지는 점점 희미해 지더니 결국 암흑으로 사라지고 말았다.",
								" 당신은 어떻게 하겠는가?"
							});

							ShowMenu(MenuMode.FinalChoice, new string[] {
								"레드 안타레스의 말대로 한다",
								"다크 메이지와 싸우고 싶다"
							});
						}
						else if (specialEvent == SpecialEventType.EnterUnderworld)
						{
							InvokeAnimation(AnimationType.EnterUnderworld);
						}
						else if (specialEvent == SpecialEventType.SeeDeadBody)
						{
							mFace = 5;
							InvokeAnimation(AnimationType.GoInsideMenace);
						}
						else if (specialEvent == SpecialEventType.BattleMenace)
						{
							mBattleEvent = BattleEvent.MenaceMurder;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BackToBattleMode)
						{
							if (BattlePanel.Visibility == Visibility.Collapsed)
								DisplayEnemy();
							BattleMode();
						}
						else if (specialEvent == SpecialEventType.HelpRedAntares)
						{
							AppendText(new string[] { $"[color={RGB.White}] 레드 안타레스는 '차원 이탈' 을 모든 적에게 사용했다.[/color]", "" });

							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Unconscious)
								{
									AppendText($"[color={RGB.LightRed}] {enemy.NameSubjectJosa} 의식을 잃었다[/color]", true);
									enemy.HP = 0;
									enemy.Unconscious = true;
								}
							}

							DisplayEnemy();
							ContinueText.Visibility = Visibility.Visible;

							mSpecialEvent = SpecialEventType.HelpRedAntares2;
						}
						else if (specialEvent == SpecialEventType.HelpRedAntares2)
						{
							Talk($"[color={RGB.LightMagenta}] 나는 언제나 당신 주위에서  당신에게 도움을 주도록 하겠네. 그리고 내가 알아낸 바로는 다크 메이지는  존재 한다네." +
							"  이제 부터가 다크 메이지와의 결전이 시작 되는 걸세. 그럼 다음에 또 보도록 하세.[/color]");

							foreach (var player in mPlayerList)
							{
								if (player.HP < 0 || player.Unconscious > 0 || player.Dead > 0)
								{
									player.HP = 1;
									player.Unconscious = 0;
									player.Dead = 0;
								}
							}

							if (mAssistPlayer != null && (mAssistPlayer.HP < 0 || mAssistPlayer.Unconscious > 0 || mAssistPlayer.Dead > 0))
							{
								mAssistPlayer.HP = 1;
								mAssistPlayer.Unconscious = 0;
								mAssistPlayer.Dead = 0;
							}

							DisplayHP();
							DisplayCondition();

							mSpecialEvent = SpecialEventType.HelpRedAntares3;
						}
						else if (specialEvent == SpecialEventType.HelpRedAntares3)
						{
							AppendText("");

							mEncounterEnemyList.Clear();
							ShowMap();
						}
						else if (specialEvent == SpecialEventType.CantBuyWeapon)
							ShowWeaponTypeMenu(mWeaponTypeID);
						else if (specialEvent == SpecialEventType.CantBuyExp)
							ShowExpStoreMenu();
						else if (specialEvent == SpecialEventType.CantBuyItem)
							ShowItemStoreMenu();
						else if (specialEvent == SpecialEventType.CantBuyMedicine)
							ShowMedicineStoreMenu();
						else if (specialEvent == SpecialEventType.MeetGeniusKie)
						{
							AppendText(new string[] {
								" 나는 이곳에서 너무 많이 머물렀다네.",
								" 자네도 만났으니 나의 할 일은 다 끝났다네.",
								" 조금만 있다가 나도 새로운 여행을 떠나야 하지. 그럼 다음에 보도록 하지"
							});

							mParty.Etc[34] |= 1 << 4;
						}
						else if (specialEvent == SpecialEventType.WizardEye)
						{
							AppendText("");
							mWizardEyeTimer.Stop();
							mWizardEyePosBlink = false;
						}
						else if (specialEvent == SpecialEventType.Penetration)
							AppendText("");
						else if (specialEvent == SpecialEventType.Telescope) {
							if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
							{
								if ((mTelescopeXCount != 0 && (mParty.XAxis + mTelescopeXCount <= 4 || mParty.XAxis + mTelescopeXCount >= mMapWidth - 4)) ||
									(mTelescopeXCount != 0 && (mParty.YAxis + mTelescopeYCount <= 5 || mParty.YAxis + mTelescopeYCount >= mMapHeight - 4)))
								{
									mTelescopeXCount = 0;
									mTelescopeYCount = 0;
									return;
								}

								if (mTelescopeXCount < 0)
									mTelescopeXCount++;

								if (mTelescopeXCount > 0)
									mTelescopeXCount--;

								if (mTelescopeYCount < 0)
									mTelescopeYCount++;

								if (mTelescopeYCount > 0)
									mTelescopeYCount--;

								if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
								{
									Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
									mSpecialEvent = SpecialEventType.Telescope;
								}
								else
									AppendText("");
							}
						}
						else if (specialEvent == SpecialEventType.BattleCaveOfBerialEntrance) {
							mBattleEvent = BattleEvent.CaveOfBerialEntrance;
							mEncounterEnemyList.Clear();
							for (var i = 0; i < 8; i++)
								JoinEnemy(31);

							DisplayEnemy();
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.InvestigationCave) {
							AppendText(new string[] {
								$" {mPlayerList[1].NameSubjectJosa} 동굴의 입구를 조사하더니 당신에게 말했다",
								"",
								$"[color={RGB.Cyan}] 잠깐 여기를 보게. 이 입구에 떨어져 있는 흙은 다른 곳의 흙과는 다르다네. 이 걸 보게나." +
								"모래가 거의 유리처럼 변해있지 않은가. 이 건 분명히 핵 폭발이  여기에서 일어 났었다는 증거 일세." +
								"  이 동굴의 입구가 함몰된 이유는 바로 핵 무기나 아니면 거기에 필적하는 초 자연적인  마법에 의해서 인것 같네." +
								"  그렇다면 이 동굴 안의 세계에 존재하는 인물 중에서 이 정도의 고수준 마법을  사용하는 사람이  있다는 셈이라네.[/color]"
							});

							mParty.Etc[30] |= 1 << 7;
						}
						else if (specialEvent == SpecialEventType.BattleCaveOfAsmodeusEntrance) {
							mEncounterEnemyList.Clear();
							for (var i = 0; i < 8; i++) {
								if (i == 3)
									JoinEnemy(63);
								else if (i == 7)
									JoinEnemy(64);
								else
									JoinEnemy(54);
							}

							mBattleEvent = BattleEvent.CaveOfAsmodeusEntrance;
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.GetCromaticShield) {
							AppendText($"[color={RGB.LightGreen}] 유골이 지니고 있던 크로매틱 방패를 가질 사람을 고르시오.[/color]");

							ShowCharacterMenu(MenuMode.ChooseEquipCromaticShield, false);
						}
						else if (specialEvent == SpecialEventType.GetCromaticShield)
						{
							AppendText($"[color={RGB.LightGreen}] 유골이 지니고 있던  양날 전투 도끼를  가질 사람을 고르시오.[/color]");

							ShowCharacterMenu(MenuMode.ChooseEquipBattleAxe, false);
						}
						else if (specialEvent == SpecialEventType.GetObsidianArmor)
						{
							AppendText($"[color={RGB.LightGreen}] 호수에서 떠오른  흑요석 갑옷을 장착할 사람을 고르시오.[/color]");

							ShowCharacterMenu(MenuMode.ChooseEquipObsidianArmor, false);
						}
						else if (specialEvent == SpecialEventType.BattleGuardOfObsidianArmor) {
							mBattleEvent = BattleEvent.GuardOfObsidianArmor;

							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleSlaim) {
							mBattleEvent = BattleEvent.Slaim;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 8; i++)
								JoinEnemy(22);

							DisplayEnemy();
							StartBattle(false);
						}
						else if (specialEvent == SpecialEventType.BattleCaveEntrance)
						{
							mBattleEvent = BattleEvent.CaveEntrance;

							mEncounterEnemyList.Clear();

							for (var i = 0; i < 5; i++)
								JoinEnemy(i + 25);

							DisplayEnemy();
							StartBattle(false);
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp ||
						args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft ||
						args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight ||
						args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
						return;

					ContinueText.Visibility = Visibility.Collapsed;

					//if (mBattleTurn == BattleTurn.None)
					//{
					//	DialogText.Blocks.Clear();
					//	DialogText.TextHighlighters.Clear();
					//}

					if (mCureResult.Count > 0)
					{
						ShowCureResult();
					}
					else if (mRemainDialog.Count > 0)
					{
						DialogText.Blocks.Clear();
						DialogText.TextHighlighters.Clear();

						var added = true;
						while (added && mRemainDialog.Count > 0)
						{
							added = AppendText(mRemainDialog[0], true);
							if (added)
								mRemainDialog.RemoveAt(0);
						}

						ContinueText.Visibility = Visibility.Visible;
					}
					else if (mSpecialEvent != SpecialEventType.None)
						await InvokeSpecialEventLaterPart();
					else if (mCureBattle)
					{
						mCureBattle = false;
						//AddBattleCommand(true);
					}
					else if (mBattleTurn == BattleTurn.Player)
					{
						if (mBattleCommandQueue.Count == 0)
						{
							var allUnavailable = true;
							foreach (var enemy in mEncounterEnemyList)
							{
								if (!enemy.Dead && !enemy.Unconscious)
								{
									allUnavailable = false;
									break;
								}
							}

							if (allUnavailable)
							{
								mBattleTurn = BattleTurn.Win;
								await EndBattle();
							}
							else
								ExecuteBattle();
						}
						else
							ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.Enemy)
					{
						ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.RunAway || mBattleTurn == BattleTurn.Win || mBattleTurn == BattleTurn.Lose)
					{
						await EndBattle();
					}
					else if (mWeaponShopEnd)
					{
						//mWeaponShopEnd = false;
						//GoWeaponShop();
					}
				}
				else if (mMenuMode == MenuMode.None && (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadMenu))
				{
					AppendText($"[color={RGB.Red}]당신의 명령을 고르시오 ===>[/color]");

					ShowMenu(MenuMode.Game, new string[]
					{
						"일행의 상황을 본다",
						"개인의 상황을 본다",
						"일행의 건강 상태를 본다",
						"마법을 사용한다",
						"초능력을 사용한다",
						"여기서 쉰다",
						"물품을 서로 교환한다",
						"물품을 사용한다",
						"게임 선택 상황"
					});
				}
				else if (mSpinnerType != SpinnerType.None)
				{
					void CloseSpinner()
					{
						SpinnerText.TextHighlighters.Clear();
						SpinnerText.Blocks.Clear();
						SpinnerText.Visibility = Visibility.Collapsed;

						mSpinnerItems = null;
						mSpinnerID = 0;
						mSpinnerType = SpinnerType.None;
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						mSpinnerID = (mSpinnerID + 1) % mSpinnerItems.Length;

						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mSpinnerID == 0)
							mSpinnerID = mSpinnerItems.Length - 1;
						else
							mSpinnerID--;

						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						AppendText("");
						CloseSpinner();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						var spinnerType = mSpinnerType;
						mSpinnerType = SpinnerType.None;

						if (spinnerType == SpinnerType.TeleportationRange)
						{
							int moveX = mParty.XAxis;
							int moveY = mParty.YAxis;

							switch (mTeleportationDirection)
							{
								case 0:
									moveY -= mSpinnerItems[mSpinnerID].Item2;
									break;
								case 1:
									moveY += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 2:
									moveX += mSpinnerItems[mSpinnerID].Item2;
									break;
								case 3:
									moveX -= mSpinnerItems[mSpinnerID].Item2;
									break;
							}

							if (moveX < 4 || moveX > mMapWidth - 4 || moveY < 4 || moveY > mMapHeight - 4)
								AppendText("공간 이동이 통하지 않습니다.");
							else
							{
								var valid = false;
								if (mPosition == PositionType.Town)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Ground)
								{
									if (24 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Den)
								{
									if (41 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}
								else if (mPosition == PositionType.Keep)
								{
									if (27 <= GetTileInfo(moveX, moveY) && GetTileInfo(moveX, moveY) <= 47)
										valid = true;
								}

								if (!valid)
									AppendText("공간 이동 장소로 부적합 합니다.");
								else
								{
									mMagicPlayer.SP -= 50;

									if (GetTileInfo(moveX, moveY) == 0 || ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(moveX, moveY) == 52))
										AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신을 배척합니다.[/color]");
									else
									{
										mParty.XAxis = moveX;
										mParty.YAxis = moveY;

										AppendText($"[color={RGB.White}]공간 이동 마법이 성공했습니다.[/color]");
									}
								}
							}
						}
						else if (spinnerType == SpinnerType.RestTimeRange) {
							var append = false;
							var restTime = mSpinnerItems[mSpinnerID].Item2;

							void RestPlayer(Lore player) {
								if (mParty.Food <= 0)
									AppendText($"[color={RGB.Red}]일행은 식량이 바닥났다[/color]", append);
								else
								{
									if (player.Dead > 0)
										AppendText($"{player.NameSubjectJosa} 죽었다", append);
									else if (player.Unconscious > 0 && player.Poison == 0)
									{
										player.Unconscious = player.Unconscious - (player.Level * restTime / 2);
										if (player.Unconscious <= 0)
										{
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 의식이 회복되었다[/color]", append);
											player.Unconscious = 0;
											if (player.HP <= 0)
												player.HP = 1;

#if DEBUG
											//mParty.Food--;
#else
											mParty.Food--;
#endif

										}
										else
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 여전히 의식 불명이다[/color]", append);
									}
									else if (player.Unconscious > 0 && player.Poison > 0)
										AppendText($"독때문에 {player.Name}의 의식은 회복되지 않았다", append);
									else if (player.Poison > 0)
										AppendText($"독때문에 {player.Name}의 건강은 회복되지 않았다", append);
									else
									{
										var recoverPoint = player.Level * restTime;
										if (player.HP >= player.Endurance * player.Level * 10)
										{
											if (mParty.Food < 255)
											{
#if DEBUG
												//mParty.Food++;
#else
												mParty.Food++;
#endif
											}
										}

										player.HP += recoverPoint;

										if (player.HP >= player.Endurance * player.Level * 10)
										{
											player.HP = player.Endurance * player.Level * 10;

											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 모든 건강이 회복되었다[/color]", append);
										}
										else
											AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 치료되었다[/color]", append);

#if DEBUG
										//mParty.Food--;
#else
										mParty.Food--;
#endif
									}

									if (append == false)
										append = true;
								}
							}

							void RecoverStat(Lore player) {
								if (player.ClassType == ClassCategory.Magic)
									player.SP = player.Mentality * player.Level * 10;
								else if (player.ClassType == ClassCategory.Sword)
									player.SP = player.Mentality * player.Level * 5;
								else
									player.SP = 0;

								AppendText("", true);

								if (player.IsAvailable) {
									var exp = player.PotentialExperience;
									var levelUp = 0;
									do
									{
										levelUp++;
									} while (levelUp < Common.GetLevelUpExperience(levelUp) || levelUp > 40);
									levelUp--;

									if (player.Level < levelUp || levelUp == 40) {
										if (levelUp < 40 || player.Level != 40) {
											AppendText($"[color={RGB.LightCyan}]{player.Name}의 레벨은 {levelUp}입니다");
											player.Level = levelUp;
										}
									}
								}
							}

							foreach (var player in mPlayerList)
							{
								RestPlayer(player);
							}

							if (mAssistPlayer != null)
								RestPlayer(mAssistPlayer);

							if (mParty.Etc[0] > 0)
							{
								var decPoint = restTime / 3 + 1;
								if (mParty.Etc[0] >= decPoint)
									mParty.Etc[0] -= decPoint;
								else
									mParty.Etc[0] = 0;
							}

							for (var i = 1; i < 4; i++)
								mParty.Etc[i] = 0;

							foreach (var player in mPlayerList)
							{
								RecoverStat(player);
							}

							if (mAssistPlayer != null)
								RecoverStat(mAssistPlayer);

							UpdatePlayersStat();
							PlusTime(restTime, 0, 0);
						}

						CloseSpinner();
					}
				}
				else if (mMenuMode != MenuMode.None)
				{
					void ShowTrainMessage() {
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 충분한 전투 경험을 쌓았다면, 당신은 더욱 능숙하게 무기를 다룰것입니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.TrainSkill;
					}

					void ShowTrainMagicMessage() {
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 충분한 실전 경험을 쌓았다면, 당신은 더욱 능숙하게 마법을 다룰것입니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.TrainMagic;
					}

					void ShowChangeJobForSwordMessage() {
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 군사 훈련소 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.ChangeJobForSword;
					}

					void ShowChangeJobForMagicMessage()
					{
						Talk(new string[] {
							$"[color={RGB.White}] 여기는 마법 학교 입니다.[/color]",
							$"[color={RGB.White}] 만약 당신이 원한다면 새로운 계급으로 바꿀 수가 있습니다.[/color]",
						});

						mSpecialEvent = SpecialEventType.ChangeJobForMagic;
					}

					void ShowCastOneMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.AttackMagic / 10;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(0, i);
							}

							ShowMenu(MenuMode.CastOneMagic, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastAllMagicMenu()
					{
						string[] menuStr;

						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.AttackMagic / 10;

						if (availCount > 0)
						{
							menuStr = new string[availCount];
							for (var i = 1; i <= 10; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(1, i);
							}

							ShowMenu(MenuMode.CastAllMagic, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastSpecialMenu()
					{
						var player = mPlayerList[mBattlePlayerID];

						int availCount = player.SpecialMagic / 10;

						if (availCount > 0)
						{
							var menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(4, i);
							}

							ShowMenu(MenuMode.CastSpecial, menuStr);
						}
						else
							BattleMode();
					}

					void ShowCastESPMenu() {
						var menuStr = new string[] { Common.GetMagicName(5, 3), Common.GetMagicName(5, 5) };
						ShowMenu(MenuMode.CastESP, menuStr);
					}

					void ShowCureDestMenu(Lore player, MenuMode menuMode)
					{
						int curePoint;

						mMagicPlayer = mPlayerList[mBattlePlayerID];
						if (mMagicPlayer.ClassType == ClassCategory.Magic)
							curePoint = mMagicPlayer.CureMagic / 10;
						else
							curePoint = mMagicPlayer.AxeSkill / 10;

						if (curePoint <= 0)
						{
							Talk("당신은 치료 마법을 사용할 능력이 없습니다.");
							if (menuMode == MenuMode.ChooseBattleCureSpell)
								mSpecialEvent = SpecialEventType.BackToBattleMode;
							else
								ContinueText.Visibility = Visibility.Visible;
						}
						else
						{
							AppendText(new string[] { "누구에게" });
							string[] playerList;

							int availCount;
							if (player.ClassType == ClassCategory.Magic)
								availCount = player.CureMagic / 10;
							else
								availCount = player.AxeSkill / 10;

							if (availCount < 6)
							{
								playerList = new string[mAssistPlayer == null ? mPlayerList.Count : mPlayerList.Count + 1];
								if (mAssistPlayer != null)
									playerList[playerList.Length - 1] = mAssistPlayer.Name;
							}
							else
							{
								playerList = new string[mAssistPlayer == null ? mPlayerList.Count + 1 : mPlayerList.Count + 2];
								if (mAssistPlayer != null)
									playerList[playerList.Length - 2] = mAssistPlayer.Name;
								playerList[playerList.Length - 1] = "모든 사람들에게";
							}


							for (var i = 0; i < mPlayerList.Count; i++)
								playerList[i] = mPlayerList[i].Name;

							ShowMenu(menuMode, playerList);
						}
					}

					void ShowSummonMenu() {
						var player = mPlayerList[mBattlePlayerID];

						var availCount = player.SummonMagic / 10;

						if (availCount > 0)
						{
							var menuStr = new string[availCount];
							for (var i = 1; i <= availCount; i++)
							{
								menuStr[i - 1] = Common.GetMagicName(6, i);
							}

							ShowMenu(MenuMode.CastSummon, menuStr);
						}
						else
							BattleMode();
					}

					void UseItem(Lore player, bool battle) {
						mUsableItemIDList.Clear();
						mItemUsePlayer = player;

						var itemNames = new string[] { "체력 회복약', '마법 회복약', '해독의 약초', '의식의 약초', '부활의 약초', '소환 문서', '대형 횃불', '수정 구슬', '비행 부츠', '이동 구슬" };

						var itemMenuItemList = new List<string>();
						for (var i = 0; i < mParty.Item.Length; i++) {
							if (mParty.Item[i] > 0)
							{
								itemMenuItemList.Add(itemNames[i]);
								mUsableItemIDList.Add(i);
							}
						}

						if (battle)
						{
							if (mUsableItemIDList.Count > 0)
								ShowMenu(MenuMode.BattleChooseItem, itemMenuItemList.ToArray());
							else
								BattleMode();
						}
						else
						{
							if (mUsableItemIDList.Count > 0)
								ShowMenu(MenuMode.ChooseItem, itemMenuItemList.ToArray());
							else
								AppendText("가지고 있는 아이템이 없습니다.");
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								if (mEnemyFocusID == 0)
									mEnemyFocusID = mEncounterEnemyList.Count - 1;
								else
									mEnemyFocusID--;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);


							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else
						{
							if (mMenuFocusID == 0)
								mMenuFocusID = mMenuCount - 1;
							else
								mMenuFocusID--;

							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						if (mMenuMode == MenuMode.EnemySelectMode)
						{
							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

							var init = mEnemyFocusID;
							do
							{
								mEnemyFocusID = (mEnemyFocusID + 1) % mEncounterEnemyList.Count;
							} while (init != mEnemyFocusID && mEncounterEnemyList[mEnemyFocusID].Dead == true);

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}
						else
						{
							mMenuFocusID = (mMenuFocusID + 1) % mMenuCount;
							FocusMenuItem();
						}
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
					{
						AppendText("");
						var menuMode = HideMenu();

						if (menuMode == MenuMode.ChooseTrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (menuMode == MenuMode.ChooseTrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (menuMode == MenuMode.BuyWeapon || menuMode == MenuMode.BuyShield || menuMode == MenuMode.BuyArmor)
							ShowWeaponShopMenu();
						else if (menuMode != MenuMode.None && menuMode != MenuMode.BattleLose && menuMode != MenuMode.ChooseGameOverLoadGame && mSpecialEvent == SpecialEventType.None)
						{
							if (menuMode == MenuMode.CastOneMagic ||
							menuMode == MenuMode.CastAllMagic ||
							menuMode == MenuMode.CastSpecial ||
							menuMode == MenuMode.ChooseBattleCureSpell ||
							menuMode == MenuMode.CastESP ||
							menuMode == MenuMode.CastSummon)
							{
								BattleMode();
							}
							else if (menuMode == MenuMode.ChooseESPMagic)
							{
								ShowCastESPMenu();
							}
							else if (menuMode == MenuMode.EnemySelectMode)
							{
								mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.Transparent);

								switch (mBattleCommandID)
								{
									case 0:
										BattleMode();
										break;
									case 1:
										ShowCastOneMagicMenu();
										break;
									case 3:
										ShowCastSpecialMenu();
										break;
									case 5:
										BattleMode();
										break;
								}
							}
							else if (menuMode == MenuMode.ApplyBattleCureSpell || menuMode == MenuMode.ApplyBattleCureAllSpell)
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							else if (menuMode == MenuMode.BattleStart ||
								menuMode == MenuMode.BattleCommand)
								return;
							else if (menuMode == MenuMode.ConfirmExitMap)
							{
								mParty.YAxis--;
							}
							else if (menuMode == MenuMode.AskEnter) {
								if (mTryEnterType == EnterType.CabinOfRegulus) {
									mParty.XAxis = mPrevX;
									mParty.YAxis = mPrevY;
								}
							}
						}
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						void SelectEnemy()
						{
							mMenuMode = MenuMode.EnemySelectMode;

							for (var i = 0; i < mEncounterEnemyList.Count; i++)
							{
								if (!mEncounterEnemyList[i].Dead)
								{
									mEnemyFocusID = i;
									break;
								}
							}

							mEnemyBlockList[mEnemyFocusID].Background = new SolidColorBrush(Colors.LightGray);
						}

						void ShowFileMenu(MenuMode mode)
						{
							if (mode == MenuMode.ChooseLoadGame || mode == MenuMode.ChooseGameOverLoadGame)
								AppendText("불러내고 싶은 게임을 선택하십시오.");
							else
								AppendText("게임의 저장 장소를 선택하십시오.");

							ShowMenu(mode, new string[] {
													"본 게임 데이터",
													"게임 데이터 1 (부)",
													"게임 데이터 2 (부)",
													"게임 데이터 3 (부)",
													"게임 데이터 4 (부)",
													"게임 데이터 5 (부)",
													"게임 데이터 6 (부)",
													"게임 데이터 7 (부)",
													"게임 데이터 8 (부)"
												});
						}

						async Task<bool> LoadGame(int id)
						{
							mMenuMode = MenuMode.None;

							var success = await LoadFile(id);
							if (success)
							{
								mBattlePlayerID = 0;
								mBattleFriendID = 0;
								mBattleCommandID = 0;
								mBattleToolID = 0;
								mEnemyFocusID = 0;
								mBattleCommandQueue.Clear();
								//mBatteEnemyQueue.Clear();
								mBattleTurn = BattleTurn.None;

								mSpecialEvent = SpecialEventType.None;
								mBattleEvent = BattleEvent.None;

								//ShowMap();

								AppendText(new string[] { $"[color={RGB.LightCyan}]저장했던 게임을 다시 불러옵니다.[/color]" });

								return true;
							}
							else
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]해당 슬롯에는 저장된 게임이 없습니다. 다른 슬롯을 선택해 주십시오.[/color]" });

								ShowFileMenu(MenuMode.ChooseLoadGame);
								return false;
							}
						}

						void ShowApplyItemResult(MenuMode choiceMenuMode, string result)
						{
							switch (choiceMenuMode) {
								case MenuMode.BattleChooseItem:
								case MenuMode.BattleUseItemWhom:
									Talk(result);
									mSpecialEvent = SpecialEventType.BattleUseItem;
									break;
								default:
									AppendText(result);
									break;
							}
						}

						void ShowWizardEye() {
							var xInit = 0;
							var yInit = 0;
							var width = 0;
							var height = 0;
							
							if (mMapWidth <= 100) {
								xInit = 0;
								width = mMapWidth;
							}
							else {
								xInit = mParty.XAxis - 50;

								if (xInit <= 0)
								{
									xInit = 0;

									if (mMapWidth > 100)
										width = 100;
									else
										width = mMapWidth;
								}
								else if (xInit + 100 > mMapWidth)
								{
									if (mMapWidth > 100)
									{
										xInit = mMapWidth - 100;
										width = 100;
									}
									else
									{
										xInit = 0;
										width = mMapWidth;
									}
								}
								else
									width = 100;
							}

							if (mMapHeight <= 80) {
								yInit = 0;
								Height = mMapHeight;
							}
							else
							{
								yInit = mParty.YAxis - 40;

								if (yInit <= 0)
								{
									yInit = 0;

									if (mMapHeight > 80)
										height = 80;
									else
										height = mMapHeight;
								}
								else if (yInit + 80 > mMapHeight)
								{
									if (mMapHeight > 80)
									{
										yInit = mMapHeight - 80;
										height = 80;
									}
									else
									{
										yInit = 0;
										height = mMapHeight;
									}
								}
								else
									height = 80;
							}

							MapCanvas.Width = width * 4;
							MapCanvas.Height = height * 4;

							lock (mWizardEye) {
								mWizardEye.Set(width, height);

								var offset = 0;

								const int BLACK = 0;
								const int BLUE = 1;
								const int GREEN = 2;
								const int CYAN = 3;
								const int RED = 4;
								const int MAGENTA = 5;
								const int BROWN = 6;
								const int LIGHTGRAY = 7;
								const int DARKGRAY = 8;
								const int LIGHTBLUE = 9;
								const int LIGHTGREEN = 10;
								const int LIGHTCYAN = 11;
								const int LIGHTRED = 12;
								const int LIGHTMAGENTA = 13;
								const int YELLOW = 14;
								const int WHITE = 15;

								for (var y = yInit; y < yInit + height; y++) {
									for (var x = xInit; x < xInit + width; x++) {
										if (mParty.XAxis == x && mParty.YAxis == y) {
											mWizardEyePosX = offset % width;
											mWizardEyePosY = offset / width;
										}

										var tileInfo = GetTileInfo(x, y);
										if (mPosition == PositionType.Town)  {
											if ((1 <= tileInfo && tileInfo <= 18) || tileInfo == 20 || tileInfo == 21)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 22)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 23)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 24)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 25)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 26)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0 || tileInfo == 19 || (27 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}
										else if (mPosition == PositionType.Ground) {
											if (1 <= tileInfo && tileInfo <= 20)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 22)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 23 || tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0) {
												if (mParty.Map == 4)
													mWizardEye.Data[offset] = WHITE;
												else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (24 <= tileInfo && tileInfo <= 47)
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTGREEN;

										}
										else if (mPosition == PositionType.Den) {
											if ((1 <= tileInfo && tileInfo <= 40) || tileInfo == 51)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 54)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 53)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 52)
											{
												if (mParty.Map == 2)
													mWizardEye.Data[offset] = LIGHTBLUE;
												else
													mWizardEye.Data[offset] = BLACK;
											}
											else if (tileInfo == 0 || (41 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}
										else if (mPosition == PositionType.Keep) {
											if ((1 <= tileInfo && tileInfo <= 39) || tileInfo == 51)
												mWizardEye.Data[offset] = WHITE;
											else if (tileInfo == 54)
												mWizardEye.Data[offset] = LIGHTGREEN;
											else if (tileInfo == 53)
												mWizardEye.Data[offset] = LIGHTCYAN;
											else if (tileInfo == 48)
												mWizardEye.Data[offset] = LIGHTBLUE;
											else if (tileInfo == 49)
												mWizardEye.Data[offset] = CYAN;
											else if (tileInfo == 50)
												mWizardEye.Data[offset] = LIGHTRED;
											else if (tileInfo == 0 || tileInfo == 52 || (40 <= tileInfo && tileInfo <= 47))
												mWizardEye.Data[offset] = BLACK;
											else
												mWizardEye.Data[offset] = LIGHTMAGENTA;
										}

										offset++;
									}
								}
							}

							mWizardEyeTimer.Start();
							MapCanvas.Visibility = Visibility.Visible;

							Talk(" 주지사의 눈을 통해 이 지역을 바라보고 있다.");
							mSpecialEvent = SpecialEventType.WizardEye;
						}

						void ShowCureSpellMenu(Lore player, int whomID, MenuMode applyCureMode, MenuMode applyAllCureMode)
						{
							if (whomID < mPlayerList.Count)
							{
								int availableCount;
								if (player.ClassType == ClassCategory.Magic)
									availableCount = player.CureMagic / 10;
								else
									availableCount = player.AxeSkill / 10;

								if (availableCount > 5)
									availableCount = 5;

								var cureMagicMenu = new string[availableCount];
								for (var i = 1; i <= availableCount; i++)
									cureMagicMenu[i - 1] = Common.GetMagicName(3, i);

								ShowMenu(applyCureMode, cureMagicMenu);
							}
							else
							{
								int availableCount;
								if (player.ClassType == ClassCategory.Magic)
									availableCount = player.CureMagic / 10 - 5;
								else
									availableCount = player.AxeSkill / 10 - 5;

								var cureMagicMenu = new string[availableCount];
								for (var i = 6; i < 6 + availableCount; i++)
									cureMagicMenu[i - 6] = Common.GetMagicName(3, i);

								ShowMenu(applyAllCureMode, cureMagicMenu);
							}
						}

						void Teleport(MenuMode newMenuMode) {
							if (mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
								AppendText($"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 공간 이동을 방해 합니다.[/color]");
							else
							{
								AppendText($"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]");

								ShowMenu(newMenuMode, new string[] { "북쪽으로 공간이동",
														"남쪽으로 공간이동",
														"동쪽으로 공간이동",
														"서쪽으로 공간이동" });
							}
						}

						bool VerifyWeapon(Lore equipPlayer, int weapon)
						{
							if (equipPlayer.ClassType == ClassCategory.Magic)
								return false;
							else if ((equipPlayer.Class == 1 && 1 <= weapon && weapon <= 28) ||
								(equipPlayer.Class == 2 && 1 <= weapon && weapon <= 21) ||
								(equipPlayer.Class == 3 && 1 <= weapon && weapon <= 7) ||
								(equipPlayer.Class == 4 && 8 <= weapon && weapon <= 28) ||
								(equipPlayer.Class == 6 && ((1 <= weapon && weapon <= 7) || (15 <= weapon && weapon <= 28))) ||
								(equipPlayer.Class == 7 && ((1 <= weapon && weapon <= 7) || (15 <= weapon && weapon <= 28))))
								return true;
							else
								return false;
						}

						bool VerifyShield(Lore equipPlayer, int shield)
						{
							if (equipPlayer.ClassType != ClassCategory.Sword)
								return false;
							else if (equipPlayer.Class == 1 || equipPlayer.Class == 2 || equipPlayer.Class == 3 || equipPlayer.Class == 7)
								return true;
							else
								return false;
						}

						bool VerifyArmor(Lore equipPlayer, int armor)
						{
							if ((equipPlayer.ClassType == ClassCategory.Magic && armor == 1) ||
								(equipPlayer.ClassType == ClassCategory.Sword && ((1 <= armor && armor <= 10) || armor == 255)))
								return true;
							else
								return false;
						}

						var menuMode = HideMenu();

						if (menuMode == MenuMode.EnemySelectMode)
						{
							AddBattleCommand();
						}							
						else if (menuMode == MenuMode.Game)
						{
							if (mMenuFocusID == 0)
							{
								ShowPartyStatus();
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
								ShowCharacterMenu(MenuMode.ViewCharacter);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText("");
								DialogText.Visibility = Visibility.Collapsed;

								for (var i = 0; i < 6; i++)
								{
									if (i < mPlayerList.Count)
										mHealthTextList[i].Update(mPlayerList[i].Name, mPlayerList[i].Poison, mPlayerList[i].Unconscious, mPlayerList[i].Dead);
									else if (i == mPlayerList.Count)
										mHealthTextList[i].Update(mAssistPlayer.Name, mAssistPlayer.Poison, mAssistPlayer.Unconscious, mAssistPlayer.Dead);
									else
										mHealthTextList[i].Clear();
								}

								StatHealthPanel.Visibility = Visibility.Visible;
								ContinueText.Visibility = Visibility.Visible;
							}
							else if (mMenuFocusID == 3)
							{
								ShowCharacterMenu(MenuMode.CastSpell, false);
							}
							else if (mMenuFocusID == 4)
							{
								ShowCharacterMenu(MenuMode.Extrasense);
							}
							else if (mMenuFocusID == 5)
							{
								Rest();
							}
							else if (mMenuFocusID == 6)
							{
								AppendText("서로 바꿀 물품을 고르시오");

								ShowMenu(MenuMode.ExchangeItem, new string[] {
									"사용중인 무기",
									"사용중인 방패",
									"사용중인 갑옷"
								});
							}
							else if (mMenuFocusID == 7)
								ShowCharacterMenu(MenuMode.UseItemPlayer, false);
							else if (mMenuFocusID == 8)
							{
								AppendText(new string[] { "게임 선택 상황" });

								var gameOptions = new string[]
								{
														"난이도 조절",
														"정식 일행의 순서 정렬",
														"일행의 장비를 해제",
														"일행에서 제외 시킴",
														"이전의 게임을 재개",
														"현재의 게임을 저장",
														"게임을 마침",
								};

								ShowMenu(MenuMode.GameOptions, gameOptions);
							}
						}
						else if (menuMode == MenuMode.ViewCharacter)
						{
							DialogText.Visibility = Visibility.Collapsed;
							StatPanel.Visibility = Visibility.Visible;

							var player = mMenuFocusID < mPlayerList.Count ? mPlayerList[mMenuFocusID] : mAssistPlayer;

							StatPlayerName.Text = player.Name;
							StatPlayerGender.Text = player.GenderStr;
							StatPlayerClass.Text = player.ClassStr;

							StatStrength.Text = player.Strength.ToString();
							StatMentality.Text = player.Mentality.ToString();
							StatConcentration.Text = player.Concentration.ToString();
							StatEndurance.Text = player.Endurance.ToString();
							StatResistance.Text = player.Resistance.ToString();
							StatAgility.Text = player.Agility.ToString();
							StatAccuracy.Text = player.Accuracy.ToString();
							StatLuck.Text = player.Luck.ToString();

							if (player.ClassType == ClassCategory.Sword) {
								StatAbility1Title.Text = "베는 무기 기술치 :";
								StatAbility1Value.Text = player.SwordSkill.ToString();

								if (player.Class != 7)
									StatAbility2Title.Text = "찍는 무기 기술치 :";
								else
									StatAbility2Title.Text = "치료 마법 능력치 :";
								StatAbility2Value.Text = player.AxeSkill.ToString();

								StatAbility3Title.Text = "찌르는 무기 기술치 :";
								StatAbility3Value.Text = player.SpearSkill.ToString();

								StatAbility4Title.Text = "쏘는 무기 기술치 :";
								StatAbility4Value.Text = player.BowSkill.ToString();

								StatAbility5Title.Text = "방패 사용 기술치 :";
								StatAbility5Value.Text = player.ShieldSkill.ToString();

								StatAbility6Title.Text = "맨손 사용 기술치 :";
								StatAbility6Value.Text = player.FistSkill.ToString();
							}
							else if (player.ClassType == ClassCategory.Magic) {
								StatAbility1Title.Text = "공격 마법 능력치 :";
								StatAbility1Value.Text = player.AttackMagic.ToString();

								StatAbility2Title.Text = "변화 마법 능력치 :";
								StatAbility2Value.Text = player.PhenoMagic.ToString();

								StatAbility3Title.Text = "치료 마법 능력치 :";
								StatAbility3Value.Text = player.CureMagic.ToString();

								StatAbility4Title.Text = "특수 마법 능력치 :";
								StatAbility4Value.Text = player.SpecialMagic.ToString();

								StatAbility5Title.Text = "초 자연력 능력치 :";
								StatAbility5Value.Text = player.ESPMagic.ToString();

								StatAbility6Title.Text = "소환 마법 능력치 :";
								StatAbility6Value.Text = player.SummonMagic.ToString();
							}
		
							StatExp.Text = player.Experience.ToString("#,#0");
							StatLevel.Text = player.Level.ToString();

							StatWeapon.Text = Common.GetWeaponName(mPlayerList[mMenuFocusID].Weapon);
							StatShield.Text = Common.GetShieldName(mPlayerList[mMenuFocusID].Shield);
							StatArmor.Text = Common.GetArmorName(mPlayerList[mMenuFocusID].Armor);
						}
						else if (menuMode == MenuMode.CastSpell)
						{
							if (!mPlayerList[mMenuFocusID].IsAvailable)
								AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}는 마법을 사용할 수 있는 상태가 아닙니다");
							else if (mPlayerList[mMenuFocusID].ClassType == ClassCategory.Sword && mPlayerList[mMenuFocusID].Class != 7)
								AppendText($"{mPlayerList[mMenuFocusID].NameSubjectJosa} 마법을 사용할 수 없는 계열입니다.");
							else
							{
								mMagicPlayer = mPlayerList[mMenuFocusID];
								AppendText("사용할 마법의 종류 ===>");
								ShowMenu(MenuMode.SpellCategory, new string[]
								{
									"치료 마법",
									"변화 마법"
								});
							}
						}
						else if (menuMode == MenuMode.SpellCategory)
						{
							if (mMenuFocusID == 0)
							{
								ShowCureDestMenu(mMagicPlayer, MenuMode.ChooseCureSpell);
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.ClassType == ClassCategory.Sword)
									AppendText($"{mMagicPlayer.NameSubjectJosa} 변화 마법을 사용하는 계열이 아닙니다.");
								else
								{
									AppendText(new string[] { "선택" });

									int availableCount = mMagicPlayer.PhenoMagic / 10;
									if (availableCount > 10)
										availableCount = 10;

									var phenominaMagicMenu = new string[availableCount];
									for (var i = 0; i < availableCount; i++)
										phenominaMagicMenu[i] = Common.GetMagicName(2, i + 1);

									ShowMenu(MenuMode.ApplyPhenominaMagic, phenominaMagicMenu);
								}
							}
						}
						else if (menuMode == MenuMode.ChooseCureSpell || menuMode == MenuMode.ChooseBattleCureSpell)
						{
							AppendText(new string[] { "선택" });

							if (mMenuFocusID < mPlayerList.Count)
								mMagicWhomPlayer = mPlayerList[mMenuFocusID];
							else if (mMenuFocusID == mPlayerList.Count && mAssistPlayer != null)
								mMagicWhomPlayer = mAssistPlayer;
							else
							{
								int curePoint;
								if (mMagicPlayer.ClassType == ClassCategory.Magic)
									curePoint = mMagicPlayer.CureMagic / 10 - 5;
								else
									curePoint = mMagicPlayer.AxeSkill / 10 - 5;

								if (curePoint < 1)
								{
									Talk("강한 치료 마법은 아직 불가능 합니다.");

									if (menuMode == MenuMode.ChooseBattleCureSpell)
										mSpecialEvent = SpecialEventType.BackToBattleMode;
									else
										ContinueText.Visibility = Visibility.Visible;
									return;
								}
							}

							if (menuMode == MenuMode.ChooseCureSpell)
								ShowCureSpellMenu(mMagicPlayer, mMenuFocusID, MenuMode.ApplyCureMagic, MenuMode.ApplyCureAllMagic);
							else
								ShowCureSpellMenu(mPlayerList[mBattlePlayerID], mMenuFocusID, MenuMode.ApplyBattleCureSpell, MenuMode.ApplyBattleCureAllSpell);
						}
						else if (menuMode == MenuMode.ApplyCureMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

							ShowCureResult();
						}
						else if (menuMode == MenuMode.ApplyCureAllMagic)
						{
							mMenuMode = MenuMode.None;

							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

							ShowCureResult();
						}
						else if (menuMode == MenuMode.ApplyPhenominaMagic)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mMagicPlayer.SP < 1)
									ShowNotEnoughSP();
								else
								{
									if (mParty.Etc[0] + mMagicPlayer.PhenoMagic / 10 < 256)
										mParty.Etc[0] += mMagicPlayer.PhenoMagic / 10;
									else
										mParty.Etc[0] = 255;

									AppendText($"[color={RGB.White}]일행은 마법의 횃불을 밝혔습니다.[/color]");
									mMagicPlayer.SP--;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 1) {
								if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else {
									mMagicPlayer.SP -= 5;
									ShowWizardEye();
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[3] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 공중부상중 입니다.[/color]" });
									mMagicPlayer.SP -= 5;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mMagicPlayer.SP < 10)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[1] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 물 위를 걸을 수 있습니다.[/color]" });
									mMagicPlayer.SP -= 10;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 4)
							{
								if (mMagicPlayer.SP < 20)
									ShowNotEnoughSP();
								else
								{
									mParty.Etc[2] = 255;

									AppendText(new string[] { $"[color={RGB.White}]일행은 늪 위를 걸을 수 있습니다.[/color]" });
									mMagicPlayer.SP -= 20;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 5)
							{
								if (mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText($"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 기화 이동을 방해 합니다.[/color]");
								else if (mMagicPlayer.SP < 25)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" });

									ShowMenu(MenuMode.VaporizeMoveDirection, new string[] { "북쪽으로 기화 이동",
														"남쪽으로 기화 이동",
														"동쪽으로 기화 이동",
														"서쪽으로 기화 이동" });
								}
							}
							else if (mMenuFocusID == 6)
							{
								if (mParty.Map == 4 || mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText(new string[] { $"[color={RGB.LightMagenta}]이 지역의 악의 힘이 지형 변화를 방해 합니다.[/color]" });
								else if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TransformDirection, new string[] { "북쪽에 지형 변화",
														"남쪽에 지형 변화",
														"동쪽에 지형 변화",
														"서쪽에 지형 변화" });
								}
							}
							else if (mMenuFocusID == 7)
							{
								if (mMagicPlayer.SP < 50)
									ShowNotEnoughSP();
								else
									Teleport(MenuMode.TeleportationDirection);
							}
							else if (mMenuFocusID == 8)
							{
								if (mMagicPlayer.SP < 30)
									ShowNotEnoughSP();
								else
								{
									var count = mPlayerList.Count;
									if (mParty.Food + count > 255)
										mParty.Food = 255;
									else
										mParty.Food = mParty.Food + count;
									mMagicPlayer.SP -= 30;
									DisplaySP();

									AppendText(new string[] { $"[color={RGB.White}]식량 제조 마법은 성공적으로 수행되었습니다[/color]",
										$"[color={RGB.White}]            {count} 개의 식량이 증가됨[/color]",
										$"[color={RGB.LightCyan}]      일행의 현재 식량은 {mParty.Food} 개 입니다[/color]" });
								}
							}
							else if (mMenuFocusID == 9) {
								if (mParty.Map == 4 || mParty.Map == 15 || mParty.Map == 16 || mParty.Map == 17)
									AppendText(new string[] { $"[color={RGB.LightMagenta}]이 지역의 악의 힘이 지형 변화를 방해 합니다.[/color]" });
								else if (mMagicPlayer.SP < 60)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.BigTransformDirection, new string[] { "북쪽에 지형 변화",
														"남쪽에 지형 변화",
														"동쪽에 지형 변화",
														"서쪽에 지형 변화" });
								}
							}

							//DisplaySP();
						}
						else if (menuMode == MenuMode.VaporizeMoveDirection)
						{
							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + 2 * xOffset;
							var newY = mParty.YAxis + 2 * yOffset;

							if (newX < 4 || newX >= mMapWidth - 4 || newY < 5 || newY >= mMapHeight - 4)
								return;

							var canMove = false;

							var moveTile = mMapLayer[newX + mMapWidth * newY];
							switch (mPosition)
							{
								case PositionType.Town:
									if (moveTile == 0 || (27 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Ground:
									if (moveTile == 0 || (24 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Den:
									if (moveTile == 0 || (41 <= moveTile && moveTile <= 47))
										canMove = true;
									break;
								case PositionType.Keep:
									if (moveTile == 0 || (40 <= moveTile && moveTile <= 47))
										canMove = true;
									break;

							}

							if (!canMove)
								AppendText($"기화 이동이 통하지 않습니다.");
							else
							{
								mMagicPlayer.SP -= 25;
								DisplaySP();

								if (GetTileInfo(newX, newY) == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && (GetTileInfo(newX, newY) == 52)))
								{
									AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
								}
								else
								{
									mParty.XAxis = newX;
									mParty.YAxis = newY;

									AppendText($"[color={RGB.White}]기화 이동을 마쳤습니다.[/color]");
								}

							}
						}
						else if (menuMode == MenuMode.TransformDirection)
						{

							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + xOffset;
							var newY = mParty.YAxis + yOffset;


							mMagicPlayer.SP -= 30;
							DisplaySP();

							if (GetTileInfo(newX, newY) == 0 ||
									((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(newX, newY) == 52) ||
									(mPosition == PositionType.Town && GetTileInfo(newX, newY) == 48))
								AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
							else
							{
								byte tile;

								switch (mPosition)
								{
									case PositionType.Town:
										tile = 47;
										break;
									case PositionType.Ground:
										tile = 41;
										break;
									case PositionType.Den:
										tile = 43;
										break;
									default:
										tile = 43;
										break;
								}

								UpdateTileInfo(newX, newY, tile);

								AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
							}
						}
						else if (menuMode == MenuMode.TransformDirection)
						{

							mMenuMode = MenuMode.None;

							int xOffset = 0, yOffset = 0;
							switch (mMenuFocusID)
							{
								case 0:
									yOffset = -1;
									break;
								case 1:
									yOffset = 1;
									break;
								case 2:
									xOffset = 1;
									break;
								case 3:
									xOffset = -1;
									break;
							}

							var newX = mParty.XAxis + xOffset;
							var newY = mParty.YAxis + yOffset;

							var range = xOffset == 0 ? 5 : 4;

							mMagicPlayer.SP -= 60;
							DisplaySP();

							byte tile;

							switch (mPosition)
							{
								case PositionType.Town:
									tile = 47;
									break;
								case PositionType.Ground:
									tile = 41;
									break;
								case PositionType.Den:
									tile = 43;
									break;
								default:
									tile = 43;
									break;
							}

							for (var i = 1; i <= range; i++)
							{
								if (GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 0 ||
										((mPosition == PositionType.Den || mPosition == PositionType.Keep) && GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 52) ||
										(mPosition == PositionType.Town && GetTileInfo(mParty.XAxis + xOffset * i, mParty.YAxis + yOffset * i) == 48))
								{
									AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]");
									return;
								}
								else
								{
									UpdateTileInfo(newX, newY, tile);
								}
							}

							AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
						}
						else if (menuMode == MenuMode.TeleportationDirection)
						{
							mTeleportationDirection = mMenuFocusID;

							var rangeItems = new List<Tuple<string, int>>();
							for (var i = 1; i <= 9; i++)
							{
								rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i * 1000}[/color] [color={RGB.White}] 공간 이동력[/color]", i));
							}

							ShowSpinner(SpinnerType.TeleportationRange, rangeItems.ToArray(), 5);
						}
						else if (menuMode == MenuMode.Extrasense)
						{
							if (mPlayerList[mMenuFocusID].IsAvailable)
							{
								if (mPlayerList[mMenuFocusID].ClassType == ClassCategory.Sword)
									AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}에게는 초감각의 능력이 없습니다.");
								else
								{
									mMagicPlayer = mPlayerList[mMenuFocusID];

									AppendText(new string[] { "사용할 초감각의 종류 ===>" });

									var extrsenseMenu = new string[4];
									for (var i = 0; i < 4; i++)
										extrsenseMenu[i] = Common.GetMagicName(5, i + 1);

									ShowMenu(MenuMode.ChooseExtrasense, extrsenseMenu);
								}
							}
							else
								AppendText($"{mPlayerList[mMenuFocusID].GenderPronoun}는 초감각을 사용할 수 있는 상태가 아닙니다");
						}
						else if (menuMode == MenuMode.ChooseExtrasense)
						{
							HideMenu();
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mMagicPlayer.ESPMagic < 70)
									AppendText($"{mMagicPlayer.GenderPronoun}는 투시를 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 10)
									ShowNotEnoughSP();
								else {
									Talk($"[color={RGB.White}]일행은 주위를 투시하고 있다.[/color]");
									mMagicPlayer.SP -= 10;
									DisplaySP();

									mSpecialEvent = SpecialEventType.Penetration;
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mMagicPlayer.ESPMagic < 10)
									AppendText($"{mMagicPlayer.GenderPronoun}는 예언을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 5)
									ShowNotEnoughSP();
								else
								{
									var predictStr = new string[]
									{
										"로드 안을 만날",
										"메너스로 갈",
										"다시 로드 안에게 갈",
										"다시 메너스를 조사할",
										"다시 로어 성으로 돌아갈",
										"라스트 디치의 성주를 만날",
										"피라미드 속의 로어 헌터를 찾을",
										"라스트 디치의 군주에게로 돌아갈",
										"피라미드 속의 두 동굴에서 두개의 석판을 찾을",
										"첫번째 월식을 기다릴",
										"메너스를 통해 지하 세계로 갈",
										"빛의 지붕을 방문할 것",
										"빛의 사원에서 악의 추종자들의 메세지를 볼",
										"필멸의 생존을 탐험할 것",
										"지하 세계 중앙의 통로를 통해 지상으로 갈",
										"두번째 월식을 기다릴",
										"베리알의 동굴에서 결전을 벌일",
										"몰록의 동굴에서 결전을 벌일",
										"마지막 세번째 월식을 기다릴",
										"아스모데우스의 동굴에서 결전을 벌일",
										"메피스토펠레스와 대결을 벌일"
									};

									int predict = -1;
									switch (mParty.Etc[9]) {
										case 0:
											predict = 0;
											break;
										case 1:
											predict = 1;
											break;
										case 2:
											predict = 2;
											break;
										case 3:
											predict = 3;
											break;
										case 4:
											predict = 4;
											break;
										case 5:
											switch (mParty.Etc[10]) {
												case 0:
													predict = 5;
													break;
												case 1:
													predict = 6;
													break;
												case 2:
													predict = 7;
													break;
												case 3:
													predict = 8;
													break;
												case 4:
													predict = 7;
													break;
												case 5:
													predict = 2;
													break;
											}
											break;
										case 6:
											predict = 2;
											break;
										case 7:
											predict = 0;
											break;
										case 8:
											predict = 9;
											break;
										case 9:
											if ((mParty.Etc[8] & (1 << 3)) == 0)
												predict = 13;
											else if ((mParty.Etc[8] & (1 << 2)) == 0)
												predict = 12;
											else if ((mParty.Etc[8] & (1 << 1)) == 0)
												predict = 11;
											else if ((mParty.Etc[8] & 1) == 0)
												predict = 10;
											else
												predict = 14;
											break;
										case 10:
											predict = 2;
											break;
										case 11:
											predict = 15;
											break;
										case 12:
											if ((mParty.Etc[8] & (1 << 6)) == 0)
												predict = 17;
											else if ((mParty.Etc[8] & (1 << 5)) == 0)
												predict = 16;
											else if ((mParty.Etc[8] & (1 << 4)) == 0)
												predict = 10;
											break;
										case 13:
											predict = 2;
											break;
										case 14:
											predict = 18;
											break;
										case 15:
											if ((mParty.Etc[8] & (1 << 2)) == 0)
												predict = 20;
											else if ((mParty.Etc[8] & (1 << 1)) == 0)
												predict = 19;
											else if ((mParty.Etc[8] & 1) == 0)
												predict = 10;
											break;
										case 16:
											predict = 21;
											break;
									}

									AppendText(new string[] { $" 당신은 당신의 미래를 예언한다 ...", "" });
									if (0 < predict && predict >= predictStr.Length)
										AppendText(new string[] { $"[color={RGB.LightGreen}] #[/color] [color={RGB.White}]당신은 어떤 힘에 의해 예언을 방해 받고 있다[/color]" }, true);
									else
										AppendText(new string[] { $"[color={RGB.LightGreen}] #[/color] [color={RGB.White}]당신은 {predictStr[predict]} 것이다[/color]" }, true);
									
									mMagicPlayer.SP -= 5;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mMagicPlayer.ESPMagic < 40)
									AppendText($"{mMagicPlayer.GenderPronoun}는 독심을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < 20)
									ShowNotEnoughSP();
								else
								{
									AppendText($"[color={RGB.White}]당신은 잠시 동안 다른 사람의 마음을 읽을 수 있다.[/color]");
									mParty.Etc[4] = 3;

									mMagicPlayer.SP -= 20;
									DisplaySP();
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mMagicPlayer.ESPMagic < 55)
									AppendText($"{mMagicPlayer.GenderPronoun}는 천리안을 시도해 보았지만 아직은 역부족이었다.");
								else if (mMagicPlayer.SP < mMagicPlayer.Level * 5)
									ShowNotEnoughSP();
								else
								{
									AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

									ShowMenu(MenuMode.TelescopeDirection, new string[] { "북쪽으로 천리안을 사용",
														"남쪽으로 천리안을 사용",
														"동쪽으로 천리안을 사용",
														"서쪽으로 천리안을 사용" });
								}
							}
						}
						else if (menuMode == MenuMode.TelescopeDirection)
						{
							mMagicPlayer.SP -= mMagicPlayer.Level * 5;
							DisplaySP();

							mTelescopePeriod = mMagicPlayer.Level;
							switch (mMenuFocusID)
							{
								case 0:
									mTelescopeYCount = -mMagicPlayer.Level;
									break;
								case 1:
									mTelescopeYCount = mMagicPlayer.Level;
									break;
								case 2:
									mTelescopeXCount = mMagicPlayer.Level;
									break;
								case 3:
									mTelescopeXCount = -mMagicPlayer.Level;
									break;
							}

							Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
							mSpecialEvent = SpecialEventType.Telescope;
						}
						else if (menuMode == MenuMode.ExchangeItem) {
							mExchangeCategory = mMenuFocusID;

							switch (mExchangeCategory) {
								case 0:
									AppendText("무기를 바꿀 일원");
									break;
								case 1:
									AppendText("방패를 바꿀 일원");
									break;
								case 2:
									AppendText("갑옷을 바꿀 일원");
									break;
							}
							
							ShowCharacterMenu(MenuMode.ExchangeItemWhom, false);
						}
						else if (menuMode == MenuMode.ExchangeItemWhom) {
							mExchangePlayer = mPlayerList[mMenuFocusID];

							switch (mExchangeCategory)
							{
								case 0:
									AppendText("무기를 바꿀 대상 일원");
									break;
								case 1:
									AppendText("방패를 바꿀 대상 일원");
									break;
								case 2:
									AppendText("갑옷을 바꿀 대상 일원");
									break;
							}

							ShowCharacterMenu(MenuMode.SwapItem, false);
						}
						else if (menuMode == MenuMode.SwapItem) {
							var whomPlayer = mPlayerList[mMenuFocusID];

							switch (mExchangeCategory) {
								case 0:
									{
										var temp = mExchangePlayer.Weapon;
										mExchangePlayer.Weapon = whomPlayer.Weapon;
										whomPlayer.Weapon = temp;
										break;
									}
								case 1:
									{
										var temp = mExchangePlayer.Shield;
										mExchangePlayer.Shield = whomPlayer.Shield;
										whomPlayer.Shield = temp;
										break;
									}
								case 2:
									{
										var temp = mExchangePlayer.Armor;
										mExchangePlayer.Armor = whomPlayer.Armor;
										whomPlayer.Armor = temp;
										break;
									}
							}

							UpdateItem(mExchangePlayer);
							UpdateItem(whomPlayer);

							UpdatePlayersStat();
							AppendText("");
						}
						else if (menuMode == MenuMode.UseItemPlayer) {
							if (mPlayerList[mMenuFocusID].IsAvailable)
								UseItem(mPlayerList[mMenuFocusID], false);
							else
								AppendText($" {mPlayerList[mMenuFocusID].NameSubjectJosa} 물품을 사용할 수 있는 상태가 아닙니다.");
						}	
						else if (menuMode == MenuMode.GameOptions)
						{
							if (mMenuFocusID == 0)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]" });

								var maxEnemyStr = new string[5];
								for (var i = 0; i < maxEnemyStr.Length; i++)
									maxEnemyStr[i] = $"{i + 3}명의 적들";

								ShowMenu(MenuMode.SetMaxEnemy, maxEnemyStr);
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]현재의 일원의 전투 순서를 정렬 하십시오.[/color]",
												"[color=e0ffff]순서를 바꿀 일원[/color]" });

								ShowCharacterMenu(MenuMode.OrderFromCharacter, false);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText(new string[] { $"[color={RGB.LightRed}]장비를 제거할 사람을 고르시오.[/color]" });

								ShowCharacterMenu(MenuMode.UnequipCharacter, false);
							}
							else if (mMenuFocusID == 3)
							{
								AppendText($"[color={RGB.LightRed}]일행에서 제외 시키고 싶은 사람을 고르십시오.[/color]");

								ShowCharacterMenu(MenuMode.DelistCharacter);
							}
							else if (mMenuFocusID == 4)
							{
								ShowFileMenu(MenuMode.ChooseLoadGame);
							}
							else if (mMenuFocusID == 5)
							{
								ShowFileMenu(MenuMode.ChooseSaveGame);
							}
							else if (mMenuFocusID == 6)
							{
								AppendText(new string[] { $"[color={RGB.LightGreen}]정말로 끝내겠습니까 ?[/color]" });

								ShowMenu(MenuMode.ConfirmExit, new string[] {
									"<< 아니오 >>",
									"<<   예   >>"
								});
							}
						}
						else if (menuMode == MenuMode.SetMaxEnemy)
						{
							mMaxEnemy = mMenuFocusID + 3;

							AppendText($"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]");

							ShowMenu(MenuMode.SetEncounterType, new string[]
							{
								"일부러 전투를 피하고 싶다",
								"너무 잦은 전투는 원하지 않는다",
								"마주친 적과는 전투를 하겠다",
								"보이는 적들과는 모두 전투하겠다",
								"그들은 피에 굶주려 있다"
							});
						}
						else if (menuMode == MenuMode.SetEncounterType)
						{
							mEncounter = 6 - (mMenuFocusID + 1);

							AppendText($"[color={RGB.LightRed}]의식 불명인 적까지 공격 하겠습니까?[/color]");

							ShowMenu(MenuMode.AttackCruelEnemy, new string[]
							{
								"물론 그렇다",
								"그렇지 않다"
							});
						}
						else if (menuMode == MenuMode.AttackCruelEnemy) {
							AppendText("");

							if (mMenuFocusID == 0)
								mParty.Cruel = true;
							else
								mParty.Cruel = false;
						}
						else if (menuMode == MenuMode.OrderFromCharacter)
						{
							mMenuMode = MenuMode.None;

							mOrderFromPlayerID = mMenuFocusID;

							AppendText($"[color={RGB.LightCyan}]순서를 바꿀 대상 일원[/color]");

							ShowCharacterMenu(MenuMode.OrderToCharacter, false);
						}
						else if (menuMode == MenuMode.OrderToCharacter)
						{
							var tempPlayer = mPlayerList[mOrderFromPlayerID];
							mPlayerList[mOrderFromPlayerID] = mPlayerList[mMenuFocusID];
							mPlayerList[mMenuFocusID] = tempPlayer;

							DisplayPlayerInfo();

							AppendText("");
						}
						else if (menuMode == MenuMode.UnequipCharacter) {
							if (mUnequipPlayer.Weapon == 0 && mUnequipPlayer.Shield == 0 && mUnequipPlayer.Armor == 0)
								AppendText($"해제할 장비가 없습니다.");
							else
							{
								mUnequipPlayer = mPlayerList[mMenuFocusID];

								AppendText($"[color={RGB.LightRed}]제거할 장비를 고르시오.[/color]");

								var menuList = new List<string>();
								if (mUnequipPlayer.Weapon != 0)
									menuList.Add(Common.GetWeaponName(mUnequipPlayer.Weapon));

								if (mUnequipPlayer.Shield != 0)
									menuList.Add(Common.GetShieldName(mUnequipPlayer.Shield));

								if (mUnequipPlayer.Armor != 0)
									menuList.Add(Common.GetArmorName(mUnequipPlayer.Armor));

								ShowMenu(MenuMode.Unequip, menuList.ToArray());
							}
						}
						else if (menuMode == MenuMode.Unequip) {
							var weaponType = 0;

							if (mMenuFocusID == 0) {
								if (mUnequipPlayer.Weapon != 0)
									weaponType = 0;
								else if (mUnequipPlayer.Shield != 0)
									weaponType = 1;
								else
									weaponType = 2;
							}
							else if (mMenuFocusID == 1) {
								if (mUnequipPlayer.Shield != 0)
									weaponType = 1;
								else
									weaponType = 2;
							}
							else if (mMenuFocusID == 2)
								weaponType = 2;

							switch (weaponType) {
								case 0:
									mUnequipPlayer.Weapon = 0;
									AppendText($"{mUnequipPlayer.Name}의 무기는 해제 되었습니다.");
									break;
								case 1:
									mUnequipPlayer.Shield = 0;
									AppendText($"{mUnequipPlayer.Name}의 방패는 해제 되었습니다.");
									break;
								case 2:
									mUnequipPlayer.Armor = 0;
									AppendText($"{mUnequipPlayer.Name}의 갑옷은 해제 되었습니다.");
									break;
							}

							UpdateItem(mUnequipPlayer);
						}
						else if (menuMode == MenuMode.DelistCharacter)
						{
							mPlayerList.RemoveAt(mMenuFocusID);

							DisplayPlayerInfo();
							AppendText("");
						}
						else if (menuMode == MenuMode.ConfirmExit)
						{
							if (mMenuFocusID == 0)
								AppendText("");
							else
								CoreApplication.Exit();
						}
						else if (menuMode == MenuMode.ChooseWeaponType) {
							ShowWeaponTypeMenu(mMenuFocusID);
						}
						else if (menuMode == MenuMode.BuyWeapon)
						{
							var price = weaponPrice[mWeaponTypeID, mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetWeaponNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseWeaponCharacter);
							}
						}
						else if (menuMode == MenuMode.UseWeaponCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyWeapon(player, (mWeaponTypeID - 1) * 7 + mBuyWeaponID)) {
								player.Weapon = (mWeaponTypeID - 1) * 7 + mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= weaponPrice[mWeaponTypeID, mBuyWeaponID - 1];
							}
							else {
								Talk($" 이 무기는 {player.Name}에게는 맞지 않습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.BuyShield)
						{
							var price = shieldPrice[mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetShieldNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseShieldCharacter);
							}
						}
						else if (menuMode == MenuMode.UseShieldCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyShield(player, mBuyWeaponID))
							{
								player.Shield = mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= shieldPrice[mBuyWeaponID - 1];
							}
							else
							{
								Talk($" {player.NameSubjectJosa} 이 방패를 사용 할 수 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.BuyArmor)
						{
							var price = armorPrice[mMenuFocusID];

							if (mParty.Gold < price)
								ShowNotEnoughMoney(SpecialEventType.CantBuyWeapon);
							else
							{
								mBuyWeaponID = mMenuFocusID + 1;

								AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetArmorNameJosa(mBuyWeaponID)} 사용하시겠습니까?[/color]" });

								ShowCharacterMenu(MenuMode.UseArmorCharacter);
							}
						}
						else if (menuMode == MenuMode.UseArmorCharacter)
						{
							var player = mPlayerList[mMenuFocusID];

							if (VerifyArmor(player, mBuyWeaponID))
							{
								player.Armor = mBuyWeaponID;
								UpdateItem(player);

								mParty.Gold -= armorPrice[mBuyWeaponID - 1];
							}
							else
							{
								Talk($" {player.NameSubjectJosa} 이 갑옷을 사용 할 수 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyWeapon;
							}
						}
						else if (menuMode == MenuMode.ChooseFoodAmount) {
							if (mParty.Gold < (mMenuFocusID + 1) * 100)
								ShowNotEnoughMoney(SpecialEventType.None);
							else {
								mParty.Gold -= (mMenuFocusID + 1) * 100;
								var food = (mMenuFocusID + 1) * 10;
								if (mParty.Food + food > 255)
									mParty.Food = 255;
								else
									mParty.Food += food;

								AppendText("매우 고맙습니다.");
							}
						}
						else if (menuMode == MenuMode.BuyExp) {
							if (mParty.Gold < (mMenuFocusID + 1) * 10_000)
							{
								Talk(" 일행은 충분한 금이 없었다.");
								mSpecialEvent = SpecialEventType.CantBuyExp;
							}
							else {
								AppendText($"[color={RGB.White}]  일행은 ${mMenuFocusID + 1}시간동안 훈련을 받게 되었다.");
								mTrainTime = mMenuFocusID + 1;
								InvokeAnimation(AnimationType.BuyExp);
							}						
						}
						else if (menuMode == MenuMode.SelectItem) {
							AppendText($"[color={RGB.White}] 갯수를 지정 하십시오.[/color]");

							mBuyItemID = mMenuFocusID;

							var itemCountArr = new string[10];
							for (var i = 0; i < itemCountArr.Length; i++) {
								if (mBuyItemID == 0)
									itemCountArr[i] = $"{mItems[i]} {(i + 1) * 10}개 : 금 {mItemPrices[i].ToString("#,#0")}개";
								else
									itemCountArr[i] = $"{mItems[i]} {i + 1}개 : 금 {mItemPrices[i].ToString("#,#0")}개";
							}

							ShowMenu(MenuMode.SelectItemAmount, itemCountArr);
						}
						else if (menuMode == MenuMode.SelectItemAmount) {
							if (mParty.Gold < mItemPrices[mBuyItemID] * (mMenuFocusID + 1))
							{
								Talk(" 당신에게는 이 것을 살 돈이 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyItem;
							}

							mParty.Gold -= mItemPrices[mBuyItemID] * (mMenuFocusID + 1);
							if (mBuyItemID == 0)
							{
								if (mParty.Arrow + (mMenuFocusID + 1) * 10 < 32_768)
									mParty.Arrow += (mMenuFocusID + 1) * 10;
								else
									mParty.Arrow = 32_767;
							}
							else {
								if (mParty.Item[mBuyItemID + 4] + mMenuFocusID + 1 < 256)
									mParty.Item[mBuyItemID + 4] += mMenuFocusID + 1;
								else
									mParty.Item[mBuyItemID + 4] = 255;
							}

							ShowItemStoreMenu();
						}
						else if (menuMode == MenuMode.SelectMedicine)
						{
							AppendText($"[color={RGB.White}] 갯수를 지정 하십시오.[/color]");

							mBuyMedicineID = mMenuFocusID;

							var itemCountArr = new string[10];
							for (var i = 0; i < itemCountArr.Length; i++)
							{
								itemCountArr[i] = $"{mMedicines[i]} {i + 1}개 : 금 {mMedicinePrices[i].ToString("#,#0")}개";
							}

							ShowMenu(MenuMode.SelectMedicineAmount, itemCountArr);
						}
						else if (menuMode == MenuMode.SelectMedicineAmount)
						{
							if (mParty.Gold < mMedicinePrices[mBuyMedicineID] * (mMenuFocusID + 1))
							{
								Talk(" 당신에게는 이 것을 살 돈이 없습니다.");
								mSpecialEvent = SpecialEventType.CantBuyMedicine;
							}

							mParty.Gold -= mMedicinePrices[mBuyMedicineID] * (mMenuFocusID + 1);
							
							if (mParty.Item[mBuyMedicineID] + mMenuFocusID + 1 < 256)
								mParty.Item[mBuyMedicineID] += mMenuFocusID + 1;
							else
								mParty.Item[mBuyMedicineID] = 255;
						
							ShowItemStoreMenu();
						}
						else if (menuMode == MenuMode.Hospital)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == mPlayerList.Count)
								mCurePlayer = mAssistPlayer;
							else
								mCurePlayer = mPlayerList[mMenuFocusID];

							ShowHealType();
						}
						else if (menuMode == MenuMode.HealType)
						{
							if (mMenuFocusID == 0)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 의식불명입니다");
								else if (mCurePlayer.Poison > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 독이 퍼진 상태입니다");
								else if (mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level * 10)
									AppendText($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 치료가 필요하지 않습니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison > 0 || mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level * 10)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Endurance * mCurePlayer.Level * 10 - mCurePlayer.HP;
									payment = payment * mCurePlayer.Level * 10 / 2 + 1;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(mSpecialEvent = SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.HP = mCurePlayer.Endurance * mCurePlayer.Level * 10;

										DisplayHP();

										Talk($"[color={RGB.White}]{mCurePlayer.Name}의 모든 건강이 회복되었다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 1)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 의식불명입니다");
								else if (mCurePlayer.Poison == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 독에 걸리지 않았습니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Level * 10;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Poison = 0;

										DisplayCondition();

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 독이 제거 되었습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 2)
							{
								if (mCurePlayer.Dead > 0)
									AppendText($"{mCurePlayer.NameSubjectJosa} 이미 죽은 상태입니다");
								else if (mCurePlayer.Unconscious == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.Name} 의식불명이 아닙니다[/color]");

								if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Unconscious * 2;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Unconscious = 0;
										mCurePlayer.HP = 1;

										DisplayCondition();
										DisplayHP();

										Talk($"[color={RGB.White}]{mCurePlayer.NameSubjectJosa} 의식을 차렸습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
							else if (mMenuFocusID == 3)
							{
								if (mCurePlayer.Dead == 0)
									AppendText($"[color={RGB.White}]{mCurePlayer.Name}(은)는 죽지 않았습니다[/color]");

								if (mCurePlayer.Dead == 0)
								{
									ContinueText.Visibility = Visibility;
									mSpecialEvent = SpecialEventType.CureComplete;
								}
								else
								{
									var payment = mCurePlayer.Dead * 100 + 400;

									if (mParty.Gold < payment)
										ShowNotEnoughMoney(SpecialEventType.NotCured);
									else
									{
										mParty.Gold -= payment;
										mCurePlayer.Dead = 0;

										if (mCurePlayer.Unconscious > mCurePlayer.Endurance * mCurePlayer.Level)
											mCurePlayer.Unconscious = mCurePlayer.Endurance * mCurePlayer.Level;

										DisplayCondition();

										Talk($"[color={RGB.White}]{mCurePlayer.Name}(은)는 다시 살아났습니다[/color]");
										mSpecialEvent = SpecialEventType.CureComplete;
									}
								}
							}
						}
						else if (menuMode == MenuMode.TrainingCenter)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
								ShowTrainMessage();
							else if (mMenuFocusID == 1)
								ShowTrainMagicMessage();
							else if (mMenuFocusID == 2)
								ShowChangeJobForSwordMessage();
							else if (mMenuFocusID == 3)
								ShowChangeJobForMagicMessage();
						}
						else if (menuMode == MenuMode.ChooseTrainSkillMember)
						{
							mMenuMode = MenuMode.None;

							mTrainPlayer = mPlayerList[mMenuFocusID];

							if (mTrainPlayer.ClassType != ClassCategory.Sword)
							{
								AppendText(" 당신은 전투사 계열이 아닙니다.");
								return;
							}

							var readyToLevelUp = true;
							for (var i = 0; i < 6; i++)
							{
								int skill;
								switch (i)
								{
									case 0:
										skill = mTrainPlayer.SwordSkill;
										break;
									case 1:
										skill = mTrainPlayer.AxeSkill;
										break;
									case 2:
										skill = mTrainPlayer.SpearSkill;
										break;
									case 3:
										skill = mTrainPlayer.BowSkill;
										break;
									case 4:
										skill = mTrainPlayer.ShieldSkill;
										break;
									default:
										skill = mTrainPlayer.FistSkill;
										break;

								}

								if (swordEnableClass[mTrainPlayer.Class - 1, i] > 0)
								{
									if (skill < swordEnableClass[mTrainPlayer.Class - 1, i])
									{
										readyToLevelUp = false;
										break;
									}
								}
							}

							if (readyToLevelUp)
							{
								AppendText(" 당신은 모든 과정을 수료했으므로 모든 경험치를 레벨로 바꾸겠습니다.");

								mTrainPlayer.PotentialExperience += mTrainPlayer.Experience;
								mTrainPlayer.Experience = 0;

								return;
							}

							ShowTrainSkillMenu(0);
						}
						else if (menuMode == MenuMode.ChooseTrainSkill)
						{
							mMenuMode = MenuMode.None;

							int skill;
							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									skill = mTrainPlayer.SwordSkill;
									break;
								case 1:
									skill = mTrainPlayer.AxeSkill;
									break;
								case 2:
									skill = mTrainPlayer.SpearSkill;
									break;
								case 3:
									skill = mTrainPlayer.BowSkill;
									break;
								case 4:
									skill = mTrainPlayer.ShieldSkill;
									break;
								default:
									skill = mTrainPlayer.FistSkill;
									break;
							}

							if (skill >= mTrainSkillList[mMenuFocusID].Item2)
							{
								Talk("이 분야는 더 배울 것이 없습니다");
								mSpecialEvent = SpecialEventType.CantTrain;

								return;
							}

							var needExp = 15 * skill * skill;

							if (needExp > mTrainPlayer.Experience)
							{
								Talk("아직 경험치가 모자랍니다");
								mSpecialEvent = SpecialEventType.CantTrain;

								return;
							}

							mTrainPlayer.Experience -= needExp;
							mTrainPlayer.PotentialExperience += needExp;

							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									mTrainPlayer.SwordSkill++;
									break;
								case 1:
									mTrainPlayer.AxeSkill++;
									break;
								case 2:
									mTrainPlayer.SpearSkill++;
									break;
								case 3:
									mTrainPlayer.BowSkill++;
									break;
								case 4:
									mTrainPlayer.ShieldSkill++;
									break;
								default:
									mTrainPlayer.FistSkill++;
									break;
							}

							ShowTrainSkillMenu(mMenuFocusID);
						}
						else if (menuMode == MenuMode.ChooseTrainMagicMember)
						{
							mMenuMode = MenuMode.None;

							mTrainPlayer = mPlayerList[mMenuFocusID];

							if (mTrainPlayer.ClassType != ClassCategory.Magic)
							{
								AppendText(" 당신은 마법사 계열이 아닙니다.");
								return;
							}

							var readyToLevelUp = true;
							for (var i = 0; i < 6; i++)
							{
								int skill;
								switch (i)
								{
									case 0:
										skill = mTrainPlayer.AttackMagic;
										break;
									case 1:
										skill = mTrainPlayer.PhenoMagic;
										break;
									case 2:
										skill = mTrainPlayer.CureMagic;
										break;
									case 3:
										skill = mTrainPlayer.SpecialMagic;
										break;
									case 4:
										skill = mTrainPlayer.ESPMagic;
										break;
									default:
										skill = mTrainPlayer.SummonMagic;
										break;

								}

								if (magicEnableClass[mTrainPlayer.Class - 1, i] > 0)
								{
									if (skill < magicEnableClass[mTrainPlayer.Class - 1, i])
									{
										readyToLevelUp = false;
										break;
									}
								}
							}

							if (readyToLevelUp)
							{
								AppendText(" 당신은 모든 과정을 수료했으므로 모든 경험치를 레벨로 바꾸겠습니다.");

								mTrainPlayer.PotentialExperience += mTrainPlayer.Experience;
								mTrainPlayer.Experience = 0;

								return;
							}

							ShowTrainMagicMenu(0);
						}
						else if (menuMode == MenuMode.ChooseTrainMagic)
						{
							int skill;
							switch (mTrainSkillList[mMenuFocusID].Item1)
							{
								case 0:
									skill = mTrainPlayer.AttackMagic;
									break;
								case 1:
									skill = mTrainPlayer.PhenoMagic;
									break;
								case 2:
									skill = mTrainPlayer.CureMagic;
									break;
								case 3:
									skill = mTrainPlayer.SpecialMagic;
									break;
								case 4:
									skill = mTrainPlayer.ESPMagic;
									break;
								default:
									skill = mTrainPlayer.SummonMagic;
									break;
							}

							if (skill >= mTrainSkillList[mMenuFocusID].Item2)
							{
								Talk("이 분야는 더 배울 것이 없습니다");
								mSpecialEvent = SpecialEventType.CantTrainMagic;
							}
							else
							{

								var needExp = 15 * skill * skill;

								if (needExp > mTrainPlayer.Experience)
								{
									Talk("아직 경험치가 모자랍니다");
									mSpecialEvent = SpecialEventType.CantTrainMagic;
								}
								else
								{

									mTrainPlayer.Experience -= needExp;
									mTrainPlayer.PotentialExperience += needExp;

									switch (mTrainSkillList[mMenuFocusID].Item1)
									{
										case 0:
											mTrainPlayer.AttackMagic++;
											break;
										case 1:
											mTrainPlayer.PhenoMagic++;
											break;
										case 2:
											mTrainPlayer.CureMagic++;
											break;
										case 3:
											mTrainPlayer.SpecialMagic++;
											break;
										case 4:
											mTrainPlayer.ESPMagic++;
											break;
										default:
											mTrainPlayer.SummonMagic++;
											break;
									}

									ShowTrainMagicMenu(mMenuFocusID);
								}
							}
						}
						else if (menuMode == MenuMode.ConfirmExitMap)
						{
							if (mMenuFocusID == 0)
							{
								if (mParty.Map == 2) {
									mParty.Map = 1;
									mParty.XAxis = 82;
									mParty.YAxis = 85;

									await RefreshGame();
								}
								if (mParty.Map == 6)
								{
									mParty.Map = 1;
									mParty.XAxis = 19;
									mParty.YAxis = 11;

									await RefreshGame();
								}
								else if (mParty.Map == 7) {
									mParty.Map = 1;
									mParty.XAxis = 76;
									mParty.YAxis = 56;

									await RefreshGame();
								}
								else if (mParty.Map == 10) {
									mParty.Map = 1;
									mParty.XAxis = 17;
									mParty.YAxis = 88;

									await RefreshGame();
								}
							}
							else
							{
								AppendText("");
								if (mParty.Map != 2)
									mParty.YAxis--;
							}
						}
						else if (menuMode == MenuMode.ChooseChangeSwordMember)
						{
							mTrainPlayer = mPlayerList[mMenuFocusID];

							mChangableClassList.Clear();
							mChangableClassIDList.Clear();

							var swordEnableClassMin = new int[,] {
								{  10,  10,  10,  10,  10,   0 },
								{  10,  10,   5,   0,  20,   0 },
								{  40,   0,   0,   0,   0,   0 },
								{   0,   5,   5,  40,   0,   0 },
								{   0,   0,   0,   0,   0,  40 },
								{  10,   0,   0,  10,   0,  20 },
								{  25,   0,   5,   0,  20,  10 }
							};

							for (var i = 0; i < swordEnableClassMin.GetLength(0); i++)
							{
								var changable = true;

								if (swordEnableClassMin[i, 0] > mTrainPlayer.SwordSkill)
									changable = false;

								if (swordEnableClassMin[i, 1] > mTrainPlayer.AxeSkill)
									changable = false;

								if (swordEnableClassMin[i, 2] > mTrainPlayer.SpearSkill)
									changable = false;

								if (swordEnableClassMin[i, 3] > mTrainPlayer.BowSkill)
									changable = false;

								if (swordEnableClassMin[i, 4] > mTrainPlayer.ShieldSkill)
									changable = false;

								if (swordEnableClassMin[i, 5] > mTrainPlayer.FistSkill)
									changable = false;

								if (changable)
								{
									mChangableClassIDList.Add(i + 1);
									mChangableClassList.Add(Common.SwordClass[i]);
								}
							}

							AppendText(new string[] { 
								$"[color={RGB.LightRed}]당신이 바뀌고 싶은 계급을 고르시오.[/color]",
								$"[color={RGB.White}]비용 : 금 10000 개[/color]"
							});

							ShowMenu(MenuMode.ChooseSwordJob, mChangableClassList.ToArray());
							return;
						}
						else if (menuMode == MenuMode.ChooseSwordJob)
						{
							if (mTrainPlayer.Class != mChangableClassIDList[mMenuFocusID]) {
								mTrainPlayer.Class = mChangableClassIDList[mMenuFocusID];

								AppendText($"[color={RGB.LightGreen}]{mTrainPlayer.NameJosa} 이제 {mTrainPlayer.ClassStr} 계급이 되었다.");

								if (mTrainPlayer.Class < 7)
									mTrainPlayer.SP = 0;

								mParty.Gold -= 10_000;
								UpdateItem(mTrainPlayer);
								DisplaySP();
							}
						}
						else if (menuMode == MenuMode.ChooseChangeMagicMember)
						{
							mTrainPlayer = mPlayerList[mMenuFocusID];

							mChangableClassList.Clear();
							mChangableClassIDList.Clear();

							var magicEnableClassMin = new int[,] {
								{ 10, 10, 10,  0,  0,  0 },
								{  0, 10, 10,  0,  0, 10 },
								{  0,  0, 10,  0,  0, 10 },
								{ 40, 25, 25,  0,  0,  0 },
								{ 20, 20, 40,  0,  0, 40 },
								{ 10, 40, 30,  0,  0, 20 },
								{ 40, 40, 40, 20, 20, 20 }
							};

							for (var i = 0; i < magicEnableClassMin.GetLength(0); i++)
							{
								var changable = true;

								if (magicEnableClassMin[i, 0] > mTrainPlayer.AttackMagic)
									changable = false;

								if (magicEnableClassMin[i, 1] > mTrainPlayer.PhenoMagic)
									changable = false;

								if (magicEnableClassMin[i, 2] > mTrainPlayer.CureMagic)
									changable = false;

								if (magicEnableClassMin[i, 3] > mTrainPlayer.SpecialMagic)
									changable = false;

								if (magicEnableClassMin[i, 4] > mTrainPlayer.ESPMagic)
									changable = false;

								if (magicEnableClassMin[i, 5] > mTrainPlayer.SummonMagic)
									changable = false;

								if (changable)
								{
									mChangableClassIDList.Add(i + 1);
									mChangableClassList.Add(Common.MagicClass[i]);
								}
							}

							AppendText(new string[] {
								$"[color={RGB.LightRed}]당신이 바뀌고 싶은 계급을 고르시오.[/color]",
								$"[color={RGB.White}]비용 : 금 10000 개[/color]"
							});

							ShowMenu(MenuMode.ChooseMagicJob, mChangableClassList.ToArray());
							return;
						}
						else if (menuMode == MenuMode.ChooseMagicJob)
						{
							if (mTrainPlayer.Class != mChangableClassIDList[mMenuFocusID])
							{
								mTrainPlayer.Class = mChangableClassIDList[mMenuFocusID];

								AppendText($"[color={RGB.LightGreen}]{mTrainPlayer.NameJosa} 이제 {mTrainPlayer.ClassStr} 계급이 되었다.");

								mParty.Gold -= 10_000;
								UpdateItem(mTrainPlayer);
							}
						}
						else if (menuMode == MenuMode.BattleCommand)
						{
							mBattleCommandID = mMenuFocusID;

							if (mMenuFocusID == 0)
							{
								SelectEnemy();
							}
							else if (mMenuFocusID == 1)
							{
								ShowCastOneMagicMenu();
							}
							else if (mMenuFocusID == 2)
							{
								ShowCastAllMagicMenu();
							}
							else if (mMenuFocusID == 3)
							{
								ShowCastSpecialMenu();
							}
							else if (mMenuFocusID == 4)
							{
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							}
							else if (mMenuFocusID == 5)
							{
								ShowCastESPMenu();
							}
							else if (mMenuFocusID == 6)
							{
								ShowSummonMenu();
							}
							else if (mMenuFocusID == 7) {
								UseItem(mPlayerList[mBattlePlayerID], true);
							}
							else if (mMenuFocusID == 8)
							{
								if (mBattlePlayerID == 0)
								{
									foreach (var player in mPlayerList)
									{
										if (player.IsAvailable)
										{
											var method = 0;
											var tool = 0;
											var enemyID = 0;

											if (player.ClassType == ClassCategory.Sword) {
												method = 0;
												tool = 0;
											}
											else if (player.ClassType == ClassCategory.Magic) {
												if (player.AttackMagic > 9 || player.SP > player.AttackMagic) {
													method = 1;
													tool = 0;
												}
												else {
													method = 0;
													tool = 0;
												}
											}

											mBattleCommandQueue.Enqueue(new BattleCommand()
											{
												Player = player,
												FriendID = 0,
												Method = method,
												Tool = tool,
												EnemyID = enemyID
											});
										}
									}

									DialogText.TextHighlighters.Clear();
									DialogText.Blocks.Clear();

									ExecuteBattle();
								}
								else
								{
									mBattleCommandID = mMenuFocusID;
									AddBattleCommand();
								}
							}

							return;
						}
						else if (menuMode == MenuMode.CastOneMagic)
						{
							mBattleToolID = mMenuFocusID + 1;

							SelectEnemy();
							return;
						}
						else if (menuMode == MenuMode.CastAllMagic)
						{
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
							return;
						}
						else if (menuMode == MenuMode.CastSpecial)
						{
							mBattleToolID = mMenuFocusID;

							SelectEnemy();
							return;
						}
						else if (menuMode == MenuMode.CastESP)
						{
							if (mMenuFocusID == 0)
							{
								mBattleToolID = mMenuFocusID;
								SelectEnemy();
							}
							else
							{
								var player = mPlayerList[mBattlePlayerID];

								var availCount = 0;
								if (player.ESPMagic > 19)
									availCount = 1;
								else if (player.ESPMagic > 29)
									availCount = 2;
								else if (player.ESPMagic > 79)
									availCount = 3;
								else if (player.ESPMagic > 89)
									availCount = 4;
								else if (player.ESPMagic > 99)
									availCount = 5;

								var espMagicMenuItem = new string[availCount];

								for (var i = 1; i <= availCount; i++)
									espMagicMenuItem[i] = Common.GetMagicName(5, i);

								ShowMenu(MenuMode.ChooseESPMagic, espMagicMenuItem);
							}

							return;
						}
						else if (menuMode == MenuMode.ChooseESPMagic) {
							mBattleToolID = mMenuFocusID + 1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.CastESP) {
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.CastSummon) {
							mBattleToolID = mMenuFocusID + 1;
							mEnemyFocusID = -1;

							AddBattleCommand();
						}
						else if (menuMode == MenuMode.ApplyBattleCureSpell)
						{
							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

							mCureBattle = true;

							ShowCureResult();
						}
						else if (menuMode == MenuMode.ApplyBattleCureAllSpell)
						{
							DialogText.TextHighlighters.Clear();
							DialogText.Blocks.Clear();

							CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

							mCureBattle = true;

							ShowCureResult();
						}
						//else if (menuMode == MenuMode.BattleLose)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//		ShowFileMenu(MenuMode.ChooseGameOverLoadGame);
						//	else
						//		CoreApplication.Exit();
						//}
						else if (menuMode == MenuMode.AskEnter)
						{
							if (mMenuFocusID == 0)
							{
								switch (mTryEnterType)
								{
									case EnterType.CastleLore:
										if (mParty.Etc[9] == 16)
										{
											Talk(new string[] {
												" 당신이 서둘러 로어 성에 다다랐지만 이미 로어 성은 폐허가 되어 버렸다.",
												" 당신은 아찔한 감을 느끼며  알비레오의 예언에 굴복함을 느꼈다." +
												$" 이제는 다시 돌이킬 수가 없는 일이 일어나 버렸다. 우리는 [color={RGB.LightCyan}]다크 메이지 실리안 카미너스[/color]에게 완패한 것이었다."
											});

											mSpecialEvent = SpecialEventType.DestructCastleLore;
										}
										else
										{
											mParty.Map = 6;
											mParty.XAxis = 50;
											mParty.YAxis = 93;

											await RefreshGame();

											for (var x = 48; x < 53; x++)
												UpdateTileInfo(x, 87, 44);

											if ((mParty.Etc[49] & 1) > 0)
												UpdateTileInfo(88, 22, 44);

											if ((mParty.Etc[49] & (1 << 1)) > 0)
												UpdateTileInfo(8, 63, 44);

											if ((mParty.Etc[49] & (1 << 2)) > 0)
												UpdateTileInfo(20, 32, 44);

											if ((mParty.Etc[49] & (1 << 3)) > 0)
												UpdateTileInfo(87, 37, 44);

											if (mParty.Etc[9] == 4)
											{
												AppendText(" 당신이 로어 성에 들어오자  주민들의 열렬한 환영을 받았다." +
												"  하지만 주민들은 당신이 메너스에서 레드 안타레스가 악마 사냥꾼에게 거의 패배했던 당신을 도와준 사실을  알 턱이 없었다." +
												" 메너스의 살인 원인을 제거 하는데 당신이 한 일은 사실 아무것도 없다는 점에 당신은 씁쓸한 웃음을 지을 수 밖에 없었다.");
											}
										}

										break;
									case EnterType.CastleLastDitch:
										if (mParty.Etc[9] >= 16)
											AppendText(" 라스트 디치성은 용암의 대지로 변했다.");
										else {
											mParty.Map = 7;
											mParty.XAxis = 37;
											mParty.YAxis = 68;

											await RefreshGame();

											if ((mParty.Etc[49] & (1 << 7)) > 0)
												UpdateTileInfo(40, 17, 44);

											if ((mParty.Etc[50] & 1) == 0 && (mParty.Etc[31] & (1 << 5)) > 0 && (mParty.Etc[34] & (1 << 4)) > 0)
												UpdateTileInfo(53, 55, 53);
										}
										break;
									case EnterType.Menace:
										if (mParty.Etc[9] >= 16)
											AppendText(" 메너스는  형체도 알아 볼 수 없을 정도로 무너져 버렸다.");
										else if (mParty.Etc[9] == 15) {
											mParty.Map = 4;
											mParty.XAxis = 9;
											mParty.YAxis = 91;

											Talk("당신이 메너스에 입구에 들어서자 마자 저항할수 없는 강한 힘이 일행을 빨아들이기 시작 했다." +
											" 순간 당신은 메너스의 입구 자체가 커다란 통로로 변해 있음을 알아 챘다.");

											mSpecialEvent = SpecialEventType.EnterUnderworld;
										}
										else
										{
											mParty.Map = 10;
											mParty.XAxis = 25;
											mParty.YAxis = 43;

											await RefreshGame();

											switch (mParty.Etc[9]) {
												case 1:
													for (var x = 24; x < 26; x++)
														UpdateTileInfo(x, 42, 52);
													break;
												case 3:
													for (var x = 24; x < 26; x++)
														UpdateTileInfo(x, 42, 52);
													break;
												case 9:
													for (var x = 23; x < 27; x++)
														UpdateTileInfo(x, 8, 0);
													for (var x = 22; x < 28; x++)
														UpdateTileInfo(x, 9, 0);
													for (var x = 22; x < 28; x++)
														UpdateTileInfo(x, 10, 0);
													for (var x = 23; x < 27; x++)
														UpdateTileInfo(x, 11, 0);
													break;
												case 12:
													for (var x = 11; x < 14; x++)
														UpdateTileInfo(x, 9, 0);
													for (var x = 10; x < 14; x++)
														UpdateTileInfo(x, 10, 0);
													for (var x = 21; x < 14; x++)
														UpdateTileInfo(x, 11, 0);
													break;
											}
										}
										break;
									case EnterType.UnknownPyramid:
										if (mParty.Etc[9] >= 16)
											AppendText(" 그러나, 피라미드는 파괴 되었다.");
										else {
											mParty.Map = 2;
											mParty.XAxis = 98;
											mParty.YAxis = 99;

											await RefreshGame();
										}
										break;
									case EnterType.ProofOfInfortune:
										mParty.Map = 11;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.ClueOfInfortune:
										mParty.Map = 12;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.RoofOfLight:
										mParty.Map = 8;
										mParty.XAxis = 24;
										mParty.YAxis = 92;

										await RefreshGame();

										if ((mParty.Etc[39] & 1) > 0)
											UpdateTileInfo(24, 60, 48);
										break;
									case EnterType.TempleOfLight:
										mParty.Map = 13;
										mParty.XAxis = 24;
										mParty.YAxis = 6;

										await RefreshGame();
										break;
									case EnterType.SurvivalOfPerishment:
										mParty.Map = 14;
										mParty.XAxis = 24;
										mParty.YAxis = 43;

										await RefreshGame();

										if ((mParty.Etc[39] & (1 << 7)) == 0) {
											for (var y = 5; y < 17; y++) {
												for (var x = 16; x < 36; x++)
													UpdateTileInfo(x, y, 31);
											}
										}
										else
										{
											UpdateTileInfo(24, 40, 0);
											UpdateTileInfo(25, 40, 0);
										}
										break;
									case EnterType.CaveOfBerial:
										if ((mParty.Etc[9] & (1 << 5)) == 0)
										{
											Talk(" 일행이 동굴 입구에 다가서자  무언가 반짝이는 눈 같은 것이 보였다.");
											mSpecialEvent = SpecialEventType.BattleCaveOfBerialEntrance;
										}
										else
											AppendText(" 하지만 동굴의 입구는 막혀 있어서 들어갈 수가 없습니다.");
										break;
									case EnterType.CaveOfMolok:
										mParty.Map = 15;
										mParty.XAxis = 9;
										mParty.YAxis = 43;

										await RefreshGame();
										break;
									case EnterType.TeleportationGate1:
										mParty.XAxis = 12;
										mParty.YAxis = 71;

										AppendText(" 일행은 다른 곳으로 이동 되었다.");
										break;
									case EnterType.TeleportationGate2:
										mParty.XAxis = 41;
										mParty.YAxis = 76;

										AppendText(" 일행은 다른 곳으로 이동 되었다.");
										break;
									case EnterType.TeleportationGate3:
										mParty.XAxis = 12;
										mParty.YAxis = 71;

										AppendText(" 일행은 다른 곳으로 이동 되었다.");
										break;
									case EnterType.CaveOfAsmodeus1:
										if (mParty.YAxis == 43 && (mParty.Etc[40] & (1 << 6)) == 0) {
											mEncounterEnemyList.Clear();
											JoinEnemy(63);
											JoinEnemy(64);
											DisplayEnemy();

											Talk(new string[] {
												$"[color={RGB.LightMagenta}] 너희들이 이 동굴에 들어 가겠다고?[/color]",
												$"[color={RGB.LightMagenta}] 우리들은  아스모데우님의 경호를  맡고 있는 가디안 라이트와 가디안 레프트라고 한다." +
												"  소개는 이쯤에서 끝내도록하고 바로 대결을 하도록하지."
											});

											mSpecialEvent = SpecialEventType.BattleCaveOfAsmodeusEntrance;
										}
										else {
											mParty.Map = 17;
											mParty.XAxis = 24;
											mParty.YAxis = 43;

											await RefreshGame();

											mParty.Etc[42] = 0;
										}
										break;
									case EnterType.CaveOfAsmodeus2:
										mParty.Map = 17;
										mParty.XAxis = 24;
										mParty.YAxis = 6;

										await RefreshGame();

										mParty.Etc[42] = 0;
										break;
									case EnterType.FortressOfMephistopheles:
										mParty.Map = 17;
										mParty.XAxis = 24;
										mParty.YAxis = 6;

										await RefreshGame();

										mFace = 5;
										// 메피스토텔레스 애니메이션 구현
										//InvokeAnimation(AnimationType.EnterFortressOfMephistopheles);
										break;
									case EnterType.CabinOfRegulus:
										if (mParty.Etc[9] >= 16)
											AppendText(" 이미 오두막은 파괴된 후였다.");
										else {
											mParty.Map = 9;
											mParty.XAxis = 24;
											mParty.YAxis = 39;

											await RefreshGame();

											if ((mParty.Etc[32] != 0 || mParty.Etc[33] != 0) && (mParty.Etc[31] & (1 << 4)) == 0) {
												if (mParty.Day >= mParty.Etc[33] * 256 + mParty.Etc[32]) {
													if (mParty.Hour >= 12) {
														UpdateTileInfo(39, 12, 49);
														mParty.Etc[31] |= 1 << 4;
													}
												}
												else if ((mParty.Etc[31] & (1 << 4)) > 0) {
													if ((mParty.Etc[49] & (1 & 6)) == 0)
														UpdateTileInfo(39, 12, 49);
												}
											}
										}
										break;
								}
							}
							else
							{
								AppendText(new string[] { "" });

								if (mTryEnterType == EnterType.CabinOfRegulus) {
									mParty.XAxis = mPrevX;
									mParty.YAxis = mPrevY;
								}
							}
						}
						//else if (menuMode == MenuMode.SwapMember)
						//{
						//	mMenuMode = MenuMode.None;

						//	mPlayerList[mMenuFocusID + 1] = mReserveMember;
						//	mReserveMember = null;

						//	LeaveMemberPosition();

						//	DisplayPlayerInfo();

						//	AppendText(new string[] { "" });
						//}
						else if (menuMode == MenuMode.ChooseLoadGame || menuMode == MenuMode.ChooseGameOverLoadGame)
						{
							await LoadGame(mMenuFocusID);
						}
						else if (menuMode == MenuMode.ChooseSaveGame)
						{
							var saveData = new SaveData()
							{
								PlayerList = mPlayerList,
								Party = mParty,
								Map = new Map()
								{
									Width = mMapWidth,
									Height = mMapHeight,
									Data = mMapLayer
								},
								Encounter = mEncounter,
								MaxEnemy = mMaxEnemy,
								SaveTime = DateTime.Now.Ticks
							};

							var saveJSON = JsonConvert.SerializeObject(saveData);

							var idStr = "";
							if (mMenuFocusID > 0)
								idStr = mMenuFocusID.ToString();

							var storageFolder = ApplicationData.Current.LocalFolder;
							var saveFile = await storageFolder.CreateFileAsync($"darkSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
							await FileIO.WriteTextAsync(saveFile, saveJSON);

							AppendText($"[color={RGB.LightRed}]현재의 게임을 저장합니다.[/color]");

							//var users = await User.FindAllAsync();
							//var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-000063336555");

							//if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
							//{
							//	var gameSaveProvider = gameSaveTask.Value;

							//	var gameSaveContainer = gameSaveProvider.CreateContainer("LoreSaveContainer");

							//	var buffer = Encoding.UTF8.GetBytes(saveJSON);

							//	var writer = new DataWriter();
							//	writer.WriteUInt32((uint)buffer.Length);
							//	writer.WriteBytes(buffer);
							//	var dataBuffer = writer.DetachBuffer();

							//	var blobsToWrite = new Dictionary<string, IBuffer>();
							//	blobsToWrite.Add($"loreSave{idStr}", dataBuffer);

							//	var gameSaveOperationResult = await gameSaveContainer.SubmitUpdatesAsync(blobsToWrite, null, "LoreSave");
							//	if (gameSaveOperationResult.Status == GameSaveErrorStatus.Ok)
							//		AppendText(new string[] { $"[color={RGB.LightRed}]현재의 게임을 저장합니다.[/color]" });
							//	else
							//		AppendText(new string[] {
							//								$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 저장하지 못했습니다.[/color]",
							//								$"[color={RGB.LightRed}]에러 코드: {gameSaveOperationResult.Status}[/color]"
							//							});
							//}
							//else
							//{
							//	AppendText(new string[] {
							//								$"[color={RGB.LightRed}]현재의 게임을 기기에 저장했지만, 클라우드에 연결할 수 없습니다.[/color]",
							//								$"[color={RGB.LightRed}]에러 코드: {gameSaveTask.Status}[/color]"
							//							});
							//}
						}
						else if (menuMode == MenuMode.JoinMadJoe)
						{
							Lore madJoe = new Lore() {
								Name = "미친 조",
								Gender = GenderType.Male,
								Class = 0,
								ClassType = ClassCategory.Unknown,
								Level = 2,
								Strength = 10,
								Mentality = 5,
								Concentration = 6,
								Endurance = 9,
								Resistance = 5,
								Agility = 7,
								Accuracy = 5,
								Luck = 20,
								Poison = 0,
								Unconscious = 0,
								Dead = 0,
								SP = 0,
								Experience = 0,
								Weapon = 0,
								Shield = 0,
								Armor = 0,
								PotentialAC = 1,
								SwordSkill = 5,
								AxeSkill = 5,
								SpearSkill = 0,
								BowSkill = 0,
								ShieldSkill = 0,
								FistSkill = 0
							};

							madJoe.HP = madJoe.Endurance * madJoe.Level * 10;
							madJoe.UpdatePotentialExperience();
							UpdateItem(madJoe);

							if (mPlayerList.Count > 6)
								mPlayerList[5] = madJoe;
							else
								mPlayerList.Add(madJoe);

							UpdateTileInfo(39, 14, 47);

							mParty.Etc[49] |= 1 << 4;

							DisplayPlayerInfo();
							AppendText("");
						}
						else if (menuMode == MenuMode.JoinMercury) {
							if (mPlayerList.Count < 5)
							{
								Lore mercury = new Lore()
								{
									Name = "머큐리",
									Gender = GenderType.Male,
									Class = 6,
									ClassType = ClassCategory.Sword,
									Level = 2,
									Strength = 12,
									Mentality = 5,
									Concentration = 6,
									Endurance = 11,
									Resistance = 18,
									Agility = 19,
									Accuracy = 16,
									Luck = 19,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									SP = 0,
									Experience = 0,
									Weapon = 0,
									Shield = 0,
									Armor = 0,
									PotentialAC = 2,
									SwordSkill = 10,
									AxeSkill = 0,
									SpearSkill = 0,
									BowSkill = 20,
									ShieldSkill = 0,
									FistSkill = 20
								};

								mercury.HP = mercury.Endurance * mercury.Level * 10;
								mercury.UpdatePotentialExperience();
								UpdateItem(mercury);

								mPlayerList.Add(mercury);
								UpdateTileInfo(62, 9, 47);

								mParty.Etc[49] |= 1 << 5;

								DisplayPlayerInfo();

								AppendText(" 고맙소. 그리고 병사들에게 들키지 않게 여기를 나가야 된다는 것 정도는 알고 있겠지요.");
							}
							else {
								AppendText(" 벌써 사람이 모두 채워져 있군요.  다음 기회를 기다리지요.");
							}
						}
						else if (menuMode == MenuMode.JoinHercules)
						{
							if (mPlayerList.Count < 5)
							{
								Lore hercules = new Lore()
								{
									Name = "헤라클레스",
									Gender = GenderType.Male,
									Class = 2,
									ClassType = ClassCategory.Sword,
									Level = 2,
									Strength = 18,
									Mentality = 5,
									Concentration = 6,
									Endurance = 15,
									Resistance = 12,
									Agility = 14,
									Accuracy = 14,
									Luck = 12,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									SP = 0,
									Experience = 0,
									Weapon = 3,
									Shield = 1,
									Armor = 1,
									PotentialAC = 2,
									SwordSkill = 10,
									AxeSkill = 10,
									SpearSkill = 5,
									BowSkill = 0,
									ShieldSkill = 20,
									FistSkill = 5
								};

								hercules.HP = hercules.Endurance * hercules.Level * 10;
								hercules.UpdatePotentialExperience();
								UpdateItem(hercules);

								mPlayerList.Add(hercules);
								UpdateTileInfo(88, 22, 44);

								mParty.Etc[49] |= 1;

								DisplayPlayerInfo();
								AppendText("");
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.JoinTitan)
						{
							if (mPlayerList.Count < 5)
							{
								Lore titan = new Lore()
								{
									Name = "타이탄",
									Gender = GenderType.Male,
									Class = 1,
									ClassType = ClassCategory.Sword,
									Level = 2,
									Strength = 19,
									Mentality = 3,
									Concentration = 4,
									Endurance = 17,
									Resistance = 10,
									Agility = 13,
									Accuracy = 11,
									Luck = 14,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									SP = 0,
									Experience = 0,
									Weapon = 2,
									Shield = 0,
									Armor = 2,
									PotentialAC = 2,
									SwordSkill = 10,
									AxeSkill = 10,
									SpearSkill = 10,
									BowSkill = 10,
									ShieldSkill = 10,
									FistSkill = 10
								};

								titan.HP = titan.Endurance * titan.Level * 10;
								titan.UpdatePotentialExperience();
								UpdateItem(titan);

								mPlayerList.Add(titan);
								UpdateTileInfo(20, 32, 44);

								mParty.Etc[49] |= 1 << 2;

								DisplayPlayerInfo();
								AppendText("");
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.JoinMerlin)
						{
							if (mPlayerList.Count < 5)
							{
								Lore merlin = new Lore()
								{
									Name = "머린",
									Gender = GenderType.Male,
									Class = 1,
									ClassType = ClassCategory.Magic,
									Level = 2,
									Strength = 5,
									Mentality = 15,
									Concentration = 16,
									Endurance = 10,
									Resistance = 14,
									Agility = 8,
									Accuracy = 13,
									Luck = 17,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									Experience = 0,
									Weapon = 0,
									Shield = 0,
									Armor = 1,
									PotentialAC = 0,
									AttackMagic = 20,
									PhenoMagic = 10,
									CureMagic = 10,
									SpecialMagic = 0,
									ESPMagic = 10,
									SummonMagic = 0
								};

								merlin.HP = merlin.Endurance * merlin.Level * 10;
								merlin.SP = merlin.Mentality * merlin.Level * 10;
								merlin.UpdatePotentialExperience();
								UpdateItem(merlin);

								mPlayerList.Add(merlin);
								UpdateTileInfo(8, 63, 44);

								mParty.Etc[49] |= 1 << 1;

								DisplayPlayerInfo();
								AppendText("");
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.JoinBetelgeuse)
						{
							if (mPlayerList.Count < 5)
							{
								Lore betelgeuse = new Lore()
								{
									Name = "베텔규스",
									Gender = GenderType.Male,
									Class = 2,
									ClassType = ClassCategory.Magic,
									Level = 2,
									Strength = 7,
									Mentality = 17,
									Concentration = 15,
									Endurance = 8,
									Resistance = 12,
									Agility = 10,
									Accuracy = 15,
									Luck = 10,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									Experience = 0,
									Weapon = 0,
									Shield = 0,
									Armor = 1,
									PotentialAC = 0,
									AttackMagic = 10,
									PhenoMagic = 20,
									CureMagic = 10,
									SpecialMagic = 0,
									ESPMagic = 0,
									SummonMagic = 10
								};

								betelgeuse.HP = betelgeuse.Endurance * betelgeuse.Level * 10;
								betelgeuse.SP = betelgeuse.Mentality * betelgeuse.Level * 10;
								betelgeuse.UpdatePotentialExperience();
								UpdateItem(betelgeuse);

								mPlayerList.Add(betelgeuse);
								UpdateTileInfo(87, 37, 44);

								mParty.Etc[49] |= 1 << 3;

								DisplayPlayerInfo();
								AppendText("");
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.JoinPolaris)
						{
							if (mPlayerList.Count < 5)
							{
								Lore polaris = new Lore()
								{
									Name = "폴라리스",
									Gender = GenderType.Male,
									Class = 7,
									ClassType = ClassCategory.Sword,
									Level = 4,
									Strength = 18,
									Mentality = 10,
									Concentration = 6,
									Endurance = 12,
									Resistance = 16,
									Agility = 14,
									Accuracy = 17,
									Luck = 12,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									SP = 0,
									Experience = 0,
									Weapon = 4,
									Shield = 3,
									Armor = 4,
									PotentialAC = 2,
									SwordSkill = 25,
									AxeSkill = 10,
									SpearSkill = 5,
									BowSkill = 0,
									ShieldSkill = 25,
									FistSkill = 10
								};

								polaris.HP = polaris.Endurance * polaris.Level * 10;
								polaris.UpdatePotentialExperience();
								UpdateItem(polaris);

								mPlayerList.Add(polaris);
								UpdateTileInfo(40, 17, 44);

								mParty.Etc[49] |= 1 << 7;

								DisplayPlayerInfo();
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.JoinGeniusKie)
						{
							if (mPlayerList.Count < 5)
							{
								Lore geniusKie = new Lore()
								{
									Name = "지니어스 기",
									Gender = GenderType.Male,
									Class = 7,
									ClassType = ClassCategory.Sword,
									Level = 10,
									Strength = 19,
									Mentality = 15,
									Concentration = 10,
									Endurance = 14,
									Resistance = 18,
									Agility = 16,
									Accuracy = 18,
									Luck = 17,
									Poison = 0,
									Unconscious = 0,
									Dead = 0,
									Experience = 0,
									Weapon = 19,
									Shield = 4,
									Armor = 6,
									PotentialAC = 3,
									SwordSkill = 25,
									AxeSkill = 30,
									SpearSkill = 30,
									BowSkill = 10,
									ShieldSkill = 40,
									FistSkill = 15
								};

								geniusKie.HP = geniusKie.Endurance * geniusKie.Level * 10;
								geniusKie.SP = geniusKie.Mentality * geniusKie.Level * 5;
								geniusKie.UpdatePotentialExperience();
								UpdateItem(geniusKie);

								mPlayerList.Add(geniusKie);
								UpdateTileInfo(53, 55, 44);

								mParty.Etc[50] |= 1;

								DisplayPlayerInfo();
							}
							else
								AppendText(" 나도 당신의 일행에 참가하고 싶지만 벌써 사람이 모두 채워져 있군. 미안하게 됐네.");
						}
						else if (menuMode == MenuMode.BattleChooseItem || menuMode == MenuMode.ChooseItem) {
							var itemID = mUsableItemIDList[mMenuFocusID];


							if (itemID == 0)
							{
								mItemUsePlayer.HP += 1_000;
								if (mItemUsePlayer.HP >= mItemUsePlayer.Endurance * mItemUsePlayer.Level * 10)
								{
									mItemUsePlayer.HP = mItemUsePlayer.Endurance * mItemUsePlayer.Level * 10;
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 건강이 회복 되었습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 건강이 회복 되었습니다.[/color]");

								DisplayHP();
							}
							else if (itemID == 1)
							{
								if (mItemUsePlayer.ClassType != ClassCategory.Magic && mItemUsePlayer.Class != 7)
									ShowApplyItemResult(menuMode, $" {mItemUsePlayer.NameSubjectJosa} 마법사 계열이 아닙니다.");
								else
								{
									if (mItemUsePlayer.ClassType == ClassCategory.Magic)
									{
										mItemUsePlayer.SP += 1_000;
										if (mItemUsePlayer.SP > mItemUsePlayer.Mentality * mItemUsePlayer.Level * 10)
										{
											mItemUsePlayer.SP = mItemUsePlayer.Mentality * mItemUsePlayer.Level * 10;
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 마법 지수가 회복 되었습니다.[/color]");
										}
										else
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 마법 지수가 회복 되었습니다.[/color]");
									}
									else
									{
										mItemUsePlayer.SP += 1_000;
										if (mItemUsePlayer.SP > mItemUsePlayer.Mentality * mItemUsePlayer.Level * 5)
										{
											mItemUsePlayer.SP = mItemUsePlayer.Mentality * mItemUsePlayer.Level * 5;
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 모든 마법 지수가 회복 되었습니다.[/color]");
										}
										else
											ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.Name}의 마법 지수가 회복 되었습니다.[/color]");
									}

									DisplaySP();
								}
							}
							else if (itemID == 2)
							{
								if (mItemUsePlayer.Poison == 0)
									ShowApplyItemResult(menuMode, $" {mItemUsePlayer.NameSubjectJosa} 중독 되지 않았습니다.");
								else
								{
									mItemUsePlayer.Poison = 0;
									ShowApplyItemResult(menuMode, $" [color={RGB.White}]{mItemUsePlayer.NameSubjectJosa} 해독 되었습니다.[/color]");
								}

								DisplayCondition();
							}
							else if (itemID == 3 || itemID == 4)
							{
								mUseItemID = itemID;
								if (menuMode == MenuMode.BattleChooseItem)
									ShowCharacterMenu(MenuMode.BattleUseItemWhom);
								else
									ShowCharacterMenu(MenuMode.UseItemWhom);
							}
							else if (itemID == 5)
							{
								int GetBonusPoint(int seed)
								{
									return mRand.Next(seed * 2 + 1) - seed;
								}

								var ability = 0;
								var livePlayerCount = 0;
								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
									{
										livePlayerCount++;
										ability += player.Level;
									}
								}

								ability /= (int)Math.Round((double)ability * 5 / livePlayerCount);

								var summonPlayer = new Lore();
								switch (mRand.Next(8))
								{
									case 0:
										summonPlayer.Name = "밴더스내치";
										summonPlayer.Endurance = 15 + GetBonusPoint(5);
										summonPlayer.Resistance = 8 + GetBonusPoint(5);
										summonPlayer.Accuracy = 12 + GetBonusPoint(5);
										summonPlayer.Weapon = 33;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 1:
										summonPlayer.Name = "캐리온 크롤러";
										summonPlayer.Endurance = 20 + GetBonusPoint(5);
										summonPlayer.Resistance = 14 + GetBonusPoint(5);
										summonPlayer.Accuracy = 13 + GetBonusPoint(5);
										summonPlayer.Weapon = 34;
										summonPlayer.WeaPower = ability;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 2:
										summonPlayer.Name = "켄타우루스'";
										summonPlayer.Endurance = 17 + GetBonusPoint(5);
										summonPlayer.Resistance = 12 + GetBonusPoint(5);
										summonPlayer.Accuracy = 18 + GetBonusPoint(5);
										summonPlayer.Weapon = 35;
										summonPlayer.WeaPower = (int)Math.Round((double)ability * 1.5);
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
									case 3:
										summonPlayer.Name = "데모고르곤'";
										summonPlayer.Endurance = 18 + GetBonusPoint(5);
										summonPlayer.Resistance = 5 + GetBonusPoint(5);
										summonPlayer.Accuracy = 17 + GetBonusPoint(5);
										summonPlayer.Weapon = 36;
										summonPlayer.WeaPower = ability * 4;
										summonPlayer.PotentialAC = 4;
										summonPlayer.AC = 4;
										break;
									case 4:
										summonPlayer.Name = "듈라한";
										summonPlayer.Endurance = 10 + GetBonusPoint(5);
										summonPlayer.Resistance = 20;
										summonPlayer.Accuracy = 17;
										summonPlayer.Weapon = 16;
										summonPlayer.WeaPower = ability;
										summonPlayer.PotentialAC = 3;
										summonPlayer.AC = 3;
										break;
									case 5:
										summonPlayer.Name = "에틴";
										summonPlayer.Endurance = 10 + GetBonusPoint(5);
										summonPlayer.Resistance = 20;
										summonPlayer.Accuracy = 10 + GetBonusPoint(9);
										summonPlayer.Weapon = 8;
										summonPlayer.WeaPower = (int)Math.Round((double)ability * 0.8);
										summonPlayer.PotentialAC = 1;
										summonPlayer.AC = 1;
										break;
									case 6:
										summonPlayer.Name = "헬하운드";
										summonPlayer.Endurance = 14 + GetBonusPoint(5);
										summonPlayer.Resistance = 9 + GetBonusPoint(5);
										summonPlayer.Accuracy = 11 + GetBonusPoint(5);
										summonPlayer.Weapon = 33;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
									case 7:
										summonPlayer.Name = "미노타우루스";
										summonPlayer.Endurance = 13 + GetBonusPoint(5);
										summonPlayer.Resistance = 11 + GetBonusPoint(5);
										summonPlayer.Accuracy = 14 + GetBonusPoint(5);
										summonPlayer.Weapon = 9;
										summonPlayer.WeaPower = ability * 3;
										summonPlayer.PotentialAC = 2;
										summonPlayer.AC = 2;
										break;
								}
								summonPlayer.Gender = GenderType.Neutral;
								summonPlayer.Class = 0;
								summonPlayer.ClassType = ClassCategory.Unknown;
								summonPlayer.Level = ability / 5;
								summonPlayer.Strength = 10 + GetBonusPoint(5);
								summonPlayer.Mentality = 10 + GetBonusPoint(5);
								summonPlayer.Concentration = 10 + GetBonusPoint(5);
								summonPlayer.Agility = 0;
								summonPlayer.Luck = 10 + GetBonusPoint(5);
								summonPlayer.Poison = 0;
								summonPlayer.Unconscious = 0;
								summonPlayer.Dead = 0;
								summonPlayer.HP = summonPlayer.Endurance * summonPlayer.Level * 10;
								summonPlayer.SP = 0;
								summonPlayer.Experience = 0;
								summonPlayer.PotentialExperience = 0;
								summonPlayer.Shield = 0;
								summonPlayer.ShiPower = 0;
								summonPlayer.Armor = 0;
								summonPlayer.SwordSkill = 0;
								summonPlayer.AxeSkill = 0;
								summonPlayer.SpearSkill = 0;
								summonPlayer.BowSkill = 0;
								summonPlayer.ShieldSkill = 0;
								summonPlayer.FistSkill = 0;

								if (mPlayerList.Count >= 6)
									mPlayerList[5] = summonPlayer;
								else
									mPlayerList.Add(summonPlayer);

								DisplayPlayerInfo();

								ShowApplyItemResult(menuMode, $" [color={RGB.White}]{summonPlayer.NameSubjectJosa} 다른 차원으로 부터 소환 되어졌습니다.[/color]");
							}
							else if (itemID == 6)
							{
								if (mParty.Etc[0] + 10 > 255)
									mParty.Etc[0] = 255;
								else
									mParty.Etc[0] = 255;

								UpdateView();

								ShowApplyItemResult(menuMode, $"[color={RGB.White}] 일행은 대형 횃불을 켰습니다.[/color]");
							}
							else if (itemID == 7)
								ShowWizardEye();
							else if (itemID == 8) {
								mParty.Etc[1] = 255;
								mParty.Etc[2] = 255;
								mParty.Etc[3] = 255;

								ShowApplyItemResult(menuMode, $"[color={RGB.White}] 일행은 모두 비행 부츠를 신었습니다.[/color]");
							}
							else if (itemID == 9) {
								// 공간 이동 구현 필요
							}
						}
						else if (menuMode == MenuMode.BattleUseItemWhom || menuMode == MenuMode.UseItemWhom) {
							var player = mPlayerList[mMenuFocusID];

							if (mUseItemID == 3)
							{
								if (player.Unconscious == 0)
								{
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 의식이 있습니다.");
									return;
								}

								player.Unconscious = 0;
								if (player.Dead == 0)
								{
									if (player.HP <= 0)
										player.HP = 1;

									ShowApplyItemResult(menuMode, $"[color={RGB.White}] {player.NameSubjectJosa} 의식을 차렸습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 이미 죽은 상태 입니다.");
							}
							else if (mUseItemID == 4) {
								if (player.Dead == 0) {
									ShowApplyItemResult(menuMode, $" {player.NameSubjectJosa} 죽지 않았습니다.");
									return;
								}

								if (player.Dead < 10000) {
									player.Dead = 0;
									if (player.Unconscious >= player.Endurance * player.Level)
										player.Unconscious = player.Endurance * player.Level - 1;

									ShowApplyItemResult(menuMode, $"[color={RGB.White}] {player.NameSubjectJosa} 다시 살아났습니다.[/color]");
								}
								else
									ShowApplyItemResult(menuMode, $" {player.Name}의 죽음은 이 약초로는 살리지 못합니다.");
							}
						}
						else if (menuMode == MenuMode.ChooseEquipCromaticShield) {
							var player = mPlayerList[mMenuFocusID];
							if (VerifyShield(player, 4))
							{
								player.Shield = 4;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 크로매틱 방패를 장착했다.[/color]");
								mParty.Etc[48] |= 1 << 7;
							}
							else
								AppendText($"{player.Name}에게는 이 방패가 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.ChooseEquipBattleAxe)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyWeapon(player, 13))
							{
								player.Weapon = 13;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 양날 전투 도끼를 장착했다.[/color]");
								mParty.Etc[44] |= 1;
							}
							else
								AppendText($"{player.Name}에게는 이 무기가 맞지 않습니다.");
						}
						else if (menuMode == MenuMode.ChooseEquipObsidianArmor)
						{
							var player = mPlayerList[mMenuFocusID];
							if (VerifyWeapon(player, 255))
							{
								player.Weapon = 255;
								UpdateItem(player);

								AppendText($"[color={RGB.White}]{player.NameSubjectJosa} 흑요석 갑옷을 장착했다.[/color]");
								mParty.Etc[44] |= 1 << 2;
							}
							else
								AppendText($"{player.Name}에게는 이 갑옷이 맞지 않습니다.");
						}
					}
					//				else if (args.VirtualKey == VirtualKey.P || args.VirtualKey == VirtualKey.GamepadView)
					//				{
					//					ShowPartyStatus();
					//				}
					//				else if (args.VirtualKey == VirtualKey.V || args.VirtualKey == VirtualKey.GamepadLeftTrigger)
					//				{
					//					AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
					//					ShowCharacterMenu(MenuMode.ViewCharacter);
					//				}
					//				else if (args.VirtualKey == VirtualKey.C || args.VirtualKey == VirtualKey.GamepadRightShoulder)
					//				{
					//					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					//					ShowCharacterMenu(MenuMode.CastSpell);
					//				}
					//				else if (args.VirtualKey == VirtualKey.E || args.VirtualKey == VirtualKey.GamepadRightShoulder)
					//				{
					//					AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
					//					ShowCharacterMenu(MenuMode.Extrasense);
					//				}
					
				}
				else if (args.VirtualKey == VirtualKey.R || args.VirtualKey == VirtualKey.GamepadLeftShoulder)
				{
					// 휴식 단축키
					Rest();
				}
			};

			Window.Current.CoreWindow.KeyDown += gamePageKeyDownEvent;
			Window.Current.CoreWindow.KeyUp += gamePageKeyUpEvent;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			if (e.Parameter != null)
			{
				var payload = e.Parameter as Payload;
				mPlayerList = new List<Lore>();
				mPlayerList.Add(payload.Player);
				mParty = payload.Party;

				InitialFirstPlay();
			}
			else
				LoadFile();
		}

		private async void InitialFirstPlay() {
			await LoadMapData();
			InitializeMap();

			mLoading = false;
		}

		private byte GetTileInfo(int x, int y) {
			return (byte)(mMapLayer[x + mMapWidth * y] & 0x7F);
		}
		private byte GetTileInfo(byte[] layer, int index)
		{
			return (byte)(layer[index] & 0x7F);
		}

		private void UpdateTileInfo(int x, int y, int tile) {
			mMapLayer[x + mMapWidth * y] = (byte)((mMapLayer[x + mMapWidth * y] & 0x80) | tile);
		}

		private void MovePlayer(int moveX, int moveY)
		{
			bool UpdatePlayersState(Lore player) {
				var needUpdate = false;
				if (player.Poison > 0)
					player.Poison++;

				if (player.Poison > 10)
				{
					player.Poison = 1;
					if (0 < player.Dead && player.Dead < 100)
						player.Dead++;
					else if (player.Unconscious > 0)
					{
						player.Unconscious++;
						if (player.Unconscious > player.Endurance * player.Level)
							player.Dead = 1;
					}
					else
					{
						player.HP--;
						if (player.HP <= 0)
							player.Unconscious = 1;
					}

					needUpdate = true;
				}

				return needUpdate;
			}

			mParty.XAxis = moveX;
			mParty.YAxis = moveY;

			bool needUpdateStat = false;
			foreach (var player in mPlayerList)
			{
				if (UpdatePlayersState(player))
					needUpdateStat = true;
			}

			if (mAssistPlayer != null) {
				if (UpdatePlayersState(mAssistPlayer))
					needUpdateStat = true;
			}

			if (needUpdateStat)
			{
				DisplayHP();
				DisplayCondition();
			}

			DetectGameOver();

			if (mParty.Etc[4] > 0)
				mParty.Etc[4]--;

			if (!(GetTileInfo(moveX, moveY) == 0 || (mPosition == PositionType.Den && GetTileInfo(moveX, moveY) == 52)) && mRand.Next(mEncounter * 20) == 0)
				EncounterEnemy();
				

			if (mPosition == PositionType.Ground)
				PlusTime(0, 2, 0);
			else
				PlusTime(0, 0, 5);
		}


		private void StartBattle(bool assualt = true)
		{
			mParty.Etc[5] = 1;

			DialogText.TextHighlighters.Clear();
			DialogText.Blocks.Clear();

			if (assualt)
			{
				mBattleTurn = BattleTurn.Player;

				for (var i = 0; i < mPlayerList.Count; i++)
				{
					if (mPlayerList[i].IsAvailable)
					{
						mBattlePlayerID = i;
						break;
					}
				}

				mBattleCommandQueue.Clear();

				BattleMode();
			}
			else
			{
				mBattleTurn = BattleTurn.Player;
				ExecuteBattle();
			}
		}

		private async Task RefreshGame()
		{
			mLoading = true;

			MapCanvas.Visibility = Visibility.Collapsed;

			AppendText("");
			await LoadMapData();
			InitializeMap();

			mLoading = false;
		}

		private void ExecuteBattle()
		{
			void CheckBattleStatus()
			{
				var allPlayerDead = true;
				foreach (var player in mPlayerList)
				{
					if (player.IsAvailable)
					{
						allPlayerDead = false;
						break;
					}
				}

				if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
					allPlayerDead = false;

				if (allPlayerDead)
				{
					mBattleTurn = BattleTurn.Lose;
					mParty.Etc[5] = 0;
				}
				else
				{
					var allEnemyDead = true;

					foreach (var enemy in mEncounterEnemyList)
					{
						if (!enemy.Dead)
						{
							allEnemyDead = false;
							break;
						}
					}

					if (allEnemyDead)
					{
						mBattleTurn = BattleTurn.Win;
						mParty.Etc[5] = 0;
					}
				}
			}

			void ShowBattleResult(List<string> battleResult)
			{
				var lineHeight = 0d;
				if (DialogText.Blocks.Count > 0)
				{
					var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
					lineHeight = startRect.Height;
				}

				var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

				var append = false;
				if (lineCount + battleResult.Count + 1 <= DIALOG_MAX_LINES)
				{
					if (lineHeight > 0)
						battleResult.Insert(0, "");
					append = true;
				}

				AppendText(battleResult.ToArray(), append);

				DisplayPlayerInfo();
				DisplayEnemy();

				ContinueText.Visibility = Visibility.Visible;

				CheckBattleStatus();
			}


			if (mBattleCommandQueue.Count == 0 && mBatteEnemyQueue.Count == 0)
			{
				DialogText.TextHighlighters.Clear();
				DialogText.Blocks.Clear();

				switch (mBattleTurn)
				{
					case BattleTurn.Player:
						mBattleTurn = BattleTurn.Enemy;
						break;
					case BattleTurn.Enemy:
						mBattleTurn = BattleTurn.Player;
						break;
				}

				switch (mBattleTurn)
				{
					case BattleTurn.Player:
						for (var i = 0; i < mPlayerList.Count; i++)
						{
							if (mPlayerList[i].IsAvailable)
							{
								mBattlePlayerID = i;
								break;
							}
						}

						BattleMode();
						return;
					case BattleTurn.Enemy:
						foreach (var enemy in mEncounterEnemyList)
						{
							if (!enemy.Dead && !enemy.Unconscious)
								mBatteEnemyQueue.Enqueue(enemy);
						}

						break;
				}
			}


			if (mBattleCommandQueue.Count > 0)
			{
				var battleCommand = mBattleCommandQueue.Dequeue();
				var battleResult = new List<string>();

				BattleEnemyData GetDestEnemy()
				{
					bool AllEnemyDead()
					{
						for (var i = 0; i < mEncounterEnemyList.Count; i++)
						{
							if (!mEncounterEnemyList[i].Dead && ((!mParty.Cruel && !mEncounterEnemyList[i].Unconscious) || mParty.Cruel))
								return false;
						};

						return true;
					}

					if (!AllEnemyDead())
					{
						var enemyID = battleCommand.EnemyID;
						while (mEncounterEnemyList[enemyID].Dead)
							enemyID = (enemyID + 1) % mEncounterEnemyList.Count;

						return mEncounterEnemyList[enemyID];
					}
					else
						return null;
				}

				void GetBattleStatus(BattleEnemyData enemy)
				{
					var player = battleCommand.Player;

					switch (battleCommand.Method)
					{
						case 0:
							int messageType;
							if (player.Weapon - 1 >= 0)
								messageType = (player.Weapon - 1) / 7;
							else
								messageType = 0;


							switch (messageType)
							{
								case 1:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 내려쳤다[/color]");
									break;
								case 2:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 찔렀다[/color]");
									break;
								case 3:
									if (mParty.Arrow > 0)
										battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 쏘았다[/color]");
									else
										battleResult.Add($"[color={RGB.White}]화살이 다 떨어져 공격할 수 없었다[/color]");
									break;
								default:
									battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 공격했다[/color]");
									break;
							}
							break;
						case 1:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameJosa(0, battleCommand.Tool)}로 {enemy.Name}에게 공격했다[/color]");
							break;
						case 2:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameJosa(1, battleCommand.Tool)}로 {enemy.Name}에게 공격했다[/color]");
							break;
						case 3:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {enemy.Name}에게 {Common.GetMagicNameJosa(4, battleCommand.Tool)}로 특수 공격을 했다[/color]");
							break;
						case 4:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {mPlayerList[battleCommand.FriendID].Name}에게 {Common.GetMagicNameMokjukJosa(3, battleCommand.Tool)} 사용했다[/color]");
							break;
						case 5:
							if (enemy == null)
								battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} 모든 적에게 {Common.GetMagicNameMokjukJosa(5, battleCommand.Tool)} 사용했다[/color]");
							else
								battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {enemy.Name}에게 {Common.GetMagicNameMokjukJosa(5, battleCommand.Tool)} 사용했다[/color]");
							break;
						case 6:
							battleResult.Add($"[color={RGB.White}]{player.NameSubjectJosa} {Common.GetMagicNameMokjukJosa(6, battleCommand.Tool)} 사용했다[/color]");
							break;
						case 8:
							battleResult.Add($"[color={RGB.White}]일행은 도망을 시도했다[/color]");
							break;
						default:
							battleResult.Add($"[color={RGB.White}]{player.Name}(은)는 잠시 주저했다[/color]");
							break;
					}
				}

				void PlusExperience(BattleEnemyData enemy)
				{
#if DEBUG
					var exp = 50_000;
#else
					var exp = (enemy.ENumber + 1) * (enemy.ENumber + 1) * (enemy.ENumber + 1) / 8;
					if (exp == 0)
						exp = 1;
#endif

					if (!enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.Yellow}]{battleCommand.Player.NameSubjectJosa}[/color] [color={RGB.LightCyan}]{exp.ToString("#,#0")}[/color][color={RGB.Yellow}]만큼 경험치를 얻었다![/color]");
						battleCommand.Player.Experience += exp;
					}
					else
					{
						foreach (var player in mPlayerList)
						{
							if (player.IsAvailable)
								player.Experience += exp;
						};

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += exp;
					}
				}

				void AttackOne()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					var player = battleCommand.Player;


					if (enemy.Unconscious)
					{
						switch (mRand.Next(4))
						{
							case 0:
								battleResult.Add($"[color={RGB.LightRed}]{player.GenderPronoun}의 무기가 {enemy.Name}의 심장을 꿰뚫었다[/color]");
								break;
							case 1:
								battleResult.Add($"[color={RGB.LightRed}]{enemy.Name}의 머리는 {player.GenderPronoun}의 공격으로 산산 조각이 났다[/color]");
								break;
							case 2:
								battleResult.Add($"[color={RGB.LightRed}]적의 피가 사방에 뿌려졌다[/color]");
								break;
							case 3:
								battleResult.Add($"[color={RGB.LightRed}]적은 비명과 함께 찢겨 나갔다[/color]");
								break;

						}

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

					if (mRand.Next(20) > player.Accuracy)
					{
						battleResult.Add($"{player.GenderPronoun}의 공격은 빗나갔다 ....");
						return;
					}

					int power;
					switch ((player.Weapon + 6) / 7) {
						case 0:
							power = player.FistSkill;
							break;
						case 1:
							power = player.SwordSkill;
							break;
						case 2:
							power = player.AxeSkill;
							break;
						case 3:
							power = player.SpearSkill;
							break;
						case 4:
							power = player.BowSkill;
							break;
						default:
							power = player.Level * 5;
							break;
					}

					int attackPoint;
					if ((player.ClassType == ClassCategory.Sword) && (player.Class == 5 || player.Class == 6))
						attackPoint = player.Strength * power * 5;
					else
						attackPoint = (int)(Math.Round((double)player.Strength * player.WeaPower * power / 10));

					attackPoint -= attackPoint * mRand.Next(50) / 100;

					if (mRand.Next(100) < enemy.Resistance)
					{
						battleResult.Add($"적은 {player.GenderPronoun}의 공격을 저지했다");
						return;
					}

					var defensePoint = enemy.AC * enemy.Level * (mRand.Next(10) + 1);
					attackPoint -= defensePoint;
					if (attackPoint <= 0)
					{
						battleResult.Add($"그러나, 적은 {player.GenderPronoun}의 공격을 막았다");
						return;
					}

					enemy.HP -= attackPoint;
					if (enemy.HP <= 0)
					{
						enemy.HP = 0;
						enemy.Unconscious = false;
						enemy.Dead = false;

						battleResult.Add($"[color={RGB.LightRed}]적은 {player.GenderPronoun}의 공격으로 의식불명이 되었다[/color]");
						PlusExperience(enemy);
						enemy.Unconscious = true;
					}
					else
					{
						battleResult.Add($"적은 [color={RGB.White}]{attackPoint}[/color]만큼의 피해를 입었다");
					}
				}

				void CastOne()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					var player = battleCommand.Player;

					battleResult.Add($"[color={RGB.White}]{Common.GetWeaponNameJosa(player.Weapon)}로 {enemy.NameJosa} 공격했다[/color]");

					if (enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.LightRed}]{player.GenderPronoun}의 마법은 적을 완전히 제거해 버렸다[/color]");

						PlusExperience(enemy);
						enemy.HP = 0;
						enemy.Dead = true;
						DisplayEnemy();
						return;
					}

#if DEBUG
					var magicPoint = 1;
#else
					var magicPoint = (int)Math.Round((double)battleCommand.Player.AttackPoint * battleCommand.Tool * battleCommand.Tool / 10);
					if (battleCommand.Player.SP < magicPoint)
					{
						battleResult.Add($"마법 지수가 부족했다");
						return;
					}
#endif
					battleCommand.Player.SP -= magicPoint;
					DisplaySP();

					if (mRand.Next(20) >= player.Accuracy)
					{
						battleResult.Add($"그러나, {enemy.NameJosa} 빗나갔다");
						return;
					}

					magicPoint = battleCommand.Tool * battleCommand.Tool * player.AttackMagic * 3;

					if (mRand.Next(100) < enemy.Resistance)
					{
						battleResult.Add($"{enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법을 저지했다");
						return;
					}

					var defensePoint = enemy.AC * enemy.Level * (mRand.Next(10) + 1);
					magicPoint -= defensePoint;

					if (magicPoint <= 0)
					{
						battleResult.Add($"그러나, {enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법 공격을 막았다");
						return;
					}

					enemy.HP -= magicPoint;
					if (enemy.HP <= 0)
					{
						battleResult.Add($"[color={RGB.LightRed}]{enemy.NameSubjectJosa} {battleCommand.Player.GenderPronoun}의 마법에 의해 의식불능이 되었다[/color]");
						PlusExperience(enemy);

						enemy.HP = 0;
						enemy.Unconscious = true;
					}
					else
					{
						battleResult.Add($"{enemy.NameSubjectJosa} [color={RGB.White}]{magicPoint}[/color]만큼의 피해를 입었다");
					}
				}

				void CastSpecialMagic()
				{
					var enemy = GetDestEnemy();
					if (enemy == null)
						return;

					GetBattleStatus(enemy);

					if (battleCommand.Tool == 0)
					{
						const int SKILL_POINT = 200;

						if (battleCommand.Player.SP < SKILL_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= SKILL_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"기술 무력화 공격은 저지 당했다");
							return;
						}

						if (mRand.Next(60) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"기술 무력화 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 특수 공격 능력이 제거되었다[/color]");
						enemy.Special = 0;
					}
					else if (battleCommand.Tool == 1)
					{
						const int DEFENCE_POINT = 50;

						if (battleCommand.Player.SP < DEFENCE_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= DEFENCE_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"방어 무력화 공격은 저지 당했다");
							return;
						}


						int resistancePoint;
						if (enemy.AC < 5)
							resistancePoint = 40;
						else
							resistancePoint = 25;

						if (mRand.Next(resistancePoint) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"방어 무력화 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 방어 능력이 저하되었다[/color]");
						if ((enemy.Resistance < 31 || mRand.Next(2) == 0) && enemy.AC > 0)
							enemy.AC--;
						else
						{
							enemy.Resistance -= 10;
							if (enemy.Resistance > 0)
								enemy.Resistance = 0;
						}
					}
					else if (battleCommand.Tool == 2)
					{
						const int ABILITY_POINT = 100;

						if (battleCommand.Player.SP < ABILITY_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= ABILITY_POINT;
						DisplaySP();

						if (mRand.Next(200) < enemy.Resistance)
						{
							battleResult.Add($"능력 저하 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(30) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"능력 저하 공격은 빗나갔다");
							return;
						}

						battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 전체적인 능력이 저하되었다[/color]");
						if (enemy.Level > 0)
							enemy.Level--;

						enemy.Resistance -= 10;
						if (enemy.Resistance > 0)
							enemy.Resistance = 0;
					}
					else if (battleCommand.Tool == 3)
					{
						const int MAGIC_POINT = 150;

						if (battleCommand.Player.SP < MAGIC_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= MAGIC_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"마법 불능 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"마법 불능 공격은 빗나갔다");
							return;
						}

						if (enemy.CastLevel > 1)
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력이 저하되었다[/color]");
						else
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 마법 능력은 사라졌다[/color]");

						if (enemy.CastLevel > 0)
							enemy.CastLevel--;
					}
					else if (battleCommand.Tool == 4)
					{
						const int SUPERMAN_POINT = 400;

						if (battleCommand.Player.SP < SUPERMAN_POINT)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						battleCommand.Player.SP -= SUPERMAN_POINT;
						DisplaySP();

						if (mRand.Next(100) < enemy.Resistance)
						{
							battleResult.Add($"탈 초인화 공격은 저지 당했다");
							return;
						}


						if (mRand.Next(100) > battleCommand.Player.Accuracy)
						{
							battleResult.Add($"탈 초인화 공격은 빗나갔다");
							return;
						}

						if (enemy.SpecialCastLevel > 1)
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력이 저하되었다[/color]");
						else
							battleResult.Add($"[color={RGB.Red}]{enemy.Name}의 초자연적 능력은 사라졌다[/color]");

						if (enemy.SpecialCastLevel > 0)
							enemy.SpecialCastLevel--;
					}
				}

				void CastESP()
				{
					var player = battleCommand.Player;

					if (battleCommand.Tool == 0)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						if (player.ESPMagic < 40)
						{
							battleResult.Add("독심을 샤용하려 하였지만 아직 능력이 부족했다");
							return;
						}

						if (player.SP < 15)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 15;
						DisplaySP();

						if (enemy.ENumber != 5 &&
							enemy.ENumber != 9 &&
							enemy.ENumber != 19 &&
							enemy.ENumber != 23 &&
							enemy.ENumber != 26 &&
							enemy.ENumber != 28 &&
							enemy.ENumber != 32 &&
							enemy.ENumber != 34 &&
							enemy.ENumber != 39 &&
							enemy.ENumber != 46 &&
							enemy.ENumber != 52 &&
							enemy.ENumber != 61 &&
							enemy.ENumber != 69)
						{
							battleResult.Add($"독심술은 전혀 통하지 않았다");
							return;
						}

						var requireLevel = enemy.Level;
						if (enemy.ENumber == 69)
							requireLevel = 17;

						if (requireLevel > player.Level && mRand.Next(2) == 0)
						{
							battleResult.Add($"적의 마음을 끌어들이기에는 아직 능력이 부족했다");
							return;
						}

						if (mRand.Next(60) > (player.Level - requireLevel) * 2 + battleCommand.Player.Concentration)
						{
							battleResult.Add($"적의 마음은 흔들리지 않았다");
							return;
						}

						battleResult.Add($"[color={RGB.LightCyan}]적은 우리의 편이 되었다[/color]");
						JoinMemberFromEnemy(enemy.ENumber);
						enemy.Dead = true;
						enemy.Unconscious = true;
						enemy.HP = 0;
						enemy.Level = 0;
					}
					else if (battleCommand.Tool == 1) {
						GetBattleStatus(null);

						if (player.ESPMagic < 20)
						{
							battleResult.Add($"{player.GenderPronoun}는 주위의 원소들을 이용하려 했으나 역부족이었다");
							return;
						}

						if (player.SP < 100) {
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 100;
						DisplaySP();

						if (mRand.Next(2) == 0)
							battleResult.Add("갑자기 땅속의 우라늄이 핵분열을 일으켜 고온의 열기가 적의 주위를 감싸기 시작한다");
						else
							battleResult.Add("공기중의 수소가 돌연히 핵융합을 일으켜 질량 결손의 에너지를 적들에게 방출하기 시작한다");

						foreach (var enemy in mEncounterEnemyList) {
							if (enemy.Dead || (enemy.Unconscious && !mParty.Cruel))
								continue;

							var impactPoint = enemy.HP;
							if (impactPoint < player.Concentration * player.Level)
								impactPoint = 0;
							else
								impactPoint -= player.Concentration * player.Level;
							enemy.HP = impactPoint;

							if (enemy.Unconscious && !enemy.Dead)
							{
								enemy.Dead = true;
								PlusExperience(enemy);
							}
							else if (impactPoint == 0 || !enemy.Unconscious) {
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
						}
					}
					else if (battleCommand.Tool == 2) {
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{player.GenderPronoun}는 적에게 공포심을 불어 넣었다");

						if (player.ESPMagic < 30)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 100)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 100;
						DisplaySP();

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Resistance < 5)
								enemy.Resistance = 0;
							else
								enemy.Resistance -= 5;
							return;
						}

						if (mRand.Next(60) > battleCommand.Player.Concentration)
						{
							if (enemy.Endurance < 5)
								enemy.Endurance = 0;
							else
								enemy.Endurance -= 5;

							return;
						}

						enemy.Dead = true;
						battleResult.Add($"[color={RGB.LightGreen}]{enemy.Name}(은)는 겁을 먹고는 도망가 버렸다[/color]");
					}
					else if (battleCommand.Tool == 3) {
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{player.GenderPronoun}는 적을 환상속에 빠지게 하려한다");

						if (player.ESPMagic < 80)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 300)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 300;
						DisplaySP();

						if (mRand.Next(30) < player.Concentration) {
							for (var i = 0; i < 2; i++) {
								if (enemy.Accuracy[i] < 4)
									enemy.Accuracy[i] = 0;
								else
									enemy.Accuracy[i] -= 4;
							}
						}

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Agility < 5)
								enemy.Agility = 0;
							else
								enemy.Agility -= 5;
							return;
						}
					}
					else if (battleCommand.Tool == 4)
					{
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{battleCommand.Player.GenderPronoun}는 적의 신진대사를 조절하여 적의 체력을 점차 약화 시키려 한다");

						if (player.ESPMagic < 90)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 500)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 500;
						DisplaySP();

						if (mRand.Next(40) > battleCommand.Player.Concentration)
							return;

						if (enemy.Posion)
						{
							if (enemy.HP > 500)
								enemy.HP -= 50;
							else
							{
								enemy.HP = 0;
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
						}
						else
							enemy.Posion = true;
					}
					else if (battleCommand.Tool == 5) {
						var enemy = GetDestEnemy();
						if (enemy == null)
							return;

						GetBattleStatus(enemy);

						battleResult.Add($"{battleCommand.Player.GenderPronoun}는 염력으로 적의 영혼을 분리 시키려 한다");

						if (player.ESPMagic < 100)
						{
							battleResult.Add($"{player.GenderPronoun}는 아직 능력이 부족 했다");
							return;
						}

						if (player.SP < 1000)
						{
							battleResult.Add($"마법 지수가 부족했다");
							return;
						}

						player.SP -= 1000;
						DisplaySP();

						if (mRand.Next(40) < enemy.Resistance)
						{
							if (enemy.Resistance < 20)
								enemy.Resistance = 0;
							else
								enemy.Resistance -= 20;
							return;
						}

						if (mRand.Next(80) > player.Concentration)
						{
							if (enemy.HP < 500)
							{
								enemy.HP = 0;
								enemy.Unconscious = true;
								PlusExperience(enemy);
							}
							else
								enemy.HP -= 500;

							return;
						}

						enemy.Unconscious = true;
					}
				}


				if (battleCommand.Method == 0)
				{
					AttackOne();
				}
				else if (battleCommand.Method == 1)
				{
					CastOne();
				}
				else if (battleCommand.Method == 2)
				{
					for (var i = 0; i < mEncounterEnemyList.Count; i++)
					{
						if (!mEncounterEnemyList[i].Dead)
						{
							battleCommand.EnemyID = i;
							CastOne();
						}
					}
				}
				else if (battleCommand.Method == 3)
				{
					if (battleCommand.Tool < 5)
						CastSpecialMagic();
					else
					{
						battleCommand.Tool -= 5;
						for (var i = 0; i < mEncounterEnemyList.Count; i++)
						{
							if (!mEncounterEnemyList[i].Dead && ((mEncounterEnemyList[i].Unconscious && mParty.Cruel) || !mParty.Cruel))
							{
								battleCommand.EnemyID = i;
								CastSpecialMagic();
							}
						}
					}
				}
				else if (battleCommand.Method == 4)
				{
					//if (battleCommand.FriendID < mPlayerList.Count)
					//	CureSpell(battleCommand.Player, mPlayerList[battleCommand.FriendID], battleCommand.Tool, battleResult);
					//else
					//	CureAllSpell(battleCommand.Player, battleCommand.Tool, battleResult);
				}
				else if (battleCommand.Method == 5)
				{
					CastESP();
				}
				else if (battleCommand.Method == 8)
				{
					GetBattleStatus(null);

					if (mRand.Next(50) > battleCommand.Player.Agility)
						battleResult.Add($"그러나, 일행은 성공하지 못했다");
					else
					{
						mBattleTurn = BattleTurn.RunAway;
						battleResult.Add($"[color={RGB.LightCyan}]성공적으로 도망을 갔다[/color]");

						mParty.Etc[5] = 2;
					}
				}

				ShowBattleResult(battleResult);
			}
			else
			{
				BattleEnemyData enemy = null;

				do
				{
					if (mBatteEnemyQueue.Count == 0)
						break;

					enemy = mBatteEnemyQueue.Dequeue();

					if (enemy.Posion)
					{
						if (enemy.Unconscious)
							enemy.Dead = true;
						else
						{
							enemy.HP--;
							if (enemy.HP <= 0)
								enemy.Unconscious = true;
						}
					}

					if (!enemy.Unconscious && !enemy.Dead)
						break;
					else
						enemy = null;
				} while (mBatteEnemyQueue.Count > 0);


				if (enemy == null)
				{
					mBattleTurn = BattleTurn.Win;
					ContinueText.Visibility = Visibility.Visible;
				}
				else
				{
					var battleResult = new List<string>();

					var liveEnemyCount = 0;
					foreach (var otherEnemy in mEncounterEnemyList)
					{
						if (!otherEnemy.Dead)
							liveEnemyCount++;
					}

					if (enemy.SpecialCastLevel > 0 && enemy.ENumber > 0)
					{
						if (liveEnemyCount < (mRand.Next(3) + 2) && mRand.Next(3) == 0)
						{
							var newEnemy = JoinEnemy(enemy.ENumber + mRand.Next(4) - 20);
							DisplayEnemy();
							battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {newEnemy.NameJosa} 생성시켰다[/color]");
						}

						if (enemy.SpecialCastLevel > 1)
						{
							liveEnemyCount = 0;
							foreach (var otherEnemy in mEncounterEnemyList)
							{
								if (!otherEnemy.Dead)
									liveEnemyCount++;
							}

							if (mPlayerList.Count >= 6 && liveEnemyCount < 7 && (mRand.Next(5) == 0))
							{
								var turnEnemy = TurnMind(mPlayerList[5]);
								mPlayerList.RemoveAt(5);

								DisplayPlayerInfo();
								DisplayEnemy();

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {turnEnemy.NameJosa} 자기편으로 끌어들였다[/color]");
							}
						}

						if (enemy.SpecialCastLevel > 2 && enemy.Special != 0 && mRand.Next(5) == 0)
						{
							void Cast(Lore player) {
								if (player.Dead == 0)
								{
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}에게 죽음의 공격을 시도했다[/color]");

									if (mRand.Next(60) > player.Agility)
										battleResult.Add($"죽음의 공격은 실패했다");
									else if (mRand.Next(20) < player.Luck)
										battleResult.Add($"그러나, {player.NameSubjectJosa} 죽음의 공격을 피했다");
									else
									{
										battleResult.Add($"[color={RGB.Red}]{player.NameSubjectJosa} 죽었다!![/color]");

										if (player.Dead == 0)
										{
											player.Dead = 1;
											if (player.HP > 0)
												player.HP = 0;
										}
									}
								}
							}

							foreach (var player in mPlayerList)
							{
								Cast(player);
							}

							if (mAssistPlayer != null)
								Cast(mAssistPlayer);
						}
					}

					var agility = enemy.Agility;
					if (agility > 20)
						agility = 20;

					var specialAttack = false;
					if (enemy.Special > 0 && mRand.Next(50) < agility)
					{
						void EnemySpecialAttack()
						{
							if (enemy.Special == 1)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Poison == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 독 공격을 시도했다[/color]");
								if (mRand.Next(40) > enemy.Agility)
								{
									battleResult.Add($"독 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.Name}(은)는 독 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 중독 되었다 !![/color]");

								if (destPlayer.Poison == 0)
									destPlayer.Poison = 1;
							}
							else if (enemy.Special == 2)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Unconscious == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 치명적 공격을 시도했다[/color]");
								if (mRand.Next(50) > enemy.Agility)
								{
									battleResult.Add($"치명적 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.Name}(은)는 치명적 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 의식불명이 되었다 !![/color]");

								if (destPlayer.Unconscious == 0)
								{
									destPlayer.Unconscious = 1;

									if (destPlayer.HP > 0)
										destPlayer.HP = 0;
								}
							}
							else if (enemy.Special == 3)
							{
								var normalList = new List<Lore>();

								foreach (var player in mPlayerList)
								{
									if (player.Dead == 0)
										normalList.Add(player);
								}

								if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
									normalList.Add(mAssistPlayer);

								var destPlayer = normalList[mRand.Next(normalList.Count)];

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 죽음의 공격을 시도했다[/color]");
								if (mRand.Next(60) > enemy.Agility)
								{
									battleResult.Add($"죽음의 공격은 실패했다");
									return;
								}

								if (mRand.Next(20) < destPlayer.Luck)
								{
									battleResult.Add($"그러나, {destPlayer.Name}(은)는 죽음의 공격을 피했다");
									return;
								}

								battleResult.Add($"[color={RGB.Red}]{destPlayer.Name}(은)는 죽었다 !![/color]");

								if (destPlayer.Dead == 0)
								{
									destPlayer.Dead = 1;

									if (destPlayer.HP > 0)
										destPlayer.HP = 0;
								}
							}
						}

						liveEnemyCount = 0;
						foreach (var otherEnemy in mEncounterEnemyList)
						{
							if (!otherEnemy.Dead)
								liveEnemyCount++;
						}

						if (liveEnemyCount > 3)
						{
							EnemySpecialAttack();
							specialAttack = true;
						}
					}


					if (!specialAttack)
					{
						void EnemyAttack()
						{
							if (mRand.Next(20) >= enemy.Accuracy[0])
							{
								battleResult.Add($"{enemy.Name}(은)는 빗맞추었다");
								return;
							}

							var normalList = new List<Lore>();

							foreach (var player in mPlayerList)
							{
								if (player.IsAvailable)
									normalList.Add(player);
							}

							if (mAssistPlayer != null && mAssistPlayer.Poison == 0)
								normalList.Add(mAssistPlayer);

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							if (destPlayer.ClassType == ClassCategory.Sword && destPlayer.Shield > 0) {
								if (mRand.Next(550) < destPlayer.ShieldSkill * destPlayer.ShiPower) {
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.NameJosa} 공격했다[/color]");
									battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 방패로 적의 공격을 저지했다");
									return;
								}
							}

							var attackPoint = enemy.Strength * enemy.Level * (mRand.Next(10) + 1) / 5 - (destPlayer.AC * destPlayer.Level * (mRand.Next(10) + 1) / 10);

							if (attackPoint <= 0)
							{
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {destPlayer.NameJosa} 공격했다[/color]");
								battleResult.Add($"그러나, {destPlayer.NameSubjectJosa} 적의 공격을 방어했다");
								return;
							}

							if (destPlayer.Dead > 0)
								destPlayer.Dead += attackPoint;

							if (destPlayer.Unconscious > 0 && destPlayer.Dead == 0)
								destPlayer.Unconscious += attackPoint;

							if (destPlayer.HP > 0)
								destPlayer.HP -= attackPoint;

							battleResult.Add($"[color={RGB.LightMagenta}]{destPlayer.NameSubjectJosa} {enemy.Name}에게 공격받았다[/color]");
							battleResult.Add($"[color={RGB.Magenta}]{destPlayer.NameSubjectJosa}[/color] [color={RGB.LightMagenta}]{attackPoint}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
						}

						if (mRand.Next(enemy.Accuracy[0] * 1000) > mRand.Next(enemy.Accuracy[1] * 1000) && enemy.Strength > 0 || enemy.CastLevel == 0)
						{
							EnemyAttack();
						}
						else
						{
							void CastAttack(int castPower, Lore player)
							{
								if (mRand.Next(20) >= enemy.Accuracy[1])
								{
									battleResult.Add($"{enemy.Name}의 마법공격은 빗나갔다");
									return;
								}

								if (mRand.Next(50) < player.Resistance)
								{
									battleResult.Add($"그러나, {player.NameSubjectJosa} 적의 마법을 저지했다");
									return;
								}

								castPower -= mRand.Next(castPower / 2);
								castPower -= player.AC * player.Level * (mRand.Next(10) + 1) / 10;
								if (castPower <= 0)
								{
									battleResult.Add($"그러나, {player.NameSubjectJosa} 적의 마법을 막아냈다");
									return;
								}

								if (player.Dead > 0)
									player.Dead += castPower;

								if (player.Unconscious > 0 && player.Dead == 0)
									player.Unconscious += castPower;

								if (player.HP > 0)
									player.HP -= castPower;

								battleResult.Add($"[color={RGB.Magenta}]{player.NameSubjectJosa}[/color] [color={RGB.LightMagenta}]{castPower}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
							}

							void CastAttackOne(Lore player)
							{
								string castName;
								int castPower;
								if (1 <= enemy.Mentality && enemy.Mentality <= 3)
								{
									castName = "충격";
									castPower = 1;
								}
								else if (4 <= enemy.Mentality && enemy.Mentality <= 8)
								{
									castName = "냉기";
									castPower = 2;
								}
								else if (9 <= enemy.Mentality && enemy.Mentality <= 10)
								{
									castName = "고통";
									castPower = 4;
								}
								else if (11 <= enemy.Mentality && enemy.Mentality <= 14)
								{
									castName = "혹한";
									castPower = 6;
								}
								else if (15 <= enemy.Mentality && enemy.Mentality <= 18)
								{
									castName = "화염";
									castPower = 7;
								}
								else
								{
									castName = "번개";
									castPower = 10;
								}

								castPower *= enemy.Level;
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {player.Name}에게 {castName}마법을 사용했다[/color]");

								CastAttack(castPower, player);
							}

							void CastAttackAll(List<Lore> destPlayerList)
							{
								string castName;
								int castPower;
								if (1 <= enemy.Mentality && enemy.Mentality <= 6)
								{
									castName = "열파";
									castPower = 1;
								}
								else if (7 <= enemy.Mentality && enemy.Mentality <= 12)
								{
									castName = "에너지";
									castPower = 2;
								}
								else if (13 <= enemy.Mentality && enemy.Mentality <= 16)
								{
									castName = "초음파";
									castPower = 3;
								}
								else if (17 <= enemy.Mentality && enemy.Mentality <= 20)
								{
									castName = "혹한기";
									castPower = 5;
								}
								else
								{
									castName = "화염폭풍";
									castPower = 8;
								}

								castPower *= enemy.Level;
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} 일행 모두에게 {castName}마법을 사용했다[/color]");

								foreach (var player in destPlayerList)
									CastAttack(castPower, player);

								if (mAssistPlayer != null)
									CastAttack(castPower, mAssistPlayer);
							}

							void CureEnemy(BattleEnemyData whomEnemy, int curePoint)
							{
								if (enemy == whomEnemy)
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} 자신을 치료했다[/color]");
								else
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {whomEnemy.NameJosa} 치료했다[/color]");

								if (whomEnemy.Dead)
									whomEnemy.Dead = false;
								else if (whomEnemy.Unconscious)
								{
									whomEnemy.Unconscious = false;
									if (whomEnemy.HP <= 0)
										whomEnemy.HP = 1;
								}
								else
								{
									whomEnemy.HP += curePoint;
									if (whomEnemy.HP > whomEnemy.Endurance * whomEnemy.Level * 10)
										whomEnemy.HP = whomEnemy.Endurance * whomEnemy.Level * 10;
								}
							}

							void CastHighLevel(List<Lore> destPlayerList)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 4) && mRand.Next(3) == 0)
								{
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
									return;
								}

								var avgAC = 0;
								var avgCount = 0;

								foreach (var player in mPlayerList)
								{
									if (player.IsAvailable)
									{
										avgAC += player.AC;
										avgCount++;
									}
								}

								if (mAssistPlayer != null && mAssistPlayer.IsAvailable) {
									avgAC += mAssistPlayer.AC;
									avgCount++;
								}

								avgAC /= avgCount;

								if (avgAC > 4 && mRand.Next(5) == 0)
								{
									void BreakArmor(Lore player) {
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.NameSubjectJosa} {player.Name}의 갑옷파괴를 시도했다[/color]");
										if (player.Luck > mRand.Next(21))
											battleResult.Add($"그러나, {enemy.NameSubjectJosa} 성공하지 못했다");
										else
										{
											battleResult.Add($"[color={RGB.Magenta}]{player.Name}의 갑옷은 파괴되었다[/color]");

											if (player.AC > 0)
												player.AC--;
										}
									}

									foreach (var player in mPlayerList)
									{
										BreakArmor(player);
									}

									if (mAssistPlayer != null)
										BreakArmor(mAssistPlayer);

									DisplayPlayerInfo();
								}
								else
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level * 10;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(3) != 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality * 2);
									}
									else if (mRand.Next(destPlayerList.Count) < 2)
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										if (mAssistPlayer != null && mAssistPlayer.IsAvailable && mAssistPlayer.HP < weakestPlayer.HP)
											weakestPlayer = mAssistPlayer;

										CastAttackOne(weakestPlayer);
									}
									else
										CastAttackAll(destPlayerList);
								}
							}


							var normalList = new List<Lore>();

							foreach (var player in mPlayerList)
							{
								if (player.IsAvailable)
									normalList.Add(player);
							}

							if (mAssistPlayer != null && mAssistPlayer.IsAvailable)
								normalList.Add(mAssistPlayer);

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							if (enemy.CastLevel == 1)
							{
								CastAttackOne(destPlayer);
							}
							else if (enemy.CastLevel == 2)
							{
								CastAttackOne(destPlayer);
							}
							else if (enemy.CastLevel == 3)
							{
								if (mRand.Next(normalList.Count) < 2)
									CastAttackOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 4)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 3) && mRand.Next(2) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
								else if (mRand.Next(normalList.Count) < 2)
									CastAttackOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 5)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level * 3) && mRand.Next(3) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality * 3);
								else if (mRand.Next(normalList.Count) < 2)
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level * 10;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(2) == 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality * 2);
									}
									else
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										if (mAssistPlayer != null && mAssistPlayer.IsAvailable && mAssistPlayer.HP < weakestPlayer.HP)
											weakestPlayer = mAssistPlayer;

										CastAttackOne(weakestPlayer);
									}
								}
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 6)
							{
								CastHighLevel(normalList);
							}
						}
					}

					ShowBattleResult(battleResult);
				}
			}
		}

		private void CureSpell(Lore player, Lore whomPlayer, int magic, List<string> cureResult = null)
		{
			switch (magic)
			{
				case 0:
					HealOne(player, whomPlayer, cureResult);
					break;
				case 1:
					CureOne(player, whomPlayer, cureResult);
					break;
				case 2:
					CureOne(player, whomPlayer, cureResult);
					HealOne(player, whomPlayer, cureResult);
					break;
				case 3:
					ConsciousOne(player, whomPlayer, cureResult);
					break;
				case 4:
					RevitalizeOne(player, whomPlayer, cureResult);
					break;
				case 5:
					ConsciousOne(player, whomPlayer, cureResult);
					CureOne(player, whomPlayer, cureResult);
					HealOne(player, whomPlayer, cureResult);
					break;
				case 6:
					RevitalizeOne(player, whomPlayer, cureResult);
					ConsciousOne(player, whomPlayer, cureResult);
					CureOne(player, whomPlayer, cureResult);
					HealOne(player, whomPlayer, cureResult);
					break;
			}

			UpdatePlayersStat();
		}

		private void CureAllSpell(Lore player, int magic, List<string> cureResult = null)
		{
			switch (magic)
			{
				case 0:
					HealAll(player, cureResult);
					break;
				case 1:
					CureAll(player, cureResult);
					break;
				case 2:
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;
				case 3:
					ConsciousAll(player, cureResult);
					break;
				case 4:
					ConsciousAll(player, cureResult);
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;
				case 5:
					RevitalizeAll(player, cureResult);
					break;
				case 6:
					RevitalizeAll(player, cureResult);
					ConsciousAll(player, cureResult);
					CureAll(player, cureResult);
					HealAll(player, cureResult);
					break;

			}

			UpdatePlayersStat();
		}

		private void BattleMode()
		{
			var player = mPlayerList[mBattlePlayerID];
			mBattleFriendID = 0;
			mBattleToolID = 0;

			AppendText($"{player.Name}의 전투 모드 ===>");

			ShowMenu(MenuMode.BattleCommand, new string[] {
				$"한 명의 적을 {Common.GetWeaponNameJosa(player.Weapon)}로 공격",
				"한 명의 적에게 마법 공격",
				"모든 적에게 마법 공격",
				"적에게 특수 마법 공격",
				"일행을 치료",
				"적에게 초능력 사용",
				"소환 마법 사용",
				"약초를 사용",
				mBattlePlayerID == 0 ? "일행에게 무조건 공격 할 것을 지시" : "도망을 시도함"
			});
		}

		private void ShowPartyStatus()
		{
			string CheckEnable(int i)
			{
				if (mParty.Etc[i] == 0)
					return "불가";
				else
					return "가능";
			}

			DialogText.Visibility = Visibility.Collapsed;
			PartyInfoPanel.Visibility = Visibility.Visible;

			XPosText.Text = (mParty.XAxis + 1).ToString();
			YPosText.Text = (mParty.YAxis + 1).ToString();

			FoodText.Text = mParty.Food.ToString();
			GoldText.Text = mParty.Gold.ToString("#,#0");
			ArrowText.Text = mParty.Arrow.ToString();

			EnableLightText.Text = CheckEnable(0);
			EnableLevitationText.Text = CheckEnable(3);
			EnableFloatingWaterText.Text = CheckEnable(1);
			EnableFloatingSwampText.Text = CheckEnable(2);

			HPPotionText.Text = mParty.Item[0].ToString();
			SPPotionText.Text = mParty.Item[1].ToString();
			AntidoteText.Text = mParty.Item[2].ToString();
			ConsciousText.Text = mParty.Item[3].ToString();
			RevivalText.Text = mParty.Item[4].ToString();

			SummonScrollText.Text = mParty.Item[5].ToString();
			BigTorchText.Text = mParty.Item[6].ToString();
			CrystalText.Text = mParty.Item[7].ToString();
			FlyingBootsText.Text = mParty.Item[8].ToString();
			TransportationMarbleText.Text = mParty.Item[9].ToString();

			DateText.Text = $"{mParty.Year}년 {mParty.Day / 30 + 1}월 {mParty.Day % 30 + 1}일";
			TimeText.Text = $"{mParty.Hour}시 {mParty.Min}분";
		}

		private void Rest() {
			AppendText($"[color={RGB.White}]일행이 여기서 쉴 시간을 지정 하십시오.[/color]");

			var rangeItems = new List<Tuple<string, int>>();
			for (var i = 1; i <= 24; i++)
			{
				rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i}[/color][color={RGB.White}] 시간 동안[/color]", i));
			}

			ShowSpinner(SpinnerType.RestTimeRange, rangeItems.ToArray(), 0);
		}

		private void ShowEnterMenu(EnterType enterType)
		{
			mTryEnterType = enterType;

			AppendText(new string[] { $"{mEnterTypeMap[enterType]}에 들어가기를 원합니까 ?" });

			ShowMenu(MenuMode.AskEnter, new string[] {
				"예, 그렇습니다.",
				"아니오, 원하지 않습니다."
			});
		}

		private async Task<bool> InvokeSpecialEvent(int prevX, int prevY)
		{
			var triggered = true;

			void FindGold(int id, int bit, int gold) {
				if ((mParty.Etc[id] & bit) == 0)
				{
					AppendText($"당신은 금화 {gold}개를 발견했다.");
					mParty.Gold += gold;
					mParty.Etc[id] |= bit;
				}

				triggered = false;
			}

			void FindItem(int id, int bit, int item, int count) {
				var itemName = new string[] {
					"체력 회복약을", "마법 회복약을", "해독의 약초를", "의식의 약초를", "부활의 약초를",
					"소환 문서를", "대형 횃불을", "수정 구슬을", "비행 부츠를", "이동 구슬을"
				};

				if ((mParty.Etc[id] & bit) == 0) {
					AppendText($"일행은 {itemName[item]} {count}개 발견했다.");

					if (mParty.Item[item] + count < 256)
						mParty.Item[item] += count;
					else
						mParty.Item[item] = 255;

					mParty.Etc[id] |= bit;
				}

				triggered = false;
			}

			if (mParty.Map == 1)
			{
				if (mParty.XAxis == 81 && mParty.YAxis == 8)
				{
					mPrevX = prevX;
					mPrevY = prevY;

					ShowEnterMenu(EnterType.CabinOfRegulus);
				}
			}
			else if (mParty.Map == 2)
			{
				if ((mParty.XAxis == 95 && mParty.YAxis == 93) || (mParty.XAxis == 95 && mParty.YAxis == 94))
				{
					AppendText(" 일행은 고대의 유적으로 공간 이동이 되었다.");
					mParty.XAxis = 140;
					mParty.YAxis = 115;
				}
				else if (mParty.XAxis == 136 && (114 <= mParty.YAxis && mParty.YAxis <= 118))
				{
					AppendText(" 일행은 공간 이동이 되었다.");
					mParty.XAxis = 94;
					mParty.YAxis = 100;
				}
				else if (mParty.XAxis == 143 && mParty.YAxis == 55)
					FindGold(43, 1 << 1, 5_000);
				else if (mParty.XAxis == 145 && mParty.YAxis == 56)
					FindItem(43, 1 << 2, 7, 5);
				else if (mParty.XAxis == 149 && mParty.YAxis == 57)
					FindGold(43, 1 << 3, 8_000);
				else if (mParty.XAxis == 153 && mParty.YAxis == 55)
					FindItem(43, 1 << 4, 8, 3);
				else if (mParty.XAxis == 136 && mParty.YAxis == 109)
					FindGold(43, 1 << 5, 15_000);
				else if (mParty.XAxis == 108 && mParty.YAxis == 88)
					FindItem(43, 1 << 6, 4, 2);
				else if (mParty.XAxis == 143 && mParty.YAxis == 79)
					FindGold(43, 1 << 7, 20_000);
				else if (mParty.XAxis == 104 && mParty.YAxis == 87)
					FindGold(44, 1 << 1, 18_000);
				else if (mParty.XAxis == 7 && mParty.YAxis == 15)
					FindGold(46, 1, 1_000);
				else if (mParty.XAxis == 7 && mParty.YAxis == 48)
					FindGold(46, 1 << 1, 2_000);
				else if (mParty.XAxis == 19 && mParty.YAxis == 54)
					FindGold(46, 1 << 2, 3_000);
				else if (mParty.XAxis == 22 && mParty.YAxis == 50)
					FindGold(46, 1 << 3, 4_000);
				else if (mParty.XAxis == 35 && mParty.YAxis == 40)
					FindGold(46, 1 << 4, 5_000);
				else if (mParty.XAxis == 116 && mParty.YAxis == 115)
					FindGold(46, 1 << 5, 6_000);
				else if (mParty.XAxis == 142 && mParty.YAxis == 72)
					FindGold(46, 1 << 6, 7_000);
				else if (mParty.XAxis == 166 && mParty.YAxis == 37)
					FindGold(46, 1 << 7, 8_000);
				else if (mParty.XAxis == 40 && mParty.YAxis == 60)
					FindItem(47, 1, 0, 3);
				else if (mParty.XAxis == 114 && mParty.YAxis == 6)
					FindItem(47, 1 << 1, 1, 2);
				else if (mParty.XAxis == 161 && mParty.YAxis == 14)
					FindItem(47, 1 << 2, 2, 1);
				else if (mParty.XAxis == 162 && mParty.YAxis == 50)
					FindItem(47, 1 << 3, 3, 3);
				else if (mParty.XAxis == 163 && mParty.YAxis == 60)
					FindItem(47, 1 << 4, 4, 2);
				else if (mParty.XAxis == 168 && mParty.YAxis == 51)
					FindItem(47, 1 << 5, 5, 3);
				else if (mParty.XAxis == 96 && mParty.YAxis == 59)
					FindItem(47, 1 << 6, 6, 4);
				else if (mParty.XAxis == 98 && mParty.YAxis == 111)
					FindItem(47, 1 << 7, 7, 5);
				else if (mParty.XAxis == 48 && mParty.YAxis == 13)
					FindGold(48, 1, 30_000);
				else if (mParty.XAxis == 98 && mParty.YAxis == 111)
					FindItem(48, 1 << 1, 4, 5);
				else if (mParty.XAxis == 139 && mParty.YAxis == 6)
					FindGold(48, 1 << 2, 25_000);
				else if (mParty.XAxis == 84 && mParty.YAxis == 88)
					FindGold(48, 1 << 3, 20_000);
				else if (mParty.XAxis == 96 && mParty.YAxis == 23)
					FindItem(48, 1 << 4, 8, 5);
				else if (mParty.XAxis == 73 && mParty.YAxis == 77)
					FindGold(48, 1 << 5, 35_000);
				else if (mParty.XAxis == 10 && mParty.YAxis == 70)
					FindItem(48, 1 << 6, 9, 2);
				else if (mParty.XAxis == 192 && mParty.YAxis == 118 && (mParty.Etc[48] & (1 << 7)) == 0)
				{
					Talk(" 일행이 밑을 보자 거기에는 뼈만 남은 기사가 쓰러져 있었다." +
					" 그의 무기와 갑옷은 형편 없이 깨어져 있었지만  그의 크로매틱 방패는  아직 사용할 수 있었다. 일행은 그 방패를 가지기로 하였다.");

					mSpecialEvent = SpecialEventType.GetCromaticShield;
				}
				else if (mParty.XAxis == 62 && mParty.YAxis == 67 && (mParty.Etc[44] & 1) == 0)
				{
					Talk(" 일행이 밑을 보자 거기에는 뼈만 남은 기사가 쓰러져 있었다." +
					" 그의 방패와 갑옷은 형편 없이 깨어져 있었지만  그의 양날 전투 도끼는 아직 사용할 수 있었다. 일행은 그 무기를 가지기로 하였다.");

					mSpecialEvent = SpecialEventType.GetBattleAxe;
				}
				else if (mParty.XAxis == 103 && mParty.YAxis == 9 && (mParty.Etc[44] & (1 << 2)) == 0)
				{
					if ((mParty.Etc[44] & (1 << 3)) > 0)
					{
						Talk($" 일행이 물위에 다다르자  물속에서 검은 광채를 내며  전설의 [color={RGB.LightCyan}]흑요석 갑옷[/color]이 떠올랐다." +
						"  이 갑옷은  현재의 기술로 만들 수 있는 플래티움 재질의 갑옷보다 두배의 방어력을 지닌다고 한다.");

						mSpecialEvent = SpecialEventType.GetObsidianArmor;
					}
				}
				else if (((mParty.XAxis == 103 && mParty.YAxis == 13) || (mParty.XAxis == 104 && mParty.YAxis == 13)) && (mParty.Etc[44] & (1 << 3)) == 0) {
					mEncounterEnemyList.Clear();

					for (var i = 0; i < 5; i++)
						JoinEnemy(33);
					JoinEnemy(35);
					JoinEnemy(38);
					JoinEnemy(44);

					DisplayEnemy();

					Talk($"[color={RGB.LightMagenta}] 잠깐, 우리들은 전설의 갑옷을 지키는 파수꾼들이다. 당신들은 더 이상 들어 갈수 없다. 자 각오해라.[/color]");

					mSpecialEvent = SpecialEventType.BattleGuardOfObsidianArmor;
				}
				else if (mParty.XAxis == 148 && mParty.YAxis == 50 && (mParty.Etc[44] & (1 << 4)) == 0) {
					Talk(" 일행이 앞으로 진행하려 했지만  점액 생물이 일행을 포위했다.");

					mSpecialEvent = SpecialEventType.BattleSlaim;
				}
				else if (mParty.XAxis == 148 && mParty.YAxis == 65 && (mParty.Etc[44] & (1 << 5)) == 0) {
					Talk(" 일행이 동굴 앞에 섰을때 동굴의 입구를 지키는 괴물들이 일행의 앞을 가로 막았다.");

					mSpecialEvent = SpecialEventType.BattleCaveEntrance;
				}
				else if (GetTileInfo(mParty.XAxis, mParty.YAxis) == 52) {
					if (!(mParty.XAxis == 103 && mParty.YAxis == 9) && !(GetTileInfo(mParty.XAxis, mParty.YAxis) != mMapLayer[mParty.XAxis + mParty.YAxis * mMapWidth])) {
						AppendText(" 일행이 물위로 가려고 하자 갑자기 물은 용암으로 바뀌고 말았다.");

						for (var y = 60; y < 119; y++) {
							for (var x = 7; x < 106; x++) {
								if (GetTileInfo(x, y) == 52)
									mMapLayer[x + y * mMapWidth] = 50;
							}

						}
					}
				}
				else if (mParty.XAxis == 183 && mParty.YAxis == 30 && mParty.Etc[10] == 1) {
					AppendText(new string[] {
						$" 아니 ! {mPlayerList[0].Name} 자네가 여기 웬일인가 ?",
						" 정말 오래간 만이네.",
						"",
						$"[color={RGB.Cyan}] 나도 역시 반갑네. 나는 지금 라스트 디치 성주님에게 당신의 신변을 알아오라고 부탁을 받았다네. 자네가 살아 있어서 정말 다행이네.[/color]",
						"",
						" 그런가 ?  성주님이 나를 그렇게 생각해 주시다니 정말 고맙군.  그런 그렇고 이 안에 있는 두개의 동굴 속에는  이미 지하 세계와의 통로가  단절 되어 버렸다네." +
						"  나와 지니어스 기가 그 곳을 탈출 하자마자  지하 세계의 마법사에 의해 입구가 봉쇄된 것 같네.  그래도 한번 탐험에 보는 것도 좋은 경험일테고." +
						" 지금은 그것보다는  나의 안부를 성주님께 알리는게 더 중요 하겠군. 그럼 먼저 가보도록 하게."
					});

					mParty.Etc[10]++;
				}
			}
			else if (mParty.Map == 6)
			{
				if (mParty.XAxis == 18 && (mParty.Etc[29] & 1) == 0)
				{

					InvokeAnimation(AnimationType.LordAhnCall);
				}
				else if (mParty.XAxis == 40 && mParty.YAxis == 78)
				{
					if ((mParty.Etc[29] & (1 << 3)) == 0)
					{
						mParty.Etc[29] |= 1 << 3;

						InvokeAnimation(AnimationType.GetDefaultWeapon);
					}
				}
				else if ((mParty.XAxis == 50 && mParty.YAxis == 11) || (mParty.XAxis == 51 && mParty.YAxis == 11))
				{
					if ((mParty.Etc[49] & (1 << 4)) > 0 || (mParty.Etc[49] & (1 << 5)) > 0)
					{
						if ((mParty.Etc[49] & (1 << 4)) > 0)
						{
							// 전투 시스템 미구현
						}
					}
				}
				else if (mParty.XAxis == 85 && mParty.YAxis == 47)
				{
					AppendText(" 당신은 열쇠를 발견 했다.");
					UpdateTileInfo(86, 47, 44);
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 37 && (mParty.Etc[30] & 1) == 0)
				{
					AppendText(" 당신은 금화 2000 개를 발견했다.");
					mParty.Gold += 2000;
					mParty.Etc[30] |= 1;
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 40 && (mParty.Etc[30] & (1 << 1)) == 0)
				{
					AppendText(" 당신은 100 개의 식량을 발견했다.");
					if (mParty.Food + 100 < 256)
						mParty.Food += 100;
					else
						mParty.Food = 255;

					mParty.Etc[30] |= 1 << 1;
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 41 && (mParty.Etc[30] & (1 << 2)) == 0)
				{
					AppendText(" 당신은 금화 1000 개를 발견했다.");
					mParty.Gold += 1000;

					mParty.Etc[30] |= 1 << 2;
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 42 && (mParty.Etc[30] & (1 << 3)) == 0)
				{
					AppendText(" 당신은 금화 2500 개를 발견했다.");
					mParty.Gold += 2500;

					mParty.Etc[30] |= 1 << 3;
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 43 && (mParty.Etc[30] & (1 << 4)) == 0)
				{
					AppendText(" 당신은 금화 5000 개를 발견했다.");
					mParty.Gold += 5000;

					mParty.Etc[30] |= 1 << 4;
				}
				else if (mParty.XAxis == 81 && mParty.YAxis == 51 && (mParty.Etc[31] & 1) == 0)
				{
					AppendText(" 당신은 금화 5000 개를 발견했다.");
					mParty.Gold += 5000;

					mParty.Etc[31] |= 1;
				}
				else if (mParty.XAxis == 83 && mParty.YAxis == 51 && (mParty.Etc[31] & (1 << 1)) == 0)
				{
					AppendText(" 당신은 금화 5000 개를 발견했다.");
					mParty.Gold += 5000;

					mParty.Etc[30] |= 1 << 1;
				}
				else if (mParty.XAxis == 88 && mParty.YAxis == 51 && (mParty.Etc[31] & (1 << 2)) == 0)
				{
					AppendText(" 당신은 금화 5000 개를 발견했다.");
					mParty.Gold += 5000;

					mParty.Etc[30] |= 1 << 2;
				}
				else if (mParty.XAxis == 90 && mParty.YAxis == 51 && (mParty.Etc[31] & (1 << 3)) == 0)
				{
					AppendText(" 당신은 금화 5000 개를 발견했다.");
					mParty.Gold += 5000;

					mParty.Etc[30] |= 1 << 3;
				}
				else if (mParty.YAxis == 94)
				{
					ShowExitMenu();
				}
			}
			else if (mParty.Map == 7)
			{
				if (mParty.XAxis == 49)
					AppendText(" 여기는  두번째의 대륙으로 이동하는  게이트입니다. 하지만 당신은 그 곳에서 하여야할 임무가 없습니다. 다시 돌아가 주십시오.");
				else if (mParty.XAxis == 29 || mParty.XAxis == 31)
				{
					UpdateTileInfo(30, mParty.YAxis, 45);
					triggered = false;
				}
				else if (mParty.YAxis == 69)
				{
					ShowExitMenu();
				}
			}
			else if (mParty.Map == 10)
			{
				if ((mParty.XAxis == 24 && mParty.YAxis == 42) || (mParty.XAxis == 25 && mParty.YAxis == 42))
				{
					if (mParty.Etc[9] == 1)
					{
						mParty.Etc[9] = 2;
						triggered = false;
					}
					else if (mParty.Etc[9] == 3)
					{
						Talk(new string[] {
							" 당신이 메너스에 들어오자 마자  어떤 사람의 시체가 놓여져 있었다." +
							" 그 시체는 형체를 알아볼 수 없을 정도로 피 투성이가 되어있었고 그의 등에는  커다란 독 화살이 예리하게 관통해 있었다.",
							" 동굴 입구에서는 때마침  초승달이 비치고 있었고 그 달빛은 그 시체에서 흘러서 나와 고인피에 비쳐서 붉게 물들여 졌다.",
							" 순간 당신은 알비레오의 예언의 구절이 떠 올랐다. 그것은 바로 이런 것이었다.",
							$"[color={RGB.LightCyan}] \" 메너스의 달이 붉게 물들때  어둠의 영혼이 나타나 세계의 종말을 예고한다. \"[/color]"
						});

						mSpecialEvent = SpecialEventType.SeeDeadBody;
					}
					else
						triggered = false;
				}
				else if (22 <= mParty.XAxis && mParty.XAxis <= 27 && 8 <= mParty.YAxis && mParty.YAxis <= 11)
				{
					Talk(" 일행이 절벽에 서자마자  위압적인 힘이 일행을 끌어 당기기 시작했고 결국 일행은 그 힘을 버티지 못하고  의문의 구멍 속으로 빠져 들고 말았다.");

					mParty.Map = 3;
					mParty.XAxis = 50;
					mParty.YAxis = 50;

					await RefreshGame();

					// 추가 구현 필요
					//mSpecialEvent = SpecialEventType.ManHoleInMenace;
				}
				else if (10 <= mParty.XAxis && mParty.XAxis <= 14 && 9 <= mParty.YAxis && mParty.YAxis <= 11)
				{
					Talk(" 일행은 절벽 앞으로 섰고,  저번과 같이 어떤 강한 힘에 의해서 구멍 속으로 빨려 들어갔다.");

					mParty.Map = 3;
					mParty.XAxis = 13;
					mParty.YAxis = 23;

					await RefreshGame();

					// 추가 구현 필요
					//mSpecialEvent = SpecialEventType.ManHoleInMenace2;
				}
				else if (mParty.YAxis == 44)
					ShowExitMenu();
			}

			return triggered;
		}

		private void HealOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0 || whomPlayer.Poison > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.HP >= whomPlayer.Endurance * whomPlayer.Level * 10)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 치료할 필요가 없습니다.");
			}
			else
			{
				var needSP = 2 * player.Level;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.HP += needSP * 10;
					if (whomPlayer.HP > whomPlayer.Level * whomPlayer.Endurance)
						whomPlayer.HP = whomPlayer.Level * whomPlayer.Endurance;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.NameSubjectJosa} 치료되어 졌습니다.[/color]");
				}
			}
		}

		private void CureOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0 || whomPlayer.Unconscious > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 독이 치료될 상태가 아닙니다.");
			}
			else if (whomPlayer.Poison == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 독에 걸리지 않았습니다.");
			}
			else if (player.SP < 15)
			{
				if (mParty.Etc[5] == 0)
					ShowNotEnoughSP(cureResult);

			}
			else
			{
				player.SP -= 15;
				DisplaySP();

				whomPlayer.Poison = 0;

				cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}의 독은 제거 되었습니다.[/color]");
			}
		}

		private void ConsciousOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead > 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 의식이 돌아올 상태가 아닙니다.");
			}
			else if (whomPlayer.Unconscious == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 의식불명이 아닙니다.");
			}
			else
			{
				var needSP = 10 * whomPlayer.Unconscious;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Unconscious = 0;
					if (whomPlayer.HP <= 0)
						whomPlayer.HP = 1;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}(은)는 의식을 되찾았습니다.[/color]");
				}
			}
		}

		private void RevitalizeOne(Lore player, Lore whomPlayer, List<string> cureResult)
		{
			if (whomPlayer.Dead == 0)
			{
				if (mParty.Etc[5] == 0)
					cureResult.Add($"{whomPlayer.NameSubjectJosa} 아직 살아 있습니다.");
			}
			else
			{
				var needSP = player.Dead * 30;
				if (player.SP < needSP)
				{
					if (mParty.Etc[5] == 0)
						ShowNotEnoughSP(cureResult);
				}
				else
				{
					player.SP -= needSP;
					DisplaySP();

					whomPlayer.Dead = 0;
					if (whomPlayer.Unconscious > whomPlayer.Endurance * whomPlayer.Level)
						whomPlayer.Unconscious = whomPlayer.Endurance * whomPlayer.Level;

					if (whomPlayer.Unconscious == 0)
						whomPlayer.Unconscious = 1;

					cureResult.Add($"[color={RGB.White}]{whomPlayer.Name}(은)는 다시 생명을 얻었습니다.[/color]");

				}
			}
		}

		private void HealAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				HealOne(player, whomPlayer, cureResult);
			});
		}

		private void CureAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				CureOne(player, whomPlayer, cureResult);
			});
		}

		private void ConsciousAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				ConsciousOne(player, whomPlayer, cureResult);
			});
		}

		private void RevitalizeAll(Lore player, List<string> cureResult)
		{
			mPlayerList.ForEach(delegate (Lore whomPlayer)
			{
				RevitalizeOne(player, whomPlayer, cureResult);
			});
		}

		private void ShowNotEnoughSP(List<string> result = null)
		{
			var message = "그러나, 마법 지수가 충분하지 않습니다.";
			if (result == null)
				AppendText(message);
			else
				result.Add(message);
		}

		//		private void ShowNotEnoughESP(List<string> result = null)
		//		{
		//			var message = "ESP 지수가 충분하지 않습니다.";
		//			if (result == null)
		//			{
		//				AppendText(new string[] { message }, true);
		//				ContinueText.Visibility = Visibility.Visible;
		//			}
		//			else
		//				result.Add(message);
		//		}

		private void ShowNotEnoughMoney(SpecialEventType specialEvent)
		{
			var noMoneyStr = "당신은 충분한 돈이 없습니다.";
			if (specialEvent == SpecialEventType.None)
				AppendText(noMoneyStr);
			else {
				Talk(noMoneyStr);
				mSpecialEvent = specialEvent;
			}
		}

		//		private void ShowThankyou()
		//		{
		//			AppendText(new string[] { "매우 고맙습니다." }, true);
		//			ContinueText.Visibility = Visibility.Visible;
		//		}

		//		private void ShowNoThanks()
		//		{
		//			AppendText(new string[] { "당신이 바란다면 ..." });
		//			ContinueText.Visibility = Visibility.Visible;
		//		}

		//		private string GetGenderData(Lore player)
		//		{
		//			if (player.Gender == "male")
		//				return "그";
		//			else
		//				return "그녀";
		//		}

		private void ShowCharacterMenu(MenuMode menuMode, bool includeAssistPlayer = true)
		{
			AppendText($"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]", true);

			var menuStr = new string[mPlayerList.Count + (mAssistPlayer != null ? 1 : 0)];
			for (var i = 0; i < mPlayerList.Count; i++)
				menuStr[i] = mPlayerList[i].Name;

			if (mAssistPlayer != null)
				menuStr[menuStr.Length - 1] = mAssistPlayer.Name;

			ShowMenu(menuMode, menuStr);
		}

		//		private void JoinMember(Lore player)
		//		{
		//			if (mPlayerList.Count < 6)
		//			{
		//				mPlayerList.Add(player);
		//				DisplayPlayerInfo();

		//				if (mMemberLeftTile > 0)
		//					LeaveMemberPosition();
		//			}
		//			else
		//			{
		//				AppendText(new string[] { "교체 시킬 인물은 누구입니까 ?" });
		//				var memberNames = new string[mPlayerList.Count - 1];
		//				for (var i = 1; i < mPlayerList.Count; i++)
		//					memberNames[i - 1] = mPlayerList[i].Name;

		//				mReserveMember = player;

		//				ShowMenu(MenuMode.SwapMember, memberNames);
		//			}
		//		}

		//		private void LeaveMemberPosition()
		//		{
		//			if (mMemberX >= 0 && mMemberY >= 0)
		//				mMapLayer[mMemberX + mMapWidth * mMemberY] = mMemberLeftTile;

		//			mMemberLeftTile = 0;
		//			mMemberX = -1;
		//			mMemberY = -1;
		//		}

		private Color GetColor(string color) {
			return Color.FromArgb(0xff, Convert.ToByte(color.Substring(0, 2), 16), Convert.ToByte(color.Substring(2, 2), 16), Convert.ToByte(color.Substring(4, 2), 16));
		}

		private void FocusMenuItem()
		{
			for (var i = 0; i < mMenuCount; i++)
			{
				if (i == mMenuFocusID)
					mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.White));
				else
					mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.LightGray));
			}
		}

		private void ShowMenu(MenuMode menuMode, string[] menuItem, int focusID = 0)
		{
			mMenuMode = menuMode;
			mMenuCount = menuItem.Length;
			mMenuFocusID = focusID;

			for (var i = 0; i < mMenuList.Count; i++)
			{
				if (i < mMenuCount)
				{
					mMenuList[i].Text = menuItem[i];
					mMenuList[i].Visibility = Visibility.Visible;

					if (i == focusID)
						mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.White));
					else
						mMenuList[i].Foreground = new SolidColorBrush(GetColor(RGB.LightGray));
				}
				else
					mMenuList[i].Visibility = Visibility.Collapsed;
			}


			FocusMenuItem();
		}

		private void ShowSpinner(SpinnerType spinnerType, Tuple<string, int>[] items, int defaultId)
		{
			mSpinnerType = spinnerType;

			mSpinnerItems = items;
			mSpinnerID = defaultId;

			AppendText(SpinnerText, mSpinnerItems[defaultId].Item1);
			SpinnerText.Visibility = Visibility.Visible;
		}

		//		private void ShowMenu(MenuMode menuMode, List<Tuple<string, Color>> menuItem)
		//		{
		//			mMenuMode = menuMode;
		//			mMenuCount = menuItem.Count;
		//			mMenuFocusID = 0;

		//			for (var i = 0; i < mMenuList.Count; i++)
		//			{
		//				if (i < mMenuCount)
		//				{
		//					mMenuList[i].Text = menuItem[i].Item1;
		//					mMenuList[i].Foreground = new SolidColorBrush(menuItem[i].Item2);
		//					mMenuList[i].Visibility = Visibility.Visible;
		//				}
		//				else
		//					mMenuList[i].Visibility = Visibility.Collapsed;
		//			}


		//			FocusMenuItem();
		//		}

		private MenuMode HideMenu()
		{
			var menuMode = mMenuMode;
			mMenuMode = MenuMode.None;

			for (var i = 0; i < mMenuCount; i++)
			{
				mMenuList[i].Visibility = Visibility.Collapsed;
			}

			return menuMode;
		}

		private bool AppendText(string str, bool append = false)
		{
			return AppendText(DialogText, str, append);
		}

		private void AppendText(string[] text, bool append = false)
		{
			var added = true;
			for (var i = 0; i < text.Length; i++)
			{
				if (added)
				{
					if (i == 0)
						added = AppendText(text[i], append);
					else
						added = AppendText(text[i], true);
				}

				if (!added)
					mRemainDialog.Add(text[i]);
			}
		}

		private bool AppendText(RichTextBlock textBlock, string str, bool append = false)
		{

			if (PartyInfoPanel.Visibility == Visibility.Visible)
				PartyInfoPanel.Visibility = Visibility.Collapsed;

			if (StatPanel.Visibility == Visibility.Visible)
				StatPanel.Visibility = Visibility.Collapsed;

			if (StatHealthPanel.Visibility == Visibility.Visible)
				StatHealthPanel.Visibility = Visibility.Collapsed;

			if (DialogText.Visibility == Visibility.Collapsed)
				DialogText.Visibility = Visibility.Visible;

			var totalLen = 0;

			if (append)
			{
				foreach (Paragraph prevParagraph in textBlock.Blocks)
				{
					foreach (Run prevRun in prevParagraph.Inlines)
					{
						totalLen += prevRun.Text.Length;
					}
				}
			}
			else
			{
				textBlock.TextHighlighters.Clear();
				textBlock.Blocks.Clear();
			}

			var highlighters = new List<TextHighlighter>();
			var paragraph = new Paragraph();
			textBlock.Blocks.Add(paragraph);

			var startIdx = 0;
			while ((startIdx = str.IndexOf("[", startIdx)) >= 0)
			{
				if (startIdx < str.Length - 1 && str[startIdx + 1] == '[')
				{
					str = str.Remove(startIdx, 1);
					startIdx++;
					continue;
				}

				var preRun = new Run
				{
					Text = str.Substring(0, startIdx)
				};

				paragraph.Inlines.Add(preRun);
				var preTextHighlighter = new TextHighlighter()
				{
					Foreground = new SolidColorBrush(GetColor(RGB.LightGray)),
					Background = new SolidColorBrush(Colors.Transparent),
					Ranges = { new TextRange()
									{
										StartIndex = totalLen,
										Length = preRun.Text.Length
									}
								}
				};

				highlighters.Add(preTextHighlighter);
				textBlock.TextHighlighters.Add(preTextHighlighter);

				totalLen += preRun.Text.Length;
				str = str.Substring(startIdx + 1);

				startIdx = str.IndexOf("]");
				if (startIdx < 0)
					break;

				var tag = str.Substring(0, startIdx);
				str = str.Substring(startIdx + 1);
				var tagData = tag.Split("=");

				var endTag = $"[/{tagData[0]}]";
				startIdx = str.IndexOf(endTag);

				if (startIdx < 0)
					break;


				if (tagData[0] == "color" && tagData.Length > 1 && tagData[1].Length == 6)
				{
					var tagRun = new Run
					{
						Text = str.Substring(0, startIdx).Replace("[[", "[")
					};

					paragraph.Inlines.Add(tagRun);

					var textHighlighter = new TextHighlighter()
					{
						Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(tagData[1].Substring(0, 2), 16), Convert.ToByte(tagData[1].Substring(2, 2), 16), Convert.ToByte(tagData[1].Substring(4, 2), 16))),
						Background = new SolidColorBrush(Colors.Transparent),
						Ranges = { new TextRange()
									{
										StartIndex = totalLen,
										Length = tagRun.Text.Length
									}
								}
					};

					highlighters.Add(textHighlighter);
					textBlock.TextHighlighters.Add(textHighlighter);

					totalLen += tagRun.Text.Length;
				}

				str = str.Substring(startIdx + endTag.Length);
				startIdx = 0;
			}

			var run = new Run
			{
				Text = str
			};

			paragraph.Inlines.Add(run);
			var postTextHighlighter = new TextHighlighter()
			{
				Foreground = new SolidColorBrush(Color.FromArgb(0xff, Convert.ToByte(RGB.LightGray.Substring(0, 2), 16), Convert.ToByte(RGB.LightGray.Substring(2, 2), 16), Convert.ToByte(RGB.LightGray.Substring(4, 2), 16))),
				Background = new SolidColorBrush(Colors.Transparent),
				Ranges = { new TextRange()
								{
									StartIndex = totalLen,
									Length = run.Text.Length
								}
							}
			};

			highlighters.Add(postTextHighlighter);
			textBlock.TextHighlighters.Add(postTextHighlighter);

			totalLen += run.Text.Length;


			textBlock.UpdateLayout();
			//var DialogText.Tag

			var lineHeight = 0d;
			if (textBlock.Blocks.Count > 0)
			{
				var startRect = DialogText.Blocks[0].ContentStart.GetCharacterRect(LogicalDirection.Forward);
				lineHeight = startRect.Height;
			}

			var lineCount = lineHeight == 0 ? 0 : (int)Math.Ceiling(DialogText.ActualHeight / lineHeight);

			if (lineCount > DIALOG_MAX_LINES)
			{
				textBlock.Blocks.Remove(paragraph);
				foreach (var highlighter in highlighters)
					textBlock.TextHighlighters.Remove(highlighter);

				return false;
			}
			else
				return true;
		}

		private void Talk(string dialog)
		{
			Talk(new string[] { dialog });
		}

		private void Talk(string[] dialog)
		{
			AppendText(dialog);
			ContinueText.Visibility = Visibility.Visible;
		}

		private void TalkMode(int moveX, int moveY, VirtualKey key = VirtualKey.None)
		{
			void ShowClassTrainingMenu()
			{
				AppendText("어떤 일을 원하십니까 ?");

				ShowMenu(MenuMode.TrainingCenter, new string[] {
					"전투사 계열의 기술을 습득",
					"마법사 계열의 능력을 습득",
					"전투사 계열의 계급을 바꿈",
					"마법사 계열의 계급을 바꿈"
				});
			}

			void ShowHospitalMenu()
			{
				Talk($"[color={RGB.White}]여기는 병원입니다.[/color]");

				mSpecialEvent = SpecialEventType.CureComplete;
			}

			void ShowGroceryMenu()
			{
				AppendText(new string[] {
					$"[color={RGB.White}]여기는 식료품점 입니다.[/color]",
					$"[color={RGB.White}]몇개를 원하십니까 ?[/color]",
				});

				ShowMenu(MenuMode.ChooseFoodAmount, new string[] {
					"10인분 : 금 100개",
					"20인분 : 금 200개",
					"30인분 : 금 300개",
					"40인분 : 금 400개",
					"50인분 : 금 500개"
				});
			}

			if (mParty.Map == 6)
			{
				if ((moveX == 20 && moveY == 11) || (moveX == 24 && moveY == 12))
					ShowClassTrainingMenu();
				else if ((moveX == 85 && moveY == 11) || (moveX == 86 && moveY == 13))
					ShowHospitalMenu();
				else if ((moveX == 13 && moveY == 72) || (moveX == 7 && moveY == 70) || (moveX == 13 && moveY == 68))
					ShowWeaponShopMenu();
				else if ((moveX == 86 && moveY == 72) || (moveX == 90 && moveY == 64) || (moveX == 93 && moveY == 67))
					ShowGroceryMenu();
				else if (moveX == 23 && moveY == 49)
				{
					if (mParty.Etc[9] == 0)
						AppendText("어서 로드 안 님에게 가보도록 하게.");
					else
					{
						AppendText(new string[] {
							$" 이번에도 큰일을 맞게 됐군, {mPlayerList[0].Name}.",
							" 이번에도 분명 당신은 해낼 수 있으리라고 믿네.  과거의 영웅은  이제 잊혀져 가지만 다시 일어서는 날이 바로 지금인것 같네." +
							" 이번 일만 성공하면  다시 그때의 영광을 찾을수 있을 걸세. 자네의 성공을 빌어 주겠네."
						});
					}
				}
				else if (moveX == 23 && moveY == 53)
					AppendText(" 로드 안 님이 당신을 부르셨다면서요. 시대의 영웅이었던  당신처럼 되기 위해  저도 열심히 수련을 하고 있습니다.");
				else if (moveX == 18 && moveY == 52)
					AppendText($" 이 세계의 창시자는 [color={RGB.LightGreen}]안 영기[/color]님 이시며, 그는 위대한 프로그래머 입니다.");
				else if (moveX == 12 && moveY == 54)
					AppendText($" 지금 어떤 예언서가 발견되어  주민들의 관심을 끌고 있더군요.");
				else if ((moveX == 12 && moveY == 26) || (moveX == 17 && moveY == 26))
				{
					string message;
					if (mRand.Next(2) == 0)
						message = $" 거기 {mPlayerList[0].GenderStr}분 어서 오십시오.";
					else
						message = "위스키에서 칵테일까지 마음껏 선택하십시오.";

					AppendText(new string[] {
						" 어서 오십시오. 여기는 로어 주점입니다.",
						message
					});
				}
				else if (moveX == 20 && moveY == 35)
					AppendText($" 으~~~ 취한다.");
				else if (moveX == 17 && moveY == 37)
					AppendText($" 예언서 ?  뭐가 예언서야 ! 그런걸 믿다니 우리가 무슨 어린애 인가.");
				else if (moveX == 14 && moveY == 34)
					AppendText($" 큰일났군. 벌써 내 친구도 두명이나 메너스에서 희생되었는데...");
				else if (moveX == 12 && moveY == 31)
				{
					AppendText($" 타임 워커인 알비레오를 전에 한번 본적이 있지.  그는 예언서 하나만 남겨 놓고는 다시 시간 속으로 여행을 떠났다네." +
					" 아무도 그가 어느 시대의 사람인지 아니면 이 차원의 사람인지도 모른다네." +
					"  나도 그가 예언서를 전해주기 위해 로어 대륙에 왔을때 잠시 본 적이 있었는데 정말 대단한 분이셨지. 아마도 고대의 마법사 레드 안타레스에 필적하는 실력이더군");
				}
				else if (moveX == 17 && moveY == 32)
					AppendText($" 흑흑흑~~~ 저의 오빠가 메너스에서 죽었어요. 하지만 아무도 원인 조차 알 수 없다고 하더군요.");
				else if (moveX == 9 && moveY == 29)
					AppendText($" 당신께서  또 새로운 모험을 하신다고 들었는데  옛날의 동료들은 모두 흩어져 있어서 다시 모으셔야 되겠군요.");
				else if (moveX == 71 && moveY == 77)
				{
					AppendText(new string[] {
						$" 과거의 영웅이었던 {mPlayerList[0].Name}님.",
						" 오늘도 역시  베라트릭스님의 무덤에  오셨군요. 아참 !  전에 어떤 전사분께서 베라트릭스님의 무덤에 들리고는  거기에 메모를 남겨 두었더군요." +
						" 이름은 지니어스 기라고 하던데, 옛날에 들어본 이름 같기도 하고..."
					});
				}
				else if (moveX == 66 && moveY == 75)
				{
					Talk(new string[] {
						"묘비에 쓰여 있기를 ...",
						"",
						"",
						$"[color={RGB.LightCyan}]          여기는 베라트릭스의 묘[/color]",
						$"[color={RGB.White}]     772 년 11 월 27 일 여기에 잠들다.[/color]",
						"",
						"",
						"",
						"",
						"",
						" 자세히 살펴 보니  묘비 밑에 어떤 쪽지가 있었다."
					});

					mSpecialEvent = SpecialEventType.ViewGeniusKieLetter;
				}
				else if (moveX == 49 && moveY == 10)
					AppendText(" 이 안에 갇혀있는 사람들에게는 일체 면회가 허용되지 않습니다. 나가 주십시오.");
				else if (moveX == 52 && moveY == 10)
				{
					AppendText(new string[] {
						" 여기는 로드 안의 체제에 대해서  깊은 반감을 가지고 있는 자들을 수용하고 있습니다.",
						" 아마 그들은 죽기전에는 이곳을 나올수 없을겁니다."
					});
				}
				else if (moveX == 40 && moveY == 9)
				{
					Talk(" 나는 이곳의 기사로서  이 세계의 모든 대륙을 탐험하고 돌아왔었습니다." +
					" 내가 마지막 대륙을 돌았을때  나는 새로운 존재를 발견했습니다." +
					" 그는 바로 예전까지도 로드 안과 대립하던 에인션트 이블이라는 존재였습니다." +
					" 지금 우리의 성에서는 철저하게 배격하도록 어릴때부터 가르침 받아온 그 에인션트 이블이었습니다." +
					"  하지만 그곳에서 본 그는 우리가 알고있는 그와는 전혀 다른 인간미를 가진  말 그대로  신과같은 존재였습니다." +
					"  내가 그의 신앙 아래 있는 어느 도시를 돌면서 내가 느낀것은 정말 로드 안에게서는 찾아볼수가 없는 그런 자애와 따뜻한 정이었습니다." +
					"  그리고 여태껏 내가 알고 있는 그에 대한 지식이  정말 잘못되었다는 것과  이런 사실을 다른 사람에게도 알려주고 싶다는 이유로  그의 사상을 퍼뜨리다 이렇게 잡히게 된것입니다.");

					mSpecialEvent = SpecialEventType.TalkPrisoner;
				}
				else if (moveX == 39 && moveY == 14)
				{
					AppendText(" 히히히... 위대한 용사님. 낄낄낄.. 내가 당신들의 일행에 끼이면 안될까요 ? 우히히히..");

					ShowMenu(MenuMode.JoinMadJoe, new string[] {
						"그렇다면 당신을 받아들이지요",
						"당신은 이곳에 그냥 있는게 낫겠소"
					});
				}
				else if (moveX == 62 && moveY == 9 && (mParty.Etc[49] & (1 << 5)) == 0)
				{
					AppendText(new string[] {
						" 내가 몇년 전에 황금 방패를 숨겨 놓았던  메너스에 새로운 일들이 생겼다는게 사실이오 ?",
						" 내가 저번에는 용기가 없어서 당신과 함께 동행하자는 말을 못했지만 이번에는 한번 부탁하겠소.",
						" 나는 비록 도둑질을 해서 여기에 들어왔지만 암살자로서의 임무도 잘 해낼거요."
					});

					ShowMenu(MenuMode.JoinMercury, new string[] {
						"승락하겠소, 그럼 잘 부탁하오",
						"죄수를 탈출 시키면 나 역시 힘들게 되오"
					});
				}
				else if (moveX == 59 && moveY == 14)
					AppendText(" 당신들에게 경고해 두겠는데 건너편 방에 있는 조는 오랜 수감생활 끝에  미쳐 버리고 말았소.  그의 말에 속아서 당신네 일행에 참가시키는 그런 실수는 하지마시오.");
				else if ((moveX == 41 && moveY == 77) || (moveX == 41 && moveY == 79))
				{
					if ((mParty.Etc[29] & (1 << 3)) == 0)
						AppendText(" 로드 안 님의 명령에 의해서  당신들에게 한가지의 무기를 드리겠습니다.  들어가셔서 무기를 선택해 주십시오.");
					else
						AppendText(" 여기서 가져가신 무기를 잘 사용하셔서 임무를 수행하는데 잘 활용 하십시오.");
				}
				else if (moveX == 78 && moveY == 46)
					AppendText(" 우리 은행은 철저하게 보안 되고 있습니다.");
				else if (moveX == 78 && moveY == 48)
					AppendText(" 손님께서는 예금 창구의 안쪽으로는 들어가시면 안됩니다. 그 때의 일은 책임 질 수 없습니다.");
				else if (moveX == 84 && moveY == 47)
					AppendText(" 지금은 민심이 어수선해서 은행의 입출금을 금지하고 있습니다. 양해하여 주십시오.");
				else if (moveX == 89 && moveY == 46)
				{
					Talk(new string[] {
						" 누구냐 !  앗 은행 강도다.",
						" 경비병 !  경비병 !"
					});

					mSpecialEvent = SpecialEventType.BattleBankGuard;
				}
				else if (moveX == 88 && moveY == 22 && (mParty.Etc[49] & 1) == 0)
				{
					AppendText(new string[] {
						$" 오, {mPlayerList[0].Name}!",
						" 자네는 날 기억하겠지. 나는 헤라클레스일세. 자네와 네크로만서를 물리치던 때가  생각나는군." +
						" 그래, 이번에도 중대한 일이 발생했다고들 하던데 그게 사실인가 ?  사실이라면 이번에도 나를 모험에 참가 시켜주게."
					});

					ShowMenu(MenuMode.JoinHercules, new string[] {
						"나 역시 자네의 도움이 필요했다네",
						"자네까지 나설 정도로 심각한일은 아니네"
					});
				}
				else if (moveX == 20 && moveY == 32 && (mParty.Etc[49] & (1 << 2)) == 0)
				{
					AppendText(new string[] {
						$" 안녕하신가 ? {mPlayerList[0].Name}.",
						" 설마 날 잊었다고는 하지 않겠지." +
						"  지금은 사건이 없어서  한가하게 이런 일을 하고 있지만 간혹 여기를 들리는 사람중에는 이 타이탄님을 기억하는 사람이 있다네." +
						"  요새는 세상이 너무 조용해서 이런일은 나하고는 정말 안 어울린다네.  자네 모습을 보니 모험을 떠나려는 것 같은데 나도 끼이면 안 되겠나 ?"
					});

					ShowMenu(MenuMode.JoinTitan, new string[] {
						"물론 나는 환영하지",
						"별일 아니라서 자네는 재미 없을 걸세"
					});
				}
				else if (moveX == 8 && moveY == 63 && (mParty.Etc[49] & (1 << 1)) == 0)
				{
					AppendText(new string[] {
						$" 오래간 만이군 {mPlayerList[0].Name}.",
						" 2-3 년전에 자네와 함께  모험을 떠났던 것이 엊그제 같은데 벌써 세월은 이렇게 지났군. 그간 생활이 어떠했나 ?" +
						" 그건 그렇고, 또 이 곳에 무슨일이 생겼다지 ? 이번에도 자네에게 나의 마법의 힘을 빌려 주고 싶은데 자네는 어떻게 생각하나 ?"
					});

					ShowMenu(MenuMode.JoinMerlin, new string[] {
						"머린, 자네의 힘이 필요하네",
						"글쎄.. 꼭 그럴것 까지는 없지"
					});
				}
				else if (moveX == 87 && moveY == 37 && (mParty.Etc[49] & (1 << 3)) == 0)
				{
					AppendText(new string[] {
						$" 요새는 좀처럼 볼 기회가 없군요. {mPlayerList[0].Name}.",
						" 나는 네크로만서와의 결전이 끝난후에 컨져러의 수업을 쌓고 있었죠. 지금은 초보적인 수준밖에는 안되지만." +
						" 당신이 또 새로운 임무를 받았다고들 하던데 사실인가요 ?  그렇다면 저도 그 임무에 빠질수가 없지요." +
						"  몇년전 그때처럼 당신과 함께 싸우고 싶군요."
					});

					ShowMenu(MenuMode.JoinBetelgeuse, new string[] {
						"베텔규스, 자네라면 환영하지",
						"글쎄.. 임무가 하찮은 거라서.."
					});
				}
				else if (moveX == 50 && moveY == 86)
				{
					if ((mParty.Etc[29] & (1 << 1)) == 0)
					{
						for (var x = 48; x < 53; x++)
							UpdateTileInfo(x, 87, 44);

						AppendText($"난 당신을 믿소, {mPlayerList[0].Name}.");
					}
					else
						AppendText($"힘내시오, {mPlayerList[0].Name}.");
				}
				else if (moveX == 84 && moveY == 54)
				{
					AppendText(new string[] {
						$" 살려 주십시오. {mPlayerList[0].Name}님.",
						" 아마 조금만 있으면 다크 메이지가 이곳을 폐허로 만들겁니다."
					});
				}
				else if (moveX == 71 && moveY == 72)
					AppendText(" 나는 어릴적에  할아버지로 부터 지하 세계의 사람들에 대해 들은 적이 있는데  지금은 기억이 잘 나지 않는군요.");
				else if (moveX == 50 && moveY == 13)
					AppendText(" 당신이 이 세계를 네크로만서로 부터 구한 후에는 다른 대륙도 원래의 모습을 찾았습니다.");
				else if (moveX == 89 && moveY == 58)
				{
					AppendText(new string[] {
						$" {mPlayerList[0].Name}님.",
						" 만나게 되어서 기쁩니다. 이번 모험을 하는데 약간의 도움이 되었으면 합니다. 이 밭에 있는 채소를 조금 드릴테니 식량으로 사용해 주십시오."
					});

					if ((mParty.Etc[29] & (1 << 2)) == 0)
					{
						AppendText(new string[] {
							"",
							$"[color={RGB.LightCyan}] 당신은 그녀에게 약간의 식량을 받았다.[/color]"
						}, true);

						if (mParty.Food + 20 < 256)
							mParty.Food += 20;
						else
							mParty.Food = 255;

						mParty.Etc[29] |= 1 << 2;
					}
				}
				else if (moveX == 91 && moveY == 79)
					AppendText(" 알비레오님의 예언서에 의하면 다크 메이지가 이 세계를 멸망 시킨다고 하던데 정말 큰 일이군요.");
				else if (moveX == 90 && moveY == 80)
				{
					AppendText(" 얼마 전에 옆집에 살던 분이 메너스 광산에서 일하던 중 의문의 살인을 당했어요." +
					" 시체를 본 사람에 의하면  날카로운 독 화살에 맞아서 즉사했다고 하던데요.");
				}
				else if (moveX == 57 && moveY == 73)
					AppendText(" 이번에도 우리들은 당신만 믿고 있습니다.");
				else if (moveX == 62 && moveY == 26)
					AppendText(" 당신과 함께 싸웠던  전투승 레굴루스는 로어성을 떠나  대륙의 북동쪽 섬의 오두막에 산다고 합니다.");
				else if (moveX == 50 && moveY == 71)
					AppendText(" 당신과 함께 싸웠던 폴라리스는 라스트디치성 군주의 수석 호위관이 되어 있더군요.");
				else if (moveX == 49 && moveY == 50)
					AppendText(" 타이탄님은 당신과의 모험이 끝난후  로어 주점의 경비와 치안을 맡고 있더군요.");
				else if (moveX == 51 && moveY == 50)
					AppendText(" 이번의 모험도 역시 성공하기를 빕니다.");
				else if (46 <= moveX && moveX <= 54 && 29 <= moveY && moveY <= 37)
				{
					if (mParty.Etc[9] == 0)
						AppendText(" 성주님께서 당신을 찾으십니다.");
					else
						AppendText(" 이번의 모험도 역시 성공하기를 빕니다.");
				}
				else if (moveX == 50 && moveY == 27)
				{
					if (mParty.Etc[9] == 0)
					{
						Talk(new string[] {
							$" {mPlayerList[0].Name}.",
							" 네크로만서를 물리친후 잘 쉬었는가 ?  또 자네를 부를일이 있다는 그 자체는 그 만큼 세상이 혼란스러워 졌다는 증거일세. 그럼 말을 계속 해 보도록 하겠네."
						});

						mSpecialEvent = SpecialEventType.MeetLordAhn;
					}
					else if (mParty.Etc[9] == 1)
						AppendText(" 메너스에서 일어난  의문의 살인에 대한 원인과 다크 메이지의 정보도 알아오게. 자네만 믿겠네.");
					else if (mParty.Etc[9] == 2)
					{
						AppendText(new string[] {
							$" 돌아왔군, {mPlayerList[0].Name}.",
							" 메너스에서 알아낸것이 없다고 ? 이거 큰일이네. 자네가 이곳으로 돌아오던 시간에 또 한명이 메너스에서 살해 당했다네." +
							"  그리고 주민들의 여론은 더욱 악화 되었다는 것은 자명한 사실 이네. 자시 자네에게 부탁하겠네.  다시 그곳으로 가서 살인에 대한 단서를 찾아오게.",
							" 꼭, 부탁하네."
						});

						mParty.Etc[9]++;
					}
					else if (mParty.Etc[9] == 3)
						AppendText(" 다시 메너스로 가서 살인에 대한 단서를 찾아오도록 하게. 부탁하네.");
					else if (mParty.Etc[9] == 4)
					{
						Talk(new string[] {
							$" 정말 잘해냈군 {mPlayerList[0].Name}.",
							" 역시 과거의 영웅은 현재의 영웅이군. 미궁에 빠졌던  살인 사건의 원인을 알아내고  그들을 제거 해내다니. 당신에게 그 댓가로 약간의 상금을 주도록 하겠네.",
							$"[color={RGB.LightCyan}] [[ 경험치 + 0 ] [[ 황금 + 5000 ][/color]"
						});

						mParty.Gold += 5000;

						mSpecialEvent = SpecialEventType.MeetLordAhn3;
					}
					else if (mParty.Etc[9] == 5)
						AppendText(" 자네는 라스트 디치성에서의 일을  모두 끝내고 다시 돌아 오도록 하게.");
					else if (mParty.Etc[9] == 6) {
						Talk(new string[] {
							" 라스트 디치에서의 임무를  잘 수행했다고 들었네. 어디, 자네가 지금 들고 있는 석판을 좀 볼까 ? 음.......",
							$" 그러면 [color={RGB.White}]흉성의 증거[/color]에서 가져온 것 부터 해독해 보도록하지."
						});

						mSpecialEvent = SpecialEventType.MeetLordAhn6;
					}
					else if (mParty.Etc[9] == 7) {
						var eclipseDay = mParty.Day + 20;
						var eclipseYear = mParty.Year;

						if (eclipseDay > 360) {
							eclipseYear++;
							eclipseDay %= 360;
						}

						AppendText(new string[] {
							$" 어서오게, {mPlayerList[0].Name}.",
							" 어제 해독하지 못 했던 석판에서 아주 중요한 것을 알아 내었다네. 에인션트 이블이 물론 해독해 주었다네.",
							" 이 석판은 어제 해독한 실리안 카미너스란 존재가 나타나 있는 석판의 내용을  더욱 보강해 주더군. 그리고 새로운 사실을 알았다네." +
							" 바로 지하 세계와 지상 세계가 연결되는 시기를  알려 주더군. 그것은 바로 월식이 일어나는 시기를 알면 된다네." +
							" 석판의 고대어에 의하면 월식이 일어난 후 위협의 동굴에 지하 세계와의 통로가 생기게 된다네." +
							" 여기서 \"위협의 동굴\" 이란 바로 메너스(MENACE)를 뜻하는 말이된다네." +
							$" 그리고, 점성가를 통해서  다음 월식이 일어날 때를 알아보니  바로 20일 후인 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일이 될거라고 하더군. 그때 메너스에 가보게." +
							" 그리고 조심하게. 아직 남은 20일 동안 체력도 보강 시키도록하게. 그럼 행운을 빌겠네.",
							$"[color={RGB.LightCyan}] [[ 경험치 + 0 ] [[ 황금 + 100000 ][/color]"
						});

						mParty.Gold += 100000;
						mParty.Etc[36] = eclipseDay / 256;
						mParty.Etc[35] = eclipseDay % 256;
						mParty.Etc[38] = eclipseYear / 256;
						mParty.Etc[37] = eclipseYear % 256;

						mParty.Etc[9]++;
					}
					else if (mParty.Etc[9] == 8) {
						var eclipseDay = mParty.Etc[36] * 256 + mParty.Etc[35];
						var eclipseYear = mParty.Etc[38] * 256 + mParty.Etc[37];

						if (mParty.Year > eclipseYear || (mParty.Year == eclipseYear && mParty.Day > eclipseDay)) {
							mParty.Etc[9]++;
							TalkMode(moveX, moveY, key);
						}
						else {
							AppendText(new string[] {
								" 메너스에 지하 세계로의 통로가  열리게 되는 첫번째 월식때 까지 기다리게.",
								" 날짜는 전에도 말했듯이  {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일 밤일세. 그때를 위해 열심히 훈련하도록 하게나."
							});
						}
					}
					else if (mParty.Etc[9] == 9)
						AppendText(" 드디어 우리가 기다리던 월식은 일어났네. 자네는 한시바삐 메너스를 통해 지하세계로 가도록 하게. 이제 이 세계의 운명은 자네의 두 손에 달려있다네.");
					else if (mParty.Etc[9] == 10) {
						Talk(new string[] {
							" 자네 드디어 돌아왔군. 그래, 지하 세계에 대해서 좀 말 해주게나.",
							"",
							$"[color={RGB.LightCyan}] [[ 경험치 + 500000 ] [[ 황금 + 100000 ][/color]"
						});

						foreach (var player in mPlayerList) {
							player.Experience += 500_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 500_000;

						mParty.Gold += 100_000;

						mSpecialEvent = SpecialEventType.MeetLordAhn8;
					}
					else if (mParty.Etc[9] == 11) {
						var eclipseDay = mParty.Etc[36] * 256 + mParty.Etc[35];
						var eclipseYear = mParty.Etc[38] * 256 + mParty.Etc[37];

						if (mParty.Year > eclipseYear || (mParty.Year == eclipseYear && mParty.Day > eclipseDay))
						{
							mParty.Etc[9]++;
							TalkMode(moveX, moveY, key);
						}
						else
						{
							AppendText(new string[] {
								" 메너스에 지하 세계로의 통로가  열리게 되는 두번째 월식때 까지 기다리게.",
								$" 날짜는 전에도 말했듯이  {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일 밤일세. 이번에는 치열한 전투도 각오해야 할 걸세. 열심히 단련하게나."
							});
						}
					}
					else if (mParty.Etc[9] == 12) {
						AppendText(" 드디어 우리가 기다리던 월식은 일어났네. 자네는 한시바삐 메너스를 통해 지하세계로 가도록 하게. 이제 이 세계의 운명은 자네의 두 손에 달려있다네." +
						"  언제나 자네에게 이런일을 시켜서 미안하게 됐네. 자네 앞에 언제나 행운이 따르기를 빌어 주겠네.");
					}
					else if (mParty.Etc[9] == 13) {
						Talk(new string[] {
							" 돌아왔군. 정말 수고 했네. 그동안 있었던 일을 나에게 모두 이야기 해주게.",
							"",
							$"[color={RGB.LightCyan}] [ 경험치 + 1000000 ] [ 황금 + 500000 ][/color]"
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 1_000_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 1_000_000;

						mParty.Gold += 500_000;

						mSpecialEvent = SpecialEventType.MeetLordAhn10;
					}
					else if (mParty.Etc[9] == 14) {
						var eclipseDay = mParty.Etc[36] * 256 + mParty.Etc[35];
						var eclipseYear = mParty.Etc[38] * 256 + mParty.Etc[37];

						if (mParty.Year > eclipseYear || (mParty.Year == eclipseYear && mParty.Day > eclipseDay))
						{
							mParty.Etc[9]++;
							TalkMode(moveX, moveY, key);
						}
						else
						{
							AppendText(new string[] {
								$" 운명의 세번째 월식이  바로 {eclipseYear}년 {eclipseDay / 30 + 1}월 {eclipseDay % 30 + 1}일 밤에 일어나게 된다네." +
								" 아마 자네의 마지막 결전이 될 걸세. 각오를 단단히 하게. 운명의 시간은 점점 우리에게 다가오고 있다네."
							});

							mParty.Etc[8] = 0;
						}
					}
					else if (mParty.Etc[9] == 15) {
						AppendText(new string[] {
							" 드디어 우리가 기다리던  마지막 세번째 월식이 일어났네. 정말 이번에는 단단한 각오를 해야 할 걸세." +
							"  월식이 일어나던 밤에 메너스 쪽에서 큰 지진이 있었다네.  아마 이건 다크 메이지가 부활이 거의 다 끝나간다는 것을  말해주고 있는 것 같네."
						});
					}
				}
			}
			else if (mParty.Map == 7) {
				if ((moveX == 15 && moveY == 23) || (moveX == 20 && moveY == 20) || (moveX == 23 && moveY == 18))
					ShowClassTrainingMenu();
				else if (moveX == 17 && moveY == 18)
					ShowExpStoreMenu();
				else if (moveX == 16 && moveY == 55)
					ShowMedicineStoreMenu();
				else if ((moveX == 16 && moveY == 57) || (moveX == 16 && moveY == 59))
					ShowHospitalMenu();
				else if ((moveX == 58 && moveY == 59) || (moveX == 58 && moveY == 57) || (moveX == 58 && moveY == 55))
					ShowWeaponShopMenu();
				else if ((moveX == 56 && moveY == 16) || (moveX == 53 && moveY == 19))
					ShowItemStoreMenu();
				else if ((moveX == 57 && moveY == 21) || (moveX == 58 && moveY == 24))
					ShowGroceryMenu();
				else if (moveX == 36 && moveY == 40)
					AppendText(" 제가 폴라리스님의 후임으로  이 성문을 지키고 있습니다.");
				else if (moveX == 39 && moveY == 40)
				{
					AppendText(new string[] {
						$" 아~~~ 당신이 바로 폴라리스님이 말씀 하시던 바로 그 {mPlayerList[0].Name}님이시군요.",
						" 폴라리스님께 말씀 많이 들었습니다."
					});
				}
				else if (moveX == 35 && moveY == 18)
					AppendText(" 이 성의 병원에는  희귀한 약초를  파는 곳이 있습니다. 체력이나 마법을 올려 주는 그런 약물이나 독을 해독하거나 심지어 죽은 사람마저도 살릴 수 있는 약초도 있더군요.");
				else if (moveX == 35 && moveY == 20)
					AppendText(" 저는 직접  타임 워커 알비레오님을 보았는데 같은 마법사로서 존경하지 않을 수 없더군요. 그 분의 예언은 틀림없다고 확신 할 수 있습니다.");
				else if (moveX == 40 && moveY == 19)
					AppendText(" 내가 듣기로는 지하 세계를 발견하기 위해 알 수 없는 피라미드로 들어간 사람이 몇 명 있더군요.");
				else if (moveX == 40 && moveY == 21)
					AppendText(" 저는 당신의 능력을 믿습니다.  꼭 이 세계를 구해 주십시오.");
				else if (moveX == 40 && moveY == 17 && (mParty.Etc[49] & (1 << 7)) == 0)
				{
					AppendText(new string[] {
						$" 오래간 만이군 !! 이 폴라리스의 이름을 걸고 {mPlayerList[0].Name} 자네를 환영하네.",
						" 이번에는 내가 참여 할 만한 모험이 없나?",
						" 분명히 있는 것 같은데  안 그런가?  이번의 일에도 나를 참가 시켜 주게.",
						" 꼭 부탁하네."
					});

					ShowMenu(MenuMode.JoinPolaris, new string[] {
						"원한다면 허락하지",
						"자네는 이 곳에서 더 할일이 많다네"
					});
				}
				else if (moveX == 20 && moveY == 60)
					AppendText(" 저는 이 성의 남쪽에 있는 피라미드에 들어갔다가 상처를 입어서  여기서 치료를 받고 있습니다.  그곳에는 몇년 전에 없어진줄로만 알고있었던 괴물들이 득실 거리더군요.");
				else if (moveX == 55 && moveY == 58)
					AppendText(" 저는 이 곳의 점성가인데  근래 들어 달의 운행이 점점 이상해지고 있습니다. 분명 큰 재앙이 닥칠 증거 같습니다.");
				else if (moveX == 55 && moveY == 58)
					AppendText(" 이 위의 상인들은 로어성의 상인과는 달리 마법 물품들을 판매하고 있더군요.");
				else if (moveX == 60 && moveY == 45)
					AppendText(" 알비레오의 예언은 정말일까요?  지금 이 대륙에는  그의 예언서가 안 알려진 곳이 없습니다. 때문에 민심도 많이 동요 되고 있습니다.");
				else if (moveX == 46 && moveY == 35)
					AppendText(" 만약 당신이 돈은 많은데  경험치가 모자란다면 서쪽에 있는 군사 훈련소의 어떤 자에게 부탁 하시오.  그는 금액 만큼의 전투 방법을 가르쳐서 경험치를 올려 준다더군요.");
				else if (moveX == 53 && moveY == 55 && (mParty.Etc[50] & 1) == 0)
				{
					if (mParty.Etc[4] == 0)
					{
						Talk(new string[] {
							" 용하게도 나를 찾아 냈군. 내가 로어 성에 남긴 메모 쪽지를 보았겠지. 그것에 관한 이야긴데 말이야..." +
							"  나는 몇 개월 전에 로어 헌터와 이 성의 남쪽에 있는 피라미드에 들어 갔었지. 우리가 그 곳에서 본 것은 복잡한 미로와 함께 두개의 동굴이었지." +
							" 그 두개의 동굴은 모두 지하의 신세계와 이어져 있었고  그 지하 세계는 완전한 하나의 또 다른 세계를 이루고 있었지." +
							$" 그 곳은 [color={RGB.LightCyan}]빛의 지붕[/color]이라고 불리는  마을도 있었고 동굴도 몇개가 있었다고 기억 된다네." +
							" 하지만 우리가 갔을 때는 새로운 변화가 일어 나고 있었네.  그 곳은 네크로만서와는 다른 존재가 반란을 꾀하고 있었고  그와 그 일당들의 힘은 정말 대단하였지." +
							" 그의 힘에 의해 지하 세계의 피라미드가 튕겨져 나왔다는  사실을 알았을때는 정말 놀랬다네." +
							"  도저히 우리들의 힘으로는 그들을 당할 수가 없어서 쫓겨 다니던 중 다시 피라미드를 통해 나오게 되었던 것이네." +
							$" 그 일당은 모두 4명의 보스로 구성되어 있었는데 모두 최강의 마법사들이었네." +
							$"  우리가 [color={RGB.LightCyan}]흉성의 증거[/color]라는 동굴을 나올 때 그 중의 우두머리인 [color={RGB.LightCyan}]메피스토펠레스[/color]가" +
							"  공기중의 수소를 이용한 행융합 마법을 사용하여  동굴 입구를 폭파 시켰다네.  정말 그 힘은 레드 안타레스의 마법을 능가하는 힘이 었다네." +
							" 그 4 명의 보스의 이름도 참고로 알아 두게나. 그 일당의 일인자가 방금 말한 메피스토펠레스라고하고,  이인자가 아스모데우스이고, 그 다음이 몰록이며,  마지막이 베리알이라고 하더군." +
							"  지금은 결국 그들을 모두 따돌렸지만  그들도 그들의 존재가  알려진 이상 가만히 있지 않을걸세." +
							" 만약 자네가 지하세계를 탐험하려 한다면 어서 빨리 가보도록하게 분명 그들은 입구를 봉쇄하려 할 걸세.  아니면 벌써 입구를 막아 버렸는지도 모르겠지만." +
							" 지금 지하 세계에서 일어나는 변화는 분명히 알비레오의 예언과 관련이 있다네.  아마 자가 해야할 일도 거기와 관련된 것일 걸세."
						});

						mSpecialEvent = SpecialEventType.MeetGeniusKie;
					}
					else
					{
						AppendText(new string[] {
							" 다시 생각해 보니 나도 자네의 일행에 참가해야할 것같네." +
							"  나는 얼마 전에 지하 세계에 다녀 왔기 때문에  전투 감각이 아직 살아았는데다가 전사라는 계급 때문에  자네에게 많은 도움이 될 걸세.",
							" 자네에게 한번 부탁하네."
						});

						ShowMenu(MenuMode.JoinGeniusKie, new string[] {
							"자네라면 큰 도움이 될걸세",
							"지금은 좀 곤란하군"
						});
					}
				}
				else if (moveX == 37 && moveY == 16)
				{
					if (mParty.Etc[10] == 0)
					{
						AppendText(new string[] {
							$" 잘왔소, {mPlayerList[0].Name}공.",
							" 공만이 이 일을 해결할 수 있을것 같아서  한가지 부탁을 하겠소." +
							" 벌써 발견했는 지는 모르겠지만 이 성의 남쪽에는 알 수 없는 피라미드가 땅속 깊은 곳으로 부터 솟아 올랐소." +
							" 그 곳을 이미 로어 헌터가 지니어스 기란 용사와 같이 탐험을 했었소. 거기에서 로어 헌터는 지하세계를 발견하고는  구사일생으로 살아서 돌아왔다오." +
							"  하지만 그는 지하 세계에 대한 더 자세한 정보를 얻기 위해  단신으로 다시 피라미드에 들어갔다오." +
							" 지금 그의 생사는 도저히 알 수가 없어서 공에게 그를 도와 달라고 부탁 하는 것이오.  피라미드에는 괴물들이 많이 있으니 주의해야 할것이오. 꼭 그를 찾아내시오."
						});
						mParty.Etc[10]++;
					}
					else if (mParty.Etc[10] == 1)
						AppendText(" 피라미드 속에서 소식이 끊어진  로어 헌터의 생사를 알아 주시오.");
					else if (mParty.Etc[10] == 2)
					{
						AppendText(new string[] {
							" 로어 헌터가 살아 있었다니 정말 다행이군요.",
							" 공에게 이 일을 맡기기를  정말 잘 선택한 것 같소.",
							"",
							$"[color={RGB.LightCyan}] [[ 경험치 + 50000 ] [ 황금 + 10000 ][/color]"
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 50_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 50_000;

						mParty.Gold += 10_000;
						mParty.Etc[10]++;
					}
					else if (mParty.Etc[10] == 3)
					{
						if ((mParty.Etc[30] & (1 << 5)) > 0 && (mParty.Etc[31] & (1 << 7)) > 0)
						{
							mParty.Etc[10]++;
							TalkMode(moveX, moveY, key);
							return;
						}
						else
						{
							AppendText(new string[] {
								" 이번에 공이 할 일은  피라미드 속의 두 동굴을 탐험하고 지하 세계에 대한 정보를 알아 오는 일이오.",
								" 이번 일도 부탁하오."
							});
						}
					}
					else if (mParty.Etc[10] == 4)
					{
						AppendText(new string[] {
							" 공이 피라미드 안의 두 동굴에서 발견한 것이 이 두개의 석판이오 ?  유감스럽게도 나에게는 이 고대어를 해석할 만한 능력이 없소." +
							" 분명히 로드 안이라면 해석할 수 있을 것이오.",
							"",
							$"[color={RGB.LightCyan}] [[ 경험치 + 100000 ] [ 황금 + 15000 ][/color]"
						});

						foreach (var player in mPlayerList)
						{
							player.Experience += 100_000;
						}

						if (mAssistPlayer != null)
							mAssistPlayer.Experience += 100_000;

						mParty.Gold += 15_000;
						mParty.Etc[10]++;
					}
					else
					{
						AppendText(" 이제 공이 이 곳에서 할 일은 다 끝났소");
						if (mParty.Etc[9] == 5)
							mParty.Etc[9]++;
					}
				}
			}
		}
		
		private async void InvokeAnimation(AnimationType animationEvent, int aniX = 0, int aniY = 0)
		{
			mAnimationEvent = animationEvent;

			var animationTask = Task.Run(() =>
			{
				if (mAnimationEvent == AnimationType.LordAhnCall)
				{
					for (var i = 1; i <= 4 + Math.Abs(mParty.YAxis - 48); i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.LeaveSoldier) {
					for (var i = 1; i <= 4 + Math.Abs(mParty.YAxis - 48); i++)
					{
						mAnimationFrame = i;
						Task.Delay(500).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.SleepLoreCastle) {
					var i = 0;
					while (true)
					{
						i++;
						mAnimationFrame = i;
						if (i <= 59)
							Task.Delay(5).Wait();
						else if (i == 60) {
							mFace = 3;
							mParty.YAxis = 49;
							mParty.XAxis = 15;
							Task.Delay(500).Wait();
						}
						else if (61 <= i && i <= 65) {
							mParty.XAxis--;
							Task.Delay(500).Wait();
						}
						else if (i == 66) {
							mFace = 1;
							Task.Delay(1000).Wait();
						}
						else if (i == 67) {
							mParty.Etc[0] = 0;
							Task.Delay(1000).Wait();
							AppendText(" 당신은 다음날 아침까지 여기서 자기로 했다.");
							mParty.Min = 0;
						}
						else if (i >= 68 && mParty.Hour != 9) {
							void UpdateState(Lore player) {
								if (player.Dead == 0 && player.Unconscious == 0)
								{
									if (player.Poison == 0)
									{
										player.HP += player.Level;
										if (player.HP > player.Endurance * player.Level * 10)
											player.HP = player.Endurance * player.Level * 10;
									}
									else
									{
										player.HP--;
										if (player.HP <= 0)
											player.Unconscious = 1;
									}
								}
								else if (player.Dead == 0 && player.Unconscious > 0)
								{
									if (player.Poison == 0)
									{
										if (player.Unconscious - player.Level > 0)
											player.Unconscious = player.Unconscious + player.Level;
										else
										{
											player.Unconscious = 0;
											player.HP = 1;
										}
									}
									else
									{
										player.Unconscious++;
										if (player.Unconscious > player.Endurance * player.Level)
											player.Dead = 1;
									}
								}
							}

							PlusTime(0, 20, 0);

							foreach (var player in mPlayerList) {
								UpdateState(player);
							}

							if (mAssistPlayer != null)
								UpdateState(mAssistPlayer);

							UpdatePlayersStat();
						}
						else {
							MovePlayer(mParty.XAxis + 1, mParty.YAxis);
							mFace = 2;

							break;
						}
					}
				}
				else if (mAnimationEvent == AnimationType.TalkLordAhn ||
					mAnimationEvent == AnimationType.TalkLordAhn2) {
					for (var i = 1; i <= 59; i++) {
						mAnimationFrame = i;
						Task.Delay(5).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.GetDefaultWeapon) {
					for (var i = 0; i < 3; i++)
					{
						Task.Delay(500).Wait();
						MovePlayer(mParty.XAxis - 1, mParty.YAxis);
					}
				}
				else if (mAnimationEvent == AnimationType.EnterUnderworld) {
					for (var i = 1; i <= 59; i++)
					{
						mAnimationFrame = i;
						Task.Delay(5).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.GoInsideMenace) {
					for (var i = 0; i < 2; i++) {
						MovePlayer(mParty.XAxis, mParty.YAxis - 1);
						Task.Delay(2000).Wait();
					}
				}
				else if (mAnimationEvent == AnimationType.GoInsideMenace2)
					Task.Delay(5000).Wait();
				else if (mAnimationEvent == AnimationType.BuyExp) {
					var totalTrainTime = mTrainTime * 3;

					for (var i = 0; i < totalTrainTime; i++) {
						PlusTime(0, 20, 0);
						Task.Delay(500).Wait();
					}
				}
			});

			await animationTask;

			if (mAnimationEvent == AnimationType.LordAhnCall)
			{
				Talk(new string[] {
					$" 일어났군, {mPlayerList[0].Name}.",
					" 자네 빨리 로드 안 님께 가보도록 하게. 급히 찾으시는 것 보니 아무래도 무슨 큰 일이 벌어진것 같네. 네크로만서 이후 몇년간 세상이 조용하더니만...",
					" 그건 그렇고 빨리 서두르게."
				});

				mSpecialEvent = SpecialEventType.LeaveSoldier;
			}
			else if (mAnimationEvent == AnimationType.LeaveSoldier)
			{
				for (var y = 48; y < 51; y++)
					UpdateTileInfo(18, y, 44);
				mParty.Etc[29] |= 1;

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.SleepLoreCastle)
			{
				AppendText($"[color={RGB.White}] 아침이 밝았다.[/color]");

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.TalkLordAhn || mAnimationEvent == AnimationType.TalkLordAhn)
			{
				Talk(" 한참후 ...");

				if (mAnimationEvent == AnimationType.TalkLordAhn)
					mSpecialEvent = SpecialEventType.MeetLordAhn9;
				else
					mSpecialEvent = SpecialEventType.MeetLordAhn11;
			}
			else if (mAnimationEvent == AnimationType.GetDefaultWeapon)
			{
				void Equip(Lore player)
				{
					if (player.Weapon == 0 && player.ClassType == ClassCategory.Sword && player.Class != 5)
					{
						player.Weapon = 1;
						player.WeaPower = 5;
					}
				}

				UpdateTileInfo(40, 78, 44);
				AppendText(" 일행은 가장 기본적인 무기로  모두  무장을 하였다.");

				foreach (var player in mPlayerList)
				{
					Equip(player);
				}

				if (mAssistPlayer != null)
					Equip(mAssistPlayer);

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.EnterUnderworld)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mParty.Etc[8] |= 1;

				mFace = 4;
				AppendText(" 한참 후에 당신은 정신이 들었고 여기가 지하세계임을 알아 차렸다.");
			}
			else if (mAnimationEvent == AnimationType.GoInsideMenace)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				AppendText(" 당신이 조금 더 안쪽으로 들어 갔을때 누군가가 당신을 지켜 보고 있다는 느낌이 들었다.");
				InvokeAnimation(AnimationType.GoInsideMenace2);
			}
			else if (mAnimationEvent == AnimationType.GoInsideMenace2)
			{

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				StartBattleEvent(BattleEvent.MenaceMurder);
			}
			else if (mAnimationEvent == AnimationType.GoInsideMenace)
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				StartBattleEvent(BattleEvent.MenaceMurder);
			}
			else if (mAnimationEvent == AnimationType.BuyExp) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				foreach (var player in mPlayerList)
					player.Experience += mTrainTime * 10_000;

				if (mAssistPlayer != null)
					mAssistPlayer.Experience += mTrainTime * 10_000;

				AppendText($"[color={RGB.White}] 일행은 훈련을 끝 마쳤다.[/color]", true);
				ContinueText.Visibility = Visibility.Visible;
			}
			else
			{
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
		}

		void StartBattleEvent(BattleEvent battleEvent) {
			if (battleEvent == BattleEvent.MenaceMurder)
			{
				mEncounterEnemyList.Clear();

				for (var i = 0; i < 7; i++)
					JoinEnemy(18);
				var enemy = JoinEnemy(26);
				enemy.Strength = 100;
				enemy.CastLevel = 0;
				enemy.Special = 0;

				DisplayEnemy();

				if (mParty.Etc[9] == 3)
				{
					Talk(new string[] {
						$" 드디어 만났군, {mPlayerList[0].Name}.",
						" 나는 네크로만서님이 너에게 당한 후 그 복수를 하기 위해  몇년을 기다렸다.  그리고 나의 병사들도 가지게 되었지." +
						" 나는 너를 이 곳으로 끌어 들이기 위해 계속 이 곳에서 살인을 저질렀지. 결국 걸려 들었군. 아~하~하~하~~~"
					});

					mParty.Etc[9]++;

					mSpecialEvent = SpecialEventType.BattleMenace;
				}
				else
				{
					Talk(" 그러나 나는 그렇게 쉽게 죽지는 않는다.  다시 나의 공격을 받아라 !");

					mSpecialEvent = SpecialEventType.BattleMenace;
				}
			}
		}

		private void ShowWeaponShopMenu()
		{
			AppendText(new string[] {
				$"[color={RGB.White}]여기는 무기상점입니다.[/color]",
				$"[color={RGB.White}]우리들은 무기, 방패, 갑옷을 팔고있습니다.[/color]",
				$"[color={RGB.White}]어떤 종류를 원하십니까 ?[/color]",
			});

			ShowMenu(MenuMode.ChooseWeaponType, new string[] {
				"베는 무기류",
				"찍는 무기류",
				"찌르는 무기류",
				"쏘는 무기류",
				"방패류",
				"갑옷류"
			});
		}

		private void ShowExpStoreMenu()
		{
			AppendText(new string[] {
					$"[color={RGB.White}] 여기에서는 황금의 양만큼  훈련을 시켜서 경험치를 올려 주는 곳입니다.[/color]",
					$"[color={RGB.LightCyan}] 원하시는 금액을 고르십시오.[/color]"
				});

			ShowMenu(MenuMode.BuyExp, new string[] {
					"금 10,000개; 소요시간 : 1 시간",
					"금 20,000개; 소요시간 : 2 시간",
					"금 30,000개; 소요시간 : 3 시간",
					"금 40,000개; 소요시간 : 4 시간",
					"금 50,000개; 소요시간 : 5 시간",
					"금 60,000개; 소요시간 : 6 시간",
					"금 70,000개; 소요시간 : 7 시간",
					"금 80,000개; 소요시간 : 8 시간",
					"금 90,000개; 소요시간 : 9 시간",
					"금 100,000개; 소요시간 : 10 시간",
				});
		}

		private void ShowItemStoreMenu() {
			AppendText(new string[] {
				$"[color={RGB.White}] 여기는 여러가지 물품을 파는 곳입니다.[/color]",
				"",
				$"[color={RGB.White}] 당신이 사고 싶은 물건을 고르십시오.[/color]"
			});

			ShowMenu(MenuMode.SelectItem, mItems);
		}

		private void ShowMedicineStoreMenu()
		{
			AppendText(new string[] {
				$"[color={RGB.White}] 여기는 약초를 파는 곳입니다.[/color]",
				"",
				$"[color={RGB.White}] 당신이 사고 싶은 약이나 약초를 고르십시오.[/color]"
			});

			ShowMenu(MenuMode.SelectMedicine, mMedicines);
		}

		private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			args.TrackAsyncAction(LoadImages(sender.Device).AsAsyncAction());
		}

		private async Task LoadImages(CanvasDevice device)
		{
			try
			{
				mMapTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_tile.png"), new Vector2(52, 52), Vector2.Zero);
				mCharacterTiles = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/lore_sprite.png"), new Vector2(52, 52), Vector2.Zero);

				await LoadEnemyData();
			
			}
			catch (Exception e)
			{
				Debug.WriteLine($"에러: {e.Message}");
			}
		}

		private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			var playerX = mParty.XAxis;
			var playerY = mParty.YAxis;

			var xOffset = 0;
			var yOffset = 0;
			if (mTelescopeXCount != 0)
			{
				if (mTelescopeXCount < 0)
					xOffset = -(mTelescopePeriod - Math.Abs(mTelescopeXCount));
				else
					xOffset = mTelescopePeriod - Math.Abs(mTelescopeXCount);
			}

			if (mTelescopeYCount != 0)
			{
				if (mTelescopeYCount < 0)
					yOffset = -(mTelescopePeriod - Math.Abs(mTelescopeYCount));
				else
					yOffset = mTelescopePeriod - Math.Abs(mTelescopeYCount);
			}

			var transform = Matrix3x2.Identity * Matrix3x2.CreateTranslation(-new Vector2(52 * (playerX - 4 + xOffset), 52 * (playerY - 5 + yOffset)));
			args.DrawingSession.Transform = transform;

			var size = sender.Size.ToVector2();

			var options = ClampToSourceRect ? CanvasSpriteOptions.ClampToSourceRect : CanvasSpriteOptions.None;
			//var options = CanvasSpriteOptions.None;
			//var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), InterpolationMode);
			var interpolation = (CanvasImageInterpolation)Enum.Parse(typeof(CanvasImageInterpolation), CanvasImageInterpolation.HighQualityCubic.ToString());

			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, options))
			{
				lock (mapLock)
				{
					for (int i = 0; i < mMapLayer.Length; ++i)
					{
						DrawTile(sb, mMapLayer, i, playerX, playerY);
					}
				}

				if (mCharacterTiles != null)
				{
					mCharacterTiles.Draw(sb, mFace, mCharacterTiles.SpriteSize * new Vector2(playerX, playerY), Vector4.One);
				}
			}

			//for (var i = 0; i < 117; i++)
			//	args.DrawingSession.FillRectangle(new Rect((playerX - 4)* 52 + (i * 4), (playerY - 5) * 52, 2, 52 * 11), Colors.Black);
		}

		void DrawTile(CanvasSpriteBatch sb, byte[] layer, int index, int playerX, int playerY)
		{
			int row = index / mMapWidth;
			int column = index % mMapWidth;

			Vector4 tint;
			
			if ((layer[index] & 0x80) > 0 || (mXWide == 0 && mYWide == 0) || (playerX - mXWide <= column && column <= playerX + mXWide && playerY - mYWide <= row && row <= playerY + mYWide))
				tint = Vector4.One;
			else
				tint = new Vector4(0.1f, 0.1f, 0.6f, 1);

			if (mMapTiles != null)
			{	
				var mapIdx = 56;

				if (mPosition == PositionType.Town)
					mapIdx = 0;
				else if (mPosition == PositionType.Keep)
					mapIdx *= 1;
				else if (mPosition == PositionType.Ground)
					mapIdx *= 2;
				else if (mPosition == PositionType.Den)
					mapIdx *= 3;


				byte tileIdx = GetTileInfo(layer, index);

				if (mSpecialEvent == SpecialEventType.Penetration)
				{
					if ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && tileIdx == 52)
						tileIdx = 0;
				}
				else if (tileIdx == 0)
				{
					switch (mParty.Map)
					{
						case 1:
							tileIdx = 2;
							break;
						case 2:
							tileIdx = 0;
							break;
						case 3:
							tileIdx = 0;
							break;
						case 4:
							tileIdx = 41;
							break;
						case 5:
							tileIdx = 0;
							break;
						case 6:
							tileIdx = 44;
							break;
						case 7:
							tileIdx = 45;
							break;
						case 8:
							tileIdx = 47;
							break;
						case 9:
							tileIdx = 44;
							break;
						case 10:
							tileIdx = 27;
							break;
						case 11:
							tileIdx = 44;
							break;
						case 12:
							tileIdx = 0;
							break;
						case 13:
							tileIdx = 42;
							break;
						case 14:
							tileIdx = 44;
							break;
						case 15:
							tileIdx = 39;
							break;
						case 16:
							tileIdx = 41;
							break;
						case 17:
							tileIdx = 41;
							break;
						case 18:
							tileIdx = 43;
							break;
						case 19:
							tileIdx = 49;
							break;
						case 20:
							tileIdx = 44;
							break;
						case 21:
							tileIdx = 40;
							break;
						case 22:
							tileIdx = 40;
							break;
						case 23:
							tileIdx = 46;
							break;
						case 24:
							tileIdx = 47;
							break;
						case 25:
							tileIdx = 41;
							break;
						case 26:
							tileIdx = 44;
							break;
						case 27:
							tileIdx = 44;
							break;
					}
				}

				if (mAnimationEvent == AnimationType.LordAhnCall) {
					if (row == 48 && 1 <= mAnimationFrame && mAnimationFrame <= 4) {
						if (column == 23 - mAnimationFrame)
							mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						else
							mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else if (mAnimationFrame >= 5) {
						if (column == 19) {
							if (playerY > 48 && row == 44 + mAnimationFrame) {
								mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
							}
							else if (playerY < 48 && row == playerY - (mAnimationFrame - 5))
							{
								mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
							}
							else
								mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						}
						else
							mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else if (mAnimationEvent == AnimationType.LeaveSoldier) {
					if (mAnimationFrame <= 3)
					{
						if (column == mAnimationFrame + 18 && row == playerY)
							mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						else
							mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else if (mAnimationFrame <= Math.Abs(playerY - 48) + 3)
					{
						if (playerY < 48 && row == playerY + (mAnimationFrame - 3) && column == 21)
							mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						else if (playerY > 48 && row == playerY - (mAnimationFrame - 3) && column == 21)
							mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
						else
							mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					}
					else if (column == 22 && row == 48)
						mMapTiles.Draw(sb, 53 + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
					else
						mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
				}
				else
					mMapTiles.Draw(sb, tileIdx + mapIdx, mMapTiles.SpriteSize * new Vector2(column, row), tint);
			}
		}

		private async Task LoadMapData()
		{
			string mapFileName;
			switch (mParty.Map)
			{
				case 1:
					mapFileName = "Ground";
					break;
				case 2:
					mapFileName = "Pyramid";
					break;
				case 3:
					mapFileName = "U_World1";
					break;
				case 4:
					mapFileName = "U_World2";
					break;
				case 5:
					mapFileName = "Ahn Young-Kie is Genius !!";
					break;
				case 6:
					mapFileName = "Lore";
					break;
				case 7:
					mapFileName = "LastDitc";
					break;
				case 8:
					mapFileName = "Dome";
					break;
				case 9:
					mapFileName = "Regulus";
					break;
				case 10:
					mapFileName = "Menace";
					break;
				case 11:
					mapFileName = "Evidence";
					break;
				case 12:
					mapFileName = "Clue";
					break;
				case 13:
					mapFileName = "LTemple";
					break;
				case 14:
					mapFileName = "Mortal";
					break;
				case 15:
					mapFileName = "Belial";
					break;
				case 16:
					mapFileName = "Moloch";
					break;
				case 17:
					mapFileName = "Asmodeus";
					break;
				case 18:
					mapFileName = "Mephisto";
					break;
				default:
					mapFileName = "PYRAMID1";
					break;
			}

			var mapFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{mapFileName}.MAP"));
			var stream = (await mapFile.OpenReadAsync()).AsStreamForRead();
			var reader = new BinaryReader(stream);

			lock (mapLock)
			{
				mMapWidth = reader.ReadByte();
				mMapHeight = reader.ReadByte();

				mMapLayer = new byte[mMapWidth * mMapHeight];

				for (var i = 0; i < mMapWidth * mMapHeight; i++)
				{
					mMapLayer[i] = reader.ReadByte();
				}
			}
		}

		private void ShowMap()
		{
			BattlePanel.Visibility = Visibility.Collapsed;
			canvas.Visibility = Visibility.Visible;
		}

		private void HideMap()
		{
			BattlePanel.Visibility = Visibility.Visible;
			canvas.Visibility = Visibility.Collapsed;
		}

		private void InitializeMap()
		{
			Uri musicUri;
			if (mParty.Map == 1 || mParty.Map == 3 || mParty.Map == 4)
			{
				mPosition = PositionType.Ground;
				musicUri = new Uri("ms-appx:///Assets/ground.mp3");
			}
			else if (6 <= mParty.Map && mParty.Map <= 9)
			{
				mPosition = PositionType.Town;
				musicUri = new Uri("ms-appx:///Assets/town.mp3");
			}
			else if (mParty.Map == 2 || (10 <= mParty.Map && mParty.Map <= 17))
			{
				mPosition = PositionType.Den;
				musicUri = new Uri("ms-appx:///Assets/den.mp3");
			}
			else
			{
				mPosition = PositionType.Keep;
				musicUri = new Uri("ms-appx:///Assets/keep.mp3");
			}

			if (mMapHeight / 2 > mParty.YAxis)
				mFace = 0;
			else
				mFace = 1;

			if (mPosition != PositionType.Town || mParty.Map == 26)
				mFace = mFace + 4;

			switch (mParty.Etc[11])
			{

			}

			//ShowMap();
			//BGMPlayer.Source = musicUri;

			UpdateView();
		}

		private async Task LoadEnemyData()
		{
			var enemyFileFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/EnemyData.dat"));
			mEnemyDataList = JsonConvert.DeserializeObject<List<EnemyData>>(await FileIO.ReadTextAsync(enemyFileFile));
		}

		private async Task<bool> LoadFile(int id = 0)
		{
			mLoading = true;

			var storageFolder = ApplicationData.Current.LocalFolder;

			var idStr = "";
			if (id > 0)
				idStr = id.ToString();

			var saveFile = await storageFolder.CreateFileAsync($"darkSave{idStr}.dat", CreationCollisionOption.OpenIfExists);
			var saveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(saveFile));

			if (saveData == null)
			{
				mLoading = false;
				return false;
			}

			mParty = saveData.Party;
			mPlayerList = saveData.PlayerList;

			if (saveData.Map.Data.Length == 0)
			{
				await LoadMapData();
			}
			else
			{
				lock (mapLock)
				{
					mMapWidth = saveData.Map.Width;
					mMapHeight = saveData.Map.Height;

					mMapLayer = saveData.Map.Data;
				}
			}

			mEncounter = saveData.Encounter;
			if (1 > mEncounter || mEncounter > 3)
				mEncounter = 2;

			mMaxEnemy = saveData.MaxEnemy;
			if (3 > mMaxEnemy || mMaxEnemy > 7)
				mMaxEnemy = 5;

			DisplayPlayerInfo();

			InitializeMap();

			mLoading = false;

			return true;
		}

		private void DisplayPlayerInfo()
		{
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
				{
					mPlayerNameList[i].Text = mPlayerList[i].Name;
					mPlayerNameList[i].Foreground = new SolidColorBrush(Colors.White);
				}
				else
				{
					mPlayerNameList[i].Text = "";
				}
			}

			DisplayHP();
			DisplaySP();
			DisplayCondition();
		}

		private void UpdatePlayersStat()
		{
			DisplayHP();
			DisplaySP();
			DisplayCondition();
		}

		private void DisplayHP()
		{
			for (var i = 0; i < mPlayerHPList.Count; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerHPList[i].Text = mPlayerList[i].HP.ToString();
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					mPlayerHPList[i].Text = mAssistPlayer.HP.ToString();
				else
					mPlayerHPList[i].Text = "";
			}
		}

		private void DisplaySP()
		{
			for (var i = 0; i < mPlayerSPList.Count; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerSPList[i].Text = mPlayerList[i].SP.ToString();
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					mPlayerSPList[i].Text = mAssistPlayer.SP.ToString();
				else
					mPlayerSPList[i].Text = "";
			}
		}

		private void DisplayCondition()
		{
			void UpdateCondition(TextBlock conditionText, Lore player) {
				if (player.HP <= 0 && player.Unconscious == 0)
					player.Unconscious = 1;

				if (player.Unconscious > player.Endurance * player.Level && player.Dead == 0)
					player.Dead = 1;

				if (player.Dead > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "죽은 상태";
				}
				else if (player.Unconscious > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "의식불명";
				}
				else if (player.Poison > 0)
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.LightRed));
					conditionText.Text = "중독";
				}
				else
				{
					conditionText.Foreground = new SolidColorBrush(GetColor(RGB.White));
					conditionText.Text = "좋음";
				}
			}

			for (var i = 0; i < mPlayerConditionList.Count; i++)
			{
				if (i < mPlayerList.Count)
					UpdateCondition(mPlayerConditionList[i], mPlayerList[i]);
				else if (i == mPlayerList.Count && mAssistPlayer != null)
					UpdateCondition(mPlayerConditionList[i], mAssistPlayer);
				else
					mPlayerConditionList[i].Text = "";
			}
		}

		private bool EnterWater()
		{
			if (mParty.Etc[1] > 0)
			{
				mParty.Etc[1]--;

				if (mRand.Next(mEncounter * 30) == 0)
					EncounterEnemy();

				return true;
			}
			else
				return false;
		}

		private void EnterSwamp()
		{
			void PoisonEffectPlayer(Lore player) {
				if (player.Poison > 0)
					player.Poison++;

				if (player.Poison > 10)
				{
					player.Poison = 1;

					if (0 < player.Dead && player.Dead < 100)
						player.Dead++;
					else if (player.Unconscious > 0)
					{
						player.Unconscious++;

						if (player.Unconscious > player.Endurance * player.Level)
							player.Dead = 1;
					}
					else
					{
						player.HP--;
						if (player.HP <= 0)
							player.Unconscious = 1;
					}

				}
			}
			
			foreach (var player in mPlayerList)
			{
				PoisonEffectPlayer(player);
			}

			if (mAssistPlayer != null)
				PoisonEffectPlayer(mAssistPlayer);

			if (mParty.Etc[2] > 0)
				mParty.Etc[2]--;
			else
			{
				AppendText(new string[] { $"[color={RGB.LightRed}]일행은 독이 있는 늪에 들어갔다 !!![/color]", "" });

				foreach (var player in mPlayerList)
				{
					if (mRand.Next(20) + 1 >= player.Luck)
					{
						AppendText($"[color={RGB.LightMagenta}]{player.NameSubjectJosa}(은)는 중독 되었다.[/color]", true);
						if (player.Poison == 0)
							player.Poison = 1;
					}
				}

				if (mAssistPlayer != null) {
					if (mRand.Next(20) + 1 >= mAssistPlayer.Luck)
					{
						AppendText($"[color={RGB.LightMagenta}]{mAssistPlayer.NameSubjectJosa}(은)는 중독 되었다.[/color]", true);
						if (mAssistPlayer.Poison == 0)
							mAssistPlayer.Poison = 1;
					}
				}
			}

			UpdatePlayersStat();
			DetectGameOver();
		}

		private void EnterLava()
		{
			void LavaEffectPlayer(Lore player) {
				var damage = (mRand.Next(40) + 40 - 2 * mRand.Next(player.Luck)) * 10;

				if (player.HP > 0 && player.Unconscious == 0)
				{
					player.HP -= damage;
					if (player.HP <= 0)
						player.Unconscious = 1;
				}
				else if (player.HP > 0 && player.Unconscious > 0)
					player.HP -= damage;
				else if (player.Unconscious > 0 && player.Dead == 0)
				{
					player.Unconscious += damage;
					if (player.Unconscious > player.Endurance * player.Level)
						player.Dead = 1;
				}
				else if (player.Dead > 0)
				{
					if (player.Dead + damage > 30_000)
						player.Dead = 30_000;
					else
						player.Dead += damage;

				}
			}

			AppendText(new string[] { $"[color={RGB.LightRed}]일행은 용암지대로 들어섰다!!![/color]", "" });

			foreach (var player in mPlayerList)
			{
				LavaEffectPlayer(player);
			}

			UpdatePlayersStat();
			DetectGameOver();
		}

		private BattleEnemyData JoinEnemy(int ENumber)
		{
			BattleEnemyData enemy = new BattleEnemyData(ENumber, mEnemyDataList[ENumber]);

			AssignEnemy(enemy);

			return enemy;
		}

		private BattleEnemyData TurnMind(Lore player)
		{
			var enemy = new BattleEnemyData(0, new EnemyData()
			{
				Name = player.Name,
				Strength = player.Strength,
				Mentality = player.Mentality,
				Endurance = player.Endurance,
				Resistance = player.Resistance * 5 > 99 ? 99 : player.Resistance * 5,
				Agility = player.Agility,
				Accuracy = new int[] { player.Accuracy, player.Accuracy },
				AC = player.AC,
				Special = player.Class == 7 ? 2 : 0,
				CastLevel = player.Level / 4,
				SpecialCastLevel = 0,
				Level = player.Level,
			});

			enemy.HP = enemy.Endurance * enemy.Level * 10;
			enemy.Posion = false;
			enemy.Unconscious = false;
			enemy.Dead = false;

			AssignEnemy(enemy);

			return enemy;
		}

		private void AssignEnemy(BattleEnemyData enemy)
		{
			var inserted = false;
			for (var i = 0; i < mEncounterEnemyList.Count; i++)
			{
				if (mEncounterEnemyList[i].Dead)
				{
					mEncounterEnemyList[i] = enemy;
					inserted = true;
					break;
				}
			}

			if (!inserted)
			{
				if (mEncounterEnemyList.Count == 7)
					mEncounterEnemyList[mEncounterEnemyList.Count - 1] = enemy;
				else
					mEncounterEnemyList.Add(enemy);
			}
		}


		private Lore GetMemberFromEnemy(int id)
		{
			var enemy = mEnemyDataList[id];

			var player = new Lore()
			{
				Name = enemy.Name,
				Gender = GenderType.Neutral,
				Class = 0,
				Strength = enemy.Strength,
				Mentality = enemy.Mentality,
				Concentration = 0,
				Endurance = enemy.Endurance,
				Resistance = enemy.Resistance / 2,
				Agility = enemy.Agility,
				Accuracy = enemy.Accuracy[1],
				Luck = 10,
				Poison = 0,
				Unconscious = 0,
				Dead = 0,
				Level = enemy.Level,
				AC = enemy.AC,
				Weapon = 50,
				Shield = 6,
				Armor = 11
			};

			player.HP = player.Endurance * player.Level * 10;
			player.SP = player.Mentality * player.Level * 10;
			player.WeaPower = player.Level * 2 + 10;
			player.ShiPower = 0;
			player.ArmPower = player.AC;
			player.Experience = 0;

			return player;
		}

		private void JoinMemberFromEnemy(int id)
		{
			var player = GetMemberFromEnemy(id);

			if (mPlayerList.Count >= 6)
				mPlayerList[5] = player;
			else
				mPlayerList.Add(player);
			
			DisplayPlayerInfo();
		}

		private void EncounterEnemy()
		{
			int range;
			int init;
			switch (mParty.Map)
			{
				case 2:
					range = 20;
					init = 0;
					break;
				case 11:
					range = 15;
					init = 14;
					break;
				case 12:
					range = 15;
					init = 24;
					break;
				case 14:
					range = 1;
					init = 34;
					break;
				case 15:
					range = 10;
					init = 30;
					break;
				case 16:
					range = 9;
					init = 40;
					break;
				case 17:
					range = 9;
					init = 52;
					break;
				default:
					range = 0;
					init = 0;
					break;
			}

			if (range == 0)
				return;

			mTriggeredDownEvent = true;

			var enemyNumber = mRand.Next(mMaxEnemy) + 1;
			if (enemyNumber > mMaxEnemy)
				enemyNumber = mMaxEnemy;

			mEncounterEnemyList.Clear();
			for (var i = 0; i < enemyNumber; i++)
			{
				var enemyID = mRand.Next(range) + init;

				mEncounterEnemyList.Add(new BattleEnemyData(enemyID, mEnemyDataList[enemyID]));
			}

			DisplayEnemy();
			HideMap();

			var avgAgility = 0;
			mEncounterEnemyList.ForEach(delegate (BattleEnemyData enemy)
			{
				avgAgility += enemy.Agility;
			});

			avgAgility /= mEncounterEnemyList.Count;

			AppendText(new string[] {
				$"[color={RGB.LightMagenta}]적이 출현했다 !!![/color]", "",
				$"[color={RGB.LightCyan}]적의 평균 민첩성 : {avgAgility}[/color]"
			});

			ShowMenu(MenuMode.BattleStart, new string[] {
				"적과 교전한다",
				"도망간다"
			});
		}

		private Color GetEnemyColor(BattleEnemyData enemy)
		{
			if (enemy.Dead)
				return GetColor(RGB.Black);
			else if (enemy.HP == 0 || enemy.Unconscious)
				return GetColor(RGB.DarkGray);
			else if (1 <= enemy.HP && enemy.HP <= 199)
				return GetColor(RGB.LightRed);
			else if (200 <= enemy.HP && enemy.HP <= 499)
				return GetColor(RGB.Red);
			else if (500 <= enemy.HP && enemy.HP <= 999)
				return GetColor(RGB.Brown);
			else if (1000 <= enemy.HP && enemy.HP <= 1999)
				return GetColor(RGB.Yellow);
			else if (2000 <= enemy.HP && enemy.HP <= 3999)
				return GetColor(RGB.Green);
			else
				return GetColor(RGB.LightGreen);
		}

		private void DisplayEnemy()
		{
			for (var i = 0; i < mEnemyTextList.Count; i++)
			{
				if (i < mEncounterEnemyList.Count)
				{
					mEnemyBlockList[i].Visibility = Visibility.Visible;
					mEnemyTextList[i].Text = mEncounterEnemyList[i].Name;
					mEnemyTextList[i].Foreground = new SolidColorBrush(GetEnemyColor(mEncounterEnemyList[i]));
				}
				else
					mEnemyBlockList[i].Visibility = Visibility.Collapsed;
			}

			HideMap();
		}

		private void ShowGameOver(string[] gameOverMessage)
		{
			AppendText(gameOverMessage);

			ShowMenu(MenuMode.BattleLose, new string[] {
						"이전의 게임을 재개한다",
						"게임을 끝낸다"
					});
		}

		private void DetectGameOver()
		{
			var allPlayerDead = true;
			foreach (var player in mPlayerList)
			{
				if (player.IsAvailable)
				{
					allPlayerDead = false;
					break;
				}
			}

			if (allPlayerDead)
			{
				mParty.Etc[5] = 255;

				ShowGameOver(new string[] { "일행은 모험 중에 모두 목숨을 잃었다." });
				mTriggeredDownEvent = true;
			}
		}


		private void ShowExitMenu()
		{
			AppendText(new string[] { $"[color={RGB.LightCyan}]여기서 나가기를 원합니까?[/color]" });

			ShowMenu(MenuMode.ConfirmExitMap, new string[] {
					"예, 그렇습니다.",
					"아니오, 원하지 않습니다."});
		}

		private void ShowSign(int x, int y)
		{
			AppendText(new string[] { "푯말에 쓰여있기로 ...\r\n\r\n" });

			if (mParty.Map == 6)
			{
				if (x == 50 && y == 83)
				{
					AppendText(new string[] { $"       [color={RGB.White}]여기는[/color] [color={RGB.LightCyan}]로어 성[/color]입니다",
								$"         [color={RGB.White}]여러분을 환영합니다[/color]",
								"",
								"",
								"",
								$"               [color={RGB.LightMagenta}]로드 안[/color]" }, true);
				}
				else if (x == 23 && y == 30)
				{
					AppendText(new string[] { "",
								$"             [color={RGB.White}]여기는 로어 주점[/color]",
								$"       [color={RGB.White}]여러분 모두를 환영합니다!![/color]" }, true);
				}
				else if ((x == 50 && y == 17) || (x == 51 && y == 17))
				{
					AppendText(new string[] { "",
							$"          [color={RGB.White}]로어 왕립 죄수 수용소[/color]" }, true);
				}
				else if (x == 76 && y == 47)
					AppendText($"          [color={RGB.White}]여기는 로어 은행입니다[/color]", true);
				else if (x == 76 && y == 41)
					AppendText($"[color={RGB.White}]       허락 없이 들어가지 마시오!![/color]", true);
				else if (x == 76 && y == 35)
					AppendText($"[color={RGB.White}]           마법사 베텔규스의 집[/color]", true);
				else if (x == 76 && y == 29)
					AppendText($"[color={RGB.White}]   지금 집 주인인 저 레굴루스는 여행을 떠나고 없습니다. 저에게 용건이 있으신 분은 북쪽 해안으로 오십시오.[/color]", true);
				else if (x == 76 && y == 23)
					AppendText($"[color={RGB.White}]         여기는 헤라클레스의 집[/color]", true);
			}
			else if (mParty.Map == 7) {
				if (x == 38 && y == 67)
				{
					AppendText(new string[] {
						$"[color={RGB.White}]       여기는[/color] [color={RGB.LightCyan}]라스트디치성[/color][color={RGB.White}]입니다[/color]",
						$"[color={RGB.White}]          여러분을 환영합니다[/color]"
					});
				}
				else if (x == 38 && y == 7)
					AppendText($"[color={RGB.LightRed}]        여기는 옛 피라미드 의 입구[/color]");
				else if (x == 53 && y == 8)
					AppendText($"[color={RGB.LightGreen}]       여기는 그라운드 게이트의 입구[/color]");
			}
		}

		private void PlusTime(int hour, int min, int sec) {
			mParty.Hour += hour;
			mParty.Min += min;
			mParty.Sec += sec;

			while (mParty.Sec > 59) {
				mParty.Sec -= 60;
				mParty.Min++;
			}

			while (mParty.Min > 59) {
				mParty.Min -= 60;
				mParty.Hour++;
			}

			while (mParty.Hour > 23) {
				mParty.Hour -= 24;
				mParty.Day++;
			}

			while (mParty.Day > 360) {
				mParty.Day -= 360;
				mParty.Year++;
			}

			UpdateView();
		}

		private void UpdateView()
		{
			bool dark;
			if (mPosition == PositionType.Den)
			{
				dark = true;

				mXWide = 1;
				mYWide = 1;
			}
			else if (7 > mParty.Hour || mParty.Hour > 17)
			{
				dark = true;
				if (mParty.Hour == 18)
				{
					if (0 <= mParty.Min && mParty.Min <= 19)
					{
						mXWide = 4;
						mYWide = 4;
					}
					else if (20 <= mParty.Min && mParty.Min <= 39)
					{
						mXWide = 3;
						mYWide = 3;
					}
					else
					{
						mXWide = 2;
						mYWide = 2;
					}
				}
				else if (mParty.Hour == 6)
				{
					if (0 <= mParty.Min && mParty.Min <= 19)
					{
						mXWide = 2;
						mYWide = 2;
					}
					else if (20 <= mParty.Min && mParty.Min <= 39)
					{
						mXWide = 3;
						mYWide = 3;
					}
					else
					{
						mXWide = 4;
						mYWide = 4;
					}
				}
				else
				{
					mXWide = 1;
					mYWide = 1;
				}
			}
			else
				dark = false;

			if (dark && mParty.Etc[0] > 0)
			{
				if (1 <= mParty.Etc[0] && mParty.Etc[0] <= 2)
				{
					if (mXWide < 2 && mYWide < 2)
					{
						mXWide = 2;
						mYWide = 2;
					}
				}
				else if (3 <= mParty.Etc[0] && mParty.Etc[0] <= 4)
				{
					if (mXWide < 3 && mYWide < 3)
					{
						mXWide = 3;
						mYWide = 3;
					}
				}
				else if (5 <= mParty.Etc[0] && mParty.Etc[0] <= 6)
				{
					if (mXWide < 4 && mYWide < 4)
					{
						mXWide = 4;
						mYWide = 4;
					}
				}
				else
				{
					mXWide = 4;
					mYWide = 4;
				}
			}
			else if (!dark)
			{
				mXWide = 0;
				mYWide = 0;
			}
		}

		private enum PositionType
		{
			Town,
			Ground,
			Den,
			Keep
		}

		private enum MenuMode
		{
			None,
			Game,
			GameOptions,
			TrainingCenter,
			ChooseTrainSkillMember,
			ChooseTrainSkill,
			ChooseTrainMagicMember,
			ChooseTrainMagic,
			ChooseChangeSwordMember,
			ChooseChangeMagicMember,
			ChooseSwordJob,
			ChooseMagicJob,
			Hospital,
			HealType,
			ChooseWeaponType,
			BuyWeapon,
			BuyShield,
			BuyArmor,
			ChooseFoodAmount,
			JoinMadJoe,
			JoinMercury,
			JoinHercules,
			JoinTitan,
			JoinMerlin,
			JoinBetelgeuse,
			ChooseSaveGame,
			ChooseLoadGame,
			ChooseGameOverLoadGame,
			BattleStart,
			BattleLose,
			ConfirmExitMap,
			AskEnter,
			FinalChoice,
			BattleCommand,
			EnemySelectMode,
			CastOneMagic,
			CastAllMagic,
			CastSpecial,
			CastESP,
			ChooseESPMagic,
			CastSummon,
			BattleChooseItem,
			ChooseItem,
			BattleUseItem,
			BattleUseItemWhom,
			UseItemWhom,
			ChooseBattleCureSpell,
			ApplyBattleCureSpell,
			ApplyBattleCureAllSpell,
			UseWeaponCharacter,
			UseShieldCharacter,
			UseArmorCharacter,
			BuyExp,
			SelectItem,
			SelectItemAmount,
			SelectMedicine,
			SelectMedicineAmount,
			JoinPolaris,
			JoinGeniusKie,
			ViewCharacter,
			CastSpell,
			SpellCategory,
			ChooseCureSpell,
			ApplyCureMagic,
			ApplyCureAllMagic,
			ApplyPhenominaMagic,
			VaporizeMoveDirection,
			TransformDirection,
			TeleportationDirection,
			BigTransformDirection,
			Extrasense,
			ChooseExtrasense,
			TelescopeDirection,
			ExchangeItem,
			ExchangeItemWhom,
			SwapItem,
			UseItemPlayer,
			SetMaxEnemy,
			SetEncounterType,
			AttackCruelEnemy,
			OrderFromCharacter,
			OrderToCharacter,
			UnequipCharacter,
			Unequip,
			DelistCharacter,
			ConfirmExit,
			ChooseEquipCromaticShield,
			ChooseEquipBattleAxe,
			ChooseEquipObsidianArmor
		}

		private enum SpinnerType
		{
			None,
			TeleportationRange,
			RestTimeRange
		}

		private enum BattleTurn
		{
			None,
			Player,
			Enemy,
			RunAway,
			AlmostWin,
			Win,
			Lose
		}

		private class BattleCommand
		{
			public Lore Player
			{
				get;
				set;
			}

			public int FriendID
			{
				get;
				set;
			}

			public int EnemyID
			{
				get;
				set;
			}

			public int Method
			{
				get;
				set;
			}

			public int Tool
			{
				get;
				set;
			}
		}

		private enum EnterType
		{
			None,
			CastleLore,
			CastleLastDitch,
			Menace,
			UnknownPyramid,
			ProofOfInfortune,
			ClueOfInfortune,
			RoofOfLight,
			TempleOfLight,
			SurvivalOfPerishment,
			CaveOfBerial,
			CaveOfMolok,
			TeleportationGate1,
			TeleportationGate2,
			TeleportationGate3,
			CaveOfAsmodeus1,
			CaveOfAsmodeus2,
			FortressOfMephistopheles,
			CabinOfRegulus
		}

		private enum AnimationType
		{
			None,
			LordAhnCall,
			LeaveSoldier,
			SleepLoreCastle,
			TalkLordAhn,
			TalkLordAhn2,
			GetDefaultWeapon,
			EnterUnderworld,
			GoInsideMenace,
			GoInsideMenace2,
			BuyExp
		}

		private enum SpecialEventType
		{
			None,
			CantTrain,
			CantTrainMagic,
			TrainSkill,
			TrainMagic,
			ChangeJobForSword,
			ChangeJobForMagic,
			LeaveSoldier,
			ViewGeniusKieLetter,
			ViewGeniusKieLetter2,
			TalkPrisoner,
			BattleBankGuard,
			MeetLordAhn,
			MeetLordAhn2,
			MeetLordAhn3,
			MeetLordAhn4,
			MeetLordAhn5,
			MeetLordAhn6,
			MeetLordAhn7,
			SleepLoreCastle,
			MeetLordAhn8,
			MeetLordAhn9,
			MeetLordAhn10,
			MeetLordAhn11,
			DestructCastleLore,
			DestructCastleLore2,
			DestructCastleLore3,
			EnterUnderworld,
			SeeDeadBody,
			BattleMenace,
			BattleUseItem,
			BackToBattleMode,
			HelpRedAntares,
			HelpRedAntares2,
			HelpRedAntares3,
			CureComplete,
			NotCured,
			CantBuyWeapon,
			CantBuyExp,
			CantBuyItem,
			CantBuyMedicine,
			MeetGeniusKie,
			WizardEye,
			Penetration,
			Telescope,
			BattleCaveOfBerialEntrance,
			InvestigationCave,
			BattleCaveOfAsmodeusEntrance,
			GetCromaticShield,
			GetBattleAxe,
			GetObsidianArmor,
			BattleGuardOfObsidianArmor,
			BattleSlaim,
			BattleCaveEntrance
		}

		private enum BattleEvent {
			None,
			MenaceMurder,
			CaveOfBerialEntrance,
			CaveOfAsmodeusEntrance,
			GuardOfObsidianArmor,
			Slaim,
			CaveEntrance
		}

		private class HealthTextBlock
		{
			private TextBlock mName;
			private TextBlock mPoison;
			private TextBlock mUnconscious;
			private TextBlock mDead;

			public HealthTextBlock(TextBlock name, TextBlock poison, TextBlock unconscious, TextBlock dead)
			{
				mName = name;
				mPoison = poison;
				mUnconscious = unconscious;
				mDead = dead;
			}

			public void Update(string name, int poison, int unconscious, int dead)
			{
				mName.Text = name;
				mPoison.Text = poison.ToString();
				mUnconscious.Text = unconscious.ToString();
				mDead.Text = dead.ToString();
			}

			public void Clear()
			{
				mName.Text = "";
				mPoison.Text = "";
				mUnconscious.Text = "";
				mDead.Text = "";
			}
		}

		private class WizardEye {
			public void Set(int width, int height) {
				Data = new byte[width * height];
				Width = width;
				Height = height;
			}

			public byte[] Data {
				get;
				private set;
			}

			public byte GetData(int x, int y) {
				return Data[x + y * Width];
			}

			public int Width {
				get;
				private set;
			}

			public int Height {
				get;
				private set;
			}
		}

		private void MapCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			async Task LoadTile(CanvasDevice device)
			{
				try
				{
					mWizardEyeTile = await SpriteSheet.LoadAsync(device, new Uri("ms-appx:///Assets/WizardEyeTile.png"), new Vector2(2, 2), Vector2.Zero);

				}
				catch (Exception e)
				{
					Debug.WriteLine($"에러: {e.Message}");
				}
			}

			args.TrackAsyncAction(LoadTile(sender.Device).AsAsyncAction());
		}

		private void MapCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			using (var sb = args.DrawingSession.CreateSpriteBatch(CanvasSpriteSortMode.None, CanvasImageInterpolation.NearestNeighbor, CanvasSpriteOptions.ClampToSourceRect))
			{
				lock (mWizardEye)
				{
					for (var y = 0; y < mWizardEye.Height; y++) {
						for (var x = 0; x < mWizardEye.Width; x++) {
							if (mWizardEyePosX == x && mWizardEyePosY == y) {
								if (mWizardEyePosBlink)
									mWizardEyeTile.Draw(sb, 12, mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
								else
									mWizardEyeTile.Draw(sb, 0, mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
							}
							else
								mWizardEyeTile.Draw(sb, mWizardEye.GetData(x, y), mWizardEyeTile.SpriteSize * new Vector2(x * 2, y * 2), Vector4.One);
						}
					}
				}
			}
		}
	}
}
