using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapTracker : MonoBehaviour
{
    public RectTransform carIcon;        // The UI icon representing the car
    public RectTransform mapRect;        // The map image UI RectTransform
    public RectTransform startPositionIcon;

    public Vector2 mapSize = new Vector2(550, 1600);  // Size of the circuit in world units 500, 1644
    public Vector2 mapOffset = Vector2.zero;         // Offset to align world to map -122, 2

    void Start()
    {
        carIcon.anchoredPosition = PositionOnStart(MyGameController.instance.MyManager.carLamb.position);
        startPositionIcon.anchoredPosition = PositionOnStart(MyGameController.instance.MyManager.flagStartObject.position);
    }
    Vector2 PositionOnStart(Vector3 pos)
    {
        // Normalize position relative to circuit world size
        float normalizedX = (pos.x + mapSize.x / 2f + mapOffset.x) / mapSize.x;
        float normalizedY = (pos.z + mapSize.y / 2f + mapOffset.y) / mapSize.y;

        // Convert to UI position
        float uiX = (normalizedX - 0.5f) * mapRect.rect.width;
        float uiY = (normalizedY - 0.5f) * mapRect.rect.height;

        return new Vector2(uiX, uiY);
    }

    void Update()
    {
        if (MyGameController.instance.isGameOver || MyGameController.instance.isGamePause)
            return; // Skip updating if game is over or paused

        if (MyGameController.instance.gameMode == GameMode.GrandPrix && MyGameController.instance.isGameStart)
        {
            carIcon.anchoredPosition = PositionOnStart(MyGameController.instance.MyManager.carLamb.position);
            // Vector3 pos = MyGameController.instance.MyManager.carLamb.position;

            // // Normalize position relative to circuit world size
            // float normalizedX = (pos.x + mapSize.x / 2f + mapOffset.x) / mapSize.x;
            // float normalizedY = (pos.z + mapSize.y / 2f + mapOffset.y) / mapSize.y;

            // // Convert to UI position
            // float uiX = (normalizedX - 0.5f) * mapRect.rect.width;
            // float uiY = (normalizedY - 0.5f) * mapRect.rect.height;

            // carIcon.anchoredPosition = new Vector2(uiX, uiY);
        }
    }
}
