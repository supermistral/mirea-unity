using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingBox : MonoBehaviour
{
    [SerializeField]
    private float _rotatingSpeed, _waitDelay;

    public int RotateValue;

    private void Start()
    {
        RotateValue = 0;
        StartCoroutine(Rotate());
    }

    private IEnumerator Rotate()
    {
        float timePassed = 0f;

        Vector3 rotation = transform.rotation.eulerAngles;
        RotateValue = _rotatingSpeed > 0 ? 1 : -1;

        while (timePassed < 1f)
        {
            transform.Rotate(_rotatingSpeed * Time.deltaTime * Vector3.forward);
            timePassed += Time.deltaTime;
            yield return null;
        }

        rotation.z += _rotatingSpeed;
        transform.rotation = Quaternion.Euler(rotation);
        RotateValue = 0;
        yield return new WaitForSeconds(_waitDelay);

        StartCoroutine(Rotate());
    }
}
