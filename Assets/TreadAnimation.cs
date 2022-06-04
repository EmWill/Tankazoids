using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadAnimation : MonoBehaviour
{
    public Material material1;
    public Material material2;

    private int time = 0;
    private bool isMaterial1;

    void Update()
    {
        time = (time + 1) % 5;

        if (Random.RandomRange(0, 3000) < 2500)
        {
            if (isMaterial1)
            {
                isMaterial1 = false;
                gameObject.GetComponent<MeshRenderer>().material = material2;
            } else
            {
                isMaterial1 = true;
                gameObject.GetComponent<MeshRenderer>().material = material1;
            }
        }
    }
}
