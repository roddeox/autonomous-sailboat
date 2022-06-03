using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObstacleArea : MonoBehaviour
{
    //Declaração de variáveis
    [SerializeField] private Rigidbody movingObstacle, instance;
    [SerializeField] private Transform movingObstacles;
    [SerializeField] private BoatAgent boatAgent;
    private float[] spawnSide, velX;
    private float sideWindVar;
    private int boatQtd = 3;

    void Awake()
    {   
        //Variável que irá dizer qual o lado que o obstáculo irá spawnmar.
        spawnSide = new float[boatQtd];

        //Velocidade do barco.
        velX = new float[boatQtd];

        //Se sideWindVar = 1, a velocidade dos obstáculos será diminuida.
        if (boatAgent.sideWindMovingObstacles) sideWindVar = 1f;
        else sideWindVar = 0f;
    }

    /// <summary>
    /// Reseta a posição e velocidade dos obstáculos móveis.
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
    /// Destrói o obstáculo.
    /// </summary>
    public void DestroyObjects()
    {
        for (int i = 0; i < transform.childCount; i++) 
            Destroy(transform.GetChild(i).gameObject);
        CancelInvoke();
    }

    /// <summary>
    /// Spawnma o obstáculo.
    /// </summary>
    private void SpawnObstacle()
    {
        float offset = 0f;
        for (int i = 0; i < boatQtd; i++)
        {   
            //Rotação do obstáculo
            Quaternion rotation = Quaternion.Euler(0f, 90f + 90f * spawnSide[i], 0f);
            //Posição do obstáculo
            Vector3 spawnPosition = new Vector3(-60f * spawnSide[i] + transform.parent.localPosition.x, 3.5f, -20f + transform.parent.localPosition.z + offset);
            //Criação da instância do obstáculo
            Rigidbody instance = Instantiate(movingObstacle, spawnPosition, rotation, movingObstacles);
            //Velocidade do obstáculo
            instance.velocity = new Vector3(velX[i] * spawnSide[i], 0f, 0f);
            //Distância entre os obstáculos no eixo z
            offset -= 30f;
        }
    }
}
