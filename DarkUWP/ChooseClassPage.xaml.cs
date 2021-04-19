using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace DarkUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class ChooseClassPage : Page
	{
		private Lore mPlayer;
		private int mExtraPoint = 0;
		private FocusItem mFocusItem = FocusItem.SetSkill;
		private int mFocusID = 0;

		private List<Tuple<TextBlock, TextBlock>> mSkillTextList = new List<Tuple<TextBlock, TextBlock>>();
		private List<TextBlock> mClassTextList = new List<TextBlock>();
		private List<bool> mClassAvailableList = new List<bool>();

		public ChooseClassPage()
		{
			this.InitializeComponent();

			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText1, SkillValueText1));
			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText2, SkillValueText2));
			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText3, SkillValueText3));
			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText4, SkillValueText4));
			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText5, SkillValueText5));
			mSkillTextList.Add(new Tuple<TextBlock, TextBlock>(SkillNameText6, SkillValueText6));

			mClassTextList.Add(ClassNameText1);
			mClassTextList.Add(ClassNameText2);
			mClassTextList.Add(ClassNameText3);
			mClassTextList.Add(ClassNameText4);
			mClassTextList.Add(ClassNameText5);
			mClassTextList.Add(ClassNameText6);
			mClassTextList.Add(ClassNameText7);

			for (var i = 0; i < mClassTextList.Count; i++)
				mClassAvailableList.Add(false);

			TypedEventHandler<CoreWindow, KeyEventArgs> chooseClassPageKeyEvent = null;
			chooseClassPageKeyEvent = async (sender, args) =>
			{
				if (mFocusItem == FocusItem.SetSkill)
				{
					void UpdateSkillCursor()
					{
						for (var i = 0; i < 6; i++)
						{
							if (mFocusID == i)
							{
								mSkillTextList[i].Item1.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
								mSkillTextList[i].Item2.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
							}
							else
							{
								mSkillTextList[i].Item1.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
								mSkillTextList[i].Item2.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
							}
						}
					}

					void UpdateSkillPoint(int point)
					{
						var skillLine = new StringBuilder();
						for (var i = 0; i < point; i++)
							skillLine.Append("─");

						mSkillTextList[mFocusID].Item2.Text = skillLine.ToString();
						UpdateClassAvailable();
					}

					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						if (mPlayer.ClassType == ClassCategory.Sword)
						{
							switch (mFocusID)
							{
								case 0:
									if (mPlayer.SwordSkill > 0)
									{
										mPlayer.SwordSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.SwordSkill);
									}
									break;
								case 1:
									if (mPlayer.AxeSkill > 0)
									{
										mPlayer.AxeSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.AxeSkill);
									}
									break;
								case 2:
									if (mPlayer.SpearSkill > 0)
									{
										mPlayer.SpearSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.SpearSkill);
									}
									break;
								case 3:
									if (mPlayer.BowSkill > 0)
									{
										mPlayer.BowSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.BowSkill);
									}
									break;
								case 4:
									if (mPlayer.ShieldSkill > 0)
									{
										mPlayer.ShieldSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.ShieldSkill);
									}
									break;
								case 5:
									if (mPlayer.FistSkill > 0)
									{
										mPlayer.FistSkill--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.FistSkill);
									}
									break;
							}
						}
						else
						{
							switch (mFocusID)
							{
								case 0:
									if (mPlayer.AttackMagic > 0)
									{
										mPlayer.AttackMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.AttackMagic);
									}
									break;
								case 1:
									if (mPlayer.PhenoMagic > 0)
									{
										mPlayer.PhenoMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.PhenoMagic);
									}
									break;
								case 2:
									if (mPlayer.CureMagic > 0)
									{
										mPlayer.CureMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.CureMagic);
									}
									break;
								case 3:
									if (mPlayer.SpecialMagic > 0)
									{
										mPlayer.SpecialMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.SpecialMagic);
									}
									break;
								case 4:
									if (mPlayer.ESPMagic > 0)
									{
										mPlayer.ESPMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.ESPMagic);
									}
									break;
								case 5:
									if (mPlayer.SummonMagic > 0)
									{
										mPlayer.SummonMagic--;
										mExtraPoint++;

										UpdateSkillPoint(mPlayer.SummonMagic);
									}
									break;
							}
						}
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						if (mPlayer.ClassType == ClassCategory.Sword)
						{
							switch (mFocusID)
							{
								case 0:
									if (mExtraPoint > 0)
									{
										mPlayer.SwordSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.SwordSkill);
									}
									break;
								case 1:
									if (mExtraPoint > 0)
									{
										mPlayer.AxeSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.AxeSkill);
									}
									break;
								case 2:
									if (mExtraPoint > 0)
									{
										mPlayer.SpearSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.SpearSkill);
									}
									break;
								case 3:
									if (mExtraPoint > 0)
									{
										mPlayer.BowSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.BowSkill);
									}
									break;
								case 4:
									if (mExtraPoint > 0)
									{
										mPlayer.ShieldSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.ShieldSkill);
									}
									break;
								case 5:
									if (mExtraPoint > 0)
									{
										mPlayer.FistSkill++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.FistSkill);
									}
									break;
							}
						}
						else
						{
							switch (mFocusID)
							{
								case 0:
									if (mExtraPoint > 0)
									{
										mPlayer.AttackMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.AttackMagic);
									}
									break;
								case 1:
									if (mExtraPoint > 0)
									{
										mPlayer.PhenoMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.PhenoMagic);
									}
									break;
								case 2:
									if (mExtraPoint > 0)
									{
										mPlayer.CureMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.CureMagic);
									}
									break;
								case 3:
									if (mExtraPoint > 0)
									{
										mPlayer.SpecialMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.SpecialMagic);
									}
									break;
								case 4:
									if (mExtraPoint > 0)
									{
										mPlayer.ESPMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.ESPMagic);
									}
									break;
								case 5:
									if (mExtraPoint > 0)
									{
										mPlayer.SummonMagic++;
										mExtraPoint--;

										UpdateSkillPoint(mPlayer.SummonMagic);
									}
									break;
							}
						}


					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						mFocusID = (mFocusID + 1) % mSkillTextList.Count;

						UpdateSkillCursor();
					}
					else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mFocusID == 0)
							mFocusID = mSkillTextList.Count - 1;
						else
							mFocusID--;

						UpdateSkillCursor();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						var isAvailable = false;
						var defaultID = 0;
						for (var i = 0; i < mClassAvailableList.Count; i++)
						{
							if (mClassAvailableList[i])
							{
								isAvailable = true;
								defaultID = i;
								break;
							}
						}

						if (isAvailable)
						{
							mSkillTextList[mFocusID].Item1.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
							mSkillTextList[mFocusID].Item2.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));

							mClassTextList[defaultID].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
							mFocusID = defaultID;
							mFocusItem = FocusItem.ChooseClass;
						}
					}
				}
				else if (mFocusItem == FocusItem.ChooseClass)
				{
					void UpdateClassFocus(int focusID)
					{
						mFocusID = focusID;

						for (var i = 0; i < mClassTextList.Count; i++)
						{
							if (mFocusID == i)
								mClassTextList[i].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
							else if (mClassAvailableList[i])
								mClassTextList[i].Foreground = new SolidColorBrush(Colors.White);
							else
								mClassTextList[i].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
						}
					}

					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						int prevID = mFocusID;
						do
						{
							if (prevID == 0)
								prevID = mClassAvailableList.Count - 1;
							else
								prevID--;
						} while (!mClassAvailableList[prevID] && prevID != mFocusID);

						UpdateClassFocus(prevID);
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						int nextID = mFocusID;
						do
						{
							nextID = (nextID + 1) % mClassAvailableList.Count;
						} while (!mClassAvailableList[nextID] && nextID != mFocusID);

						UpdateClassFocus(nextID);
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						mPlayer.Class = mFocusID + 1;

						PlayerClassText.Text = $"당신의 계급 : {mPlayer.ClassStr}";
						PlayerClassText.Visibility = Visibility.Visible;

						SkillPanel.Visibility = Visibility.Collapsed;
						ClassPanel.Visibility = Visibility.Collapsed;

						PrologText.Visibility = Visibility.Visible;
						ConfirmPanel.Visibility = Visibility.Visible;

						mFocusItem = FocusItem.Confirm;
						mFocusID = 0;
					}
				}
				else if (mFocusItem == FocusItem.Confirm)
				{
					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						mFocusID = 0;

						ConfirmYesText.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
						ConfirmNoText.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						mFocusID = 1;

						ConfirmYesText.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
						ConfirmNoText.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xf3, 0xf3, 0x28));
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						Window.Current.CoreWindow.KeyUp -= chooseClassPageKeyEvent;

						if (mFocusID == 0) {
							if (mPlayer.ClassType == ClassCategory.Sword)
								mPlayer.PotentialAC = 2;
							else
								mPlayer.PotentialAC = 0;

							mPlayer.Level = 1;
							mPlayer.Poison = 0;
							mPlayer.Unconscious = 0;
							mPlayer.Dead = 0;
							mPlayer.HP = mPlayer.Endurance * mPlayer.Level * 10;

							if (mPlayer.ClassType == ClassCategory.Magic)
								mPlayer.SP = mPlayer.Mentality * mPlayer.Level * 10;
							else
								mPlayer.SP = 0;

							mPlayer.AC = 0;
							mPlayer.Experience = 0;
							mPlayer.PotentialExperience = 0;
							mPlayer.Weapon = 0;
							mPlayer.Shield = 0;
							mPlayer.Armor = 0;
							mPlayer.WeaPower = 2;
							mPlayer.ShiPower = 0;
							mPlayer.ArmPower = 0;

							var party = new LorePlayer()
							{
								Year = 774,
								Day = 18,
								Hour = 10,
								Min = 10,
								Map = 6,
								XAxis = 9,
								YAxis = 47,
								Food = 0,
								Gold = 0,
								Arrow = 0,
								Checksum = new int[2],
								Item = new int[10],
								Etc = new int[100]
							};

							Frame.Navigate(typeof(GamePage), new Payload()
							{
								Player = mPlayer,
								Party = party
							});
						}
						else
							Frame.Navigate(typeof(MainPage));
					}
				}
			};

			Window.Current.CoreWindow.KeyUp += chooseClassPageKeyEvent;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			mPlayer = e.Parameter as Lore;

			PlayerNameText.Text = $"당신의 이름 : {mPlayer.Name}";
			PlayerGenderText.Text = $"당신의 성별 : {mPlayer.GenderStr}";
			PlayerCategoryText.Text = $"당신의 계열 : {mPlayer.ClassTypeStr}";

			if (mPlayer.ClassType == ClassCategory.Sword) {
				mPlayer.SwordSkill = 10;
				mPlayer.AxeSkill = 10;
				mPlayer.SpearSkill = 10;
				mPlayer.BowSkill = 10;
				mPlayer.ShieldSkill = 10;
				mPlayer.FistSkill = 10;

				SkillNameText1.Text = "베는  무기류  기술치";
				SkillNameText2.Text = "찍는  무기류  기술치";
				SkillNameText3.Text = "찌르는 무기류 기술치";
				SkillNameText4.Text = "쏘는  무기류  기술치";
				SkillNameText5.Text = "방패  사용    기술치";
				SkillNameText6.Text = "맨손  전투    기술치";
			}
			else {
				mPlayer.AttackMagic = 10;
				mPlayer.PhenoMagic = 10;
				mPlayer.CureMagic = 10;
				mPlayer.SpecialMagic = 10;
				mPlayer.ESPMagic = 10;
				mPlayer.SummonMagic = 10;

				SkillNameText1.Text = "공격 마법 능력";
				SkillNameText2.Text = "변화 마법 능력";
				SkillNameText3.Text = "치료 마법 능력";
				SkillNameText4.Text = "특수 마법 능력";
				SkillNameText5.Text = "초 자연력 능력";
				SkillNameText6.Text = "소환 마법 능력";
			}

			for (var i = 0; i < mClassTextList.Count; i++)
				mClassTextList[i].Text = (i + 1) + ". " + Common.GetClass(mPlayer.ClassType, i + 1);
			UpdateClassAvailable();

			base.OnNavigatedTo(e);
		}

		private void UpdateClassAvailable() {
			for (var i = 0; i < mClassTextList.Count; i++) {
				if (mPlayer.IsClassAvailable(i + 1))
				{
					mClassTextList[i].Foreground = new SolidColorBrush(Colors.White);
					mClassAvailableList[i] = true;
				}	
				else {
					mClassTextList[i].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xaa, 0xaa, 0xaa));
					mClassAvailableList[i] = false;
				}
			}
		}

		private enum FocusItem {
			SetSkill,
			ChooseClass,
			Confirm
		}
	}
}
