using UnityEngine;
using UWBsummercampAPI;

public class flyingCube : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectVelocity(0, 0, -40);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        if(getObjectPositionZ() < -25)
        {
            destroyThisObject();
        }

        if (hasPlayerCollisionWithThisObject())
        {
            loseGame();
        }
		
	}
	
}
