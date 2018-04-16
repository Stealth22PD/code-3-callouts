Imports LSPD_First_Response.Engine
Imports Rage
Imports Stealth.Common.Extensions.ListExtensions

Namespace Models.Interiors

    Public Module InteriorDatabase
        Public Enum InteriorType
            Null
            FranklinDavis
            FranklinVinewood
            HighEndApt
            MediumApt
            LowApt
            BeachApt
            Mansion
            Trailer
        End Enum

#Region "Interiors"
        Dim mFranklinDavisHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(138.001877, -12.48652, -1435.11987, 31.1015415), New SpawnPoint(34.9052658, -16.5492859, -1434.896, 31.1018143), New SpawnPoint(342.281464, -19.194952, -1434.945, 31.1015415), New SpawnPoint(251.543335, -18.3190479, -1436.31287, 31.1015453), New SpawnPoint(4.039397, -16.9346142, -1442.224, 31.1015472), New SpawnPoint(2.97746944, -10.7433681, -1442.078, 31.1015472)})
        Public FranklinDavisInterior As New Interior(InteriorType.Null, New SpawnPoint(346.55542, -14.2325087, -1440.61816, 31.1015358), mFranklinDavisHidingPlaces)

        Dim mFranklinVinewoodHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(219.24826, 1.85578763, 530.829, 174.628128), New SpawnPoint(329.294617, -2.06409645, 526.6569, 174.6274), New SpawnPoint(176.3604, 1.54652059, 536.543, 175.342422), New SpawnPoint(183.81459, -9.09865, 529.8819, 174.999741), New SpawnPoint(64.59452, -9.073013, 516.714966, 174.628128), New SpawnPoint(27.3926754, 11.0909615, 535.9227, 170.61734), New SpawnPoint(186.818054, -4.34661865, 530.5489, 170.617111), New SpawnPoint(252.272552, 6.19103336, 530.0777, 170.617218), New SpawnPoint(22.9368057, 8.738861, 525.194336, 170.617218), New SpawnPoint(171.413376, 3.53543353, 533.035339, 170.617218), New SpawnPoint(28.9142075, 1.93503189, 514.4129, 168.362961)})
        Public FranklinVinewoodInterior As New Interior(InteriorType.FranklinVinewood, New SpawnPoint(158.609177, 7.34170628, 539.0925, 176.028168), mFranklinVinewoodHidingPlaces)

        Dim mHighEndAptHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(248.121918, -27.8785686, -588.499756, 90.12349), New SpawnPoint(166.364014, -41.4112, -581.7259, 88.71225), New SpawnPoint(339.1511, -30.6411037, -587.8548, 88.71225), New SpawnPoint(358.666046, -32.36888, -587.74585, 83.95474), New SpawnPoint(180.446442, -35.0193443, -582.269653, 83.90752), New SpawnPoint(124.476982, -29.7174854, -574.992249, 83.90752)})
        Public HighEndAptInterior As New Interior(InteriorType.HighEndApt, New SpawnPoint(14.7586613, -17.2118511, -583.5977, 90.11482), mHighEndAptHidingPlaces)

        Dim mMediumAptHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(174.479263, 349.481476, -995.0915, -99.19619), New SpawnPoint(338.978455, 346.862549, -995.4851, -99.1119), New SpawnPoint(28.72055, 343.780945, -1002.42322, -99.19618), New SpawnPoint(181.082413, 341.091, -1002.16339, -99.19618), New SpawnPoint(249.983719, 338.898468, -993.1397, -99.19621)})
        Public MediumAptInterior As New Interior(InteriorType.MediumApt, New SpawnPoint(20.72276, 348.134827, -1006.76941, -99.1962), mMediumAptHidingPlaces)

        Dim mLowAptHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(347.0228, 264.4592, -1003.53973, -99.0085754), New SpawnPoint(92.68601, 266.63208, -1000.64227, -99.00723), New SpawnPoint(168.358337, 256.208771, -998.2693, -99.00859), New SpawnPoint(182.282333, 254.139877, -1001.00946, -98.92755), New SpawnPoint(332.377441, 256.316071, -1001.46021, -99.00989), New SpawnPoint(76.91789, 262.881226, -1002.58545, -99.00863)})
        Public LowAptInterior As New Interior(InteriorType.LowApt, New SpawnPoint(6.29989243, 266.471375, -1007.0708, -100.954315), mLowAptHidingPlaces)

        Dim mBeachAptHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(10.1507082, -1146.27844, -1518.2843, 11.2665434), New SpawnPoint(197.003342, -1150.69788, -1513.29736, 10.6327238), New SpawnPoint(89.18252, -1147.61084, -1518.2345, 11.2566767), New SpawnPoint(67.76632, -1152.70557, -1521.93469, 10.6424961), New SpawnPoint(215.032761, -1155.65942, -1523.03918, 10.6327267), New SpawnPoint(32.22273, -1156.87134, -1521.32617, 10.6327257)})
        Public BeachAptInterior As New Interior(InteriorType.BeachApt, New SpawnPoint(34.983593, -1150.42053, -1521.00916, 10.6327257), mBeachAptHidingPlaces)

        Dim mMansionHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(290.318024, -802.9741, 169.57, 72.84468), New SpawnPoint(270.486572, -806.13324, 176.927231, 72.83473), New SpawnPoint(202.681366, -799.5398, 181.63414, 72.83474), New SpawnPoint(319.561859, -803.941162, 185.433533, 72.6055), New SpawnPoint(22.9908848, -797.446045, 177.303635, 72.8346939), New SpawnPoint(289.63562, -810.2528, 185.37265, 72.47695), New SpawnPoint(113.616432, -801.538757, 180.212128, 76.74074), New SpawnPoint(178.186539, -809.6971, 177.243, 76.74076), New SpawnPoint(255.050659, -809.2597, 167.964874, 76.74076), New SpawnPoint(119.015236, -805.681763, 168.770157, 76.745285), New SpawnPoint(275.0577, -809.7103, 172.858719, 76.74034), New SpawnPoint(44.5935631, -803.06134, 169.690018, 76.74047), New SpawnPoint(129.176315, -800.2695, 174.836411, 76.74332), New SpawnPoint(352.06485, -798.7609, 170.97522, 76.98913), New SpawnPoint(22.5033836, -800.2174, 170.3043, 76.7453842), New SpawnPoint(149.381851, -809.439, 179.75885, 77.17688), New SpawnPoint(200.631256, -815.502441, 180.501633, 76.74539), New SpawnPoint(146.873245, -814.549, 174.928421, 77.17677), New SpawnPoint(128.137939, -811.515137, 175.489838, 76.74539), New SpawnPoint(34.800087, -800.5564, 176.043, 73.0022), New SpawnPoint(108.352608, -796.1358, 183.382431, 72.8364258)})
        Public MansionInterior As New Interior(InteriorType.Mansion, New SpawnPoint(293.1361, -815.688049, 178.250946, 72.1531143), mMansionHidingPlaces)

        Dim mTrailerHidingPlaces As New List(Of SpawnPoint)(New SpawnPoint() {New SpawnPoint(35.1557579, 1978.55518, 3819.22852, 33.4287224), New SpawnPoint(327.24823, 1969.84448, 3818.20215, 33.49355), New SpawnPoint(66.83172, 1969.60071, 3814.10913, 33.42872), New SpawnPoint(202.592545, 1969.10437, 3817.83887, 33.5231438)})
        Public TrailerInterior As New Interior(InteriorType.Trailer, New SpawnPoint(25.63886, 1972.93237, 3816.08032, 33.4286957), mTrailerHidingPlaces)
