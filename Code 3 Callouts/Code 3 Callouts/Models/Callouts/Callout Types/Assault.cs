using LSPD_First_Response.Engine.Scripting;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Stealth.Common.Extensions;
using Stealth.Plugins.Code3Callouts.Models.Peds;
using Stealth.Plugins.Code3Callouts.Util;
using Stealth.Plugins.Code3Callouts.Util.Audio;
using Stealth.Plugins.Code3Callouts.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static Stealth.Common.Models.QuestionWindow;
using static Stealth.Plugins.Code3Callouts.Common;
using static Stealth.Plugins.Code3Callouts.Util.Audio.AudioDatabase;

namespace Stealth.Plugins.Code3Callouts.Models.Callouts.CalloutTypes
{
    [CalloutInfo("Assault", CalloutProbability.High)]
    internal class Assault : CalloutBase
	{

		EType AssaultType = EType.Genuine;
		string[] SuspectModels = {
			"A_M_Y_SouCent_01",
			"A_M_Y_StWhi_01",
			"A_M_Y_StBla_01",
			"A_M_Y_Downtown_01",
			"A_M_Y_BevHills_01",
			"G_M_Y_MexGang_01",
			"G_M_Y_MexGoon_01",
			"G_M_Y_StrPunk_01"
		};
		string[] VictimModels = {
			"A_F_Y_GenHot_01",
			"A_F_Y_Hippie_01",
			"A_F_Y_Hipster_01",
			"A_F_Y_BevHills_01",
			"A_F_Y_BevHills_02",
			"A_F_M_Tourist_01",
			"A_F_M_FatWhite_01",
			"A_F_M_Business_02"
		};
		string[] WitnessModels = {
			"A_M_M_BevHills_01",
			"A_M_M_GenFat_01",
			"A_M_M_Business_01",
			"A_M_M_Golfer_01",
			"A_M_M_Skater_01",
			"A_M_M_Salton_01",
			"A_M_M_Tourist_01"

		};
		int weaponFactor = 0;
		bool suspectIdentified = false;
		bool anyoneArrested = false;
		bool arrestedTipDisplayed = false;

		bool suspectReturning = false;
        public Assault() : base("Assault", CallResponseType.Code_2)
        {
			RadioCode = 240;
			CrimeEnums = new List<DISPATCH.CRIMES>() {
				DISPATCH.CRIMES.CODE_240,
				DISPATCH.CRIMES.ASSAULT
			};

			CallDetails = string.Format("[{0}] ", CallDispatchTime.ToString("M/d/yyyy HH:mm:ss"));
			CallDetails += "Female RP called to report that she was assualted by a male suspect who left the area.";
			CallDetails += Environment.NewLine;
			CallDetails += Environment.NewLine;
			CallDetails += "A witness is on scene awaiting police arrival; no further information at this time.";

			Objective = "Interview the ~o~victim ~w~ and ~o~witness.~w~~n~Determine what ~r~actually ~w~happened!";
		}

