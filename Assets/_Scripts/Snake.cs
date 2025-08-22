using System.Collections;
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

    private int coinScore = 1;

    [SyncVar(hook = nameof(OnShieldStateChanged))]
    private bool isShield;

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
            GameManager.Instance.AddScore(coinScore);
        }
        
        if (other.CompareTag("Short"))
        {
            GetItem(other.gameObject, Item.Short);
            UseShort();
        }
        
        if (other.CompareTag("Shield"))
        {
            GetItem(other.gameObject, Item.Shield);
            UseSheild();
        }

        if (other.CompareTag("Tail"))
        {
            if (isShield) // 무적
                return;
            
            Tail tail = other.GetComponent<Tail>();
            if (tail.ownerIdentity != netIdentity) // 내 꼬리가 아니라면
                Died();
        }
    }

    [Command]
    void UseShort()
    {
        for (int i=0; i < 2; i++)
        {
            RemoveLastTail();
        }
    }
    
    [Server]
    void RemoveLastTail()
    {
        if (tails.Count <= 0)
            return;
            
        GameObject lastTail = tails[tails.Count - 1];
        tails.RemoveAt(tails.Count - 1);
        NetworkServer.Destroy(lastTail);
    }
    
    [Command]
    void UseSheild()
    {
        isShield = true;
    }
    
    void OnShieldStateChanged(bool oldState, bool newState)
    {
        if (newState == true)
            StartCoroutine(ShieldRoutine());
    }
    
    IEnumerator ShieldRoutine()
    {
        Color c = headRenderer.material.color;
        headRenderer.material.color = Color.blue;
        isShield = true;
        yield return new WaitForSeconds(15f);
        isShield = false;
        headRenderer.material.color = c;
    }
    
    [Command]
    void GetItem(GameObject item, Item itemType)
    {
        DestroyItem(item);
        GameManager.Instance.SpawnItem(itemType);
    }
    
    [Server]
    void DestroyItem(GameObject item)
    {
        NetworkServer.Destroy(item);
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
