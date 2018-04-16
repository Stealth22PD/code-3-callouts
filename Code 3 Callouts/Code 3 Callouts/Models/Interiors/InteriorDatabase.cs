using LSPD_First_Response.Engine;
using Rage;
using System.Collections.Generic;
using System.Linq;

namespace Stealth.Plugins.Code3Callouts.Models.Interiors
{

	internal static class InteriorDatabase
	{
		internal enum InteriorType
		{
			Null,
			FranklinDavis,
			FranklinVinewood,
			HighEndApt,
			MediumApt,
			LowApt,
			BeachApt,
			Mansion,
			Trailer
		}

        static List<SpawnPoint> mFranklinDavisHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(138.001877f, -12.48652f, -1435.11987f, 31.1015415f),
			new SpawnPoint(34.9052658f, -16.5492859f, -1434.896f, 31.1018143f),
			new SpawnPoint(342.281464f, -19.194952f, -1434.945f, 31.1015415f),
			new SpawnPoint(251.543335f, -18.3190479f, -1436.31287f, 31.1015453f),
			new SpawnPoint(4.039397f, -16.9346142f, -1442.224f, 31.1015472f),
			new SpawnPoint(2.97746944f, -10.7433681f, -1442.078f, 31.1015472f)
		});

		internal static Interior FranklinDavIsInterior = new Interior(InteriorType.Null, new SpawnPoint(346.55542f, -14.2325087f, -1440.61816f, 31.1015358f), mFranklinDavisHidingPlaces);
		static List<SpawnPoint> mFranklinVinewoodHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(219.24826f, 1.85578763f, 530.829f, 174.628128f),
			new SpawnPoint(329.294617f, -2.06409645f, 526.6569f, 174.6274f),
			new SpawnPoint(176.3604f, 1.54652059f, 536.543f, 175.342422f),
			new SpawnPoint(183.81459f, -9.09865f, 529.8819f, 174.999741f),
			new SpawnPoint(64.59452f, -9.073013f, 516.714966f, 174.628128f),
			new SpawnPoint(27.3926754f, 11.0909615f, 535.9227f, 170.61734f),
			new SpawnPoint(186.818054f, -4.34661865f, 530.5489f, 170.617111f),
			new SpawnPoint(252.272552f, 6.19103336f, 530.0777f, 170.617218f),
			new SpawnPoint(22.9368057f, 8.738861f, 525.194336f, 170.617218f),
			new SpawnPoint(171.413376f, 3.53543353f, 533.035339f, 170.617218f),
			new SpawnPoint(28.9142075f, 1.93503189f, 514.4129f, 168.362961f)
		});

		internal static Interior FranklinVinewoodInterior = new Interior(InteriorType.FranklinVinewood, new SpawnPoint(158.609177f, 7.34170628f, 539.0925f, 176.028168f), mFranklinVinewoodHidingPlaces);
        static List<SpawnPoint> mHighEndAptHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(248.121918f, -27.8785686f, -588.499756f, 90.12349f),
			new SpawnPoint(166.364014f, -41.4112f, -581.7259f, 88.71225f),
			new SpawnPoint(339.1511f, -30.6411037f, -587.8548f, 88.71225f),
			new SpawnPoint(358.666046f, -32.36888f, -587.74585f, 83.95474f),
			new SpawnPoint(180.446442f, -35.0193443f, -582.269653f, 83.90752f),
			new SpawnPoint(124.476982f, -29.7174854f, -574.992249f, 83.90752f)
		});

		internal static Interior HighEndAptInterior = new Interior(InteriorType.HighEndApt, new SpawnPoint(14.7586613f, -17.2118511f, -583.5977f, 90.11482f), mHighEndAptHidingPlaces);
        static List<SpawnPoint> mMediumAptHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(174.479263f, 349.481476f, -995.0915f, -99.19619f),
			new SpawnPoint(338.978455f, 346.862549f, -995.4851f, -99.1119f),
			new SpawnPoint(28.72055f, 343.780945f, -1002.42322f, -99.19618f),
			new SpawnPoint(181.082413f, 341.091f, -1002.16339f, -99.19618f),
			new SpawnPoint(249.983719f, 338.898468f, -993.1397f, -99.19621f)
		});

		internal static Interior MediumAptInterior = new Interior(InteriorType.MediumApt, new SpawnPoint(20.72276f, 348.134827f, -1006.76941f, -99.1962f), mMediumAptHidingPlaces);
        static List<SpawnPoint> mLowAptHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(347.0228f, 264.4592f, -1003.53973f, -99.0085754f),
			new SpawnPoint(92.68601f, 266.63208f, -1000.64227f, -99.00723f),
			new SpawnPoint(168.358337f, 256.208771f, -998.2693f, -99.00859f),
			new SpawnPoint(182.282333f, 254.139877f, -1001.00946f, -98.92755f),
			new SpawnPoint(332.377441f, 256.316071f, -1001.46021f, -99.00989f),
			new SpawnPoint(76.91789f, 262.881226f, -1002.58545f, -99.00863f)
		});

		internal static Interior LowAptInterior = new Interior(InteriorType.LowApt, new SpawnPoint(6.29989243f, 266.471375f, -1007.0708f, -100.954315f), mLowAptHidingPlaces);
        static List<SpawnPoint> mBeachAptHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(10.1507082f, -1146.27844f, -1518.2843f, 11.2665434f),
			new SpawnPoint(197.003342f, -1150.69788f, -1513.29736f, 10.6327238f),
			new SpawnPoint(89.18252f, -1147.61084f, -1518.2345f, 11.2566767f),
			new SpawnPoint(67.76632f, -1152.70557f, -1521.93469f, 10.6424961f),
			new SpawnPoint(215.032761f, -1155.65942f, -1523.03918f, 10.6327267f),
			new SpawnPoint(32.22273f, -1156.87134f, -1521.32617f, 10.6327257f)
		});

		internal static Interior BeachAptInterior = new Interior(InteriorType.BeachApt, new SpawnPoint(34.983593f, -1150.42053f, -1521.00916f, 10.6327257f), mBeachAptHidingPlaces);
        static List<SpawnPoint> mMansionHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(290.318024f, -802.9741f, 169.57f, 72.84468f),
			new SpawnPoint(270.486572f, -806.13324f, 176.927231f, 72.83473f),
			new SpawnPoint(202.681366f, -799.5398f, 181.63414f, 72.83474f),
			new SpawnPoint(319.561859f, -803.941162f, 185.433533f, 72.6055f),
			new SpawnPoint(22.9908848f, -797.446045f, 177.303635f, 72.8346939f),
			new SpawnPoint(289.63562f, -810.2528f, 185.37265f, 72.47695f),
			new SpawnPoint(113.616432f, -801.538757f, 180.212128f, 76.74074f),
			new SpawnPoint(178.186539f, -809.6971f, 177.243f, 76.74076f),
			new SpawnPoint(255.050659f, -809.2597f, 167.964874f, 76.74076f),
			new SpawnPoint(119.015236f, -805.681763f, 168.770157f, 76.745285f),
			new SpawnPoint(275.0577f, -809.7103f, 172.858719f, 76.74034f),
			new SpawnPoint(44.5935631f, -803.06134f, 169.690018f, 76.74047f),
			new SpawnPoint(129.176315f, -800.2695f, 174.836411f, 76.74332f),
			new SpawnPoint(352.06485f, -798.7609f, 170.97522f, 76.98913f),
			new SpawnPoint(22.5033836f, -800.2174f, 170.3043f, 76.7453842f),
			new SpawnPoint(149.381851f, -809.439f, 179.75885f, 77.17688f),
			new SpawnPoint(200.631256f, -815.502441f, 180.501633f, 76.74539f),
			new SpawnPoint(146.873245f, -814.549f, 174.928421f, 77.17677f),
			new SpawnPoint(128.137939f, -811.515137f, 175.489838f, 76.74539f),
			new SpawnPoint(34.800087f, -800.5564f, 176.043f, 73.0022f),
			new SpawnPoint(108.352608f, -796.1358f, 183.382431f, 72.8364258f)
		});

		internal static Interior MansionInterior = new Interior(InteriorType.Mansion, new SpawnPoint(293.1361f, -815.688049f, 178.250946f, 72.1531143f), mMansionHidingPlaces);
        static List<SpawnPoint> mTrailerHidingPlaces = new List<SpawnPoint>(new SpawnPoint[] {
			new SpawnPoint(35.1557579f, 1978.55518f, 3819.22852f, 33.4287224f),
			new SpawnPoint(327.24823f, 1969.84448f, 3818.20215f, 33.49355f),
			new SpawnPoint(66.83172f, 1969.60071f, 3814.10913f, 33.42872f),
			new SpawnPoint(202.592545f, 1969.10437f, 3817.83887f, 33.5231438f)
		});
		internal static Interior TrailerInterior = new Interior(InteriorType.Trailer, new SpawnPoint(25.63886f, 1972.93237f, 3816.08032f, 33.4286957f), mTrailerHidingPlaces);

		internal static List<Residence> BlankList = new List<Residence>(new Residence[] {
			

		});
		internal static List<Residence> FranklinDavisHomes = new List<Residence>(new Residence[] {
			new Residence(FranklinDavIsInterior, new Vector3(-14.2404652f, -1442.08862f, 31.1009922f)),
			new Residence(FranklinDavIsInterior, new Vector3(16.1402378f, -1444.59485f, 30.2416763f)),
			new Residence(FranklinDavIsInterior, new Vector3(151.742233f, -1823.0376f, 27.1729145f)),
			new Residence(FranklinDavIsInterior, new Vector3(105.032425f, -1883.87683f, 23.5767632f)),
			new Residence(FranklinDavIsInterior, new Vector3(191.417984f, -1884.22729f, 24.3091831f)),
			new Residence(FranklinDavIsInterior, new Vector3(179.263626f, -1925.81592f, 20.88603f)),
			new Residence(FranklinDavIsInterior, new Vector3(149.055038f, -1961.283f, 19.3368244f))
		});
		internal static Residence FranklinVinewoodHome = new Residence(FranklinVinewoodInterior, new Vector3(8.64219f, 540.748169f, 176.02742f));
		internal static List<Residence> MansionHomes = new List<Residence>(new Residence[] {
			new Residence(MansionInterior, new Vector3(-896.0268f, -4.626393f, 43.7989159f)),
			new Residence(MansionInterior, new Vector3(-886.0716f, 41.516674f, 48.2712021f)),
			new Residence(MansionInterior, new Vector3(-928.8969f, 17.910387f, 47.309227f)),
			new Residence(MansionInterior, new Vector3(-969.8663f, 123.712334f, 56.4165459f)),
			new Residence(MansionInterior, new Vector3(-997.532166f, 156.484528f, 61.62756f)),
			new Residence(MansionInterior, new Vector3(-950.4857f, 193.823669f, 66.90572f)),
			new Residence(MansionInterior, new Vector3(-904.9712f, 190.794876f, 68.9588f)),
			new Residence(MansionInterior, new Vector3(-1039.608f, 221.771851f, 63.890213f)),
			new Residence(MansionInterior, new Vector3(-1039.33228f, 313.585663f, 66.57543f)),
			new Residence(MansionInterior, new Vector3(-1135.28857f, 376.451172f, 70.81383f)),
			new Residence(MansionInterior, new Vector3(-1025.38831f, 360.4215f, 70.87604f)),
			new Residence(MansionInterior, new Vector3(-966.223938f, 436.5653f, 79.640564f)),
			new Residence(MansionInterior, new Vector3(-967.9861f, 508.730225f, 81.1842041f)),
			new Residence(MansionInterior, new Vector3(-988.0282f, 487.890533f, 81.78128f)),
			new Residence(MansionInterior, new Vector3(-997.441467f, 518.2446f, 83.66925f)),
			new Residence(MansionInterior, new Vector3(-1040.72083f, 506.3708f, 83.8927155f)),
			new Residence(MansionInterior, new Vector3(-1062.1886f, 475.215759f, 80.8278351f)),
			new Residence(MansionInterior, new Vector3(-1122.505f, 485.523834f, 81.76508f)),
			new Residence(MansionInterior, new Vector3(-1215.95667f, 459.9835f, 91.3674f)),
			new Residence(MansionInterior, new Vector3(-1276.8833f, 497.015381f, 97.4066849f)),
			new Residence(MansionInterior, new Vector3(-1291.52307f, 648.9525f, 141.020889f)),
			new Residence(MansionInterior, new Vector3(-1165.52332f, 727.918335f, 155.116074f)),
			new Residence(MansionInterior, new Vector3(-1100.64465f, 797.102f, 166.66922f)),
			new Residence(MansionInterior, new Vector3(-867.3231f, 786.4017f, 191.449936f)),
			new Residence(MansionInterior, new Vector3(-658.873352f, 887.6366f, 228.764709f)),
			new Residence(MansionInterior, new Vector3(-477.378174f, 648.4198f, 144.3867f)),
			new Residence(MansionInterior, new Vector3(-308.079742f, 642.8398f, 175.65332f)),
			new Residence(MansionInterior, new Vector3(-245.883118f, 621.182434f, 187.328018f)),
			new Residence(MansionInterior, new Vector3(-126.810745f, 588.9814f, 204.122818f)),
			new Residence(MansionInterior, new Vector3(-114.294708f, 985.7382f, 235.2677f)),
			new Residence(MansionInterior, new Vector3(-86.41081f, 834.86554f, 235.439377f)),
			new Residence(MansionInterior, new Vector3(-718.8368f, 449.558258f, 106.427635f)),
			new Residence(MansionInterior, new Vector3(-538.0757f, 478.432678f, 102.680229f))

		});
		internal static List<Residence> CountrySideLowApts = new List<Residence>(new Residence[] {
			new Residence(LowAptInterior, new Vector3(1435.47461f, 3656.96533f, 34.3992f)),
			new Residence(LowAptInterior, new Vector3(1843.46606f, 3778.111f, 33.5896f)),
			new Residence(LowAptInterior, new Vector3(194.82457f, 3030.87f, 44.0196838f)),
			new Residence(LowAptInterior, new Vector3(-35.52826f, 2871.53857f, 59.60971f)),
			new Residence(LowAptInterior, new Vector3(392.422516f, 2633.99976f, 44.6720963f)),
			new Residence(LowAptInterior, new Vector3(1586.33447f, 2906.89844f, 57.9703f)),
			new Residence(LowAptInterior, new Vector3(1725.42871f, 4642.52051f, 43.9143143f)),
			new Residence(LowAptInterior, new Vector3(2434.14233f, 5011.93262f, 46.831192f)),
			new Residence(LowAptInterior, new Vector3(-356.521179f, 6207.25146f, 31.8465824f)),
			new Residence(LowAptInterior, new Vector3(11.619132f, 6578.411f, 33.07021f)),
			new Residence(LowAptInterior, new Vector3(-229.7221f, 6445.53125f, 31.19743f)),
			new Residence(LowAptInterior, new Vector3(-447.814819f, 6260.125f, 30.0478611f)),
			new Residence(LowAptInterior, new Vector3(-374.346252f, 6191.24463f, 31.729475f)),
			new Residence(LowAptInterior, new Vector3(1510.1814f, 6325.636f, 24.6071f)),
			new Residence(LowAptInterior, new Vector3(2232.06079f, 5611.546f, 54.91447f)),
			new Residence(LowAptInterior, new Vector3(3688.05151f, 4563.132f, 25.183075f)),
			new Residence(LowAptInterior, new Vector3(-3050.1582f, 475.0962f, 6.779648f)),
			new Residence(LowAptInterior, new Vector3(-3108.9707f, 304.0891f, 8.381037f)),
			new Residence(LowAptInterior, new Vector3(-3101.86963f, 743.868958f, 21.2848415f)),
			new Residence(LowAptInterior, new Vector3(-3228.69849f, 1092.5686f, 10.7726889f)),
			new Residence(LowAptInterior, new Vector3(-3187.37915f, 1273.48645f, 12.6712236f)),
			new Residence(LowAptInterior, new Vector3(-263.9402f, 2196.685f, 130.398758f))
		});
		internal static List<Residence> CountrySideTrailers = new List<Residence>(new Residence[] {
			new Residence(TrailerInterior, new Vector3(1932.91f, 3804.91333f, 32.9133949f)),
			new Residence(TrailerInterior, new Vector3(1915.65564f, 3909.14746f, 33.4415932f)),
			new Residence(TrailerInterior, new Vector3(1748.677f, 3783.79077f, 34.8348656f)),
			new Residence(TrailerInterior, new Vector3(1436.22351f, 3639.092f, 34.94693f)),
			new Residence(TrailerInterior, new Vector3(14.9594641f, 3688.74316f, 40.2140236f)),
			new Residence(TrailerInterior, new Vector3(78.12931f, 3732.41919f, 40.27184f)),
			new Residence(TrailerInterior, new Vector3(404.297577f, 2584.686f, 43.5195274f)),
			new Residence(TrailerInterior, new Vector3(564.6371f, 2598.58154f, 43.8774452f)),
			new Residence(TrailerInterior, new Vector3(1779.28467f, 3640.77637f, 34.5047073f)),
			new Residence(TrailerInterior, new Vector3(1642.959f, 3726.969f, 35.0671463f)),
			new Residence(TrailerInterior, new Vector3(1691.95056f, 3866.01367f, 34.907505f)),
			new Residence(TrailerInterior, new Vector3(2167.8562f, 3330.7146f, 46.51468f)),
			new Residence(TrailerInterior, new Vector3(-23.58868f, 3036.23486f, 41.6740265f)),
			new Residence(TrailerInterior, new Vector3(-453.1538f, 6336.834f, 13.1130323f)),
			new Residence(TrailerInterior, new Vector3(1381.60474f, 4381.95361f, 45.18859f)),
			new Residence(TrailerInterior, new Vector3(1662.09912f, 4775.96045f, 42.0771027f)),
			new Residence(TrailerInterior, new Vector3(858.8164f, 2877.46265f, 57.9828377f))

		});
		internal static List<Residence> ElBurroLowApts = new List<Residence>(new Residence[] {
			new Residence(LowAptInterior, new Vector3(1294.99231f, -1739.78918f, 54.27178f)),
			new Residence(LowAptInterior, new Vector3(1354.9187f, -1690.47668f, 60.4912338f)),
			new Residence(LowAptInterior, new Vector3(1193.999f, -1656.25061f, 43.02641f)),
			new Residence(LowAptInterior, new Vector3(1230.72144f, -1590.78113f, 53.7664032f)),
			new Residence(LowAptInterior, new Vector3(1390.90381f, -1508.41907f, 58.4358f)),
			new Residence(LowAptInterior, new Vector3(1327.56873f, -1553.17883f, 54.051548f))

		});
		internal static List<Residence> HighEndAptHomes = new List<Residence>(new Residence[] {
			new Residence(HighEndAptInterior, new Vector3(-47.4945f, -585.6966f, 37.9532433f)),
			new Residence(HighEndAptInterior, new Vector3(-243.471741f, -811.8866f, 30.0672f)),
			new Residence(HighEndAptInterior, new Vector3(-297.095856f, -829.958069f, 31.7607975f)),
			new Residence(HighEndAptInterior, new Vector3(-248.5903f, -954.769653f, 30.5720463f)),
			new Residence(HighEndAptInterior, new Vector3(267.28772f, -641.4206f, 41.37113f)),
			new Residence(HighEndAptInterior, new Vector3(-213.7946f, -727.5539f, 33.56074f)),
			new Residence(HighEndAptInterior, new Vector3(-589.469849f, -707.2298f, 36.27952f)),
			new Residence(HighEndAptInterior, new Vector3(-916.5324f, -449.67572f, 39.59985f)),
			new Residence(HighEndAptInterior, new Vector3(-937.3823f, -379.789429f, 38.30904f)),
			new Residence(HighEndAptInterior, new Vector3(-933.1073f, -213.453247f, 37.5639572f)),
			new Residence(HighEndAptInterior, new Vector3(-595.3701f, 36.3861465f, 42.95644f)),
			new Residence(HighEndAptInterior, new Vector3(-676.61554f, 311.5035f, 82.43256f)),
			new Residence(HighEndAptInterior, new Vector3(-773.3317f, 309.860077f, 85.0467148f)),
			new Residence(HighEndAptInterior, new Vector3(-743.0462f, 245.767532f, 76.68338f)),
			new Residence(HighEndAptInterior, new Vector3(145.532532f, -830.668945f, 30.4993725f)),
			new Residence(HighEndAptInterior, new Vector3(105.573547f, -933.4377f, 29.14731f)),
			new Residence(HighEndAptInterior, new Vector3(-885.8701f, -1231.64392f, 5.65591049f)),
			new Residence(HighEndAptInterior, new Vector3(-1442.28259f, -545.52594f, 34.74182f))

		});
		internal static List<Residence> MirrorParkHomes = new List<Residence>(new Residence[] {
			new Residence(MediumAptInterior, new Vector3(1046.52283f, -498.0525f, 64.07932f)),
			new Residence(MediumAptInterior, new Vector3(1099.7865f, -438.68927f, 67.7905655f)),
			new Residence(MediumAptInterior, new Vector3(987.432f, -433.026367f, 64.04353f)),
			new Residence(MediumAptInterior, new Vector3(902.7793f, -615.806458f, 58.4533043f)),
			new Residence(MediumAptInterior, new Vector3(1229.53748f, -724.8105f, 60.95649f)),
			new Residence(MediumAptInterior, new Vector3(1250.439f, -621.1722f, 69.5560455f)),
			new Residence(MediumAptInterior, new Vector3(1341.28674f, -597.400146f, 74.70091f))
		});

		internal static List<Residence> MediumAptHomes = new List<Residence>(new Residence[] { new Residence(MediumAptInterior, new Vector3(388.584045f, -75.2914f, 68.1805f)) });

		internal static Residence GetRandomHouse()
		{
			List<Residence> houseList = new List<Residence>();

			houseList.Add(FranklinVinewoodHome);
			houseList.AddRange(FranklinDavisHomes);
			houseList.AddRange(MansionHomes);
			houseList.AddRange(CountrySideLowApts);
			houseList.AddRange(CountrySideTrailers);
			houseList.AddRange(ElBurroLowApts);
			houseList.AddRange(HighEndAptHomes);
			houseList.AddRange(MirrorParkHomes);
			houseList.AddRange(MediumAptHomes);


			List<Residence> closeHouses = (from x in houseList where Game.LocalPlayer.Character.DistanceTo(x.EntryPoint) < 1500 && Game.LocalPlayer.Character.DistanceTo(x.EntryPoint) > 150 select x).ToList();
			houseList = null;

			Residence r = null;

			if (closeHouses.Count > 0f) {
				r = closeHouses[Common.gRandom.Next(closeHouses.Count)];
			}

			return r;
		}

	}

}