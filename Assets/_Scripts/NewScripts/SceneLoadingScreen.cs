using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoadingScreen : Singleton<SceneLoadingScreen>
{
    [SerializeField] Canvas loadingScreenCanvas;
    [SerializeField] float fadeInSpeed = 5f;
    [SerializeField] float fadeOutSpeed = 2f;
    [SerializeField] Image loadingScreenBackground;
    [SerializeField] Color32 transitionColor = Color.white;

    protected override void Awake()
    {
        base.Awake();
        loadingScreenCanvas.enabled = false;
        Color c = loadingScreenBackground.color;
        c = transitionColor;
        c.a = 0f;
        loadingScreenBackground.color = c;
    }
    public IEnumerator LoadSceneAsyn<T>(T sceneName)
    {
        loadingScreenCanvas.enabled = true;
        yield return StartCoroutine(Fade(0,1,fadeInSpeed));
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName.ToString(),LoadSceneMode.Additive);
        while (!operation.isDone)
        {
            operation.allowSceneActivation = true;
            yield return null;
        }
        yield return StartCoroutine(Fade(1,0,fadeOutSpeed));
        loadingScreenCanvas.enabled = false;
    }
    private IEnumerator Fade(float fromAlpha, float toAlpha, float speed)
    {
        float timer = 0f;
        Color c = loadingScreenBackground.color;
        while (Mathf.Abs(c.a - toAlpha) > 0.01f)
        {
            timer += Time.unscaledDeltaTime * speed;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, timer);
            loadingScreenBackground.color = c;
            yield return null;
        }
        c.a = toAlpha;
        loadingScreenBackground.color = c;
    }
}
