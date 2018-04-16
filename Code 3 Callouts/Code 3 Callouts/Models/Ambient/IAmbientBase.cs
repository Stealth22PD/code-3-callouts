using Rage;

namespace Stealth.Plugins.Code3Callouts.Models.Ambient
{

	internal interface IAmbientBase
	{

		bool Start();
		void Init();
		bool IsEventStarted();
		bool GetRequiredPeds();
		bool GetNearbyPeds();
		bool SpawnRequiredPeds();
		bool CheckPeds();
		bool EndBasedOnDistance();
		void CreateEntityBlips();
		void Process();
		void Dispatch911Call(Vector3 pPosition);
		void End();

		void Delete();
		bool Active { get; set; }
		int PedsRequired { get; }

		bool CanUseExistingPeds { get; }
	}

}