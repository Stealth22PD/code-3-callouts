using Rage;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal class Suspect : PedBase
	{

		internal Suspect(string pName, Rage.Vector3 position, bool confirmedSuspect) : base(pName, PedType.Suspect, position)
        {
			if (confirmedSuspect == false) {
				this.Type = PedType.Unknown;
			}
		}

		internal Suspect(string pName, Rage.Model model, Rage.Vector3 position, float heading, bool confirmedSuspect) : base(pName, PedType.Suspect, model, position, heading)
        {
			if (confirmedSuspect == false) {
				this.Type = PedType.Unknown;
			}
		}

		protected internal Suspect(string pName, Rage.PoolHandle handle, bool confirmedSuspect) : base(pName, PedType.Suspect, handle)
        {
			if (confirmedSuspect == false) {
				this.Type = PedType.Unknown;
			}
		}

	}

}