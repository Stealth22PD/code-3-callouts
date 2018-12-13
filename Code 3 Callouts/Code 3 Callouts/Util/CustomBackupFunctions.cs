using Rage;
using System;
using CustomBackupLocations;
using System.Collections.Generic;

class CustomBackupFunctions
{   

    internal static void RequestBackup(Vector3 location, LSPD_First_Response.EBackupUnitType type, LSPD_First_Response.EBackupResponseType response, int count = 1)
    {

        BackupManager LocalManager = new BackupManager();

        if (count == 1)
            LocalManager.BackupRespond(location, type, response);

        else {
            for (int loopCount = 0; loopCount < 1; loopCount++) {
                LocalManager.BackupRespond(location, type, response);
            }
        }
    }
}