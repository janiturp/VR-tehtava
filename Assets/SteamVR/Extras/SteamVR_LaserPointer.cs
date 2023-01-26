//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;

namespace Valve.VR.Extras
{
    public class SteamVR_LaserPointer : MonoBehaviour
    {
        public SteamVR_Behaviour_Pose pose;

        //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
        public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");
        //public SteamVR_Action_Boolean gripObject = SteamVR_Input.GetBooleanAction("GrabGrip");

        public bool active = true;
        public Color color;
        public float thickness = 0.002f;
        public Color clickColor = Color.green;
        public GameObject holder;
        public GameObject pointer;
        bool isActive = false;
        public bool addRigidBody = false;
        public Transform reference;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public event PointerEventHandler PointerClick;
        public event PointerEventHandler PointerDown;
        public event PointerEventHandler PointerUp;

        // Added by Jani
        bool isDown = false;
        public Ray CurrentRayR;
        public Ray CurrentRayL;
        public GameObject leftHandRayLockGo;
        public GameObject rightHandRayLockGo;
        public float comparedControllerAngleR = 0.0f;
        public float comparedControllerAngleL = 0.0f;
        public GameObject rightHand;
        public GameObject leftHand;
        // End

        Transform previousContact = null;


        private void Start()
        {

            if (pose == null)
                pose = this.GetComponent<SteamVR_Behaviour_Pose>();
            if (pose == null)
                Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

            if (interactWithUI == null)
                Debug.LogError("No ui interaction action has been set on this component.", this);


            holder = new GameObject();
            holder.transform.parent = this.transform;
            holder.transform.localPosition = Vector3.zero;
            holder.transform.localRotation = Quaternion.identity;

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, 100f);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;

        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);

            // Added by Jani
            // Check if either hand's laser targets Grabbable object. rightHandRayLockGo keeps track of the targeted object.
            if (e.fromInputSource == SteamVR_Input_Sources.RightHand && e.target.CompareTag("Grab") && rightHand != null)
            {
                rightHandRayLockGo = e.target.gameObject;
                comparedControllerAngleR = rightHand.transform.eulerAngles.x;
            }
            else if (e.fromInputSource == SteamVR_Input_Sources.LeftHand && e.target.CompareTag("Grab") && leftHand != null)
            {
                leftHandRayLockGo = e.target.gameObject;
                comparedControllerAngleL = leftHand.transform.eulerAngles.x;
            }
            // End
        }

        public virtual void OnPointerClick(PointerEventArgs e)
        {
            if (PointerClick != null)
                PointerClick(this, e);

        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);


            // Added by Jani. Checks if LaserPointer's Ray moves away from grabbable object.
            comparedControllerAngleR = 0.0f;
            comparedControllerAngleL = 0.0f;

            if (rightHandRayLockGo)
            {
                rightHandRayLockGo = null;
            }

            if(leftHandRayLockGo)
            {
                leftHandRayLockGo = null;
            }
        }

        // Added by Jani. Ended up being useless.
        public virtual void OnPointerDown(PointerEventArgs e)
        {
            if (PointerDown != null)
            {
                PointerDown(this, e);
            }

        }

        // Added by Jani. Ended up being useless.
        public virtual void OnPointerUp(PointerEventArgs e)
        {
            if (PointerUp != null)
            {
                PointerUp(this, e);
            }
        }

        private void Update()
        {
            if (!isActive)
            {
                isActive = true;
                this.transform.GetChild(0).gameObject.SetActive(true);
            }

            float dist = 100f;

            Ray raycast = new Ray(transform.position, transform.forward);

            // Added and modified by Jani
            // Raycasts for left and right hand.
            CurrentRayR = raycast;
            CurrentRayL = raycast;
            // End

            RaycastHit hit;
            bool bHit = Physics.Raycast(raycast, out hit);

            if (previousContact && previousContact != hit.transform)
            {
                PointerEventArgs args = new PointerEventArgs();
                args.fromInputSource = pose.inputSource;
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
            }
            if (bHit && previousContact != hit.transform)
            {
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.fromInputSource = pose.inputSource;
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;
            }
            if (!bHit)
            {
                previousContact = null;
            }
            if (bHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }

            if (bHit && interactWithUI.GetStateUp(pose.inputSource))
            {
                PointerEventArgs argsClick = new PointerEventArgs();
                argsClick.fromInputSource = pose.inputSource;
                argsClick.distance = hit.distance;
                argsClick.flags = 0;
                argsClick.target = hit.transform;
                OnPointerClick(argsClick);
                OnPointerUp(argsClick);
                isDown = false;
            }

            if (interactWithUI != null && interactWithUI.GetState(pose.inputSource))
            {
                pointer.transform.localScale = new Vector3(thickness * 5f, thickness * 5f, dist);
                pointer.GetComponent<MeshRenderer>().material.color = clickColor;

                // Added by Jani. Ended up being useless.
                if (!isDown)
                {
                    isDown = true;
                    PointerEventArgs argsIn = new PointerEventArgs();
                    argsIn.fromInputSource = pose.inputSource;
                    argsIn.distance = hit.distance;
                    argsIn.flags = 0;
                    argsIn.target = hit.transform;
                    OnPointerDown(argsIn);
                }
            }
            else
            {
                pointer.transform.localScale = new Vector3(thickness, thickness, dist);
                pointer.GetComponent<MeshRenderer>().material.color = color;
            }
            pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);
        }

        // Everything inside FixedUpdate was added by Jani.
        private void FixedUpdate()
        {
            // Check the angle of hand. If hand is moved in a correct way, targeted grabbable object's position is changed
            // to the end of ray in 4.7f distance. Arbritray distance chosen.
            if (rightHandRayLockGo != null && rightHand.transform.eulerAngles.x >= comparedControllerAngleR + 30.0f)
            {
                Ray ray = GetComponent<SteamVR_LaserPointer>().CurrentRayR;
                Vector3 pos = ray.origin + (ray.direction * 4.7f);
                rightHandRayLockGo.transform.position = pos;

            }
            // If hand's angle moves to the opposite direction of the if-sentence, the targeted grabbable object
            // is dropped and untargeted.
            else if (rightHandRayLockGo && rightHand.transform.eulerAngles.x <= comparedControllerAngleR - 30.0f)
            {
                rightHandRayLockGo.transform.position = rightHandRayLockGo.transform.position;
                rightHandRayLockGo = null;
            }

            // Same as above, but for left hand.
            if (leftHandRayLockGo != null && leftHand.transform.eulerAngles.x >= comparedControllerAngleL + 30.0f)
            {
                Ray ray = GetComponent<SteamVR_LaserPointer>().CurrentRayL;
                Vector3 pos = ray.origin + (ray.direction * 4.7f);
                leftHandRayLockGo.transform.position = pos;

            }
            else if (leftHandRayLockGo && leftHand.transform.eulerAngles.x <= comparedControllerAngleL - 30.0f)
            {
                leftHandRayLockGo.transform.position = leftHandRayLockGo.transform.position;
                leftHandRayLockGo = null;
            }
        }
    }

    public struct PointerEventArgs
    {
        public SteamVR_Input_Sources fromInputSource;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);
}