using UnityEngine;
using UWBsummercampAPI;

public class rollingEnemy2 : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool hasCollided = false;

	void buildGame () {
		
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {

        if(hasCollided == false)
        {
            setObjectVelocity(-10, 0, 0);
        }
        else
        {
            setObjectVelocity(0, 0, 20);
        }

        if (getObjectPositionY() < -1)
        {
            destroyThisObject();
        }

        if (hasCollisionWithOtherObject("throwSphere"))
        {
            hasCollided = true;
        }

        if (hasCollisionWithOtherObject("farWall"))
        {
            destroyThisObject();
        }

        if (hasCollisionWithOtherObject("winWall"))
        {
            givePoints(200);
            destroyThisObject();
        }
    }
	
}
