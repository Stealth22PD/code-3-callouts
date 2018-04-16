using Rage;
using Stealth.Common;
using Stealth.Plugins.Code3Callouts.Util;
using System;
using System.Collections.Generic;

namespace Stealth.Plugins.Code3Callouts.Models.Peds
{

    internal static class PedHelper
	{

		internal static Vector3 GetPedSpawnPoint(Vector3 pReferencePoint)
		{
			Vector3 resultV3 = GetSafeCoordinatesForPed(pReferencePoint);

			if (resultV3 == Vector3.Zero) {
				return pReferencePoint;
			} else {
				return resultV3;
			}
		}

		internal static Vector3 GetSafeCoordinatesForPed(Vector3 pPosition)
		{
			Vector3 resultV3 = Vector3.Zero;

			try {
				//coordinatesFound = Rage.Native.NativeFunction.CallByName(Of Uinteger)("GET_SAFE_COORD_FOR_PED", pPosition.X, pPosition.Y, pPosition.Z, True, resultV3.X, resultV3.Y, resultV3.Z, 0)
				resultV3 = Stealth.Common.Natives.Peds.GetSafeCoordinatesForPed(pPosition);
			} catch (Exception ex) {
				resultV3 = Vector3.Zero;
				Logger.LogVerboseDebug("Error getting safe ped coordinates -- " + ex.Message);
			}

			return resultV3;
		}

		internal static List<string> RandomizeDriverStory(int rint, int pSpeed)
		{
			List<string> list = new List<string>();

			switch (rint) {
				case 0:
					list.Add("The other driver came out of nowhere and hit me.");
                    break;
				case 1:
					list.Add("I was on an important call...I don't know what happened.");
                    break;
				case 2:
                    list.Add("The other driver ran a red light and hit me.");
                    break;
				case 3:
                    list.Add("[Slurred Speech] I uhh...don't know wha' happen.");
                    break;
				case 4:
                    list.Add("The other driver was on their cell phone.");
					list.Add("They were not paying any attention to the road.");
                    break;
				case 5:
                    list.Add("I was in a hurry, and didn't see the other car.");
					list.Add("Can we make this quick? I have to get home.");
                    break;
				case 6:
                    list.Add("I'm not going to say anything without my lawyer.");
                    break;
				case 7:
                    list.Add("That other driver needs to go back to school...");
                    break;
				case 8:
                    list.Add("What's going to happen to my car?");
                    break;
				case 9:
                    list.Add("Oh no...my car...my brand new car!!!");
                    break;
				case 10:
                    list.Add("The other driver was on the wrong side of the road!");
					list.Add("I tried to swerve, but I couldn't avoid them.");
                    break;
				case 11:
                    list.Add("This is my mom's car...she is going to be pissed.");
                    break;
				case 12:
                    list.Add("I was singing along to some tunes when it happened.");
					list.Add("I was on a high note, and my windshield shattered!");
                    break;
				case 13:
                    list.Add("I didn't even get to finish my text message to my BFF!!");
					list.Add("I MEAN...uhh...");
					list.Add("Seriously, what was the other driver thinking?!");
                    break;
				case 14:
                    list.Add("My dad is a cop...he'll take care of all this.");
                    break;
				default:
                    list.Add("I'm so dazed...I don't remember what happened.");
                    break;
            }

            list.Add(string.Format("I was going around {0} MPH.", pSpeed));

			return list;
		}

		internal static List<string> RandomizeImpairedDriverStory(int rint)
		{
			List<string> list = new List<string>();

			list.Add("[Slurred Speech] I uhh...don't know wha' happen.");

			switch (rint) {
				case 0:
					list.Add("Them things were comin outta nowhere and hitting me!");
                    break;
				case 1:
                    list.Add("It was the gremlins, I tell you!");
                    break;
				case 2:
                    list.Add("Can I go home, Occifer?");
                    break;
				case 3:
                    list.Add("Can I go? Got someone waitin at home, if ya know what I mean...");
                    break;
				case 4:
                    list.Add("The car jusss' decided to go batshit crazy on me!!");
                    break;
				case 5:
                    list.Add("Hey, ain't you that guy from the TV?");
                    break;
				case 6:
                    list.Add("I want my lawyer!!");
                    break;
				case 7:
                    list.Add("What's going to happen to my car?");
                    break;
				case 8:
                    list.Add("Ohh shhhiittt...this is ma' mom's car...");
                    break;
				case 9:
                    list.Add("*Hiccup* Hey Occifer, you want a beer?");
                    break;
				case 10:
                    list.Add("I spilled my beer all over the car!!");
                    break;
				case 11:
                    list.Add("I'm so dazed...I don't remember what happened.");
                    break;
				case 12:
                    list.Add("Mind if I call a cab? My car ain't workin so good.");
                    break;
				case 13:
                    list.Add("I...uhhh...had a little accident, Occifer...");
                    break;
				case 14:
                    list.Add("My dad's ya cop, ya know!!");
                    break;
				default:
                    break;
			}

			return list;
		}

	}

}