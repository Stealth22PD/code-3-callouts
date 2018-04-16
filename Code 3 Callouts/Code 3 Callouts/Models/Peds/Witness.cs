using Rage;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal class Witness : PedBase
	{

		internal Witness(string pName, Rage.Vector3 position) : base(pName, PedType.Witness, position)
        {
			
		}

		internal Witness(string pName, Rage.Model model, Rage.Vector3 position, float heading) : base(pName, PedType.Witness, model, position, heading)
        {
			
		}

		protected internal Witness(string pName, Rage.PoolHandle handle) : base(pName, PedType.Witness, handle)
        {
			
		}

	}

}