using Rage;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using System;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts
{

	internal interface ICalloutBase
	{

		void DisplayObjective();
		Vector3 CalculateSpawnpoint();
		bool PerformPedChecks();
		void OnArrivalAtScene();
		void CreateBlip();
		void DeleteBlip();
		void OfficerDown();
		void DeleteEntities();

		void AskPedToFollowOfficer();
		Guid CalloutID { get; set; }
		string Objective { get; set; }
		DateTime CallDispatchTime { get; set; }
		CallResponseType ResponseType { get; set; }
		bool IsFixedSpawnPoint { get; }
		bool RequiresSafePedPoint { get; }
		CalloutState CalloutState { get; set; }
		Blip CallBlip { get; set; }
		string CallDetails { get; set; }
		List<Blip> Markers { get; set; }
		bool ShowAreaBlipBeforeAccepting { get; }

		bool SkipRespondingState { get; set; }

		List<Rage.PoolHandle> PedsToIgnore { get; set; }

		bool FoundPedSafeSpawn { get; set; }
	}

}