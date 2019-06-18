using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using Valve.VR.InteractionSystem;
using Valve.VR;
using TMPro;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace UWBsummercampAPI
{

    //class to hold collision events
    public class CollisionEvent
    {
        public string object1 = null;
        public string object2 = null;
        public int framesInList = 0;
        public List<int> idList = new List<int>();
    }

    /// <summary>
    /// Parent class for GameManager class. Allows use of any API methods
    /// that are not directly used on a single object.
    /// </summary>
    public class coreManagerBehavior : MonoBehaviour
    {
  
        //list of collision events
        public static List<CollisionEvent> collisionList = new List<CollisionEvent>();

        //true if any object is grabbed -- used to turn off controller
        // trigger for other object actions. Assumes that only one object will be held at a time
        //TODO may need to change to register of objects with grabbed propery (LIST)
        public static bool isAnyObjectGrabbed = false;
        public static bool isAnyObjectHovered = false;

        //flags
        protected bool hasTimer = false;

        //dynamic variables
        public GameObject playerObj;
        private float timer = 0;
        private float timerMax = 10;
        private static float gameFinishedTimer = 0;
        public static bool isGameFinished = false;

        static public int totalPoints = 0;

        //[0] = obj1, [1] = obj2, [2] = frames since collision
        public static string [] collisionArray = new string[3];

    
        // Start is called before the first frame update
        void Start()
        {
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
            //instatiate child method
            MethodInfo method = this.GetType().GetMethod("updateGame", BindingFlags.Instance |
                BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (method != null)
            {
                method.Invoke(this, new object[0]);
            }

            //handle end game logic
            if (isGameFinished)
            {
                //Debug.Log("game finshed timer: " + gameFinishedTimer);
                gameFinishedTimer += Time.deltaTime;
                if (gameFinishedTimer > 5)
                {
                    isGameFinished = false;
                    gameFinishedTimer = 0;
                    totalPoints = 0;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }

            //handle logic for collisions between objects
            for (int i = 0; i < collisionList.Count; i++)
            {
                if(collisionList[i].framesInList > 1)
                {
                    collisionList.RemoveAt(i);
                    i--;
                }
                else
                {
                    collisionList[i].framesInList++;
                }
            }
        }

        /// <summary>
        /// Give points to the player in specified amount.
        /// </summary>
        /// <param name="points">Amount of points to give</param>
        public bool givePoints(int points = 10)
        {
            totalPoints += points;
            return true;
        }

        /// <summary>
        /// Remove points from the player in specified amount
        /// </summary>
        /// <param name="points">Amount of points to remove</param>
        public bool takePoints(int points = 10)
        {
            totalPoints -= points;
            return true;
        }

        /// <summary>
        /// Check the amount of points the player has
        /// </summary>
        /// <returns>The player's points</returns>
        public int checkPoints()
        {
            return totalPoints;
        }

        /// <summary>
        /// Enable win game scenario. Sets win game text on the screen and 
        ///resets the current scene.
        /// </summary>
        public static void winGame()
        {
            if (!isGameFinished)
            {
                GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
                TextMeshProUGUI text = HUD.transform.Find("GameFinishedText").GetComponent<TextMeshProUGUI>();
                text.SetText("You Win");
                isGameFinished = true;
            }
        }

        /// <summary>
        /// Enable lose game scenario. Sets lose game text on the 
        /// screen and resets the current scene.
        /// </summary>
        public static void loseGame()
        {
            if (!isGameFinished)
            {
                Debug.Log("lose game");
                GameObject HUD = GameObject.FindGameObjectWithTag("HUD");
                TextMeshProUGUI text = HUD.transform.Find("GameFinishedText").GetComponent<TextMeshProUGUI>();
                text.SetText("You Lose");
                isGameFinished = true;
            }
        }
    }
}

