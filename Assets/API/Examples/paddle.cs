using UnityEngine;
using UWBsummercampAPI;

public class paddle : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectName("paddle");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        if(getObjectPositionX() >= 12 || getObjectPositionX() <= -12)
        {
            setObjectVelocity(0, 0, 0);
        }
		if(isControllerRightTiltedRight() && getObjectPositionX() <= 12)
        {
            setObjectVelocity(10, 0, 0);
        }

        if(isControllerRightTiltedLeft() && getObjectPositionX() >= -12)
        {
            setObjectVelocity(-10, 0, 0);
        }
	}
	
}
