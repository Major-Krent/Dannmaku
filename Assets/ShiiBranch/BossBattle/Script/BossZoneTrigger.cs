using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    [SerializeField] private BossBattleManager battleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (battleManager != null)
            {
                battleManager.StartBossBattle();
            }
            Destroy(gameObject);

        }
    }
}
