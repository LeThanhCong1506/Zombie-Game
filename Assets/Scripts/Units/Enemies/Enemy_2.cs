using System.Collections;
using UnityEngine;

public class Enemy_2 : CellObject
{
    [Header("---------Enemy Settings---------")]
    public float MoveSpeed;
    public int Damage;
    public int Health;
    public int DefaultDamage;
    public int DefaultHealth;

    private AudioManager m_AudioManager;
    private Animator m_Animator;
    private bool m_IsMoving;
    private bool m_IsMovingConsecutively;
    private Vector3 m_MoveTarget;
    private int m_CurrentHealth;
    private int m_CurrentDamage;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_AudioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick += TurnHappened;
        }
    }

    public override void Init(Vector2Int coord)
    {
        base.Init(coord);
        m_IsMoving = false;
        m_IsMovingConsecutively = false;
        m_CurrentHealth = Health;
        m_CurrentDamage = Damage;
    }

    public void Spawn(bool animator)
    {
        m_Animator.SetBool("Moving", animator);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.TurnManager != null)
        {
            GameManager.Instance.TurnManager.OnTick -= TurnHappened;
        }
    }

    public override bool PlayerWantsToEnter()
    {
        m_CurrentHealth -= 1;

        if (m_CurrentHealth == 0)
        {
            m_AudioManager.PlaySFX(m_AudioManager.EnemyDeath);
            GameManager.Instance.PlayerController.Animator.SetBool("Panic", false);
            GameManager.Instance.EnemiesKilledStats++;
            Debug.Log("Enemies Killed: " + GameManager.Instance.EnemiesKilledStats);
            Destroy(gameObject);
        }

        return false;
    }

    void TurnHappened()
    {
        if (GameManager.Instance.IsGamePaused())
        {
            return;
        }

        //We added a public property that return the player current cell!
        var playerCell = GameManager.Instance.PlayerController.Cell();

        int xDist = playerCell.x - m_Cell.x;
        int yDist = playerCell.y - m_Cell.y;

        int absXDist = Mathf.Abs(xDist);
        int absYDist = Mathf.Abs(yDist);

        if ((xDist == 0 && absYDist == 1)
            || (yDist == 0 && absXDist == 1))
        {
            //we are adjacent to the player, attack!
            GameManager.Instance.UpdateHealth(-m_CurrentDamage);
            StartCoroutine(AttackAnimatorAfterSeconds(1.0f));
            // Start attack animation and stop after 1 second
            StartCoroutine(GameManager.Instance.PlayerController.PanicAnimatorAfterSeconds(1.5f));
        }
        else
        {
            if (absXDist > absYDist)
            {
                if (!TryMoveInX(xDist))
                {
                    //if our move was not successful (so no move and not attack)
                    //we try to move along Y
                    TryMoveInY(yDist);
                }
            }
            else
            {
                if (!TryMoveInY(yDist))
                {
                    TryMoveInX(xDist);
                }
            }
        }
    }

    IEnumerator AttackAnimatorAfterSeconds(float delay)
    {
        m_Animator.SetBool("Attack", true);
        m_AudioManager.PlaySFX(m_AudioManager.EnemyAttack);
        yield return new WaitForSeconds(delay);
        m_Animator.SetBool("Attack", false);
    }

    bool TryMoveInX(int xDist)
    {
        //try to get closer in x

        //player to our right
        if (xDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.right, false);
        }

        //player to our left
        return MoveTo(m_Cell + Vector2Int.left, false);
    }

    bool TryMoveInY(int yDist)
    {
        //try to get closer in y

        //player on top
        if (yDist > 0)
        {
            return MoveTo(m_Cell + Vector2Int.up, false);
        }

        //player below
        return MoveTo(m_Cell + Vector2Int.down, false);
    }

    bool MoveTo(Vector2Int coord, bool immediate)
    {
        var board = GameManager.Instance.BoardManager;
        var targetCell = board.GetCellData(coord);

        if (targetCell == null
            || !targetCell.Passable
            || targetCell.ContainedObject != null)
        {
            return false;
        }

        if (immediate == false)
        {
            if (m_IsMoving)
                m_IsMovingConsecutively = true;

            m_IsMoving = true;

            //remove enemy from current cell
            var currentCell = board.GetCellData(m_Cell);
            currentCell.ContainedObject = null;

            //add it to the next cell
            targetCell.ContainedObject = this;
            m_Cell = coord;
            m_MoveTarget = board.CellToWorld(coord);
            StartCoroutine(MovingCoroutine(m_MoveTarget));
        }

        m_Animator.SetBool("Moving", m_IsMoving);
        return true;
    }
    IEnumerator MovingCoroutine(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;

        if (!m_IsMovingConsecutively)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            m_IsMoving = false;
            m_Animator.SetBool("Moving", false);
        }
        else
            m_IsMovingConsecutively = false;
    }
}