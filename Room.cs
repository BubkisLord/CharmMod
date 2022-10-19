﻿using GlobalEnums;
using ItemChanger;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChanger.UIDefs;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Fyrenest
{
    /// <summary>
    /// Handles changes to rooms or areas of the game
    /// </summary>
    public abstract class Room
    {
        public static Room instance;
        /// <summary>
        /// The different types of transition gateways
        /// </summary>
        public enum TransitionType { top, left, right, bottom }

        /// <summary>
        /// The scene name of the room to be modified (if none is provided, OnBeforeLoad and OnLoad never trigger)
        /// </summary>
        public string RoomName;

        /// <summary>
        /// Should the room be flipped?
        /// </summary>
        public bool IsFlipped = false;

        /// <summary>
        /// What's the minimum amount of damage any hit should deal?
        /// </summary>
        public int MinDamage = 0;

        public int MaxDamage = 100;

        public int Revision = 1; //World init gets called if save from older Revision is loaded


        /// <param name="roomName">The scene name of this room (or a placeholder if there is none)</param>
        protected Room(string roomName)
        {
            RoomName = roomName;
            instance = this;
        }

        /// <summary>
        /// Called on initialization of the mod
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// Called on creation of a new save file
        /// </summary>
        public virtual void OnWorldInit() { }

        /// <summary>
        /// Called before any "Start" functions are executed in this room
        /// </summary>
        public virtual void OnBeforeLoad() { }

        /// <summary>
        /// Called after the "Start" functions of the room have executed
        /// </summary>
        public virtual void OnLoad() { }

        /// <summary>
        /// Creates a transition and sets its destination.
        /// </summary>
        /// <param name="sourceScene">Must be the scene it is called from.</param>
        /// <param name="sourceGateName">Name of the gate - eg. bot1, left2, etc.</param>
        /// <param name="targetScene">The target scene the transition will take you to.</param>
        /// <param name="targetGate">Name of the recieving gate - eg. bot1, right1, etc.</param>
        /// <param name="transitionType">The type of transition it is. (top, left, right, bottom)</param>
        /// <param name="x">The x coordinate of the gate.</param>
        /// <param name="y">The y coordinate of the gate.</param>
        /// <param name="oneWay">Whether the transition is one-way.</param>
        public void PlaceTransition(TransitionType transitionType, string sourceScene, string sourceGateName, string targetScene, string targetGate, float x, float y, bool oneWay = false)
        {
            //Add new transition
            GameObject gate1 = UnityEngine.Object.Instantiate(Prefabs.TOP_TRANSITION.Object, new Vector3(15.5f, 12, 0), Quaternion.identity);
            gate1.transform.SetScaleY(300);
            gate1.SetActive(true);
            gate1.name = sourceGateName;
            SetTransition(sourceScene, sourceGateName, targetScene, targetGate, oneWay);
        }

        /// <summary>
        /// Changes an item location using ItemChanger (Call in OnWorldInit)
        /// </summary>
        /// <param name="location">The item location to change</param>
        /// <param name="item">The item it should be changed to</param>
        /// <param name="merge">Should the item be merged with other items place here (used for shops,grubfather and seer)?</param>
        /// <param name="alternateName">Alternate name to be displayed in shops</param>
        /// <param name="alternateDesc">Alternate description to be displayed in shops</param>
        /// <param name="destroySeerRewards">Should the normal rewards of Seer be removed? (only applicable at that location)</param>
        public void SetItem(string location, string item, bool merge = false, int geoCost = 0, int essenceCost = 0, int grubCost = 0, string alternateName = null, string alternateDesc = null, bool destroySeerRewards = false, bool nonIncremental = false)
        {
            //find item and location
            AbstractPlacement placement = Finder.GetLocation(location).Wrap();
            AbstractItem aitem = Finder.GetItem(item);

            if (nonIncremental)
            {
                aitem.RemoveTags<IItemModifierTag>();
            }

            //set cost tags if necessary
            if (geoCost > 0)
            {
                if (placement is ISingleCostPlacement)
                {
                    ((ISingleCostPlacement)placement).Cost = new GeoCost(geoCost);
                }
                else
                {
                    aitem.AddTag<CostTag>().Cost = new GeoCost(geoCost);
                }
            }

            if (essenceCost > 0)
            {
                aitem.AddTag<CostTag>().Cost = new PDIntCost(essenceCost, nameof(PlayerData.dreamOrbs), essenceCost + " Essence");
            }

            if (grubCost > 0)
            {
                aitem.AddTag<CostTag>().Cost = new PDIntCost(grubCost, nameof(PlayerData.grubsCollected), grubCost + " Grubs");
            }

            //change UIDef names if necessary
            if (alternateName != null) ((MsgUIDef)aitem.UIDef).name = new BoxedString(alternateName);
            if (alternateDesc != null) ((MsgUIDef)aitem.UIDef).shopDesc = new BoxedString(alternateDesc);

            placement.Add(aitem);

            //add DestroySeerRewardTag if necessary
            if (destroySeerRewards)
            {
                placement.AddTag<DestroySeerRewardTag>().destroyRewards = SeerRewards.All;
            }

            //choose conflict reolution methos
            PlacementConflictResolution resolution = merge ? PlacementConflictResolution.MergeKeepingNew : PlacementConflictResolution.Replace;

            //add placement
            ItemChangerMod.AddPlacements(new AbstractPlacement[] { placement }, resolution);
        }

        /// <summary>
        /// Changes a transition using ItemChanger
        /// </summary>
        /// <param name="oneWay">If true, no transition from target to source will be set</param>
        public void SetTransition(string sourceScene, string sourceGate, string targetScene, string targetGate, bool oneWay = false)
        {
            Transition t1 = new Transition(sourceScene, sourceGate);
            Transition t2 = new Transition(targetScene, targetGate);

            ItemChangerMod.AddTransitionOverride(t1, t2);

            if (!oneWay) ItemChangerMod.AddTransitionOverride(t2, t1);
        }

        /// <summary>
        /// Places a prefab (Call in OnBeforeLoad or OnLoad)
        /// </summary>
        public GameObject PlaceGO(GameObject prefab, float x, float y, Quaternion? rotation = null)
        {
            GameObject go = GameObject.Instantiate(prefab, new Vector3(x, y, 0), rotation.GetValueOrDefault(Quaternion.identity));
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// Destroys the named GameObject (Call in OnBeforeLoad or OnLoad)
        /// </summary>
        public void DestroyGO(string name)
        {
            GameObject.Destroy(GameObject.Find(name));
        }

        /// <summary>
        /// Adds a text replacement to the TextChanger (Call in OnInit)
        /// </summary>
        public void ReplaceText(string key, string text, string sheetKey = "")
        {
            Fyrenest.instance.AddReplacement(key, text, sheetKey);
        }

        /// <summary>
        /// Changes whether the room should be dark without lantern (Call in OnBeforeLoad)
        /// </summary>
        /// <param name="dark">If it is dark or not.</param>
        public void SetDarkness(bool dark)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().darknessLevel = dark ? 2 : 0;
            }
        }

        /// <summary>
        /// Sets the hue of the room (Call in OnBeforeLoad)
        /// </summary>
        /// <param name="color">Sets the washed-out color of the room</param>
        public void SetColor(Color color)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().defaultColor = color;
            }
        }

        /// <summary>
        /// Sets the environment (Call in OnBeforeLoad)
        /// 0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet
        /// </summary>
        /// <param name="environmentType">0 = Dust, 1 = Grass, 2 = Bone, 3 = Spa, 4 = Metal, 5 = No Effect, 6 = Wet</param>
        public void SetEnvironment(int environmentType)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().environmentType = environmentType;
            }
        }

        /// <summary>
        /// Sets it to be windy like in the howling cliffs. (Call in OnBeforeLoad)
        /// </summary>
        /// <param name="isWindy">Sets if it is windy or not.</param>
        public void SetWindy(bool isWindy)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().SetWindy(isWindy);
            }
        }
        /// <summary>
        /// Sets the light that the hero emits. (Call in OnBeforeLoad)
        /// </summary>
        /// <param name="color">The color that the knight emits.</param>
        public void SetHeroLightColor(Color color)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().heroLightColor = color;
            }
        }
        /// <summary>
        /// Sets the saturation of the room (Call in OnBeforeLoad)
        /// </summary>
        /// <param name="saturation">The saturation of the room</param>
        public void SetSaturation(float saturation)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(go => go.name == "_SceneManager"))
            {
                go.GetComponent<SceneManager>().saturation = saturation;
            }
        }
    }
}
