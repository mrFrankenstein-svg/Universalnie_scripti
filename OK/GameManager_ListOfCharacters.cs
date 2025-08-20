using UnityEngine;

[Tooltip("İòîò ñêğèïò ñàçäàí äëÿ êàòàëîãèçàöèè ïåğñîíàæåé íà ñöåíå.")]
public class GameManager_ListOfCharacters
{
    public GameObject objectItself { get; }
    public ÑharacterStats stats;
    public GameManager_ListOfCharacters(GameObject obj, ÑharacterStats ñharacterStats)
    {
        objectItself= obj;
        stats=ñharacterStats;
    }
}
