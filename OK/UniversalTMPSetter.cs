using UnityEngine;
using TMPro;

[Tooltip("���� ������ ������������� ����� � TextMeshProUGUI ��� TextMeshPro.")]
public enum UniversalTMPSetterSearchEnum
{
    Tag,
    Name
}
public static class UniversalTMPSetter 
{
    //[Header("������ ������ �������")]
    //public TextMeshProUGUI uiText;        // ���� UI-����� (Canvas)
    //public TextMeshPro worldText;         // ���� 3D-����� � �����
    //public string objectName;             // ��� ������� ��� ������
    //public string objectTag;              // ��� ������� ��� ������

    //[Header("����� ��� ���������")]
    //[TextArea]
    //public string newText;

    //void Start()
    //{
    //    AssignText(newText);
    //}

    /// <summary>
    /// ������������� ��������� ������ � TextMeshPro
    /// </summary>
    
    public static void SetTextByFileNameOrTag(UniversalTMPSetterSearchEnum function, string target, string text)
    {
        GameObject obj;
        switch (function)
        {
            case UniversalTMPSetterSearchEnum.Tag:
                #region �����_��_����

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
                #region �����_��_�����

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
    /// �������� ���������� ����� � ��������� �������
    /// </summary>
    private static void TrySetTMP(GameObject obj, string text)
    {
        var tmpUI = obj.GetComponent<TextMeshProUGUI>();
        var tmp3D = obj.GetComponent<TextMeshPro>();

        if (tmpUI != null) tmpUI.text = text;
        else if (tmp3D != null) tmp3D.text = text;
        else Debug.LogError($"UniversalTMPSetter: �� ������� {obj.name} ��� ���������� TextMeshPro.");
    }
}
