using UnityEngine;
using UWBsummercampAPI;

public class tetherTest : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        //tetherObject();
        setObjectName("Ball");
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {

        if(getPlayerPositionX() > 1 - 3 && getPlayerPositionX() < 1 + 3 && 
            getPlayerPositionZ() > 10 - 3.5 && getPlayerPositionZ() < 10 + 3.5)
        {
            setTetherObjectToRightController(true);
        }


        if (isControllerTriggerClicked())
        {
            setObjectVelocity(0, 10, 0);
        }

        setObjectColor("blue");
	}
	
}
