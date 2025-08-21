using Mirror;
using UnityEngine;

public class Snake : NetworkBehaviour
{
    [SerializeField] private GameObject tailPrefab;
    [SerializeField] private MeshRenderer headRenderer;
    
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 120f;
    [SerializeField] private float lerpSpeed = 5f;

    private float tailOffset = 0.1f;
    
    private SyncList<GameObject> tails = new SyncList<GameObject>();
    
    [SyncVar(hook = nameof(OnDeathStateChanged))]
    private bool isDead = false;

    // public override void OnStartClient()
    // {
    //     base.OnStartClient();
    //     // tails.Callback += OnTailUpdated;
    // }

    public override void OnStartLocalPlayer()
    {
        headRenderer.material.color = new Color(0.8f, 1f, 0.8f);
    }

    void Update()
    {
        if (isLocalPlayer && !isDead)
            MoveHead();
    }

    private void LateUpdate()
    {
        MoveTail();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer || isDead)
            return;
        
        if (other.CompareTag("Coin"))
        {
            GetCoin();
        }

        if (other.CompareTag("Tail"))
        {
            Tail tail = other.GetComponent<Tail>();
            if (tail.ownerIdentity != netIdentity) // 내 꼬리가 아니라면
                Died();
        }
    }

    void MoveHead()
    {
        transform.Translate(moveSpeed * Vector3.up * Time.deltaTime);

        float h = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.back * h * turnSpeed * Time.deltaTime);
    }
    
    void MoveTail()
    {
        Transform target = transform;
        foreach (GameObject tail in tails)
        {
            if (tail == null)
                continue;
            
            tail.transform.position = Vector3.Lerp(tail.transform.position, target.position, lerpSpeed * Time.deltaTime);
            tail.transform.rotation = Quaternion.Lerp(tail.transform.rotation, target.rotation, lerpSpeed * Time.deltaTime);

            target = tail.transform;
        }  
    }

    [Command]
    void GetCoin()
    {
        GameManager.Instance.MoveCoin();
        AddTail();
    }

    [Server]
    void AddTail()
    {
        Transform spawnTarget = transform;
        if (tails.Count > 0)
        {
            spawnTarget = tails[tails.Count - 1].transform;
        }

        Vector3 spawnPos = spawnTarget.position;
        Quaternion spawnRot = spawnTarget.rotation;

        GameObject newTail = Instantiate(tailPrefab, spawnPos, spawnRot);
        newTail.GetComponent<Tail>().ownerIdentity = netIdentity;
        
        NetworkServer.Spawn(newTail, connectionToClient); 
        tails.Add(newTail);
    }
    
    // void OnTailUpdated(SyncList<GameObject>.Operation op, int index, GameObject oldTail, GameObject newTail)
    // {
    //     // 꼬리가 추가(ADD) 되었고, 로컬 플레이어인 경우
    //     if (op == SyncList<GameObject>.Operation.OP_ADD && isLocalPlayer)
    //     {
    //         Transform target = transform;
    //
    //         if (index > 0)
    //         {
    //             target = tails[index - 1].transform;
    //         }
    //
    //         newTail.transform.position = target.position;
    //         newTail.transform.rotation = target.rotation;
    //     }
    // }
    
    [Command]
    void Died()
    {
        isDead = true;
    }

    void OnDeathStateChanged(bool oldState, bool newState)
    {
        if (newState)
        {
            headRenderer.material.color = Color.gray;
        }
    }
}
