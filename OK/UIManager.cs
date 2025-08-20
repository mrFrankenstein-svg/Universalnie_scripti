using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UniversalTMPSetter;

public class UIManager : MonoBehaviour
{
    public GameObject targetGameObject;
    public HealthSystem playerHealth;

    private void OnEnable()
    {
        GameManager.OnCaracterHasIncreasedTheValue += updatingTheScoreText;
    }
    private void OnDisable()
    {
        GameManager.OnCaracterHasIncreasedTheValue -= updatingTheScoreText;
    }
    private void updatingTheScoreText(GameObject obj, byte value)
    {
        if (obj == targetGameObject)
        {
            SetTextByFileNameOrTag(UniversalTMPSetterSearchEnum.Name, "Score", value.ToString());
        }
    }

}

