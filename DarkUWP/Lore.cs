using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkUWP
{
	class Lore
	{
		public string Name
		{
			get;
			set;
		}

		public GenderType Gender
		{
			get;
			set;
		}

		public int Class
		{
			get;
			set;
		}

		public int Strength
		{
			get;
			set;
		}

		public int Mentality
		{
			get;
			set;
		}

		public int Concentration
		{
			get;
			set;
		}

		public int Endurance
		{
			get;
			set;
		}

		public int Resistance
		{
			get;
			set;
		}

		public int Agility
		{
			get;
			set;
		}

		public int Accuracy
		{
			get;
			set;
		}

		public int Luck
		{
			get;
			set;
		}

		public int Poison
		{
			get;
			set;
		}

		public int Unconscious
		{
			get;
			set;
		}

		public int Dead
		{
			get;
			set;
		}

		public int HP
		{
			get;
			set;
		}

		public int SP
		{
			get;
			set;
		}

		public int AC
		{
			get;
			set;
		}

		public int Level
		{
			get;
			set;
		}

		public long Experience
		{
			get;
			set;

		}

		public long PotentialExperience
		{
			get;
			set;
		}

		public int Weapon
		{
			get;
			set;
		}

		public int Shield
		{
			get;
			set;
		}

		public int Armor
		{
			get;
			set;
		}

		public int PotentialAC
		{
			get;
			set;
		}

		public int WeaPower
		{
			get;
			set;
		}

		public int ShiPower
		{
			get;
			set;
		}

		public int ArmPower
		{
			get;
			set;
		}

		public ClassCategory ClassType {
			get;
			set;
		}

		public int SwordSkill {
			get;
			set;
		}

		public int AxeSkill {
			get;
			set;
		}

		public int SpearSkill {
			get;
			set;
		}

		public int BowSkill {
			get;
			set;
		}

		public int ShieldSkill {
			get;
			set;
		}

		public int FistSkill {
			get;
			set;
		}

		public int AttackMagic {
			get;
			set;
		}

		public int PhenoMagic {
			get;
			set;
		}

		public int CureMagic {
			get;
			set;
		}

		public int SpecialMagic {
			get;
			set;
		}

		public int ESPMagic {
			get;
			set;
		}

		public int SummonMagic {
			get;
			set;
		}

		public bool IsClassAvailable(int requestClass)
		{
			if (ClassType == ClassCategory.Sword) {
				switch (requestClass) {
					case 1:
						if (SwordSkill >= 10 && AxeSkill >= 10 && SpearSkill >= 10 && BowSkill >= 10 && ShieldSkill >= 10)
							return true;
						else
							return false;
					case 2:
						if (SwordSkill >= 10 && AxeSkill >= 10 && SpearSkill >= 5 && ShieldSkill >= 20)
							return true;
						else
							return false;
					case 3:
						if (SwordSkill >= 40)
							return true;
						else
							return false;
					case 4:
						if (AxeSkill >= 5 && SpearSkill >= 5 && BowSkill >= 40)
							return true;
						else
							return false;
					case 5:
						if (FistSkill >= 40)
							return true;
						else
							return false;
					case 6:
						if (SwordSkill >= 10 && BowSkill >= 10 && FistSkill >= 20)
							return true;
						else
							return false;
					default:
						if (SwordSkill >= 25 && SpearSkill >= 5 && ShieldSkill >= 20 && FistSkill >= 10)
							return true;
						else
							return false;
				}
			}
			else {
				switch (requestClass) {
					case 1:
						if (AttackMagic >= 10 && PhenoMagic >= 10 && CureMagic >= 10)
							return true;
						else
							return false;
					case 2:
						if (SummonMagic >= 10 && PhenoMagic >= 10 && CureMagic >= 10)
							return true;
						else
							return false;
					case 3:
						if (SummonMagic >= 10 && CureMagic >= 10)
							return true;
						else
							return false;
					case 4:
						if (AttackMagic >= 40 && PhenoMagic >= 25 && CureMagic >= 25)
							return true;
						else
							return false;
					case 5:
						if (AttackMagic >= 20 && PhenoMagic >= 20 && CureMagic >= 40 && SummonMagic >= 40)
							return true;
						else
							return false;
					case 6:
						if (AttackMagic >= 10 && PhenoMagic >= 40 && CureMagic >= 30 && SummonMagic >= 20)
							return true;
						else
							return false;
					default:
						if (AttackMagic >= 40 && PhenoMagic >= 40 && CureMagic >= 40 && SpecialMagic >= 20 && ESPMagic >= 20 && SummonMagic >= 20)
							return true;
						else
							return false;
				}
			}
		}

		public string GenderStr {
			get
			{
				switch (Gender)
				{
					case GenderType.Male:
						return "남성";
					case GenderType.Female:
						return "여성";
					default:
						return "불확실함";
				}
			}
		}

		public string ClassTypeStr {
			get {
				switch (ClassType) {
					case ClassCategory.Sword:
						return "전투사계";
					default:
						return "마법사계";
				}
			}
		}

		public string ClassStr {
			get {
				return Common.GetClass(ClassType, Class);
			}
		}
	}

	public enum GenderType {
		Male,
		Female,
		Neutral
	}

	public enum ClassCategory {
		Sword,
		Magic
	}
}
