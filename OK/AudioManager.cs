/* ������ ���
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

    [Header(" ������ ��������� ������")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    [Header(" ��� �������� � AudioSource")]
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] private GameObject audioSourcePrefab;

    private Dictionary<SoundType, AudioClip> soundDict = new Dictionary<SoundType, AudioClip>();
    private Queue<AudioSource> audioPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // �������� ������� ������
        foreach (var entry in sounds)
        {
            if (!soundDict.ContainsKey(entry.type))
                soundDict.Add(entry.type, entry.clip);
        }

        // �������� ���� ����������
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
            Debug.LogWarning($" ���� {type} �� ������ � �������!");
            return;
        }

        // ����� AudioSource �� ���� ��� ������� �����
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
            Debug.LogWarning($" ���� {type} �� ������ � �������!");
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
        obj.transform.SetParent(transform); // ���������� � AudioManager
        audioPool.Enqueue(source);
    }
}
*/
using System;
using System.Collections.Generic;
using UnityEngine;

// ��������� ������
public enum SoundType
{
    Gunshot,
    Explosion,
    Footstep,
    UI
}

// �������� ������ ��� ���������
[Serializable] // ��� ������� ����� ����� ����������� � ����������
public class SoundEntry
{
    public SoundType type;            // ���������
    public AudioClip[] clips;         // ������ ������ ��� ���������
    public int maxSimultaneous = 5;   // ������������ ���������� ������������� ���������������

    [Header("��������� �����������")]
    public Vector2 volumeRange = new Vector2(0.9f, 1f); // ��������� ���������
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f); // ��������� ����
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // �������� ��� ����������� �������

    [Header("��������� ����������")]
    [SerializeField] private int initialPoolSize = 10;       // ��������� ������ ���� AudioSource
    [SerializeField] private AudioSource audioSourcePrefab;  // ������ ��������� �����

    [Header("����� �� ����������")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>(); // ������ ��������� ������

    private Dictionary<SoundType, SoundEntry> soundDict;    // ������� ��� �������� ������� � ������
    private Queue<AudioSource> audioPool = new Queue<AudioSource>(); // ��� ��������� ����������
    private Dictionary<AudioClip, int> activeClips = new Dictionary<AudioClip, int>(); // ������� ��� ���� ���� ������

    private void Awake()
    {
        // ���������� ���������
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject); // ��������� ������ ����� �������

        // ��������� ������� ��� �������� ������� � ������ �� ����
        soundDict = new Dictionary<SoundType, SoundEntry>();
        foreach (var entry in sounds)
        {
            if (!soundDict.ContainsKey(entry.type))
                soundDict.Add(entry.type, entry);
        }

        // ������� ��������� ��� ����������
        for (int i = 0; i < initialPoolSize; i++)
        {
            audioPool.Enqueue(CreateNewSource());
        }
    }

    // �������� ������ ��������� �����
    private AudioSource CreateNewSource()
    {
        var source = Instantiate(audioSourcePrefab, transform); // ������� �� �������
        source.playOnAwake = false;                             // ����� �� ����� �������������
        return source;
    }

    /// <summary>
    /// ������������� ���� ���������� ����
    /// </summary>
    public void Play(SoundType type, Vector3 position, Transform parent = null)
    {
        // ���������, ���� �� � ������� ������ ���������
        if (!soundDict.TryGetValue(type, out var entry) || entry.clips.Length == 0)
        {
            Debug.LogWarning($"[AudioManager] ��� ������ ��� ��������� {type}");
            return;
        }

        // �������� ��������� ���� �� ���������
        var clip = entry.clips[UnityEngine.Random.Range(0, entry.clips.Length)];

        // ���������, �� �������� �� ����� ������������� ��������������� ����� �����
        if (activeClips.TryGetValue(clip, out int count) && count >= entry.maxSimultaneous)
            return;

        // ����� �������� �� ���� ��� ������� �����
        var source = audioPool.Count > 0 ? audioPool.Dequeue() : CreateNewSource();

        // ����������� ��������
        source.clip = clip;
        source.transform.position = position;
        source.transform.SetParent(parent != null ? parent : transform);

        // ��������� ��������� � ����
        source.volume = UnityEngine.Random.Range(entry.volumeRange.x, entry.volumeRange.y);
        source.pitch = UnityEngine.Random.Range(entry.pitchRange.x, entry.pitchRange.y);

        // �������� � �����������
        source.gameObject.SetActive(true);
        source.Play();

        // ����������� ������� �������� ��������������� ����� �����
        if (!activeClips.ContainsKey(clip)) activeClips[clip] = 0;
        activeClips[clip]++;

        // ����, ���� ���� ��������, � ���������� �������� � ���
        StartCoroutine(ReturnToPoolAfterPlay(source, clip));
    }

    // ��������� ��� �������� ��������� � ��� ����� ��������� �����
    private System.Collections.IEnumerator ReturnToPoolAfterPlay(AudioSource source, AudioClip clip)
    {
        yield return new WaitWhile(() => source.isPlaying); // ���� ����� ���������������

        // ��������� ������� �������� ������
        if (activeClips.ContainsKey(clip))
        {
            activeClips[clip]--;
            if (activeClips[clip] <= 0)
                activeClips.Remove(clip);
        }

        // ���������� �������� � ���
        source.gameObject.SetActive(false);
        source.clip = null;
        source.transform.SetParent(transform);
        audioPool.Enqueue(source);
    }
}
