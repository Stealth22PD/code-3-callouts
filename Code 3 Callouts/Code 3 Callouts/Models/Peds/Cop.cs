using Rage;
using LSPD_First_Response.Mod.API;
using static Stealth.Plugins.Code3Callouts.Common;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

	internal class Cop : PedBase
	{

		internal static Cop Create(string pName, Rage.Vector3 position, float heading, bool pIsMale)
		{
			CopType pType = CopType.PoliceOfficer;

			if (Common.IsPlayerinLosSantos() == false) {
				pType = CopType.Sheriff;
			}

			if (pType == CopType.PoliceOfficer) {
				return CreateCityCop(pName, position, heading, pIsMale);
			} else {
				return CreateSheriff(pName, position, heading, pIsMale);
			}
		}

		internal static Cop Create(string pName, Rage.Vector3 position, float heading, bool pIsMale, CopType pType)
		{
			if (pType == CopType.PoliceOfficer) {
				return CreateCityCop(pName, position, heading, pIsMale);
			} else {
				return CreateSheriff(pName, position, heading, pIsMale);
			}
		}

		private static Cop CreateCityCop(string pName, Rage.Vector3 position, float heading, bool pIsMale)
		{
			if (pIsMale == true) {
				return new Cop(pName, "S_M_Y_COP_01", position, heading);
			} else {
				return new Cop(pName, "S_F_Y_COP_01", position, heading);
			}
		}

		private static Cop CreateSheriff(string pName, Rage.Vector3 position, float heading, bool pIsMale)
		{
			if (pIsMale == true) {
				return new Cop(pName, "S_M_Y_SHERIFF_01", position, heading);
			} else {
				return new Cop(pName, "S_F_Y_SHERIFF_01", position, heading);
			}
		}

		//internal Sub New(ByVal pName As String, ByVal position As Rage.Vector3)
		//    MyBase.New(pName, PedType.Cop, position)
		//End Sub

		internal Cop(string pName, Rage.Model model, Rage.Vector3 position, float heading) : base(pName, PedType.Cop, model, position, heading)
        {
			initCop();
		}

		protected internal Cop(string pName, Rage.PoolHandle handle) : base(pName, PedType.Cop, handle)
        {
			initCop();
		}

		private void initCop()
		{
			this.BlockPermanentEvents = true;
			this.RelationshipGroup = new RelationshipGroup("COP");
			this.DisplayName = "Officer";
			this.MakePersistent();
			Functions.SetCopAsBusy(this, true);
		}

		public override void Dismiss()
		{
			Functions.SetCopAsBusy(this, false);
			base.Dismiss();
		}

		internal enum CopType
		{
			PoliceOfficer,
			Sheriff
		}

	}

}