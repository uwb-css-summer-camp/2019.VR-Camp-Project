using UnityEngine;
using UWBsummercampAPI;

public class createFlyingObjects : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        startTimer(1);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		if(isTimerFinished())
        {
            float xPos = getRandomNumber(10) - 5;
            Vector3 position = new Vector3(xPos, 1f, 35f);
            createNewObject(position, "flyingRockPrefab");
            givePoints(100);
            startTimer(1);
           
        }
	}
	
}
