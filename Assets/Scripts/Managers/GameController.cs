using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    Board m_gameBoard;
    Spawner m_spawner;
    ScoreManager m_scoreManager;
    Ghost m_ghost;

    // currently active shape
    Shape m_activeShape;

    public float m_dropInterval = 0.3f;

    float m_timeToDrop;

    float m_timeToNextKeyLeftRight;

    [Range(0.02f, 1f)]
    public float m_keyRepeatRateLeftRight = 0.15f;

    float m_timeToNextKeyDown;

    [Range(0.01f, 1f)]
    public float m_keyRepeatRateDown = 0.05f;

    float m_timeToNextKeyRotate;
    float m_dropIntervalModded;

    [Range(0.02f, 1f)]
    public float m_keyRepeatRateRotate = 0.25f;

    bool m_gameOver = false;

    public GameObject m_gameOverPanel;

    SoundManager m_soundManager;

    public IconToggle m_rotIconToggle;
    bool m_clockwise = true;

    public bool m_isPaused = false;
    public GameObject m_pausePanel;

    public Holder m_holder;
    public ParticlePlayer m_gameOverFx;

    float m_timeToNextSwipe;
    float m_timeToNextDrag;

    [Range(0.05f, 1f)]
    public float m_minTimeToDrag = 0.15f;

    [Range(0.05f, 1f)]
    public float m_minTimeToSwipe = 0.3f;

    bool m_didTap = false;

    //public Text diagnosticText; 

    void OnEnable()
    {
        TouchController.DragEvent += DragHandler;
        TouchController.SwipeEvent += SwipeHandler;
        TouchController.TapEvent += TapHandler;
    }

    void OnDisable()
    {
        TouchController.DragEvent -= DragHandler;
        TouchController.SwipeEvent -= SwipeHandler;
        TouchController.TapEvent -= TapHandler;
    }

    // Start is called before the first frame update
    void Start()
    { 
        m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;
        m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;
        m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;

        m_gameBoard = GameObject.FindObjectOfType<Board>();
        m_spawner = GameObject.FindObjectOfType<Spawner>();
        m_soundManager = GameObject.FindObjectOfType<SoundManager>();
        m_scoreManager = GameObject.FindObjectOfType<ScoreManager>();
        m_ghost = GameObject.FindObjectOfType<Ghost>();
        m_holder = GameObject.FindObjectOfType<Holder>();

        if (m_spawner)
        {
            if (m_activeShape == null)
            {
                m_activeShape = m_spawner.SpawnShape();
            }

            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);
        }

        if (!m_gameBoard)
        {
            Debug.LogWarning("WARNING! There is no game board defined.");
        }

        if (!m_spawner)
        {
            Debug.LogWarning("WARNING! There is no spawner defined.");
        }
        else
        {
            m_spawner.transform.position = Vectorf.Round(m_spawner.transform.position);
            if (!m_activeShape)
            {
                m_activeShape = m_spawner.SpawnShape();
            }
        }

        if (!m_soundManager)
        {
            Debug.LogWarning("WARNING! There is no sound manager defined.");
        }
        if (!m_scoreManager)
        {
            Debug.LogWarning("WARNING! There is no score manager defined.");
        }

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(false);
        }
        if (m_pausePanel)
        {
            m_pausePanel.SetActive(false);
        }

        m_dropIntervalModded = m_dropInterval;
        //if (diagnosticText)
        //{
        //    diagnosticText.text = "";
        //}

    }

    void MoveLeft()
    {
        m_activeShape.MoveLeft();
        m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;
        if (!m_gameBoard.IsValidPosition(m_activeShape))
        {
            m_activeShape.MoveRight();
            PlaySound(m_soundManager.m_errorSound, 0.5f);
        }
        else
        {
            PlaySound(m_soundManager.m_moveSound, 0.5f);
        }
    }

    void MoveRight()
    {
        m_activeShape.MoveRight();
        m_timeToNextKeyLeftRight = Time.time + m_keyRepeatRateLeftRight;

        if (!m_gameBoard.IsValidPosition(m_activeShape))
        {
            m_activeShape.MoveLeft();
            PlaySound(m_soundManager.m_errorSound, 0.5f);
        }
        else
        {
            PlaySound(m_soundManager.m_moveSound, 0.5f);
        }
    }

    void Rotate()
    {
        m_activeShape.RotateClockwise(m_clockwise);
        m_timeToNextKeyRotate = Time.time + m_keyRepeatRateRotate;
        if (!m_gameBoard.IsValidPosition(m_activeShape))
        {
            m_activeShape.RotateClockwise(!m_clockwise);
            PlaySound(m_soundManager.m_errorSound, 0.5f);
        }
        else
        {
            PlaySound(m_soundManager.m_moveSound, 0.5f);
        }
    }

    void MoveDown()
    {
        m_timeToDrop = Time.time + m_dropIntervalModded;
        m_timeToNextKeyDown = Time.time + m_keyRepeatRateDown;
        m_activeShape.MoveDown();

        if (!m_gameBoard.IsValidPosition(m_activeShape))
        {
            if (m_gameBoard.IsOverLimit(m_activeShape))
            {
                GameOver();
            }
            else
            {
                LandShape();
            }
        }
    }

    enum Direction { None, Left, Right, Up, Down }

    Direction m_dragDirection = Direction.None;
    Direction m_swipeDirection = Direction.None; 

    void PlayerInput()
    {
        if ((Input.GetButton("MoveRight") && Time.time > m_timeToNextKeyLeftRight) || Input.GetButtonDown("MoveRight"))
        {
            MoveRight();
        }
        else if ((Input.GetButton("MoveLeft") && Time.time > m_timeToNextKeyLeftRight) || Input.GetButtonDown("MoveLeft"))
        {
            MoveLeft();
        }
        else if (Input.GetButtonDown("Rotate") && Time.time > m_timeToNextKeyRotate)
        {
            Rotate();
        }
        else if  ((Input.GetButton("MoveDown") && (Time.time > m_timeToNextKeyDown)) || (Time.time > m_timeToDrop))
        {
            MoveDown();
        }
        //Touch movement controls -----------
        else if ((m_dragDirection == Direction.Right && Time.time > m_timeToNextSwipe) ||
            (m_dragDirection == Direction.Right && Time.time > m_timeToNextDrag)) 
        {
            MoveRight();
            m_timeToNextDrag = Time.time + m_minTimeToDrag;
            m_timeToNextSwipe = Time.time + m_minTimeToSwipe;
        }
        else if ((m_dragDirection == Direction.Left && Time.time > m_timeToNextSwipe) ||
            (m_swipeDirection == Direction.Left && Time.time > m_timeToNextDrag))
        {
            MoveLeft();
            m_timeToNextDrag = Time.time + m_minTimeToDrag;
            m_timeToNextSwipe = Time.time + m_minTimeToSwipe; 
        }
        else if((m_swipeDirection == Direction.Up && Time.time > m_timeToNextSwipe) || m_didTap)
        {
            Rotate();
            m_timeToNextSwipe = Time.time + m_minTimeToSwipe;
            m_didTap = false;
        }
        else if(m_dragDirection == Direction.Down && Time.time > m_timeToNextDrag)
        {
            MoveDown();
        }
        //Touch movement controls -----------
        else if (Input.GetButtonDown("ToggleRot"))
        {
            ToggleRotDirection();
        }
        else if (Input.GetButtonDown("TogglePause"))
        {
            TogglePause();
        }
        else if (Input.GetButtonDown("Hold"))
        {
            Hold();
        }

        m_dragDirection = Direction.None;
        m_swipeDirection = Direction.None;
        m_didTap = false;
    }

    private void PlaySound(AudioClip audioClip, float volMultiplier)
    {
        if (audioClip && m_soundManager.enabled && m_soundManager.m_fxEnabled)
        {
            AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position, Mathf.Clamp(m_soundManager.m_fxVolume * volMultiplier, 0.05f, 1f));
        }
    }

    private void GameOver()
    {
        m_activeShape.MoveUp();
        m_gameOver = true;


        StartCoroutine("GameOverRoutine");
    }

    IEnumerator GameOverRoutine()
    {
        if (m_gameOverFx)
        {
            m_gameOverFx.Play();
        }
        yield return new WaitForSeconds(0.3f);

        if (m_gameOverPanel)
        {
            m_gameOverPanel.SetActive(true);
            PlaySound(m_soundManager.m_gameOverSound, 5f);
            PlaySound(m_soundManager.m_gameOverVocalClip, 1f);
        }
    }

    private void LandShape()
    {
        if (m_activeShape)
        {
            m_timeToNextKeyDown = Time.time;
            m_timeToNextKeyRotate = Time.time;
            m_timeToNextKeyLeftRight = Time.time;

            m_activeShape.MoveUp();
            m_activeShape.LandShapeFx();
            m_gameBoard.StoreShapeInGrid(m_activeShape);

            m_activeShape = m_spawner.SpawnShape();

            m_gameBoard.StartCoroutine("ClearAllRows");

            if (m_ghost) m_ghost.Reset();
            if (m_holder) m_holder.m_canRelease = true;

            if (m_gameBoard.m_completedRows > 0)
            {
                m_scoreManager.ScoreLines(m_gameBoard.m_completedRows);
                if (m_scoreManager.m_didLevelUp)
                {
                    PlaySound(m_soundManager.m_levelUpVocalClip, 1f);
                    m_dropIntervalModded = Mathf.Clamp(m_dropInterval - (((float)m_scoreManager.m_level - 1) * 0.05f), 0.05f, 1f);
                }
                else
                {
                    if (m_gameBoard.m_completedRows > 1)
                    {
                        AudioClip randomVocal = m_soundManager.GetRandomClip(m_soundManager.m_vocalClips);
                        PlaySound(randomVocal, 1f);

                    }
                }
                PlaySound(m_soundManager.m_clearRowRound, 0.5f);
            }

            PlaySound(m_soundManager.m_dropSound, 0.75f);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_gameBoard || !m_spawner || !m_activeShape || m_gameOver || !m_soundManager || !m_scoreManager)
        {
            return;
        }

        PlayerInput();
    }

    public void Restart()
    {
        Debug.Log("Restarted");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ToggleRotDirection()
    {
        m_clockwise = !m_clockwise;
        if (m_rotIconToggle)
        {
            m_rotIconToggle.ToggleIcon(m_clockwise);
        }
    }

    public void TogglePause()
    {
        if (m_gameOver)
        {
            return;
        }
        m_isPaused = !m_isPaused;
        if (m_pausePanel)
        {
            m_pausePanel.SetActive(m_isPaused);
            if (m_soundManager)
            {
                m_soundManager.m_musicSource.volume = m_isPaused ? m_soundManager.m_musicVolume * 0.25f : m_soundManager.m_musicVolume;
            }
            Time.timeScale = m_isPaused ? 0 : 1;
        }
        
    }

    private void LateUpdate()
    {
        if (m_ghost)
        {
            m_ghost.DrawGhost(m_activeShape, m_gameBoard);
        }
    }

    public void Hold()
    {
        if (!m_holder)
        {
            return;
        }
        if (!m_holder.m_heldShape) {
            m_holder.Catch(m_activeShape);
            m_activeShape = m_spawner.SpawnShape();
            PlaySound(m_soundManager.m_holdSound, 1f);
        }
        else if(m_holder.m_canRelease)
        {
            Shape temp = m_activeShape;
            m_activeShape = m_holder.Release();
            m_activeShape.transform.position = m_spawner.transform.position;
            m_holder.Catch(temp);
            PlaySound(m_soundManager.m_holdSound, 1f);
        }
        else
        {
            Debug.LogWarning("HOLDER Warning! Please wait for cool down!");
            PlaySound(m_soundManager.m_errorSound, 1f);
        }

        
        if (m_ghost)
        {
            m_ghost.Reset();
        }
    }

    void DragHandler (Vector2 dragMovement)
    {
        m_dragDirection = GetDirection(dragMovement);
    }

    void SwipeHandler(Vector2 swipeMovement)
    {
        m_swipeDirection = GetDirection(swipeMovement);
    }

    void TapHandler(Vector2 tapMovement)
    {
        m_didTap = true;
    }

    Direction GetDirection(Vector2 swipeMovement)
    {
        Direction swipeDir = Direction.None;
        if(Mathf.Abs(swipeMovement.x) > Mathf.Abs(swipeMovement.y))
        {
            swipeDir = (swipeMovement.x >= 0) ? Direction.Right : Direction.Left;
        }
        else
        {
            swipeDir = (swipeMovement.y >= 0) ? Direction.Up : Direction.Down;
        }

        return swipeDir;
    }
}
