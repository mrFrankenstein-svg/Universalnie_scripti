using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//using static ScriptHubUpdateFunction;
public interface IScriptHubTechnicalInterface
{
    void OnEnable();
    void OnDisable();
}
public interface IScriptHubUpdateFunction: IScriptHubTechnicalInterface
{
    void ScriptHubUpdate();
}
public interface IScriptHubFixUpdateFunction : IScriptHubTechnicalInterface
{
    void ScriptHubFixUpdate();
}
public interface IScriptHubOneSecondUpdateFunction : IScriptHubTechnicalInterface
{
    void ScriptHubOneSecondUpdate();
}
//public enum ScriptHubUpdateFunction
//{
//    FunctionUpdate,
//    FunctionFixedUpdate,
//    FunctionOneSecondUpdate
//}

public class ScriptHub : MonoBehaviour
{

    //private static ScriptHub scriptHub;
    [SerializeField] List<object> updateScripts = new List<object>();
    [SerializeField] List<object> fixUpdateScripts = new List<object>();
    [SerializeField] List<object> oneSecondUpdate=new List<object>();
    public static Action<IScriptHubTechnicalInterface> OnAddToScriptsList;
    public static Action<IScriptHubTechnicalInterface> OnRemoveFromScriptsList;

    private void OnEnable()
    {
        OnAddToScriptsList += AddToScriptsList;
        OnRemoveFromScriptsList += RemoveFromScriptsList;
    }
    private void OnDisable()
    {
        OnAddToScriptsList -= AddToScriptsList;
        OnRemoveFromScriptsList -= RemoveFromScriptsList;
    }
    private void Start()
    {
        gameObject.name = "ScriptHub";
        StartCoroutine(OncePerSecond());
    }

    private void Update()
    {
        foreach (object obj in updateScripts)
        {
            try
            {
                IScriptHubUpdateFunction script = (IScriptHubUpdateFunction)obj;
                script.ScriptHubUpdate();
            }
            catch (Exception e)
            {
                Debug.LogError(this.name + " ERORR WILE TRYING DO ScriptHubUpdate() ON" + obj + "\n\n" + e);
            }
        }
    }
    private void FixedUpdate()
    {
        foreach (object obj in fixUpdateScripts)
        {
            try
            {
                IScriptHubFixUpdateFunction script = (IScriptHubFixUpdateFunction)obj;
                script.ScriptHubFixUpdate();
            }
            catch (Exception e)
            {
                Debug.Log(this.name + " ERORR WILE TRYING DO ScriptHubFixUpdate() ON" + obj + "\n\n\n" + e);
            }
        }
    }
    private IEnumerator OncePerSecond()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1f);

            foreach (object obj in oneSecondUpdate)
            {
                try
                {
                    IScriptHubOneSecondUpdateFunction script = (IScriptHubOneSecondUpdateFunction)obj;
                    script.ScriptHubOneSecondUpdate();
                }
                catch (Exception e)
                {
                    Debug.Log(this.name + " ERORR WILE TRYING DO OncePerSecond() ON " + obj + "\n\n\n" + e);
                }
            }
        }
    }
    #region Первая_Версия_Метода
    //работает хорошо
    //public static void AddToScriptsList(object script, ScriptHubUpdateFunction updateFunction)
    //{
    //    switch (updateFunction)
    //    {
    //        case FunctionUpdate:
    //            if (!scriptHub.updateScripts.Contains(script))
    //                scriptHub.updateScripts.Add(script);
    //            break;

    //        case FunctionFixedUpdate:
    //            if (!scriptHub.fixUpdateScripts.Contains(script))
    //                scriptHub.fixUpdateScripts.Add(script);
    //            break;

    //        case FunctionOneSecondUpdate:
    //            if (!scriptHub.oneSecondUpdate.Contains(script))
    //                scriptHub.oneSecondUpdate.Add(script);
    //            break;

    //        default:
    //            Debug.LogError("ScriptHub AddToScriptsList() error.");
    //            break;
    //    }

    //}
    #endregion
    private void AddToScriptsList(IScriptHubTechnicalInterface script)
    {
        
        byte tick=0;
        if (script is IScriptHubUpdateFunction)
        {
            if (!updateScripts.Contains(script))
                updateScripts.Add(script);
            tick++;
        }

        if (script is IScriptHubFixUpdateFunction)
        {
            if (!fixUpdateScripts.Contains(script))
                fixUpdateScripts.Add(script);
            tick++;
        }

        if (script is IScriptHubOneSecondUpdateFunction)
        {
            if (!oneSecondUpdate.Contains(script))
                oneSecondUpdate.Add(script);
            tick++;
        }
        
        if(tick==0)
            Debug.LogError("ScriptHub AddToScriptsList() error.");

    }
    private void RemoveFromScriptsList(IScriptHubTechnicalInterface script)
    {
        if (updateScripts.Contains(script))
            updateScripts.Remove(script);
        if (fixUpdateScripts.Contains(script))
            fixUpdateScripts.Remove(script);
        if (oneSecondUpdate.Contains(script))
            oneSecondUpdate.Remove(script);
    }
}
