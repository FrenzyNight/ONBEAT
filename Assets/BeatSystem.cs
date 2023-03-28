using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class BeatSystem : SingleTone<BeatSystem>
{
    public float beatsPerMinute = 120f;
    public Image beatIndicator;
    public Color onBeatColor;
    public Color offBeatColor;
    
    public float beatScaleFactor = 1.2f; // 이미지의 크기를 조절할 스케일 인자
    public float beatScaleDuration = 0.1f; // 이미지가 커지고 작아지는 데 걸리는 시간
    public float colorTransitionDuration = 0.1f; // 색상 전환에 걸리는 시간

    public BeatBarSpawner beatBarSpawner;

    public AudioSource audioSource;
    public AudioClip beatSound;

    private float beatInterval;
    private float timeSinceLastBeat;
    private bool isOnBeat;
    
    public float BeatInterval => beatInterval;
    void Start()
    {
        beatInterval = 60f / beatsPerMinute;
        timeSinceLastBeat = 0f;
        isOnBeat = false;
        beatIndicator.color = offBeatColor;
        beatBarSpawner.SetSpawnInterval(beatInterval);

    }

    void Update()
    {
        timeSinceLastBeat += Time.deltaTime;

        if (timeSinceLastBeat >= beatInterval)
        {
            timeSinceLastBeat -= beatInterval;
            isOnBeat = true;
            StartCoroutine(ResetOnBeat());
            
            beatBarSpawner.SpawnBeatBar();
            
            // 사운드 재생
            PlayBeatSound();
            
            // 이미지 크기 변경
            AnimateBeatIndicator();
        }
    }
    
    private void PlayBeatSound()
    {
        audioSource.PlayOneShot(beatSound);
    }
    private void AnimateBeatIndicator()
    {
        beatIndicator.transform.DOKill(); // 이미 실행중인 DOTween 애니메이션을 중지
        beatIndicator.transform.DOScale(beatScaleFactor, beatScaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => beatIndicator.transform.DOScale(1f, beatScaleDuration)
                .SetEase(Ease.OutBack));
        
        // 이미지 색상 변경
        beatIndicator.DOColor(onBeatColor, colorTransitionDuration)
            .OnComplete(() => beatIndicator.DOColor(offBeatColor, colorTransitionDuration));
    }
    
    
    IEnumerator ResetOnBeat()
    {
        yield return new WaitForSeconds(beatInterval / 2f);
        isOnBeat = false;
        beatIndicator.color = offBeatColor;
    }
    
    public bool IsOnBeat()
    {
        return isOnBeat;
    }
}
