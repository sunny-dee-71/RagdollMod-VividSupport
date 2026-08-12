using BepInEx;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;
using RagdollMod.Mods;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using Valve.VR;
using VividV2.Classes.Buttons;
using VividV2.Classes.Buttons.Variables;

namespace RagdollMod
{
    public class Plugin : MonoBehaviour
    {
        public static Plugin instance;

        public static GameObject Ragdoll;
        public static bool isDead;
        public static Vector3 startForward;

        public static VRRig GrabbingRig;
        public static Rigidbody GrabBody;
        public static bool GrabHand; // true = right

        private static AssetBundle assetBundle;

        private Transform bodyRoot;
        private Rigidbody bodyRB;
        private readonly List<Rigidbody> ragdollBodies = new();

        private readonly Dictionary<string, Transform> cachedBones = new();

        public void Awake()
        {
            instance = this;
        }

        public static GameObject LoadAsset(string assetName)
        {
            Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("RagdollMod.Resources.ragdoll");

            if (stream == null)
            {
                Debug.LogError("Failed to load asset stream");
                return null;
            }

            if (assetBundle == null)
                assetBundle = AssetBundle.LoadFromStream(stream);

            return Instantiate(assetBundle.LoadAsset<GameObject>(assetName));
        }

        private static List<GameObject> portedCosmetics = new();

