﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class GameMetaInfo {

    // ----- Save & State Data ----- //
    public static string _SAVE_FILE_ENCRYPTED = Path.Combine(Application.streamingAssetsPath, "wizziot.dat");
    public static string _SAVE_FILE_JSON = Path.Combine(Application.streamingAssetsPath, "wizziot.json");

    //Register types of state data here - used for verifying all state data has been saved/loaded
    public static List<string> _STATE_DATA = new List<string>()
    {
        "Scene",
        "Keybinds",
        "GameDifficulty",
        "PlayerPosition",
        "PlayerHealth",
        "Equipped",
        "Inventory",
        "Coins",
        "MissionsActive"
    };

    // ----- Keybinds & Actions ----- //
    public static void SetAbilityKeybindAction(Abilities abil, KeyCode key, Action action)
    {
        keybindActions[key] = action;
        abilityKeybinds[abil] = key;
    }

    //Default key bindings ( <Key, User's key binding, i.e.: ability used> )
    public static Dictionary<KeyCode, Action> keybindActions = new Dictionary<KeyCode, Action>();

    //Ability key bindings - Displayed in UI
    //KeyCode used as a joining link between keybindActions & abilityKeybinds
    public static Dictionary<Abilities, KeyCode> abilityKeybinds = new Dictionary<Abilities, KeyCode>();

    // ----- Gameplay ----- //
    public static Difficulty _GAME_DIFFICULTY = Difficulty.Normal;
    //Scale is "1 + difficulty %" i.e.: Easy = + 0, Normal = + 0.25, Hard = + 0.5, Suicidal = + 0.75.
    public static float _DIFFICULTY_SCALE { get { return 1 + (((float)_GAME_DIFFICULTY) / Enum.GetValues(typeof(Difficulty)).Length); } }

    public static int _MAX_COINS = 100000000;

    // ----- Game World ----- //
    //Layer Contracts
    public static string _LAYER_AFFECTABLE_OBJECT { get { return "Object"; } }
    public static string _LAYER_IMMOVABLE_OBJECT { get { return "Environment"; } }
    public static string _LAYER_GROUND_WALKABLE { get { return "Ground"; } }
    public static string _LAYER_UI { get { return "UI"; } }

    //Tag Contracts
    public static string _TAG_SHOOTABLE_BY_PLAYER { get { return "Enemy"; } }
    public static string _TAG_SHOOTABLE_BY_NPC { get { return "Player"; } }

    // ----- Abilities ----- //
    //Codes
    public static int _INSTANT_ABILITY_CODE { get { return Enum.GetValues(typeof(Abilities)).Cast<int>().First(); } }
    public static int _AREA_ABILITY_CODE { get { return Enum.GetValues(typeof(Abilities)).Cast<int>().First() + 2;  } }
    public static int _DEFENSE_ABILITY_CODE { get { return Enum.GetValues(typeof(Abilities)).Cast<int>().First() + 4; } }

    //Ability Type Checkers
    public static bool _Is_Instant_Ability(Abilities ability)
    {
        int abilityCode = (int)ability;
        if (abilityCode >= _INSTANT_ABILITY_CODE && abilityCode < _AREA_ABILITY_CODE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool _Is_AoE_Ability(Abilities ability)
    {
        int abilityCode = (int)ability;
        if (abilityCode >= _AREA_ABILITY_CODE && abilityCode < _DEFENSE_ABILITY_CODE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool _Is_Defense_Ability(Abilities ability)
    {
        int abilityCode = (int)ability;
        if (abilityCode >= _DEFENSE_ABILITY_CODE)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Ability Icons (Sprites)
    public static Dictionary<Abilities, Sprite> abilityIcons = new Dictionary<Abilities, Sprite>()
    {
        { Abilities.Zap, (Sprite)Resources.Load("Zap", typeof(Sprite)) },
        { Abilities.Confuse, (Sprite)Resources.Load("Confuse", typeof(Sprite)) },
        { Abilities.Vortex, (Sprite)Resources.Load("Vortex", typeof(Sprite)) },
        { Abilities.Singularity, (Sprite)Resources.Load("Singularity", typeof(Sprite)) },
        { Abilities.Heal, (Sprite)Resources.Load("Heal", typeof(Sprite)) },
    };

    // ----- Stats ----- //
    public static Dictionary<Stats, string> _STAT_DESCRIPTIONS = new Dictionary<Stats, string>()
    {
        { Stats.ActionCostReduction, "Modifies ability cost" },
        { Stats.DamageModifier, "Multiplies damage dealt" },
        { Stats.DamageReduction, "Reduces incoming damage" },
        { Stats.Fitness, "Modifies Stamina regeneration rate. Multiplies heals received" },
        { Stats.MaxHealthModifier, "Multiplies character health limit" },
        { Stats.MaxStaminaModifier, "Multiplies character stamina limit" },
        { Stats.MitigationChance, "Chance to prevent an attack's damage" },
        { Stats.MovementSpeed, "Multiplies character's movement speed" },
        { Stats.Notoriety, "Modifies the amount of anger your attacks generate. A notorious player will anger enemies quickly" },
        { Stats.SightRange, "Affects how far the character can target things" }
    };
}

public enum Difficulty
{
    Easy, Normal, Hard, Suicidal
}

public enum Abilities
{
    Zap, Confuse,
    Vortex, Singularity,
    Heal
}

public enum StateData
{
    Scene,
    Keybinds,
    GameDifficulty,
    PlayerPosition,
    PlayerHealth,
    Equipped,
    Inventory,
    Coins,
    MissionsActive
}