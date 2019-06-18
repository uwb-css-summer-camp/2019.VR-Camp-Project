using UnityEngine;
using UWBsummercampAPI;

public class interactObject : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool isHit = false;
	void buildGame () {
		
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        if(isHit)
        {
            setObjectColor("red");
        }
        else
        {
            setObjectColor("gray");
        }

        
        if(hasCollisionWithOtherObject("Ball"))
        {
            //Debug.LogError("Adding Velocity to Ball");
            setObjectVelocity(0, 20, 0);
            setObjectColor("red");
            isHit = true;
            givePoints(10);
            startTimer(3);
        }

        if(isTimerFinished())
        {
            //destroyObject(gameObject);
        }

	}
	
}
