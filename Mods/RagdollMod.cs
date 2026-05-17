using System;
using System.Collections.Generic;
using System.Text;
using VividV2.Classes.Buttons;

namespace RagdollMod.Mods
{
    public class RagdollMod : Module
    {
        public RagdollMod() : base("Ragdoll", Categories.Ragdoll, true)
        {
        }


        public override void Update()
        {
            if (Enabled)
            {
            }
        }

        public override void OnDisable()
        {
            Plugin.instance.DisableRagdoll();
        }

        public override void OnEnable()
        {
            Plugin.instance.EnableRagdoll();
        }
    }
}
