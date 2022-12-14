using UnityEngine;

public class CP : MonoBehaviour
{
    GameManager gm;
    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!gm.CPs.Contains(int.Parse(name)))
        {
            if (gm.CPs.Count == 0 || gm.CPs.Count == gm.CPs[gm.CPs.Count - 1])
                gm.CPs.Add(int.Parse(name));
            if (gm.CPs[gm.CPs.Count - 1] == 19)
            {
                gm.Win();
            }
        }
    }
}
