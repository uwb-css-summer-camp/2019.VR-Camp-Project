using UnityEngine;
using UWBsummercampAPI;

public class bookText : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectText("Collect the books");
        setObjectTextHeight(1);
        setObjectTextFontSize(10);
    }
	
	// updateGame() is called many times per
	// second
	void updateGame () {

    }
	
}
