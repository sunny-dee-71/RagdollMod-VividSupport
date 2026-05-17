using System;
using System.Collections.Generic;
using System.Text;
using VividV2.Classes.Buttons;
using VividV2.Classes.Enums;
using VividV2.Classes.Enums.Keybinds;

namespace RagdollMod.Mods
{
    public class RagdollMod : Module
    {
        public Variable keybind = new Variable("Ragdoll Bind", KeybindType.SingleHand, HandType.Right, KeybindButton.Primary);
        public RagdollMod() : base("Ragdoll", Categories.Ragdoll, true)
        {
            AddVariable(keybind);
        }


        public override void Update()
        {
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
