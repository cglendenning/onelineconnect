using UnityEngine;

public class GameMaster : MonoBehaviour{
    public static GameMaster instance;
    private void Awake()
    {
        Debug.Log("=== GAME MASTER AWAKE DEBUG ===");
        Debug.Log("GameMaster.Awake() called");
        
        if (instance == null) 
        {
            instance = this;
            Debug.Log("GameMaster.instance set to this");
            
            // Check if Purchaser component exists
            var purchaser = GetComponent<Purchaser>();
            Debug.Log("Purchaser component: " + (purchaser != null ? "EXISTS" : "NULL"));
            if (purchaser != null)
            {
                Debug.Log("Purchaser.instance: " + (Purchaser.instance != null ? "EXISTS" : "NULL"));
            }
        }
        else if (instance != this)
        {
            Debug.Log("Destroying duplicate GameMaster");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Debug.Log("GameMaster set to DontDestroyOnLoad");
    }
}
