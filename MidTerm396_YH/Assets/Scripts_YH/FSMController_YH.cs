using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class FSMController_YH : MonoBehaviour
{
    public NPCStateBase currentState;

    #region Movement 
    bool isAvoiding = false;
    private Vector3 tarPos;
    [SerializeField] private float movementSpeed = 5.0f;
    [SerializeField] private float rotSpeed = 2.0f;
    [SerializeField] private float minX = -9.0f;
    [SerializeField] private float maxX = 9.0f;
    [SerializeField] private float minZ = -9.0f;
    [SerializeField] private float maxZ = 9.0f;
    [SerializeField] private float targetReactionRadius = 5.0f;
    [SerializeField] private float targetVerticalOffset = 0.5f;
    #endregion

    #region Sense properties
    public GameObject goPlayer;
    public bool hasPowerUp = false;
    public bool isVisible = false;
    public float detectionRate = 1.0f;
    private float elapsedTime = 0.0f;
    #endregion
    #region Sight properties
    public int FieldOfView = 90;
    public int ViewDistance = 7;
    private Transform playerTrans;
    private Vector3 rayDirection;
    #endregion

    private void Start()
    {
        SetState(new WanderState(this));

        if (goPlayer == null)
        {
            goPlayer = GameObject.FindGameObjectWithTag("Player");
        }
        playerTrans = goPlayer.transform;
        GetNextPosition();
    }

    private void Update()
    {
        currentState?.Update();
        Debug.Log($"{currentState}");

        if(isAvoiding)
        {
            SetState(new EvadeState(this));
        }
        else
        {
            if (isVisible)
            {
                SetState(new ChaseState(this));
            }
            else
            {
                if(!(currentState is PatrolState))
                {
                    SetState(new WanderState(this));
                }
                
            }
        }


        UpdatePowerUp();
        UpdateSense();
    }

    public void SetState(NPCStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }


    private void UpdatePowerUp()
    {
        hasPowerUp = goPlayer.GetComponent<PlayerController>().hasPowerUp;
    }   

    private void UpdateSense()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= detectionRate)
        {
            DetectAspect();
            elapsedTime = 0.0f;
        }
    }
    public void ChangeColor(Color newColor)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = newColor;
        }
    }
    private void DetectAspect()
    {
        if (playerTrans == null) return;

        rayDirection = (playerTrans.position - transform.position).normalized;

        if (Vector3.Angle(rayDirection, transform.forward) < FieldOfView)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDirection, out hit, ViewDistance))
            {
                if (hit.collider.gameObject == goPlayer)
                {
                    Debug.Log("Player Detected");
                    isVisible = true;
                    ChangeColor(Color.red);
                }
                if (hasPowerUp)
                {
                    Debug.Log("Player has PowerUp and less than 4 away");
                    isAvoiding = true;
                    ChangeColor(Color.yellow);
                }
                else
                {
                    Debug.Log("Player is close but has No PowerUp");
                    isAvoiding = false;
                }
            }
            else
            {
                Debug.Log("Player Undetected");
                isVisible = false;
            }
        }
    }

    void GetNextPosition()
    {
        tarPos = new Vector3(Random.Range(minX, maxX), targetVerticalOffset, Random.Range(minZ, maxZ));
    }

    public void HandleWander()
    {
        if (Vector3.Distance(tarPos, transform.position) <= targetReactionRadius)
        {
            GetNextPosition();
        }

        Quaternion tarRot = Quaternion.LookRotation(tarPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, tarRot, rotSpeed * Time.deltaTime);
        transform.Translate(new Vector3(0, 0, movementSpeed * Time.deltaTime));
    }

    public void HandleChase()
    {
        Vector3 directionToPlayer = (playerTrans.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
        transform.Translate(directionToPlayer * movementSpeed * Time.deltaTime, Space.World);
    }
    public void HandleEvade()
    {
        Vector3 awayFromPlayer = transform.position - playerTrans.position;
        awayFromPlayer.Normalize();

        Vector3 newPosition = transform.position + awayFromPlayer * movementSpeed * Time.deltaTime;

        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
        newPosition.y = targetVerticalOffset;

        Quaternion targetRotation = Quaternion.LookRotation(newPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);

        transform.position = Vector3.MoveTowards(transform.position, newPosition, movementSpeed * Time.deltaTime);
    }

    public void HandlePatrol(bool isPartoling)
    {
        this.GetComponent<EnemyPatrol>().enabled = isPartoling;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isEditor || playerTrans == null)
            return;

        Debug.DrawLine(transform.position, playerTrans.position, Color.red);
        Vector3 frontRayPoint = transform.position + (transform.forward * ViewDistance);

        Vector3 leftRayPoint = Quaternion.Euler(0, FieldOfView * 0.5f, 0) * frontRayPoint;
        Vector3 rightRayPoint = Quaternion.Euler(0, -FieldOfView * 0.5f, 0) * frontRayPoint;

        Debug.DrawLine(transform.position, frontRayPoint, Color.green);
        Debug.DrawLine(transform.position, leftRayPoint, Color.green);
        Debug.DrawLine(transform.position, rightRayPoint, Color.green);
    }

    public IEnumerator WanderToPatrol()
    {
        while (true)
        {
            if (!(currentState is ChaseState) || !(currentState is EvadeState))
            {
                float waitTime = Random.Range(20f, 30f);
                yield return new WaitForSeconds(waitTime);
                SetState(new PatrolState(this));
            }

        }
    }

    public IEnumerator PatrolToChase()
    {
        while (true)
        {

            float waitTime = Random.Range(20f, 30f);
            yield return new WaitForSeconds(waitTime);
            SetState(new ChaseState(this));
        }
    }


}
