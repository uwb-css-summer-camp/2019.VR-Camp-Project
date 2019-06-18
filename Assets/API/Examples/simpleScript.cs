using UnityEngine;
using UWBsummercampAPI;
using TMPro;

public class simpleScript : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectThrowable(true);
        setObjectText("Match the colors");
        setObjectTextFontSize(10);
        setObjectTextHeight(2);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectText("Points: " + checkPoints());
        //addObjectText("stuff");
	}
	
}
