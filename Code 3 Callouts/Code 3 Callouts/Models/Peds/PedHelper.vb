Imports Rage
Imports Stealth.Common
Imports Stealth.Plugins.Code3Callouts.Util

Namespace Models.Peds

    Module PedHelper

        Public Function GetPedSpawnPoint(ByVal pReferencePoint As Vector3) As Vector3
            Dim resultV3 As Vector3 = GetSafeCoordinatesForPed(pReferencePoint)

            If resultV3 = Vector3.Zero Then
                Return pReferencePoint
            Else
                Return resultV3
            End If
        End Function

        Public Function GetSafeCoordinatesForPed(ByVal pPosition As Vector3) As Vector3
            Dim resultV3 As Vector3 = Vector3.Zero

            Try
                'coordinatesFound = Rage.Native.NativeFunction.CallByName(Of UInteger)("GET_SAFE_COORD_FOR_PED", pPosition.X, pPosition.Y, pPosition.Z, True, resultV3.X, resultV3.Y, resultV3.Z, 0)
                resultV3 = Natives.Peds.GetSafeCoordinatesForPed(pPosition)
            Catch ex As Exception
                resultV3 = Vector3.Zero
                Logger.LogVerboseDebug("Error getting safe ped coordinates -- " & ex.Message)
            End Try

            Return resultV3
        End Function

        Public Function RandomizeDriverStory(ByVal rInt As Integer, ByVal pSpeed As Integer) As List(Of String)
            Dim list As New List(Of String)

            Select Case rInt
                Case 0
                    list.Add("The other driver came out of nowhere and hit me.")
                Case 1
                    list.Add("I was on an important call...I don't know what happened.")
                Case 2
                    list.Add("The other driver ran a red light and hit me.")
                Case 3
                    list.Add("[Slurred Speech] I uhh...don't know wha' happen.")
                Case 4
                    list.Add("The other driver was on their cell phone.")
                    list.Add("They were not paying any attention to the road.")
                Case 5
                    list.Add("I was in a hurry, and didn't see the other car.")
                    list.Add("Can we make this quick? I have to get home.")
                Case 6
                    list.Add("I'm not going to say anything without my lawyer.")
                Case 7
                    list.Add("That other driver needs to go back to school...")
                Case 8
                    list.Add("What's going to happen to my car?")
                Case 9
                    list.Add("Oh no...my car...my brand new car!!!")
                Case 10
                    list.Add("The other driver was on the wrong side of the road!")
                    list.Add("I tried to swerve, but I couldn't avoid them.")
                Case 11
                    list.Add("This is my mom's car...she is going to be pissed.")
                Case 12
                    list.Add("I was singing along to some tunes when it happened.")
                    list.Add("I was on a high note, and my windshield shattered!")
                Case 13
                    list.Add("I didn't even get to finish my text message to my BFF!!")
                    list.Add("I MEAN...uhh...")
                    list.Add("Seriously, what was the other driver thinking?!")
                Case 14
                    list.Add("My dad is a cop...he'll take care of all this.")
                Case Else
                    list.Add("I'm so dazed...I don't remember what happened.")
            End Select

            list.Add(String.Format("I was going around {0} MPH.", pSpeed))

            Return list
        End Function

        Public Function RandomizeImpairedDriverStory(ByVal rInt As Integer) As List(Of String)
            Dim list As New List(Of String)

            list.Add("[Slurred Speech] I uhh...don't know wha' happen.")

            Select Case rInt
                Case 0
                    list.Add("Them things were comin outta nowhere and hitting me!")
                Case 1
                    list.Add("It was the gremlins, I tell you!")
                Case 2
                    list.Add("Can I go home, Occifer?")
                Case 3
                    list.Add("Can I go? Got someone waitin at home, if ya know what I mean...")
                Case 4
                    list.Add("The car jusss' decided to go batshit crazy on me!!")
                Case 5
                    list.Add("Hey, ain't you that guy from the TV?")
                Case 6
                    list.Add("I want my lawyer!!")
                Case 7
                    list.Add("What's going to happen to my car?")
                Case 8
                    list.Add("Ohh shhhiittt...this is ma' mom's car...")
                Case 9
                    list.Add("*Hiccup* Hey Occifer, you want a beer?")
                Case 10
                    list.Add("I spilled my beer all over the car!!")
                Case 11
                    list.Add("I'm so dazed...I don't remember what happened.")
                Case 12
                    list.Add("Mind if I call a cab? My car ain't workin so good.")
                Case 13
                    list.Add("I...uhhh...had a little accident, Occifer...")
                Case 14
                    list.Add("My dad's ya cop, ya know!!")
                Case Else
            End Select

            Return list
        End Function

    End Module

End Namespace