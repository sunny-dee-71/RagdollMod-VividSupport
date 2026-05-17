using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VividV2.Classes;

namespace RagdollMod
{
    internal class Manifest : ExtensionManifest
    {
        public override string Name => "Ragdoll Mod";
        public override string Author => "IIDK & SunnyDee";
        //public override string Description => "Adds a ragdoll effect to the player when they die.";

        public override void OnLoad()
        {
            base.OnLoad();
            GameObject plugin = new GameObject("RagdollMod");
            plugin.AddComponent<Plugin>();
            GameObject.DontDestroyOnLoad(plugin);
        }
    }
}
