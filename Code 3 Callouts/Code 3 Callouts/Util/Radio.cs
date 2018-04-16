using Rage;
using System;
using System.Collections.Generic;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using static LSPD_First_Response.Mod.API.Functions;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioPlayerEngine;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Util
{

	internal static class Radio
	{

		internal static void AcknowledgeCallout(string pCalloutName, CallResponseType pResponse)
		{
			Radio.PlayRadioAnimation();
			List<AudioFile> pAudio = new List<AudioFile>();
			Game.DisplayNotification(string.Format("~b~{0}~w~: ~w~Dispatch, {0} responding.", Common.gUnitNumber, pCalloutName));

			int responseint = Common.gRandom.Next(3);
			if (responseint == 1) {
				pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_EN_ROUTE));
			} else if (responseint == 2) {
				pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_ON_OUR_WAY));
			} else {
				pAudio.Add(new AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY));
			}

			pAudio.AddRange(UnitAudio);

			int rogerint = Common.gRandom.Next(2);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			}

			if (pResponse == CallResponseType.Code_3) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_3));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_2));
			}

			AudioPlayerEngine.PlayAudio(pAudio);

			DispatchMessage(string.Format("Roger. Respond ~g~{0}", ResponseString(pResponse)), true);
		}

		internal static void UnitIsOnScene()
		{
			Radio.PlayRadioAnimation();
			Game.DisplayNotification(string.Format("~b~{0}: ~w~{0} is on scene, Code 6 at location.", Common.gUnitNumber));
			DispatchMessage("Roger.", true);

			List<AudioFile> pAudio = new List<AudioFile>();
			pAudio.Add(new AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.HAVE_A_VISUAL));

			pAudio.AddRange(UnitAudio);

			int rogerint = Common.gRandom.Next(4);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else if (rogerint == 1) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
			} else if (rogerint == 2) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
			}

			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void DispatchAcknowledge()
		{
			List<AudioFile> pAudio = new List<AudioFile>();

			int rogerint = Common.gRandom.Next(4);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else if (rogerint == 1) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
			} else if (rogerint == 2) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
			}

			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void DispatchAcknowledgePlayer()
		{
			DispatchMessage("Roger", true);

			List<AudioFile> pAudio = new List<AudioFile>();
			pAudio.AddRange(UnitAudio);

			int rogerint = Common.gRandom.Next(4);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else if (rogerint == 1) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
			} else if (rogerint == 2) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
			}

			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void AIOfficerResponding()
		{
			List<AudioFile> pAudio = new List<AudioFile>();

			OFFICER.AI_UNIT_RESPONDING[] AIResponseValues = (OFFICER.AI_UNIT_RESPONDING[])Enum.GetValues(typeof(OFFICER.AI_UNIT_RESPONDING));
			OFFICER.AI_UNIT_RESPONDING AIResponse = AIResponseValues[Common.gRandom.Next(AIResponseValues.Length)];
			pAudio.Add(new AudioFile("OFFICER", AIResponse));

			int rogerint = Common.gRandom.Next(4);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else if (rogerint == 1) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
			} else if (rogerint == 2) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
			}

			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void CallIsCode4(string pCalloutName, bool pSuspectIsInCustody = false)
		{
			GameFiber.StartNew(() =>
			{
				Radio.PlayRadioAnimation();
				GameFiber.Sleep(3000);

				Game.DisplayNotification(string.Format("~b~{0}~w~: ~w~{0} to Dispatch, ~r~{1} ~w~call is Code 4.", Common.gUnitNumber, pCalloutName));
				DispatchMessage(string.Format("Roger. All units, {0} call is Code 4", pCalloutName), true);

				List<AudioFile> pAudio = new List<AudioFile>();
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));

				if (pSuspectIsInCustody == true) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.SUSPECT_IN_CUSTODY));
				}

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.WE_ARE_CODE_4));
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.NO_FURTHER_UNITS_REQUIRED));
				AudioPlayerEngine.PlayAudio(pAudio);
			});
		}

		internal static void PlayerLeftCode4(string pCalloutName)
		{
			GameFiber.StartNew(() =>
			{
				DispatchMessage(string.Format("The {0} call is Code 4 ADAM.", pCalloutName), true);

				List<AudioFile> pAudio = new List<AudioFile>();
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.WE_ARE_CODE_4A));
				AudioPlayerEngine.PlayAudio(pAudio);

				GameFiber.Sleep(3000);

				SergeantMessage("Stop by my office after shift please, over");
				Game.DisplayHelp("You left the scene!");
			});
		}

		internal static void OfficerDown()
		{
			Game.DisplayNotification("~b~Dispatch: ~w~All units, ~r~Officer Down~w~. All available units respond, Code 99.");

			List<AudioFile> pAudio = new List<AudioFile>();
			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));

			pAudio.AddRange(UnitAudio);
			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_NOT_RESPONDING));

			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.ALL_UNITS_RESPOND_CODE_99_EMERGENCY));
			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void OfficerCode99(bool pNoResponse)
		{
			Game.DisplayNotification("~b~Dispatch: ~w~All units, ~r~Officer Needs Help~w~. All available units respond, Code 99.");

			List<AudioFile> pAudio = new List<AudioFile>();
			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));

			if (pNoResponse) {
				pAudio.AddRange(UnitAudio);
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_NOT_RESPONDING));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE));
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_IN_NEED_OF_ASSISTANCE));
			}

			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.ALL_UNITS_RESPOND_CODE_99_EMERGENCY));
			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void DispatchCallout(string pCalloutName, Vector3 SpawnPoint, List<DISPATCH.CRIMES> CrimeEnums, CallResponseType pResponse, List<AudioFile> pAudio = null)
		{
			//Game.DisplayNotification(String.Format("~b~Dispatch: ~w~All units, we have a ~r~{0} ~w~in ~b~{1}~w~. Available units, respond ~g~{2}", pCalloutName, pZoneName, ResponseString(pResponse)))

			if (pAudio == null) {
				pAudio = new List<AudioFile>();

				if (pResponse == CallResponseType.Code_3) {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));
				} else {
					pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS));
					//pAudio.AddRange(UnitAudio)
				}

				int iREPORTING = Common.gRandom.Next(1, 4);
				switch (iREPORTING) {
					case 1:
						pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.CITIZENS_REPORT));
                        break;
					case 2:
						pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE));
                        break;
					default:
                        pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORTING.WEVE_GOT));
                        break;
                }

                if (CrimeEnums.Count > 0) {
					if (CrimeEnums.Count == 1) {
						pAudio.Add(new AudioFile("DISPATCH", CrimeEnums[0]));
					} else {
						pAudio.Add(new AudioFile("DISPATCH", CrimeEnums[Common.gRandom.Next(CrimeEnums.Count)]));
					}
				}

				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION));

				//If pResponse = CallResponseType.Code_3 Then
				//    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.UNITS_RESPOND_CODE_3))
				//Else
				//    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.UNITS_RESPOND_CODE_2))
				//End If
			}

			AudioPlayerEngine.PlayAudio(pAudio, SpawnPoint);
		}

		internal static void DispatchMessage(string pMessage, bool DirectedAtPlayer = false)
		{
			//AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_inTRO_02))
			if (DirectedAtPlayer == true) {
				Game.DisplayNotification(string.Format("~b~Dispatch~w~: ~w~{0}, {1}.", Common.gUnitNumber, pMessage));
			} else {
				Game.DisplayNotification(string.Format("~b~Dispatch~w~: ~w~{0}.", pMessage));
			}
		}

		internal static void SergeantMessage(string pMessage)
		{
			//AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_inTRO_02))
			Game.DisplayNotification(string.Format("~b~Duty Sergeant~w~: ~b~{0}~w~, {1}.", Common.gUnitNumber, pMessage));
		}

		internal static void UnitMessage(string pMessage)
		{
			//AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_inTRO_02))
			Game.DisplayNotification(string.Format("~b~{0}~w~: Dispatch, {1}.", Common.gUnitNumber, pMessage));
		}

		internal static void SuspectSpotted()
		{
			Radio.PlayRadioAnimation();
			Game.DisplayNotification(string.Format("~b~{0}: ~w~{0}, suspect located, moving to engage.", Common.gUnitNumber));
			DispatchMessage("Roger.", true);

			List<AudioFile> pAudio = new List<AudioFile>();
			int locatedint = Common.gRandom.Next(3);
			if (locatedint == 0) {
				pAudio.Add(new AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.HAVE_A_VISUAL));
			} else if (locatedint == 1) {
				pAudio.Add(new AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.SUSPECT_IN_SIGHT));
			} else {
				pAudio.Add(new AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.SUSPECT_LOCATED_ENGAGING));
			}

			pAudio.AddRange(UnitAudio);

			int rogerint = Common.gRandom.Next(4);
			if (rogerint == 0) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER));
			} else if (rogerint == 1) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT));
			} else if (rogerint == 2) {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR));
			} else {
				pAudio.Add(new AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT));
			}

			AudioPlayerEngine.PlayAudio(pAudio);
		}

		internal static void PlayRadioAnimation()
		{
			Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1f, AnimationFlags.UpperBodyOnly | AnimationFlags.SecondaryTask);

			//GameFiber.StartNew(
			//    Sub()
			//        Dim t As New TaskSequence(Game.LocalPlayer.Character)
			//        t.Tasks.PlayAnimation("random@arrests", "radio_enter", 1.0F, AnimationFlags.AllowPlayerRotation1)
			//        t.Tasks.PlayAnimation("random@arrests", "radio_chatter", 1.0F, AnimationFlags.AllowPlayerRotation1)
			//        t.Tasks.PlayAnimation("random@arrests", "radio_exit", 1.0F, AnimationFlags.AllowPlayerRotation1)
			//        t.Execute()
			//    End Sub)
		}

		internal static void DispatchCallingUnit()
		{
			List<AudioFile> pAudio = new List<AudioFile>();

			pAudio.Add(new AudioFile("DISPATCH", DISPATCH.ATTENTION.DISPATCH_CALLING_UNIT));
			pAudio.AddRange(UnitAudio);

			AudioPlayerEngine.PlayAudio(pAudio);
		}

	}

}