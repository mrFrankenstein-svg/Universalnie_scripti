using UnityEngine;

[Tooltip("���� ������ ������ ��� ������������� ���������� �� �����.")]
public class GameManager_ListOfCharacters
{
    public GameObject objectItself { get; }
    public �haracterStats stats;
    public GameManager_ListOfCharacters(GameObject obj, �haracterStats �haracterStats)
    {
        objectItself= obj;
        stats=�haracterStats;
    }
}
