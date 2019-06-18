using UnityEngine;
using UWBsummercampAPI;

public class enemyCreator2 : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        startTimer(2);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		
        if(isTimerFinished())
        {
            createNewObject(35, 0, 30, "rollingEnemy2");
            startTimer(2);
        }
	}
	
}
