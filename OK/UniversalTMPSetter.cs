using UnityEngine;
using TMPro;

[Tooltip("Этот скрипт устанавливает текст в TextMeshProUGUI или TextMeshPro.")]
public enum UniversalTMPSetterSearchEnum
{
    Tag,
    Name
}
public static class UniversalTMPSetter 
{
    //[Header("Способ поиска объекта")]
    //public TextMeshProUGUI uiText;        // Если UI-текст (Canvas)
    //public TextMeshPro worldText;         // Если 3D-текст в сцене
    //public string objectName;             // Имя объекта для поиска
    //public string objectTag;              // Тег объекта для поиска

    //[Header("Текст для установки")]
    //[TextArea]
    //public string newText;

    //void Start()
    //{
    //    AssignText(newText);
    //}

    /// <summary>
    /// Универсальная установка текста в TextMeshPro
    /// </summary>
    
    public static void SetTextByFileNameOrTag(UniversalTMPSetterSearchEnum function, string target, string text)
    {
        GameObject obj;
        switch (function)
        {
            case UniversalTMPSetterSearchEnum.Tag:
                #region Поиск_по_тегу

                obj = GameObject.FindGameObjectWithTag(target);
                if (obj != null)
                {
                    TrySetTMP(obj, text);
                    return;
                }
                else
                    Debug.LogError(ErorsList.OBJECT_FIND_EROR + target);

                    break;
                #endregion
            case UniversalTMPSetterSearchEnum.Name:
                #region Поиск_по_имени

                obj = GameObject.Find(target);
                if (obj != null)
                {
                    TrySetTMP(obj, text);
                    return;
                }
                else
                    Debug.LogError(ErorsList.OBJECT_FIND_EROR + target);

                #endregion
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Пытается установить текст в найденном объекте
    /// </summary>
    private static void TrySetTMP(GameObject obj, string text)
    {
        var tmpUI = obj.GetComponent<TextMeshProUGUI>();
        var tmp3D = obj.GetComponent<TextMeshPro>();

        if (tmpUI != null) tmpUI.text = text;
        else if (tmp3D != null) tmp3D.text = text;
        else Debug.LogError($"UniversalTMPSetter: На объекте {obj.name} нет компонента TextMeshPro.");
    }
}
