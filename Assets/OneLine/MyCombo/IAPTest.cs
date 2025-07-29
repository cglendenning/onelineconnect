using UnityEngine;

public class IAPTest : MonoBehaviour
{
    void Start()
    {
#if IAP && UNITY_PURCHASING
        Debug.Log("IAP symbols are working correctly!");
#else
        Debug.LogError("IAP symbols are NOT working!");
#endif
    }
} 