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
        // 确保从黑屏开始淡入
        fadeCanvasGroup.alpha = 1;
        StartCoroutine(FadeIn());

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void Update()
    {
        if (!hasEnded && Input.anyKeyDown)
        {
            Debug.Log("用户按键跳过视频");
            hasEnded = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!hasEnded)
        {
            Debug.Log("视频播放完毕，准备切换");
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
