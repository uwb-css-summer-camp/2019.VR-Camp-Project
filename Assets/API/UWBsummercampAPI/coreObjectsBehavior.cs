using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Valve.VR.InteractionSystem;
using Valve.VR;
using UnityEditor;
using TMPro;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


/// <summary>
/// API for UWB VR Summer Camp
/// </summary>
namespace UWBsummercampAPI
{
    /// <summary>
    /// Parent class for objects created with Creation Window.
    /// Allows access to API methods.
    /// </summary>
    public class coreObjectsBehavior : MonoBehaviour
    {           
        private bool shouldSpinFlag = false;
        private bool hasTimer = false;
        private bool isRightHandTethered = false;
        private bool isLeftHandTethered = false;
        public bool isGrabbed = false;
        private bool isGameFinished = false;
        private float gameFinishedTimer = 0;
        private bool setDestroyObj = false;
        private bool isHandTouching = false;
        private bool isLeftHandTouching = false;
        private bool isRightHandTouching = false;

        private bool playerCollison = false;
        private bool releaseAsThrowable = false;

        /// <summary>
        /// Reference to the object this object is currently colliding with
        /// </summary>
        protected GameObject interactionObj = null;

        /// <summary>
        /// Reference to the player gameobject
        /// </summary>
        protected GameObject playerObj;
        private float timer = 0;
        private float timerMax = 10;
        private bool timerFinished = false;
        //TODO check why rigidbody is getting message
        private Rigidbody rigidbody;
        private float objectTextHeight = .5f;
        private float objectTextFontSize = 5;

        //used for calcuating 3rd person movement (tether object)
        private Vector3 diffVector;
        private Vector3 perpVector;

        //used for calculating random numbers (getRandomNumber() method) 
        private System.Random rand;

        //get state of controller trigger action
        [SteamVR_DefaultAction("GrabPinch", "default")]
        private SteamVR_Action_Boolean triggerAction;

        //get state of controller grip action
        [SteamVR_DefaultAction("GrabGrip", "default")]
        private SteamVR_Action_Boolean gripAction;
      
        //reference to player hand objects (VR Controllers)
        private Hand rightHand;
        private Hand leftHand;

        // Start is called before the first frame update
        private void Start()
        {
            //initialize variables
            rand = new System.Random();
            rigidbody = GetComponent<Rigidbody>();
            triggerAction = SteamVR_Input.__actions_default_in_GrabPinch;
            gripAction = SteamVR_Input.__actions_default_in_GrabGrip;
            playerObj = GameObject.FindGameObjectWithTag("Player");
            leftHand = Valve.VR.InteractionSystem.Player.instance.leftHand;
            rightHand = Valve.VR.InteractionSystem.Player.instance.rightHand;

            //invoke child buildGame method
            MethodInfo method = this.GetType().GetMethod("buildGame", BindingFlags.Instance |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null)
            {
                method.Invoke(this, new object[0]);
            }
        }

        // Update is called once per frame
        void Update()
        {
            //tried to destroy a grabbed object but cannot do so until the player has released the object.
            if(setDestroyObj && !isObjectGrabbed())
            {
                setDestroyObj = false;
                Destroy(gameObject);
            }

            //handle timer logic
            if (hasTimer)
            {
                if(timer > timerMax)
                {
                    hasTimer = false;
                    timer = 0;
                }
                else
                {
                    timer += Time.deltaTime;
                }
            }


            //invoke child updateGame method
            MethodInfo method = this.GetType().GetMethod("updateGame", BindingFlags.Instance |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null)
            {
                method.Invoke(this, new object[0]);
            }
            
            //End game logic - waits 5 seconds after end game trigger
            // then resets the scene
            if(isGameFinished)
            {
                gameFinishedTimer += Time.deltaTime;
                if(gameFinishedTimer > 5)
                {
                    isGameFinished = false;
                    gameFinishedTimer = 0;
                    takePoints(checkPoints());
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }

            //handle spinning object logic
            if (shouldSpinFlag)
            {
                transform.Rotate(0, 20 * Time.deltaTime, 0);
            }

            //If object is tethered, rotation of the controller will move this object
            //Object is moved if controller is moved on the X axis or the Z axis
            if (isRightHandTethered)
            {
                //TODO check tethered logic once more
                Quaternion handRotation = rightHand.trackedObject.poseAction.GetLastLocalRotation(rightHand.handType);
                Vector3 playerPos = playerObj.transform.position;
                Vector3 thisPos = transform.position;
                diffVector = playerPos - thisPos;
                perpVector = Vector3.Cross(diffVector, Vector3.up);
                diffVector.Normalize();
                perpVector.Normalize();
                Vector3 newVelocity = (handRotation.x * 10 * diffVector) + (handRotation.z * 10 * perpVector);
                rigidbody.velocity = new Vector3(-newVelocity.x, rigidbody.velocity.y, newVelocity.z);
            }

            //check if left hand is tethered
            if(isLeftHandTethered)
            {
                //TODO check tethered logic once more
                Quaternion handRotation = leftHand.trackedObject.poseAction.GetLastLocalRotation(leftHand.handType);
                Vector3 playerPos = playerObj.transform.position;
                Vector3 thisPos = transform.position;
                diffVector = playerPos - thisPos;

                //Vector2 perpVector2D = new Vector2(-diffVector.z, diffVector.x);
                perpVector = Vector3.Cross(diffVector, Vector3.up);
                diffVector.Normalize();
                perpVector.Normalize();
                Vector3 newVelocity = (handRotation.x * 10 * diffVector) + (handRotation.z * 10 * perpVector);
                rigidbody.velocity = new Vector3(-newVelocity.x, rigidbody.velocity.y, newVelocity.z);
            }

            //if object set to not throwable when the object was held, make not throwable
            // when the object is released
            if (releaseAsThrowable && !isGrabbed)
            {
                Throwable throwable = GetComponent<Throwable>();
                Destroy(throwable);
                releaseAsThrowable = false;
            }
        }



        #region Controller Bindings

        /// <summary>
        /// Checks if the right or left controller trigger is held down. 
        /// Continuously returns true as long as the trigger is down.
        /// </summary>
        /// <returns>True if left or right trigger is held down</returns>
        public bool isControllerTriggerDown()
        {
            return isControllerRightTriggerDown() || isControllerLeftTriggerDown();
        }

        /// <summary>
        /// Checks if controller trigger is held down. 
        /// Continuously returns true as long as trigger is down.
        /// </summary>
        /// <returns>True if left trigger is down</returns>
        public bool isControllerLeftTriggerDown()
        {
            //(for testing w/o VR headset)
            if(Input.GetKey(KeyCode.LeftBracket)) { return true; }

            bool triggerBlocked = coreManagerBehavior.isAnyObjectHovered || coreManagerBehavior.isAnyObjectGrabbed;
            return triggerAction.GetState(leftHand.handType) && !triggerBlocked;
        }

        /// <summary>
        /// Checks if right controller trigger is held down.
        /// Continuously returns true as long as the trigger is down.
        /// </summary>
        /// <returns>True if the right controller trigger is down</returns>
        public bool isControllerRightTriggerDown()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.RightBracket)) { return true; }