		public override bool OnBeforeCalloutDisplayed()
		{
			bool baseReturn = base.OnBeforeCalloutDisplayed();

			if (baseReturn == false) {
				return false;
			}

			Vector3 suspectSpawn = PedHelper.GetSafeCoordinatesForPed(World.GetNextPositionOnStreet(SpawnPoint.Around(20)));
			if (suspectSpawn == Vector3.Zero) {
				suspectSpawn = World.GetNextPositionOnStreet(SpawnPoint.Around(20));
			}
			Suspect s = new Suspect("Suspect1", SuspectModels[Common.gRandom.Next(SuspectModels.Length)], suspectSpawn, 0, false);
			s.DisplayName = "Suspect";
			s.BlockPermanentEvents = true;
			s.MakePersistent();

			int suspectStory = Common.gRandom.Next(5);

			int genuineFactor = Common.gRandom.Next(1, 11);
			if (genuineFactor < 7) {
				AssaultType = EType.Genuine;

				weaponFactor = Common.gRandom.Next(3);
				if (weaponFactor == 1) {
					s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_KNIFE"), 56, false);
				} else if (weaponFactor == 2) {
					s.Inventory.GiveNewWeapon(new WeaponDescriptor("WEAPON_PISTOL"), 56, false);
				}

				s.PhysicalCondition = "Subject appears nervous, shifting his weight; he is looking around and avoiding eye contact.";
			} else {
				AssaultType = EType.Fabricated;
				suspectStory = 4;
				s.PhysicalCondition = "Subject speaking confidently, and is making firm eye contact.";
			}

			List<QAItem> mSuspectQAItems = new List<QAItem>();
			mSuspectQAItems.Add(new QAItem("Why don't you tell me what happened?"));
			mSuspectQAItems.Add(new QAItem("Why is she calling the police on you?"));
			mSuspectQAItems.Add(new QAItem("Who is she? Do you know her?"));
			mSuspectQAItems.Add(new QAItem("The other guy standing there, do you know him at all?"));
			mSuspectQAItems.Add(new QAItem("What am I going to find on you in my computer? Do you have any warrants?"));

			if (suspectStory == 0) {
				{
					s.SpeechLines.Add("I ain't got nothin to hide, alright?");
					s.SpeechLines.Add("You can't believe a WORD that she says!!");
					s.SpeechLines.Add("She was all up in my face, throwing herself at me!");
					s.SpeechLines.Add("I told her I got a girl, and she starts screamin!!");
					s.SpeechLines.Add("Saying I did this shit and that, it ain't true!");
					s.SpeechLines.Add("She crazy, man! I ain't done NOTHIN!");
					s.SpeechLines.Add("I could have left, ya know?! But I stayed!!");
					s.SpeechLines.Add("Cause I wanted to talk to you, give my side!!");

					mSuspectQAItems[0].Answer = "I told you, she threw herself at me!";
					mSuspectQAItems[1].Answer = "I don't know, man! She wants attention or something!";
					mSuspectQAItems[2].Answer = "How many times do I have to say it? NO!";
					mSuspectQAItems[3].Answer = "I think he wants to get with her, if you know what I mean.";
					mSuspectQAItems[4].Answer = "You ain't gonna find shit. It'll tell you to fuck off and leave me alone!";
				}
			} else if (suspectStory == 1) {
				{
					s.SpeechLines.Add("I ain't got nothing to say, Officer.");
					s.SpeechLines.Add("I want my lawyer before I talk to you.");

					mSuspectQAItems[0].Answer = "I believe I have the right to remain silent.";
					mSuspectQAItems[1].Answer = "I don't know, ask her!";
					mSuspectQAItems[2].Answer = "I'm refusing to answer that.";
					mSuspectQAItems[3].Answer = "No. in fact, he attacked me. You should arrest him.";
					mSuspectQAItems[4].Answer = "It'll tell you I have the right to an attorney.";
				}
			} else if (suspectStory == 2) {
				{
					s.SpeechLines.Add("Officer, you gotta believe me, I didn't do anything!");
					s.SpeechLines.Add("She's making it up, I swear I didn't hit her!");
					s.SpeechLines.Add("I mean...uhh...no English!");

					mSuspectQAItems[0].Answer = "She's a friend of mine, Officer. We were going out for coffee.";
					mSuspectQAItems[1].Answer = "I don't know! I swear!";
					mSuspectQAItems[2].Answer = "Yeah, she's my girlfriend...I mean, my coworker.";
					mSuspectQAItems[3].Answer = "He's the one who assaulted me!";
					mSuspectQAItems[4].Answer = "Nothing man, I'm clean!";
				}
			} else if (suspectStory == 3) {
				{
					s.SpeechLines.Add("Seriously, I don't get why that bitch called the cops.");
					s.SpeechLines.Add("All I did was say hi to her, man. She looked kinda hot, ya know?");
					s.SpeechLines.Add("You can't blame a guy for trying!");
					s.SpeechLines.Add("Come on, is that a crime, for god's sake?!");

					mSuspectQAItems[0].Answer = "I just wanted to get her number, man, that's it.";
					mSuspectQAItems[1].Answer = "Cause she's a straight up BITCH, that's why.";
					mSuspectQAItems[2].Answer = "No, man. And I'm glad, too. She looks hot, but she's crazy.";
					mSuspectQAItems[3].Answer = "Her boyfriend, maybe? I don't know.";
					mSuspectQAItems[4].Answer = "I had a DUI a few months ago, but that's it.";
				}
			} else {
				{
					s.SpeechLines.Add("I could have left, Officer. But...");
					s.SpeechLines.Add("When I saw you pull up, I had to come back.");
					s.SpeechLines.Add("I haven't got anything to hide, sir.");
					s.SpeechLines.Add("I came back to speak with you personally.");
					s.SpeechLines.Add("I just wanted to make sure you got the TRUTH.");
					s.SpeechLines.Add("Someone has to show her that she can't just make stuff up.");
					s.SpeechLines.Add("She was screaming that I threatened her with a knife.");
					s.SpeechLines.Add("You can check me, I don't have any weapons.");

					mSuspectQAItems[0].Answer = "I was walking down the street, and she just starts screaming at me.";
					mSuspectQAItems[1].Answer = "How should I know? I was just minding my own business!";
					mSuspectQAItems[2].Answer = "Hell no, man. I don't know that bitch.";
					mSuspectQAItems[3].Answer = "I've seen him on YouTube...he's Jeff Favignano, isn't he?";
					mSuspectQAItems[4].Answer = "Not that I know of.";
				}
			}
			s.QAItems = mSuspectQAItems;

			s.Tasks.Wander();
			Peds.Add(s);

			List<QAItem> vicQAItems = new List<QAItem>();
			vicQAItems.Add(new QAItem("Do you know the man who attacked you?"));
			vicQAItems.Add(new QAItem("What happened leading up to the attack?"));
			vicQAItems.Add(new QAItem("Did you see any weapons?"));
			vicQAItems.Add(new QAItem("Did he say anything to you?"));
			vicQAItems.Add(new QAItem("Do you need medical attention?"));

			Victim v = new Victim("Victim1", VictimModels[Common.gRandom.Next(VictimModels.Length)], SpawnPoint, 0);
			v.DisplayName = "Victim";
			v.SpeechLines.Add("Oh, Officer, thank god you're here!!");
			v.BlockPermanentEvents = true;
			v.MakePersistent();

			int injuryFactor = 0;

			injuryFactor = Common.gRandom.Next(3);

			switch (injuryFactor) {
				case 0:
					v.PhysicalCondition = "Some bruIsIng on the victim's arm";
					break;
				case 1:
					v.PhysicalCondition = "Victim has bruIsIng on her cheek";
					break;
				default:
					v.PhysicalCondition = "Dirty clothing; victim's knee is scraped. Wound consistent with a fall to the sidewalk";
					break;
			}

			switch (weaponFactor) {
				case 1:
					vicQAItems[2].Answer = "Just before the other guy rescued me, the attacker was pulling out a knife.";
					break;
				case 2:
					vicQAItems[2].Answer = "I think I saw something in his waistband. Could it have been a gun?";
					break;
				default:
					vicQAItems[2].Answer = "No...I didn't see one, at least.";
					break;
			}

			int victimStory = Common.gRandom.Next(5);
			if (AssaultType == EType.Fabricated)
				victimStory = 4;

			if (victimStory == 0) {
				{
					v.SpeechLines.Add("The man left just before you got here.");
					v.SpeechLines.Add("I was just walking down the street, and he came up to me...");
					v.SpeechLines.Add("He started saying things like \"I love you\", it was really creepy!");
					v.SpeechLines.Add("I told him to screw off and leave me alone, and walked away.");
					v.SpeechLines.Add("All of a sudden, he jumps on me, and starts hitting me!");
					v.SpeechLines.Add("This nice guy here rescued me and chased him off.");
					v.SpeechLines.Add("The guy ran off...but said he would be back.");
					v.SpeechLines.Add("Be careful, Officer. I think he had a weapon.");

					vicQAItems[0].Answer = "No, Officer, I've never seen him before.";
					vicQAItems[1].Answer = "Nothing, I mean, he just came out of nowhere.";
					vicQAItems[3].Answer = "Just that he loved me, and wanted to be with me forever.";
					vicQAItems[4].Answer = "I...I think I'll be ok. I just need a bandaid, if you have one.";
				}

			} else if (victimStory == 1) {
				{
					v.SpeechLines.Add("I didn't know what to do, I was so scared!");
					v.SpeechLines.Add("This guy, he just, grabbed me!");
					v.SpeechLines.Add("He came up behind me, put his arms around me...");
					v.SpeechLines.Add("I thought it was my boyfriend at first...");
					v.SpeechLines.Add("I was on my way to meet him, so I thought it was him...");
					v.SpeechLines.Add("But when he spoke, I didn't recognize the voice.");
					v.SpeechLines.Add("I don't care what happens, I just want him away from me, please!!");

					vicQAItems[0].Answer = "No way...like I said, I thought he was my boyfriend at first, but he's not!!";
					vicQAItems[1].Answer = "I was just walking down the street, he came up suddenly from behind.";
					vicQAItems[3].Answer = "Yeah, he hugged me and said 'Hey baby, you want to have some fun?'. It was so creepy!!";
					vicQAItems[4].Answer = "No, Officer, I'm fine, thank you.";
				}

			} else if (victimStory == 2) {
				{
					v.SpeechLines.Add("I didn't know what to do, I was so scared!");
					v.SpeechLines.Add("I was at Up-n-Atom, getting a bite to eat.");
					v.SpeechLines.Add("He came in, and he was just STARING at me while ordering his food.");
					v.SpeechLines.Add("I left, and he followed me outside.");
					v.SpeechLines.Add("He followed me onto the bus, and he just wouldn't go away.");
					v.SpeechLines.Add("I got off the bus, and he followed me!");
					v.SpeechLines.Add("I wanted to ask the bus driver for help, but...");
					v.SpeechLines.Add("I was just so scared.");
					v.SpeechLines.Add("This man here came up to me, called 911, and gave me his phone.");
					v.SpeechLines.Add("That's what happened. I just... *starts crying*");

					vicQAItems[0].Answer = "Never seen him before in my life.";
					vicQAItems[1].Answer = "He didn't get a chance to attack me...but he was stalking me!";
					vicQAItems[3].Answer = "Yeah, he said 'Hey honey, you want a real man to sleep with tonight?'.";
					vicQAItems[4].Answer = "I think...I'll be fine. I just need a moment.";
				}

			} else if (victimStory == 3) {
				{
					v.SpeechLines.Add("This guy came up to me, saying I dropped a $20 bill.");
					v.SpeechLines.Add("I told him that no, it wasn't mine.");
					v.SpeechLines.Add("He smiled and asked, 'Can you blame a guy for trying?'");
					v.SpeechLines.Add("He seemed cute, so I started talking to him.");
					v.SpeechLines.Add("He was really charming at first, I didn't think anything was wrong.");
					v.SpeechLines.Add("Then he just GRABS me and tries to drag me into an alley!!");
					v.SpeechLines.Add("I didn't know what to do! I was so scared, I just SCREAMED!");
					v.SpeechLines.Add("This man here chased him off, and he called 911.");
					v.SpeechLines.Add("I don't care what happens, I just want him away from me, please!!");
					v.SpeechLines.Add("He needs to be locked up, please!! He's DANGEROUS!!");

					vicQAItems[0].Answer = "No, Officer! Not at all!";
					vicQAItems[1].Answer = "Like I said, he was charming at first, then he turned into a fucking monster.";
					vicQAItems[3].Answer = "Yeah, he grabbed me and said 'Come on, I'm going to fuck your brains out, right now!'";
					vicQAItems[4].Answer = "No...but, um...I'm free tonight, if you're...single.";
				}
			} else {
				{
					AssaultType = EType.Fabricated;

					injuryFactor = Common.gRandom.Next(3);

					switch (injuryFactor) {
						case 0:
							v.PhysicalCondition = "No visible injuries";
							break;
						case 1:
							v.PhysicalCondition = "Victim's behavior is strange; very much on the defensive.";
							break;
						default:
							v.PhysicalCondition = "Scratches on victim's arm; but they seem to match her own fingernails";
							break;
					}

					v.SpeechLines.Add("The man left just before you got here.");
					v.SpeechLines.Add("I was just walking down the street, and he came up to me...");
					v.SpeechLines.Add("He started saying things like \"I love you\", it was really creepy!");
					v.SpeechLines.Add("I was just minding my own business!!");
					v.SpeechLines.Add("And he...he just...punched me!!");
					v.SpeechLines.Add("He called me a bitch, I swear, he's crazy!");
					v.SpeechLines.Add("He said he wanted to rape me!!");
					v.SpeechLines.Add("This nice guy here rescued me and chased him off.");
					v.SpeechLines.Add("He was crazy! Seriously! You have to arrest him!!");
					v.SpeechLines.Add("You look like a nice guy...can you help a girl out?");
					v.SpeechLines.Add("Pleeeeeaseeee?");

					vicQAItems[0].Answer = "He's the asshole that...I mean, no! The asshole that attacked me!";
					vicQAItems[1].Answer = "He just like, attacked me, for no reason at all!!";
					vicQAItems[3].Answer = "He said he had a girlfriend, but that I had a better ass.";
					vicQAItems[4].Answer = "I need some attention from you in my bed, if you get my drift.";
				}
			}
			v.QAItems = vicQAItems;

			Peds.Add(v);

			Witness w = new Witness("Witness1", WitnessModels[Common.gRandom.Next(WitnessModels.Length)], SpawnPoint.Around(5f), 0);
			w.DisplayName = "Witness";
			w.BlockPermanentEvents = true;
			w.MakePersistent();

			List<QAItem> mWitnessQA = new List<QAItem>();
			mWitnessQA.Add(new QAItem("So you were just passing by, is that correct?"));
			mWitnessQA.Add(new QAItem("What happened leading up to the incident?"));
			mWitnessQA.Add(new QAItem("Did the man have a weapon?"));
			mWitnessQA.Add(new QAItem("Did you hear the suspect say anything?"));
			mWitnessQA.Add(new QAItem("Did you use physical force at all?"));

			switch (weaponFactor) {
				case 1:
					mWitnessQA[2].Answer = "Yeah, just before I stepped in, he had a knife.";
					break;
				case 2:
					mWitnessQA[2].Answer = "I didn't see it, but I have a feeling he had one.";
					break;
				default:
					mWitnessQA[2].Answer = "No, sir, I didn't see a weapon.";
					break;
			}

			{
				switch (victimStory) {
					case 0:
						w.SpeechLines.Add("All I saw was him chasing her, and her telling him to fuck off.");
						w.SpeechLines.Add("The guy tried to grab her, so I pushed him back.");
						w.SpeechLines.Add("I told him, 'Relax dude, walk away.'");
						w.SpeechLines.Add("Kind of stupid, in hindsight, I guess.");
						w.SpeechLines.Add("But you hit a woman, you deserve what you get, ya know?");

						mWitnessQA[0].Answer = "Yes, sir. I got out of a cab, and caught the end of it.";
						mWitnessQA[1].Answer = "I don't know, as I said, I didn't see the whole thing.";
						mWitnessQA[3].Answer = "He kept saying he loved her, but she didn't know him. Weird.";
						mWitnessQA[4].Answer = "Not a lot...I just put my hand up and told him to walk away.";
						break;
					case 1:
						w.SpeechLines.Add("He was hugging her from behind, Officer.");
						w.SpeechLines.Add("He was claiming to be her boyfriend, and she was pushing him away.");
						w.SpeechLines.Add("She told me she didn't know him, and asked me to call you guys.");

						mWitnessQA[0].Answer = "Yeah, I was across from her when it happened.";
						mWitnessQA[1].Answer = "He just randomly walked up, and hugged her from behind.";
						mWitnessQA[3].Answer = "He said something like 'Hey baby, want to have fun?', I think.";
						mWitnessQA[4].Answer = "No, when she asked me for help, he ran off, that was it.";
						break;
					case 2:
						w.SpeechLines.Add("Whatever she says is true, Officer.");
						w.SpeechLines.Add("I don't know about following her onto the bus, but he got on with her.");
						w.SpeechLines.Add("He followed her off the bus, that's when I stepped in.");
						w.SpeechLines.Add("I wanted to stop him before anything bad happened.");

						mWitnessQA[0].Answer = "Well, I was on the bus before she got on.";
						mWitnessQA[1].Answer = "He was trying to talk to her, then followed her offf the bus.";
						mWitnessQA[3].Answer = "Not much, aside from trying to start a conversation with her.";
						mWitnessQA[4].Answer = "Nope, I just said I was calling the cops, and he ran off.";
						break;
					case 3:
						w.SpeechLines.Add("The guy ran off when he saw me, sir.");
						w.SpeechLines.Add("He was trying to drag her into the alley.");
						w.SpeechLines.Add("If I hadn't shown up, I think he mighta raped her.");
						w.SpeechLines.Add("Not sure if the guy is armed or not.");

						mWitnessQA[0].Answer = "Yes, Officer. I was just walking by.";
						mWitnessQA[1].Answer = "I saw them talking, and suddenly he grabbed her, and she screamed.";
						mWitnessQA[3].Answer = "Yeah, he told her, 'I'm going to fuck your brains out!', or something.";
						mWitnessQA[4].Answer = "No, sir. I just yelled 'Hey!', and he saw me, and took off.";
						break;
					default:
						w.SpeechLines.Add("Officer, she's making up the whole thing.");
						w.SpeechLines.Add("Guy doesn't give her any attention, so...she cries rape.");
						w.SpeechLines.Add("Maybe she's telling the truth, but, it doesn't seem genuine.");
						w.SpeechLines.Add("I don't know, something just seems off to me.");

						mWitnessQA[0].Answer = "I was waiting for the bus, and he was passing by.";
						mWitnessQA[1].Answer = "Nothing, really. She ran up to him and started flirting with him.";
						mWitnessQA[3].Answer = "He was just saying he had a girlfriend, and wasn't interested.";
						mWitnessQA[4].Answer = "Nope, I was staying out of it completely.";
						break;
				}

				w.SpeechLines.Add("I think that's him, coming back to the scene.");
				w.SpeechLines.Add("I'll give you a statement if you want.");
			}

			w.QAItems = mWitnessQA;
			w.PhysicalCondition = "Subject speaking normally, and making eye contact when he speaks.";

            LSPD_First_Response.Engine.Scripting.Entities.Persona pWitness = Functions.GetPersonaForPed(w);
            LSPD_First_Response.Engine.Scripting.Entities.Persona pNewWitness = BuildPersona(w, pWitness.Gender, pWitness.Birthday, 0, pWitness.Forename, pWitness.Surname, LSPD_First_Response.Engine.Scripting.Entities.ELicenseState.Valid, 0, false, false,
			false);
			Functions.SetPersonaForPed(w, pNewWitness);

			Peds.Add(w);

			if (Common.IsComputerPlusRunning()) {
				AddPedToCallout(v);
				AddPedToCallout(w);
			}

			if (PerformPedChecks()) {
				return baseReturn;
			} else {
				return false;
			}
		}

