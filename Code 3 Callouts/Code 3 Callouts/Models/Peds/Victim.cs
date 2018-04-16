using Rage;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal class Victim : PedBase, IPedBase
	{

		internal Victim(string pName, Rage.Vector3 position) : base(pName, PedType.Victim, position)
        {
			
		}

		internal Victim(string pName, Rage.Model model, Rage.Vector3 position, float heading) : base(pName, PedType.Victim, model, position, heading)
        {
			
		}

		protected internal Victim(string pName, Rage.PoolHandle handle) : base(pName, PedType.Victim, handle)
        {
			
		}

	}

}