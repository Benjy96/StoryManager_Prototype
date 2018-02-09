﻿using System.Collections.Generic;
using UnityEngine;

//TODO: Research generics to make a mission class/classes

[CreateAssetMenu(fileName = "Standard Mission", menuName = "Missions/Standard Mission")]
public class Mission : ScriptableObject {

    MissionManager missionManager = MissionManager.Instance;

    bool completed = false;
    new public string name = "";
    public Mission[] additionalMissionStages;

    public virtual void CompleteMission()
    {
        if (additionalMissionStages == null)
        {
            if (missionManager.activeMissions.Contains(this))
            {
                completed = true;
                missionManager.activeMissions.Remove(this);
            }
        }
        else
        {
            //TODO: Add multi mission management support in Mission Manager (depends on how we track the currently engaged missions, etc)
            //missionManager.CurrentMission(x).SetStep(additionalMissionStages[count];
        }
    }

    public virtual bool UpdateMission(Targetable target)
    {
        Debug.Log("Update the mission progress");
        return false;   //Not completed
    }
}