            bool triggerBlocked = coreManagerBehavior.isAnyObjectHovered || coreManagerBehavior.isAnyObjectGrabbed;
            return triggerAction.GetState(rightHand.handType) && !triggerBlocked;
        }

        /// <summary>
        /// Checks if right or left trigger was clicked. Returns true only once for 
        /// each trigger clicked event.
        /// </summary>
        /// <returns>True if a trigger was not clicked last frame, 
        /// but is clicked in current frame</returns>
        public bool isControllerTriggerClicked()
        {
            return isControllerLeftTriggerClicked() || isControllerRightTriggerClicked();
        }

        /// <summary>
        /// Checks if left trigger was clicked. Returns true only once for
        /// each trigger click event.
        /// </summary>
        /// <returns>True if left trigger was not clicked last frame, 
        /// but is clicked in the current frame</returns>
        public bool isControllerLeftTriggerClicked()
        {
            //(for testing w/o VR headset)
            if (Input.GetKeyDown(KeyCode.LeftBracket)) { return true; }

            bool triggerBlocked = coreManagerBehavior.isAnyObjectHovered || coreManagerBehavior.isAnyObjectGrabbed;
            return triggerAction.GetStateDown(leftHand.handType) && !triggerBlocked;
        }

        /// <summary>
        /// Checks if right trigger was clicked. Returns true only once for
        /// each trigger click event.
        /// </summary>
        /// <returns>True if right trigger was not clicked last frame,
        /// but is clicked in the current frame</returns>
        public bool isControllerRightTriggerClicked()
        {
            //(for testing w/o VR headset)
            if (Input.GetKeyDown(KeyCode.RightBracket)) { return true; }

            bool triggerBlocked = coreManagerBehavior.isAnyObjectHovered || coreManagerBehavior.isAnyObjectGrabbed;
            return triggerAction.GetStateDown(rightHand.handType) && !triggerBlocked;
        }

        /// <summary>
        /// Check if side grips of right or left controller are held down. 
        /// Continuously returns true as long as the grips are held down.
        /// </summary>
        /// <returns>True as long as right or left controller grip is held down</returns>
        public bool isControllerGripDown()
        {
            return isControllerRightGripDown() || isControllerLeftGripDown();
        }

        /// <summary>
        /// Check if side grips of right controller are held down.
        /// Continuously returns true as long as the grips are held down.
        /// </summary>
        /// <returns>True if the right controller side grips are held down</returns>
        public bool isControllerRightGripDown()
        {
            //(for testing w/o VR headset)
            if(Input.GetKey(KeyCode.Quote)) { return true; }

            return gripAction.GetState(rightHand.handType);
        }

        /// <summary>
        /// Check if side grips of left controller are held down.
        /// Continuously returns true as long as the grips are held down.
        /// </summary>
        /// <returns>True if the left controller side grips are held down</returns>
        public bool isControllerLeftGripDown()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.Semicolon)) { return true; }

            return gripAction.GetState(leftHand.handType);
        }

        /// <summary>
        /// Check if the right or left controller is touching (colliding with) this object
        /// </summary>
        /// <returns>True if right or left controller are colliding with this object</returns>
        public bool isControllerTouching()
        {
            return isHandTouching;
        }

        /// <summary>
        /// Check if the right controller is touching (colliding with) this object
        /// </summary>
        /// <returns>True if right controller is colliding with this object</returns>
        public bool isControllerRightTouching()
        {
            return isRightHandTouching;
        }

        /// <summary>
        /// Check if the left controller is touching (colliding with) this object
        /// </summary>
        /// <returns>True if left controller is colliding with this object</returns>
        public bool isControllerLeftTouching()
        {
            return isLeftHandTouching;
        }

        /// <summary>
        /// Check if the right controller is tilted up (along x-axis)
        /// </summary>
        /// <returns>True if right controller is tilted up</returns>
        public bool isControllerRightTiltedUp()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.O)) { return true; }

            Quaternion handRotation = rightHand.trackedObject.poseAction.GetLastLocalRotation(rightHand.handType);
            return handRotation.x < -.4 && handRotation.x > -.8;
 
        }

        /// <summary>
        /// Check if right controller is tiled down (on the x-axis)
        /// </summary>
        /// <returns>True if right controller is tilted down</returns>
        public bool isControllerRightTiltedDown()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.Comma)) { return true; }

            Quaternion handRotation = rightHand.trackedObject.poseAction.GetLastLocalRotation(rightHand.handType);
            return handRotation.x > .4 && handRotation.x < .8;
        }

        /// <summary>
        /// Check if right controller is tilted right (on the z-axis)
        /// </summary>
        /// <returns>True if right controller is tilted right</returns>
        public bool isControllerRightTiltedRight()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.L)) { return true; }

            Quaternion handRotation = rightHand.trackedObject.poseAction.GetLastLocalRotation(rightHand.handType);
            return handRotation.z < -.4 && handRotation.z > -.8;
        }


        /// <summary>
        /// Check if right controller is tilted left (on the z-axis)
        /// </summary>
        /// <returns>True if the right controller is tilted left</returns>
        public bool isControllerRightTiltedLeft()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.K)) { return true; }

            Quaternion handRotation = rightHand.trackedObject.poseAction.GetLastLocalRotation(rightHand.handType);
            return handRotation.z > .4 && handRotation.z < .8;
        }

        /// <summary>
        /// Check if left controller is tilted up (on the x-axis)
        /// </summary>
        /// <returns>True if left controller is tilted up</returns>
        public bool isControllerLeftTiltedUp()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.U)) { return true; }

            Quaternion handRotation = leftHand.trackedObject.poseAction.GetLastLocalRotation(leftHand.handType);
            return handRotation.x < -.4 && handRotation.x > -.8;
        }

        /// <summary>
        /// Check if left controller is tilted down (on the x-axis)
        /// </summary>
        /// <returns>True if left controller is tilted down</returns>
        public bool isControllerLeftTiltedDown()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.N)) { return true; }

            Quaternion handRotation = leftHand.trackedObject.poseAction.GetLastLocalRotation(leftHand.handType);
            return handRotation.x > .4 && handRotation.x < .8;
        }

        /// <summary>
        /// Check if left controller is tilted right (on the z-axis)
        /// </summary>
        /// <returns>True if left controller is tilted right</returns>
        public bool isControllerLeftTiltedRight()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.J)) { return true; }

            Quaternion handRotation = leftHand.trackedObject.poseAction.GetLastLocalRotation(leftHand.handType);
            return handRotation.z < -.4 && handRotation.z > -.8;
        }

        /// <summary>
        /// Check if left controller is tilted left (on the z-axis)
        /// </summary>
        /// <returns>True if left controller is tilted left</returns>
        public bool isControllerLeftTiltedLeft()
        {
            //(for testing w/o VR headset)
            if (Input.GetKey(KeyCode.H)) { return true; }

            Quaternion handRotation = leftHand.trackedObject.poseAction.GetLastLocalRotation(leftHand.handType);
            return handRotation.z > .4 && handRotation.z < .8;
        }

        /// <summary>
        /// Check if this object is currently held by the player.
        /// The player holds an object when a controller collides with
        /// a throwable object and holds a controller trigger.
        /// </summary>
        /// <returns>True if this object is currently held by the player</returns>
        public bool isObjectGrabbed()
        {
            return isGrabbed;
        }

        /// <summary>
        /// Sets or unsets an object to tether to the right controller.
        /// A tethered object will move with the tilt movements of the controller.
        /// Tilting the controller up/down (x-axis) will move the tethered object to/from the player.
        /// Tilting the controller right/left (z-axis) will move the tethered object orthogonal (perpendicular) to the player.
        /// </summary>
        /// <param name="tetherValue">True to tether this object to right controller. False to remove tethering.</param>
        public void setTetherObjectToRightController(bool tetherValue)
        {
            if(gameObject.tag == "Empty") { return; }

            if(tetherValue)
            {
                if (!isRightHandTethered)
                {
                    rigidbody.isKinematic = false;
                    Vector3 playerPos = playerObj.transform.position;
                    Vector3 thisPos = transform.position;
                    diffVector = playerPos - thisPos;
                    perpVector = Vector3.Cross(diffVector, Vector3.up);
                    diffVector.Normalize();
                    perpVector.Normalize();
                }
                isRightHandTethered = true;
            }
            else
            {
                isRightHandTethered = false;
            }
        }


        /// <summary>
        /// Sets or unsets an object to tether to the left controller.
        /// A tethered object will move with the tilt movements of the controller.
        /// Tilting the controller up/down (x-axis) will move the tethered object to/from the player.
        /// Tilting the controller right/left (z-axis) will move the tethered object orthogonal (perpendicular) to the player.
        /// </summary>
        /// <param name="tetherValue">True to tether this object to left controller. False to remove tethering.</param>
        public void setTetherObjectToLeftController(bool tetherValue)
        {
            if(gameObject.tag == "Empty") { return; }

            if (tetherValue)
            {
                if (!isLeftHandTethered)
                {
                    rigidbody.isKinematic = false;
                    Vector3 playerPos = playerObj.transform.position;
                    Vector3 thisPos = transform.position;

                    diffVector = playerPos - thisPos;
                    perpVector = Vector3.Cross(playerPos, thisPos);
                    diffVector.Normalize();
                    perpVector.Normalize();
                }
                isLeftHandTethered = true;
            }
            else
            {
                isLeftHandTethered = false;
            }
        }

        /// <summary>
        /// Check if this object is currently tethered to a controller
        /// </summary>
        /// <returns>True if object is tethered to a controller</returns>
        public bool isObjectTetheredToController()
        {
            return isRightHandTethered || isLeftHandTethered;
        }
        #endregion

        #region New API Methods
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                playerCollison = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(other.tag == "Player"){
                playerCollison = false;   
            }
        }
        /// <summary>
        /// Check if the player is colliding wlith this object. 
        /// Continuously returns true as long as the player is colliding with the object.
        /// </summary>
        /// <param name="objectName">The name of the object to check for collision with the player</param>
        /// <returns>True if the player and the specified object are colliding</returns>
        public bool hasPlayerCollisionWithThisObject()
        {
            if(gameObject.tag == "Empty") { return false; }

            if (playerCollison)
            {
                return true;
            }
            return false;
        }
            /// <summary>
            /// Check if the player is colliding, or standing on, the specified object. 
            /// Continuously returns true as long as the player is colliding with the object.
            /// </summary>
            /// <param name="objectName">The name of the object to check for collision with the player</param>
            /// <returns>True if the player and the specified object are colliding</returns>
            public bool hasPlayerCollisionWithObject(string objectName)
        {
            if (gameObject.tag == "Empty") { return false; }

            if (playerCollison)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if a collision has occurred between the two specified objects
        /// Returns true once per collision.
        /// </summary>
        /// <param name="object1">The first object to check for the collision</param>
        /// <param name="object2">The second object to check for the collision</param>
        /// <returns>True if the two specified objects have collided</returns>
        public bool hasCollisionBetweenObjects(string object1, string object2)
        {
            if (gameObject.tag == "Empty") { return false; }

            foreach (CollisionEvent e in coreManagerBehavior.collisionList)
            {
                if (e.object1 == object1 && e.object2 == object2 ||
                    e.object2 == object1 && e.object1 == object2)
                {
                    //check if this object has accessed this collision event already
                    foreach (int i in e.idList)
                    {
                        if (i == gameObject.GetInstanceID())
                        {
                            return false;
                        }
                    }
                    e.idList.Add(gameObject.GetInstanceID());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Set the player position relative to the player's current position
        /// </summary>
        /// <param name="changeX">The change in X position</param>
        /// <param name="changeY">The change in Y position</param>
        /// <param name="changeZ">The change in Z position</param>
        public void setPlayerPositionRelative(float changeX, float changeY, float changeZ)
        {
            Vector3 currPos = playerObj.transform.position;
            playerObj.transform.position = new Vector3(currPos.x + changeX, currPos.y + changeY, currPos.z + changeZ);
        }

        /// <summary>
        /// Set a new player position (not relative to current position)
        /// </summary>
        /// <param name="newX">The new X player position</param>
        /// <param name="newY">The new Y player position</param>
        /// <param name="newZ">The new Z player position</param>
        public void setPlayerPositionAbsolute(float newX, float newY, float newZ)
        {
            playerObj.transform.position = new Vector3(newX, newY, newZ);
        }

        //sets properties when object is grabbed
        private void onGrabbed()
        {
            isGrabbed = true;
            coreManagerBehavior.isAnyObjectGrabbed = true;
        }

        //sets properties when object is released
        private void onReleased()
        {
            isGrabbed = false;
            coreManagerBehavior.isAnyObjectGrabbed = false;
        }

        ///<summary>
        /// Set velocity for this object
        /// </summary>
        /// <param name="x">New velocity for X direction</param>
        /// <param name="y">New velocity for Y direction</param>
        /// <param name="z">New velocity for Z direction</param>
        public void setObjectVelocity(float x, float y, float z)
        {
            if (gameObject.tag == "Empty") { return; }

            if (rigidbody.isKinematic)
            {
                rigidbody.isKinematic = false;
            }
            rigidbody.velocity = new Vector3(x, y, z);
        }

        /// <summary>
        /// Set velocity for this object
        /// </summary>
        /// <param name="newVelocity">Vector of new object velocity</param>
        public void setObjectVelocity(Vector3 newVelocity)
        {
            if (gameObject.tag == "Empty") { return; }

            if (rigidbody.isKinematic)
            {
                rigidbody.isKinematic = false;
            }
            rigidbody.velocity = newVelocity;
        }

        /// <summary>
        /// Create a new prefab GameObject
        /// </summary>
        /// <param name="location">The vector location of the GameObject</param>
        /// <param name="prefabName">The prefab name of the GameObject to create</param>
        /// <param name="objName">Optional: New name for the GameObject (otherwise defaults to prefab name)</param>
        /// <returns>Reference to newly created object</returns>
        public GameObject createNewObject(Vector3 location, string prefabName, string objName=null)
        {
            return createNewObject(location.x, location.y, location.z, prefabName, objName);
        }

        /// <summary>
        /// Create a new prefab GameObject
        /// </summary>
        /// <param name="x">The X location of the new GameObject</param>
        /// <param name="y">The Y location of the new GameObject</param>
        /// <param name="z">The Z location of the new GameObject</param>
        /// <param name="prefabName">The prefab name of the GameObject to create</param>
        /// <param name="objectName">Optional: New name for the GameObject (otherwise defaults to prefab name)</param>
        /// <returns>Reference to the newly created object</returns>
        public GameObject createNewObject(float x, float y, float z, string prefabName, string objName=null)
        {
            string prefabPath = "";
            string[] searchFolders = new string[1];
            searchFolders[0] = "Assets/Resources";
            string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                int pathLastIndex = path.LastIndexOf("/") > path.LastIndexOf("\\") ?
                    path.LastIndexOf("/") : path.LastIndexOf("\\");

                string currName = path.Substring(pathLastIndex + 1).Split('.').First();
                if (currName == prefabName)
                {
                    prefabPath = path;
                    //get rid of "Assets/Resources/" at beggining of path
                    prefabPath = prefabPath.Substring(prefabPath.IndexOf("/") + 1);
                    prefabPath = prefabPath.Substring(prefabPath.IndexOf("/") + 1);
                    prefabPath = prefabPath.Split('.').First();
                    break;
                }
            }

            if (prefabPath != "")
            {
                GameObject newObj = Instantiate(Resources.Load(prefabPath)) as GameObject;
                newObj.transform.localPosition = new Vector3(x, y, z);
                if(objName != null)
                {
                    newObj.name = objName;
                }
                return newObj;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Destroy this object instance.
        /// </summary>
        public void destroyThisObject()
        {
            if(isObjectGrabbed())
            {
                setDestroyObj = true;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Destroys a single GameObject of the object name specified. If more than one object with the
        /// provided name exists, this destroys the first instance found.
        /// </summary>
        /// <param name="objectName">The name of the GameObject to destroy</param>
        public void destroySingleObject(string objectName)
        {
            GameObject destroyObj = null;

            if(destroyObj = GameObject.Find(objectName))
            {
                Destroy(destroyObj);
            }
        }

        /// <summary>
        /// Destroys all GameObjects of the object name specified.
        /// </summary>
        /// <param name="objectName">The name of the GameObject(s) to destroy</param>
        public void destroyAllObjects(string objectName)
        {
            GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();

            foreach(GameObject obj in objs)
            {
                if(obj.name == objectName)
                {
                    Destroy(obj);
                }
            }
           
            //clean up teleportMarker array in teleport.cs so that it does not include destroyed objects
            GameObject teleportObj = GameObject.FindGameObjectWithTag("teleporting");
            teleportObj.GetComponent<Teleport>().teleportMarkers = GameObject.FindObjectsOfType<TeleportMarkerBase>();
        }

        /// <summary>
        /// Set a new size for this GameObject. Current size before change does not affect new size.
        /// </summary>
        /// <param name="newX">New X value size of object</param>
        /// <param name="newY">New Y value size of object</param>
        /// <param name="newZ">New Z value size of object</param>
        public void setObjectSizeAbsolute(float newX, float newY, float newZ)
        {
            if (gameObject.tag == "Empty") { return; }

            transform.localScale = new Vector3(newX, newY, newZ);
        }

        /// <summary>
        /// Change this object size. The new object size is the sum of the current size plus the values provied.
        /// </summary>
        /// <param name="changeX">The X-axis value change in object size</param>
        /// <param name="changeY">The Y-axis value change in object size</param>
        /// <param name="changeZ">The Z-axis value change in object size</param>
        public void setObjectSizeRelative(float changeX, float changeY, float changeZ)
        {
            if (gameObject.tag == "Empty") { return; }

            Vector3 currentScale = transform.localScale;

            //make sure change doesn't make values go below zero (for negative arguments)
            if (currentScale.x + changeX > 0 && currentScale.y + changeX > 0 &&
                currentScale.z + changeZ > 0)
            {
                transform.localScale += new Vector3(changeX, changeY, changeZ);
            }
        }

        /// <summary>
        /// Change this object's position. The new position of the object is the value of the current position added to
        /// the change in position specified.
        /// </summary>
        /// <param name="changeX">The X-axis change in position</param>
        /// <param name="changeY">The Y-axis change in position</param>
        /// <param name="changeZ">The Z-axis change in position</param>
        public void setObjectPositionRelative(float changeX, float changeY, float changeZ)
        {
            if (gameObject.tag == "Empty") { return; }

            transform.position += new Vector3(changeX, changeY, changeZ);
        }

        /// <summary>
        /// Set a new position for this object. The new position is not affected by the object's current position.
        /// </summary>
        /// <param name="newX">The new X-axis position</param>
        /// <param name="newY">The new Y-axis position</param>
        /// <param name="newZ">The new Z-axis position</param>
        public void setObjectPositionAbsolute(float newX, float newY, float newZ)
        {
            if (gameObject.tag == "Empty") { return; }

            transform.position = new Vector3(newX, newY, newZ);
        }

        /// <summary>
        /// Start a timer for this object, the length of the timer specified in the argument.
        /// To check if the timer is finished, use the isTimerFinished() method.
        /// </summary>
        /// <param name="seconds">The seconds length of the timer</param>
        public void startTimer(float seconds)
        {
            hasTimer = true;
            timer = 0;
            timerMax = seconds;
        }

        /// <summary>
        /// Check if the timer set by the startTimer() method has finished. 
        /// Returns true only in the single frame that the timer finished.
        /// </summary>
        /// <returns>Return true if the timer finished this frame. False otherwise.</returns>
        public bool isTimerFinished()
        {
            if (!hasTimer)
            {
                return false;
            }

            if (timer >= timerMax)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set whether the object uses gravity and is subject to environmental physics.
        /// </summary>
        /// <param name="gravityValue">True to set gravity. False to unset gravity.</param>
        public void setObjectGravity(bool gravityValue)
        {
            if (gameObject.tag == "Empty") { return; }

            rigidbody.isKinematic = !gravityValue;
            rigidbody.useGravity = gravityValue;
        }

        //TODO reasses this method--  need to analyze to see how it affects object behavior
        /// <summary>
        /// Set the mass of this object. 
        /// </summary>
        /// <param name="gravityValue">The mass value of this object</param>
        public void setObjectGravityIntensity(float gravityValue)
        {
            if (gameObject.tag == "Empty") { return; }

            rigidbody.mass = gravityValue;
        }


        /// <summary>
        /// Set whether this object may be grabbed and thrown.
        /// An object set as throwable has gravity set by default.
        /// </summary>
        /// <param name="isThrowable">True to set the object as throwable. False to unset as throwable.</param>
        public void setObjectThrowable(bool isThrowable)
        {
            if (gameObject.tag == "Empty") { return; }

            if (isThrowable)
            {
                rigidbody.isKinematic = false;
                if (GetComponent<Interactable>() == null)
                {
                    gameObject.AddComponent<Interactable>();
                }
                if (GetComponent<Throwable>() == null)
                {
                    gameObject.AddComponent<Throwable>();
                }
                gameObject.GetComponent<Throwable>().onPickUp = new UnityEvent();
                gameObject.GetComponent<Throwable>().onPickUp.AddListener(onGrabbed);
                gameObject.GetComponent<Throwable>().onDetachFromHand = new UnityEvent();
                gameObject.GetComponent<Throwable>().onDetachFromHand.AddListener(onReleased);
            }
            else
            {
                if (GetComponent<Throwable>() == null)
                {
                    return;
                }
                else if (isGrabbed)
                {
                    //cannot make non-throwable if the object is currently held
                    //wait until object is not grabbed to make not throwable
                    releaseAsThrowable = true;
                }
                else
                {
                    Throwable throwable = GetComponent<Throwable>();
                    Destroy(throwable);
                }
            }
        }

        /// <summary>
        /// Set a new name for this GameObject
        /// </summary>
        /// <param name="newName">The new name for this GameObject</param>
        public void setObjectName(string newName)
        {
            gameObject.name = newName;
        }

        /// <summary>
        /// Check if this object has collided with the other object specified.
        /// Only returns true once for each collision instance.
        /// </summary>
        /// <param name="objName">The other GameObject to check for collision with this object</param>
        /// <returns>True if this object is colliding with the other object and this collision instance
        /// has not already been checked by this object</returns>
        public bool hasCollisionWithOtherObject(string objName)
        {
            if (gameObject.tag == "Empty") { return false; }

            if (interactionObj != null)
            {
                if (interactionObj.name == objName)
                {
                    interactionObj = null;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get a random number up to (and including) the specified max value.
        /// Returns 0 if the max value specified is a negative number.
        /// </summary>
        /// <param name="maxValue">The maximum value that may be randomly returned.</param>
        /// <returns>A random number from 0 to the maximum value specified.</returns>
        public int getRandomNumber(int maxValue)
        {
            if(maxValue <= 0)
            {
                return 0;
            }
            return rand.Next(maxValue);
        }

        /// <summary>
        /// Set text to display directly above this object. 
        /// If the text to display is long, the text height and font size may need to 
        /// be adjusted in the methods: setObjectTextHeight and setObjectTextFontSize.
        /// </summary>
        /// <param name="newText">The text to display.</param>
        public void setObjectText(string newText)
        {
            GameObject textObj = null;

            if (!transform.Find("textObj"))
            {
                textObj = new GameObject();
                textObj.name = "textObj";
                textObj.AddComponent<TextMeshPro>();
            }
            else
            {
                textObj = transform.Find("textObj").gameObject;
                textObj.GetComponent<TextMeshPro>().SetText(newText);
                return;
            }

            TextMeshPro text = textObj.GetComponent<TextMeshPro>();
            RectTransform textTransform = textObj.GetComponent<RectTransform>();
            float yPos = transform.position.y + transform.localScale.y / 2 + objectTextHeight + .5f;
            textTransform.position = new Vector3(transform.position.x, yPos, transform.position.z);
            text.SetText(newText);
            text.text = newText;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = objectTextFontSize;
            textTransform.sizeDelta = new Vector2(objectTextFontSize, 5);
            text.enableWordWrapping = true;
            textObj.GetComponent<RectTransform>().SetParent(this.transform);
        }

        /// <summary>
        /// Set the height that text set on this object will sit above the object.
        /// The height value specified is relative to the current height (default .5 above the top of the object).
        /// </summary>
        /// <param name="newHeight">The change in height of the text above this object</param>
        public void setObjectTextHeight(float newHeight)
        {
            TextMeshPro text = null;
            objectTextHeight = newHeight;
            if(!(text = GetComponentInChildren<TextMeshPro>()))
            {
                return;
            }
            RectTransform textTransform = GetComponentInChildren<RectTransform>();
            float newY = transform.position.y + transform.localScale.y / 2 + 0.5f + newHeight;
            textTransform.position = new Vector3(textTransform.position.x, newY, textTransform.position.z);
        }

        /// <summary>
        /// Set the font size of text set above this object.
        /// The default fonot size if 5.
        /// </summary>
        /// <param name="fontSize">The new font size for text set above this object.</param>
        public void setObjectTextFontSize(int fontSize)
        {
            TextMeshPro text = null;
            objectTextFontSize = fontSize;
            if (!(text = GetComponentInChildren<TextMeshPro>()))
            {
                return;
            }
            text.fontSize = fontSize;
            //adjust box containing font
            RectTransform textTransform = GetComponentInChildren<RectTransform>();
            textTransform.sizeDelta = new Vector2(fontSize, 5);
        }

        /// <summary>
        /// Set whether an object may be teleported to by the player.
        /// Objects are set to be teleportable by defaults when they are created.
        /// </summary>
        /// <param name="isTeleportable">True to set the object as teleportable. 
        /// False to unset as teleportable.</param>
        public void setObjectTeleportable(bool isTeleportable)
        {
            if (gameObject.tag == "Empty") { return; }

            var teleportable = gameObject.GetComponent<Teleportable>();
            if(!isTeleportable && teleportable)
            {
                Destroy(teleportable);
            }
            else if(isTeleportable && !teleportable)
            {
                gameObject.AddComponent<Teleportable>();
            }
        }

        #endregion

        #region Informational Methods

        /// <summary>
        /// Get 3D vector position of this object
        /// </summary>
        /// <returns>3D vector of object position</returns>
        public Vector3 getObjectPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Get x-axis position of this object
        /// </summary>
        /// <returns>X-axis position of this object</returns>
        public float getObjectPositionX()
        {
            return transform.position.x;
        }

        /// <summary>
        /// Get y-axis position of this object
        /// </summary>
        /// <returns>Y-axis position of this object</returns>
        public float getObjectPositionY()
        {
            return transform.position.y;
        }

        /// <summary>
        /// Get z-axis position of this object
        /// </summary>
        /// <returns>Z-axis position of this object</returns>
        public float getObjectPositionZ()
        {
            return transform.position.z;
        }

        /// <summary>
        /// Get 3D vector position of player
        /// </summary>
        /// <returns>Position of player as 3D vector</returns>
        public Vector3 getPlayerPosition()
        {
            return playerObj.transform.position;
        }

        /// <summary>
        /// Get x-axis position of player
        /// </summary>
        /// <returns>X-axis position of player</returns>
        public float getPlayerPositionX()
        {
            return playerObj.transform.position.x;
        }

        /// <summary>
        /// Get y-axis position of player
        /// </summary>
        /// <returns>Y-axis position of player</returns>
        public float getPlayerPositionY()
        {
            return playerObj.transform.position.y;
        }

        /// <summary>
        /// Get z-axis position of player
        /// </summary>
        /// <returns>Z-axis position of player</returns>
        public float getPlayerPositionZ()
        {
            return playerObj.transform.position.z;
        }

        #endregion

        //called when player hand hovers over this object
        private void OnHandHoverBegin(Hand hand)
        {
            coreManagerBehavior.isAnyObjectHovered = true;
            isHandTouching = true;

            if(hand.name == "RightHand")
            {
                isRightHandTouching = true;
            }
            else
            {
                isLeftHandTouching = true;
            }  
        }

        //called when player hand stops hovering over this object
        private void OnHandHoverEnd(Hand hand)
        {
            coreManagerBehavior.isAnyObjectHovered = false;
            isHandTouching = false;
            isLeftHandTouching = false;
            isRightHandTouching = false;
        }

        //Called upon collision between this object and other object
        private void OnCollisionEnter(Collision collision)
        {
            //interactionObj is saved to check collision in checkCollision()
            interactionObj = collision.gameObject;

            //make entry in GameManager collision list
            CollisionEvent colEvent = new CollisionEvent();
            colEvent.object1 = gameObject.name;
            colEvent.object2 = interactionObj.name;
            coreManagerBehavior.collisionList.Add(colEvent);
        }

        //Called when this object stops colliding with other object
        private void OnCollisionExit(Collision collision)
        {
            //When this object stops colliding with interactionObj, set it to null
            interactionObj = null;
        }

        #region Legacy API Methods


        /// <summary>
        /// Rotate this object along the y axis
        /// </summary>
        /// <param name="shouldSpin">True if object should spin, false otherwise</param>
        public void setObjectSpin(bool shouldSpin)
        {
            if (gameObject.tag == "Empty") { return; }

            shouldSpinFlag = shouldSpin;
        }

        /// <summary>
        /// Set whether this object is visible or not
        /// </summary>
        /// <param name="isVisible">True to set the object as visible. False to set as invisible.</param>
        public void setObjectVisible(bool isVisible)
        {
            if (gameObject.tag == "Empty") { return; }

            //TODO refactor this method, object may have renderer on parent and children
            if (isVisible)
            {
                if (GetComponent<Renderer>() == null)
                {
                    //renderer is not on parent -- it must be on children
                    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer rend in renderers)
                    {
                        rend.enabled = true;
                    }
                }
                else
                {
                    gameObject.GetComponent<Renderer>().enabled = true;
                }
            }
            else
            {
                Renderer renderer;
                if ((renderer = GetComponent<Renderer>()) == null)
                {
                    //renderer is not on parent -- it must be on children
                    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer rend in renderers)
                    {
                        rend.enabled = false;
                    }
                }
                else
                {
                    renderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Check if this object is visible.
        /// </summary>
        /// <returns>True if object is visible.</returns>
        public bool isObjectVisible()
        {
            if (gameObject.tag == "Empty") { return false; }

            return gameObject.GetComponent<Renderer>().isVisible;
        }

        /// <summary>
        /// Set this object to a random color.
        /// Only changes color of primitive objects.
        /// </summary>
        public void setObjectColor()
        {
            if (!GetComponent<Renderer>()) { return; }
            setObjectColor(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        }

        /// <summary>
        /// Change object to color in specified list of available colors.
        /// Color must from list: "red", "blue", "yellow", "green", "orange", "purple", "pink", "white", "gray", "black"
        /// Only changes color of primitive objects
        /// </summary>
        /// <param name="color">New color fot this object</param>
        public void setObjectColor(string color)
        {
            if(!GetComponent<Renderer>()) { return; }
            switch (color.ToLower())
            {
                case "red":
                case "r":
                    setObjectColor(1.0f, 0, 0);
                    break;
                case "blue":
                case "b":
                    setObjectColor(0, 0, 1.0f);
                    break;
                case "yellow":
                case "y":
                    setObjectColor(1.0f, 1.0f, 0);
                    break;
                case "green":
                case "g":
                    setObjectColor(0, 1.0f, 0);
                    break;
                case "orange":
                case "o":
                    setObjectColor(1.0f, 0.5f, 0);
                    break;
                case "purple":
                case "p":
                    setObjectColor(0.6f, 0, 0.6f);
                    break;
                case "pink":
                    setObjectColor(1.0f, 0.25f, 1.0f);
                    break;
                case "white":
                case "w":
                    setObjectColor(1.0f, 1.0f, 1.0f);
                    break;
                case "gray":
                case "grey":
                    setObjectColor(0.6f, 0.6f, 0.6f);
                    break;
                case "black":
                case "k":
                    setObjectColor(0, 0, 0);
                    break;
            }
        }

        /// <summary>
        /// Change color of this object based on numeric rgb values.
        /// Only changes the color of primitive objects.
        /// </summary>
        /// <param name="r">Red color value.</param>
        /// <param name="g">Green color value.</param>
        /// <param name="b">Blue color value.</param>
        public void setObjectColor(float r, float g, float b)
        {
            if (!GetComponent<Renderer>()) { return; }
            Material material = GetComponent<Renderer>().material;
            material.color = new Color(r, g, b);
        }

        /// <summary>
        /// Give points to the player in specified amount.
        /// </summary>
        /// <param name="points">Amount of points to give</param>
        public void givePoints(int points = 10)
        {
            GameManager.totalPoints += points;
        }

        /// <summary>
        /// Remove points from the player in specified amount
        /// </summary>
        /// <param name="points">Amount of points to remove</param>
        public void takePoints(int points = 10)
        {
            GameManager.totalPoints -= points;
        }

        /// <summary>
        /// Check the amount of points the player has
        /// </summary>
        /// <returns>The player's points</returns>
        public int checkPoints()
        {
            return GameManager.totalPoints;
        }

        /// <summary>
        /// Enable win game scenario. Sets win game text on the screen and 
        ///resets the current scene.
        /// </summary>
        public void winGame()
        {
            coreManagerBehavior.winGame();
        }

        /// <summary>
        /// Enable lose game scenario. Sets lose game text on the 
        /// screen and resets the current scene.
        /// </summary>
        public void loseGame()
        {
            coreManagerBehavior.loseGame();
        }

        /// <summary>
        /// Set string value to display in the lower right corner of the screen.
        /// </summary>
        /// <param name="text">The value to display</param>
        public void setHUD(string text)
        {
            GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
            TextMeshProUGUI textObj = HUD.transform.Find("UpdateHUD").GetComponent<TextMeshProUGUI>();
            textObj.SetText(text);
        }

        /// <summary>
        /// Set integer value to display in the lower right corner of the screen.
        /// </summary>
        /// <param name="text">The value to display</param>
        public void setHUD(int text)
        {
            GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
            TextMeshProUGUI textObj = HUD.transform.Find("UpdateHUD").GetComponent<TextMeshProUGUI>();
            textObj.SetText(text.ToString());
        }
        #endregion
    }
}
