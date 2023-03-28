using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BeatBar : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public float beatScaleFactor = 1.5f; // 이미지의 크기를 조절할 스케일 인자
    private float beatScaleDuration = 0.01f; // 이미지가 커지고 작아지는 데 걸리는 시간
    
    private float lifetime;
    private float tempTime;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(float lifetime, float tempTime)
    {
        this.lifetime = lifetime;
        this.beatScaleDuration = tempTime;
        StartCoroutine(AnimateBeatIndicator());
    }

    IEnumerator AnimateBeatIndicator()
    {
        yield return new WaitForSeconds(((lifetime- beatScaleDuration) / 2) - beatScaleDuration);
        //transform.DOKill(); // 이미 실행중인 DOTween 애니메이션을 중지
        transform.DOScale(beatScaleFactor, beatScaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(1f, beatScaleDuration)
                .SetEase(Ease.OutBack));
    }
}