		public override bool OnCalloutAccepted()
		{
			Victim v = GetPed<Victim>("Victim1");
			if (v != null) {
				if (v.Exists()) {
					v.CreateBlip();
				}
			}

			Witness w = (Witness)GetPed("Witness1");
			if (w != null) {
				if (w.Exists()) {
					w.CreateBlip();
				}
			}

			return base.OnCalloutAccepted();
		}

        public override void OnArrivalAtScene()
		{
			base.OnArrivalAtScene();

			Victim v = (Victim)GetPed("Victim1");
			if (v.Exists())
				v.TurnToFaceEntity(Game.LocalPlayer.Character);

			Witness w = (Witness)GetPed("Witness1");
			if (w.Exists())
				w.TurnToFaceEntity(Game.LocalPlayer.Character);

			GameFiber.StartNew(() =>
			{
				Game.DisplayHelp("Press " + Config.SpeakKey.ToString() + " to speak with a subject.", 8000);
				GameFiber.Sleep(8000);
				Game.DisplayHelp("Use the interaction menu to observe or question a subject.", 8000);
				GameFiber.Sleep(8000);
				Game.DisplayHelp("Press " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + " to end this callout", 8000);
			});
		}

		private void ReturnSuspectToScene()
		{
			GameFiber.StartNew(() =>
			{
				try {
					Suspect s = (Suspect)GetPed("Suspect1");

					if (s.Exists()) {
						s.Tasks.ClearImmediately();

						Vector3 v3SusToPlayer = (Game.LocalPlayer.Character.Position - s.Position);
						v3SusToPlayer.Normalize();
						float hdg = MathHelper.ConvertDirectionToHeading(v3SusToPlayer);

						s.Tasks.FollowNavigationMeshToPosition(Game.LocalPlayer.Character.GetOffsetPositionFront(5f), hdg, 1f).WaitForCompletion();
						s.TurnToFaceEntity(Game.LocalPlayer.Character);
						//Stealth.Common.Natives.Functions.CallByName("TASK_GO_TO_ENTITY", s.Model, Game.LocalPlayer, -1, 8.0F, 1.0F, 1073741824, 0)
					}
				} catch (Exception ex) {
					Logger.LogVerboseDebug("Error returning suspect to scene -- " + ex.Message);
				}
			});
		}

