using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
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

		private SpriteSheet mMapTiles;
		private SpriteSheet mCharacterTiles;
		private byte[] mMapLayer = null;
		private readonly object mapLock = new object();

		private int mXWide; // 시야 범위
		private int mYWide; // 시야 범위

		bool ClampToSourceRect = true;

		private LorePlayer mParty;
		private List<Lore> mPlayerList;

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
		private CureMenuState mCureMenuState = CureMenuState.None;

		private int mTeleportationDirection = 0;

		private Lore mTrainPlayer;
		private static readonly List<Tuple<int, int>> list = new List<Tuple<int, int>>();
		private List<Tuple<int, int>> mTrainSkillList = list;
		private readonly List<string> mChangableClassList = new List<string>();
		private readonly List<int> mChangableClassIDList = new List<int>();

		private int mUseItemID;
		private Lore mItemUsePlayer;
		private readonly List<int> mUsableItemIDList = new List<int>();


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

		private bool mPenetration = false;
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

		public GamePage()
		{
			var rootFrame = Window.Current.Content as Frame;

			this.InitializeComponent();

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
			mEnterTypeMap[EnterType.ProofOfDarkStar] = "흉성의 증거";
			mEnterTypeMap[EnterType.ClueOfDarkStar] = "흉성의 단서";
			mEnterTypeMap[EnterType.RoofOfLight] = "빛의 지붕";
			mEnterTypeMap[EnterType.TempleOfLight] = "빛의 사원";
			mEnterTypeMap[EnterType.SurvivalOfPerishment] = "필멸의 생존";
			mEnterTypeMap[EnterType.CaveOfBerial] = "베리알의 동굴";
			mEnterTypeMap[EnterType.CaveOfMolok] = "몰록의 동굴";
			mEnterTypeMap[EnterType.TeleportationGate] = "공간 이동 게이트";
			mEnterTypeMap[EnterType.CaveOfAsmodeus] = "아스모데우스의 동굴";
			mEnterTypeMap[EnterType.FortressOfMephistopheles] = "메피스토펠레스의 요새";

			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName1, HealthPoison1, HealthUnconscious1, HealthDead1));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName2, HealthPoison2, HealthUnconscious2, HealthDead2));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName3, HealthPoison3, HealthUnconscious3, HealthDead3));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName4, HealthPoison4, HealthUnconscious4, HealthDead4));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName5, HealthPoison5, HealthUnconscious5, HealthDead5));
			mHealthTextList.Add(new HealthTextBlock(HealthPlayerName6, HealthPoison6, HealthUnconscious6, HealthDead6));

			gamePageKeyDownEvent = (sender, args) =>
			{
				if (mLoading || mSpecialEvent > 0 || mAnimationEvent != AnimationType.None || ContinueText.Visibility == Visibility.Visible)
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
						}

						if (mPosition == PositionType.Town)
						{
							if (GetTileInfo(x, y) == 0 || GetTileInfo(x, y) == 19)
							{
								var oriX = mParty.XAxis;
								var oriY = mParty.YAxis;
								MovePlayer(x, y);
								InvokeSpecialEvent(oriX, oriY);
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
							//	if (EnterWater())
							//		MovePlayer(x, y);
							//	mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 25)
							{
							//	EnterSwamp();
							//	MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 26)
							{
							//	EnterLava();
							//	MovePlayer(x, y);
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
								InvokeSpecialEvent(oriX, oriY);

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
								//if (EnterWater())
								//	MovePlayer(x, y);
								//mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 23 || GetTileInfo(x, y) == 49)
							{
								//EnterSwamp();
								//MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								//EnterLava();
								//MovePlayer(x, y);
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
								InvokeSpecialEvent(oriX, oriY);

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
								//if (EnterWater())
								//	MovePlayer(x, y);
								//mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 49)
							{
								//EnterSwamp();
								//MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								//EnterLava();
								//MovePlayer(x, y);
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
								//if (EnterWater())
								//	MovePlayer(x, y);
								//mTriggeredDownEvent = true;
							}
							else if (GetTileInfo(x, y) == 49)
							{
								//EnterSwamp();
								//MovePlayer(x, y);
							}
							else if (GetTileInfo(x, y) == 50)
							{
								//EnterLava();
								//MovePlayer(x, y);
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

				void ShowTrainSkillMenu()
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

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 수련 하고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainSkill, trainSkillMenuList.ToArray());
				}

				void ShowTrainMagicMenu()
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

					AppendText($"[color={RGB.LightGreen}] 여분의 경험치 :\t{mTrainPlayer.Experience}[/color]", true);

					AppendText($"[color={RGB.LightRed}]당신이 배우고 싶은 부분을 고르시오.[/color]", true);

					ShowMenu(MenuMode.ChooseTrainMagic, trainSkillMenuList.ToArray());
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

				//				void ShowHealType()
				//				{
				//					AppendText(new string[] { $"[color={RGB.White}]어떤 치료입니까 ?[/color]" });

				//					ShowMenu(MenuMode.HealType, new string[]
				//					{
				//							"상처를 치료",
				//							"독을 제거",
				//							"의식의 회복",
				//							"부활"
				//					});
				//				}

				//				async Task ExitCastleLore()
				//				{
				//					mParty.XAxis = 19;
				//					mParty.YAxis = 11;
				//					mParty.Map = 1;

				//					await RefreshGame();
				//				}

				//				async Task EndBattle()
				//				{
				//					void DefeatAstralMud()
				//					{
				//						mEncounterEnemyList.Clear();
				//						mBattleEvent = 0;

				//						ShowMap();

				//						Talk(" 당신은 이 동굴에 보관되어 있는 봉인을 발견했다. 그리고는 봉쇄 되었던 봉인을 풀어버렸다.");
				//						mSpecialEvent = SpecialEventType.DefeatAstralMud;
				//					}

				//					async Task CheckPassSwampKeepExitEvent()
				//					{
				//						if (mEncounterEnemyList[0].Dead)
				//							mParty.Etc[41] |= 1 << 2;

				//						if (mEncounterEnemyList[1].Dead)
				//							mParty.Etc[41] |= 1 << 3;

				//						if (mEncounterEnemyList[0].Dead && mEncounterEnemyList[1].Dead)
				//							mParty.Etc[41] |= 1;

				//						mParty.Map = 4;
				//						mParty.XAxis = 47;
				//						mParty.YAxis = 35;

				//						await RefreshGame();
				//					}

				//					void SwampKeepBattleEvent()
				//					{
				//						if (mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] == 0)
				//							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
				//						else
				//							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 46;
				//					}

				//					async Task DefeatImperiumMinorKeeper()
				//					{
				//						if (mEncounterEnemyList[6].Dead)
				//							mParty.Etc[42] |= 1 << 2;

				//						mParty.Map = 5;
				//						mParty.XAxis = 14;
				//						mParty.YAxis = 31;

				//						await RefreshGame();
				//					}

				//					void WinNecromancer()
				//					{
				//						Talk(new string[] {
				//							$" [color={RGB.LightMagenta}]욱!!! 역시 너희들의 능력으로 여기까지 뚫고 들어왔다는게 믿어지는구나. 대단한 힘이다.[/color]",
				//							$" [color={RGB.LightMagenta}]내가 졌다는걸 인정하마. 하지만 나는 완전히 너에게 진것은 아니야. 나에게는 탈출할 수단이 있기 때문이지. 안심해라." +
				//							" 그렇지만 다시는 나와 만날 인연은 없으니까. 블랙홀이 생기기 시작하는구나. 다음 공간에서 또다시 힘을 길러야 겠군." +
				//							" 내가 이 블랙홀로 들어간다면 다시 이 공간으로 올 확률이 거의 제로이지. 흠, 멋진 나의 도전자여 안녕. 나는 이런 공간의 패러독스를 운명적으로 반복하는 생명체로 태어난 내가 참으로 비참하지." +
				//							" 무한히 많은 3 차원의 공간중에서 내가 여기로 온것도 이 공간의 생명이 끝날때까지도 한번 있을까 말까한 희귀한 일이었다고 기억해다오." +
				//							" 이제 블랙홀이 완전히 생겼군. 자! 나의 멋진 도전자 친구여 영원히 안녕 ! !'[/color]",
				//						});

				//						mSpecialEvent = SpecialEventType.Ending;
				//					}

				//					async Task CheckImperiumMinorEntraceBattleResult()
				//					{
				//						var allDead = true;
				//						foreach (var enemy in mEncounterEnemyList)
				//						{
				//							if (enemy.ENumber == 64 && enemy.Dead)
				//								mParty.Etc[41] |= 1 << 4;
				//							else if (enemy.ENumber == 63 && enemy.Dead)
				//								mParty.Etc[41] |= 1 << 5;

				//							if (!allDead)
				//								allDead = false;
				//						}

				//						if (allDead)
				//							mParty.Etc[41] |= 1 << 1;

				//						mEncounterEnemyList.Clear();
				//						mBattleEvent = 0;

				//						mParty.Map = 22;
				//						mParty.XAxis = 24;
				//						mParty.YAxis = 5;

				//						if ((mParty.Etc[41] & (1 << 6)) == 0)
				//						{
				//							JoinEnemy(67);

				//							DisplayEnemy();

				//							Talk(new string[] { " 역시 당신들은 나의 예상대로 마지막 대륙까지 무난하게 왔군요. 이번에 가게될 라바 대륙은 이 세계에 있는 모든 대륙중에서 가장 작은 대륙이오." +
				//							" 적의 요새도 또한 2개 밖에 없는 곳이오. 하지만 이번에 도착할 임페리움 마이너나 마지막으로 거칠 이블 컨센츄레이션 은 말 그대로 악의 집결지인 것이오." +
				//							" 거기에는 최강의 괴물들과 네크로맨서 의 심복들로 가득차있는 곳이지만 임페리움 마이너의 지하에는 마지막으로 살아남은 사람들의 도시가 있소." +
				//							" 원래 거기는 에인션트 이블이 전에 세운 악의 동굴이었지만 네크로맨서의 침략으로 지상의 도시가 함락되자 그 곳의 사람들은 모두 거기로 피난 했던 것이고" +
				//							" 거기는 에인션트 이블의 영적인 힘으로 보호되고 있어서 적들이 침략을 하지 못하는 이유가 되지요. 그러므로 모든 도움과 물자는 거기서 받도록 하시오.",
				//							" 그러면, 나는 당신이 네크로맨서와 상대하게 될 때 다시 에인션트 이블과 같이 나타나겠소."});

				//							mSpecialEvent = SpecialEventType.EnterImperiumMinor;
				//						}
				//						else
				//						{
				//							await RefreshGame();
				//						}
				//					}

				//					async Task CheckDungeonOfEvilBattleResult()
				//					{
				//						if (mEncounterEnemyList[2].Dead)
				//							mParty.Etc[43] |= 1 << 1;

				//						mParty.Map = 25;
				//						mParty.XAxis = 24;
				//						mParty.YAxis = 44;

				//						await RefreshGame();
				//					}

				//					mBattleCommandQueue.Clear();
				//					mBatteEnemyQueue.Clear();

				//					if (mBattleTurn == BattleTurn.Win)
				//					{

				//						var endMessage = "";

				//						if (mParty.Etc[5] == 2)
				//							endMessage = "";
				//						else
				//						{
				//#if DEBUG
				//							var goldPlus = 10000;
				//#else
				//							var goldPlus = 0;
				//							foreach (var enemy in mEncounterEnemyList)
				//							{
				//								var enemyInfo = mEnemyDataList[enemy.ENumber];
				//								var point = enemyInfo.AC == 0 ? 1 : enemyInfo.AC;
				//								var plus = enemyInfo.Level;
				//								plus *= enemyInfo.Level;
				//								plus *= enemyInfo.Level;
				//								plus *= point;
				//								goldPlus += plus;
				//							}
				//#endif

				//							mParty.Gold += goldPlus;

				//							endMessage = $"일행은 {goldPlus}개의 금을 얻었다.";

				//							AppendText(new string[] { endMessage, "" });
				//						}

				//						if (mBattleEvent == 1)
				//						{
				//							AppendText(new string[] { $"[color={RGB.White}]당신들은 수감소 병사들을 물리쳤다.[/color]" }, true);
				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[49] |= 1 << 2;
				//							mMapLayer[50 + mMapWidth * 11] = 44;
				//							mMapLayer[51 + mMapWidth * 11] = 44;
				//							mMapLayer[49 + mMapWidth * 10] = 44;
				//							mMapLayer[52 + mMapWidth * 10] = 44;
				//						}
				//						else if (mBattleEvent == 2)
				//						{
				//							if (mParty.Etc[5] != 255)
				//							{
				//								if (mParty.Etc[5] == 0)
				//								{
				//									AppendText(new string[] {
				//										$"[color={RGB.White}]당신들은 미이라 장군을 물리쳤다.[/color]",
				//										$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]"
				//									}, true);

				//									ContinueText.Visibility = Visibility.Visible;

				//									mParty.Etc[12]++;
				//								}
				//							}
				//						}
				//						else if (mBattleEvent == 3)
				//						{
				//							mParty.Etc[43] |= 1;

				//							mParty.Map = 23;
				//							mParty.XAxis = 24;
				//							mParty.YAxis = 44;

				//							await RefreshGame();
				//						}
				//						else if (mBattleEvent == 4)
				//						{
				//							AppendText(new string[] { $"[color={RGB.White}]당신은 아키가고일을 물리쳤다.[/color]" }, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[13]++;
				//						}
				//						else if (mBattleEvent == 5)
				//						{
				//							mParty.Etc[36] = 3;
				//						}
				//						else if (mBattleEvent == 6)
				//						{
				//							AppendText(new string[] {
				//								$"[color={RGB.White}]당신들은 히드라를 물리쳤다.[/color]",
				//								$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]",
				//								$"[color={RGB.White}]다시 워터 필드의 군주에게로 돌아가라.[/color]",
				//							}, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[14] = 2;
				//							mSpecialEvent = SpecialEventType.AfterBattleHydra;
				//						}
				//						else if (mBattleEvent == 7)
				//							mParty.Etc[38] |= 1 << 2;
				//						else if (mBattleEvent == 8)
				//						{
				//							AppendText(new string[] {
				//								$"[color={RGB.White}]당신들은 거대 드래곤을 물리쳤다.[/color]",
				//								$"[color={RGB.LightCyan}]그리고 당신은 이 임무에 성공했다.[/color]",
				//								$"[color={RGB.White}]다시 워터 필드의 군주에게로 돌아가라.[/color]",
				//							}, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[14] = 4;
				//						}
				//						else if (mBattleEvent == 9)
				//						{
				//							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 49;
				//						}
				//						else if (mBattleEvent == 10)
				//						{
				//							AppendText(new string[] { " 당신은 이 동굴에 보관되어 있는 봉인을 발견했다. 그러고는 봉쇄되었던 봉인을 풀어버렸다." }, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[39] |= 1;
				//						}
				//						else if (mBattleEvent == 11)
				//							mParty.Etc[40] |= 1 << 3;
				//						else if (mBattleEvent == 12)
				//						{
				//							mParty.Etc[40] |= 1 << 1;
				//							await CheckMuddyFinalBattle();
				//							return;
				//						}
				//						else if (mBattleEvent == 13)
				//						{
				//							mParty.Etc[40] |= 1 << 2;
				//							await CheckMuddyFinalBattle();
				//							return;
				//						}
				//						else if (mBattleEvent == 14)
				//						{
				//							DefeatAstralMud();
				//						}
				//						else if (mBattleEvent == 15)
				//							await CheckPassSwampKeepExitEvent();
				//						else if (mBattleEvent == 16)
				//							SwampKeepBattleEvent();
				//						else if (mBattleEvent == 17)
				//							await DefeatImperiumMinorKeeper();
				//						else if (mBattleEvent == 18)
				//							mParty.Etc[42] |= 1 << 1;
				//						else if (mBattleEvent == 19)
				//							mParty.Etc[42] |= 1;
				//						else if (mBattleEvent == 20)
				//							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
				//						else if (mBattleEvent == 21)
				//						{
				//							mBattleTurn = BattleTurn.None;

				//							mEncounterEnemyList.Clear();

				//							var enemy = JoinEnemy(60);
				//							enemy.Name = "네크로맨서";
				//							enemy.ENumber = 0;

				//							DisplayEnemy();

				//							AppendText(new string[] {
				//								$"[color={RGB.LightMagenta}] 환상에서 벗어나다니 대단한 의지력이군.[/color]",
				//								$"[color={RGB.LightMagenta}] 하지만 진짜 적은 바로 나다. 받아라 !![/color]"
				//							}, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mBattleEvent = 0;
				//							mSpecialEvent = SpecialEventType.BattleFackNecromancer;
				//							return;
				//						}
				//						else if (mBattleEvent == 22)
				//						{
				//							AppendText(new string[] { $"[color={RGB.LightMagenta}] 욱! 너의 힘은 대단하구나. 나는 너에게 졌다고 인정하겠다. 흐흐, 그러나 사실 나는 너희 찾던 네크로맨서님이 아니다. 만약 그분이라 이렇게 쉽게 당하지는 않았을 테니까." +
				//							" 내 생명이 얼마 안남았구나. 네크로맨서님 만세 !![/color]" }, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mSpecialEvent = SpecialEventType.AfterBattleFakeNecromancer;
				//						}
				//						else if (mBattleEvent == 23)
				//						{
				//							for (var x = 23; x < 27; x++)
				//								mMapLayer[x + mMapWidth * mParty.YAxis] = 41;

				//							AppendText(new string[] { " 당신이 적을 물리치자 조금후에 이상하리만큼 편안한 기운이 일행을 감쌌다." }, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mSpecialEvent = SpecialEventType.AfterBattlePanzerViper;
				//						}
				//						else if (mBattleEvent == 24)
				//							WinNecromancer();
				//						else if (mBattleEvent == 25)
				//						{
				//							AppendText(new string[] { $"[color={RGB.White}]당신들은 고르곤을 물리쳤다.[/color]" }, true);

				//							ContinueText.Visibility = Visibility.Visible;

				//							mParty.Etc[37] |= 1 << 4;
				//						}
				//						else if (mBattleEvent == 26)
				//						{
				//							await CheckImperiumMinorEntraceBattleResult();

				//							return;
				//						}
				//						else if (mBattleEvent == 27)
				//							await CheckDungeonOfEvilBattleResult();
				//						else if (mBattleEvent == 28)
				//						{
				//							mParty.Map = 26;
				//							mParty.XAxis = 24;
				//							mParty.YAxis = 14;

				//							await RefreshGame();

				//							mFace = 5;

				//							InvokeAnimation(AnimationType.EnterChamberOfNecromancer);
				//						}

				//						mEncounterEnemyList.Clear();
				//						mBattleEvent = 0;

				//						ShowMap();

				//					}
				//					else if (mBattleTurn == BattleTurn.RunAway)
				//					{
				//						AppendText(new string[] { "" });

				//						if (mBattleEvent == 5)
				//						{
				//							var aliveWivernCount = 0;
				//							foreach (var enemy in mEncounterEnemyList)
				//							{
				//								if (!enemy.Dead)
				//									aliveWivernCount++;
				//							}

				//							mParty.Etc[36] = 3 - aliveWivernCount;
				//						}
				//						else if (mBattleEvent == 6)
				//						{
				//							mParty.XAxis++;
				//						}
				//						else if (mBattleEvent == 8)
				//						{
				//							mParty.XAxis = 24;
				//							mParty.YAxis = 93;
				//						}
				//						else if (mBattleEvent == 10)
				//							mParty.YAxis++;
				//						else if (mBattleEvent == 12)
				//							mParty.YAxis++;
				//						else if (mBattleEvent == 13)
				//							mParty.YAxis++;
				//						else if (mBattleEvent == 14)
				//						{
				//							if (mEncounterEnemyList[6].Dead)
				//								DefeatAstralMud();
				//							else
				//								mParty.YAxis++;
				//						}
				//						else if (mBattleEvent == 15)
				//							await CheckPassSwampKeepExitEvent();
				//						else if (mBattleEvent == 16)
				//							SwampKeepBattleEvent();
				//						else if (mBattleEvent == 17)
				//							await DefeatImperiumMinorKeeper();
				//						else if (mBattleEvent == 20)
				//							mMapLayer[mParty.XAxis + mMapWidth * mParty.YAxis] = 40;
				//						else if (mBattleEvent == 21)
				//						{
				//							Talk(" 하지만 당신은 환상에서 벗어나지 못했다.");
				//							mSpecialEvent = SpecialEventType.FailRunawayBattleFakeNecromancer;

				//							return;
				//						}
				//						else if (mBattleEvent == 22)
				//							mParty.YAxis++;
				//						else if (mBattleEvent == 23)
				//							mParty.YAxis++;
				//						else if (mBattleEvent == 24)
				//						{
				//							if (!mEncounterEnemyList[6].Dead)
				//							{
				//								Talk($" [color={RGB.LightMagenta}]하지만 나에게 도전한 이상 도주는 허용할 수 없다는 점이 안타깝군.[/color]");

				//								mSpecialEvent = SpecialEventType.FailRunawayBattleNecromancer;
				//								return;
				//							}
				//							else
				//								WinNecromancer();
				//						}
				//						else if (mBattleEvent == 25)
				//						{
				//							mParty.YAxis++;
				//						}
				//						else if (mBattleEvent == 26)
				//						{
				//							await CheckImperiumMinorEntraceBattleResult();
				//							return;
				//						}
				//						else if (mBattleEvent == 27)
				//							await CheckDungeonOfEvilBattleResult();
				//						else if (mBattleEvent == 28)
				//						{
				//							mParty.XAxis = 24;
				//							mParty.YAxis = 44;
				//						}

				//						mEncounterEnemyList.Clear();
				//						mBattleEvent = 0;
				//						ShowMap();
				//					}
				//					else if (mBattleTurn == BattleTurn.Lose)
				//					{
				//						ShowGameOver(new string[] {
				//							$"[color={RGB.LightMagenta}]일행은 모두 전투에서 패했다 !![/color]",
				//							$"[color={RGB.LightGreen}]    어떻게 하시겠습니까 ?[/color]"
				//						});
				//					}

				//					mBattleTurn = BattleTurn.None;
				//				}

				void AddBattleCommand(bool skip = false)
				{
					mMenuMode = MenuMode.None;

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
							ShowTrainSkillMenu();
						else if (specialEvent == SpecialEventType.TrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (specialEvent == SpecialEventType.TrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForSword)
							ShowChooseChangeSwordMemberMenu();
						else if (specialEvent == SpecialEventType.ChangeJobForMagic)
							ShowChooseChangeMagicMemberMenu();
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

							specialEvent = SpecialEventType.MeetLordAhn5;
						}
						else if (mSpecialEvent == SpecialEventType.MeetLordAhn5)
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
						else if (mSpecialEvent == SpecialEventType.MeetLordAhn11)
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
						else if (specialEvent == SpecialEventType.DestructCastleLore) {
							Talk(new string[] {
								" 다크 메이지는 가공할 힘으로 전 대륙에 결계를 형성하기 시작했고 결계속의 물체들은 서서히 형체를 잃어가기 시작했다." +
								"  이제는 실리안 카미너스를 제어할 수있는 메피스토펠레스마저 사라져 버려서  그녀는 의지의 중심을 잃고 한없이 그녀의 힘을 방출하기 시작했다.",
								" 이제는 누구도 그녀의 기하 급수적인 힘의 폭주를 막을 수가 없었고 이미 당신의 의식도 흐려져 갔다."
							});

							mSpecialEvent = SpecialEventType.DestructCastleLore2;
						}
						else if (specialEvent == SpecialEventType.DestructCastleLore2) {
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
						else if (specialEvent == SpecialEventType.DestructCastleLore3) {
							AppendText(new string[] {
								" 그의 의지는 점점 희미해 지더니 결국 암흑으로 사라지고 말았다.",
								" 당신은 어떻게 하겠는가?"
							});

							ShowMenu(MenuMode.FinalChoice, new string[] {
								"레드 안타레스의 말대로 한다",
								"다크 메이지와 싸우고 싶다"
							});
						}
						else if (specialEvent == SpecialEventType.EnterUnderworld) {
							InvokeAnimation(AnimationType.EnterUnderworld);
						}
						else if (mSpecialEvent == SpecialEventType.SeeDeadBody) {
							mFace = 5;
							InvokeAnimation(AnimationType.GoInsideMenace);
						}
						else if (mSpecialEvent == SpecialEventType.BattleMenace) {
							
						}
						else if (mSpecialEvent == SpecialEventType.BackToBattleMode) {
							BattleMode();
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp ||
						args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft ||
						args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight ||
						args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
						return;

					if (StatPanel.Visibility == Visibility.Visible)
						StatPanel.Visibility = Visibility.Collapsed;

					if (StatHealthPanel.Visibility == Visibility.Visible)
						StatHealthPanel.Visibility = Visibility.Collapsed;

					if (DialogText.Visibility == Visibility.Collapsed)
						DialogText.Visibility = Visibility.Visible;

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
					else if (mSpecialEvent > 0)
						await InvokeSpecialEventLaterPart();
					else if (mCureBattle)
					{
						mCureBattle = false;
						//AddBattleCommand(true);
					}
					else if (mPenetration == true)
					{
						mPenetration = false;
					}
					else if (mTelescopeXCount != 0 || mTelescopeYCount != 0)
					{
						if ((mTelescopeXCount != 0 && (mParty.XAxis + mTelescopeXCount <= 4 || mParty.XAxis + mTelescopeXCount >= mMapWidth - 3)) ||
							(mTelescopeXCount != 0 && (mParty.YAxis + mTelescopeYCount <= 4 || mParty.YAxis + mTelescopeYCount >= mMapHeight - 3)))
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
							Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
					}
					else if (mBattleTurn == BattleTurn.Player)
					{
						//if (mBattleCommandQueue.Count == 0)
						//{
						//	var allUnavailable = true;
						//	foreach (var enemy in mEncounterEnemyList)
						//	{
						//		if (!enemy.Dead && !enemy.Unconscious)
						//		{
						//			allUnavailable = false;
						//			break;
						//		}
						//	}

						//	if (allUnavailable)
						//	{
						//		mBattleTurn = BattleTurn.Win;
						//		await EndBattle();
						//	}
						//	else
						//		ExecuteBattle();
						//}
						//else
						//	ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.Enemy)
					{
						//ExecuteBattle();
					}
					else if (mBattleTurn == BattleTurn.RunAway || mBattleTurn == BattleTurn.Win || mBattleTurn == BattleTurn.Lose)
					{
						//await EndBattle();
					}
					else if (mWeaponShopEnd)
					{
						//mWeaponShopEnd = false;
						//GoWeaponShop();
					}
					else if (mCureMenuState == CureMenuState.NotCure)
					{
						//mCureMenuState = CureMenuState.None;
						//ShowHealType();
					}
					else if (mCureMenuState == CureMenuState.CureEnd)
					{
						//mCureMenuState = CureMenuState.None;
						//GoHospital();
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
						"게임 선택 상황"
					});
				}
				//				else if (mSpinnerType != SpinnerType.None)
				//				{
				//					void CloseSpinner()
				//					{
				//						SpinnerText.TextHighlighters.Clear();
				//						SpinnerText.Blocks.Clear();
				//						SpinnerText.Visibility = Visibility.Collapsed;

				//						mSpinnerItems = null;
				//						mSpinnerID = 0;
				//						mSpinnerType = SpinnerType.None;
				//					}

				//					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
				//					{
				//						mSpinnerID = (mSpinnerID + 1) % mSpinnerItems.Length;

				//						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
				//					}
				//					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
				//					{
				//						if (mSpinnerID == 0)
				//							mSpinnerID = mSpinnerItems.Length - 1;
				//						else
				//							mSpinnerID--;

				//						AppendText(SpinnerText, mSpinnerItems[mSpinnerID].Item1);
				//					}
				//					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB)
				//					{
				//						if (mSpinnerType == SpinnerType.TeleportationRange)
				//						{
				//							AppendText("");

				//							CloseSpinner();
				//						}
				//					}
				//					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
				//					{
				//						if (mSpinnerType == SpinnerType.TeleportationRange)
				//						{
				//							int moveX = mParty.XAxis;
				//							int moveY = mParty.YAxis;

				//							switch (mTeleportationDirection)
				//							{
				//								case 0:
				//									moveY -= mSpinnerItems[mSpinnerID].Item2;
				//									break;
				//								case 1:
				//									moveY += mSpinnerItems[mSpinnerID].Item2;
				//									break;
				//								case 2:
				//									moveX += mSpinnerItems[mSpinnerID].Item2;
				//									break;
				//								case 3:
				//									moveX -= mSpinnerItems[mSpinnerID].Item2;
				//									break;
				//							}

				//							if (moveX < 4 || moveX > mMapWidth - 4 || moveY < 4 || moveY > mMapHeight - 4)
				//								AppendText("공간 이동이 통하지 않습니다.");
				//							else
				//							{
				//								var valid = false;
				//								if (mPosition == PositionType.Town)
				//								{
				//									if (27 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
				//										valid = true;
				//								}
				//								else if (mPosition == PositionType.Ground)
				//								{
				//									if (24 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
				//										valid = true;
				//								}
				//								else if (mPosition == PositionType.Den)
				//								{
				//									if (41 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
				//										valid = true;
				//								}
				//								else if (mPosition == PositionType.Keep)
				//								{
				//									if (27 <= mMapLayer[moveX + mMapWidth * moveY] && mMapLayer[moveX + mMapWidth * moveY] <= 47)
				//										valid = true;
				//								}

				//								if (!valid)
				//									AppendText("공간 이동 장소로 부적합 합니다.");
				//								else
				//								{
				//									mMagicPlayer.SP -= 50;

				//									if (mMapLayer[moveX + mMapWidth * moveY] == 0 || ((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[moveX + mMapWidth * moveY] == 52))
				//										AppendText($"[color={RGB.LightMagenta}]알 수 없는 힘이 당신을 배척합니다.[/color]");
				//									else
				//									{
				//										mParty.XAxis = moveX;
				//										mParty.YAxis = moveY;

				//										AppendText($"[color={RGB.White}]공간 이동 마법이 성공했습니다.[/color]");
				//									}
				//								}
				//							}
				//						}

				//						CloseSpinner();
				//					}
				//				}
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
						AppendText(new string[] { "누구에게" });
						string[] playerList;

						int availCount;
						if (player.ClassType == ClassCategory.Magic)
							availCount = player.CureMagic / 10;
						else
							availCount = player.AxeSkill / 10;

						if (availCount < 6)
							playerList = new string[mPlayerList.Count];
						else
						{
							playerList = new string[mPlayerList.Count + 1];
							playerList[playerList.Length - 1] = "모든 사람들에게";
						}


						for (var i = 0; i < mPlayerList.Count; i++)
							playerList[i] = mPlayerList[i].Name;
						
						ShowMenu(menuMode, playerList);
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
						if (mMenuMode == MenuMode.ChooseTrainSkill)
							ShowChooseTrainSkillMemberMenu();
						else if (mMenuMode == MenuMode.ChooseTrainMagic)
							ShowChooseTrainMagicMemberMenu();
						else if (mMenuMode != MenuMode.None && mMenuMode != MenuMode.BattleLose && mMenuMode != MenuMode.ChooseGameOverLoadGame && mSpecialEvent == SpecialEventType.None)
						{
							AppendText("");
							HideMenu();

							if (mMenuMode == MenuMode.CastOneMagic ||
							mMenuMode == MenuMode.CastAllMagic ||
							mMenuMode == MenuMode.CastSpecial ||
							mMenuMode == MenuMode.ChooseBattleCureSpell ||
							mMenuMode == MenuMode.CastESP ||
							mMenuMode == MenuMode.CastSummon)
							{
								BattleMode();
							}
							else if (mMenuMode == MenuMode.ChooseESPMagic) {
								ShowCastESPMenu();
							}
							else if (mMenuMode == MenuMode.EnemySelectMode)
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
							else if (mMenuMode == MenuMode.ApplyBattleCureSpell || mMenuMode == MenuMode.ApplyBattleCureAllSpell)
								ShowCureDestMenu(mPlayerList[mBattlePlayerID], MenuMode.ChooseBattleCureSpell);
							else if (mMenuMode == MenuMode.BattleStart ||
								mMenuMode == MenuMode.BattleCommand)
								return;
							else if (mMenuMode == MenuMode.ConfirmExitMap)
							{
								mParty.YAxis--;

								mMenuMode = MenuMode.None;
							}
							else
								mMenuMode = MenuMode.None;
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
								mBattleEvent = 0;

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

							if (mMapHeight <= 80) {
								yInit = 0;
								Height = mMapHeight;
							}

							if (width == 0 || height == 0) {
								xInit = mParty.XAxis - 50;
								yInit = mParty.YAxis - 50;

								if (xInit <= 0)
								{
									xInit = 0;

									if (mMapWidth > 100)
										width = 100;
									else
										width = mMapWidth;
								}

								if (yInit <= 0)
								{
									yInit = 0;

									if (mMapHeight > 100)
										height = 100;
									else
										height = mMapHeight;
								}


								if (xInit + 50 > mMapWidth)
								{
									if (mMapWidth > 100) {
										xInit = mMapWidth - 100;
										width = 100;
									}
									else {
										xInit = 0;
										width = mMapWidth;
									}
								}

								if (yInit + 50 > mMapHeight)
								{
									if (mMapWidth > 100)
									{
										yInit = mMapHeight - 100;
										height = 100;
									}
									else
									{
										yInit = 0;
										height = mMapHeight;
									}
								}
							}

							lock(mWizardEye) {
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
									}
								}
							}

							MapCanvas.Visibility = Visibility.Visible;
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

						var menuMode = HideMenu();

						if (menuMode == MenuMode.EnemySelectMode)
						{
							AddBattleCommand();
						}							
						else if (menuMode == MenuMode.Game)
						{
							if (mMenuFocusID == 0)
							{
								//ShowPartyStatus();
							}
							else if (mMenuFocusID == 1)
							{
								AppendText(new string[] { "능력을 보고 싶은 인물을 선택하시오" });
								//ShowCharacterMenu(MenuMode.ViewCharacter);
							}
							else if (mMenuFocusID == 2)
							{
								AppendText("");
								DialogText.Visibility = Visibility.Collapsed;

								for (var i = 0; i < 6; i++)
								{
									if (i < mPlayerList.Count)
										mHealthTextList[i].Update(mPlayerList[i].Name, mPlayerList[i].Poison, mPlayerList[i].Unconscious, mPlayerList[i].Dead);
									else
										mHealthTextList[i].Clear();
								}

								StatHealthPanel.Visibility = Visibility.Visible;
								ContinueText.Visibility = Visibility.Visible;
							}
							else if (mMenuFocusID == 3)
							{
								AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
								//ShowCharacterMenu(MenuMode.CastSpell);
							}
							else if (mMenuFocusID == 4)
							{
								AppendText(new string[] { $"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]" }, true);
								//ShowCharacterMenu(MenuMode.Extrasense);
							}
							else if (mMenuFocusID == 5)
							{
								//Rest();
							}
							else if (mMenuFocusID == 6)
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
						//else if (menuMode == MenuMode.ViewCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	AppendText("");

						//	StatPlayerName.Text = mPlayerList[mMenuFocusID].Name;
						//	StatPlayerGender.Text = mPlayerList[mMenuFocusID].GenderName;
						//	StatPlayerClass.Text = Common.GetClassStr(mPlayerList[mMenuFocusID]);

						//	StatStrength.Text = mPlayerList[mMenuFocusID].Strength.ToString();
						//	StatMentality.Text = mPlayerList[mMenuFocusID].Mentality.ToString();
						//	StatConcentration.Text = mPlayerList[mMenuFocusID].Concentration.ToString();
						//	StatEndurance.Text = mPlayerList[mMenuFocusID].Endurance.ToString();

						//	StatResistance.Text = mPlayerList[mMenuFocusID].Resistance.ToString();
						//	StatAgility.Text = mPlayerList[mMenuFocusID].Agility.ToString();
						//	StatLuck.Text = mPlayerList[mMenuFocusID].Luck.ToString();

						//	StatWeaponAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[0].ToString();
						//	StatMagicAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[1].ToString();
						//	StatESPAccuracy.Text = mPlayerList[mMenuFocusID].Accuracy[2].ToString();

						//	StatAttackLevel.Text = mPlayerList[mMenuFocusID].Level[0].ToString();
						//	StatMagicLevel.Text = mPlayerList[mMenuFocusID].Level[1].ToString();
						//	StatESPLevel.Text = mPlayerList[mMenuFocusID].Level[2].ToString();

						//	StatExp.Text = mPlayerList[mMenuFocusID].Experience.ToString();

						//	StatWeapon.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Weapon);
						//	StatShield.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Shield);
						//	StatArmor.Text = Common.GetWeaponStr(mPlayerList[mMenuFocusID].Armor);

						//	DialogText.Visibility = Visibility.Collapsed;
						//	StatPanel.Visibility = Visibility.Visible;

						//	ContinueText.Visibility = Visibility.Visible;
						//}
						//else if (menuMode == MenuMode.CastSpell)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mPlayerList[mMenuFocusID].IsAvailable)
						//	{
						//		mMagicPlayer = mPlayerList[mMenuFocusID];
						//		AppendText(new string[] { "사용할 마법의 종류 ===>" });
						//		ShowMenu(MenuMode.SpellCategory, new string[]
						//		{
						//							"공격 마법",
						//							"치료 마법",
						//							"변화 마법"
						//		});
						//	}
						//	else
						//	{
						//		AppendText(new string[] { $"{GetGenderData(mPlayerList[mMenuFocusID])}는 마법을 사용할 수 있는 상태가 아닙니다" });
						//	}
						//}
						//else if (menuMode == MenuMode.SpellCategory)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//	{
						//		AppendText(new string[] { "전투 모드가 아닐 때는 공격 마법을 사용할 수 없습니다." });
						//		ContinueText.Visibility = Visibility.Visible;
						//	}
						//	else if (mMenuFocusID == 1)
						//	{
						//		ShowCureDestMenu(mMagicPlayer, MenuMode.ChooseCureSpell);
						//	}
						//	else if (mMenuFocusID == 2)
						//	{
						//		mMenuMode = MenuMode.None;

						//		AppendText(new string[] { "선택" });

						//		int availableCount;
						//		if (mMagicPlayer.Level[1] > 1)
						//			availableCount = mMagicPlayer.Level[1] / 2 + 1;
						//		else
						//			availableCount = 1;

						//		if (availableCount > 7)
						//			availableCount = 7;

						//		var totalMagicCount = 40 - 33 + 1;
						//		if (availableCount < totalMagicCount)
						//			totalMagicCount = availableCount;

						//		var phenominaMagicMenu = new string[totalMagicCount];
						//		for (var i = 33; i < 33 + availableCount; i++)
						//			phenominaMagicMenu[i - 33] = Common.GetMagicStr(i);

						//		ShowMenu(MenuMode.ApplyPhenominaMagic, phenominaMagicMenu);
						//	}
						//}
						//else if (menuMode == MenuMode.ChooseCureSpell)
						//{
						//	mMenuMode = MenuMode.None;

						//	AppendText(new string[] { "선택" });

						//	if (mMenuFocusID < mPlayerList.Count)
						//		mMagicWhomPlayer = mPlayerList[mMenuFocusID];

						//	ShowCureSpellMenu(mMagicPlayer, mMenuFocusID, MenuMode.ApplyCureMagic, MenuMode.ApplyCureAllMagic);
						//}
						else if (menuMode == MenuMode.ChooseBattleCureSpell)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID < mPlayerList.Count)
								mMagicWhomPlayer = mPlayerList[mMenuFocusID];
							else {
								int curePoint;
								if (mMagicPlayer.ClassType == ClassCategory.Magic)
									curePoint = mMagicPlayer.CureMagic / 10 - 5;
								else
									curePoint = mMagicPlayer.AxeSkill / 10 - 5;

								if (curePoint < 1) {
									Talk("강한 치료 마법은 아직 불가능 합니다.");

									mSpecialEvent = SpecialEventType.BackToBattleMode;
									return;
								}
							}

							ShowCureSpellMenu(mPlayerList[mBattlePlayerID], mMenuFocusID, MenuMode.ApplyBattleCureSpell, MenuMode.ApplyBattleCureAllSpell);
						}
						//else if (menuMode == MenuMode.ApplyCureMagic)
						//{
						//	mMenuMode = MenuMode.None;

						//	DialogText.TextHighlighters.Clear();
						//	DialogText.Blocks.Clear();

						//	CureSpell(mMagicPlayer, mMagicWhomPlayer, mMenuFocusID, mCureResult);

						//	ShowCureResult();
						//}
						//else if (menuMode == MenuMode.ApplyCureAllMagic)
						//{
						//	mMenuMode = MenuMode.None;

						//	DialogText.TextHighlighters.Clear();
						//	DialogText.Blocks.Clear();

						//	CureAllSpell(mMagicPlayer, mMenuFocusID, mCureResult);

						//	ShowCureResult();
						//}
						//else if (menuMode == MenuMode.ApplyPhenominaMagic)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//	{
						//		if (mMagicPlayer.SP < 1)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			if (mParty.Etc[0] < 255)
						//			{
						//				mParty.Etc[0]++;
						//				ShowMap();
						//			}

						//			AppendText(new string[] { $"[color={RGB.White}]일행은 마법의 횃불을 밝혔습니다.[/color]" });
						//			mMagicPlayer.SP--;
						//			DisplaySP();
						//		}
						//	}
						//	else if (mMenuFocusID == 1)
						//	{
						//		if (mMagicPlayer.SP < 5)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			mParty.Etc[3] = 255;

						//			AppendText(new string[] { $"[color={RGB.White}]일행은 공중부상중 입니다.[/color]" });
						//			mMagicPlayer.SP -= 5;
						//			DisplaySP();
						//		}
						//	}
						//	else if (mMenuFocusID == 2)
						//	{
						//		if (mMagicPlayer.SP < 10)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			mParty.Etc[1] = 255;

						//			AppendText(new string[] { $"[color={RGB.White}]일행은 물 위를 걸을 수 있습니다.[/color]" });
						//			mMagicPlayer.SP -= 10;
						//			DisplaySP();
						//		}
						//	}
						//	else if (mMenuFocusID == 3)
						//	{
						//		if (mMagicPlayer.SP < 20)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			mParty.Etc[2] = 255;

						//			AppendText(new string[] { $"[color={RGB.White}]일행은 늪 위를 걸을 수 있습니다.[/color]" });
						//			mMagicPlayer.SP -= 20;
						//			DisplaySP();
						//		}
						//	}
						//	else if (mMenuFocusID == 4)
						//	{
						//		if (mMagicPlayer.SP < 25)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" });

						//			ShowMenu(MenuMode.VaporizeMoveDirection, new string[] { "북쪽으로 기화 이동",
						//								"남쪽으로 기화 이동",
						//								"동쪽으로 기화 이동",
						//								"서쪽으로 기화 이동" });
						//		}
						//	}
						//	else if (mMenuFocusID == 5)
						//	{
						//		if (mParty.Map == 20 || mParty.Map == 25 || mParty.Map == 26)
						//			AppendText(new string[] { $"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
						//		else if (mMagicPlayer.SP < 30)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

						//			ShowMenu(MenuMode.TransformDirection, new string[] { "북쪽에 지형 변화",
						//								"남쪽에 지형 변화",
						//								"동쪽에 지형 변화",
						//								"서쪽에 지형 변화" });
						//		}
						//	}
						//	else if (mMenuFocusID == 6)
						//	{
						//		if (mParty.Map == 20 || mParty.Map == 25 || mParty.Map == 26)
						//			AppendText(new string[] { $"[color={RGB.LightMagenta}]이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
						//		else if (mMagicPlayer.SP < 50)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

						//			ShowMenu(MenuMode.TeleportationDirection, new string[] { "북쪽으로 공간이동",
						//								"남쪽으로 공간이동",
						//								"동쪽으로 공간이동",
						//								"서쪽으로 공간이동" });
						//		}
						//	}
						//	else if (mMenuFocusID == 7)
						//	{
						//		if (mMagicPlayer.SP < 30)
						//			ShowNotEnoughSP();
						//		else
						//		{
						//			var count = mPlayerList.Count;
						//			if (mParty.Food + count > 255)
						//				mParty.Food = 255;
						//			else
						//				mParty.Food = mParty.Food + count;
						//			mMagicPlayer.SP -= 30;
						//			DisplaySP();

						//			AppendText(new string[] { $"[color={RGB.White}]식량 제조 마법은 성공적으로 수행되었습니다[/color]",
						//							$"[color={RGB.White}]            {count} 개의 식량이 증가됨[/color]",
						//							$"[color={RGB.White}]      일행의 현재 식량은 {mParty.Food} 개 입니다[/color]" }, true);
						//		}
						//	}

						//	DisplaySP();
						//}
						//else if (menuMode == MenuMode.VaporizeMoveDirection)
						//{
						//	mMenuMode = MenuMode.None;

						//	int xOffset = 0, yOffset = 0;
						//	switch (mMenuFocusID)
						//	{
						//		case 0:
						//			yOffset = -1;
						//			break;
						//		case 1:
						//			yOffset = 1;
						//			break;
						//		case 2:
						//			xOffset = 1;
						//			break;
						//		case 3:
						//			xOffset = -1;
						//			break;
						//	}

						//	var newX = mParty.XAxis + 2 * xOffset;
						//	var newY = mParty.YAxis + 2 * yOffset;

						//	var canMove = false;

						//	var moveTile = mMapLayer[newX + mMapWidth * newY];
						//	switch (mPosition)
						//	{
						//		case PositionType.Town:
						//			if (moveTile == 0 || (27 <= moveTile && moveTile <= 47))
						//				canMove = true;
						//			break;
						//		case PositionType.Ground:
						//			if (moveTile == 0 || (24 <= moveTile && moveTile <= 47))
						//				canMove = true;
						//			break;
						//		case PositionType.Den:
						//			if (moveTile == 0 || (41 <= moveTile && moveTile <= 47))
						//				canMove = true;
						//			break;
						//		case PositionType.Keep:
						//			if (moveTile == 0 || (40 <= moveTile && moveTile <= 47))
						//				canMove = true;
						//			break;

						//	}

						//	if (!canMove)
						//		AppendText(new string[] { $"기화 이동이 통하지 않습니다." }, true);
						//	else
						//	{
						//		mMagicPlayer.SP -= 25;
						//		DisplaySP();

						//		if (mMapLayer[(mParty.XAxis + xOffset) + mMapWidth * (mParty.YAxis + yOffset)] == 0 ||
						//			((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[(newX + xOffset) + mMapWidth * (newY + yOffset)] == 52))
						//		{
						//			AppendText(new string[] { $"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
						//		}
						//		else
						//		{
						//			mParty.XAxis = newX;
						//			mParty.YAxis = newY;

						//			AppendText(new string[] { $"[color={RGB.White}]기화 이동을 마쳤습니다.[/color]" }, true);
						//		}

						//	}
						//}
						//else if (menuMode == MenuMode.TransformDirection)
						//{

						//	mMenuMode = MenuMode.None;

						//	int xOffset = 0, yOffset = 0;
						//	switch (mMenuFocusID)
						//	{
						//		case 0:
						//			yOffset = -1;
						//			break;
						//		case 1:
						//			yOffset = 1;
						//			break;
						//		case 2:
						//			xOffset = 1;
						//			break;
						//		case 3:
						//			xOffset = -1;
						//			break;
						//	}

						//	var newX = mParty.XAxis + xOffset;
						//	var newY = mParty.YAxis + yOffset;


						//	mMagicPlayer.SP -= 30;
						//	DisplaySP();

						//	if (mMapLayer[newX + mMapWidth * newY] == 0 ||
						//			((mPosition == PositionType.Den || mPosition == PositionType.Keep) && mMapLayer[newX + mMapWidth * newY] == 52))
						//	{
						//		AppendText(new string[] { $"[color={RGB.LightMagenta}]알 수 없는 힘이 당신의 마법을 배척합니다.[/color]" }, true);
						//	}
						//	else
						//	{
						//		byte tile;

						//		switch (mPosition)
						//		{
						//			case PositionType.Town:
						//				tile = 47;
						//				break;
						//			case PositionType.Ground:
						//				tile = 41;
						//				break;
						//			case PositionType.Den:
						//				tile = 43;
						//				break;
						//			default:
						//				tile = 43;
						//				break;
						//		}

						//		mMapLayer[newX + mMapWidth * newY] = tile;

						//		AppendText($"[color={RGB.White}]지형 변화에 성공했습니다.[/color]");
						//	}
						//}
						//else if (menuMode == MenuMode.TeleportationDirection)
						//{
						//	mMenuMode = MenuMode.None;

						//	mTeleportationDirection = mMenuFocusID;

						//	var rangeItems = new List<Tuple<string, int>>();
						//	for (var i = 1; i <= 9; i++)
						//	{
						//		rangeItems.Add(new Tuple<string, int>($"[color={RGB.White}]##[/color] [color={RGB.LightGreen}]{i * 1000}[/color] [color={RGB.White}] 공간 이동력[/color]", i));
						//	}

						//	ShowSpinner(SpinnerType.TeleportationRange, rangeItems.ToArray(), 5);
						//}
						//else if (menuMode == MenuMode.Extrasense)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mPlayerList[mMenuFocusID].IsAvailable)
						//	{
						//		if (mPlayerList[mMenuFocusID].Class != 2 && mPlayerList[mMenuFocusID].Class != 3 && mPlayerList[mMenuFocusID].Class != 6 && (mParty.Etc[37] & 1) == 0)
						//		{
						//			AppendText(new string[] { $"당신에게는 아직 능력이 없습니다.'" }, true);
						//			ContinueText.Visibility = Visibility.Visible;
						//		}
						//		else
						//		{
						//			mMagicPlayer = mPlayerList[mMenuFocusID];

						//			AppendText(new string[] { "사용할 초감각의 종류 ===>" });

						//			var extrsenseMenu = new string[5];
						//			for (var i = 41; i <= 45; i++)
						//				extrsenseMenu[i - 41] = Common.GetMagicStr(i);

						//			ShowMenu(MenuMode.ChooseExtrasense, extrsenseMenu);
						//		}
						//	}
						//	else
						//	{
						//		AppendText(new string[] { $"{GetGenderData(mPlayerList[mMenuFocusID])}는 초감각을 사용할수있는 상태가 아닙니다" });
						//	}
						//}
						//else if (menuMode == MenuMode.ChooseExtrasense)
						//{
						//	HideMenu();
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//	{
						//		if (mParty.Map > 24)
						//			AppendText(new string[] { $" 이 동굴의 악의 힘이 이 마법을 방해합니다." });
						//		else if (mMagicPlayer.ESP < 10)
						//			ShowNotEnoughESP();
						//		else
						//		{
						//			mPenetration = true;
						//			Talk($"[color={RGB.White}]일행은 주위를 투시하고 있다.[/color]");
						//		}
						//	}
						//	else if (mMenuFocusID == 1)
						//	{
						//		if (mMagicPlayer.ESP < 5)
						//			ShowNotEnoughESP();
						//		else
						//		{
						//			int GetPredict()
						//			{
						//				int predict = -1;
						//				switch (mParty.Etc[9])
						//				{
						//					case 0:
						//					case 1:
						//					case 2:
						//						predict = 0;
						//						break;
						//					case 3:
						//						predict = 1;
						//						break;
						//					case 4:
						//					case 5:
						//						predict = 2;
						//						break;
						//					case 6:
						//						predict = 3;
						//						break;
						//				}

						//				if (predict == 3 && mParty.Map == 7)
						//					predict = 4;

						//				switch (mParty.Etc[12])
						//				{
						//					case 1:
						//						predict = 5;
						//						break;
						//					case 2:
						//						predict = 6;
						//						break;
						//					case 3:
						//						predict = 7;
						//						break;
						//				}

						//				if (mParty.Etc[12] == 3)
						//				{
						//					switch (mParty.Etc[13])
						//					{
						//						case 0:
						//							predict = 8;
						//							break;
						//						case 1:
						//							predict = 9;
						//							break;
						//						case 2:
						//							predict = 10;
						//							break;
						//						case 3:
						//							predict = 8;
						//							break;
						//						case 4:
						//							predict = 11;
						//							break;
						//						case 5:
						//							predict = 10;
						//							break;
						//						case 6:
						//							predict = 12;
						//							break;
						//					}

						//					if (mParty.Etc[13] == 6 && mParty.Map == 15)
						//						predict = 13;

						//					if (mParty.Etc[13] == 6 && mParty.Map == 2)
						//						predict = 12;
						//					else if (predict == 12)
						//					{
						//						switch (mParty.Etc[14])
						//						{
						//							case 0:
						//								predict = 14;
						//								break;
						//							case 1:
						//								predict = 15;
						//								break;
						//							case 2:
						//								predict = 14;
						//								break;
						//							case 3:
						//								predict = 16;
						//								break;
						//							case 4:
						//								predict = 14;
						//								break;
						//							case 5:
						//								predict = 17;
						//								break;
						//						}

						//						if (mParty.Map == 13 && mParty.Etc[39] % 2 == 0 && mParty.Etc[39] % 2 == 0)
						//							predict = 18;

						//						if (mParty.Etc[14] == 5)
						//						{
						//							if (mParty.Map == 4 || (19 <= mParty.Map && mParty.Map <= 21))
						//								predict = 20;

						//							if (mParty.Etc[39] % 2 == 1 && mParty.Etc[40] % 2 == 1)
						//								predict = 19;
						//						}

						//						if (mParty.Map == 5)
						//							predict = 22;

						//						if (mParty.Etc[14] == 5 && mParty.Map > 21)
						//						{
						//							switch (mParty.Map)
						//							{
						//								case 22:
						//								case 24:
						//									predict = 21;
						//									break;
						//								case 23:
						//									predict = 22;
						//									break;
						//								case 25:
						//									predict = 23;
						//									break;
						//								case 26:
						//									predict = 24;
						//									break;
						//							}
						//						}
						//					}
						//				}


						//				return predict;
						//			}

						//			var predictStr = new string[]
						//			{
						//								"로드 안을 만날",
						//								"메나스를 탐험할",
						//								"로드 안에게 다시 돌아갈",
						//								"라스트디치로 갈",
						//								"라스트디치의 성주를 만날",
						//								"피라미드 속의 미이라 장군을 물리칠",
						//								"라스트디치의 성주에게로 돌아갈",
						//								"라스트디치의 지하 출입구로 갈",
						//								"가이아 테라의 성주를 만날",
						//								"이블 씰에서 황금의 봉인을 발견할",
						//								"가이아 테라의 성주에게 돌아갈",
						//								"퀘이크에서 아키가고일를 물리칠",
						//								"북동쪽의 와이번 동굴에 갈",
						//								"워터 필드로 갈",
						//								"워터 필드의 군주를 만날",
						//								"노티스 속의 히드라를 물리칠",
						//								"락업 속의 드래곤을 물리칠",
						//								"가이아 테라의 스왐프 게이트로 갈",
						//								"위쪽의 게이트를 통해 스왐프 킵으로 갈",
						//								"스왐프 대륙에 존재하는 두개의 봉인을 풀",
						//								"스왐프 킵의 라바 게이트를 작동시킬",
						//								"적의 집결지인 이블 컨센츄레이션으로 갈",
						//								"숨겨진 적의 마지막 요새로 들어갈",
						//								"위쪽의 동굴에서 네크로맨서를 만날",
						//								"네크로맨서와 마지막 결전을 벌일"
						//			};

						//			if (mMagicPlayer.ESP < 5)
						//				ShowNotEnoughESP();
						//			else
						//			{
						//				var predict = GetPredict();

						//				AppendText(new string[] { $" 당신은 당신의 미래를 예언한다 ...", "" });
						//				if (0 <= predict && predict < predictStr.Length)
						//					AppendText(new string[] { $" # 당신은 [color={RGB.LightGreen}]{predictStr[predict]} 것이다[/color]" }, true);
						//				else
						//					AppendText(new string[] { $" # [color={RGB.LightGreen}]당신은 어떤 힘에 의해 예언을 방해 받고 있다[/color]" }, true);

						//				mMagicPlayer.ESP -= 5;
						//				DisplayESP();

						//				ContinueText.Visibility = Visibility.Visible;
						//			}
						//		}
						//	}
						//	else if (mMenuFocusID == 2)
						//	{
						//		if (mMagicPlayer.ESP < 20)
						//			ShowNotEnoughESP();
						//		else
						//		{
						//			AppendText(new string[] { $"[color={RGB.White}]당신은 잠시 동안 다른 사람의 마음을 읽을 수 있다.[/color]" });
						//			mParty.Etc[4] = 3;
						//		}
						//	}
						//	else if (mMenuFocusID == 3)
						//	{
						//		if (mParty.Map > 24)
						//			AppendText(new string[] { $"[color={RGB.LightMagenta}] 이 동굴의 악의 힘이 이 마법을 방해합니다.[/color]" });
						//		else if (mMagicPlayer.ESP < mMagicPlayer.Level[2] * 5)
						//			ShowNotEnoughESP();
						//		else
						//		{
						//			AppendText(new string[] { $"[color={RGB.White}]<<<  방향을 선택하시오  >>>[/color]" }, true);

						//			ShowMenu(MenuMode.TelescopeDirection, new string[] { "북쪽으로 천리안을 사용",
						//								"남쪽으로 천리안을 사용",
						//								"동쪽으로 천리안을 사용",
						//								"서쪽으로 천리안을 사용" });
						//		}
						//	}
						//	else if (mMenuFocusID == 4)
						//	{
						//		AppendText(new string[] { $"{Common.GetMagicStr(45)}은 전투 모드에서만 사용됩니다." });
						//	}
						//}
						//else if (menuMode == MenuMode.TelescopeDirection)
						//{
						//	mMagicPlayer.ESP -= mMagicPlayer.Level[2] * 5;
						//	DisplayESP();

						//	mTelescopePeriod = mMagicPlayer.Level[2];
						//	switch (mMenuFocusID)
						//	{
						//		case 0:
						//			mTelescopeYCount = -mMagicPlayer.Level[2];
						//			break;
						//		case 1:
						//			mTelescopeYCount = mMagicPlayer.Level[2];
						//			break;
						//		case 2:
						//			mTelescopeXCount = mMagicPlayer.Level[2];
						//			break;
						//		case 3:
						//			mTelescopeXCount = -mMagicPlayer.Level[2];
						//			break;
						//	}

						//	Talk($"[color={RGB.White}]천리안 사용중 ...[/color]");
						//}
						else if (menuMode == MenuMode.GameOptions)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								//AppendText(new string[] { $"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]" });

								//var maxEnemyStr = new string[5];
								//for (var i = 0; i < maxEnemyStr.Length; i++)
								//	maxEnemyStr[i] = $"{i + 3} 명의 적들";

								//ShowMenu(MenuMode.SetMaxEnemy, maxEnemyStr);
							}
							else if (mMenuFocusID == 1)
							{
								//AppendText(new string[] { $"[color={RGB.LightRed}]한번에 출현하는 적들의 최대치를 기입하십시오[/color]",
								//				"[color=e0ffff]순서를 바꿀 일원[/color]" });

								//ShowCharacterMenu(MenuMode.OrderFromCharacter);
							}
							else if (mMenuFocusID == 2)
							{

							}
							else if (mMenuFocusID == 3)
							{
								//AppendText(new string[] { $"[color={RGB.LightRed}]일행에서 제외 시키고 싶은 사람을 고르십시오.[/color]" });

								//ShowCharacterMenu(MenuMode.DelistCharacter);
							}
							else if (mMenuFocusID == 4)
							{
								//ShowFileMenu(MenuMode.ChooseLoadGame);
							}
							else if (mMenuFocusID == 5)
							{
								ShowFileMenu(MenuMode.ChooseSaveGame);
							}
							else if (mMenuFocusID == 6)
							{
								//AppendText(new string[] { $"[color={RGB.LightGreen}]정말로 끝내겠습니까 ?[/color]" });

								//ShowMenu(MenuMode.ConfirmExit, new string[] {
								//					"<< 아니오 >>",
								//					"<<   예   >>"
								//				});
							}
						}
						//else if (menuMode == MenuMode.SetMaxEnemy)
						//{
						//	mMenuMode = MenuMode.None;

						//	mMaxEnemy = mMenuFocusID + 3;
						//	if (mMaxEnemy == 2)
						//		mMaxEnemy = 5;

						//	AppendText(new string[] { $"[color={RGB.LightRed}]일행들의 지금 성격은 어떻습니까 ?[/color]" });

						//	ShowMenu(MenuMode.SetEncounterType, new string[]
						//	{
						//						"일부러 전투를 피하고 싶다",
						//						"너무 잦은 전투는 원하지 않는다",
						//						"마주친 적과는 전투를 하겠다",
						//						"보이는 적들과는 모두 전투하겠다",
						//						"그들은 피에 굶주려 있다"
						//	});
						//}
						//else if (menuMode == MenuMode.SetEncounterType)
						//{
						//	mMenuMode = MenuMode.None;

						//	AppendText(new string[] { "" });

						//	mEncounter = 6 - (mMenuFocusID + 1);
						//}
						//else if (menuMode == MenuMode.OrderFromCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	mOrderFromPlayerID = mMenuFocusID;

						//	AppendText(new string[] { $"[color={RGB.LightCyan}]순서를 바꿀 일원[/color]" });

						//	ShowCharacterMenu(MenuMode.OrderToCharacter);
						//}
						//else if (menuMode == MenuMode.OrderToCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	var tempPlayer = mPlayerList[mOrderFromPlayerID];
						//	mPlayerList[mOrderFromPlayerID] = mPlayerList[mMenuFocusID];
						//	mPlayerList[mMenuFocusID] = tempPlayer;

						//	DisplayPlayerInfo();

						//	DialogText.TextHighlighters.Clear();
						//	DialogText.Blocks.Clear();
						//}
						//else if (menuMode == MenuMode.DelistCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	mPlayerList.RemoveAt(mMenuFocusID);

						//	DisplayPlayerInfo();

						//	DialogText.TextHighlighters.Clear();
						//	DialogText.Blocks.Clear();
						//}
						//else if (menuMode == MenuMode.ConfirmExit)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//	{
						//		DialogText.TextHighlighters.Clear();
						//		DialogText.Blocks.Clear();
						//	}
						//	else
						//		CoreApplication.Exit();
						//}
						//else if (menuMode == MenuMode.Grocery)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mParty.Gold < (mMenuFocusID + 1) * 100)
						//		ShowNotEnoughMoney();
						//	else
						//	{
						//		mParty.Gold -= (mMenuFocusID + 1) * 100;
						//		var food = (mMenuFocusID + 1) * 10;

						//		if (mParty.Food + food > 255)
						//			mParty.Food = 255;
						//		else
						//			mParty.Food += food;

						//		ShowThankyou();
						//	}
						//}
						//else if (menuMode == MenuMode.WeaponType)
						//{
						//	mMenuMode = MenuMode.None;

						//	if (mMenuFocusID == 0)
						//	{
						//		AppendText(new string[] { $"[color={RGB.White}]어떤 무기를 원하십니까?[/color]" });

						//		ShowMenu(MenuMode.BuyWeapon, new string[]
						//		{
						//							$"{Common.GetWeaponStr(1)} : 금 500 개",
						//							$"{Common.GetWeaponStr(2)} : 금 1500 개",
						//							$"{Common.GetWeaponStr(3)} : 금 3000 개",
						//							$"{Common.GetWeaponStr(4)} : 금 5000 개",
						//							$"{Common.GetWeaponStr(5)} : 금 10000 개",
						//							$"{Common.GetWeaponStr(6)} : 금 30000 개",
						//							$"{Common.GetWeaponStr(7)} : 금 60000 개",
						//							$"{Common.GetWeaponStr(8)} : 금 80000 개",
						//							$"{Common.GetWeaponStr(9)} : 금 100000 개"
						//		});
						//	}
						//	else if (mMenuFocusID == 1)
						//	{
						//		AppendText(new string[] { $"[color={RGB.White}]어떤 방패를 원하십니까?[/color]" });

						//		ShowMenu(MenuMode.BuyShield, new string[]
						//		{
						//							$"{Common.GetDefenseStr(1)}방패 : 금 1000 개",
						//							$"{Common.GetDefenseStr(2)}방패 : 금 5000 개",
						//							$"{Common.GetDefenseStr(3)}방패 : 금 25000 개",
						//							$"{Common.GetDefenseStr(4)}방패 : 금 80000 개",
						//							$"{Common.GetDefenseStr(5)}방패 : 금 100000 개"
						//		});
						//	}
						//	else if (mMenuFocusID == 2)
						//	{
						//		AppendText(new string[] { $"[color={RGB.White}]어떤 갑옷를 원하십니까?[/color]" });

						//		ShowMenu(MenuMode.BuyArmor, new string[]
						//		{
						//							$"{Common.GetDefenseStr(1)}갑옷 : 금 5000 개",
						//							$"{Common.GetDefenseStr(2)}갑옷 : 금 25000 개",
						//							$"{Common.GetDefenseStr(3)}갑옷 : 금 80000 개",
						//							$"{Common.GetDefenseStr(4)}갑옷 : 금 100000 개",
						//							$"{Common.GetDefenseStr(5)}갑옷 : 금 200000 개"
						//		});
						//	}
						//}
						//else if (menuMode == MenuMode.BuyWeapon)
						//{
						//	mMenuMode = MenuMode.None;

						//	var price = GetWeaponPrice(mMenuFocusID + 1);

						//	if (mParty.Gold < price)
						//	{
						//		mWeaponShopEnd = true;
						//		ShowNotEnoughMoney();
						//	}
						//	else
						//	{
						//		mBuyWeaponID = mMenuFocusID + 1;

						//		AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetWeaponStr(mBuyWeaponID)}를 사용하시겠습니까?[/color]" });

						//		ShowCharacterMenu(MenuMode.UseWeaponCharacter);
						//	}
						//}
						//else if (menuMode == MenuMode.UseWeaponCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	var player = mPlayerList[mMenuFocusID];

						//	if (player.Class == 5)
						//	{
						//		AppendText(new string[] { $"[color={RGB.LightMagenta}]전투승은 이 무기가 필요없습니다.[/color]" });

						//		mWeaponShopEnd = true;
						//		ContinueText.Visibility = Visibility.Visible;
						//	}
						//	else
						//	{
						//		var power = 0;
						//		switch (mBuyWeaponID)
						//		{
						//			case 1:
						//				power = 5;
						//				break;
						//			case 2:
						//				power = 7;
						//				break;
						//			case 3:
						//				power = 9;
						//				break;
						//			case 4:
						//				power = 10;
						//				break;
						//			case 5:
						//				power = 15;
						//				break;
						//			case 6:
						//				power = 20;
						//				break;
						//			case 7:
						//				power = 30;
						//				break;
						//			case 8:
						//				power = 40;
						//				break;
						//			case 9:
						//				power = 50;
						//				break;
						//		}

						//		player.Weapon = mBuyWeaponID;
						//		player.WeaPower = power;

						//		if (player.Class == 1)
						//			player.WeaPower = (int)Math.Round(player.WeaPower * 1.5);

						//		mParty.Gold -= GetWeaponPrice(mBuyWeaponID);

						//		GoWeaponShop();
						//	}
						//}
						//else if (menuMode == MenuMode.BuyShield)
						//{
						//	mMenuMode = MenuMode.None;

						//	var price = GetShieldPrice(mMenuFocusID + 1);

						//	if (mParty.Gold < price)
						//	{
						//		mWeaponShopEnd = true;
						//		ShowNotEnoughMoney();
						//	}
						//	else
						//	{
						//		mBuyWeaponID = mMenuFocusID + 1;

						//		AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}방패를 사용하시겠습니까?[/color]" });

						//		ShowCharacterMenu(MenuMode.UseShieldCharacter);
						//	}
						//}
						//else if (menuMode == MenuMode.UseShieldCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	var player = mPlayerList[mMenuFocusID];

						//	player.Shield = mBuyWeaponID;
						//	player.ShiPower = mBuyWeaponID;
						//	player.AC = player.ShiPower + player.ArmPower;

						//	if (player.Class == 1)
						//		player.AC++;

						//	if (player.AC > 10)
						//		player.AC = 10;

						//	mParty.Gold -= GetShieldPrice(mBuyWeaponID);

						//	GoWeaponShop();
						//}
						//else if (menuMode == MenuMode.BuyArmor)
						//{
						//	mMenuMode = MenuMode.None;

						//	var price = GetArmorPrice(mMenuFocusID + 1);

						//	if (mParty.Gold < price)
						//	{
						//		mWeaponShopEnd = true;
						//		ShowNotEnoughMoney();
						//	}
						//	else
						//	{
						//		mBuyWeaponID = mMenuFocusID + 1;

						//		AppendText(new string[] { $"[color={RGB.White}]누가 이 {Common.GetDefenseStr(mBuyWeaponID)}갑옷을 사용하시겠습니까?[/color]" });

						//		ShowCharacterMenu(MenuMode.UseArmorCharacter);
						//	}
						//}
						//else if (menuMode == MenuMode.UseArmorCharacter)
						//{
						//	mMenuMode = MenuMode.None;

						//	var player = mPlayerList[mMenuFocusID];

						//	player.Armor = mBuyWeaponID;
						//	player.ArmPower = mBuyWeaponID + 1;
						//	player.AC = player.ShiPower + player.ArmPower;

						//	if (player.Class == 1)
						//		player.AC++;

						//	if (player.AC > 10)
						//		player.AC = 10;

						//	mParty.Gold -= GetArmorPrice(mBuyWeaponID);

						//	GoWeaponShop();
						//}
						//else if (menuMode == MenuMode.Hospital)
						//{
						//	mMenuMode = MenuMode.None;

						//	mCurePlayer = mPlayerList[mMenuFocusID];

						//	ShowHealType();
						//}
						//else if (menuMode == MenuMode.HealType)
						//{
						//	if (mMenuFocusID == 0)
						//	{
						//		if (mCurePlayer.Dead > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
						//		else if (mCurePlayer.Unconscious > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 의식불명입니다" });
						//		else if (mCurePlayer.Poison > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 독이 퍼진 상태입니다" });
						//		else if (mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level[0])
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 치료가 필요하지 않습니다" });

						//		if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison > 0 || mCurePlayer.HP >= mCurePlayer.Endurance * mCurePlayer.Level[0])
						//		{
						//			ContinueText.Visibility = Visibility;

						//			mCureMenuState = CureMenuState.NotCure;
						//		}
						//		else
						//		{
						//			var payment = mCurePlayer.Endurance * mCurePlayer.Level[0] - mCurePlayer.HP;
						//			payment = payment * mCurePlayer.Level[0] / 2 + 1;

						//			if (mParty.Gold < payment)
						//			{
						//				mCureMenuState = CureMenuState.NotCure;
						//				ShowNotEnoughMoney();
						//			}
						//			else
						//			{
						//				mParty.Gold -= payment;
						//				mCurePlayer.HP = mCurePlayer.Endurance * mCurePlayer.Level[0];

						//				AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}의 모든 건강이 회복되었다[/color]" });

						//				DisplayHP();

						//				ContinueText.Visibility = Visibility;

						//				mCureMenuState = CureMenuState.CureEnd;
						//			}
						//		}
						//	}
						//	else if (mMenuFocusID == 1)
						//	{
						//		if (mCurePlayer.Dead > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
						//		else if (mCurePlayer.Unconscious > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 의식불명입니다" });
						//		else if (mCurePlayer.Poison == 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 독에 걸리지 않았습니다" });

						//		if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious > 0 || mCurePlayer.Poison == 0)
						//		{
						//			ContinueText.Visibility = Visibility;

						//			mCureMenuState = CureMenuState.NotCure;
						//		}
						//		else
						//		{
						//			var payment = mCurePlayer.Level[0] * 10;

						//			if (mParty.Gold < payment)
						//			{
						//				mCureMenuState = CureMenuState.NotCure;
						//				ShowNotEnoughMoney();
						//			}
						//			else
						//			{
						//				mParty.Gold -= payment;
						//				mCurePlayer.Poison = 0;

						//				AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 독이 제거 되었습니다[/color]" });

						//				DisplayCondition();

						//				ContinueText.Visibility = Visibility;
						//				mCureMenuState = CureMenuState.CureEnd;
						//			}
						//		}
						//	}
						//	else if (mMenuFocusID == 2)
						//	{
						//		if (mCurePlayer.Dead > 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 이미 죽은 상태입니다" });
						//		else if (mCurePlayer.Unconscious == 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 의식불명이 아닙니다" });

						//		if (mCurePlayer.Dead > 0 || mCurePlayer.Unconscious == 0)
						//		{
						//			ContinueText.Visibility = Visibility;

						//			mCureMenuState = CureMenuState.NotCure;
						//		}
						//		else
						//		{
						//			var payment = mCurePlayer.Unconscious * 2;

						//			if (mParty.Gold < payment)
						//			{
						//				mCureMenuState = CureMenuState.NotCure;
						//				ShowNotEnoughMoney();
						//			}
						//			else
						//			{
						//				mParty.Gold -= payment;
						//				mCurePlayer.Unconscious = 0;
						//				mCurePlayer.HP = 1;

						//				AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 의식을 차렸습니다[/color]" });

						//				DisplayCondition();
						//				DisplayHP();

						//				ContinueText.Visibility = Visibility;
						//				mCureMenuState = CureMenuState.CureEnd;
						//			}
						//		}
						//	}
						//	else if (mMenuFocusID == 3)
						//	{
						//		if (mCurePlayer.Dead == 0)
						//			AppendText(new string[] { $"{mCurePlayer.Name}(은)는 죽지 않았습니다" });

						//		if (mCurePlayer.Dead == 0)
						//		{
						//			ContinueText.Visibility = Visibility;

						//			mCureMenuState = CureMenuState.NotCure;
						//		}
						//		else
						//		{
						//			var payment = mCurePlayer.Dead * 100 + 400;

						//			if (mParty.Gold < payment)
						//			{
						//				mCureMenuState = CureMenuState.NotCure;
						//				ShowNotEnoughMoney();
						//			}
						//			else
						//			{
						//				mParty.Gold -= payment;
						//				mCurePlayer.Dead = 0;

						//				if (mCurePlayer.Unconscious > mCurePlayer.Endurance * mCurePlayer.Level[0])
						//					mCurePlayer.Unconscious = mCurePlayer.Endurance * mCurePlayer.Level[0];

						//				AppendText(new string[] { $"[color={RGB.White}]{mCurePlayer.Name}(은)는 다시 살아났습니다[/color]" });

						//				DisplayCondition();

						//				ContinueText.Visibility = Visibility;

						//				mCureMenuState = CureMenuState.CureEnd;
						//			}
						//		}
						//	}
						//}
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

							ShowTrainSkillMenu();
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

							ShowTrainSkillMenu();
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

							ShowTrainMagicMenu();
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
								mSpecialEvent = SpecialEventType.CantTrain;
							}
							else
							{

								var needExp = 15 * skill * skill;

								if (needExp > mTrainPlayer.Experience)
								{
									Talk("아직 경험치가 모자랍니다");
									mSpecialEvent = SpecialEventType.CantTrain;
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
											mTrainPlayer.ESPMagic++;
											break;
										case 4:
											mTrainPlayer.SummonMagic++;
											break;
										default:
											mTrainPlayer.FistSkill++;
											break;
									}

									ShowTrainSkillMenu();
								}
							}
						}
						else if (menuMode == MenuMode.ConfirmExitMap)
						{
							mMenuMode = MenuMode.None;

							if (mMenuFocusID == 0)
							{
								if (mParty.Map == 6)
								{
									mParty.Map = 1;
									mParty.XAxis = 19;
									mParty.YAxis = 11;

									await RefreshGame();
								}

							}
							else
							{
								AppendText("");
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
								int curePoint;

								mMagicPlayer = mPlayerList[mBattlePlayerID];
								if (mMagicPlayer.ClassType == ClassCategory.Magic)
									curePoint = mMagicPlayer.CureMagic / 10;
								else
									curePoint = mMagicPlayer.AxeSkill / 10;

								if (curePoint <= 0)
								{
									Talk("당신은 치료 마법을 사용할 능력이 없습니다.");
									mSpecialEvent = SpecialEventType.BackToBattleMode;
								}
								else
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

								}
							}
							else
							{
								AppendText(new string[] { "" });
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
							if (mPlayerList.Count < 6)
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
							if (mPlayerList.Count < 6)
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
							if (mPlayerList.Count < 6)
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
							if (mPlayerList.Count < 6)
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
							if (mPlayerList.Count < 6)
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
					//				else if (args.VirtualKey == VirtualKey.R || args.VirtualKey == VirtualKey.GamepadLeftShoulder)
					//				{
					//					// 휴식 단축키
					//					Rest();
					//				}
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
			mParty.XAxis = moveX;
			mParty.YAxis = moveY;

			bool needUpdateStat = false;
			foreach (var player in mPlayerList)
			{
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

					needUpdateStat = true;
				}
			}

			if (needUpdateStat)
			{
				DisplayHP();
				DisplayCondition();
			}

			//DetectGameOver();

			if (mParty.Etc[4] > 0)
				mParty.Etc[4]--;

			if (!(GetTileInfo(moveX, moveY) == 0 || (mPosition == PositionType.Den && GetTileInfo(moveX, moveY) == 52)) && mRand.Next(mEncounter * 20) == 0)
			{
				EncounterEnemy();
				mTriggeredDownEvent = true;
			}

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
			AppendText(new string[] { "" });
			await LoadMapData();
			InitializeMap();
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
							if (!mEncounterEnemyList[i].Dead && (!mParty.Cruel && mEncounterEnemyList[i].Unconscious))
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
						case 7:
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
					var exp = 50000;
