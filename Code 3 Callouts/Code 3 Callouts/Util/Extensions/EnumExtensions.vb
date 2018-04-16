Imports System.Runtime.CompilerServices

Namespace Util.Extensions

    Module EnumExtensions

        <Extension()>
        Friend Function ToFriendlyString(ByVal e As [Enum]) As String
            Return e.ToString().Replace("_", " ")
        End Function

    End Module

End Namespace