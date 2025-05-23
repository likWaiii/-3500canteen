// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class SoundManager : MonoBehaviour
// {
//     private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectVolume";

//     public static SoundManager Instance { get; private set; }

//     [SerializeField]
//     private AudioClipRefsSO audioClipRefsSO;

//     private float volume = 1f;

//     private void Awake()
//     {
//         Instance = this;

//         volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
//     }

//     private void Start()
//     {
//         DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
//         DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
//         CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
//         Player.Instance.OnPickedSomething += Player_OnPickedSomething;
//         BaseCounter.OnAnyObjectPlaceHere += BaseCounter_OnAnyObjectPlaceHere;
//         TrashCounter.OnAnyObjectTrash += TrashCounter_OnAnyObjectTrash;
//     }

//     private void TrashCounter_OnAnyObjectTrash(object sender, System.EventArgs e)
//     {
//         TrashCounter trashCounter = sender as TrashCounter;
//         PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
//     }

//     private void BaseCounter_OnAnyObjectPlaceHere(object sender, System.EventArgs e)
//     {
//         BaseCounter baseCounter = sender as BaseCounter;
//         PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
//     }

//     private void Player_OnPickedSomething(object sender, System.EventArgs e)
//     {
//         PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
//     }

//     private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
//     {
//         CuttingCounter cuttingCounter = sender as CuttingCounter;
//         PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position);
//     }

//     private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
//     {
//         DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
//         PlaySound(audioClipRefsSO.deliveryFail, deliveryCounter.transform.position);
//     }

//     private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
//     {
//         DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
//         PlaySound(audioClipRefsSO.deliverySucess, deliveryCounter.transform.position);
//     }

//     private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
//     {
//         PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
//     }

//     private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
//     {
//         AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
//     }

//     public void PlayFootstepSound(Vector3 position, float volume)
//     {
//         PlaySound(audioClipRefsSO.footStep, position, volume);
//     }

//     public void PlayCountDownSound()
//     {
//         PlaySound(audioClipRefsSO.warnings, Vector3.zero);
//     }

//     public void PlayWarningSound(Vector3 position)
//     {
//         PlaySound(audioClipRefsSO.warnings, position);
//     }

//     public void ChangeVolume()
//     {
//         volume += .1f;

//         if (volume > 1f)
//         {
//             volume = 0f;
//         }

//         PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
//         PlayerPrefs.Save();
//     }

//     public float GetVolume()
//     {
//         return volume;
//     }
// }
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectVolume";

    public static SoundManager Instance { get; private set; }

    [SerializeField]
    private AudioClipRefsSO audioClipRefsSO;

    private float volume = 1f;
    private Dictionary<string, float> lastPlayedTime = new Dictionary<string, float>();
    private float minInterval = 0.1f; // 最小播放间隔

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
        Debug.Log("[SoundManager] 在 Awake 中初始化完成");
    }

    private void Start()
    {
        StartCoroutine(SubscribeToNetworkEvents());
    }

    private IEnumerator SubscribeToNetworkEvents()
    {
        while (DeliveryManager.Instance == null)
        {
            Debug.Log("[SoundManager] 等待 DeliveryManager 初始化...");
            yield return null;
        }
        DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
        Debug.Log("[SoundManager] 已订阅 DeliveryManager 事件");

        while (Player.Instance == null)
        {
            Debug.Log("[SoundManager] 等待 Player 初始化...");
            yield return null;
        }
        Player.Instance.OnPickedSomething -= Player_OnPickedSomething;
        Player.Instance.OnPickedSomething += Player_OnPickedSomething;
        Debug.Log("[SoundManager] 已订阅 Player 事件");

        CuttingCounter.OnAnyCut -= CuttingCounter_OnAnyCut;
        BaseCounter.OnAnyObjectPlaceHere -= BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrash -= TrashCounter_OnAnyObjectTrash;
        CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
        BaseCounter.OnAnyObjectPlaceHere += BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrash += TrashCounter_OnAnyObjectTrash;
        Debug.Log("[SoundManager] 已订阅 Cutting/Base/Trash Counter 事件");
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
            DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
        }
        if (Player.Instance != null)
        {
            Player.Instance.OnPickedSomething -= Player_OnPickedSomething;
        }
        CuttingCounter.OnAnyCut -= CuttingCounter_OnAnyCut;
        BaseCounter.OnAnyObjectPlaceHere -= BaseCounter_OnAnyObjectPlaceHere;
        TrashCounter.OnAnyObjectTrash -= TrashCounter_OnAnyObjectTrash;
        Debug.Log("[SoundManager] 已取消所有事件订阅");
    }

    private void TrashCounter_OnAnyObjectTrash(object sender, System.EventArgs e)
    {
        TrashCounter trashCounter = sender as TrashCounter;
        if (trashCounter != null)
            PlaySound(audioClipRefsSO.trash, trashCounter.transform.position);
    }

    private void BaseCounter_OnAnyObjectPlaceHere(object sender, System.EventArgs e)
    {
        BaseCounter baseCounter = sender as BaseCounter;
        if (baseCounter != null)
            PlaySound(audioClipRefsSO.objectDrop, baseCounter.transform.position);
    }

    private void Player_OnPickedSomething(object sender, System.EventArgs e)
    {
        if (Player.Instance != null)
            PlaySound(audioClipRefsSO.objectPickup, Player.Instance.transform.position);
    }

    private void CuttingCounter_OnAnyCut(object sender, System.EventArgs e)
    {
        CuttingCounter cuttingCounter = sender as CuttingCounter;
        if (cuttingCounter != null)
            PlaySound(audioClipRefsSO.chop, cuttingCounter.transform.position);
    }

    private void DeliveryManager_OnRecipeFailed(object sender, System.EventArgs e)
    {
        if (DeliveryCounter.Instance != null)
        {
            PlaySound(audioClipRefsSO.deliveryFail, DeliveryCounter.Instance.transform.position);
        }
        else
        {
            PlaySound(audioClipRefsSO.deliveryFail, Vector3.zero);
            Debug.LogWarning("[SoundManager] DeliveryCounter.Instance 为 null");
        }
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, System.EventArgs e)
    {
        if (DeliveryCounter.Instance != null)
        {
            PlaySound(audioClipRefsSO.deliverySucess, DeliveryCounter.Instance.transform.position);
        }
        else
        {
            PlaySound(audioClipRefsSO.deliverySucess, Vector3.zero);
            Debug.LogWarning("[SoundManager] DeliveryCounter.Instance 为 null");
        }
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f)
    {
        if (audioClipArray != null && audioClipArray.Length > 0)
        {
            PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
        }
    }

    private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (audioClip != null)
        {
            string clipName = audioClip.name;
            if (
                !lastPlayedTime.ContainsKey(clipName)
                || Time.time - lastPlayedTime[clipName] > minInterval
            )
            {
                AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
                lastPlayedTime[clipName] = Time.time;
            }
        }
    }

    public void PlayFootstepSound(Vector3 position, float volume)
    {
        PlaySound(audioClipRefsSO.footStep, position, volume);
    }

    public void PlayCountDownSound()
    {
        PlaySound(audioClipRefsSO.warnings, Vector3.zero);
    }

    public void PlayWarningSound(Vector3 position)
    {
        PlaySound(audioClipRefsSO.warnings, position);
    }

    public void ChangeVolume()
    {
        volume += .1f;
        if (volume > 1f)
        {
            volume = 0f;
        }
        PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        return volume;
    }
}
