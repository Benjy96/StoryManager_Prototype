﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Outputs the story in the game world
/// </summary>
public class StoryDisplayManager : MonoBehaviour {

    // ----- Worldspace UI ----- //
    [Header("Set in Inspector")]
    [SerializeField] private GameObject storyDisplayCanvas;
    [SerializeField] private Text storyText;

    public void EnableStoryDisplay(Transform attachTo)
    {
        storyDisplayCanvas.SetActive(true);
        storyDisplayCanvas.transform.SetParent(attachTo);
    }

    public void DisableStoryDisplay(Transform attachTo)
    {
        storyDisplayCanvas.SetActive(false);
        storyDisplayCanvas.transform.SetParent(attachTo);
    }

    public string DisplayedStoryText
    {
        set { storyText.text = value; }
    }

    public Transform SetDisplayPosition
    {
        set
        {
            float npcHeight = value.localScale.y;
            Vector3 newTextPosition = new Vector3(value.position.x, value.position.y + npcHeight, value.position.z);

            storyDisplayCanvas.transform.position = newTextPosition;
        }
    }
}
