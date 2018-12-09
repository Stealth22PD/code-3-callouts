using LSPD_First_Response.Engine;
using Rage;
using Rage.Native;
using System.Collections.Generic;
using static Stealth.Plugins.Code3Callouts.Models.Interiors.InteriorDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Interiors
{

	internal class Interior
	{
		internal Interior()
		{
			Init();
		}

		internal Interior(InteriorType pType, SpawnPoint pInteriorSpawnPoint, List<SpawnPoint> pHidingPlaces)
		{
			Type = pType;
			InteriorSpawnPoint = pInteriorSpawnPoint;
			HidingPlaces = pHidingPlaces;
		}

		private void Init()
		{
			Type = InteriorType.Null;
			InteriorSpawnPoint = SpawnPoint.Zero;
			HidingPlaces = new List<SpawnPoint>();
		}

		internal void LoadInterior()
		{
			if (InteriorSpawnPoint != SpawnPoint.Zero) {
				int mInterior = NativeFunction.Natives.GetInteriorAtCoords<int>(InteriorSpawnPoint.Position.X, InteriorSpawnPoint.Position.Y, InteriorSpawnPoint.Position.Z);
                NativeFunction.Natives.x2ca429c029ccf247(mInterior); // _LOAD_INTERIOR
                NativeFunction.Natives.SetInteriorActive(mInterior, true);
                NativeFunction.Natives.DisableInterior(mInterior, false);
			}
		}

		internal InteriorType Type { get; set; }
		internal SpawnPoint InteriorSpawnPoint { get; set; }
		internal List<SpawnPoint> HidingPlaces { get; set; }
	}

}