        public static void DisableCosmetics()
        {
            try
            {
                VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemLeftShoulder").gameObject.SetActive(false);
                VRRig.LocalRig.transform.Find("rig/body_pivot/TransferrableItemRightShoulder").gameObject.SetActive(false);
                VRRig.LocalRig.transform.Find("rig/head/gorillaface").gameObject.layer = LayerMask.NameToLayer("Default");

                foreach (GameObject c in VRRig.LocalRig.cosmetics)
                {
                    if (c.activeSelf && c.transform.parent == VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"))
                    {
                        portedCosmetics.Add(c);
                        c.transform.SetParent(VRRig.LocalRig.headMesh.transform, false);
                        c.transform.localPosition += new Vector3(0f, 0.1333f, 0.1f);
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

            foreach (GameObject c in portedCosmetics)
            {
                c.transform.SetParent(VRRig.LocalRig.mainCamera.transform.Find("HeadCosmetics"), false);
                c.transform.localPosition -= new Vector3(0f, 0.1333f, 0.1f);
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

            target.excludeLayers = source.excludeLayers;
            target.includeLayers = source.includeLayers;
        }


        public void Die(Vector3 extraVelocity)
        {
            if (Ragdoll != null)
                Destroy(Ragdoll);

            VRRig.LocalRig.enabled = false;
            DisableCosmetics();

            Ragdoll = LoadAsset("ragdoll");
            Rigidbody goodBody = Ragdoll.transform.Find("Stand/Gorilla Rig/body/head").GetComponent<Rigidbody>();
            foreach (Rigidbody Rbody in Ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                CopyRigidbodySettings(Rbody, goodBody);
            }
                SetupSounds(Ragdoll.transform);

            CacheBones();

            Transform body = GetBone("Stand/Gorilla Rig/body");

            body.position = VRRig.LocalRig.transform.Find("rig/body_pivot").position;
            body.rotation = VRRig.LocalRig.transform.Find("rig/body_pivot").rotation;

            SetHandPose("L", VRRig.LocalRig.leftHand.rigTarget);
            SetHandPose("R", VRRig.LocalRig.rightHand.rigTarget);
           
            ApplyInitialVelocity(extraVelocity);

            CacheRagdoll();
            Ragdoll.transform.Find("Stand/Mesh").gameObject.GetComponent<Renderer>().renderingLayerMask = 0;
            startForward = Ragdoll.transform.forward;
        }

        private void CacheBones()
        {
            cachedBones.Clear();

            foreach (Transform t in Ragdoll.GetComponentsInChildren<Transform>(true))
                cachedBones[t.name] = t;
        }

        private Transform GetBone(string path)
        {
            string key = path.Split('/').Last();
            return cachedBones.TryGetValue(key, out var t) ? t : null;
        }

        private void SetHandPose(string side, Transform target)
        {
            string hand = side == "L"
                ? "Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L"
                : "Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R";

            Transform h = GetBone(hand);
            if (h == null) return;

            h.position = target.position;
            h.rotation = target.rotation;
        }

        private void ApplyInitialVelocity(Vector3 extra)
        {
            string[] sets =
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

            foreach (string p in sets)
            {
                var rb = GetBone(p)?.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.linearVelocity = GorillaTagger.Instance.rigidbody.linearVelocity + extra;
            }
        }

        private void CacheRagdoll()
        {
            bodyRoot = GetBone("Stand/Gorilla Rig/body");
            bodyRB = bodyRoot.GetComponent<Rigidbody>();
            ragdollBodies.Clear();
            ragdollBodies.AddRange(Ragdoll.GetComponentsInChildren<Rigidbody>());
        }

        public void Update()
        {
            if (GorillaLocomotion.GTPlayer.Instance == null)
                return;

            if (!isDead && Ragdoll != null)
            {
                VRRig.LocalRig.enabled = true;
                EnableCosmetics();

                Destroy(Ragdoll);
                Ragdoll = null;
                return;
            }

            if (isDead && Ragdoll != null)
            {
                VRRig.LocalRig.enabled = false;
                UpdateRigPos();
            }
        }

        public void SetupSounds(Transform parent)
        {
            Transform[] transforms = parent.GetComponentsInChildren<Transform>();
            foreach (var t in transforms)
                t.AddComponent<Hit>();
        }

        public void UpdateRigPos()
        {
            if (Ragdoll == null || bodyRoot == null) return;

            VRRig.LocalRig.transform.position = bodyRoot.position;
            VRRig.LocalRig.transform.rotation = bodyRoot.rotation;

            Transform l = GetBone("Stand/Gorilla Rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L");
            Transform r = GetBone("Stand/Gorilla Rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R");
            Transform h = GetBone("Stand/Gorilla Rig/body/head");

            if (l && r && h)
            {
                VRRig.LocalRig.leftHand.rigTarget.position = l.position;
                VRRig.LocalRig.rightHand.rigTarget.position = r.position;
                VRRig.LocalRig.head.rigTarget.rotation = h.rotation;
            }

            HandleGrab();
        }

        private void HandleGrab()
        {
            if (GrabbingRig == null)
                DetectGrab();

            if (GrabbingRig == null)
                return;

            var c = VividV2.Classes.Utils.RigUtils.GetCurrentRig(GrabbingRig);

            bool released =
                c.rightIndex.calcT < 0.5f &&
                c.rightMiddle.calcT < 0.5f &&
                c.leftIndex.calcT < 0.5f &&
                c.leftMiddle.calcT < 0.5f;

            if (released)
            {
                GrabbingRig = null;
                GrabBody = null;
                return;
            }

            if (GrabBody == null || bodyRB == null)
                return;

            Vector3 target = GrabHand
                ? c.rightHandTransform.position
                : c.leftHandTransform.position;

            GrabBody.linearVelocity = (target - bodyRB.position) * 25f;
        }

        private void DetectGrab()
        {
            foreach (VRRig rig in VividV2.Classes.Utils.RigUtils.GetVRRigs())
            {
                var c = VividV2.Classes.Utils.RigUtils.GetCurrentRig(rig);

                foreach (var body in ragdollBodies)
                {
                    float rDist = Vector3.Distance(c.rightHandTransform.position, body.position);
                    float lDist = Vector3.Distance(c.leftHandTransform.position, body.position);

                    bool right =
                        rDist < 0.3f &&
                        (c.rightIndex.calcT > 0.5f || c.rightMiddle.calcT > 0.5f);

                    bool left =
                        lDist < 0.3f &&
                        (c.leftIndex.calcT > 0.5f || c.leftMiddle.calcT > 0.5f);

                    if (right)
                    {
                        GrabbingRig = c;
                        GrabBody = body;
                        GrabHand = true;
                        return;
                    }

                    if (left)
                    {
                        GrabbingRig = c;
                        GrabBody = body;
                        GrabHand = false;
                        return;
                    }
                }
            }
        }

        public void EnableRagdoll(Vector3 extraVelocity)
        {
            if (isDead) return;

            isDead = true;
            Die(extraVelocity);
        }

        public void DisableRagdoll()
        {
            if (!isDead) return;

            isDead = false;

            if (Ragdoll != null)
            {
                VRRig.LocalRig.enabled = true;
                EnableCosmetics();

                Destroy(Ragdoll);
                Ragdoll = null;
            }
        }
    }
}