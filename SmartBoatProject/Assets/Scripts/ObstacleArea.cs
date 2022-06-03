using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleArea : MonoBehaviour
{
    //Declara��o de vari�veis
    [SerializeField] private const float spawnRange = 80f;

    [SerializeField] private GameObject smartBoat, goal;

    [SerializeField] private Transform waterPlane;

    private List<GameObject> obstacles;

    private float sideWindVar = 0f;

    private void Awake()
    {   
        //Se sideWindVar = 1, os obst�culos s�o spawnmados mais distante da �rea do barco. Isso ocorre no m�todo ResetObstacles.
        if (smartBoat.GetComponent<BoatAgent>().sideWindStaticObjects) sideWindVar = 1f;
        obstacles = new List<GameObject>();

        //Preenche uma lista "obstacles" com os obst�culos.
        FindObstacles(transform);
    }

    /// <summary>
    /// Reseta os obst�culos, posicionando-os em posi��es e rota��es aleat�rias
    /// </summary>
    public void ResetObstacles()
    {

        foreach (GameObject Obstacle in obstacles)
        {   
            //Posi��o aleat�ria em x dentro do limite de spawnRange.
            float xPosition = Random.Range(-spawnRange, spawnRange);
            //As opera��es abaixo envolvendo localScales evitam que a caixa spawne na regi�o que spawna o agente e na regi�o onde est� o Goal. A multiplica��o por 2 na escala nos obst�culos evita que spawne muito perto dos GameObjects.
            
            //Posi��o aleat�ria em z dentro do limite estabelecido. Evita que spawnme em cima da chegada ou muito perto do ponto de partida do barco.
            //O waterPlane est� multiplicado por 10, pois a escala do gameobject Plane � de 1:10 units. Se sideWindVar for diferente de 0, os obst�culos surgem mais longe do barco.
            float zPosition = Random.Range(waterPlane.localScale.z * 10f / 2 - 20f, waterPlane.localScale.z * 10f / 2 - (goal.transform.localPosition.z - smartBoat.transform.localPosition.z) + Obstacle.transform.localScale.x + 45f +10f*sideWindVar) ; //Diminuir dps
            
            //Rotaciona o obst�culo aleatoriamente
            float yRotation = Random.Range(-180f, 180f);

            //Alimenta a posi��o e a rota��o do obst�culo
            Obstacle.transform.position = new Vector3(xPosition + transform.position.x, Obstacle.transform.localScale.y / 2, zPosition + transform.position.z);
            Obstacle.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    /// <summary>
    /// Encontra todos os obst�culos que s�o filhos de uma transform pai e preenche a lista
    /// </summary>
    /// <param name="parent">O pai dos filhos que ser�o encontrados</param>
    private void FindObstacles(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            obstacles.Add(child.gameObject);
        }
    }

    /// <summary>
    /// Se o vento for de trav�s, o n�mero de  obst�culos � reduzido para 5.
    /// </summary>
    /// <param name="parent">O gameobject pai dos obst�culos</param>
    /// <param name="sideWind">O bool que diz se o vento � de trav�s</param>
    public void DeactivateObstacles(Transform parent, bool sideWind)
    {
        if (sideWind)
        {
            for (int i = 0; i < 4 * sideWindVar; i++) //Acho que pode apagar o sideWindVar da multiplica��o. //Antes de eu diminuir o n�mero de pedras total estava 7.
                parent.GetChild(i).gameObject.SetActive(false);
        }
    }
}

