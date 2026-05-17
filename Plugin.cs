using BepInEx;
using GorillaExtensions;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace RagdollMod
{
    public class Plugin : MonoBehaviour
    {
        public static Plugin instance;

        public void Awake()
        {
            instance = this;
        }

        private static AssetBundle assetBundle;
        public static GameObject LoadAsset(string assetName)
        {
            GameObject gameObject = null;

            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RagdollMod.Resources.ragdoll");
            if (stream != null)
            {
                if (assetBundle == null)
                    assetBundle = AssetBundle.LoadFromStream(stream);
                
                gameObject = Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>(assetName));
            }
            else
            {
                Debug.LogError("Failed to load asset from resource: " + assetName);
            }

            return gameObject;
        }

        private static List<GameObject> portedCosmetics = new List<GameObject> { };
        public static void DisableCosmetics()
        {
            try
            {
                VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemLeftShoulder").gameObject.SetActive(false);
                VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemRightShoulder").gameObject.SetActive(false);
                VRRig.LocalRig.transform.Find("rig/head/gorillaface").gameObject.layer = LayerMask.NameToLayer("Default");

                foreach (GameObject Cosmetic in VRRig.LocalRig.cosmetics)
                {
                    if (Cosmetic.activeSelf && Cosmetic.transform.parent == VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"))
                    {
                        portedCosmetics.Add(Cosmetic);
                        Cosmetic.transform.SetParent(VRRig.LocalRig.headMesh.transform, false);
                        Cosmetic.transform.localPosition += new Vector3(0f, 0.1333f, 0.1f);
                    }
                }
            }
            catch { }
        }

        public static void EnableCosmetics()
        {
            VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemLeftShoulder").gameObject.SetActive(true);
            VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemRightShoulder").gameObject.SetActive(true);

            VRRig.LocalRig.transform.Find("rig/head/gorillaface").gameObject.layer = LayerMask.NameToLayer("MirrorOnly");
            foreach (GameObject Cosmetic in portedCosmetics)
            {
                Cosmetic.transform.SetParent(VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"), false);
                Cosmetic.transform.localPosition -= new Vector3(0f, 0.1333f, 0.1f);
            }

            portedCosmetics.Clear();
        }
        public static void CopyRigidbodySettings(Rigidbody target, Rigidbody source)
        {
            target.mass = source.mass;
            target.drag = source.drag;
            target.angularDrag = source.angularDrag;

            target.useGravity = source.useGravity;
            target.isKinematic = source.isKinematic;

            target.collisionDetectionMode = source.collisionDetectionMode;
            target.interpolation = source.interpolation;

            target.constraints = source.constraints;
        }

        public void Die()
        {
            if (Ragdoll != null)
                Destroy(Ragdoll);

            VRRig.LocalRig.enabled = false;
            DisableCosmetics();

            Ragdoll = LoadAsset("ragdoll");

            Ragdoll.transform.Find("Stand/Gorilla Rig/body").transform.position = VRRig.LocalRig.transform.Find("rig/body_pivot").position;
            Ragdoll.transform.Find("Stand/Gorilla Rig/body").transform.rotation = VRRig.LocalRig.transform.Find("rig/body_pivot").rotation;

            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.position = VRRig.LocalRig.leftHand.rigTarget.transform.position;
            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.rotation = VRRig.LocalRig.leftHand.rigTarget.transform.rotation;

            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.position = VRRig.LocalRig.rightHand.rigTarget.transform.position;
            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.rotation = VRRig.LocalRig.rightHand.rigTarget.transform.rotation;

            string[] velocitySets = new string[]
            {
                "Stand/Gorilla Rig/body",
                "Stand/Gorilla Rig/body/head",
                "Stand/Gorilla Rig/body/shoulder.L",
                "Stand/Gorilla Rig/body/shoulder.R",
                "Stand/Gorilla Rig/body/shoulder.L/upper_arm.L",
                "Stand/Gorilla Rig/body/shoulder.R/upper_arm.R",
                "Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L",
                "Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R",
            };
            foreach (string velocity in velocitySets)
            {
                Ragdoll.transform.Find(velocity).GetComponent<Rigidbody>().linearVelocity = GorillaTagger.Instance.rigidbody.linearVelocity;
            }

            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").GetComponent<Rigidbody>().linearVelocity = GorillaLocomotion.GTPlayer.Instance.LeftHand.velocityTracker.GetAverageVelocity(true, 0);
            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").GetComponent<Rigidbody>().angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/LeftHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;

            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").GetComponent<Rigidbody>().linearVelocity = GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0);
            Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").GetComponent<Rigidbody>().angularVelocity = GameObject.Find("Player Objects/Player VR Controller/GorillaPlayer/TurnParent/RightHand Controller").GetOrAddComponent<GorillaVelocityEstimator>().angularVelocity;

            Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").transform.rotation = GorillaTagger.Instance.headCollider.transform.rotation;

            VRRig.LocalRig.head.rigTarget.transform.rotation = Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").transform.rotation;

            Ragdoll.transform.Find("Stand/Mesh").gameObject.GetComponent<Renderer>().renderingLayerMask = 0;

            Rigidbody goodBody = Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").GetComponent<Rigidbody>();

            foreach (Rigidbody body in Ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                CopyRigidbodySettings(body, goodBody);
            }

            startForward = Ragdoll.transform.forward;
        }

        public void EnableRagdoll()
        {
            if (isDead)
                return;

            isDead = true;
            Die();
        }

        public void DisableRagdoll()
        {
            if (!isDead)
                return;

            isDead = false;
        }

        public void Update()
        {
            if (GorillaLocomotion.GTPlayer.Instance == null)
                return;

            if (isDead)
            {
                if (Ragdoll != null)
                {
                    VRRig.LocalRig.enabled = false;

                    UpdateRigPos();
                }
            }
            else
            {
                if (Ragdoll != null)
                {
                    VRRig.LocalRig.enabled = true;
                    EnableCosmetics();

                    Destroy(Ragdoll);

                    Ragdoll = null;
                }
            }
        }

        public void UpdateRigPos()
        {
            VRRig.LocalRig.transform.position = Ragdoll.transform.Find("Stand/Gorilla Rig/body").gameObject.transform.position;
            VRRig.LocalRig.transform.rotation = Ragdoll.transform.Find("Stand/Gorilla Rig/body").transform.rotation;

            VRRig.LocalRig.leftHand.rigTarget.transform.position = Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.position;
            VRRig.LocalRig.rightHand.rigTarget.transform.position = Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.position;

            VRRig.LocalRig.leftHand.rigTarget.transform.rotation = Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform.rotation;
            VRRig.LocalRig.rightHand.rigTarget.transform.rotation = Ragdoll.transform.Find("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.rotation;

            VRRig.LocalRig.head.rigTarget.transform.rotation = Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").transform.rotation;
        }

        public static Vector3 startForward;
        public static bool isDead;

        public static GameObject Ragdoll;
    }
}
