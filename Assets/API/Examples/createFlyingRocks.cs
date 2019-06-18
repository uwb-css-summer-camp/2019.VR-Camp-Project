using UnityEngine;
using UWBsummercampAPI;

public class createFlyingRocks : coreObjectsBehavior {

    void buildGame()
    {
        startTimer(1);
    }

    // Update is called once per frame
    void updateGame()
    {
        if (isTimerFinished())
        {
            int xPos = getRandomNumber(24) - 12;
            createNewObject(xPos, 1, 22, "movingRock");
            startTimer(1);
        }

        if (checkPoints() >= 500)
        {
            winGame();
        }
        if (checkPoints() <= -500)
        {
            loseGame();
        }
    }

}
