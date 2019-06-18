using UnityEngine;
using UWBsummercampAPI;

public class yellowTile : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool moveForward = false;
	void buildGame () {
        startTimer(4);
        setObjectName("yellowTile");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        if (isTimerFinished())
        {
            if (moveForward)
            {
                if (getObjectPositionZ() > 20)
                {
                    moveForward = false;
                }
                setObjectPositionAbsolute(getObjectPositionX(), getObjectPositionY(), getObjectPositionZ() + 2);
                startTimer(1);
            }
            else
            {
                if (getObjectPositionZ() < 10)
                {
                    moveForward = true;
                }
                setObjectPositionAbsolute(getObjectPositionX(), getObjectPositionY(), getObjectPositionZ() - 2);
                startTimer(1);
            }
        }
    }
	
}
