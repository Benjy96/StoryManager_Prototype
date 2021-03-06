﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: track biggest influencers and then make that the state's "influencer"


/// <summary>
/// A variant of a finite state machine... The EMOTIONAL State Machine (ESM)
/// Each agent with an emotion chip has an emotional disposition. 
/// External factors can affect this agent's emotions; in addition, the way in which each emotion is influenced differs between emotional dispositions. 
/// The extent to which each emotion is influenced is then affected by weight values.
/// 
/// What is the difference between this and a Finite State Machine?
/// In an FSM, you enter into a State based upon a limited/defined set/number of conditions. With the Emotional State Machine, any number of external factors can influence the state you enter!
/// Instead of switching state based upon an internal check, any number of external factors (may not even be in game yet!) can influence the state of the agent!
/// Basically, the ESM responds to (unknown) external influences, versus a limited number of hard-coded true/false checks pre-runtime.
/// 
/// An example:
/// FSM: if(playerNear()) changeState(new RunState());
///     With the above architecture, to match the ESM, you would need hundreds of "if" checks, which you would need to create methods for, as well as design in advance
/// 
/// ESM: agent.Influence(Emotion.Anger, .4f);   //This shows how any external factor can affect state
///     With the ESM architecture, you don't need to write every possible check beforehand, but can add influences to the game as you go along throughout development
///     e.g. Add a boulder falling, and in its OnTriggerEnter method, access all nearby enemies and call Influence(Emotion.Fear, 1f);
///     If it were an FSM, you would add the boulder, then need to remember to add a check in code like: if(boulderFell() && closeTo(boulder)) changeState(new RunState());
///     
/// The essential difference between the two is knowledge - in an FSM, you need to know all the influencing factors. The ESM requires no such foreknowledge.
/// Instead of "If(any unlimited number of factors)", the ESM says: "If(emotion is high enough)". ESM does NOT need to track external factors! Has no knowledge outside its emotion variables!
///     
/// TL;DR - This class contains the logic/attributes that determine what behaviour an AI undertakes. If it's angry, then the "angry state" will execute.
/// </summary>
public class EmotionChip : MonoBehaviour {

    //Disposition changes how influencing factors affect the agent
    public Emotion disposition = Emotion.Calm;
    private Emotion currentEmotionalState;

    public Emotion CurrentEmotion { get { return currentEmotionalState; } set { currentEmotionalState = value; } }

    /// <summary>
    /// Rate at which agent tends towards their disposition. Higher reluctance takes longer to LERP to disposition.
    /// With stability of 0.5: Reluctance == .1: Time to revert to disposition (from 0) == 11s || Reluctance == .9: Time to revert to disposition (from 0) == 28s
    /// IS A WEIGHT.
    /// </summary>
    [Tooltip("Higher means lerps toward disposition more slowly")][Range(0.1f, 0.9f)] public float reluctance = 0.5f;

    /// <summary>
    /// How easy it is for an enemy is to change their current emotional state. Higher value means it is harder to influence their emotion.
    /// NOT A WEIGHT.
    /// </summary>
    [Tooltip("Higher means harder to change state")][Range(0.1f, 0.9f)] public float emotionalStability = 0.66f;

    //The agent's emotional state(s)
    public Dictionary<Emotion, float> agentEmotions = new Dictionary<Emotion, float>();
    private List<Emotion> emotionKeys;  //for iterating over dictionary when modifying it

    public float trust = 1f; //Calm weighting
    public float irascibility = 1f; //Anger weighting
    public float cowardice = 1f;    //Fear weighting

    private GameObject influencedBy;
    public GameObject LastHostileInfluence { get { return influencedBy; } set { influencedBy = value; } }

    [SerializeField] private State currentState;

    public State calmState;
    public State angryState;
    public State scaredState;
    public State fallbackState;   //fallback

    [HideInInspector] public bool enraged;  //Used to enable the fallback state - if other behaviours fail, use this as a last stand behaviour

    private void Awake()
    {
        //Store each emotion type in a state variable
        foreach (Emotion emotion in Enum.GetValues(typeof(Emotion)))
        {
            //Set the agent's most powerful emotion equal to their disposition
            if (emotion == disposition)
            {
                agentEmotions.Add(emotion, 1f);
            }
            else
            {
                agentEmotions.Add(emotion, 0f);
            }
        }
        //List for iterating over emotions
        emotionKeys = new List<Emotion>(agentEmotions.Keys);
    }

    /// <summary>
    /// Like a Finite State Machine, this method executes state behaviours.
    /// </summary>
    /// <param name="agent">The owner of this emotion chip</param>
    public void Execute(Enemy agent)
    {
        if (enraged)
        {
            TakeAction(fallbackState, agent);
        }
        else
        {
            //Step 1. Execute current emotional goal
            if (agentEmotions[Emotion.Calm] > emotionalStability)
            {
                currentEmotionalState = Emotion.Calm;
                TakeAction(calmState, agent);
            }

            if (agentEmotions[Emotion.Anger] > emotionalStability)
            {
                currentEmotionalState = Emotion.Anger;
                TakeAction(angryState, agent);
            }

            if (agentEmotions[Emotion.Fear] > emotionalStability)
            {
                currentEmotionalState = Emotion.Fear;
                TakeAction(scaredState, agent);
            }

            //Step 2. Tend towards disposition
            foreach (Emotion key in emotionKeys)
            {
                if (key == disposition)
                {
                    //.05 towards disposition every second (Execute called 50 times per second)
                    //If Stability == 0.5, then Stability / 0.05 == 10 Seconds to surpass Stability (and change state)
                    //Therefore, approx 10 seconds to return to disposition (assuming no external influences)
                    agentEmotions[key] = Mathf.Lerp(agentEmotions[key], 1f, Time.fixedDeltaTime / (10f / reluctance));
                }
                else
                {
                    agentEmotions[key] = Mathf.Lerp(agentEmotions[key], 0f, Time.fixedDeltaTime / (10f / reluctance));
                }
            }
            ScaleEmotions();
        }
    }

