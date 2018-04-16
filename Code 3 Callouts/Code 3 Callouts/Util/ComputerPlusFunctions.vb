Imports Rage
Imports ComputerPlus
Imports ComputerPlus.API

Module ComputerPlusFunctions

    Friend Function CreateCalloutData(pFullName As String, pShortName As String, pLocation As Vector3, pResponseType As EResponseType, pDescription As String,
                                      Optional ByVal pState As ECallStatus = ECallStatus.Created,
                                      Optional ByVal pPeds As List(Of Ped) = Nothing, Optional ByVal pVehicles As List(Of Vehicle) = Nothing) As CalloutData

        Return New CalloutData(pFullName, pShortName, pLocation, pResponseType, pDescription, pState, pPeds, pVehicles)
    End Function

    Friend Sub CreateCallout(ByVal pData As CalloutData)
        Functions.CreateCallout(pData)
    End Sub

    Friend Function CreateCallout(pFullName As String, pShortName As String, pLocation As Vector3, pResponseType As EResponseType, pDescription As String,
                                      Optional ByVal pState As ECallStatus = ECallStatus.Created,
                                      Optional ByVal pPeds As List(Of Ped) = Nothing, Optional ByVal pVehicles As List(Of Vehicle) = Nothing) As Guid

        Dim pData As New CalloutData(pFullName, pShortName, pLocation, pResponseType, pDescription, pState, pPeds, pVehicles)
        CreateCallout(pData)

        Return pData.ID
    End Function

    Friend Sub UpdateCallStatus(ByVal pCallID As Guid, ByVal pStatus As ECallStatus)
        Select Case pStatus
            Case ECallStatus.Unit_Responding
                Functions.SetCalloutStatusToUnitResponding(pCallID)
            Case ECallStatus.At_Scene
                Functions.SetCalloutStatusToAtScene(pCallID)
            Case Else
                Functions.UpdateCalloutStatus(pCallID, pStatus)
        End Select
    End Sub

    Friend Sub AssignCallToAIUnit(ByVal pCallID As Guid)
        Functions.AssignCallToAIUnit(pCallID)
    End Sub

    Friend Sub ConcludeCallout(ByVal pCallID As Guid)
        Functions.ConcludeCallout(pCallID)
    End Sub

    Friend Sub CancelCallout(ByVal pCallID As Guid)
        Functions.CancelCallout(pCallID)
    End Sub

    Friend Sub AddUpdateToCallout(ByVal pCallID As Guid, ByVal pText As String)
        Functions.AddUpdateToCallout(pCallID, pText)
    End Sub

    Friend Sub AddPedToCallout(ByVal pCallID As Guid, ByVal pPed As Ped)
        Functions.AddPedToCallout(pCallID, pPed)
    End Sub

    Friend Sub AddVehicleToCallout(ByVal pCallID As Guid, ByVal pVeh As Vehicle)
        Functions.AddVehicleToCallout(pCallID, pVeh)
    End Sub

End Module