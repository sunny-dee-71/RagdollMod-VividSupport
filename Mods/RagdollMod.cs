using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VividV2.Classes.Buttons;
using VividV2.Classes.Buttons.Variables;
using VividV2.Classes.Enums;
using VividV2.Classes.Enums.Keybinds;
using VividV2.Core;

namespace RagdollMod.Mods
{
    public class RagdollMod : Module
    {
        public KeybindVariable keybind;
        public static BoolVariable grav;
        public RagdollMod() : base("Ragdoll", Categories.Ragdoll, true)
        {
            keybind = new KeybindVariable("Ragdoll Bind", KeybindType.SingleHand, HandType.Right, KeybindButton.Primary);
            grav = new BoolVariable("Gravity", true);
            AddVariable(grav);
            AddVariable(keybind);
            AddVariable(new IntVariable("Ragdoll Velocity X", 0, -100, 100));
            AddVariable(new IntVariable("Ragdoll Velocity Y", 0, -100, 100));
            AddVariable(new IntVariable("Ragdoll Velocity Z", 0, -100, 100));
        }


        public override void Update()
        {
            if (!Enabled) return;

            if (keybind.Pressed)
            {
                Plugin.instance.EnableRagdoll(new UnityEngine.Vector3(
                    (float)GetVariable<IntVariable>("Ragdoll Velocity X").Value,
                    (float)GetVariable<IntVariable>("Ragdoll Velocity Y").Value,
                    (float)GetVariable<IntVariable>("Ragdoll Velocity Z").Value
                ));
            }
            else
            {
                Plugin.instance.DisableRagdoll();
            }
        }
    }
}