#else
					var exp = (enemy.ENumber + 1) * (enemy.ENumber + 1) * (enemy.ENumber + 1) / 8;
					if (exp == 0)
						exp = 1;
#endif

					if (!enemy.Unconscious)
					{
						battleResult.Add($"[color={RGB.Yellow}]{battleCommand.Player.NameSubjectJosa}[/color] [color={RGB.LightCyan}]{exp}[/color][color={RGB.Yellow}]만큼 경험치를 얻었다![/color]");
						battleCommand.Player.Experience += exp;
					}
					else
					{
						foreach (var player in mPlayerList)
						{
							if (player.IsAvailable)
								player.Experience += exp;
						};
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
				else if (battleCommand.Method == 6)
				{
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

					if (enemy.SpecialCastLevel > 0 && enemy.ENumber == 0)
					{
						if (liveEnemyCount < (mRand.Next(3) + 2) && mRand.Next(3) == 0)
						{
							var newEnemy = JoinEnemy(enemy.ENumber + mRand.Next(4) - 20);
							DisplayEnemy();
							battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {newEnemy.Name}(을)를 생성시켰다[/color]");
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

								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {turnEnemy.Name}(을)를 자기편으로 끌어들였다[/color]");
							}
						}

						if (enemy.SpecialCastLevel > 2 && enemy.Special != 0 && mRand.Next(5) == 0)
						{
							foreach (var player in mPlayerList)
							{
								if (player.Dead == 0)
								{
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}에게 죽음의 공격을 시도했다[/color]");

									if (mRand.Next(60) > player.Agility)
										battleResult.Add($"죽음의 공격은 실패했다");
									else if (mRand.Next(20) < player.Luck)
										battleResult.Add($"그러나, {player.Name}(은)는 죽음의 공격을 피했다");
									else
									{
										battleResult.Add($"[color={RGB.Red}]{player.Name}(은)는 죽었다 !![/color]");

										if (player.Dead == 0)
										{
											player.Dead = 1;
											if (player.HP > 0)
												player.HP = 0;
										}
									}
								}
							}
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

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							var attackPoint = enemy.Strength * enemy.Level * (mRand.Next(10) + 1) / 10;

							if (mRand.Next(50) < destPlayer.Resistance)
							{
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}(을)를 공격했다[/color]");
								battleResult.Add($"그러나, {destPlayer.Name}(은)는 적의 공격을 저지했다");
								return;
							}

							attackPoint -= destPlayer.AC * destPlayer.Level[0] * (mRand.Next(10) + 1) / 10;

							if (attackPoint <= 0)
							{
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}(을)를 공격했다[/color]");
								battleResult.Add($"그러나, {destPlayer.Name}(은)는 적의 공격을 방어했다");
								return;
							}

							if (destPlayer.Dead > 0)
								destPlayer.Dead += attackPoint;

							if (destPlayer.Unconscious > 0 && destPlayer.Dead == 0)
								destPlayer.Unconscious += attackPoint;

							if (destPlayer.HP > 0)
								destPlayer.HP -= attackPoint;

							battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {destPlayer.Name}에게 공격 받았다[/color]");
							battleResult.Add($"[color={RGB.Magenta}]{destPlayer.Name}(은)는[/color] [color={RGB.LightMagenta}]{attackPoint}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
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

								castPower -= mRand.Next(castPower / 2);
								castPower -= player.AC * player.Level[0] * (mRand.Next(10) + 1) / 10;
								if (castPower <= 0)
								{
									battleResult.Add($"그러나, {player.Name}(은)는 적의 마법을 막아냈다");
									return;
								}

								if (player.Dead > 0)
									player.Dead += castPower;

								if (player.Unconscious > 0 && player.Dead == 0)
									player.Unconscious += castPower;

								if (player.HP > 0)
									player.HP -= castPower;

								battleResult.Add($"[color={RGB.Magenta}]{player.Name}(은)는[/color] [color={RGB.LightMagenta}]{castPower}[/color][color={RGB.Magenta}]만큼의 피해를 입었다[/color]");
							}

							void CastAttckOne(Lore player)
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
									castName = "고통";
									castPower = 6;
								}
								else if (15 <= enemy.Mentality && enemy.Mentality <= 18)
								{
									castName = "고통";
									castPower = 7;
								}
								else
								{
									castName = "번개";
									castPower = 10;
								}

								castPower *= enemy.Level;
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {player.Name}에게 '{castName}'마법을 사용했다[/color]");

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
								battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 일행 모두에게 '{castName}'마법을 사용했다[/color]");

								foreach (var player in destPlayerList)
									CastAttack(castPower, player);
							}

							void CureEnemy(BattleEnemyData whomEnemy, int curePoint)
							{
								if (enemy == whomEnemy)
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 자신을 치료했다[/color]");
								else
									battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {whomEnemy.Name}(을)를 치료했다[/color]");

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
									if (whomEnemy.HP > whomEnemy.Endurance * whomEnemy.Level)
										whomEnemy.HP = whomEnemy.Endurance * whomEnemy.Level;
								}
							}

							void CastHighLevel(List<Lore> destPlayerList)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(3) == 0)
								{
									CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
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

								avgAC /= avgCount;

								if (avgAC > 4 && mRand.Next(5) == 0)
								{
									foreach (var player in mPlayerList)
									{
										battleResult.Add($"[color={RGB.LightMagenta}]{enemy.Name}(은)는 {player.Name}의 갑옷파괴를 시도했다[/color]");
										if (player.Luck > mRand.Next(21))
											battleResult.Add($"그러나, {enemy.Name}(은)는 성공하지 못했다");
										else
										{
											battleResult.Add($"[color={RGB.Magenta}]{player.Name}의 갑옷은 파괴되었다[/color]");

											if (player.AC > 0)
												player.AC--;
										}
									}

									DisplayPlayerInfo();
								}
								else
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(3) == 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality / 6);
									}
									else if (mRand.Next(destPlayerList.Count) < 2)
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										CastAttckOne(weakestPlayer);
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

							var destPlayer = normalList[mRand.Next(normalList.Count)];

							if (enemy.CastLevel == 1)
							{
								CastAttckOne(destPlayer);
							}
							else if (enemy.CastLevel == 2)
							{
								CastAttckOne(destPlayer);
							}
							else if (enemy.CastLevel == 3)
							{
								if (mRand.Next(normalList.Count) < 2)
									CastAttckOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 4)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(2) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
								else if (mRand.Next(normalList.Count) < 2)
									CastAttckOne(destPlayer);
								else
									CastAttackAll(normalList);
							}
							else if (enemy.CastLevel == 5)
							{
								if ((enemy.HP < enemy.Endurance * enemy.Level / 3) && mRand.Next(3) == 0)
									CureEnemy(enemy, enemy.Level * enemy.Mentality / 4);
								else if (mRand.Next(normalList.Count) < 2)
								{
									var totalCurrentHP = 0;
									var totalFullHP = 0;

									foreach (var enemyOne in mEncounterEnemyList)
									{
										totalCurrentHP += enemyOne.HP;
										totalFullHP += enemyOne.Endurance * enemyOne.Level;
									}

									totalFullHP /= 3;

									if (mEncounterEnemyList.Count > 2 && totalCurrentHP < totalFullHP && mRand.Next(2) == 0)
									{
										foreach (var enemyOne in mEncounterEnemyList)
											CureEnemy(enemyOne, enemy.Level * enemy.Mentality / 6);
									}
									else
									{
										Lore weakestPlayer = null;

										foreach (var player in mPlayerList)
										{
											if (player.IsAvailable && (weakestPlayer == null || weakestPlayer.HP > player.HP))
												weakestPlayer = player;
										}

										CastAttckOne(weakestPlayer);
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

		//		private void GoWeaponShop()
		//		{
		//			AppendText(new string[] {
		//						$"[color={RGB.White}]여기는 무기상점입니다.[/color]",
		//						$"[color={RGB.White}]우리들은 무기, 방패, 갑옷을 팔고있습니다.[/color]",
		//						$"[color={RGB.White}]어떤 종류를 원하십니까 ?[/color]"
		//					});

		//			ShowMenu(MenuMode.WeaponType, new string[]
		//			{
		//					"무기류",
		//					"방패류",
		//					"갑옷류"
		//			});
		//		}

		//		private void GoHospital()
		//		{
		//			AppendText(new string[] {
		//						$"[color={RGB.White}]여기는 병원입니다.[/color]",
		//						$"[color={RGB.White}]누가 치료를 받겠습니까 ?[/color]"
		//					});

		//			ShowCharacterMenu(MenuMode.Hospital);
		//		}

		//		private void ShowPartyStatus()
		//		{
		//			string CheckEnable(int i)
		//			{
		//				if (mParty.Etc[i] == 0)
		//					return "불가";
		//				else
		//					return "가능";
		//			}

		//			AppendText(new string[] { $"X 축 = {mParty.XAxis + 1 }",
		//				$"Y 축 = {mParty.YAxis + 1}",
		//				"",
		//				$"남은 식량 = {mParty.Food}",
		//				$"남은 황금 = {mParty.Gold}",
		//				"",
		//				$"마법의 횃불 : {CheckEnable(0)}",
		//				$"공중 부상 : {CheckEnable(3)}",
		//				$"물위를 걸음 : {CheckEnable(1)}",
		//				$"늪위를 걸음 : {CheckEnable(2)}"
		//			});
		//		}

		//		private void Rest()
		//		{
		//			var append = false;
		//			mPlayerList.ForEach(delegate (Lore player)
		//			{
		//				if (mParty.Food <= 0)
		//					AppendText(new string[] { $"[color={RGB.Red}]일행은 식량이 바닥났다[/color]" }, append);
		//				else
		//				{
		//					if (player.Dead > 0)
		//						AppendText(new string[] { $"{player.Name}(은)는 죽었다" }, append);
		//					else if (player.Unconscious > 0 && player.Poison == 0)
		//					{
		//						player.Unconscious = player.Unconscious - player.Level[0] - player.Level[1] - player.Level[2];
		//						if (player.Unconscious <= 0)
		//						{
		//							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 의식이 회복되었다[/color]" }, append);
		//							player.Unconscious = 0;
		//							if (player.HP <= 0)
		//								player.HP = 1;

		//#if DEBUG
		//							//mParty.Food--;
		//#else
		//							mParty.Food--;
		//#endif

		//						}
		//						else
		//							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 여전히 의식 불명이다[/color]" }, append);
		//					}
		//					else if (player.Unconscious > 0 && player.Poison > 0)
		//						AppendText(new string[] { $"독때문에 {player.Name}의 의식은 회복되지 않았다" }, append);
		//					else if (player.Poison > 0)
		//						AppendText(new string[] { $"독때문에 {player.Name}의 건강은 회복되지 않았다" }, append);
		//					else
		//					{
		//						var recoverPoint = (player.Level[0] + player.Level[1] + player.Level[2]) * 2;
		//						if (player.HP >= player.Endurance * player.Level[0])
		//						{
		//							if (mParty.Food < 255)
		//							{
		//#if DEBUG
		//								//mParty.Food++;
		//#else
		//								mParty.Food++;
		//#endif
		//							}
		//						}

		//						player.HP += recoverPoint;

		//						if (player.HP >= player.Endurance * player.Level[0])
		//						{
		//							player.HP = player.Endurance * player.Level[0];

		//							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 모든 건강이 회복되었다[/color]" }, append);
		//						}
		//						else
		//							AppendText(new string[] { $"[color={RGB.White}]{player.Name}(은)는 치료되었다[/color]" }, append);

		//#if DEBUG
		//						//mParty.Food--;
		//#else
		//						mParty.Food--;
		//#endif
		//					}

		//					if (append == false)
		//						append = true;
		//				}
		//			});

		//			if (mParty.Etc[0] > 0)
		//			{
		//				mParty.Etc[0]--;
		//				ShowMap();
		//			}

		//			for (var i = 1; i < 4; i++)
		//				mParty.Etc[i] = 0;

		//			mPlayerList.ForEach(delegate (Lore player)
		//			{
		//				player.SP = player.Mentality * player.Level[1];
		//				player.ESP = player.Concentration * player.Level[2];
		//			});

		//			UpdatePlayersStat();
		//			ContinueText.Visibility = Visibility.Visible;
		//		}

		private void ShowEnterMenu(EnterType enterType)
		{
			mTryEnterType = enterType;

			AppendText(new string[] { $"{mEnterTypeMap[enterType]}에 들어가기를 원합니까 ?" });

			ShowMenu(MenuMode.AskEnter, new string[] {
				"예, 그렇습니다.",
				"아니오, 원하지 않습니다."
			});
		}

		private async void InvokeSpecialEvent(int prevX, int prevY)
		{
			if (mParty.Map == 6) {
				if (mParty.XAxis == 18 && (mParty.Etc[29] & 1) == 0) {
					
					InvokeAnimation(AnimationType.LordAhnCall);
				}
				else if (mParty.XAxis == 40 && mParty.YAxis == 78) {
					if ((mParty.Etc[29] & (1 << 3)) == 0) {
						mParty.Etc[29] |= 1 << 3;

						InvokeAnimation(AnimationType.GetDefaultWeapon);
					}
				}
				else if ((mParty.XAxis == 50 && mParty.YAxis == 11) || (mParty.XAxis == 51 && mParty.YAxis == 11)) {
					if ((mParty.Etc[49] & (1 << 4)) > 0 || (mParty.Etc[49] & (1 << 5)) > 0) {
						if ((mParty.Etc[49] & (1 << 4)) > 0) {
							// 전투 시스템 미구현
						}
					}
				}
				else if (mParty.XAxis == 85 && mParty.YAxis == 47) {
					AppendText(" 당신은 열쇠를 발견 했다.");
					UpdateTileInfo(86, 47, 44);
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 37 && (mParty.Etc[30] & 1) == 0) {
					AppendText(" 당신은 금화 2000 개를 발견했다.");
					mParty.Gold += 2000;
					mParty.Etc[30] |= 1;
				}
				else if (mParty.XAxis == 89 && mParty.YAxis == 40 && (mParty.Etc[30] & (1 << 1)) == 0) {
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
				else if (mParty.YAxis == 94) {
					ShowExitMenu();
				}
			}
			else if (mParty.Map == 10) {
				if ((mParty.XAxis == 24 && mParty.YAxis == 42) || (mParty.XAxis == 25 && mParty.YAxis == 42)) {
					if (mParty.Etc[9] == 1)
						mParty.Etc[9] = 2;
					else if (mParty.Etc[9] == 3) {
						Talk(new string[] {
							" 당신이 메너스에 들어오자 마자  어떤 사람의 시체가 놓여져 있었다." +
							" 그 시체는 형체를 알아볼 수 없을 정도로 피 투성이가 되어있었고 그의 등에는  커다란 독 화살이 예리하게 관통해 있었다.",
							" 동굴 입구에서는 때마침  초승달이 비치고 있었고 그 달빛은 그 시체에서 흘러서 나와 고인피에 비쳐서 붉게 물들여 졌다.",
							" 순간 당신은 알비레오의 예언의 구절이 떠 올랐다. 그것은 바로 이런 것이었다.",
							$"[color={RGB.LightCyan}] \" 메너스의 달이 붉게 물들때  어둠의 영혼이 나타나 세계의 종말을 예고한다. \"[/color]"
						});

						mSpecialEvent = SpecialEventType.SeeDeadBody;
					}
				}
			}
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
			{
				AppendText(new string[] { message }, true);
				ContinueText.Visibility = Visibility.Visible;
			}
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

		//		private void ShowNotEnoughMoney(List<string> result = null)
		//		{
		//			AppendText(new string[] { "당신은 충분한 돈이 없습니다." }, true);
		//			ContinueText.Visibility = Visibility.Visible;
		//		}

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

		private void ShowCharacterMenu(MenuMode menuMode)
		{
			AppendText($"[color={RGB.LightGreen}]한명을 고르시오 ---[/color]", true);

			var menuStr = new string[mPlayerList.Count];
			for (var i = 0; i < mPlayerList.Count; i++)
				menuStr[i] = mPlayerList[i].Name;

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

		//		private void ShowSpinner(SpinnerType spinnerType, Tuple<string, int>[] items, int defaultId)
		//		{
		//			mSpinnerType = spinnerType;

		//			mSpinnerItems = items;
		//			mSpinnerID = defaultId;

		//			AppendText(SpinnerText, mSpinnerItems[defaultId].Item1);
		//			SpinnerText.Visibility = Visibility.Visible;
		//		}

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

			return mMenuMode;
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
				AppendText(new string[] {
					$"[color={RGB.White}]여기는 병원입니다.[/color]",
					$"[color={RGB.White}]누가 치료를 받겠습니까 ?[/color]",
				});

				ShowCharacterMenu(MenuMode.Hospital);
			}

			void ShowWeaponShopMenu()
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
							player.Experience += 500000;
						}
						mParty.Gold += 100000;

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
							player.Experience += 1000000;
						}
						mParty.Gold += 500000;

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
							PlusTime(0, 20, 0);

							foreach (var player in mPlayerList) {
								if (player.Dead == 0 && player.Unconscious == 0) {
									if (player.Poison == 0)
									{
										player.HP += player.Level;
										if (player.HP > player.Endurance * player.Level * 10)
											player.HP = player.Endurance * player.Level * 10;
									}
									else {
										player.HP--;
										if (player.HP <= 0)
											player.Unconscious = 1;
									}								
								}
								else if (player.Dead == 0 && player.Unconscious > 0) {
									if (player.Poison == 0) {
										if (player.Unconscious - player.Level > 0)
											player.Unconscious = player.Unconscious + player.Level;
										else {
											player.Unconscious = 0;
											player.HP = 1;
										}
									}
									else {
										player.Unconscious++;
										if (player.Unconscious > player.Endurance * player.Level)
											player.Dead = 1;
									}
								}
							}

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
						Task.Delay(2000);
					}

					AppendText(" 당신이 조금 더 안쪽으로 들어 갔을때 누군가가 당신을 지켜 보고 있다는 느낌이 들었다.");
					Task.Delay(5000);
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
			else if (mAnimationEvent == AnimationType.LeaveSoldier) {
				for (var y = 48; y < 51; y++)
					UpdateTileInfo(18, y, 44);
				mParty.Etc[29] |= 1;

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.SleepLoreCastle) {
				AppendText($"[color={RGB.White}] 아침이 밝았다.[/color]");

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.TalkLordAhn || mAnimationEvent == AnimationType.TalkLordAhn) {
				Talk(" 한참후 ...");

				if (mAnimationEvent == AnimationType.TalkLordAhn)
					mSpecialEvent = SpecialEventType.MeetLordAhn9;
				else
					mSpecialEvent = SpecialEventType.MeetLordAhn11;
			}
			else if (mAnimationEvent == AnimationType.GetDefaultWeapon) {
				UpdateTileInfo(40, 78, 44);
				AppendText(" 일행은 가장 기본적인 무기로  모두  무장을 하였다.");

				foreach (var player in mPlayerList) {
					if (player.Weapon == 0 && player.ClassType == ClassCategory.Sword && player.Class != 5) {
						player.Weapon = 1;
						player.WeaPower = 5;
					}
				}

				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;
			}
			else if (mAnimationEvent == AnimationType.EnterUnderworld) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				mParty.Etc[8] |= 1;

				mFace = 4;
				AppendText(" 한참 후에 당신은 정신이 들었고 여기가 지하세계임을 알아 차렸다.");
			}
			else if (mAnimationEvent == AnimationType.GoInsideMenace) {
				mAnimationEvent = AnimationType.None;
				mAnimationFrame = 0;

				StartBattleEvent(BattleEvent.MenaceMurder);
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
				tint = new Vector4(0.2f, 0.2f, 2f, 1);

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

				if (mPenetration)
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

		//		private void ShowMap()
		//		{
		//			BattlePanel.Visibility = Visibility.Collapsed;

		//			if (mPosition == PositionType.Den && mParty.Etc[0] == 0)
		//			{
		//				canvas.Visibility = Visibility.Collapsed;
		//				DarknessPanel.Visibility = Visibility.Visible;
		//			}
		//			else
		//			{
		//				canvas.Visibility = Visibility.Visible;
		//				DarknessPanel.Visibility = Visibility.Collapsed;
		//			}
		//		}

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
			BGMPlayer.Source = musicUri;

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
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerHPList[i].Text = mPlayerList[i].HP.ToString();
				else
					mPlayerHPList[i].Text = "";
			}
		}

		private void DisplaySP()
		{
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerSPList[i].Text = mPlayerList[i].SP.ToString();
				else
					mPlayerSPList[i].Text = "";
			}
		}

		//		private void DisplayESP()
		//		{
		//			for (var i = 0; i < 6; i++)
		//			{
		//				if (i < mPlayerList.Count)
		//					mPlayerESPList[i].Text = mPlayerList[i].ESP.ToString();
		//				else
		//					mPlayerESPList[i].Text = "";
		//			}
		//		}

		private void DisplayCondition()
		{
			for (var i = 0; i < 6; i++)
			{
				if (i < mPlayerList.Count)
					mPlayerConditionList[i].Text = GetConditionName(i);
				else
					mPlayerConditionList[i].Text = "";
			}
		}

		private string GetConditionName(int index)
		{
			//if (mPlayerList[index].HP <= 0 && mPlayerList[index].Unconscious == 0)
			//	mPlayerList[index].Unconscious = 1;

			//if (mPlayerList[index].Unconscious > mPlayerList[index].Endurance * mPlayerList[index].Level[0] && mPlayerList[index].Dead == 0)
			//	mPlayerList[index].Dead = 1;

			//if (mPlayerList[index].Dead > 0)
			//	return "사망";

			//if (mPlayerList[index].Unconscious > 0)
			//	return "의식불명";

			//if (mPlayerList[index].Poison > 0)
			//	return "중독";

			return "좋음";
		}

		//		private bool EnterWater()
		//		{
		//			if (mParty.Etc[1] > 0)
		//			{
		//				mParty.Etc[1]--;

		//				if (mRand.Next(mEncounter * 30) == 0)
		//					EncounterEnemy();

		//				return true;
		//			}
		//			else
		//				return false;
		//		}

		//		private void EnterSwamp()
		//		{
		//			foreach (var player in mPlayerList)
		//			{
		//				if (player.Poison > 0)
		//					player.Poison++;

		//				if (player.Poison > 10)
		//				{
		//					player.Poison = 1;

		//					if (0 < player.Dead && player.Dead < 100)
		//						player.Dead++;
		//					else if (player.Unconscious > 0)
		//					{
		//						player.Unconscious++;

		//						if (player.Unconscious > player.Endurance * player.Level[0])
		//							player.Dead = 1;
		//					}
		//					else
		//					{
		//						player.HP--;
		//						if (player.HP <= 0)
		//							player.Unconscious++;
		//					}

		//				}
		//			}

		//			if (mParty.Etc[2] > 0)
		//				mParty.Etc[2]--;
		//			else
		//			{
		//				AppendText(new string[] { $"[color={RGB.LightRed}]일행은 독이 있는 늪에 들어갔다 !!![/color]", "" });

		//				foreach (var player in mPlayerList)
		//				{
		//					if (mRand.Next(20) + 1 >= player.Luck)
		//					{
		//						AppendText(new string[] { $"[color={RGB.LightMagenta}]{player.Name}(은)는 중독 되었다.[/color]" }, true);
		//						if (player.Poison == 0)
		//							player.Poison = 1;
		//					}
		//				}
		//			}

		//			UpdatePlayersStat();
		//			DetectGameOver();
		//		}

		//		private void EnterLava()
		//		{
		//			AppendText(new string[] { $"[color={RGB.LightRed}]일행은 용암지대로 들어섰다 !!![/color]", "" });

		//			foreach (var player in mPlayerList)
		//			{
		//				var damage = mRand.Next(40) + 40 - 2 * mRand.Next(player.Luck);

		//				AppendText(new string[] { $"[color={RGB.LightMagenta}]{player.Name}(은)는 {damage}의 피해를 입었다 ![/color]" }, true);

		//				if (player.HP > 0 && player.Unconscious == 0)
		//				{
		//					player.HP -= damage;
		//					if (player.HP <= 0)
		//						player.Unconscious = 1;
		//				}
		//				else if (player.HP > 0 && player.Unconscious > 0)
		//					player.HP -= damage;
		//				else if (player.Unconscious > 0 && player.Dead == 0)
		//				{
		//					player.Unconscious += damage;
		//					if (player.Unconscious > player.Endurance * player.Level[0])
		//						player.Dead = 1;
		//				}
		//				else if (player.Dead == 1)
		//				{
		//					if (player.Dead + damage > 30000)
		//						player.Dead = 30000;
		//					else
		//						player.Dead += damage;

		//				}
		//			}

		//			UpdatePlayersStat();
		//			DetectGameOver();
		//		}

		private BattleEnemyData JoinEnemy(int ENumber)
		{
			BattleEnemyData enemy = new BattleEnemyData(ENumber, mEnemyDataList[ENumber]);

			AssignEnemy(enemy);

			return enemy;
		}

		//		private BattleEnemyData TurnMind(Lore player)
		//		{
		//			var enemy = new BattleEnemyData(1, new EnemyData()
		//			{
		//				Name = player.Name,
		//				Strength = player.Strength,
		//				Mentality = player.Mentality,
		//				Endurance = player.Endurance,
		//				Resistance = player.Resistance,
		//				Agility = player.Agility,
		//				Accuracy = new int[] { player.Accuracy[0], player.Accuracy[1] },
		//				AC = player.AC,
		//				Special = player.Class == 7 ? 2 : 0,
		//				CastLevel = player.Level[1] / 4,
		//				SpecialCastLevel = 0,
		//				Level = player.Level[0],
		//			});

		//			AssignEnemy(enemy);

		//			return enemy;
		//		}

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

		//		private void ShowGameOver(string[] gameOverMessage)
		//		{
		//			AppendText(gameOverMessage);

		//			ShowMenu(MenuMode.BattleLose, new string[] {
		//				"이전의 게임을 재개한다",
		//				"게임을 끝낸다"
		//			});
		//		}

		//		private void DetectGameOver()
		//		{
		//			var allPlayerDead = true;
		//			foreach (var player in mPlayerList)
		//			{
		//				if (player.IsAvailable)
		//				{
		//					allPlayerDead = false;
		//					break;
		//				}
		//			}

		//			if (allPlayerDead)
		//			{
		//				mParty.Etc[5] = 255;

		//				ShowGameOver(new string[] { "일행은 모험 중에 모두 목숨을 잃었다." });
		//				mTriggeredDownEvent = true;
		//			}
		//		}


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
			ChooseWeaponType,
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
			ApplyBattleCureAllSpell
		}

		private enum SpinnerType
		{
			None,
			TeleportationRange
		}

		private enum CureMenuState
		{
			None,
			NotCure,
			CureEnd
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
			ProofOfDarkStar,
			ClueOfDarkStar,
			RoofOfLight,
			TempleOfLight,
			SurvivalOfPerishment,
			CaveOfBerial,
			CaveOfMolok,
			TeleportationGate,
			CaveOfAsmodeus,
			FortressOfMephistopheles
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
			GoInsideMenace
		}

		private enum SpecialEventType
		{
			None,
			CantTrain,
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
		}

		private enum BattleEvent {
			None,
			MenaceMurder,
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

			public int Width {
				get;
				private set;
			}

			public int Height {
				get;
				private set;
			}
		}

		private void MapCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{

		}
	}
}
