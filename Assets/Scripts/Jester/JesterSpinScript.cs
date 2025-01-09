using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class JesterSpinScript : MonoBehaviour
{
    public float JesterSpeed;
    [SerializeField] GameObject target;
    [SerializeField] GameObject bomb;
    [SerializeField] GameObject turret;
    [SerializeField] float _attackSpeed = 2.0f;
    private float _attackTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(LaunchBombs());
        transform.Rotate(0f, JesterSpeed * Time.deltaTime, 0f, Space.Self);

    }

    void Fire()
    {
        GameObject b = Instantiate(bomb, turret.transform.position, turret.transform.rotation);
        Vector3 direction = (target.transform.position - turret.transform.position).normalized;
        b.GetComponent<Rigidbody>().AddForce(direction * 500);
    }

    private IEnumerator LaunchBombs()
    {
        _attackTime += Time.deltaTime;
        if (_attackTime >= _attackSpeed)
        {
            Fire();
            _attackTime = 0;
        }

        yield return new WaitForSeconds(3f);

    }
}
