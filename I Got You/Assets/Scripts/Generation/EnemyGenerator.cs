using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyGenerator : MonoBehaviour
{
    private PlayerManager playerManager_PlayerManager;
    private EnemyManager enemyManager_EnemyManager;
    private DifficultyManager difficultyManager_DifficultyManager;

    private List<GameObject> wrummels_List_GameObject = new List<GameObject>();
    private List<GameObject> wraptors_List_GameObject = new List<GameObject>();
    [SerializeField] private GameObject wrummelPrefab_GameObject;
    [SerializeField] private GameObject wraptorPrefab_GameObject;
    [SerializeField] private int enemySpawnCount_int = 20;
    [SerializeField] private int enemyInRoomLimit_int = 10;
    [SerializeField] private int enemiesToSpawnInRoomMin_int = 6;
    [SerializeField] private int enemiesToSpawnInRoomMax_int = 6;
    [SerializeField] private float wraptorSpawnChance_float = 20;
    [SerializeField] private float wraptorSpawnChanceLimit_float = 65;
    [SerializeField] private float wraptorSpawnChanceModifier_float = 2;
    [SerializeField] private float eatSpawnChance_float = 0;
    [SerializeField] private float eatSpawnChanceModifier_float = 2;

    // Start is called before the first frame update
    void Start_void()
    {
        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            Destroy(this);
            return;
        }

        playerManager_PlayerManager = FindObjectOfType<PlayerManager>();
        enemyManager_EnemyManager = FindObjectOfType<EnemyManager>();
        difficultyManager_DifficultyManager = FindObjectOfType<DifficultyManager>();

        InstantiateEnemiesInPool_void(wrummelPrefab_GameObject, wrummels_List_GameObject);

        if (wraptorPrefab_GameObject != null)
        {
            InstantiateEnemiesInPool_void(wraptorPrefab_GameObject, wraptors_List_GameObject);
        }        
    }

    private void InstantiateEnemiesInPool_void(GameObject prefab, List<GameObject> enemiesList)
    {
        for (int i = 0; i < enemySpawnCount_int; i++)
        {
            GameObject enemy = PhotonFunctionHandler.InstantiateGameObject_GameObject(prefab, Vector3.zero, Quaternion.identity);
            enemy.SetActive(false);
            enemyManager_EnemyManager.StatsOfAllEnemies.Add(enemy.transform, enemy.GetComponent<EnemyStats>());
            enemiesList.Add(enemy);
        }
    }

    public List<GeneratedEnemyInfo> GenerateEnemies_List_GeneratedEnemyInfo()
    {
        List<GeneratedEnemyInfo> generatedEnemyInfos = new List<GeneratedEnemyInfo>();

        int randomEnemyCount = Random.Range(enemiesToSpawnInRoomMin_int, enemiesToSpawnInRoomMax_int) + difficultyManager_DifficultyManager.DifficultyLevel;

        int wrummelCount = 0;
        int wraptorCount = 0;

        float wraptorChance = wraptorSpawnChance_float + (wraptorSpawnChanceModifier_float * difficultyManager_DifficultyManager.DifficultyLevel);

        if (wraptorChance > wraptorSpawnChanceLimit_float)
        {
            wraptorChance = wraptorSpawnChanceLimit_float;
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
        
        GenerateEnemy_void(generatedEnemyInfos, wrummels_List_GameObject, wrummelCount);
        GenerateEnemy_void(generatedEnemyInfos, wraptors_List_GameObject, wraptorCount);

        return generatedEnemyInfos;
    }

    private void GenerateEnemy_void(List<GeneratedEnemyInfo> generatedEnemyInfos, List<GameObject> enemyReferenceList, int count)
    {
        if (count <= 0)
        {
            return;
        }

        GeneratedEnemyInfo enemy = new GeneratedEnemyInfo();
        enemy.enemiesList_List_GameObject = new List<GameObject>(enemyReferenceList);

        for (int i = enemyInRoomLimit_int; i < enemy.enemiesList_List_GameObject.Count; i++)
        {
            enemy.availableEnemiesList_List_GameObject.Add(enemy.enemiesList_List_GameObject[i]);
        }

        foreach (GameObject item in enemy.availableEnemiesList_List_GameObject)
        {
            enemy.enemiesList_List_GameObject.Remove(item);
        }

        generatedEnemyInfos.Add(enemy);

        for (int i = 0; i < playerManager_PlayerManager.PlayersInGame.Count; i++)
        {
            enemy.enemyCount_int += count;
        }
    }

    [System.Serializable]
    public class GeneratedEnemyInfo
    {
        public List<GameObject> enemiesList_List_GameObject;
        public List<GameObject> availableEnemiesList_List_GameObject = new List<GameObject>();
        public int enemyCount_int = 0;
        public int priority_int = 0;
    }
}
