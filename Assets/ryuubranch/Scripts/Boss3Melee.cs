using UnityEngine;

public class Boss3Melee : MonoBehaviour
{
    public Collider2D meleeCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            if (meleeCollider) meleeCollider.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (meleeCollider == null) { Debug.Log("isnull"); }
        if(meleeCollider.enabled == true) { Debug.Log("isopened"); }
    }



    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("hit");
            Player_Controller playerCtrl = collision.GetComponent<Player_Controller>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeDamage(1f);
            }
        }
    }
}
