﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereNPC : InteractableNPC
{
    #region MonoBehaviours
    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = transform.localScale.y + (Mathf.Sin(Time.time * bobSpeed) * bobRange);

        transform.position = pos;
        transform.Rotate(Vector3.up, Mathf.Sin(Time.time));
    }
    #endregion

    public override void Interact()
    {
        storyActorName = "SphereState";
        Debug.Log(State);
        if (State.Equals("annoyed"))
        {
            Debug.Log("Do coroutine");
            ChangeColor();
        }
        story._inkStory.ChoosePathString(inkPath);
        story.DoStory();
    }

    protected override void SetExternalFunctions()
    {
        //Forgive me father, for I have sinned - using a try/catch like an if statement
        //Removes need for statics - this allows us to check if we have already bound the external function
        try
        {
            story._inkStory.ValidateExternalBindings();
        }
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
        {
            story._inkStory.BindExternalFunction("SpherePushOff", () => SpherePushOff());
        }

    }

    private void SpherePushOff()
    {
        Vector3 away = (player.transform.position - new Vector3(InteractingNPC.x, InteractingNPC.y, InteractingNPC.z)).normalized;
        away *= pushOffForce;    //set magnitude

        player.GetComponent<Rigidbody>().AddForce(away, ForceMode.Impulse);
    }

    private void ChangeColor()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }
}
