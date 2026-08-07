using UnityEngine;

public class seedTimeSet : MonoBehaviour
{
    [SerializeField] public float seedTime = 0f;

    public void SetSeedTime(float time)
    {
        seedTime = time;
    }

    public float GetSeedTime()
    {
        return seedTime;
    }
}