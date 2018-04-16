using Rage;
using System;

namespace Stealth.Plugins.Code3Callouts.Util
{

	internal static class Logger
	{

		internal static void LogTrivial(string pMessage)
		{
			try {
				Game.LogTrivial(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogVerbose(string pMessage)
		{
			try {
				Game.LogVerbose(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogVeryVerbose(string pMessage)
		{
			try {
				Game.LogVeryVerbose(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogExtremelyVerbose(string pMessage)
		{
			try {
				Game.LogExtremelyVerbose(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogTrivialDebug(string pMessage)
		{
			try {
				Game.LogTrivialDebug(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogVerboseDebug(string pMessage)
		{
			try {
				Game.LogVerboseDebug(FormatMessage(pMessage));
			} catch {
			}
		}

		internal static void LogExtremelyVerboseDebug(string pMessage)
		{
			try {
				Game.LogExtremelyVerboseDebug(FormatMessage(pMessage));
			} catch {
			}
		}

		private static string FormatMessage(string pMessage)
		{
			return string.Format("[{0}] {1}", Common.VersionInfo.ProductName, pMessage);
		}

	}

}