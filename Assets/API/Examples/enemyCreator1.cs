using UnityEngine;
using UWBsummercampAPI;

public class enemyCreator1 : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        startTimer(1);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {

        if (isTimerFinished())
        {
            createNewObject(35, 0, 45, "rollingEnemy");
            startTimer(1);
        }
    }
	
}
