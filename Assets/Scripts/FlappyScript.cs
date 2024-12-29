using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FlappyScript : MonoBehaviour
{
    public AudioClip FlyAudioClip, DeathAudioClip, ScoredAudioClip;
    public Sprite GetReadySprite;
    public Scene Ending_Destroy;
    public float RotateUpSpeed = 1, RotateDownSpeed = 1;
    public GameObject IntroGUI, DeathGUI;
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3;
    public float XSpeed = 1;

    [Header("Victory Button")]
    public CustomButton controlButton; // 按钮引用
    public float holdTimeToWin = 4f; // 长按时间
    private float holdTimer = 0f; // 记录长按时间
    private bool isHoldingButton = false; // 是否正在长按按钮

    [Header("UI Components")]
    public Slider holdProgressSlider; // 按住倒计时的进度条
    public Text holdProgressText; // 按住倒计时的文本

    void Start()
    {
        GameStateManager.GameState = GameState.Intro;
        // 为按钮绑定事件
        if (controlButton != null)
        {
            controlButton.onClick.AddListener(OnControlButtonClick);
        }
        else
        {
            Debug.LogError("Control button not assigned in the Inspector.");
        }

        // 设置进度条最大值
        if (holdProgressSlider != null)
        {
            holdProgressSlider.maxValue = holdTimeToWin;
            holdProgressSlider.value = 0;
        }

        ResetHoldProgressUI();
    }
    FlappyYAxisTravelState flappyYAxisTravelState;

    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    Vector3 birdRotation = Vector3.zero;

    void Update()
    {
        if (GameStateManager.GameState == GameState.Intro)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
                GameStateManager.GameState = GameState.Playing;
                IntroGUI.SetActive(false);
                ScoreManagerScript.Score = 0;
            }
        }
        else if (GameStateManager.GameState == GameState.Playing)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
            }
            if (isHoldingButton)
            {
                HandleHoldProgress();
            }
        }
        else if (GameStateManager.GameState == GameState.Dead)
        {
            Vector2 contactPoint = Vector2.zero;

            if (Input.touchCount > 0)
                contactPoint = Input.touches[0].position;
            if (Input.GetMouseButtonDown(0))
                contactPoint = Input.mousePosition;

            //check if user wants to restart the game
            if (restartButtonGameCollider == Physics2D.OverlapPoint
                (Camera.main.ScreenToWorldPoint(contactPoint)))
            {
                GameStateManager.GameState = GameState.Intro;
                Application.LoadLevel(Application.loadedLevelName);
            }
        }
    }
    bool WasTouchedOrClicked()
    {
        if (Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) || 
            (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended))
            return true;
        else
            return false;
    }
    // 按下按钮时触发
    public void OnControlButtonPress()
    {
        isHoldingButton = true;
        //BoostOnYAxis();
        holdTimer = 0f; // 重置长按计时
    }

    // 松开按钮时触发
    public void OnControlButtonRelease()
    {
        isHoldingButton = false;
    }

    // 单击按钮触发
    public void OnControlButtonClick()
    {
        //BoostOnYAxis(); // 点击按钮触发上升
    }

    void HandleHoldProgress()
    {
        holdTimer += Time.deltaTime;

        if (holdProgressSlider != null)
        {
            holdProgressSlider.value = holdTimeToWin-holdTimer;
        }

        if (holdProgressText != null)
        {
            holdProgressText.text = "长按2秒跳过广告";
        }

        if (holdTimer >= holdTimeToWin)
        {
            GameWin();
        }
    }

    void BoostOnYAxis()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, VelocityPerJump);
        GetComponent<AudioSource>().PlayOneShot(FlyAudioClip);
    }

    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void GameWin()
    {
        Debug.Log("You win!");
        isHoldingButton = false;
        ResetHoldProgressUI();
        string Ending_Destroy = "Ending_Destroy";
        SceneManager.LoadScene(Ending_Destroy);
        // 这里可以跳转到胜利场景，或者触发胜利逻辑
    }

    private void ResetHoldProgressUI()
    {
        if (holdProgressSlider != null)
        {
            holdProgressSlider.value = 2;
        }

        if (holdProgressText != null)
        {
            holdProgressText.text = "长按2秒跳过广告";
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Pipeblank")
            {
                GetComponent<AudioSource>().PlayOneShot(ScoredAudioClip);
                ScoreManagerScript.Score++;
            }
            else if (col.gameObject.tag == "Pipe")
            {
                FlappyDies();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Floor")
            {
                FlappyDies();
            }
        }
    }

    void FlappyDies()
    {
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        GetComponent<AudioSource>().PlayOneShot(DeathAudioClip);
    }

    void FixedUpdate()
    {
        if (GameStateManager.GameState == GameState.Intro)
        {
            if (GetComponent<Rigidbody2D>().velocity.y < -1)
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime));
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
        }
    }

    private void FixFlappyRotation()
    {
        if (GetComponent<Rigidbody2D>().velocity.y > 0) flappyYAxisTravelState = FlappyYAxisTravelState.GoingUp;
        else flappyYAxisTravelState = FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = 0;

        switch (flappyYAxisTravelState)
        {
            case FlappyYAxisTravelState.GoingUp:
                degreesToAdd = 6 * RotateUpSpeed;
                break;
            case FlappyYAxisTravelState.GoingDown:
                degreesToAdd = -3 * RotateDownSpeed;
                break;
        }

        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }
}
