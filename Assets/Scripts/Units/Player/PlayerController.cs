using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static BoardManager;

public class PlayerController : MonoBehaviour
{
    [Header("---------Player Settings---------")]
    public float MoveSpeed;
    public AudioManager AudioManager;
    public Animator Animator;

    private bool m_IsMoving;
    private bool m_IsGameOver;
    private float m_LastActionTime;
    private float m_ActionCoolDown = 0.25f;
    public bool CanRestart;
    public bool IsStand;
    public bool IsPlayerStoped;
    private Vector3 m_MoveTarget;
    private BoardManager m_BoardManager;
    private Vector2Int m_CurrentCell;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Init()
    {
        m_IsMoving = false;
        m_IsGameOver = false;
    }
    public void Spawn(BoardManager boardManager, Vector2Int cell)
    {
        m_BoardManager = boardManager;
        MoveTo(cell, true);
        //MoveTo để cập nhật vị trí của nhân vật trong không gian thế giới khi nhân vật di chuyển đến một ô mới. Điều này đảm bảo rằng nhân vật luôn ở vị trí chính xác trên bản đồ game.
    }

    private void Update()
    {
        if (GameOverChecked())
            return;
        if (PlayerMovingChecked())
            return;
        PlayerHasMovedChecked();
    }

    private bool GameOverChecked()
    {
        if (m_IsGameOver)
        {
            Animator.SetBool("Moving", false);
            Animator.SetBool("Panic", true);

            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                CanRestart = true;
                GameManager.Instance.StartNewGame();
            }

            return true;
        }
        return false;
    }

    private bool PlayerMovingChecked()
    {
        if (m_IsMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_MoveTarget, MoveSpeed * Time.deltaTime);


            if (transform.position == m_MoveTarget)
            {
                m_IsMoving = false;
                Animator.SetBool("Moving", false);
                var cellData = m_BoardManager.GetCellData(m_CurrentCell);
                if (cellData.ContainedObject != null)
                {
                    cellData.ContainedObject.PlayerEntered();
                }
            }

            return true;
        }
        return false;
    }

    private void PlayerHasMovedChecked()
    {
        Vector2Int newCellTarget = m_CurrentCell;
        bool hasMoved = false;
        bool hasStayed = false;
        HandleMovedInput(ref hasMoved, ref hasStayed, ref newCellTarget);
        HasMovedChecked(hasMoved, hasStayed, newCellTarget);
    }

    private void HandleMovedInput(ref bool hasMoved, ref bool hasStayed, ref Vector2Int newCellTarget)
    {
        if (Time.time <= m_LastActionTime + m_ActionCoolDown)
        {
            return;
        }

        if (IsPlayerStoped)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                hasStayed = true;
                IsPlayerStoped = false;
                m_LastActionTime = Time.time;
            }

            if (Keyboard.current.upArrowKey.wasPressedThisFrame
                || Keyboard.current.downArrowKey.wasPressedThisFrame
                || Keyboard.current.rightArrowKey.wasPressedThisFrame
                || Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                hasMoved = true;
                IsPlayerStoped = false;
                m_LastActionTime = Time.time;
            }
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            hasStayed = true;
            m_LastActionTime = Time.time;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y += 1;
            hasMoved = true;
            m_LastActionTime = Time.time;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            newCellTarget.y -= 1;
            hasMoved = true;
            m_LastActionTime = Time.time;
        }
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x += 1;
            hasMoved = true;
            m_LastActionTime = Time.time;
        }
        else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            newCellTarget.x -= 1;
            hasMoved = true;
            m_LastActionTime = Time.time;
        }
    }

    private void HasMovedChecked(bool hasMoved, bool hasStayed, Vector2Int newCellTarget)
    {
        if (hasStayed)
        {
            IsStand = true;
            GameManager.Instance.TurnManager.Tick();
        }
        else if (hasMoved)
        {
            //check if the new position is passable, then move there if it is.
            BoardManager.CellData cellData = m_BoardManager.GetCellData(newCellTarget);

            if (cellData != null && cellData.Passable)
            {
                GameManager.Instance.TurnManager.Tick();

                if (cellData.ContainedObject == null)
                {
                    MoveTo(newCellTarget, false);
                }
                else if (cellData.ContainedObject.PlayerWantsToEnter())
                {
                    MoveTo(newCellTarget, false);
                    EnteredConditionHandled(cellData);
                }
                else if (!cellData.ContainedObject.PlayerWantsToEnter())
                {
                    NotEnteredConditionHandled(cellData);
                }
            }
        }
    }

    private void EnteredConditionHandled(CellData cellData)
    {
        if (cellData.ContainedObject.gameObject.name.ToLower().Contains("smallfood"))
        {
            AudioManager.PlaySFX(AudioManager.Fruit);
            GameManager.Instance.UpdateStamina(5);
        }
        else if (cellData.ContainedObject.gameObject.name.ToLower().Contains("smalldrink"))
        {
            AudioManager.PlaySFX(AudioManager.Soda);
            GameManager.Instance.UpdateStamina(10);
        }
    }

    private void NotEnteredConditionHandled(CellData cellData)
    {
        Animator.SetTrigger("Attack");
        AudioManager.PlaySFX(AudioManager.Chop);
    }

    public void GameOver()
    {
        m_IsGameOver = true;
    }

    public IEnumerator PanicAnimatorAfterSeconds(float delay)
    {
        Animator.SetBool("Panic", true);
        yield return new WaitForSecondsRealtime(delay);
        Animator.SetBool("Panic", false);
    }

    public void MoveTo(Vector2Int cell, bool immediate)
    {
        AudioManager.PlaySFX(AudioManager.Footstep);

        m_CurrentCell = cell;

        if (immediate)
        {
            m_IsMoving = false;
            transform.position = m_BoardManager.CellToWorld(m_CurrentCell);
        }
        else
        {
            m_IsMoving = true;
            m_MoveTarget = m_BoardManager.CellToWorld(m_CurrentCell);
        }

        Animator.SetBool("Moving", m_IsMoving);
    }

    //create a method that will get player's current cell
    public Vector2Int Cell()
    {
        return m_CurrentCell;
    }

    public void Save(ref PlayerSaveData data)
    {
        data.Position = transform.position;
        data.IsPlayerStoped = IsPlayerStoped;
        data.m_IsGameOver = m_IsGameOver;
    }

    public void Load(PlayerSaveData data)
    {
        transform.position = data.Position;
        this.Spawn(GameManager.Instance.BoardManager, new Vector2Int((int)transform.position.x, (int)transform.position.y));
        IsPlayerStoped = data.IsPlayerStoped;
        m_IsGameOver = data.m_IsGameOver;
    }
}

[System.Serializable]
public struct PlayerSaveData
{
    public Vector3 Position;
    public bool IsPlayerStoped;
    public bool m_IsGameOver;
}