		public override void Process()
		{
			base.Process();

			if (Game.LocalPlayer.Character.IsDead) {
				return;
			}

			Victim v = (Victim)GetPed("Victim1");
			Witness w = (Witness)GetPed("Witness1");
			Suspect s = (Suspect)GetPed("Suspect1");

			if (CalloutState == CalloutState.UnitResponding) {
				foreach (PedBase p in Peds) {
					if (p.IsDead) {
						p.Resurrect();
						p.Health = p.MaxHealth;
					}
				}
			}

			if (CalloutState == CalloutState.AtScene) {
				if (Game.LocalPlayer.Character.IsOnFoot) {
					if (suspectReturning == false) {
						suspectReturning = true;
						ReturnSuspectToScene();
					}
				}
			}

			if (Common.IsKeyDown(Config.SpeakKey)) {
				SpeakToSubject(ref v, ref w, ref s);
			}

			ArrestCheck(ref v, ref w, ref s);

			if (Common.IsKeyDown(Config.EndCallKey, Config.EndCallModKey)) {
					Radio.CallIsCode4(this.ScriptInfo.Name, anyoneArrested);
					End();
				//if (Config.EndCallModKey == Keys.None || Game.IsKeyDownRightNow(Config.EndCallModKey)) {
    //            }
            }
		}

