using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jelly : MonoBehaviour
{

    public GameManager manager;
    public ParticleSystem effect;
    //���� �ܰ�
    public int level;
    //�巡�� ���¸� üũ�ϴ� ����
    public bool isDrag;
    // ��ĥ �� �ٸ� ������ �������� �ʵ��� ��ݿ��� ���ִ� ����
    public bool isMerge;
    public bool isAttach;

    //����ȿ�� ��Ʈ��
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
        // ���� �Ӽ� �ʱ�ȭ
        level = 0;
        isDrag = false;
        isMerge = false;
        isAttach = false;

        // ���� Ʈ������ �ʱ�ȭ
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.zero;

        // ���� ���� �ʱ�ȭ
        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circle.enabled = true;
	}
	// Update is called once per frame
	void Update()
    {
        // 1. �巡�� ������ ��
        if(isDrag)
		{
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // X�� ��� ����
            //   1) �ִ� ����
            float leftBorder = -4.2f + transform.localScale.x / 2f;
            float rightBorder = 4.2f - transform.localScale.x / 2f;

            //   2) ���� ���� ��谪���� x�� �̵� ����
            if (mousePos.x < leftBorder)
            {
                mousePos.x = leftBorder;
            }
            else if (mousePos.x > rightBorder)
            {
                mousePos.x = rightBorder;
            }

            mousePos.y = 8; //x�ุ �����̰� �ϱ� ���ؼ�
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.35f);
            //Vector3.Lerp : ��ǥ�������� �ε巴�� �̵� 
            //(A,B,C) A:���� ��ġ, B:��ǥ ��ġ, C: ���󰡴� ����[0���� 1���հ�]
            Debug.Log("�ȴ�");
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

            //��� ��ũ��Ʈ�� ������ ���� �� ���� ���� ����ǵ��� *****(��������)*****
            if(level == other.level && !isMerge && !other.isMerge && level < 10)
			{
                // ���� ��ġ�� ����

                // ���� ����� ��ġ ��������(�񱳸� ���� ���� x,y �� ��������)
                float meX = transform.position.x;
                float meY= transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 1. ���� �Ʒ��� ���� ��
                // 2. ������ ������ ��, ���� �����ʿ� ���� ��
                if(meY < otherY || (meY == otherY && meX > otherX))
				{
                    // ����� �����
                    other.Hide(transform.position);
                    // ���� ������
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

    // �̵��� ���� �ڷ�ƾ
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
