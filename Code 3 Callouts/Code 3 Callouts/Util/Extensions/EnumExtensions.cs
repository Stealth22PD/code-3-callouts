using System;
using System.Runtime.CompilerServices;

namespace Stealth.Plugins.Code3Callouts.Util.Extensions
{

	internal static class EnumExtensions
	{

		internal static string ToFriendlyString(this Enum e)
		{
			return e.ToString().Replace("_", " ");
		}

	}

}