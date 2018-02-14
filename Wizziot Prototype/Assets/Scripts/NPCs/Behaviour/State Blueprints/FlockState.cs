﻿using UnityEngine;

[CreateAssetMenu(fileName = "FlockState", menuName = "States/Flock")]
[RequireComponent(typeof(NeighbourhoodTracker))]
public class FlockState : State {   //TODO: make an "anti-flock" state based upon emotional goal (e.g. make SO for scared goal, this one for angry goal)

    public GameObject secondaryTarget;  //Notes: Area detection || Spawner: Keep list of references to all "Objects" within collider radius which can be a target
    //Or use above variable as Prefab for the target TYPE - then do FindObjectOfType<secondaryTarget>() within RADIUS (collider)
    //public GameObject[] potentialTargets; //if(checkArea(potentialTargets[i]) secondaryTarget = checkArea(potentialTargets[i]);

    public float velocityMatchingWeight = 0.25f;
    public float flockCenteringWeight = 0.2f;
    public float attractionWeight = 2f;
    public float repulsionWeight = 2f;

    private NeighbourhoodTracker neighbourhood;
    private EnemySpawnPoint spawn;

    protected override State EnterState(Enemy owner)
    {
        this.owner = owner;
        neighbourhood = owner.GetComponent<NeighbourhoodTracker>();
        spawn = owner.Spawn;
        return this;
    }

    //TODO: Add Behaviour for when player out of range
    public override void Execute()
    {
        Transform target = SetTarget();

        //Get current velocity - going to be modified
        Vector3 vel = owner.navAgent.destination;

        //Velocity Matching - match velocity of neighbors
        Vector3 velAlign = neighbourhood.AvgVel;

        //Flock centering - move towards center of local neighbors
        Vector3 velCenter = neighbourhood.AvgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= owner.Position;    //look to center
        }

        //Attraction
        Vector3 attractDelta = target.transform.position - owner.Position;    //Agent to attractor vector

        //Attract if target is within targeting distance
        bool attracted = (attractDelta.sqrMagnitude < owner.stats.sqrMaxTargetDistance && owner.navAgent.stoppingDistance < attractDelta.magnitude);   //If distance less than max target distance, NPC attracted to target

        //Apply ALL velocities - the weighting will help influence how much of an impact "influence" each has. Each vector has an affect since vel is assigned and used each time
        float fdt = Time.fixedDeltaTime;

        if (velAlign != Vector3.zero)   //If we need to align
        {
            vel = Vector3.Lerp(vel, velAlign, velocityMatchingWeight * fdt);
        }

        if (velCenter != Vector3.zero)  //If we need to center
        {
            vel = Vector3.Lerp(vel, velCenter, flockCenteringWeight * fdt);
        }

        if (attractDelta != Vector3.zero) //If we need to go towards target
        {
            if (attracted)  //if distance from attractor is big enough
            {
                vel = Vector3.Lerp(vel, attractDelta, attractionWeight * fdt);
            }
            else
            {   //go away from attractor
                vel = Vector3.Lerp(vel, -attractDelta, repulsionWeight * fdt);  //TODO: Secondary target & secondary target allocation
            }
        }
        owner.navAgent.SetDestination(vel);
        owner.transform.LookAt(target.transform);
    }

    private Transform SetTarget()
    {
        //Acquire Target Destination
        Transform target = owner.target;
        if (target == null) target = secondaryTarget.transform;

        return target;
    }
}