using UnityEngine;
using UWBsummercampAPI;

public class goalTile : coreObjectsBehavior {
	
	// buildGame() is called once, at the start
	// of the game
	void buildGame () {
        setObjectVisible(false);
        
	}
	
	// updateGame() is called many times per
	// second
	void updateGame () {
		//if(hasCollisionWithOtherObject("Player"))
  //      {
  //          givePoints(1000);
  //      }

        if(checkPoints() > 1000)
        {
            setObjectVisible(true);
            setObjectColor("yellow");
            //Debug.Log("goal met");
        }

        if(getPlayerPositionZ() > 37)
        {
            winGame();
        }
	}
	
}
