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
    public Jelly lastJelly; //씬창에 보관할 젤리

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
        // 오브젝트 활성화
        line.SetActive(true);
        bottom.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        // 사운드 플레이
        bgmPlayer.Play();
        SfxPlay(SFX.Button);

        // 게임 시작(젤리 생성)
        Invoke("NextJelly", 1.5f);

    }

    Jelly MakeJelly()
	{
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect" + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 젤리 생성
        GameObject instantJellyObj = Instantiate(jellyPrefab, jellyGroup);
        instantJellyObj.name = "Jelly" + jellyPool.Count;
        Jelly instantJelly = instantJellyObj.GetComponent<Jelly>();
        instantJelly.manager = this;
        instantJelly.effect = instantEffect;
        jellyPool.Add(instantJelly);

        return instantJelly;
    }
    // 젤리생성
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

    // 다음젤리
    void NextJelly()
	{
        if(isOver)
		{
            return;
		}

        lastJelly = GetJelly();
        //Level먼저 설정 후 젤리 활성화 *****(레벨수정)*****
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
        //1. 장면 안에 활성화 되어있는 모든 젤리 가져오기
        Jelly[] jellys = FindObjectsOfType<Jelly>();

        //2. 지우기 전에 모든 젤리의 물리효과 비활성화
        for (int index = 0; index < jellys.Length; index++)
        {
            jellys[index].rigid.simulated = false;
        }

        //3.1번의 목록을 하나씩 접근해서 지우기
        for (int index = 0; index < jellys.Length; index++)
        {
            jellys[index].Hide(Vector3.up * 100);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // 게임오버 UI 표시
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
