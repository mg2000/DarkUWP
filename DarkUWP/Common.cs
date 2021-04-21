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

		public static string GetClass(ClassCategory category, int playerClass) {
			if (category == ClassCategory.Sword)
				return SwordClass[playerClass - 1];
			else
				return MagicClass[playerClass - 1];
		}
	}
}
