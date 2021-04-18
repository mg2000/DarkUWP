using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkUWP
{
	class Common
	{
		public static string GetClass(ClassCategory category, int playerClass) {
			if (category == ClassCategory.Sword) {
				switch (playerClass)
				{
					case 1:
						return "투  사";
					case 2:
						return "기  사";
					case 3:
						return "검  사";
					case 4:
						return "사냥꾼";
					case 5:
						return "전투승";
					case 6:
						return "암살자";
					case 7:
						return "전  사";
					default:
						return "불확실함";
				};
			}
			else {
				switch (playerClass)
				{
					case 1:
						return "메이지";
					case 2:
						return "컨져러";
					case 3:
						return "주술사";
					case 4:
						return "위저드";
					case 5:
						return "강령술사";
					case 6:
						return "대마법사";
					case 7:
						return "타임워커";
					default:
						return "불확실함";
				};
			}
		}
	}
}
