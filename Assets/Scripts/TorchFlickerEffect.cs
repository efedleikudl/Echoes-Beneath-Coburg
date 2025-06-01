using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light torchLight;
    public float minIntensity = 2f;
    public float maxIntensity = 5f;

    void Update()
    {
        torchLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}