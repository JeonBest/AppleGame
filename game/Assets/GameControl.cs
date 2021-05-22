using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameControl: MonoBehaviour
{
    [Header("Editor Setting")]
    public GameObject apple;
    public RectTransform selectBox;
    public RectTransform collideBox;
    public TextMeshProUGUI scoreText;
    public GameObject gameOver;
    public TextMeshProUGUI gameOverText;
    public GameObject timeSlider;

    [Header("Game Settings")]
    public int horizontalLength;
    public int verticalLength;

    [Header("Only for Read")]
    public int nothing;
    public int score = 0;

    private Vector2 startpos = Vector2.zero;
    private Rect selectRect = new Rect();

    private List<AppleMeta> applesList;
    private List<AppleMeta> selectList;

    private TimeManager TM;

    private void Awake()
    {
        Vector3 zeroPos = apple.transform.position;
        applesList = new List<AppleMeta>();
        selectList = new List<AppleMeta>();

        for (int i = 0; i< verticalLength; i++)
        {
            for (int j = 0; j< horizontalLength; j++)
            {
                Vector3 newPos = new Vector3 (zeroPos.x + j, zeroPos.y + i, zeroPos.z);
                GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);
                AppleMeta _am = newApple.GetComponent<AppleMeta>();
                newApple.name = "Apple (" + System.Convert.ToString(i) + " ," + System.Convert.ToString(j) + ")";
                _am.number = Random.Range(1, 10);
                applesList.Add(_am);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (AppleMeta _apple in applesList)
        {
            _apple.gameObject.SetActive(true);
            _apple.isOn = true;
        }

        TM = timeSlider.GetComponent<TimeManager>();
        gameOver.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (TM.isEnd)
        {
            gameOver.SetActive(true);
            clearGame();
        }

        else
        {
            DragSystem();

            SelectSystem();

            calculateAnswer();

            scoreText.text = "Score: " + System.Convert.ToString(score);
        }
    }

    private void clearGame()
    {
        foreach (AppleMeta everyApple in applesList)
        {
            everyApple.isOn = false;
        }
        gameOverText.text = "Game Over\n\nYour Score: " + System.Convert.ToString(score);
    }

    private void calculateAnswer()
    {
        if (!Input.GetMouseButtonUp(0)) return;

        int sum = 0;
        int cnt = 0;
        foreach(AppleMeta selectedApple in selectList)
        {
            sum += selectedApple.number;
            cnt += 1;
        }

        if (sum == 10)
        {
            animateCorrect();
            score += cnt;
        }
        selectList.Clear();
    }

    private void animateCorrect()
    {
        foreach(AppleMeta selectedApple in selectList)
        {
            // ����� ������ ����
            selectedApple.isOn = false;
        }
    }

    private void SelectSystem()
    {
        // ������ ������ ��, select flag�� �����ִ� ����� selectList�� �߰�
        if (Input.GetMouseButtonUp(0))
        {
            foreach (AppleMeta everyApple in applesList)
            {
                if (everyApple.isSelected) selectList.Add(everyApple);
            }
        }

        // ������ ��ģ�� �ƴϸ�, select flag �ʱ�ȭ
        else
        {
            foreach(AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }
        }

        // ���ùڽ��� �����ִ� ����, �ڽ� ���� ����� select flag�� ���ش�.
        if (selectBox.gameObject.activeSelf)
        {
            // ��� ����� ������ �ʱ�ȭ
            foreach (AppleMeta everyApple in applesList)
            {
                everyApple.isSelected = false;
            }

            // �ݶ��̴��� ������ ����
            Collider2D[] hit = Physics2D.OverlapBoxAll(collideBox.anchoredPosition, collideBox.sizeDelta, 0f);
            foreach (Collider2D i in hit)
            {
                if (!i.CompareTag("apple"))
                    continue;

                AppleMeta hitApple = i.transform.parent.gameObject.GetComponent<AppleMeta>();

                // collide�� ����� �̹� ���� ������ ���� �����,
                if (!hitApple.isSelected)
                {
                    hitApple.isSelected = true;
                }
            }
        }
    }

    private void DragSystem()
    {
        #region ó�� Ŭ���� ��ǥ ����
        if (Input.GetMouseButtonDown(0))
        {
            selectBox.gameObject.SetActive(true);
            startpos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0))
        {
            selectBox.gameObject.SetActive(false);
        }
        #endregion

        #region �巡�� ���� ��
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x > startpos.x)
            {
                // ó�� Ŭ���� ������ ���������� �巡���� ���
                selectRect.xMin = startpos.x;
                selectRect.xMax = Input.mousePosition.x;
            }

            else
            {
                // ó�� Ŭ���� ������ �������� �巡���� ���
                selectRect.xMin = Input.mousePosition.x;
                selectRect.xMax = startpos.x;
            }

            if (Input.mousePosition.y > startpos.y)
            {
                // ó�� Ŭ���� ������ �������� �巡���� ���
                selectRect.yMin = startpos.y;
                selectRect.yMax = Input.mousePosition.y;
            }

            else
            {
                // ó�� Ŭ���� ������ �Ʒ������� �巡���� ���
                selectRect.yMin = Input.mousePosition.y;
                selectRect.yMax = startpos.y;
            }
        }
        #endregion

        #region Safe Area
        if (selectRect.xMin < 0) selectRect.xMin = 0;
        if (selectRect.yMin < 0) selectRect.yMin = 0;
        if (selectRect.xMax > Screen.width) selectRect.xMax = Screen.width;
        if (selectRect.yMax > Screen.height) selectRect.yMax = Screen.height;
        #endregion

        // ������ �ݿ�
        selectBox.offsetMin = selectRect.min;
        selectBox.offsetMax = selectRect.max;

        collideBox.offsetMin = Camera.main.ScreenToWorldPoint(selectRect.min);
        collideBox.offsetMax = Camera.main.ScreenToWorldPoint(selectRect.max);
    }

    #region ��ư �׼�
    public void OnResetButton()
    {
        SceneManager.LoadScene("Main");
    }

    public void OnHomeButton()
    {
        SceneManager.LoadScene("Title");
    }
    #endregion
}
