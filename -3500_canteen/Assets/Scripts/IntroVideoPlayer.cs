using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;

public class IntroVideoPlayer : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private int targetSceneIndex = 1;

    private bool hasEnded = false;

    private void Start()
    {
        // ȷ���Ӻ�����ʼ����
        fadeCanvasGroup.alpha = 1;
        StartCoroutine(FadeIn());

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void Update()
    {
        if (!hasEnded && Input.anyKeyDown)
        {
            Debug.Log("�û�����������Ƶ");
            hasEnded = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!hasEnded)
        {
            Debug.Log("��Ƶ������ϣ�׼���л�");
            hasEnded = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(targetSceneIndex);
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeIn()
    {
        float timer = fadeDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }
    }
}
