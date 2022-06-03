using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacleArea : MonoBehaviour
{
    //Declara��o de vari�veis
    [SerializeField] private Rigidbody movingObstacle, instance;
    [SerializeField] private Transform movingObstacles;
    [SerializeField] private BoatAgent boatAgent;
    private float[] spawnSide, velX;
    private float sideWindVar;
    private int boatQtd = 3;

    void Awake()
    {   
        //Vari�vel que ir� dizer qual o lado que o obst�culo ir� spawnmar.
        spawnSide = new float[boatQtd];

        //Velocidade do barco.
        velX = new float[boatQtd];

        //Se sideWindVar = 1, a velocidade dos obst�culos ser� diminuida.
        if (boatAgent.sideWindMovingObstacles) sideWindVar = 1f;
        else sideWindVar = 0f;
    }

    /// <summary>
    /// Reseta a posi��o e velocidade dos obst�culos m�veis.
    /// </summary>
    public void ResetMovingObjects()
    {
        for (int i = 0; i < boatQtd; i++)
        {
            spawnSide[i] = Mathf.Sign(Random.Range(-1f, 1f));
            velX[i] = Random.Range(0.9f - 0.5f * sideWindVar, 1.9f - 0.5f*sideWindVar);
        }
        Invoke("SpawnObstacle", 0f);
    }

    /// <summary>
    /// Destr�i o obst�culo.
    /// </summary>
    public void DestroyObjects()
    {
        for (int i = 0; i < transform.childCount; i++) 
            Destroy(transform.GetChild(i).gameObject);
        CancelInvoke();
    }

    /// <summary>
    /// Spawnma o obst�culo.
    /// </summary>
    private void SpawnObstacle()
    {
        float offset = 0f;
        for (int i = 0; i < boatQtd; i++)
        {   
            //Rota��o do obst�culo
            Quaternion rotation = Quaternion.Euler(0f, 90f + 90f * spawnSide[i], 0f);
            //Posi��o do obst�culo
            Vector3 spawnPosition = new Vector3(-60f * spawnSide[i] + transform.parent.localPosition.x, 3.5f, -20f + transform.parent.localPosition.z + offset);
            //Cria��o da inst�ncia do obst�culo
            Rigidbody instance = Instantiate(movingObstacle, spawnPosition, rotation, movingObstacles);
            //Velocidade do obst�culo
            instance.velocity = new Vector3(velX[i] * spawnSide[i], 0f, 0f);
            //Dist�ncia entre os obst�culos no eixo z
            offset -= 30f;
        }
    }
}
