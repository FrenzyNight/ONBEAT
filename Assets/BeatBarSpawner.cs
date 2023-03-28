using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

public class BeatBarSpawner : MonoBehaviour
{
    public GameObject beatBarPrefab;

    private float spawnInterval;
   // public float speed = 5f;
   // public Vector2 direction = Vector2.left;
    public float lifetime;
    
    private ObjectPool<BeatBar> beatBarPool;
    private float timeSinceLastSpawn;
    private float spawnPointX;
    private float spawnPointY;
    private float endPositionX;
    private RectTransform rectTransform;
    
    private Vector2 spawnPosition;
    private Vector2 endPosition;

    private float tempTime;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 오브젝트 풀 생성 및 초기화
        beatBarPool = new ObjectPool<BeatBar>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true);
        spawnInterval = BeatSystem.Instance.BeatInterval;
        SetSpawnPoint();
        //lifetime = poolSize / spawnInterval;
    }
    
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = interval;
    }
    
    private void SetSpawnPoint()
    {
        spawnPointX = (rectTransform.anchoredPosition.x - (rectTransform.rect.width * 0.5f) - (beatBarPrefab.GetComponent<RectTransform>().rect.width * 0.5f));
        spawnPointY = 0;
        
        endPositionX = (rectTransform.anchoredPosition.x + (rectTransform.rect.width * 0.5f)+ (beatBarPrefab.GetComponent<RectTransform>().rect.width * 0.5f));

        spawnPosition = new Vector2(spawnPointX, spawnPointY);
        endPosition = new Vector2(endPositionX, spawnPointY);
        
        tempTime = (beatBarPrefab.GetComponent<RectTransform>().rect.width * lifetime) / rectTransform.rect.width;
    }

    private BeatBar CreatePooledItem()
    {
        GameObject beatBarInstance = Instantiate(beatBarPrefab);
        BeatBar beatBar = beatBarInstance.GetComponent<BeatBar>();
        beatBarInstance.SetActive(false);
        return beatBar;
    }

    private void OnTakeFromPool(BeatBar beatBar)
    {
        beatBar.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(BeatBar beatBar)
    {
        beatBar.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(BeatBar beatBar)
    {
        Destroy(beatBar.gameObject);
    }


    public void SpawnBeatBar()
    {
        BeatBar beatBar = beatBarPool.Get();
        RectTransform beatBarRectTransform = beatBar.GetComponent<RectTransform>();
        beatBar.Init(lifetime, tempTime);
        beatBar.transform.SetParent(transform, false);
        beatBar.canvasGroup.alpha = 0f;
        beatBar.canvasGroup.DOFade(1f, 0.2f);

        beatBarRectTransform.anchoredPosition = spawnPosition;
        
        beatBarRectTransform.DOAnchorPosX(endPosition.x, lifetime).SetEase(Ease.Linear).OnComplete(() =>
        {
            rectTransform.DOKill();
            beatBar.canvasGroup.DOFade(0f, 0.15f).OnComplete(() =>
            {
                beatBarPool.Release(beatBar);
            });
        });
    }

}