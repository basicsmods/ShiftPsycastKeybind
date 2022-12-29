using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.AI.Group;

/*
 * https://harmony.pardeike.net/articles/patching-injections.html#__instance
 * 
 */

namespace ShiftPsycastKeybind
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("net.basics.ShiftPsycastKeybind");
            harmony.PatchAll();
        }
    }

    static class SkipUnusablePsycasts
    {
        /*
         The following block of code is in the Command.GizmoOnGUIInt method. We want to
         skip that if block whenever the command is a psycast that is currently unusable
         so that the keybinding can go to a usable version. We accomplish this by temporarily
         making the Command's hotKey var null.
         
        KeyCode keyCode = (this.hotKey == null) ? KeyCode.None : this.hotKey.MainKey;
		if (keyCode != KeyCode.None && !GizmoGridDrawer.drawnHotKeys.Contains(keyCode))
		{
		    Widgets.Label(rect, keyCode.ToStringReadable());
			   GizmoGridDrawer.drawnHotKeys.Add(keyCode);
			   if (this.hotKey.KeyDownEvent)
            {
                flag2 = true;
                Event.current.Use();
            }
        }

        */
        public static bool IsUnusablePsycast(ref Command cmd)
        {
            Command_Psycast psycast = cmd as Command_Psycast;
            if (psycast == null)
                return false;
            psycast.DisabledCheck();
            if (psycast.disabled)
                return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Command), nameof(Command.GizmoOnGUIInt),
    new Type[] { typeof(Rect), typeof(GizmoRenderParms) })]
    class Command_GizmoOnGUIInt
    {
        public static void Prefix(ref Command __instance, ref KeyBindingDef __state, Rect butRect, GizmoRenderParms parms)
        {
            __state = null;
            if (__instance.hotKey != null && SkipUnusablePsycasts.IsUnusablePsycast(ref __instance))
            {
                __state = __instance.hotKey;
                __instance.hotKey = null;
            }
        }

        public static void Postfix(ref Command __instance, ref KeyBindingDef __state, Rect butRect, GizmoRenderParms parms)
        {
            if (__state != null)
            {
                __instance.hotKey = __state;
            }
        }
    }
}