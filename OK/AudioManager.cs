/* старый код
using System.Collections.Generic;
using UnityEngine;

public enum SoundType
{
    Explosion,
    Footstep,
    Gunshot,
    Jump,
    Music
}

[System.Serializable]
public class SoundEntry
{
    public SoundType type;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header(" Список доступных звуков")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    [Header(" Пул объектов с AudioSource")]
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] private GameObject audioSourcePrefab;

    private Dictionary<SoundType, AudioClip> soundDict = new Dictionary<SoundType, AudioClip>();
    private Queue<AudioSource> audioPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // Создание словаря звуков
        foreach (var entry in sounds)
        {
            if (!soundDict.ContainsKey(entry.type))
                soundDict.Add(entry.type, entry.clip);
        }

        // Создание пула источников
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        GameObject obj = Instantiate(audioSourcePrefab, transform);
        AudioSource src = obj.GetComponent<AudioSource>();
        obj.SetActive(false);
        audioPool.Enqueue(src);
        return src;
    }

    public void Play(SoundType type, Vector3 position)
    {
        if (!soundDict.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($" Звук {type} не найден в словаре!");
            return;
        }

        // Берем AudioSource из пула или создаем новый
        AudioSource source = audioPool.Count > 0 ? audioPool.Dequeue() : CreateNewAudioSource();

        GameObject obj = source.gameObject;
        obj.transform.position = position;
        obj.SetActive(true);

        source.clip = clip;
        source.Play();

        StartCoroutine(ReturnToPoolAfterPlayback(source));
    }

    public void PlayAttached(SoundType type, Transform parent)
    {
        if (!soundDict.TryGetValue(type, out AudioClip clip))
        {
            Debug.LogWarning($" Звук {type} не найден в словаре!");
            return;
        }

        AudioSource source = audioPool.Count > 0 ? audioPool.Dequeue() : CreateNewAudioSource();

        GameObject obj = source.gameObject;
        obj.transform.SetParent(parent);
        obj.transform.localPosition = Vector3.zero;
        obj.SetActive(true);

        source.clip = clip;
        source.Play();

        StartCoroutine(ReturnToPoolAfterPlayback(source, parent));
    }

    private System.Collections.IEnumerator ReturnToPoolAfterPlayback(AudioSource source, Transform parent = null)
    {
        yield return new WaitWhile(() => source.isPlaying);

        GameObject obj = source.gameObject;
        obj.SetActive(false);
        obj.transform.SetParent(transform); // возвращаем в AudioManager
        audioPool.Enqueue(source);
    }
}
*/
using System;
using System.Collections.Generic;
using UnityEngine;

// Категории звуков
public enum SoundType
{
    Gunshot,
    Explosion,
    Footstep,
    UI
}

// Описание звуков для категории
[Serializable] // Эта строчка чтобы класс показывался в инспекторе
public class SoundEntry
{
    public SoundType type;            // Категория
    public AudioClip[] clips;         // Список клипов для категории
    public int maxSimultaneous = 5;   // Максимальное количество одновременных воспроизведений

    [Header("Настройки случайности")]
    public Vector2 volumeRange = new Vector2(0.9f, 1f); // Случайная громкость
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f); // Случайный питч
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Синглтон для глобального доступа

    [Header("Настройки источников")]
    [SerializeField] private int initialPoolSize = 10;       // Начальный размер пула AudioSource
    [SerializeField] private AudioSource audioSourcePrefab;  // Префаб источника звука

    [Header("Звуки по категориям")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>(); // Список категорий звуков

    private Dictionary<SoundType, SoundEntry> soundDict;    // Словарь для быстрого доступа к звукам
    private Queue<AudioSource> audioPool = new Queue<AudioSource>(); // Пул свободных источников
    private Dictionary<AudioClip, int> activeClips = new Dictionary<AudioClip, int>(); // Сколько раз один клип играет

    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject); // Сохраняем объект между сценами

        // Заполняем словарь для быстрого доступа к звукам по типу
        soundDict = new Dictionary<SoundType, SoundEntry>();
        foreach (var entry in sounds)
        {
            if (!soundDict.ContainsKey(entry.type))
                soundDict.Add(entry.type, entry);
        }

        // Создаем начальный пул источников
        for (int i = 0; i < initialPoolSize; i++)
        {
            audioPool.Enqueue(CreateNewSource());
        }
    }

    // Создание нового источника звука
    private AudioSource CreateNewSource()
    {
        var source = Instantiate(audioSourcePrefab, transform); // Создаем из префаба
        source.playOnAwake = false;                             // Чтобы не играл автоматически
        return source;
    }

    /// <summary>
    /// Воспроизвести звук указанного типа
    /// </summary>
    public void Play(SoundType type, Vector3 position, Transform parent = null)
    {
        // Проверяем, есть ли в словаре нужная категория
        if (!soundDict.TryGetValue(type, out var entry) || entry.clips.Length == 0)
        {
            Debug.LogWarning($"[AudioManager] Нет звуков для категории {type}");
            return;
        }

        // Выбираем случайный клип из категории
        var clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];

        // Проверяем, не превышен ли лимит одновременных воспроизведений этого клипа
        if (activeClips.TryGetValue(clip, out int count) && count >= entry.maxSimultaneous)
            return;

        // Берем источник из пула или создаем новый
        var source = audioPool.Count > 0 ? audioPool.Dequeue() : CreateNewSource();

        // Настраиваем источник
        source.clip = clip;
        source.transform.position = position;
        source.transform.SetParent(parent != null ? parent : transform);

        // Случайные громкость и питч
        source.volume = UnityEngine.Random.Range(entry.volumeRange.x, entry.volumeRange.y);
        source.pitch = UnityEngine.Random.Range(entry.pitchRange.x, entry.pitchRange.y);

        // Включаем и проигрываем
        source.gameObject.SetActive(true);
        source.Play();

        // Увеличиваем счетчик активных воспроизведений этого клипа
        if (!activeClips.ContainsKey(clip)) activeClips[clip] = 0;
        activeClips[clip]++;

        // Ждем, пока звук отыграет, и возвращаем источник в пул
        StartCoroutine(ReturnToPoolAfterPlay(source, clip));
    }

    // Короутина для возврата источника в пул после окончания звука
    private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, AudioClip clip)
    {
        yield return new WaitWhile(() => source.isPlaying); // Ждем конца воспроизведения

        // Уменьшаем счетчик активных клипов
        if (activeClips.ContainsKey(clip))
        {
            activeClips[clip]--;
            if (activeClips[clip] <= 0)
                activeClips.Remove(clip);
        }

        // Возвращаем источник в пул
        source.gameObject.SetActive(false);
        source.clip = null;
        source.transform.SetParent(transform);
        audioPool.Enqueue(source);
    }
}