		private void SpeakToSubject(ref Victim v, ref Witness w, ref Suspect s)
		{
			if (v != null && v.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(v.Position) < 3) {
					v.Speak();
					return;
				}
			} else {
				Game.DisplayNotification("Assault Callout crashed");
				End();
			}

			if (w != null && w.Exists()) {
				if (Game.LocalPlayer.Character.Position.DistanceTo(w.Position) < 3) {
					w.Speak();

					if (suspectIdentified == false) {
						if (s != null && s.Exists()) {
							suspectIdentified = true;
							s.CreateBlip();

							if (Common.IsComputerPlusRunning()) {
								AddPedToCallout(s);
							}
						}
					}

					return;
				}
			} else {
				Game.DisplayNotification("Assault Callout crashed");
				End();
			}

			if (suspectIdentified == true) {
				if (s != null && s.Exists()) {
					if (Game.LocalPlayer.Character.Position.DistanceTo(s.Position) < 3) {
						s.Speak();
					}
				} else {
					Game.DisplayNotification("Assault Callout crashed");
					End();
				}
			}
		}

		private void ArrestCheck(ref Victim v, ref Witness w, ref Suspect s)
		{
			ArrestCheck(v);
			ArrestCheck(w);
			ArrestCheck(s);

			if (arrestedTipDisplayed == false && anyoneArrested == true) {
				arrestedTipDisplayed = true;
				Game.DisplayHelp("You may end this callout with " + Config.GetKeyString(Config.EndCallKey, Config.EndCallModKey) + ", or continue investigating.");
			}
		}

		private void ArrestCheck(PedBase p)
		{
			if (p != null) {
				if (p.Exists()) {
					if (p.IsDead | p.IsArrested()) {
						p.DeleteBlip();
						anyoneArrested = true;

						if (p.Name == "Suspect1") {
							if (p.Inventory.Weapons.Count > 0) {
								p.Inventory.Weapons.Clear();

								if (weaponFactor == 1) {
									Game.DisplayNotification("While searching the suspect, you find/remove a ~r~knife~w~.");
								} else if (weaponFactor == 2) {
									Game.DisplayNotification("While searching the suspect, you find/remove a ~r~pistol~w~.");
								}
							}
						}
					}
				}
			}
		}

        public override bool RequiresSafePedPoint {
			get { return true; }
		}

		public override bool ShowAreaBlipBeforeAccepting {
			get { return true; }
		}

		private enum EType
		{
			Genuine,
			Fabricated
		}

	}

}