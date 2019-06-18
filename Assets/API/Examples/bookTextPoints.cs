using UnityEngine;
using UWBsummercampAPI;

public class bookTextPoints : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectText("Books Found: " + checkPoints());
        setObjectTextHeight(1);
        setObjectTextFontSize(10);
    }
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        setObjectText("Books Found: " + checkPoints());
        if(checkPoints() == 4)
        {
            winGame();
        }
    }
	
}
