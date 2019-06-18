using UnityEngine;
using UWBsummercampAPI;

public class makeVisible : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectVisible(false);
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
        if(checkPoints() >= 40)
        {
            setObjectVisible(true);
        }
	}
}
