using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-------------[Core]")]
    public bool isOver;
    public int score;
    public int maxLevel;

    [Header("-------------[Object Pooling]")]
    public GameObject jellyPrefab;
    public Transform jellyGroup;
    public List<Jelly> jellyPool;
 
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1,30)]
    public int poolSize;
    public int poolCursor;
    public Jelly lastJelly; //��â�� ������ ����

    [Header("-------------[Audio]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public AudioClip[] sfxClip;
    public enum SFX { LevelUp, Next, Attach, Button, GameOver };
    int sfxCursor;

    [Header("-------------[UI]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI maxScoreText;
    public TextMeshProUGUI subScoreText;

    [Header("-------------[ETC]")]
    public GameObject line;
    public GameObject bottom;




    private void Awake()
	{
        Application.targetFrameRate = 60;

        jellyPool = new List<Jelly>();
        effectPool = new List<ParticleSystem>();
        for(int index = 0; index < poolSize; index++)
        {
            MakeJelly();
        }

        if(!PlayerPrefs.HasKey("MaxScore"))
		{
            PlayerPrefs.SetInt("MaxScore",0);
		}
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }
	public void GameStart()
    {
        // ������Ʈ Ȱ��ȭ
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // ���� �÷���
        bgmPlayer.Play();
        SfxPlay(SFX.Button);

        // ���� ����(���� ����)
        Invoke("NextJelly", 1.5f);

    }

    Jelly MakeJelly()
	{
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // ���� ����
        GameObject instantJellyObj = Instantiate(jellyPrefab, jellyGroup);
        instantJellyObj.name = "Jelly" + jellyPool.Count;
        Jelly instantJelly = instantJellyObj.GetComponent<Jelly>();
        instantJelly.manager = this;
        instantJelly.effect = instantEffect;
        jellyPool.Add(instantJelly);

        return instantJelly;
    }
    // ��������
    Jelly GetJelly()
	{
        for(int index = 0; index < jellyPool.Count; index++)
		{
            poolCursor = (poolCursor+1) % jellyPool.Count;
            if(!jellyPool[poolCursor].gameObject.activeSelf)
			{
                return jellyPool[poolCursor];
			}
		}            

        return MakeJelly();
    }

    // ��������
    void NextJelly()
	{
        if(isOver)
		{
            return;
		}

        lastJelly = GetJelly();
        //Level���� ���� �� ���� Ȱ��ȭ *****(��������)*****
        lastJelly.level = Random.Range(0, maxLevel);
        lastJelly.gameObject.SetActive(true);

        SfxPlay(GameManager.SFX.Next);
        StartCoroutine("WaitNext");
    }

    IEnumerator WaitNext()
	{
        while(lastJelly != null)
		{
            yield return null;

        }
        yield return new WaitForSeconds(1.5f);

        NextJelly();
    }
    public void TouchDown()
	{
        if( lastJelly == null )
           return;

        lastJelly.Drag();
	}

    public void TouchUp()
    {
        if (lastJelly == null)
            return;

        lastJelly.Drop();
        lastJelly = null;
    }

    public void GameOver()
	{
        if(isOver)
		{
            return;
		}

        isOver = true;

        StartCoroutine("GameOverRoutine");
	}
    IEnumerator GameOverRoutine()
    {
        //1. ��� �ȿ� Ȱ��ȭ �Ǿ��ִ� ��� ���� ��������
        Jelly[] jellys = FindObjectsOfType<Jelly>();

        //2. ����� ���� ��� ������ ����ȿ�� ��Ȱ��ȭ
        for (int index = 0; index < jellys.Length; index++)
        {
            jellys[index].rigid.simulated = false;
        }

        //3.1���� ����� �ϳ��� �����ؼ� �����
        for (int index = 0; index < jellys.Length; index++)
        {
            jellys[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // �ְ� ���� ����
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // ���ӿ��� UI ǥ��
        subScoreText.text = "SCORE : " + scoreText.text;
        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(SFX.GameOver);
    }

	public void Reset()
	{
        SfxPlay(SFX.Button);
        StartCoroutine("ResetCoroutine");
	}

    IEnumerator ResetCoroutine()
	{
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Main");
	}
	public void SfxPlay(SFX type)
	{
        switch(type)
		{
            case SFX.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0,3)];
                break;

            case SFX.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;

            case SFX.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;

            case SFX.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;

            case SFX.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
	}

	private void Update()
	{
		if(Input.GetButtonDown("Cancel"))
		{
            Application.Quit();
		}
	}
	private void LateUpdate()
	{
        scoreText.text = score.ToString();
	}
}
