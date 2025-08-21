using System;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform coin;

    // Syncvar : 대상이 변경될 때 알려주는 기능
    [SyncVar(hook = nameof(OncoinPositionChanged))] // hook : 값이 변경될 때 이벤트 실행
    public Vector3 coinPosition;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public override void OnStartServer()
    {
        base.OnStartServer();
        MoveCoin();
    }
    [Server]
    public void MoveCoin()
    {
        float ranX = Random.Range(-15f, 15f);
        float ranY = Random.Range(-8f, 8f);

        coinPosition = new Vector3(ranX, ranY, 0);
    }

    public void OncoinPositionChanged(Vector3 prevPos, Vector3 newPos)
    {
        coin.position = newPos;
    }
}
