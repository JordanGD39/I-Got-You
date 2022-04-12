using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyGenerator : MonoBehaviour
{
    private PlayerManager playerManager;
    private EnemyManager enemyManager;
    private DifficultyManager difficultyManager;

    private List<GameObject> wrummels = new List<GameObject>();
    [SerializeField] private GameObject wrummelPrefab;
    [SerializeField] private int wrummelCount = 10;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            Destroy(this);
            return;
        }

        playerManager = FindObjectOfType<PlayerManager>();
        enemyManager = FindObjectOfType<EnemyManager>();
        difficultyManager = FindObjectOfType<DifficultyManager>();

        for (int i = 0; i < wrummelCount; i++)
        {
            GameObject wrummel = PhotonFunctionHandler.InstantiateGameObject(wrummelPrefab, Vector3.zero, Quaternion.identity);
            wrummel.SetActive(false);
            enemyManager.StatsOfAllEnemies.Add(wrummel.transform.GetChild(0), wrummel.GetComponent<EnemyStats>());
            wrummels.Add(wrummel);
        }
    }

    public List<GeneratedEnemyInfo> GenerateEnemies()
    {
        List<GeneratedEnemyInfo> generatedEnemyInfos = new List<GeneratedEnemyInfo>();

        GeneratedEnemyInfo wrummelEnemy = new GeneratedEnemyInfo();
        wrummelEnemy.enemiesList = wrummels;
        generatedEnemyInfos.Add(wrummelEnemy);

        for (int i = 0; i < playerManager.PlayersInGame.Count; i++)
        {
            //wrummelEnemy.enemyCount += Random.Range(6, 8) + difficultyManager.DifficultyLevel;
            wrummelEnemy.enemyCount += 40;
        }

        return generatedEnemyInfos;
    }

    public class GeneratedEnemyInfo
    {
        public List<GameObject> enemiesList;
        public List<GameObject> deadEnemiesList = new List<GameObject>();
        public int enemyCount = 0;
        public int priority = 0;
    }
}
