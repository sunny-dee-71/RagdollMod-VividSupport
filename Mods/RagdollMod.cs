using System;
using System.Collections.Generic;
using System.Text;
using VividV2.Classes.Buttons;
using VividV2.Classes.Enums;
using VividV2.Classes.Enums.Keybinds;
using VividV2.Core;

namespace RagdollMod.Mods
{
    public class RagdollMod : Module
    {
        public Variable keybind;
        public RagdollMod() : base("Ragdoll", Categories.Ragdoll, true)
        {
            keybind = AddVariable(new Variable("Ragdoll Bind", KeybindType.SingleHand, HandType.Right, KeybindButton.Primary));
        }


        public override void Update()
        {
            if (!Enabled) return;

            if (keybind.PressedValue)
            {
                Plugin.instance.EnableRagdoll();
            }
            else
            {
                Plugin.instance.DisableRagdoll();
            }
        }
    }
}
