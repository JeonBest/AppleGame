using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameControl: MonoBehaviour
{
    [Header("Editor Setting")]
    public GameObject canvas;
    public GameObject apple;
    public RectTransform selectBox;
    public RectTransform collideBox;
    public TextMeshProUGUI scoreText;
    public GameObject gameOver;
    public TextMeshProUGUI gameOverText;
    public GameObject timeSlider;
    public TextMeshProUGUI addScoreText;

    [Header("Game Settings")]
    public int horizontalLength;
    public int verticalLength;
    public float comboTime;

    [Header("Only for Read")]
    public int nothing;
    public int score = 0;
    public float comboDelta = 0f;
    public int comboCnt;

    private Vector2 startpos = Vector2.zero;
    private Rect selectRect = new Rect();

    private List<AppleMeta> applesList;
    private List<AppleMeta> selectList;

    private TimeManager TM;

    private int[,] mapApple;
    private List<Vector2> hints;

    private void Awake()
    {
        Vector3 zeroPos = apple.transform.position;
        applesList = new List<AppleMeta>();
        selectList = new List<AppleMeta>();
        mapApple = new int[verticalLength, horizontalLength];
        hints = new List<Vector2>();

        for (int i = 0; i< verticalLength; i++)
        {
            for (int j = 0; j< horizontalLength; j++)
            {
                Vector3 newPos = new Vector3 (zeroPos.x + j, zeroPos.y + i, zeroPos.z);
                GameObject newApple = Instantiate(apple, newPos, Quaternion.identity);
                AppleMeta _am = newApple.GetComponent<AppleMeta>();
                newApple.name = string.Format("Apple ({0}, {1})", i, j);
                int _number = Random.Range(1, 10);
                _am.number = _number;
                mapApple[i, j] = _number;
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

        /*
        #region ��Ʈ
        Vector2 vector = new Vector2();
        for (int x = 0; x < verticalLength; x++)
        {
            for (int y = 0; y < horizontalLength; y++)
            {
                int sum = mapApple[x, y];
                // ���η� Ž��
                for (int k = y; k < horizontalLength; k++)
                {
                    sum += mapApple[x, k];
                    if (sum > 10) break;
                    if (sum == 10) 
                    {
                        vector.Set(x, k);
                        hints.Add(vector);
                    }
                }
                // ���η� Ž��
                for (int k = x; k < verticalLength; k++)
                {
                    sum += mapApple[k, y];
                    if (sum > 10) break;
                    if (sum == 10)
                    {
                        vector.Set(k, y);
                        hints.Add(vector);
                    }
                }
            }
        }
        #endregion
        */
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

            scoreText.text = string.Format("Score: {0}", score);
        }
    }

    private void clearGame()
    {
        foreach (AppleMeta everyApple in applesList)
        {
            everyApple.isOn = false;
        }
        gameOverText.text = string.Format("Game Over\n\nYour Score: {0}", score);
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
            /* �߰���� #3 */
            // ���� �ð� �ȿ� ���������� �����ϸ� �޺�
            if (Time.time - comboDelta < comboTime && comboDelta != 0)
            {
                comboCnt += 1;
                comboDelta = Time.time;
            }
            else
            {
                comboCnt = 0;
                comboDelta = Time.time;
            }

            /* �߰���� #4 */
            // 3�� �̻��� ������� ��� �Ͷ߸� ��� ���� +10
            if (cnt >= 3)
            {
                animateCorrect(cnt, comboCnt, true);
                score += cnt + 10 + comboCnt*5;
            }
            else
            {
                animateCorrect(cnt, comboCnt, false);
                score += cnt + comboCnt*5;
            }
        }
        selectList.Clear();
    }

    private void animateCorrect(int _score, int combo, bool isMany)
    {
        foreach(AppleMeta selectedApple in selectList)
        {
            // ����� ������ ����
            selectedApple.isAnimated = true;
        }

        Vector2 midPos = new Vector2(selectBox.position.x + selectBox.rect.center.x, selectBox.position.y + selectBox.rect.center.y);
        TextMeshProUGUI addScore = Instantiate(addScoreText, midPos, Quaternion.identity);
        addScore.transform.SetParent(canvas.transform);
        addScore.gameObject.SetActive(true);
        AddScore AS = addScore.GetComponent<AddScore>();
        AS.print(_score, combo, isMany);
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

                AppleMeta hitApple = i.gameObject.GetComponent<AppleMeta>();

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
