using Rage;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal interface IPedBase
	{

		PedType Type { get; set; }
		string Name { get; set; }
		string DisplayName { get; set; }
		Blip Blip { get; set; }
		//Property SearchArea As Blip
		Vector3 OriginalSpawnPoint { get; set; }
		List<string> SpeechLines { get; set; }
		int SpeechIndex { get; set; }

		string PhysicalCondition { get; set; }
		void Speak();
		void CreateBlip(System.Drawing.Color? pColor = null);
		void DeleteBlip();
		//Sub CreateSearchArea()
		//Sub DeleteSearchArea()

		void SetIsDrunk(bool pValue);
		void AttackPed(Ped pTargetPed);

		void TurnToFaceEntity(Entity pTarget, int pTimeout = 5000);
	}

}