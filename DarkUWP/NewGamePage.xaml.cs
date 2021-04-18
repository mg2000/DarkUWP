using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
	public sealed partial class NewGamePage : Page
	{
		private Lore mPlayer;

		private FocusItem mFocusItem = FocusItem.Gender;

		private List<TextBlock> mAnswerList = new List<TextBlock>();
		private int mAnswer = 0;

		private List<QuestionInfo> mQuestionList = new List<QuestionInfo>();
		private int mQuestionIdx;
		private int[] mTransData = new int[5];

		public NewGamePage()
		{
			this.InitializeComponent();

			mPlayer = new Lore();

			mAnswerList.Add(Answer1);
			mAnswerList.Add(Answer2);
			mAnswerList.Add(Answer3);

			mQuestionList.Add(new QuestionInfo(
				"당신이 한 밤중에 공부하고 있을때 밖에서 무슨 소리가 들렸다",
				new string[] {
					"1] 밖으로 나가서 알아본다",
					"2] 그 소리가 무엇일까 생각을 한다",
					"3] 공부에만 열중한다"
				},
				new int[] {
					0, 1, 2
				}));

			mQuestionList.Add(new QuestionInfo(
				"당신은 체력장 오래달리기에서 포기할 수 없는 한 바퀴를 남겨 놓고 거의 탈진 상태가 되었다",
				new string[] {
					"1] 힘으로 밀고 나간다",
					"2] 정신력으로 버티며 달린다",
					"3] 그래도 여태까지와 마찬가지로 달린다"
				},
				new int[] {
					0, 1, 3
				}));

			mQuestionList.Add(new QuestionInfo(
				"당신은 이 게임 속에서 적들에게 완전히 포위되어 승산 없이 싸우고 있다",
				new string[] {
					"1] 힘이 남아 있는한 죽을때까지 싸운다",
					"2] 한가지라도 탈출할 가능성을 찾는다",
					"3] 일단 싸우면서 여러 방법을 생각한다"
				},
				new int[] {
					0, 1, 4
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"당신은 매우 복잡한 매듭을 풀어야하는 일이 생겼다",
				new string[] {
					"1] 칼로 매듭을 잘라 버린다",
					"2] 매듭의 끝부분 부터 차근차근 훓어본다",
					"3] 어쨌든 계속 풀려고 손을 놀린다"
				},
				new int[] {
					0, 2, 3
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"허허 벌판을 걸어가던 당신은 갑작스런 우박을 만난다",
				new string[] {
					"1] 당항한 나머지 피할곳을 찾아 뛴다",
					"2] 침착하게 주위를 살펴 안전한곳을 찾는다",
					"3] 우박 정도는 그냥 견딘다"
				},
				new int[] {
					0, 2, 4
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"집안에 불이나서 탈출하려는데 나무로 만든 문이 좀처럼 열리지 않는다",
				new string[] {
					"1] 다른 탈출구를 찾아간다",
					"2] 1] 번과 같은 불확실한 도전을 하는것 보다는 확실한 탈출구인 이 문을 끝까지 열려한다",
					"3] 나무문이 타서 구멍이 생길때까지 기다려 탈출한다"
				},
				new int[] {
					0, 3, 4
				}
			));


			mQuestionList.Add(new QuestionInfo(
				"고대에 태어난 당신은, 한날 당신의 눈앞에서 물체가 사라지는 마술을 보았을때 당신의 해석은 ?",
				new string[] {
					"1] 이것은 마법이다",
					"2] 이것은 사람의 새로운 능력이다",
					"3] 단순한 사람의 눈속임이다"
				},
				new int[] {
					1, 2, 3
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"시험 기간에 당신이 도서관에서 공부를 하려는데 주위가 너무 시끄럽다",
				new string[] {
					"1] 상관없이 참으며 공부한다",
					"2] 너무 공부를 열심히해서 그런 소리가 안 들린다",
					"3] 시끄러움에 대항하는 마음으로 공부한다"
				},
				new int[] {
					1, 2, 4
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"직장 생활을 하던 당신은 아무 이유없이 상관에게 심한 욕을 들었다",
				new string[] {
					"1] 겉으로는 순종하면서 속으로는 감정을 샇는다",
					"2] 웬만하면 참고 넘긴다",
					"3] 상관인걸 무시하고 이유를 들라며 대든다"
				},
				new int[] {
					1, 3, 4
				}
			));

			mQuestionList.Add(new QuestionInfo(
				"당신이 새로운 프로그램을 짜던중 알수없는 오류가 생겼다",
				new string[] {
					"1] 차근차근 순서도를 생각하며 오류를 찾는다",
					"2] 여러번 실행 시키며 오류를 찾는다",
					"3] 오류가 작으면 그냥 사용한다"
				},
				new int[] {
					2, 3, 4
				}
			));

			TypedEventHandler<CoreWindow, KeyEventArgs> newGamePageKeyEvent = null;
			newGamePageKeyEvent = async (sender, args) =>
			{
				void FocusMenuItem()
				{
					for (var i = 0; i < mAnswerList.Count; i++)
					{
						if (i == mAnswer)
							mAnswerList[i].Foreground = new SolidColorBrush(Colors.Yellow);
						else
							mAnswerList[i].Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x28, 0xf3, 0xf3));
					}
				}

				void ShowQuestion() {
					QuestionTitle.Text = mQuestionList[mQuestionIdx].Question;

					mAnswer = 0;
					for (var i = 0; i < 3; i++) {
						mAnswerList[i].Text = mQuestionList[mQuestionIdx].Answer[i];
					}

					FocusMenuItem();
				}

				if (mFocusItem == FocusItem.Gender)
				{
					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						mPlayer.Gender = GenderType.Male;

						GenderMale.Foreground = new SolidColorBrush(Colors.Yellow);
						GenderFemale.Foreground = new SolidColorBrush(Colors.White);
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						mPlayer.Gender = GenderType.Female;

						GenderMale.Foreground = new SolidColorBrush(Colors.White);
						GenderFemale.Foreground = new SolidColorBrush(Colors.Yellow);
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						mFocusItem = FocusItem.Category;

						GenderMale.Foreground = new SolidColorBrush(Colors.White);
						GenderFemale.Foreground = new SolidColorBrush(Colors.White);

						CategoryTitle.Visibility = Visibility.Visible;
						CategorySword.Visibility = Visibility.Visible;
						CategoryMagic.Visibility = Visibility.Visible;
					}
					else if (args.VirtualKey == VirtualKey.Escape || args.VirtualKey == VirtualKey.GamepadB) {
						Window.Current.CoreWindow.KeyUp -= newGamePageKeyEvent;
						Frame.Navigate(typeof(MainPage));
					}
				}
				else if (mFocusItem == FocusItem.Category)
				{
					if (args.VirtualKey == VirtualKey.Left || args.VirtualKey == VirtualKey.GamepadLeftThumbstickLeft || args.VirtualKey == VirtualKey.GamepadDPadLeft)
					{
						mPlayer.ClassType = ClassCategory.Sword;

						CategorySword.Foreground = new SolidColorBrush(Colors.Yellow);
						CategoryMagic.Foreground = new SolidColorBrush(Colors.White);
					}
					else if (args.VirtualKey == VirtualKey.Right || args.VirtualKey == VirtualKey.GamepadLeftThumbstickRight || args.VirtualKey == VirtualKey.GamepadDPadRight)
					{
						mPlayer.ClassType = ClassCategory.Magic;

						CategorySword.Foreground = new SolidColorBrush(Colors.White);
						CategoryMagic.Foreground = new SolidColorBrush(Colors.Yellow);
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						mFocusItem = FocusItem.Question;

						CategorySword.Foreground = new SolidColorBrush(Colors.White);
						CategoryMagic.Foreground = new SolidColorBrush(Colors.White);

						QuestionHeader.Visibility = Visibility.Visible;
						QuestionTitle.Visibility = Visibility.Visible;
						Answer1.Visibility = Visibility.Visible;
						Answer2.Visibility = Visibility.Visible;
						Answer3.Visibility = Visibility.Visible;

						ShowQuestion();
					}
				}
				else if (mFocusItem == FocusItem.Question) {
					if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp || args.VirtualKey == VirtualKey.GamepadDPadUp)
					{
						if (mAnswer == 0)
							mAnswer = mAnswerList.Count - 1;
						else
							mAnswer--;

						FocusMenuItem();
					}
					else if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown || args.VirtualKey == VirtualKey.GamepadDPadDown)
					{
						mAnswer = (mAnswer + 1) % mAnswerList.Count;

						FocusMenuItem();
					}
					else if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mQuestionIdx < mQuestionList.Count)
						{
							mTransData[mQuestionList[mQuestionIdx].TransIdx[mAnswer]]++;

							mQuestionIdx++;
							ShowQuestion();
						}
						else
						{
							for (var i = 0; i < 5; i++)
							{
								switch (mTransdata[i])
								{
									case 0:
										mTransdata[i] = 5;
										break;
									case 1:
										mTransdata[i] = 7;
										break;
									case 2:
										mTransdata[i] = 11;
										break;
									case 3:
										mTransdata[i] = 14;
										break;
									case 4:
										mTransdata[i] = 17;
										break;
									case 5:
										mTransdata[i] = 19;
										break;
									case 6:
										mTransdata[i] = 20;
										break;
									default:
										mTransdata[i] = 10;
										break;
								}
							}

							mPlayerList[0].Strength = mTransdata[0];
							mPlayerList[0].Mentality = mTransdata[1];
							mPlayerList[0].Concentration = mTransdata[2];
							mPlayerList[0].Endurance = mTransdata[3];
							mPlayerList[0].Resistance = mTransdata[4];

							var offset = 4;
							if (mPlayerList[0].Gender == "male")
							{
								mPlayerList[0].Strength += offset;
								if (mPlayerList[0].Strength <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Strength - 20;
									mPlayerList[0].Strength = 20;
								}

								mPlayerList[0].Endurance += offset;
								if (mPlayerList[0].Endurance <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Endurance - 20;
									mPlayerList[0].Endurance = 20;
								}

								mPlayerList[0].Resistance += offset;
								if (mPlayerList[0].Resistance > 20)
									mPlayerList[0].Resistance = 20;
							}
							else
							{
								mPlayerList[0].Mentality += offset;
								if (mPlayerList[0].Mentality <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Mentality - 20;
									mPlayerList[0].Mentality = 20;
								}

								mPlayerList[0].Concentration += offset;
								if (mPlayerList[0].Concentration <= 20)
									offset = 0;
								else
								{
									offset = mPlayerList[0].Concentration - 20;
									mPlayerList[0].Concentration = 20;
								}

								mPlayerList[0].Resistance += offset;
								if (mPlayerList[0].Resistance > 20)
									mPlayerList[0].Resistance = 20;
							}

							DefaultInputGrid.Visibility = Visibility.Collapsed;

							mPlayerList[0].Gender = mFocusGender;
							var genderKor = mPlayerList[0].Gender == "male" ? "남성" : "여성";

							UserNameFinalText.Text = $"당신의 이름은 {mPlayerList[0].Name}입니다.";
							UserGenderFinalText.Text = $"당신의 성별은 {genderKor}입니다.";

							StrengthText.Text = mPlayerList[0].Strength.ToString();
							MentalityText.Text = mPlayerList[0].Mentality.ToString();
							ConcentrationText.Text = mPlayerList[0].Concentration.ToString();
							EnduranceText.Text = mPlayerList[0].Endurance.ToString();
							ResistanceText.Text = mPlayerList[0].Resistance.ToString();

							mPlayerList[0].HP = mPlayerList[0].Endurance;
							mPlayerList[0].SP = mPlayerList[0].Mentality;
							mPlayerList[0].ESP = mPlayerList[0].Concentration;

							HPText.Text = mPlayerList[0].HP.ToString();
							SPText.Text = mPlayerList[0].SP.ToString();
							ESPText.Text = mPlayerList[0].ESP.ToString();

							RemainPointText.Text = mRemainPoint.ToString();

							for (var i = 0; i < 3; i++)
								mTransdata[i] = 0;

							ExtraInputGrid.Visibility = Visibility.Visible;

							mAssignID = 0;
							mFocusItem = FocusItem.AssignPoint;
						}
					}
				}
			};

			Window.Current.CoreWindow.KeyUp += newGamePageKeyEvent;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			mPlayer.Name = e.Parameter.ToString();
			mPlayer.Gender = GenderType.Male;
			mPlayer.ClassType = ClassCategory.Sword;

			base.OnNavigatedTo(e);
		}

		private enum FocusItem
		{
			Gender,
			Category,
			Question,
			AssignPoint,
			SelectClass,
			CompleteCreate,
			SelectFriend,
			Confirm
		}

		private class QuestionInfo {
			public string Question
			{
				get;
				private set;
			}

			public string[] Answer {
				get;
				private set;
			}

			public int[] TransIdx {
				get;
				private set;
			}

			public QuestionInfo(string question, string[] answer, int[] transIdx) {
				Question = question;
				Answer = answer;
				TransIdx = transIdx;
			}
		}
	}
}
