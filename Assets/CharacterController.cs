using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float jumpPower = 5f;
    public LayerMask groundLayer;
    private CapsuleCollider2D capsuleCollider;

    public GameObject onBeatEffectParent;
    
    public GameObject onBeatEffectBG;
    public GameObject onBeatEffect;

    private float onBeatBGOriScale;
    public float onBeatBGScaleFactor = 1f; // 이미지의 크기를 조절할 스케일 인자
    public float onBeatBGScaleDuration; // 이미지가 커지고 작아지는 데 걸리는 시간
    
    private ObjectPool<SpriteRenderer> onBeatEffectBGPool;
    private ObjectPool<Image> onBeatEffectPool;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private Vector3 defaultScale;
    
    public int maxJumpCount = 2;
    private int jumpCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        defaultScale = transform.localScale;
        onBeatEffectBGPool = new ObjectPool<SpriteRenderer>(CreatePooledOnBeatBG, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true);
        onBeatEffectPool = new ObjectPool<Image>(CreatePooledOnBeat, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true);

        
        onBeatBGScaleDuration = BeatSystem.Instance.BeatInterval / 2;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // 캐릭터 이동
        transform.Translate(new Vector2(horizontalInput * moveSpeed * Time.deltaTime, 0));

        // 캐릭터 좌우 반전
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(defaultScale.x, defaultScale.y, defaultScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-defaultScale.x, defaultScale.y, defaultScale.z);
        }

        // 캐릭터 점프
        isGrounded = IsCharacterGrounded();

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumpCount )
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); // 이중 점프 시 기존 속도 초기화
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            if (BeatSystem.Instance.IsOnBeat())
            {
                Debug.Log("On beat!");
                MakeOnBeatEffect();
            }
            else
            {
                Debug.Log("Off beat!");
            }
        }
        
        if (isGrounded)
            jumpCount = 0;
        else if (jumpCount == 0)
            jumpCount = 1;
    }

    private void MakeOnBeatEffect()
    {
        MakeOnBeatBG();
        MakeOnBeat();
    }

    private void MakeOnBeatBG()
    {
        SpriteRenderer onBeatBG = onBeatEffectBGPool.Get();
        onBeatBG.transform.SetParent(transform, false);
        onBeatBG.transform.position = transform.position;
        Color oriColor = onBeatBG.color;
        onBeatBG.color = new Color(oriColor.r, oriColor.g, oriColor.b, 0f);


        StartCoroutine(OnBeatBGAniEffect(onBeatBG));

    }

    IEnumerator OnBeatBGAniEffect(SpriteRenderer onBeatBG)
    {
        onBeatBG.DOFade(1f, 0.1f).SetEase(Ease.Linear);
        
        onBeatBGOriScale = onBeatEffectBG.transform.localScale.x;

        onBeatBG.gameObject.transform.DOScale(onBeatBGScaleFactor, onBeatBGScaleDuration)
            .SetEase(Ease.Linear);
        
        yield return new WaitForSeconds(onBeatBGScaleDuration - 0.1f);
        
        onBeatBG.DOFade(0f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            onBeatBG.gameObject.transform.localScale = new Vector3(onBeatBGOriScale, onBeatBGOriScale, onBeatBGOriScale);
            onBeatEffectBGPool.Release(onBeatBG);
        });
    }

    private void MakeOnBeat()
    {
        Image onBeat = onBeatEffectPool.Get();
        onBeat.transform.SetParent(onBeatEffectParent.transform, false);
        RectTransform onBeatRect = onBeat.gameObject.GetComponent<RectTransform>();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z));

        onBeatRect.position = screenPos;

        Color oriColor = onBeat.color;
        onBeat.color = new Color(oriColor.r, oriColor.g, oriColor.b, 0f);
        
        StartCoroutine(OnBeatAniEffect(onBeat));
    }
    
    IEnumerator OnBeatAniEffect(Image onBeat)
    {
        onBeat.DOFade(1f, 0.1f).SetEase(Ease.Linear);
        RectTransform onBeatRect = onBeat.gameObject.GetComponent<RectTransform>();
        onBeatRect.DOAnchorPosY(onBeatRect.anchoredPosition.y + 100f, onBeatBGScaleDuration).SetEase(Ease.Linear);
        
        yield return new WaitForSeconds(onBeatBGScaleDuration - 0.1f);
        
        onBeat.DOFade(0f, 0.1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            onBeatEffectPool.Release(onBeat);
        });
    }
    
    private SpriteRenderer CreatePooledOnBeatBG()
    {
        GameObject onBeatBGInstance = Instantiate(onBeatEffectBG);
        SpriteRenderer onBeatBG = onBeatBGInstance.GetComponent<SpriteRenderer>();
        onBeatBGInstance.SetActive(false);
        return onBeatBG;
    }
    
    private Image CreatePooledOnBeat()
    {
        GameObject onBeatInstance = Instantiate(onBeatEffect);
        Image onBeat = onBeatInstance.GetComponent<Image>();
        onBeatInstance.SetActive(false);
        return onBeat;
    }
    
    private void OnTakeFromPool(SpriteRenderer onBeatBG)
    {
        onBeatBG.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(SpriteRenderer onBeatBG)
    {
        onBeatBG.gameObject.SetActive(false);
    }
    
    private void OnDestroyPoolObject(SpriteRenderer onBeatBG)
    {
        Destroy(onBeatBG.gameObject);
    }
    
    private void OnTakeFromPool(Image onBeat)
    {
        onBeat.gameObject.SetActive(true);
    }

    private void OnReturnedToPool(Image onBeat)
    {
        onBeat.gameObject.SetActive(false);
    }
    
    private void OnDestroyPoolObject(Image onBeat)
    {
        Destroy(onBeat.gameObject);
    }
    
    private bool IsCharacterGrounded()
    {
        Vector2 capsuleColliderSize = new Vector2(capsuleCollider.size.x, capsuleCollider.size.y);
        Vector2 offset = new Vector2(capsuleCollider.offset.x, capsuleCollider.offset.y - (capsuleColliderSize.y / 2 + 0.01f));
        Vector2 origin = (Vector2)transform.position + offset;

        RaycastHit2D hit = Physics2D.BoxCast(origin, capsuleColliderSize, 0f, Vector2.down, 0.01f, groundLayer);
        return hit.collider != null;
    }
}