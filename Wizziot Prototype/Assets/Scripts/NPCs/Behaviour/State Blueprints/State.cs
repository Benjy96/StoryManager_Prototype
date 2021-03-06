﻿using UnityEngine;

public class State : ScriptableObject {

    public GameObject interestedIn;
    public GameObject secondaryInterest;
    public bool hostileToInterests;

    [HideInInspector] public GameObject lastThreat;   //use this when influencing - could be good for WHO to hide from or attack

    protected Enemy owner;
    protected EnemySpawnPoint spawn;
    protected NeighbourhoodTracker neighbourhoodTracker;
    protected AbilityComponent abilComponent;

    protected Transform target;
    protected AgentStats targetStats;
    
    /// <summary>
    /// Used to create a Scriptable Object instance
    /// </summary>
    /// <param name="owner">The agent attempting to create/use the specified state</param>
    /// <returns></returns>
    public State CreateState(Enemy owner, GameObject lastInfluence)
    {
        State newState = (State)Instantiate(Resources.Load("State Objects/" + name));
        newState.EnterState(owner, lastInfluence);
        return newState;
    }

    /// <summary>
    /// Use as a constructor to init local variables
    /// </summary>
    /// <param name="owner">The agent using this state</param>
    /// <returns></returns>
    protected virtual void EnterState(Enemy owner, GameObject lastInfluence)
    {
        this.owner = owner;
        lastThreat = lastInfluence;
        spawn = owner.Spawn;
        neighbourhoodTracker = owner.neighbourhoodTracker;
        abilComponent = owner.abilityComponent;

        //Register gameobject types as interests to the neighbourhood tracker
        neighbourhoodTracker.RegisterInterest(interestedIn);
        neighbourhoodTracker.RegisterInterest(secondaryInterest);
        neighbourhoodTracker.ScanForNearby();   //Check for registered interests on state entry
    }

    /// <summary>
    /// Agent accesses this method, which then uses a private implementation.
    /// </summary>
    public virtual void Execute()
    {
        Debug.Log("State.Execute(): Add State Behaviour Here");
    }

    /// <summary>
    /// Use to gracefully exit a state, remove interest, references, subscriptions - e.g. stop Coroutines, etc...
    /// </summary>
    /// <returns>The next state to enter. Returning null (by default) means the next State will be handled by the EmotionChip</returns>
    public virtual void ExitState()
    {
        neighbourhoodTracker.RemoveInterest(interestedIn);
        neighbourhoodTracker.RemoveInterest(secondaryInterest);
    }

    /// <summary>
    /// Selects a target. Emotional influencer used if no interests nearby
    /// </summary>
    protected virtual Transform SelectTarget()
    {
        GameObject targetGO = null;
        Transform target = null;

        targetGO = neighbourhoodTracker.RetrieveTrackedObject(interestedIn);
        if (targetGO == null ||
            (targetGO.transform.position - owner.Position).sqrMagnitude > (owner.SightRange * owner.SightRange))
        {
            targetGO = neighbourhoodTracker.RetrieveTrackedObject(secondaryInterest);
        }

        if (targetGO != null) target = targetGO.transform;
        if (target != null)
        {
            targetStats = target.GetComponent<AgentStats>();
            return target;
        }
        else if (lastThreat != null && (lastThreat.transform.position - owner.Position).sqrMagnitude < (owner.SightRange * owner.SightRange))
        {
            targetStats = lastThreat.GetComponent<AgentStats>();
            return lastThreat.transform;
        }
        return null;
    }
}