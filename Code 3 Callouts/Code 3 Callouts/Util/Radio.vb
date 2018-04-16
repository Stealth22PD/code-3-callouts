Imports Rage
Imports Stealth.Plugins.Code3Callouts.Util.Audio
Imports LSPD_First_Response.Mod.API.Functions

Namespace Util

    Module Radio

        Public Sub AcknowledgeCallout(ByVal pCalloutName As String, ByVal pResponse As CallResponseType)
            Radio.PlayRadioAnimation()
            Dim pAudio As New List(Of AudioFile)
            Game.DisplayNotification(String.Format("~b~{0}~w~: ~w~Dispatch, {0} responding.", Common.gUnitNumber, pCalloutName))

            Dim responseInt As Integer = gRandom.Next(3)
            If responseInt = 1 Then
                pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_EN_ROUTE))
            ElseIf responseInt = 2 Then
                pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.ROGER_ON_OUR_WAY))
            Else
                pAudio.Add(New AudioFile("OFFICER", AudioDatabase.OFFICER.RESPONDING.COPY_IN_VICINITY))
            End If

            pAudio.AddRange(UnitAudio)

            Dim rogerInt As Integer = gRandom.Next(2)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            End If

            If pResponse = CallResponseType.Code_3 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_3))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.RESPOND_CODE_2))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)

            DispatchMessage(String.Format("Roger. Respond ~g~{0}", ResponseString(pResponse)), True)
        End Sub

        Public Sub UnitIsOnScene()
            Radio.PlayRadioAnimation()
            Game.DisplayNotification(String.Format("~b~{0}: ~w~{0} is on scene, Code 6 at location.", Common.gUnitNumber))
            DispatchMessage("Roger.", True)

            Dim pAudio As New List(Of AudioFile)
            pAudio.Add(New AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.HAVE_A_VISUAL))

            pAudio.AddRange(UnitAudio)

            Dim rogerInt As Integer = gRandom.Next(4)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            ElseIf rogerInt = 1 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
            ElseIf rogerInt = 2 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub DispatchAcknowledge()
            Dim pAudio As New List(Of AudioFile)

            Dim rogerInt As Integer = gRandom.Next(4)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            ElseIf rogerInt = 1 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
            ElseIf rogerInt = 2 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub DispatchAcknowledgePlayer()
            DispatchMessage("Roger", True)

            Dim pAudio As New List(Of AudioFile)
            pAudio.AddRange(UnitAudio)

            Dim rogerInt As Integer = gRandom.Next(4)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            ElseIf rogerInt = 1 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
            ElseIf rogerInt = 2 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub AIOfficerResponding()
            Dim pAudio As New List(Of AudioFile)

            Dim AIResponseValues As OFFICER.AI_UNIT_RESPONDING() = [Enum].GetValues(GetType(OFFICER.AI_UNIT_RESPONDING))
            Dim AIResponse As OFFICER.AI_UNIT_RESPONDING = AIResponseValues(gRandom.Next(AIResponseValues.Length))
            pAudio.Add(New AudioFile("OFFICER", AIResponse))

            Dim rogerInt As Integer = gRandom.Next(4)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            ElseIf rogerInt = 1 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
            ElseIf rogerInt = 2 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub CallIsCode4(ByVal pCalloutName As String, Optional ByVal pSuspectIsInCustody As Boolean = False)
            GameFiber.StartNew(
                Sub()
                    Radio.PlayRadioAnimation()
                    GameFiber.Sleep(3000)

                    Game.DisplayNotification(String.Format("~b~{0}~w~: ~w~{0} to Dispatch, ~r~{1} ~w~call is Code 4.", Common.gUnitNumber, pCalloutName))
                    DispatchMessage(String.Format("Roger. All units, {0} call is Code 4", pCalloutName), True)

                    Dim pAudio As New List(Of AudioFile)
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))

                    If pSuspectIsInCustody = True Then
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.SUSPECT_IN_CUSTODY))
                    End If

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.WE_ARE_CODE_4))
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.NO_FURTHER_UNITS_REQUIRED))
                    AudioPlayerEngine.PlayAudio(pAudio)
                End Sub)
        End Sub

        Public Sub PlayerLeftCode4(ByVal pCalloutName As String)
            GameFiber.StartNew(
                Sub()
                    DispatchMessage(String.Format("The {0} call is Code 4 ADAM.", pCalloutName), True)

                    Dim pAudio As New List(Of AudioFile)
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))

                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.WE_ARE_CODE_4A))
                    AudioPlayerEngine.PlayAudio(pAudio)

                    GameFiber.Sleep(3000)

                    SergeantMessage("Stop by my office after shift please, over")
                    Game.DisplayHelp("You left the scene!")
                End Sub)
        End Sub

        Public Sub OfficerDown()
            Game.DisplayNotification("~b~Dispatch: ~w~All units, ~r~Officer Down~w~. All available units respond, Code 99.")

            Dim pAudio As New List(Of AudioFile)
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))

            pAudio.AddRange(UnitAudio)
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_NOT_RESPONDING))

            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.ALL_UNITS_RESPOND_CODE_99_EMERGENCY))
            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub OfficerCode99(ByVal pNoResponse As Boolean)
            Game.DisplayNotification("~b~Dispatch: ~w~All units, ~r~Officer Needs Help~w~. All available units respond, Code 99.")

            Dim pAudio As New List(Of AudioFile)
            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))

            If pNoResponse Then
                pAudio.AddRange(UnitAudio)
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_NOT_RESPONDING))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE))
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CRIMES.OFFICER_IN_NEED_OF_ASSISTANCE))
            End If

            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.ALL_UNITS_RESPOND_CODE_99_EMERGENCY))
            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub DispatchCallout(ByVal pCalloutName As String, ByVal SpawnPoint As Vector3, ByVal CrimeEnums As List(Of DISPATCH.CRIMES), ByVal pResponse As CallResponseType, Optional ByVal pAudio As List(Of AudioFile) = Nothing)
            'Game.DisplayNotification(String.Format("~b~Dispatch: ~w~All units, we have a ~r~{0} ~w~in ~b~{1}~w~. Available units, respond ~g~{2}", pCalloutName, pZoneName, ResponseString(pResponse)))

            If pAudio Is Nothing Then
                pAudio = New List(Of AudioFile)

                If pResponse = CallResponseType.Code_3 Then
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))
                Else
                    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.ATTENTION_ALL_UNITS))
                    'pAudio.AddRange(UnitAudio)
                End If

                Dim iReporting As Integer = gRandom.Next(1, 4)
                Select Case iReporting
                    Case 1
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.CITIZENS_REPORT))
                    Case 2
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.WE_HAVE))
                    Case Else
                        pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORTING.WEVE_GOT))
                End Select

                If CrimeEnums.Count > 0 Then
                    If CrimeEnums.Count = 1 Then
                        pAudio.Add(New AudioFile("DISPATCH", CrimeEnums(0)))
                    Else
                        pAudio.Add(New AudioFile("DISPATCH", CrimeEnums(gRandom.Next(CrimeEnums.Count))))
                    End If
                End If

                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.CONJUNCTIVES.IN_OR_ON_POSITION))

                'If pResponse = CallResponseType.Code_3 Then
                '    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.UNITS_RESPOND_CODE_3))
                'Else
                '    pAudio.Add(New AudioFile("DISPATCH", DISPATCH.RESPONSE_TYPES.UNITS_RESPOND_CODE_2))
                'End If
            End If

            AudioPlayerEngine.PlayAudio(pAudio, SpawnPoint)
        End Sub

        Public Sub DispatchMessage(ByVal pMessage As String, Optional ByVal DirectedAtPlayer As Boolean = False)
            'AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_INTRO_02))
            If DirectedAtPlayer = True Then
                Game.DisplayNotification(String.Format("~b~Dispatch~w~: ~w~{0}, {1}.", Common.gUnitNumber, pMessage))
            Else
                Game.DisplayNotification(String.Format("~b~Dispatch~w~: ~w~{0}.", pMessage))
            End If
        End Sub

        Public Sub SergeantMessage(ByVal pMessage As String)
            'AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_INTRO_02))
            Game.DisplayNotification(String.Format("~b~Duty Sergeant~w~: ~b~{0}~w~, {1}.", Common.gUnitNumber, pMessage))
        End Sub

        Public Sub UnitMessage(ByVal pMessage As String)
            'AudioPlayerEngine.PlayAudio(New AudioFile("DISPATCH", DISPATCH.GENERIC.DISPATCH_INTRO_02))
            Game.DisplayNotification(String.Format("~b~{0}~w~: Dispatch, {1}.", Common.gUnitNumber, pMessage))
        End Sub

        Public Sub SuspectSpotted()
            Radio.PlayRadioAnimation()
            Game.DisplayNotification(String.Format("~b~{0}: ~w~{0}, suspect located, moving to engage.", Common.gUnitNumber))
            DispatchMessage("Roger.", True)

            Dim pAudio As New List(Of AudioFile)
            Dim locatedInt As Integer = gRandom.Next(3)
            If locatedInt = 0 Then
                pAudio.Add(New AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.HAVE_A_VISUAL))
            ElseIf locatedInt = 1 Then
                pAudio.Add(New AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.SUSPECT_IN_SIGHT))
            Else
                pAudio.Add(New AudioFile("OFFICER", OFFICER.SUSPECT_SPOTTED.SUSPECT_LOCATED_ENGAGING))
            End If

            pAudio.AddRange(UnitAudio)

            Dim rogerInt As Integer = gRandom.Next(4)
            If rogerInt = 0 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER))
            ElseIf rogerInt = 1 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.ROGER_THAT))
            ElseIf rogerInt = 2 Then
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR))
            Else
                pAudio.Add(New AudioFile("DISPATCH", DISPATCH.REPORT_RESPONSE.TEN_FOUR_COPY_THAT))
            End If

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

        Public Sub PlayRadioAnimation()
            Game.LocalPlayer.Character.Tasks.PlayAnimation("random@arrests", "generic_radio_chatter", 1.0F, AnimationFlags.UpperBodyOnly Or AnimationFlags.SecondaryTask)

            'GameFiber.StartNew(
            '    Sub()
            '        Dim t As New TaskSequence(Game.LocalPlayer.Character)
            '        t.Tasks.PlayAnimation("random@arrests", "radio_enter", 1.0F, AnimationFlags.AllowPlayerRotation1)
            '        t.Tasks.PlayAnimation("random@arrests", "radio_chatter", 1.0F, AnimationFlags.AllowPlayerRotation1)
            '        t.Tasks.PlayAnimation("random@arrests", "radio_exit", 1.0F, AnimationFlags.AllowPlayerRotation1)
            '        t.Execute()
            '    End Sub)
        End Sub

        Public Sub DispatchCallingUnit()
            Dim pAudio As New List(Of AudioFile)

            pAudio.Add(New AudioFile("DISPATCH", DISPATCH.ATTENTION.DISPATCH_CALLING_UNIT))
            pAudio.AddRange(UnitAudio)

            AudioPlayerEngine.PlayAudio(pAudio)
        End Sub

    End Module

End Namespace