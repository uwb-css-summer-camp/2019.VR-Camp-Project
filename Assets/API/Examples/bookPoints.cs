using UnityEngine;
using UWBsummercampAPI;

public class bookPoints : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool pointsReceived = false;

	void buildGame () {
        setObjectThrowable(true);
        setObjectName("bookTreasure");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		
        if(isObjectGrabbed() && !pointsReceived)
        {
            givePoints(1);
            pointsReceived = true;
            startTimer(3);
        }

        if(isTimerFinished())
        {
            Debug.Log("attempting to destroy object");
            destroyThisObject();
        }

	}
	
}
