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
    private List<GameObject> wraptors = new List<GameObject>();
    [SerializeField] private GameObject wrummelPrefab;
    [SerializeField] private GameObject wraptorPrefab;
    [SerializeField] private int enemySpawnCount = 20;
    [SerializeField] private int enemyInRoomLimit = 10;
    [SerializeField] private int enemiesToSpawnInRoomMin = 6;
    [SerializeField] private int enemiesToSpawnInRoomMax = 6;
    [SerializeField] private int extraEnemiesPerPlayerMin = 1;
    [SerializeField] private int extraEnemiesPerPlayerMax = 3;
    [SerializeField] private float wraptorSpawnChance = 20;
    [SerializeField] private float wraptorSpawnChanceLimit = 65;
    [SerializeField] private float wraptorSpawnChanceModifier = 2;
    [SerializeField] private float eatSpawnChance = 0;
    [SerializeField] private float eatSpawnChanceModifier = 2;

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

        InstantiateEnemiesInPool(wrummelPrefab, wrummels);

        if (wraptorPrefab != null)
        {
            InstantiateEnemiesInPool(wraptorPrefab, wraptors);
        }        
    }

    private void InstantiateEnemiesInPool(GameObject prefab, List<GameObject> enemiesList)
    {
        for (int i = 0; i < enemySpawnCount; i++)
        {
            GameObject enemy = PhotonFunctionHandler.InstantiateGameObject(prefab, Vector3.zero, Quaternion.identity);
            enemy.SetActive(false);
            enemyManager.StatsOfAllEnemies.Add(enemy.transform, enemy.GetComponent<EnemyStats>());
            enemiesList.Add(enemy);
        }
    }

    public List<GeneratedEnemyInfo> GenerateEnemies(float countMultiplier)
    {
        List<GeneratedEnemyInfo> generatedEnemyInfos = new List<GeneratedEnemyInfo>();

        int randomEnemyCount = Mathf.RoundToInt(Random.Range(enemiesToSpawnInRoomMin, enemiesToSpawnInRoomMax) * countMultiplier);

        for (int i = 0; i < playerManager.PlayersInGame.Count - 1; i++)
        {
            randomEnemyCount += Random.Range(extraEnemiesPerPlayerMin, extraEnemiesPerPlayerMax);
        }

        randomEnemyCount += difficultyManager.DifficultyLevel;

        int wrummelCount = 0;
        int wraptorCount = 0;

        float wraptorChance = wraptorSpawnChance + (wraptorSpawnChanceModifier * difficultyManager.DifficultyLevel);

        if (wraptorChance > wraptorSpawnChanceLimit)
        {
            wraptorChance = wraptorSpawnChanceLimit;
        }

        for (int i = 0; i < randomEnemyCount; i++)
        {
            float rand = Random.Range(0, 100);

            if (rand > wraptorChance)
            {
                //Spawn wrummel
                wrummelCount++;
            }
            else
            {
                //Spawn wraptor
                wraptorCount++;
            }
        }
        
        GenerateEnemy(generatedEnemyInfos, wrummels, wrummelCount);
        GenerateEnemy(generatedEnemyInfos, wraptors, wraptorCount);
        float combinedPercentages = 0;

        for (int i = 1; i < generatedEnemyInfos.Count; i++)
        {
            GeneratedEnemyInfo generatedEnemyInfo = generatedEnemyInfos[i];

            float basePercent = 1 / generatedEnemyInfos.Count;
            generatedEnemyInfo.spawnPercent = generatedEnemyInfo.enemyCount / generatedEnemyInfos[0].enemyCount * basePercent;
            combinedPercentages += generatedEnemyInfo.spawnPercent;
        }

        generatedEnemyInfos[0].spawnPercent = 1 - combinedPercentages;

        return generatedEnemyInfos;
    }

    private void GenerateEnemy(List<GeneratedEnemyInfo> generatedEnemyInfos, List<GameObject> enemyReferenceList, int count)
    {
        if (count <= 0)
        {
            return;
        }

        GeneratedEnemyInfo enemy = new GeneratedEnemyInfo();
        enemy.enemiesList = new List<GameObject>(enemyReferenceList);

        for (int i = enemyInRoomLimit; i < enemy.enemiesList.Count; i++)
        {
            enemy.availableEnemiesList.Add(enemy.enemiesList[i]);
        }

        foreach (GameObject item in enemy.availableEnemiesList)
        {
            enemy.enemiesList.Remove(item);
        }

        enemy.startingEnemiesList = new List<GameObject>(enemy.enemiesList);
        enemy.startingAvailableEnemiesList = new List<GameObject>(enemy.availableEnemiesList);
        
        generatedEnemyInfos.Add(enemy);

        enemy.enemyCount += count;
    }

    [System.Serializable]
    public class GeneratedEnemyInfo
    {
        public List<GameObject> enemiesList;
        public List<GameObject> availableEnemiesList = new List<GameObject>();
        public List<GameObject> startingEnemiesList = new List<GameObject>();
        public List<GameObject> startingAvailableEnemiesList = new List<GameObject>();
        public int enemyCount = 0;
        public int priority = 0;
        public float spawnPercent = 0;
    }
}
