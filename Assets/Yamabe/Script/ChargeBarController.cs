using UnityEngine;

public class ChargeBarController : MonoBehaviour
{
    void LateUpdate()
    {
        // 親（Player）がどっちを向いていても、自分は常に右向きを維持
        Vector3 parentScale = transform.parent.localScale;
        transform.localScale = new Vector3(
            Mathf.Sign(parentScale.x) * Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
