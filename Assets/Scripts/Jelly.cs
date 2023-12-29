using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jelly : MonoBehaviour
{

    public GameManager manager;
    public ParticleSystem effect;
    //젤리 단계
    public int level;
    //드래그 상태를 체크하는 변수
    public bool isDrag;
    // 합칠 때 다른 젤리이 개입하지 않도록 잠금역할 해주는 변수
    public bool isMerge;
    public bool isAttach;

    //물리효과 컨트롤
    public Rigidbody2D rigid;
    CircleCollider2D circle;
    Animator anim;
    SpriteRenderer spriteRenderer;

    float deadTime;


	private void Awake()
	{
        rigid = GetComponent<Rigidbody2D>();
        circle = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

	void OnEnable()
	{
        anim.SetInteger("Level", level);
	}

	private void OnDisable()
	{
        // 젤리 속성 초기화
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        // 젤리 트랜스폼 초기화
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        // 젤리 물리 초기화
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
	}
	// Update is called once per frame
	void Update()
    {
        // 1. 드래그 상태일 때
        if(isDrag)
		{
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X축 경계 설정
            //   1) 최대 범위
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            //   2) 좌측 우측 경계값으로 x축 이동 제한
            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            mousePos.y = 8; //x축만 움직이게 하기 위해서
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.35f);
            //Vector3.Lerp : 목표지점으로 부드럽게 이동 
            //(A,B,C) A:현재 위치, B:목표 위치, C: 따라가는 강도[0부터 1사잇값]
            Debug.Log("안뇽");
        }

    }

	public void Drag()
	{
        isDrag = true;
	}

    public void Drop()
    {
        rigid.simulated = true;
        isDrag = false;     
    }
	void OnCollisionEnter2D(Collision2D collision)
	{
        StartCoroutine("AttachRoutine");
	}

    IEnumerator AttachRoutine()
	{
        if(isAttach)
		{
            yield break;
		}
        isAttach = true;
        manager.SfxPlay(GameManager.SFX.Attach);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;

    }

	void OnCollisionStay2D(Collision2D collision)
	{
		if(collision.gameObject.tag == "Jelly")
		{
            Jelly other = collision.gameObject.GetComponent<Jelly>();

            //상대 스크립트의 레벨과 같을 때 다음 로직 실행되도록 *****(레벨수정)*****
            if(level == other.level && !isMerge && !other.isMerge && level < 10)
			{
                // 동글 합치기 로직

                // 나와 상대편 위치 가져오기(비교를 위해 각자 x,y 값 가져오기)
                float meX = transform.position.x;
                float meY= transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 1. 내가 아래에 있을 때
                // 2. 동일한 높이일 때, 내가 오른쪽에 있을 때
                if(meY < otherY || (meY == otherY && meX > otherX))
				{
                    // 상대은 숨기기
                    other.Hide(transform.position);
                    // 나는 레벨업
                    LevelUp();
                }

            }
		}
	}

    public void Hide(Vector3 targetPos)
	{
        isMerge = true;

        rigid.simulated = false;
        circle.enabled = false;

        if(targetPos == Vector3.up * 100)
		{
            EffectPlay();
		}

        StartCoroutine(HideRoutine(targetPos));
	}

    // 이동을 위한 코루틴
    IEnumerator HideRoutine(Vector3 targetPos)
	{
        int frameCount = 0;

        while(frameCount < 20)
		{
            frameCount++;
            if(targetPos != Vector3.up*100)
			{
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if(targetPos == Vector3.up*100)
			{
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.06f);
            }

            yield return null;
        }

        manager.score += (int)Mathf.Pow(2, level);

        isMerge = false;
        gameObject.SetActive(false);
	}
    void LevelUp()
	{
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine("LevelUpRoutine");
	}        

    IEnumerator LevelUpRoutine()
	{
        yield return new WaitForSeconds(0.2f);

        anim.SetInteger("Level", level + 1);
        EffectPlay();
        manager.SfxPlay(GameManager.SFX.LevelUp);

        yield return new WaitForSeconds(0.2f);
        level++;

        if(manager.maxLevel < 7)
		{
            manager.maxLevel = Mathf.Max(level, manager.maxLevel);
        }
        

        isMerge = false;
    }

	void OnTriggerStay2D(Collider2D collision)
	{
		if(collision.tag == "Finish")
		{
            deadTime += Time.deltaTime;

            if(deadTime > 2)
			{
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
			}
            if(deadTime > 5)
			{
                manager.GameOver();
			}
		}
	}

	void OnTriggerExit2D(Collider2D collision)
	{
		if(collision.tag == "Finish")
		{
            deadTime = 0;
            spriteRenderer.color = Color.white;
		}
	}
	void EffectPlay()
	{
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
	}
}
