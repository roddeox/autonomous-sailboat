using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleArea : MonoBehaviour
{
    //Declaração de variáveis
    [SerializeField] private const float spawnRange = 80f;

    [SerializeField] private GameObject smartBoat, goal;

    [SerializeField] private Transform waterPlane;

    private List<GameObject> obstacles;

    private float sideWindVar = 0f;

    private void Awake()
    {   
        //Se sideWindVar = 1, os obstáculos são spawnmados mais distante da área do barco. Isso ocorre no método ResetObstacles.
        if (smartBoat.GetComponent<BoatAgent>().sideWindStaticObjects) sideWindVar = 1f;
        obstacles = new List<GameObject>();

        //Preenche uma lista "obstacles" com os obstáculos.
        FindObstacles(transform);
    }

    /// <summary>
    /// Reseta os obstáculos, posicionando-os em posições e rotações aleatórias
    /// </summary>
    public void ResetObstacles()
    {

        foreach (GameObject Obstacle in obstacles)
        {   
            //Posição aleatória em x dentro do limite de spawnRange.
            float xPosition = Random.Range(-spawnRange, spawnRange);
            //As operações abaixo envolvendo localScales evitam que a caixa spawne na região que spawna o agente e na região onde está o Goal. A multiplicação por 2 na escala nos obstáculos evita que spawne muito perto dos GameObjects.
            
            //Posição aleatória em z dentro do limite estabelecido. Evita que spawnme em cima da chegada ou muito perto do ponto de partida do barco.
            //O waterPlane está multiplicado por 10, pois a escala do gameobject Plane é de 1:10 units. Se sideWindVar for diferente de 0, os obstáculos surgem mais longe do barco.
            float zPosition = Random.Range(waterPlane.localScale.z * 10f / 2 - 20f, waterPlane.localScale.z * 10f / 2 - (goal.transform.localPosition.z - smartBoat.transform.localPosition.z) + Obstacle.transform.localScale.x + 45f +10f*sideWindVar) ; //Diminuir dps
            
            //Rotaciona o obstáculo aleatoriamente
            float yRotation = Random.Range(-180f, 180f);

            //Alimenta a posição e a rotação do obstáculo
            Obstacle.transform.position = new Vector3(xPosition + transform.position.x, Obstacle.transform.localScale.y / 2, zPosition + transform.position.z);
            Obstacle.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    /// <summary>
    /// Encontra todos os obstáculos que são filhos de uma transform pai e preenche a lista
    /// </summary>
    /// <param name="parent">O pai dos filhos que serão encontrados</param>
    private void FindObstacles(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            obstacles.Add(child.gameObject);
        }
    }

    /// <summary>
    /// Se o vento for de través, o número de  obstáculos é reduzido para 5.
    /// </summary>
    /// <param name="parent">O gameobject pai dos obstáculos</param>
    /// <param name="sideWind">O bool que diz se o vento é de través</param>
    public void DeactivateObstacles(Transform parent, bool sideWind)
    {
        if (sideWind)
        {
            for (int i = 0; i < 4 * sideWindVar; i++) //Acho que pode apagar o sideWindVar da multiplicação. //Antes de eu diminuir o número de pedras total estava 7.
                parent.GetChild(i).gameObject.SetActive(false);
        }
    }
}

