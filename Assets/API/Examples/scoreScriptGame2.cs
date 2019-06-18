using UnityEngine;
using UWBsummercampAPI;

public class scoreScriptGame2 : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectText("Score: " + checkPoints());
        setObjectTextFontSize(20);
        setObjectTextHeight(10);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectText("Score: " + checkPoints());
	}
}
