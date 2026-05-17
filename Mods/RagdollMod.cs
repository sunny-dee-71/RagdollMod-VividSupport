using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VividV2.Classes.Buttons;
using VividV2.Classes.Enums;
using VividV2.Classes.Enums.Keybinds;
using VividV2.Core;

namespace RagdollMod.Mods
{
    public class RagdollMod : Module
    {
        public Variable keybind;
        public static Variable grav;
        public RagdollMod() : base("Ragdoll", Categories.Ragdoll, true)
        {
            keybind = AddVariable(new Variable("Ragdoll Bind", KeybindType.SingleHand, HandType.Right, KeybindButton.Primary));
            grav = AddVariable(new Variable("Gravity", true));
            AddVariable(new Variable("Ragdoll Velocity X", 0, -100, 100));
            AddVariable(new Variable("Ragdoll Velocity Y", 0, -100, 100));
            AddVariable(new Variable("Ragdoll Velocity Z", 0, -100, 100));
        }


        public override void Update()
        {
            if (!Enabled) return;

            if (keybind.PressedValue)
            {
                Plugin.instance.EnableRagdoll(new UnityEngine.Vector3(
                    (float)GetVariable("Ragdoll Velocity X").IntValue,
                    (float)GetVariable("Ragdoll Velocity Y").IntValue,
                    (float)GetVariable("Ragdoll Velocity Z").IntValue
                ));
            }
            else
            {
                Plugin.instance.DisableRagdoll();
            }
        }
    }
}
