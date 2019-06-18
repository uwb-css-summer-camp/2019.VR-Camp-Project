using UnityEngine;
using UWBsummercampAPI;

public class planeGame1 : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool hadTimeout = false;
	void buildGame () {
        startTimer(1);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		if(isControllerGripDown() && hadTimeout)
        {
            createNewObject(getPlayerPositionX(), getPlayerPositionY(), getPlayerPositionZ() + .5f, "throwSphere2");
            hadTimeout = false;
            startTimer(1);  
        }

        if(isTimerFinished())
        {
            hadTimeout = true;
        }

	}
	
}
