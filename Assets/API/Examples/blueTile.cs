using UnityEngine;
using UWBsummercampAPI;

public class blueTile : coreObjectsBehavior {

    // buildGame() is called once, at the start
    // of the game
    bool moveForward = false;

	void buildGame () {
        startTimer(1);
        setObjectName("blueTile");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        //startTimer(2);
        if(isTimerFinished())
        {
            if(moveForward)
            {
                if (getObjectPositionZ() > 20)
                {
                    moveForward = false;
                }
                setObjectPositionAbsolute(getObjectPositionX(), getObjectPositionY(), getObjectPositionZ() + 1);
                startTimer(1);
            }
            else
            {
                if (getObjectPositionZ() < 10)
                {
                    moveForward = true;
                }
                setObjectPositionAbsolute(getObjectPositionX(), getObjectPositionY(), getObjectPositionZ() - 1);
                startTimer(1);
            }
        }
	}
}