    /// <summary>
    /// Influence this agent's state - you may become their target!
    /// </summary>
    public void Influence(GameObject attacker, Emotion intent, float amount)
    {
        Emotion emotionBeforeInfluence = currentEmotionalState;
        Influence(intent, amount);
        if (intent == Emotion.Anger || intent == Emotion.Fear)
        {
            LastHostileInfluence = attacker;  //Set actor that changed current state to influence
            if (currentState.lastThreat != LastHostileInfluence) currentState.lastThreat = LastHostileInfluence;
        }
    }

    /// <summary>
    /// This method provides a way in which to influence this agent's emotional state - will not influence who the agent targets.
    /// Calm: will tend towards the intent at a rate modified by trust.
    /// Anger: will tend towards the intent scaled by irascibility, except fear will anger the agent further.
    /// Fear: will tend towards the intent scaled by cowardice.
    /// </summary>
    /// <param name="intent">The way in which the actor intends to influence this agent</param>
    /// <param name="amount">How much to influence the agent's emotions in the range of 0 to 1. 
    /// In update methods, scale by deltaTime.</param>
    public void Influence(Emotion intent, float amount)
    {
        amount = Mathf.Clamp(amount, 0f, 1f);
        
        //Influence the NPC by the external factor's intent differently, based upon this agent's emotional disposition
        switch (disposition)
        {
            //If agent is predisposed to calm:
            case Emotion.Calm:
                switch (intent)
                {
                    //Easy to calm the agent
                    case Emotion.Calm:  
                        agentEmotions[intent] += amount * trust;
                        break;

                    //Hard to anger or scare the agent
                    case Emotion.Anger: 
                    case Emotion.Fear:
                        agentEmotions[Emotion.Calm] -= amount / trust;
                        agentEmotions[intent] += amount / trust;
                        break;
                }
                break;

            //If agent is predisposed to anger:
            case Emotion.Anger:
                switch (intent)
                {
                    case Emotion.Calm:  //If actor intends to calm the enemy, calm them, but reduce by a factor of their irascibility
                        agentEmotions[intent] += amount / irascibility;
                        agentEmotions[Emotion.Anger] -= amount / irascibility;
                        break;

                    case Emotion.Anger: //If actor intends to anger the enemy, increase the intent by a factor of their irascibility
                        agentEmotions[intent] += amount * irascibility;
                        break;

                    case Emotion.Fear:  //If actor intends to scare enemy, simply anger the agent
                        agentEmotions[Emotion.Anger] += amount;
                        break;
                }
                break;

            //If agent is predisposed to fear:
            case Emotion.Fear:
                switch (intent)
                {
                    case Emotion.Calm:  //If actor intends to calm/anger the agent, do it, but reduce calming effect in relation to the agent's cowardice
                        agentEmotions[intent] += amount / cowardice;
                        agentEmotions[Emotion.Fear] -= amount / cowardice;
                        break;

                    case Emotion.Anger: //Agent is scared but also angered slowly
                        agentEmotions[Emotion.Fear] += amount;
                        agentEmotions[intent] += amount / cowardice;
                        break;

                    case Emotion.Fear:  //If actor intends to scare enemy, scare them by a factor of the intent and cowardice
                        agentEmotions[Emotion.Fear] += amount * cowardice;
                        break;
                }
                break;
        }
        //Bring values back into the range of 0 to 1
        ScaleEmotions();    
    }

    #region Implementation
    //Scale the emotions to be within the range [0:1]
    private void ScaleEmotions()
    {
        float totalValue = 0f;

        foreach (KeyValuePair<Emotion, float> agentEmotionPair in agentEmotions)
        {
            totalValue += agentEmotionPair.Value;
        }

        List<Emotion> emotions = new List<Emotion>(agentEmotions.Keys);
        foreach (Emotion key in emotions)
        {
            agentEmotions[key] = agentEmotions[key] / totalValue;
        }
    }

    /// <summary>
    /// Handles State switching, execution, and exits
    /// </summary>
    private void TakeAction(State goal, Enemy agent)
    {
        bool exiting = false;

        if (currentState != null && goal.GetType() != currentState.GetType())
        {
            //ExitState() can tell the agent what to do next. If it is null, however, we simply do as directed by the emotional goal.
            currentState.ExitState();
            currentState = null;
            exiting = true;
        }

        //If not in any state or specified state, set it up (enter/construct) : OR : current state not same type as goal AND current goal not being worked on
        if (currentState == null || currentState.GetType() != goal.GetType())
        {
            currentState = goal.CreateState(agent, LastHostileInfluence);
        }

        if(!exiting) currentState.Execute();
    }
    #endregion
}

public enum Emotion { Calm, Anger, Fear }