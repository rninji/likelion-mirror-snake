using System;
using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Item {Short, Shield}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private int score;

    [SerializeField] private Transform coin;
    [SerializeField] private TextMeshProUGUI scoreUI;

    // Syncvar : 대상이 변경될 때 알려주는 기능
    [SyncVar(hook = nameof(OncoinPositionChanged))] // hook : 값이 변경될 때 이벤트 실행
    public Vector3 coinPosition;

    [SerializeField] private GameObject shieldItem;
    [SerializeField] private GameObject shortItem;
    
    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetScoreUI(score);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnItem(Item.Short);
        SpawnItem(Item.Shield);
        MoveCoin();
    }

    public void AddScore(int score)
    {
        this.score += score;
        SetScoreUI(this.score);
    }
    
    private void SetScoreUI(int score)
    {
        scoreUI.text = $"Score : {score}";
    }    
    
    [Server]
    public void MoveCoin()
    {
        coinPosition = SetRandomPosition();
    }

    public void OncoinPositionChanged(Vector3 prevPos, Vector3 newPos)
    {
        coin.position = newPos;
    }
    
    [Server]
    public void SpawnItem(Item item)
    {
        switch (item)
        {
            case Item.Short:
                StartCoroutine(ItemSpawnRoutine(shortItem, 3f));
                break;
            case Item.Shield:
                StartCoroutine(ItemSpawnRoutine(shieldItem, 5f));
                break;
        }
    }
    
    IEnumerator ItemSpawnRoutine(GameObject item, float coolTime)
    {
        yield return new WaitForSeconds(coolTime);
        GameObject newItem = Instantiate(item, SetRandomPosition(), Quaternion.identity);
        NetworkServer.Spawn(newItem, connectionToClient); 
    }

    public Vector3 SetRandomPosition()
    {
        float ranX = Random.Range(-15f, 15f);
        float ranY = Random.Range(-8f, 8f);
        
        return new Vector3(ranX, ranY, 0);
    }
}
