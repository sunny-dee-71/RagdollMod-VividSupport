using GorillaLocomotion;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VividV2.Classes.MonoBehaviors;

namespace RagdollMod.Mods
{
    internal class Hit : MonoBehaviour
    {
        private float LastHit;

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time < LastHit + 0.2f) return;

            if (collision.relativeVelocity.magnitude >= 2f)
            {
                LastHit = Time.time;
                if (collision.gameObject.GetComponent<GorillaSurfaceOverride>() == null)
                    return;
                GorillaTagger.Instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.All, new object[] { collision.gameObject.GetComponent<GorillaSurfaceOverride>().overrideIndex, false, collision.relativeVelocity.magnitude / 10f });
            }
        }
    }
}
