using UnityEngine;
using UWBsummercampAPI;

public class game3Text : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectText("Points: " + checkPoints());
        setObjectTextFontSize(20);
        setObjectTextHeight(5);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectText("Points: " + checkPoints());
    }
	
}
