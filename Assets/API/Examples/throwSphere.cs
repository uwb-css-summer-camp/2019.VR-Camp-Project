using UnityEngine;
using UWBsummercampAPI;

public class throwSphere : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool wasGrabbed = false;

	void buildGame () {
        setObjectThrowable(true);
        setObjectName("throwSphere");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		
        if(isObjectGrabbed())
        {
            wasGrabbed = true;
        }
        else if(wasGrabbed)
        {
            setObjectVelocity(0, 0, 20);
        }

        if (hasCollisionWithOtherObject("rollingEnemy") || hasCollisionWithOtherObject("rollingEnemy2") ||
            hasCollisionWithOtherObject("farWall") || hasCollisionWithOtherObject("winWall"))
        {
            destroyThisObject();
        }
	}
	
}
