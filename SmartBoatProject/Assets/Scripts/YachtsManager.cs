using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YachtsManager : MonoBehaviour
{
    public BoatForces yachtPrefab;
    public GameObject waterArround, mainYacht;
    public IYachtControls yachtControls;

    void Update() {
        moveWaterAreaArroundShip();
    }

    private void moveWaterAreaArroundShip() {
        if(waterArround != null) {
            waterArround.transform.position = new Vector3(yachtPrefab.transform.position.x + 12, waterArround.transform.position.y, yachtPrefab.transform.position.z + 12);
        }
    }
}
