using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseTesting : MonoBehaviour
{
    public float speed = 1;
    public float timer = 0;
    public List<GameObject> gameObjects;
    // Update is called once per frame
    void Update()
    {
        timer = timer + Time.deltaTime * speed;
        timer %= 1;
        sex(0, timer);
        sex(1, RandomFunctions.EaseIn(timer));
        sex(2, RandomFunctions.EaseOut(timer));
        sex(3, RandomFunctions.EaseInAndOut(timer));
        sex(4, RandomFunctions.EaseBounce(timer));
        sex(5, RandomFunctions.EaseOvershoot(timer));
    }
    private void sex(int index, float perc)
    {
        var x = gameObjects[index].transform.position;
        x.x = perc*14;
        x.x -= 7;
        gameObjects[index].transform.position = x;
    }
}
