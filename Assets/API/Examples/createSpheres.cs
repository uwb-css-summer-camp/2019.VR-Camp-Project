using UnityEngine;
using UWBsummercampAPI;

public class createSpheres : coreObjectsBehavior {

    // Start is called before the first frame update
    bool makeNew = true;
    void buildGame()
    {
        startTimer(1);
    }

    // Update is called once per frame
    void updateGame()
    {

        if (isTimerFinished())
        {
            makeNew = true;
        }

        if (isControllerGripDown() && makeNew)
        {
            createNewObject(getPlayerPositionX(), getPlayerPositionY() + 1, getPlayerPositionZ() + .5f, "throwSphere");
            makeNew = false;
            startTimer(1);
        }

        if (checkPoints() >= 500)
        {
            winGame();
        }
    }

}
