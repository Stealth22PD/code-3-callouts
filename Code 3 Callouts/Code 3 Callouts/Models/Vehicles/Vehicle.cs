using Rage;
using Stealth.Common.Models;
using static Stealth.Common.Natives.Vehicles;
using Stealth.Plugins.Code3Callouts.Util;
using System.Drawing;
using System;

namespace Stealth.Plugins.Code3Callouts.Models.Vehicles
{

	internal class Vehicle : Rage.Vehicle, IVehicle, IHandleable
	{

		internal Vehicle(Rage.Model model, Rage.Vector3 position) : base(model, position)
        {
            Init();
		}

		internal Vehicle(Rage.Model model, Rage.Vector3 position, float heading) : base(model, position, heading)
        {
            Init();
		}

		protected internal Vehicle(Rage.PoolHandle handle) : base(handle)
        {
            Init();
		}

        public void Init()
		{
			_Colors = new VehicleColor();
			_Colors.PrimaryColor = EPaint.Unknown;
			_Colors.SecondaryColor = EPaint.Unknown;
		}

        public void FillColorValues()
		{
			try {
				if (this != null) {
					if (this.Exists()) {
						_Colors = VehicleHelper.GetVehicleColors(this);
					} else {
						Logger.LogVerboseDebug("Error getting vehicle colors -- Vehicle does not exist");
					}
				} else {
					Logger.LogVerboseDebug("Error getting vehicle colors -- Vehicle is null");
				}
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error getting vehicle colors -- " + ex.Message);
				_Colors = new VehicleColor();
				_Colors.PrimaryColor = EPaint.Unknown;
				_Colors.SecondaryColor = EPaint.Unknown;
			}
		}

		public override void Dismiss()
		{
			Logger.LogVerboseDebug("Deleting blip and dismissing vehicle");
			DeleteBlip();
			base.Dismiss();
		}

        public override void Delete()
		{
			Logger.LogVerboseDebug("Deleting blip and vehicle");
			DeleteBlip();
			base.Delete();
		}

        public void CreateBlip(Color? color = null)
		{
			if (color == null) {
				color = Color.Red;
			}

			if (this.Exists()) {
				this.Blip = new Blip(this);
				this.Blip.Color = color.Value;
			}
		}

        public void DeleteBlip()
		{
			try {
				if (this != null && this.Exists()) {
					if (this.Blip != null) {
						if (this.Blip.Exists()) {
							this.Blip.Delete();
						}
					}
				}
			} catch (Exception ex) {
				Logger.LogVerboseDebug("Error deleting Vehicle blip -- " + ex.Message);
			}
		}

		private VehicleColor _Colors;
        public VehicleColor Colors {
			get { return _Colors; }
		}

        public EPaint PrimaryColorEnum {
			get { return _Colors.PrimaryColor; }
		}

        public string PrimaryColorName {
			get { return _Colors.PrimaryColorName; }
		}

        public EPaint SecondaryColorEnum {
			get { return _Colors.SecondaryColor; }
		}

        public string SecondaryColorName {
			get { return _Colors.SecondaryColorName; }
		}

        public Blip Blip { get; set; }
        public string Name { get; set; }

	}

}