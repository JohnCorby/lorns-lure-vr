using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace VRMaker
{
    public static class Controllers
    {
        public static bool ControllersAlreadyInit = false;


        private static CustomController vrControllers;
        private static CustomControllerMap vrGameplayMap;
        private static CustomControllerMap vrUIMap;

        private static bool hasRecentered;
        private static bool initializedMainPlayer;
        private static bool initializedLocalUser;

        private static BaseInput[] inputs;
        private static List<BaseInput> modInputs = new List<BaseInput>();

        internal static int leftJoystickID { get; private set; }
        internal static int rightJoystickID { get; private set; }

        internal static int ControllerID => vrControllers.id;

        internal static void Init()
        {
            if (!ControllersAlreadyInit)
            {
                ReInput.InputSourceUpdateEvent += UpdateVRInputs;
                SetupControllerInputs();
                ControllersAlreadyInit = true;
            }
        }





        private static void SetupControllerInputs()
        {
            vrControllers = RewiredAddons.CreateRewiredController();
            vrGameplayMap = RewiredAddons.CreateGameplayMap(vrControllers.id);

                inputs = new BaseInput[]
                {
                    new VectorInput(SteamVR_Actions.xbox_StickLeft, 0, 1),
                    new VectorInput(SteamVR_Actions.xbox_StickRight, 2, 3),
                    new ButtonInput(SteamVR_Actions.xbox_A, 4),
                    new ButtonInput(SteamVR_Actions.xbox_B, 5),
                    new ButtonInput(SteamVR_Actions.xbox_X, 6),
                    new ButtonInput(SteamVR_Actions.xbox_Y, 7),
                    new AxisInput(SteamVR_Actions.xbox_LT, 8),
                    new AxisInput(SteamVR_Actions.xbox_RT, 9),
                    new ButtonInput(SteamVR_Actions.xbox_LB, 10),
                    new ButtonInput(SteamVR_Actions.xbox_RB, 11),
                    new ButtonInput(SteamVR_Actions.xbox_StickLeftClick, 12),
                    new ButtonInput(SteamVR_Actions.xbox_StickRightClick, 13),
                    new ButtonInput(SteamVR_Actions.xbox_DUp, 14),
                    new ButtonInput(SteamVR_Actions.xbox_DDown, 15),
                    new ButtonInput(SteamVR_Actions.xbox_DLeft, 16),
                    new ButtonInput(SteamVR_Actions.xbox_DRight, 17),
                    new ButtonInput(SteamVR_Actions.xbox_Start, 18),
                    new ButtonInput(SteamVR_Actions.xbox_Select, 19),
                };
        }

        public static void Update()
        {
            if (!initializedMainPlayer)
            {
                Debug.Log("allPlayerCount: ");
                Debug.Log(ReInput.players.allPlayerCount);
                Player p = null;
                //for (int i = 0; i < ReInput.players.allPlayerCount; i++)
                //{
                //    p = ReInput.players.AllPlayers[i];
                //    if (p != null)
                //    {
                //        Logs.WriteInfo("found non null Player p with name: ");
                //        Logs.WriteInfo(p.name);
                //        break;
                //    }

                //}
                p = Input.player; // lorns lure specific

                if (AddVRController(p))
                {
                    initializedMainPlayer = true;
                    Debug.Log("VRController successfully added");
                }
            }

        }

        internal static bool AddVRController(Player inputPlayer)
        {
            if (!inputPlayer.controllers.ContainsController(vrControllers))
            {
                inputPlayer.controllers.AddController(vrControllers, false);
                vrControllers.enabled = true;
            }

            if (inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count < 1)
            {
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 0, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrGameplayMap);
                if (!vrGameplayMap.enabled)
                    vrGameplayMap.enabled = true;
            }

            return inputPlayer.controllers.ContainsController(vrControllers) && inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count >= 1;
        }

        private static void UpdateVRInputs()
        {
            
            foreach (BaseInput input in inputs)
            {
                input.UpdateValues(vrControllers);
            }

            foreach (BaseInput input in modInputs)
            {
                input.UpdateValues(vrControllers);
            }
        }


        // For printing the flatscreen game binds
        public static void LogAllGameActions(Rewired.Player player)
        {
            Debug.Log("LogAllGameActions started");
            // All elements mapped to all joysticks in the player
            foreach (Joystick j in player.controllers.Joysticks)
            {

                // Loop over all Joystick Maps in the Player for this Joystick
                foreach (JoystickMap map in player.controllers.maps.GetMaps<JoystickMap>(j.id))
                {

                    // Loop over all button maps
                    foreach (ActionElementMap aem in map.ButtonMaps)
                    {
                        Debug.Log(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name + " with actionId " + aem.actionId);
                    }

                    // Loop over all axis maps
                    foreach (ActionElementMap aem in map.AxisMaps)
                    {
                        Debug.Log(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name + " with actionId " + aem.actionId);
                    }

                    // Loop over all element maps of any type
                    foreach (ActionElementMap aem in map.AllMaps)
                    {
                        if (aem.elementType == ControllerElementType.Axis)
                        {
                            Debug.Log(aem.elementIdentifierName + " is assigned to Axis " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name + " with actionId " + aem.actionId);
                        }
                        else if (aem.elementType == ControllerElementType.Button)
                        {
                            Debug.Log(aem.elementIdentifierName + " is assigned to Button " + aem.elementIndex + " with the Action " + ReInput.mapping.GetAction(aem.actionId).name + " with actionId " + aem.actionId);
                        }
                    }
                }
            }
            Debug.Log("LogAllGameActions ended");
        } 
    }
}
