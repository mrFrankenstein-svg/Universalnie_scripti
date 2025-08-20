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
