using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private Text vel, sailAngle, rudderAngle, windDirection, windSpeed;
    [SerializeField] private Transform wind;
    [SerializeField] private Rigidbody yacht;
    [SerializeField] private HingeJoint mainSailAngle;
    private float rudderAngleNum;

    //As informa��es mostradas na HUD est�o contidas aqui.
    private void Start()
    {
        //Indicador da velocidade do vento
        windSpeed.text = "Velocidade do vento (m/s): " + yacht.GetComponent<BoatForces>().windSpeed +" ";
    }

    void Update()
    {   
        //Indicador da velocidade do barco
        vel.text = " Velocidade (m/s): " + Math.Round(yacht.velocity.magnitude, 2);

        //indicador da rota��o da vela
        if (mainSailAngle.transform.localRotation.eulerAngles.y <= 180)
            sailAngle.text = " �ngulo da vela: " + Mathf.Round(mainSailAngle.transform.localRotation.eulerAngles.y) + "�";
        else
            sailAngle.text = " �ngulo da vela: " + Mathf.Round(-360 + mainSailAngle.transform.localRotation.eulerAngles.y) + "�";

        //Indicador do leme
        rudderAngleNum = yacht.gameObject.GetComponent<BoatForces>().rudderAngle;
        if (rudderAngleNum > 0f)
            rudderAngle.text = " Leme: \\";
        else if (rudderAngleNum == 0f)
            rudderAngle.text = " Leme: |";
        else if (rudderAngleNum < 0f)
            rudderAngle.text = " Leme: /";

        //Indicador da dire��o do vento
        if (wind.rotation.eulerAngles.y == 0)
            windDirection.text = "Vento em popa (no sentido do movimento) ";
        else if (wind.rotation.eulerAngles.y == 270)
            windDirection.text = "Vento de trav�s (perpendicular ao movimento) ";

    }
}
