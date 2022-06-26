using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalTypes;

public class AirEffector : MonoBehaviour
{
    public AirEffectorType airEffectorType;
    public float speed;
    public Vector2 direction;
    private BoxCollider2D _collider;
    // Start is called before the first frame update
    void Start()
    {
        direction = transform.up;
        _collider = gameObject.GetComponent<BoxCollider2D>();
    }

    public void DeactivateEffector()
    {
        StartCoroutine("DeactivateEffectorCoroutine");
    }

    IEnumerator DeactivateEffectorCoroutine()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(.5f);
        _collider.enabled = true;
    }
}
