using UnityEngine;
using UWBsummercampAPI;

public class game2points : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
		
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectText("Points: " + checkPoints());
        setObjectTextHeight(2);
	}
	
}
