using UnityEngine;
using UWBsummercampAPI;

public class movingRock : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
		
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectVelocity(0, 0, -10);

        if(hasCollisionWithOtherObject("paddle"))
        {
            givePoints(100);
            destroyThisObject();
        }

        if(getObjectPositionZ() < -5)
        {
            takePoints(100);
            destroyThisObject();
        }
	}

	
}
