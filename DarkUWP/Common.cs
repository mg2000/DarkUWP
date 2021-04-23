using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkUWP
{
	class Common
	{
		public static string[] SwordClass = new string[] { "투사", "기사", "검사", "사냥꾼", "전투승", "암살자", "전사", "불확실함" };
		public static string[] MagicClass = new string[] { "메이지", "컨져러", "주술사", "위저드", "강령술사", "대마법사", "타임워커", "불확실함" };

		private static string[] mWeaponNames = new string[] {
			"불확실한 무기", 
			"맨손", 
			"단검", "그라디우스", "샤벨", "신월도", "인월도",
			"장검", "프렘버그", "곤봉", "소형 도끼", "프레일",
			"전투용 망치", "철퇴", "양날 전투 도끼", "핼버드", "단도",
			"기병창", "단창", "레이피어", "삼지창", "랜서",
			"도끼창", "블로우 파이프", "표창", "투석기", "투창",
			"활", "석궁", "아르발레스트",
			"화염", "해일", "폭풍", "지진", "이빨", "촉수", "창", 
			"발톱", "바위", "화염검", "동물의 뼈", "번개 마법",
			"점토", "강철 주먹", "산성 가스", "전광", "독가스",
			"불꽃", "염소 가스", "한기", "냉동 가스"
		};

		private static string[,] mMagicNames = new string[,] {
			{ "마법 화살", "마법 화구", "마법 단창", "독 바늘", "마법 발화", "냉동 광선", "춤추는 검", "맥동 광선", "직격 뇌전", "필멸 주문" },
			{ "공기 폭풍", "열선 파동", "초음파", "유독 가스", "초냉기", "화염 지대", "브리자드", "에너지 장막", "인공 지진", "차원 이탈" },
			{ "마법의 횃불", "주시자의 눈", "공중 부상", "물위를 걸음", "늪위를 걸음", "기화 이동", "지형 변화", "공간 이동", "식량 제조", "대지형 변화" },
			{ "한명 치료", "한명 독 제거", "한명 의식 돌림", "한명 부활", "한명 복합 치료", "모두 치료", "모두 독 제거", "모두 의식 돌림", "모두 부활", "모두 복합 치료" },
			{ "한명 기술 무력화", "한명 방어 무력화", "한명 능력 저하", "한명 마법 불능", "한명 탈 초인화", "모두 기술 무력화", "모두 방어 무력화", "모두 능력 저하", "모두 마법 불능", "모두 탈 초인화" },
			{ "투시", "예언", "독심", "천리안", "염력", "수소 핵융합", "공포 생성", "환상 생성", "신진 대사 조절", "유체 이탈" },
			{ "불의 정령 소환", "물의 정령 소환", "공기의 정령 소환", "땅의 정령 소환", "죽은 자의 소생", "다른 차원 생물 소환", "거인을 부름", "고렘을 부름", "용을 부름", "라이칸스로프 소환" }
		};

		public static string GetClass(ClassCategory category, int playerClass) {
			if (category == ClassCategory.Sword)
				return SwordClass[playerClass - 1];
			else
				return MagicClass[playerClass - 1];
		}

		public static string GetWeaponName(int weapon) {
			if (0 <= weapon && weapon <= 49)
				return mWeaponNames[weapon + 1];
			else
				return mWeaponNames[0];
		}

		public static string GetWeaponNameJosa(int weapon) {
			var weaponName = GetWeaponName(weapon);

			if ((weaponName[weaponName.Length - 1] - 0xAC00) % 28 + 0x11A8 - 1 == 0)
				return weaponName;
			else
				return weaponName + "으";
		}

		public static string GetMagicName(int playerClass, int magic) {
			if (0 <= playerClass && playerClass <= 6 && 1 <= magic && magic <= 10)
				return mMagicNames[playerClass, magic - 1];
			else
				return "알수 없는 마법";
		}
	}
}
