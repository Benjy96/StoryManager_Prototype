﻿using UnityEngine;

[CreateAssetMenu(fileName = "HideState", menuName = "States/Hide")]
[RequireComponent(typeof(NeighbourhoodTracker))]
public class HideState : State {

    private Transform chaser;
    private Vector3 newHideSpot;
    private Transform obstacle;
    private float chaserSightDist;

    protected override void EnterState(Enemy owner, GameObject lastInfluence)
    {
        base.EnterState(owner, lastInfluence);

        neighbourhoodTracker.TrackObstacles();
    }

    /**
        1.) Seeker.pos - object.pos = direction through object
        2.) Normalize for direction vector
        3.) Set magnitude to a factor of object radius (so hide position is not inside object)
        4.) Move agent to calculated hide position.

        Example Modifications:
    1 - Hide only if in view. 2 - Favour rear of target. 3 - Hide only if under threat.
    */
    public override void Execute()
    {
        newHideSpot = Vector3.zero;
        chaser = SelectTarget();
        if (chaser != null)
        {
            Debug.Log("Hiding");
            EntityStats eS = chaser.GetComponent<EntityStats>();
            if (eS != null) chaserSightDist = eS.sqrMaxTargetDistance;
            CalculateHideSpot();
        }
        else
        {
            owner.Influence(Emotion.Calm, 0.2f * Time.deltaTime);
        }
    }

    private void CalculateHideSpot()
    {
        Vector3 distance = Vector3.zero;

        if (CanSeeChaser())
        {
            Debug.Log("Hiding");
            //Check appropriate hide spots
            foreach (Transform pos in neighbourhoodTracker.obstacles) //List is already sorted, closest will be first!
            {
                if (PointHiddenFromChaser(pos.transform.position))
                {
                    //Calculate spot
                    obstacle = pos;
                    Debug.Log("Obstacle:  " + obstacle);
                    newHideSpot = CalculateHideSpot(obstacle, influencer);
                    owner.MoveTo(newHideSpot);
                    break;
                }
            }
        }
        else
        {
            owner.Influence(Emotion.Calm, .2f);
            //Mirror enemy movement about obstacle
            newHideSpot = CalculateHideSpot(obstacle, influencer);
            owner.MoveTo(newHideSpot);
        }
    }

    private bool CanSeeChaser()
    {
        Ray ray = new Ray(owner.Position, chaser.position);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, owner.SightRange, LayerMask.GetMask(GameMetaInfo._LAYER_AFFECTABLE_OBJECT)))
        {
            Debug.Log("Ray hit");
            if (hit.transform.tag.Equals(chaser.tag))
            {
                Debug.Log("Can see: " + hit.transform.tag);
                owner.Influence(Emotion.Fear, 1f);
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private bool PointHiddenFromChaser(Vector3 point)
    {
        Ray ray = new Ray(owner.Position, chaser.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, chaserSightDist, LayerMask.GetMask(GameMetaInfo._LAYER_AFFECTABLE_OBJECT)))
        {
            if (hit.transform.tag.Equals(chaser.tag))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private Vector3 CalculateHideSpot(Transform point, GameObject chaser)
    {
        Vector3 direction = (chaser.transform.position - point.position).normalized;

        float obstacleSizeX = point.transform.localScale.x;
        float obstacleSizeZ = point.transform.localScale.z;
        float biggest = (obstacleSizeX > obstacleSizeZ) ? obstacleSizeX : obstacleSizeZ;
        Vector3 adjustedForObstacleSizePos = direction * biggest;

        return adjustedForObstacleSizePos;
    }
}
