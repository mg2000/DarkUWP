using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.XboxLive.Storage;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DarkUWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private int mFirstLine;
		private int mLastLine;

		private float mOffset = 0;
		private float mVelocity = 0;
		private const float mTargetSpeed = 0.5f;
		private float mTargetVelocity = 0;

		private CanvasLinearGradientBrush mTextOpacityBrush;
		private CanvasLinearGradientBrush mBlurOpacityBrush;

		static CanvasTextFormat symbolText = new CanvasTextFormat()
		{
			FontSize = 30,
			FontFamily = "Segoe UI",
			HorizontalAlignment = CanvasHorizontalAlignment.Center,
			VerticalAlignment = CanvasVerticalAlignment.Center
		};

		private static float mLineHeight = symbolText.FontSize * 1.5f;

		private int mFocusItem = 1;

		public MainPage()
		{
			this.InitializeComponent();

			SystemNavigationManager.GetForCurrentView().BackRequested += (sender, e) =>
			{
				if (!e.Handled)
				{
					e.Handled = true;
				}
			};

			SyncSaveData();
		}

		private async void SyncSaveData()
		{
			var users = await User.FindAllAsync();
			var gameSaveTask = await GameSaveProvider.GetForUserAsync(users[0], "00000000-0000-0000-0000-00007ede8c1b");

			Debug.WriteLine($"클라우드 동기화 연결 결과: {gameSaveTask.Status}");

			if (gameSaveTask.Status == GameSaveErrorStatus.Ok)
			{
				var gameSaveProvider = gameSaveTask.Value;

				var gameSaveContainer = gameSaveProvider.CreateContainer("DarkSaveContainer");

				var saveData = new List<SaveData>();
				var loadFailed = false;

				for (var i = 0; i < 9; i++)
				{
					string saveName;
					if (i == 0)
						saveName = "darkSave";
					else
						saveName = $"darkSave{i}";

					var result = await gameSaveContainer.GetAsync(new string[] { saveName });
					if (result.Status == GameSaveErrorStatus.Ok)
					{
						IBuffer loadedBuffer;

						result.Value.TryGetValue(saveName, out loadedBuffer);

						if (loadedBuffer == null)
						{
							loadFailed = true;
							break;
						}

						var reader = DataReader.FromBuffer(loadedBuffer);
						var dataSize = reader.ReadUInt32();

						var buffer = new byte[dataSize];

						reader.ReadBytes(buffer);

						var loadData = Encoding.UTF8.GetString(buffer);

						saveData.Add(JsonConvert.DeserializeObject<SaveData>(loadData));
					}
					else if (result.Status == GameSaveErrorStatus.BlobNotFound)
					{
						saveData.Add(null);
					}
					else if (result.Status != GameSaveErrorStatus.BlobNotFound)
					{
						loadFailed = true;
						break;
					}
				}

				if (loadFailed)
					await new MessageDialog("클라우드 서버에서 세이브를 가져올 수 없습니다. 기기에 저장된 세이브를 사용합니다.").ShowAsync();
				else
				{
					var storageFolder = ApplicationData.Current.LocalFolder;
					var differentBuilder = new StringBuilder();
					var differentID = new List<int>();

					for (var i = 0; i < saveData.Count; i++)
					{
						string GetSaveName(int id)
						{
							if (id == 0)
								return "본 게임 데이터";
							else
								return $"게임 데이터 {id} (부)";
						}


						string idStr;
						if (i == 0)
							idStr = "";
						else
							idStr = i.ToString();

						try
						{
							var localSaveFile = await storageFolder.GetFileAsync($"darkSave{idStr}.dat");
							var localSaveData = JsonConvert.DeserializeObject<SaveData>(await FileIO.ReadTextAsync(localSaveFile));

							if (localSaveData == null)
							{
								if (saveData[i] != null)
								{
									differentBuilder.Append(GetSaveName(i)).Append("\r\n");
									differentBuilder.Append("클라우드 데이터만 존재").Append("\r\n\r\n");

									differentID.Add(i);
								}
							}
							else
							{
								if (saveData[i] == null)
								{
									differentBuilder.Append(GetSaveName(i)).Append("\r\n");
									differentBuilder.Append("기기 데이터만 존재").Append("\r\n\r\n"); ;

									differentID.Add(i);
								}
								else
								{
									if (saveData[i].SaveTime != localSaveData.SaveTime)
									{
										differentBuilder.Append(GetSaveName(i)).Append("\r\n");
										differentBuilder.Append($"클라우드: {new DateTime(saveData[i].SaveTime):yyyy.MM.dd HH:mm:ss}").Append("\r\n");
										differentBuilder.Append($"기기: {new DateTime(localSaveData.SaveTime):yyyy.MM.dd HH:mm:ss}").Append("\r\n\r\n");

										differentID.Add(i);
									}
								}
							}
						}
						catch (FileNotFoundException e) {
							Debug.WriteLine($"세이브 파일 없음: {e.Message}");

							if (saveData[i] != null)
							{
								differentBuilder.Append(GetSaveName(i)).Append("\r\n");
								differentBuilder.Append("클라우드 데이터만 존재").Append("\r\n\r\n");

								differentID.Add(i);
							}
						}
					}

					if (differentID.Count > 0)
					{
						var differentDialog = new MessageDialog("클라우드/기기간 데이터 동기화가 되어 있지 않습니다. 어느 데이터를 사용하시겠습니까?\r\n\r\n" + differentBuilder.ToString());
						differentDialog.Commands.Add(new UICommand("클라우드"));
						differentDialog.Commands.Add(new UICommand("기기"));

						differentDialog.DefaultCommandIndex = 0;
						differentDialog.CancelCommandIndex = 1;

						var chooseData = await differentDialog.ShowAsync();
						if (chooseData.Label == "클라우드")
						{
							for (var i = 0; i < differentID.Count; i++)
							{
								if (saveData[differentID[i]] != null)
								{
									string idStr;
									if (differentID[i] == 0)
										idStr = "";
									else
										idStr = differentID[i].ToString();

									var saveFile = await storageFolder.CreateFileAsync($"darkSave{idStr}.dat", CreationCollisionOption.ReplaceExisting);
									await FileIO.WriteTextAsync(saveFile, JsonConvert.SerializeObject(saveData[differentID[i]]));
								}
							}
						}
					}
				}

				InitializeKeyEvent();
			}
			else
			{
				await new MessageDialog("클라우드 서버에 접속할 수 없습니다. 기기에 저장된 세이브를 사용합니다.").ShowAsync();
				InitializeKeyEvent();
			}
		}

		private void InitializeKeyEvent()
		{
			SyncPanel.Visibility = Visibility.Collapsed;
			prologControl.Visibility = Visibility.Visible;
			mTargetVelocity = mTargetSpeed;

			TypedEventHandler<CoreWindow, KeyEventArgs> mainPageKeyUpEvent = null;
			mainPageKeyUpEvent = async (sender, args) =>
			{
				Debug.WriteLine($"키보드 테스트: {args.VirtualKey}");

				if (prologControl.Visibility == Visibility.Visible)
				{
					mTargetVelocity = 0;
					prologControl.Visibility = Visibility.Collapsed;

					mainmenuPanel.Visibility = Visibility.Visible;
				}
				else
				{
					if (args.VirtualKey == VirtualKey.Enter || args.VirtualKey == VirtualKey.GamepadA)
					{
						if (mFocusItem == 1)
						{
							Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;

							var dialog = new InputNameBox();
							var result = await dialog.ShowAsync();

							if (result == ContentDialogResult.Primary) {
								if (dialog.PlayerName == "")
									Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
								else
									Frame.Navigate(typeof(NewGamePage), dialog.PlayerName);
							}
							else
								Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
						}
						else if (mFocusItem == 2)
						{
							var saveFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync("darkSave.dat");
							if (saveFile == null)
							{
								await new MessageDialog("저장된 게임이 없습니다. 새로운 게임을 시작해 주십시오.", "저장된 게임 없음").ShowAsync();
							}
							else
							{
								Window.Current.CoreWindow.KeyUp -= mainPageKeyUpEvent;
								Frame.Navigate(typeof(GamePage), null);
							}
						}
						else
						{
							await new MessageDialog("원작: 다크 메이지 실리안 카미너스(안영기, 1994)\r\n\r\n" +
							"음악: \r\n" +
							"Town, Ground: https://www.zapsplat.com/\r\n" +
							"Den: https://juhanijunkala.com/\r\n" +
							"Keep: https://opengameart.org/content/boss-battle-theme", "저작권 정보").ShowAsync();
						}
					}
					else if (mFocusItem == 1)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 2;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 3;
						}

					}
					else if (mFocusItem == 2)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 3;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 1;
						}
					}
					else if (mFocusItem == 3)
					{
						if (args.VirtualKey == VirtualKey.Down || args.VirtualKey == VirtualKey.GamepadDPadDown || args.VirtualKey == VirtualKey.GamepadLeftThumbstickDown)
						{
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							newGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 1;
						}
						else if (args.VirtualKey == VirtualKey.Up || args.VirtualKey == VirtualKey.GamepadDPadUp || args.VirtualKey == VirtualKey.GamepadLeftThumbstickUp)
						{
							exitGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0x53, 0x50, 0xf7));
							loadGameItem.Foreground = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xff, 0xff));
							mFocusItem = 2;
						}

					}
				}
			};

			Window.Current.CoreWindow.KeyUp += mainPageKeyUpEvent;
		}

		private void prologControl_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
		{
			var stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.0f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.1f },
				new CanvasGradientStop() { Color=Color.FromArgb(0xff, 0x53, 0xef, 0xef), Position = 0.9f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 1.0f }
			};

			mTextOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);

			stops = new CanvasGradientStop[]
			{
				new CanvasGradientStop() { Color=Colors.White, Position=0.0f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.3f },
				new CanvasGradientStop() { Color=Colors.Transparent, Position = 0.7f },
				new CanvasGradientStop() { Color=Colors.White, Position = 1.0f },
			};

			mBlurOpacityBrush = new CanvasLinearGradientBrush(sender, stops, CanvasEdgeBehavior.Clamp, CanvasAlphaMode.Premultiplied);
		}

		private void prologControl_Update(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedUpdateEventArgs args)
		{
			float height = (float)sender.Size.Height;
			float totalHeight = characters.Length * mLineHeight + height;

			//if (mOffset >= 400)
			//    return;

			mVelocity = mVelocity * 0.90f + mTargetVelocity * 0.10f;

			mOffset = mOffset + mVelocity;

			mOffset = mOffset % totalHeight;
			while (mOffset < 0)
				mOffset += totalHeight;

			float top = height - mOffset;
			mFirstLine = Math.Max(0, (int)(-top / mLineHeight));
			mLastLine = Math.Min(characters.Length, (int)((height + mLineHeight - top) / mLineHeight));
		}

		private void prologControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
		{
			var textDisplay = GenerateTextDisplay(sender, (float)sender.Size.Width, (float)sender.Size.Height);

			var blurEffect = new GaussianBlurEffect()
			{
				Source = textDisplay,
				BlurAmount = 10
			};

			mTextOpacityBrush.StartPoint = mBlurOpacityBrush.StartPoint = new Vector2(0, 0);
			mTextOpacityBrush.EndPoint = mBlurOpacityBrush.EndPoint = new Vector2(0, (float)sender.Size.Height);

			var ds = args.DrawingSession;

			//using (ds.CreateLayer(mBlurOpacityBrush))
			//{
			//    ds.DrawImage(blurEffect);
			//}

			using (ds.CreateLayer(mTextOpacityBrush))
			{
				ds.DrawImage(textDisplay);
			}
		}

		private CanvasCommandList GenerateTextDisplay(ICanvasResourceCreator resourceCreator, float width, float height)
		{
			var cl = new CanvasCommandList(resourceCreator);

			using (var ds = cl.CreateDrawingSession())
			{
				float top = height - mOffset;

				float center = width / 2.0f;
				float symbolPos = center - 5.0f;
				float labelPos = center + 5.0f;

				for (int i = mFirstLine; i < mLastLine; ++i)
				{
					float y = top + mLineHeight * i;
					int index = i;

					if (index < characters.Length)
					{
						ds.DrawText(characters[index], labelPos, y, Color.FromArgb(0xff, 0x53, 0xef, 0xef), symbolText);
					}
				}
			}

			return cl;
		}

		private static string[] characters = new string[]
		{
			" 이 책을 쓴 나는 세계의 주시자이며  금빛과 푸른빛의 눈부신 이중",
			"성의 이름을 가진 타임 워커 알비레오라고 한다.",
			"",
			" 내가 시간을 여행하던 중 우연히 로드 안의 시대를  스쳐가게 되었",
			"다.  그 시대는 금빛 찬란한 문화와 영원할 것만 같은 평화가  모든",
			"대륙을 감싸고 있었고,  위대한 로드 안의 악에 대한 결계는 시리우",
			"스의 별빛처럼 강하게 그들을 보호하고 있었다.",
			" 그러던 어느 날, 나는 다섯 개의 검은 흉성이 로어 대륙의 강력한 결",
			"계를 뚫고 들어와 그들의 운명 속에 스며드는 것을 보았다. 그것들은",
			"정해진 때가 되면 하나씩 그 세계의 운명을 바꾸어 놓을 것 같았다.",
			" 그런 후에 내가 본 것은 아래의 다섯 가지였다.",
			"",
			"첫 번째 흉성이 나타나는 날,  평화는 로어의 신과 함께 대지로 추락",
			"할 것이며 공간이 어긋나고 대륙이 진동하며  새로운 존재가 나타난",
			"다.",
			"",
			"두 번째 흉성이 나타나는 날, 그는 용암의 대륙으로부터 세상을 뒤흔",
			"들게 되며 그는 네크로만서라 불린다.",
			"",
			"세 번째 흉성이 나타나는 날, 네크로만서를 이기는 자는 아무도 없게",
			"된다.",
			"",
			"네 번째 흉성이 나타나는 날,  메너스의 달이 붉게 물들 때 어둠의 영",
			"혼이 나타나 세계의 종말을 예고한다.",
			"",
			"다섯 번째 흉성이 나타나는 날,  내가 본 다섯 번의 흉성 중에 하나가",
			"나타나지 않았음을 알아낸다."
		};
	}
}
