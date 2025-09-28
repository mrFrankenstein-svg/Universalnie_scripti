using System;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;
public enum AnimState
{
    None,
    Death,
    CombatIdle,
    GuardIdle,
    Run,
    Shoot
}
[Serializable]
public class TimeOfAnimEvents
{
    public AnimState state; 
    [Header("Время срабатывания анимации. Min-0, Max-1.")]
    public float[] timeOfEvent = {-1};
}

[RequireComponent(typeof(Animator))]
public class AnimationEventsHolder : MonoBehaviour, IScriptHubUpdateFunction
{
    [SerializeField] Animator animator;
    AnimatorStateInfo stateOfAnim;

    private AnimState lastState;
    private float lastTime;

    [Header("Время срабатывания анимации")]
    [SerializeField] private List<TimeOfAnimEvents> time = new List<TimeOfAnimEvents>(); // Список моментов срабатываний анимации
    private Dictionary<AnimState, TimeOfAnimEvents> timeDict;    // Очередь для анимации


    //словарь для сопоставления хэшей с enum
    [SerializeField] private Dictionary<int, AnimState> stateMap;

    public void OnEnable()
    {
        ScriptHub.OnAddToScriptsList?.Invoke(this);
    }

    public void OnDisable()
    {
        ScriptHub.OnRemoveFromScriptsList?.Invoke(this);
    }
    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // Заполняем словарь для быстрого доступа к звукам по типу
        timeDict = new Dictionary<AnimState, TimeOfAnimEvents>();
        foreach (var entry in time)
        {
            if (!timeDict.ContainsKey(entry.state))
                timeDict.Add(entry.state, entry);
        }

        // Заполняем карту хэш  enum
        stateMap = new Dictionary<int, AnimState>
        {
            { Animator.StringToHash(AnimationsNameHolder.deathAnimationName), AnimState.Death },
            { Animator.StringToHash(AnimationsNameHolder.combatIdleAnimationName), AnimState.CombatIdle },
            { Animator.StringToHash(AnimationsNameHolder.guardIdleAnimationName), AnimState.GuardIdle },
            { Animator.StringToHash(AnimationsNameHolder.runAnimationName), AnimState.Run },
            { Animator.StringToHash(AnimationsNameHolder.shootAnimationName), AnimState.Shoot },
        };
    }

    /// <summary>
    /// Получить текущее состояние анимации как enum
    /// </summary>
    public AnimState GetCurrentState(int layer = 0)
    {
        stateOfAnim = animator.GetCurrentAnimatorStateInfo(layer);
        int currentHash = stateOfAnim.shortNameHash;

        if (stateMap.TryGetValue(currentHash, out AnimState state))
        {
            return state;
        }

        return AnimState.None;
    }

    public void ScriptHubUpdate()
    {
        AnimState currentState = GetCurrentState();
        float timeOfCurrentAnimation = ((stateOfAnim.normalizedTime % 1f) * stateOfAnim.length);
        timeOfCurrentAnimation = (float)Math.Round(timeOfCurrentAnimation, 2);

        if (timeOfCurrentAnimation == 0)
        {
            lastState = AnimState.None;
            lastTime = -1;
        }

        if (!timeDict.TryGetValue(currentState, out var entry) || entry.timeOfEvent[0]==-1)
        {
            Debug.LogWarning($"[AnimationEventsHolder] Некоректное время срабатывания анимации. {entry}");
            return;
        }
        for (int i = 0; i < entry.timeOfEvent.Length; i++)
        {
            if (timeOfCurrentAnimation == entry.timeOfEvent[i])
            {
                if (currentState != lastState)
                {
                    lastState=currentState;
                    lastTime = -1;
                }
                if(entry.timeOfEvent[i]!= lastTime)
                {
                    lastTime=entry.timeOfEvent[i];
                    PlayEvent(currentState);
                }
                
            }
        }

        
    }
    private void PlayEvent(AnimState state)
    {
        switch (state)
        {
            case AnimState.None:
                break;
            case AnimState.Death:
                break;
            case AnimState.CombatIdle:
                break;
            case AnimState.GuardIdle:
                break;
            case AnimState.Run:
                Instance.Play(SoundType.Footstep,transform.position);
                break;
            case AnimState.Shoot:
                break;
            default:
                Debug.Log("Неизвестное состояние");
                break;
        }
    }
}
