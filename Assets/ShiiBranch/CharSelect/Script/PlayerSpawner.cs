using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject meleePlayerPrefab;
    [SerializeField] private GameObject rangedPlayerPrefab;

    [SerializeField] private Transform spawnPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (SkillManager.Instance != null)
        {
            MoveExistingPlayer();
            return;
        }
        SpawnNewPlayer();
    }
    private void SpawnNewPlayer()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject prefabToSpawn = null;

        switch (GameContext.SelectedCharacter)
        {
            case GameContext.CharacterType.Melee:
                prefabToSpawn = meleePlayerPrefab;
                break;
            case GameContext.CharacterType.Ranged:
                prefabToSpawn = rangedPlayerPrefab;
                break;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, pos, Quaternion.identity);
            Debug.Log($":{GameContext.SelectedCharacter}");
        }
        else
        {
            Debug.LogError("`プレイヤーPrefabがない！");
        }
    }

    private void MoveExistingPlayer()
    {
        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        SkillManager.Instance.transform.position = pos;
        Debug.Log("プレイヤーもう存在している");
    }
}