#End Region

#Region "Houses"

        Public BlankList As New List(Of Residence)(New Residence() {})

        Public FranklinDavisHomes As New List(Of Residence)(New Residence() {New Residence(FranklinDavisInterior, New Vector3(-14.2404652, -1442.08862, 31.1009922)), New Residence(FranklinDavisInterior, New Vector3(16.1402378, -1444.59485, 30.2416763)), New Residence(FranklinDavisInterior, New Vector3(151.742233, -1823.0376, 27.1729145)), New Residence(FranklinDavisInterior, New Vector3(105.032425, -1883.87683, 23.5767632)), New Residence(FranklinDavisInterior, New Vector3(191.417984, -1884.22729, 24.3091831)), New Residence(FranklinDavisInterior, New Vector3(179.263626, -1925.81592, 20.88603)), New Residence(FranklinDavisInterior, New Vector3(149.055038, -1961.283, 19.3368244))})
        Public FranklinVinewoodHome As New Residence(FranklinVinewoodInterior, New Vector3(8.64219, 540.748169, 176.02742))
        Public MansionHomes As New List(Of Residence)(New Residence() {New Residence(MansionInterior, New Vector3(-896.0268, -4.626393, 43.7989159)), New Residence(MansionInterior, New Vector3(-886.0716, 41.516674, 48.2712021)), New Residence(MansionInterior, New Vector3(-928.8969, 17.910387, 47.309227)), New Residence(MansionInterior, New Vector3(-969.8663, 123.712334, 56.4165459)), New Residence(MansionInterior, New Vector3(-997.532166, 156.484528, 61.62756)), New Residence(MansionInterior, New Vector3(-950.4857, 193.823669, 66.90572)), New Residence(MansionInterior, New Vector3(-904.9712, 190.794876, 68.9588)), New Residence(MansionInterior, New Vector3(-1039.608, 221.771851, 63.890213)), New Residence(MansionInterior, New Vector3(-1039.33228, 313.585663, 66.57543)), New Residence(MansionInterior, New Vector3(-1135.28857, 376.451172, 70.81383)), New Residence(MansionInterior, New Vector3(-1025.38831, 360.4215, 70.87604)), New Residence(MansionInterior, New Vector3(-966.223938, 436.5653, 79.640564)), New Residence(MansionInterior, New Vector3(-967.9861, 508.730225, 81.1842041)), New Residence(MansionInterior, New Vector3(-988.0282, 487.890533, 81.78128)), New Residence(MansionInterior, New Vector3(-997.441467, 518.2446, 83.66925)), New Residence(MansionInterior, New Vector3(-1040.72083, 506.3708, 83.8927155)), New Residence(MansionInterior, New Vector3(-1062.1886, 475.215759, 80.8278351)), New Residence(MansionInterior, New Vector3(-1122.505, 485.523834, 81.76508)), New Residence(MansionInterior, New Vector3(-1215.95667, 459.9835, 91.3674)), New Residence(MansionInterior, New Vector3(-1276.8833, 497.015381, 97.4066849)), New Residence(MansionInterior, New Vector3(-1291.52307, 648.9525, 141.020889)), New Residence(MansionInterior, New Vector3(-1165.52332, 727.918335, 155.116074)), New Residence(MansionInterior, New Vector3(-1100.64465, 797.102, 166.66922)), New Residence(MansionInterior, New Vector3(-867.3231, 786.4017, 191.449936)), New Residence(MansionInterior, New Vector3(-658.873352, 887.6366, 228.764709)), New Residence(MansionInterior, New Vector3(-477.378174, 648.4198, 144.3867)), New Residence(MansionInterior, New Vector3(-308.079742, 642.8398, 175.65332)), New Residence(MansionInterior, New Vector3(-245.883118, 621.182434, 187.328018)), New Residence(MansionInterior, New Vector3(-126.810745, 588.9814, 204.122818)), New Residence(MansionInterior, New Vector3(-114.294708, 985.7382, 235.2677)), New Residence(MansionInterior, New Vector3(-86.41081, 834.86554, 235.439377)), New Residence(MansionInterior, New Vector3(-718.8368, 449.558258, 106.427635)), New Residence(MansionInterior, New Vector3(-538.0757, 478.432678, 102.680229))})

        Public CountrySideLowApts As New List(Of Residence)(New Residence() {New Residence(LowAptInterior, New Vector3(1435.47461, 3656.96533, 34.3992)), New Residence(LowAptInterior, New Vector3(1843.46606, 3778.111, 33.5896)), New Residence(LowAptInterior, New Vector3(194.82457, 3030.87, 44.0196838)), New Residence(LowAptInterior, New Vector3(-35.52826, 2871.53857, 59.60971)), New Residence(LowAptInterior, New Vector3(392.422516, 2633.99976, 44.6720963)), New Residence(LowAptInterior, New Vector3(1586.33447, 2906.89844, 57.9703)), New Residence(LowAptInterior, New Vector3(1725.42871, 4642.52051, 43.9143143)), New Residence(LowAptInterior, New Vector3(2434.14233, 5011.93262, 46.831192)), New Residence(LowAptInterior, New Vector3(-356.521179, 6207.25146, 31.8465824)), New Residence(LowAptInterior, New Vector3(11.619132, 6578.411, 33.07021)), New Residence(LowAptInterior, New Vector3(-229.7221, 6445.53125, 31.19743)), New Residence(LowAptInterior, New Vector3(-447.814819, 6260.125, 30.0478611)), New Residence(LowAptInterior, New Vector3(-374.346252, 6191.24463, 31.729475)), New Residence(LowAptInterior, New Vector3(1510.1814, 6325.636, 24.6071)), New Residence(LowAptInterior, New Vector3(2232.06079, 5611.546, 54.91447)), New Residence(LowAptInterior, New Vector3(3688.05151, 4563.132, 25.183075)), New Residence(LowAptInterior, New Vector3(-3050.1582, 475.0962, 6.779648)), New Residence(LowAptInterior, New Vector3(-3108.9707, 304.0891, 8.381037)), New Residence(LowAptInterior, New Vector3(-3101.86963, 743.868958, 21.2848415)), New Residence(LowAptInterior, New Vector3(-3228.69849, 1092.5686, 10.7726889)), New Residence(LowAptInterior, New Vector3(-3187.37915, 1273.48645, 12.6712236)), New Residence(LowAptInterior, New Vector3(-263.9402, 2196.685, 130.398758))})
        Public CountrySideTrailers As New List(Of Residence)(New Residence() {New Residence(TrailerInterior, New Vector3(1932.91, 3804.91333, 32.9133949)), New Residence(TrailerInterior, New Vector3(1915.65564, 3909.14746, 33.4415932)), New Residence(TrailerInterior, New Vector3(1748.677, 3783.79077, 34.8348656)), New Residence(TrailerInterior, New Vector3(1436.22351, 3639.092, 34.94693)), New Residence(TrailerInterior, New Vector3(14.9594641, 3688.74316, 40.2140236)), New Residence(TrailerInterior, New Vector3(78.12931, 3732.41919, 40.27184)), New Residence(TrailerInterior, New Vector3(404.297577, 2584.686, 43.5195274)), New Residence(TrailerInterior, New Vector3(564.6371, 2598.58154, 43.8774452)), New Residence(TrailerInterior, New Vector3(1779.28467, 3640.77637, 34.5047073)), New Residence(TrailerInterior, New Vector3(1642.959, 3726.969, 35.0671463)), New Residence(TrailerInterior, New Vector3(1691.95056, 3866.01367, 34.907505)), New Residence(TrailerInterior, New Vector3(2167.8562, 3330.7146, 46.51468)), New Residence(TrailerInterior, New Vector3(-23.58868, 3036.23486, 41.6740265)), New Residence(TrailerInterior, New Vector3(-453.1538, 6336.834, 13.1130323)), New Residence(TrailerInterior, New Vector3(1381.60474, 4381.95361, 45.18859)), New Residence(TrailerInterior, New Vector3(1662.09912, 4775.96045, 42.0771027)), New Residence(TrailerInterior, New Vector3(858.8164, 2877.46265, 57.9828377))})

        Public ElBurroLowApts As New List(Of Residence)(New Residence() {New Residence(LowAptInterior, New Vector3(1294.99231, -1739.78918, 54.27178)), New Residence(LowAptInterior, New Vector3(1354.9187, -1690.47668, 60.4912338)), New Residence(LowAptInterior, New Vector3(1193.999, -1656.25061, 43.02641)), New Residence(LowAptInterior, New Vector3(1230.72144, -1590.78113, 53.7664032)), New Residence(LowAptInterior, New Vector3(1390.90381, -1508.41907, 58.4358)), New Residence(LowAptInterior, New Vector3(1327.56873, -1553.17883, 54.051548))})

        Public HighEndAptHomes As New List(Of Residence)(New Residence() {New Residence(HighEndAptInterior, New Vector3(-47.4945, -585.6966, 37.9532433)), New Residence(HighEndAptInterior, New Vector3(-243.471741, -811.8866, 30.0672)), New Residence(HighEndAptInterior, New Vector3(-297.095856, -829.958069, 31.7607975)), New Residence(HighEndAptInterior, New Vector3(-248.5903, -954.769653, 30.5720463)), New Residence(HighEndAptInterior, New Vector3(267.28772, -641.4206, 41.37113)), New Residence(HighEndAptInterior, New Vector3(-213.7946, -727.5539, 33.56074)), New Residence(HighEndAptInterior, New Vector3(-589.469849, -707.2298, 36.27952)), New Residence(HighEndAptInterior, New Vector3(-916.5324, -449.67572, 39.59985)), New Residence(HighEndAptInterior, New Vector3(-937.3823, -379.789429, 38.30904)), New Residence(HighEndAptInterior, New Vector3(-933.1073, -213.453247, 37.5639572)), New Residence(HighEndAptInterior, New Vector3(-595.3701, 36.3861465, 42.95644)), New Residence(HighEndAptInterior, New Vector3(-676.61554, 311.5035, 82.43256)), New Residence(HighEndAptInterior, New Vector3(-773.3317, 309.860077, 85.0467148)), New Residence(HighEndAptInterior, New Vector3(-743.0462, 245.767532, 76.68338)), New Residence(HighEndAptInterior, New Vector3(145.532532, -830.668945, 30.4993725)), New Residence(HighEndAptInterior, New Vector3(105.573547, -933.4377, 29.14731)), New Residence(HighEndAptInterior, New Vector3(-885.8701, -1231.64392, 5.65591049)), New Residence(HighEndAptInterior, New Vector3(-1442.28259, -545.52594, 34.74182))})

        Public MirrorParkHomes As New List(Of Residence)(New Residence() {New Residence(MediumAptInterior, New Vector3(1046.52283, -498.0525, 64.07932)), New Residence(MediumAptInterior, New Vector3(1099.7865, -438.68927, 67.7905655)), New Residence(MediumAptInterior, New Vector3(987.432, -433.026367, 64.04353)), New Residence(MediumAptInterior, New Vector3(902.7793, -615.806458, 58.4533043)), New Residence(MediumAptInterior, New Vector3(1229.53748, -724.8105, 60.95649)), New Residence(MediumAptInterior, New Vector3(1250.439, -621.1722, 69.5560455)), New Residence(MediumAptInterior, New Vector3(1341.28674, -597.400146, 74.70091))})
        Public MediumAptHomes1 As New List(Of Residence)(New Residence() {New Residence(MediumAptInterior, New Vector3(388.584045, -75.2914, 68.1805))})

#End Region

        Public Function GetRandomHouse() As Residence
            Dim houseList As New List(Of Residence)

            houseList.Add(FranklinVinewoodHome)
            houseList.AddRange(FranklinDavisHomes)
            houseList.AddRange(MansionHomes)
            houseList.AddRange(CountrySideLowApts)
            houseList.AddRange(CountrySideTrailers)
            houseList.AddRange(ElBurroLowApts)
            houseList.AddRange(HighEndAptHomes)
            houseList.AddRange(MirrorParkHomes)
            houseList.AddRange(MediumAptHomes1)


            Dim closeHouses As List(Of Residence) = (From x In houseList Where Game.LocalPlayer.Character.DistanceTo(x.EntryPoint) < 1500 AndAlso Game.LocalPlayer.Character.DistanceTo(x.EntryPoint) > 150).ToList()
            houseList = Nothing

            Dim r As Residence = Nothing

            If closeHouses.Count > 0 Then
                r = closeHouses(gRandom.Next(closeHouses.Count))
            End If

            Return r
        End Function

    End Module

End Namespace
