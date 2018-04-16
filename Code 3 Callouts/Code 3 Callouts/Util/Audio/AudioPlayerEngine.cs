using Rage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Stealth.Plugins.Code3Callouts.Util.Audio
{

	internal static class AudioPlayerEngine
	{

		internal static void PlayDispatchAudio(Enum pEnum)
		{
			AudioFile a = new AudioFile("DISPATCH", pEnum);
			PlayAudio(a);
		}

		internal static void PlayOfficerAudio(Enum pEnum)
		{
			AudioFile a = new AudioFile("OFFICER", pEnum);
			PlayAudio(a);
		}

		internal static void PlayAudio(AudioFile pAudioFile)
		{
			List<AudioFile> list = new List<AudioFile>();
			list.Add(pAudioFile);
			PlayAudio(list);
		}

		internal static void PlayAudio(List<AudioFile> pAudioFiles, Vector3? pLocation = null)
		{
			try {
				GameFiber.StartNew(() =>
				{
					string[] mAudioNames = pAudioFiles.Select(x => x.FileEnum.ToString()).ToArray();
					string audio = string.Join(" ", mAudioNames);
					Logger.LogVerboseDebug("Audio String -- " + audio);

					if (pLocation == null) {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(audio);
					} else {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(audio, pLocation.Value);
					}
				});
			} catch (Exception ex) {
				Logger.LogVerbose("Error playing audio -- " + ex.Message);
			}
		}

		internal static void PlayAudio(List<string> pAudioFiles, Vector3? pLocation = null)
		{
			try {
				GameFiber.StartNew(() =>
				{
					string audio = string.Join(" ", pAudioFiles);

					if (pLocation == null) {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(audio);
					} else {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(audio, pLocation.Value);
					}
				});
			} catch (Exception ex) {
				Logger.LogVerbose("Error playing audio -- " + ex.Message);
			}
		}

		internal static void PlayAudio(string pAudio, Vector3? pLocation = null)
		{
			try {
				GameFiber.StartNew(() =>
				{
					if (pLocation == null) {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudio(pAudio);
					} else {
						LSPD_First_Response.Mod.API.Functions.PlayScannerAudioUsingPosition(pAudio, pLocation.Value);
					}
				});
			} catch (Exception ex) {
				Logger.LogVerbose("Error playing audio -- " + ex.Message);
			}
		}

		internal class AudioFile
		{
			internal AudioFile(string pClassName, Enum pAudioFile)
			{
				_class = pClassName;
				_fileEnum = pAudioFile;
			}

			private Enum _fileEnum;
			internal Enum FileEnum {
				get { return _fileEnum; }
				set { _fileEnum = value; }
			}

			private string _class;
			internal string AudioClass {
				get { return _class; }
				set { _class = value; }
			}
		}

	}

}