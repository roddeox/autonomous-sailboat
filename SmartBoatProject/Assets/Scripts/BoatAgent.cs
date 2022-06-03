using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BoatAgent : Agent
{
    //Defini��o de vari�veis
    [Tooltip("Marque aqui para ativar os obst�culos est�ticos.")]
    [SerializeField] private bool usingStaticObstacles;

    [Tooltip("Marque aqui para ativar os obst�culos m�veis. RaysLeft e RaysRight devem ser ativados")]
    [SerializeField] private bool usingMovingObstacles;

    [Tooltip("Marque aqui ap�s rotacionar o WindEmitter em 270� no caso de obst�culos est�ticos. Os obst�culos aparecem mais longe do veleiro e em menor quantidade.")]
    public bool sideWindStaticObjects;

    [Tooltip("Marque aqui ap�s rotacionar o WindEmitter em 270� no caso de obst�culos m�veis. A velocidade dos barcos � diminuida.")]
    public bool sideWindMovingObstacles;

    [SerializeField] private BoatForces yacht;

    [SerializeField] private IYachtControls yachtControls;

    [SerializeField] private GameObject goal, checkpoint, rayPerceptionLeft, rayPerceptionRight;

    [SerializeField] private Transform headSail, mainSail, waterPlane, windDirection, rudderMesh;

    [SerializeField] private ObstacleArea obstacleArea;

    [SerializeField] private MovingObstacleArea movingObstacleArea;

    [SerializeField] private Camera cam;

    [SerializeField] private HingeJoint mainSailAngle;

    public override void Initialize()
    {
        yacht = GetComponent<BoatForces>();
        mainSailAngle = yacht.transform.Find("MainSail").GetComponent<HingeJoint>();

        //Certifica que o colisor do barco vai ignorar o colisor das velas
        Physics.IgnoreCollision(GetComponent<Collider>(),headSail.GetComponent<Collider>(), true);
        Physics.IgnoreCollision(GetComponent<Collider>(), mainSail.GetComponent<Collider>(), true);

        //Certifica que as velas n�o v�o colidir entre si
        Physics.IgnoreCollision(headSail.GetComponent<Collider>(), mainSail.GetComponent<Collider>(), true);
        
        //Se os obst�culos est�ticos n�o forem utilizados, s�o desativados

        if (!usingStaticObstacles)
        {
            for (int i = 0; i < obstacleArea.gameObject.transform.childCount; i++)
            {
                obstacleArea.gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        //Zera todas as velocidades para que o agente inicie cada epis�dio parado
        yacht.yachtRigidbody.velocity = Vector3.zero;
        yacht.yachtRigidbody.angularVelocity = Vector3.zero;

        //Reseta a posi��o e rota��o do barco
        yacht.transform.rotation = Quaternion.identity;
        yacht.transform.localPosition = new Vector3(waterPlane.localPosition.x,waterPlane.localPosition.y + 0.3f ,-waterPlane.localScale.z*10f/2 + 20f);

        //Aleatorializa a posi��o dos obst�culos
        if (usingStaticObstacles)
            obstacleArea.ResetObstacles();

        //Aleatoriza posi��o e sentido do movimento dos obst�culos
        if (usingMovingObstacles)
            movingObstacleArea.ResetMovingObjects();

        //Desativa alguns obst�culos se o bool Side Wind estiver ativado
        if (sideWindStaticObjects)
            obstacleArea.DeactivateObstacles(obstacleArea.transform,sideWindStaticObjects);

        //Utiliza um m�todo da classe BoatForces para reiniciar a rota��o das velas a cada epis�dio
        yacht.setSailJointLimits(mainSail.gameObject, 0f, 0f);
        yacht.setSailJointLimits(headSail.gameObject, 0f, 0f);
        
    }

    /// <summary>
    /// �ndice 0: Controla o Head Sail
    /// �ndice 1: Controla a vela principal
    /// �ndice 2: Controla o leme
    /// </summary>
    /// <param name="actions">As a��es feitas pelo agente. Elas s�o 2 inteiros para cada componente que 
    ///indica se deve ser rotacionado um grau em sentido hor�rio ou anti-hor�rio</param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Atribui os �ndices de rota��o aos componentes do barco. Estas s�o suas a��es.
        int mainSail = actions.DiscreteActions[0];
        int rudder = actions.DiscreteActions[1];

        if (mainSail == 1) yachtControls.rotateMainSail(-1);
        if (mainSail == 2) yachtControls.rotateMainSail(1);
        if (mainSail == 3) yachtControls.rotateMainSail(-3);
        if (mainSail == 4) yachtControls.rotateMainSail(3);

        if (rudder == 1) yachtControls.rotateRudder(-1);
      
        if (rudder == 2) yachtControls.rotateRudder(1);

        //Adicionei esta linha para saber quando o leme est� parado. Uso isto para a representa��o visual do leme parado, pois
        //foram definidas explicitamente no c�digo s� os casos que o leme se move.
        if (rudder != 1 && rudder != 2) yacht.rudderAngle = 0f;

        //Recompensas
        //O agente � punido se apontar para o lado oposto.
        if (Vector3.Dot(transform.forward, goal.transform.forward) < 0)
        {
            AddReward(-0.5f);
            if (usingMovingObstacles) movingObstacleArea.DestroyObjects();
            EndEpisode();
        }
            
        //O agente � punido se locomover-se para o lado oposto.
        if (yacht.getVelocity().z < 0) AddReward(-0.0001f);

        //Puni��o caso o agente fique de lado.
        if (Mathf.Abs(transform.localRotation.z) >= 0.9f)
        {
            AddReward(-0.5f);
            if (usingMovingObstacles) movingObstacleArea.DestroyObjects();
            EndEpisode();
        }

        //Rotaciona o modelo do leme. N�o h� influ�ncias f�sicas nele.
        rudderMesh.parent.transform.localRotation = rudderAnimation(rudderMesh.parent.transform.localRotation);
    }

    /// <summary>
    /// Observa��o feita pelo agente do ambiente
    /// </summary>
    /// <param name="sensor">O vetor sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //Os valores est�o normalizados, podendo estar entre -1 e 1
        //Valores locais s�o usados para serem referenciados ao GameObject pai para que perspectiva global n�o seja passada �s �reas de treinamento clonadas.
        //Dist�ncia entre o agente e o objetivo no eixo z. (1 observa��o)
        sensor.AddObservation((goal.transform.localPosition.z - transform.localPosition.z) / (goal.transform.localPosition.z + waterPlane.localScale.z * 10 / 2));

        //Rota��o relativa ao parent deste GameObject no em torno no eixo y; (1 observa��o)
        sensor.AddObservation(transform.localRotation.y / 360.0f);

        //Rota��o relativa ao parent deste GameObject no em torno no eixo z; (1 observa��o)
        sensor.AddObservation(transform.localRotation.z / 360.0f);

        //Produto escalar entre o eixo z do barco e o eixo z do gameobject do objetivo. Caso seja positivo (+1), o barco est�
        //apontado para o objetivo, e se negativo (-1), est� apontando ao sentido oposto. (1 observa��o)
        sensor.AddObservation(Vector3.Dot(transform.forward, goal.transform.forward));

        //Sinal da velocidade do barco para identificar se o barco est� se locomovendo para o sentido oposto. Se for positivo (+1) est� indo para frente
        //negativo (-1) est� andando para tr�s. (1 observa��o)
        sensor.AddObservation(Mathf.Sign(yacht.getVelocity().z));

        //�ngulo do WindEmitter (1 observa��o)
        sensor.AddObservation(windDirection.rotation.y/360f);

        //�ngulo de rota��o da vela. (1 observa��o)
        sensor.AddObservation(mainSailAngle.transform.localRotation.y/360f);

        //Posi��o do leme. (1 observa��o)
        sensor.AddObservation(yacht.rudderAngle);
    }

    /// <summary>
    /// Por meio de Input manual, h� a alimenta��o em <see cref="OnActionReceived(ActionBuffers)"
    /// e o retorno das a��es, sem o uso da rede neural. 
    /// </summary>
    /// <param name="actionsOut">Vetor de a��es</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        //Movimenta��o da vela
        if (Input.GetKey(KeyCode.X)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.Z)) discreteActionsOut[0] = 2;
        if (Input.GetKey(KeyCode.V)) discreteActionsOut[0] = 3;
        if (Input.GetKey(KeyCode.C)) discreteActionsOut[0] = 4;

        //Movimenta��o do leme
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 2;
   
    }

    // O agente � punido se colidir com os obst�culos e o epis�dio � reiniciado.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("obstacle"))
        {
            AddReward(-0.5f); 
            if (usingMovingObstacles) movingObstacleArea.DestroyObjects();
            EndEpisode();
        }
    }

    // O agente � recompensado se tocar na chegada e punido se tocar nas fronteiras
    private void OnTriggerEnter(Collider other)
    {
       
        if (other.CompareTag("goal"))
        {
            AddReward(1f);
            if (usingMovingObstacles) movingObstacleArea.DestroyObjects();
            EndEpisode();
            
        } else if (other.CompareTag("boundary"))
        {
            AddReward(-1f);
            if (usingMovingObstacles) movingObstacleArea.DestroyObjects();
            EndEpisode();

        }
    }

    /// <summary>
    /// Rotaciona o modelo 3D do leme.
    /// </summary>
    /// <param name="rotation">Quaternion que representa a rota��o do gameobject do leme.</param>
    /// <returns></returns>
    private Quaternion rudderAnimation(Quaternion rotation)
    {
        if (yacht.rudderAngle == 1f)
            rotation = Quaternion.Euler(0f, -20f, 0f);
        else if (yacht.rudderAngle == -1f)
            rotation = Quaternion.Euler(0f, 20f, 0f);
        else
            rotation = Quaternion.identity;
        return rotation;
    }

}