using Rage;

namespace Stealth.Plugins.Code3Callouts.Models.Interiors
{

	internal class Residence
	{
		internal Residence()
		{
			init();
		}

		internal Residence(Interior pInterior, Vector3 pEntryPoint)
		{
			Interior = pInterior;
			EntryPoint = pEntryPoint;
		}

		private void init()
		{
			Interior = null;
			EntryPoint = Vector3.Zero;
		}

		internal Interior Interior { get; set; }
		internal Vector3 EntryPoint { get; set; }
	}

}