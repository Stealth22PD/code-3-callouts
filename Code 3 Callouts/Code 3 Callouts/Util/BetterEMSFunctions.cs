using Rage;
using System;
using BetterEMS;
using System.Collections.Generic;

class BetterEMSFunctions
{

    internal static void RequestBackup(Vector3 location, int count = 1)
    {

        if (count == 1)
            BetterEMS.API.EMSFunctions.RespondToLocation(location);

        else {
            for (int loopCount = 0; loopCount < 1; loopCount++) {
                BetterEMS.API.EMSFunctions.RespondToLocation(location);
            }
        }
    }
}