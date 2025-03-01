using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Header("----------UI Elements----------")]
    public UIDocument UIDoc;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private Label m_HealthLabel;
    private Label m_Stats;
    private Label m_StaminaNumber;
    private Label m_SpeedLabel;

    [Header("---------Game Settings---------")]
    public int CurrentLevel;
    public int EnemiesKilledStats;
    public int PlayerHealth;
    public int PlayerStamina;
    public int PlayerSpeed;

    [Header("-----------Game State-----------")]
    private bool m_IsGamePaused = false;
    private bool m_CanMoveTwiceFlag;
    public int StopPlayerCount;
    public int StopEnemyCount;

    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public Enemy Enemy;
    public Enemy_2 Enemy_2;

    public static GameManager Instance { get; private set; }
    public TurnManager TurnManager { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        InitializeGameSetup();
        InitializeUI();
        InitializeBoard();
        LoadGameChecked();
        StartCoroutine(CheckAndChangePlayerStatus());
    }

    private void InitializeGameSetup()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
    }

    void OnTurnHappen()
    {
        if (PlayerController.IsStand)
        {
            PlayerController.IsStand = false;
            UpdateStamina(0);
        }
        else
        {
            UpdateStamina(PlayerHealth <= 20 ? -5 : -1);
        }
    }

    public void UpdateHealth(int amount)
    {
        PlayerHealth += amount;
        PlayerHealth = Mathf.Clamp(PlayerHealth, 0, 100);

        m_HealthLabel.text = "Health : " + PlayerHealth;
    }

    public void UpdateStamina(int stamina)
    {
        PlayerStamina += stamina;
        PlayerStamina = Mathf.Clamp(PlayerStamina, 0, 60);

        UpdatePlayerSpeedBasedOnStamina(PlayerStamina);
        HandleMovementIfStaminaMax(PlayerStamina);
        HandleMovementIfStaminaMin(PlayerStamina);

        m_StaminaNumber.text = "Stamina : " + PlayerStamina;
        m_SpeedLabel.text = "Speed : " + PlayerSpeed;
    }

    private void UpdatePlayerSpeedBasedOnStamina(int stamina)
    {
        PlayerSpeed = stamina > 0 ? 1 : 0;
    }

    private void HandleMovementIfStaminaMax(int stamina)
    {
        if (stamina == 60)
        {
            PauseGame();
            m_CanMoveTwiceFlag = true;
            StopEnemyCount = 0;//reset the count
        }

        if (m_CanMoveTwiceFlag)
        {
            StopEnemyCount++;
            PlayerSpeed = 2;
            Debug.Log("StopEnemyCount: " + StopEnemyCount);
            if (StopEnemyCount >= 4)
            {
                ResumeGame();
                ResetMovementFlags();
            }
        }
    }

    private void ResetMovementFlags()
    {
        StopEnemyCount = 0;
        m_CanMoveTwiceFlag = false;
        PlayerSpeed = 1;
    }

    private void HandleMovementIfStaminaMin(int stamina)
    {
        if (stamina == 0)
        {
            StopPlayerCount++;
            PlayerSpeed = 0;
        }

        if (StopPlayerCount == 2)
        {
            PlayerController.IsPlayerStoped = true;
            StopPlayerCount = 0;
        }
    }

    private void InitializeUI()
    {
        m_GameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
        m_Stats = UIDoc.rootVisualElement.Q<Label>("Stats");
        m_HealthLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_StaminaNumber = UIDoc.rootVisualElement.Q<Label>("StaminaLabel");
        m_SpeedLabel = UIDoc.rootVisualElement.Q<Label>("SpeedLabel");


        m_GameOverMessage.text = "Level: " + CurrentLevel;
        m_Stats.text = "↑↓→← to move\n" + "Space to wait\n";
        m_HealthLabel.text = "Health : " + PlayerHealth;
        m_StaminaNumber.text = "Stamina : " + PlayerStamina;
        m_SpeedLabel.text = "Speed : " + PlayerSpeed;
    }

    public void NewLevelChecked()
    {
        BoardManager.Clean();
        if (CurrentLevel % 5 == 0)
        {
            BoardManager.Width += 1;
            BoardManager.Height += 1;
        }

        CurrentLevel++;
        Debug.Log("gameover masage: " + m_GameOverMessage.name);
        m_GameOverMessage.text = "Level: " + CurrentLevel;

        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));

        if (BoardManager.IsZombie == true)
            UpdateEnemyStats();
    }

    private void UpdateEnemyStats()
    {
        Debug.Log("current level: " + CurrentLevel);
        if (CurrentLevel >= 5 && CurrentLevel <= 8)
            XZombieAppearStatsWithInstruction();
        else
            XZombieAppearStats();

        if (CurrentLevel >= 10)
            JZombieAppearStats();
    }

    private void XZombieAppearStatsWithInstruction()
    {
        m_Stats.text = $"Move back towards the opposing\nzombie to attack!!!" +
            $"\n\n\n\n\n=== X Zombie ===\nHealth: {BoardManager.EnemyPrefab.Health}\nDamage: {BoardManager.EnemyPrefab.Damage}";
    }

    private void XZombieAppearStats()
    {
        m_Stats.text = $"=== X Zombie ===\nHealth: {BoardManager.Enemy_2_Prefab.Health}\nDamage: {BoardManager.Enemy_2_Prefab.Damage}";
    }

    private void JZombieAppearStats()
    {
        m_Stats.text = $"=== X Zombie ===\nHealth: {BoardManager.EnemyPrefab.Health}\nDamage: {BoardManager.EnemyPrefab.Damage}\n\n" +
                       $"=== J Zombie ===\nHealth: {BoardManager.Enemy_2_Prefab.Health}\nDamage: {BoardManager.Enemy_2_Prefab.Damage}";
    }

    private void InitializeBoard()
    {
        BoardManager.Clean();
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
    }

    public void StartNewGame()
    {
        ResetGameSettings();
        ResetBoardSettings();
        ResetPlayerSettings();
    }

    private void ResetGameSettings()
    {
        BoardManager.IsZombie = false;
        BoardManager.IsZombieUpDate = false;
        BoardManager.IsZombieUpDate2 = false;
        PlayerSpeed = 1;
        CurrentLevel = 1;
        PlayerHealth = 40;
        PlayerStamina = 40;
        EnemiesKilledStats = 0;
        m_HealthLabel.text = "Health : " + PlayerHealth;
        m_GameOverMessage.text = "Level: " + CurrentLevel;
        m_Stats.text =
        "↑↓→← to move\n" +
        "Space to wait\n";
        m_StaminaNumber.text = "Stamina : " + PlayerStamina;
        m_SpeedLabel.text = "Speed : " + PlayerSpeed;
        if (PlayerController.CanRestart)
        {
            ResetEnemyStats();
            PlayerController.CanRestart = false;
        }

        StartCoroutine(CheckAndChangePlayerStatus());
    }

    private void ResetEnemyStats()
    {
        BoardManager.EnemyPrefab.Damage = BoardManager.EnemyPrefab.DefaultDamage;
        BoardManager.EnemyPrefab.Health = BoardManager.EnemyPrefab.DefaultHealth;

        BoardManager.Enemy_2_Prefab.Damage = BoardManager.Enemy_2_Prefab.DefaultDamage;
        BoardManager.Enemy_2_Prefab.Health = BoardManager.Enemy_2_Prefab.DefaultHealth;
    }

    private void ResetBoardSettings()
    {
        BoardManager.Clean();
        BoardManager.Width = BoardManager.DefaultWidth;
        BoardManager.Height = BoardManager.DefaultHeight;
        BoardManager.FromNumberOfFood = BoardManager.DefaultFromNumberOfFood;
        BoardManager.ToNumberOfFood = BoardManager.DefaultToNumberOfFood;
        BoardManager.FromNumberOfWall = BoardManager.DefaultFromNumberOfWall;
        BoardManager.ToNumberOfWall = BoardManager.DefaultToNumberOfWall;
        BoardManager.Init();
    }

    private void ResetPlayerSettings()
    {
        PlayerController.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(1, 1));
        PlayerController.Animator.SetBool("Panic", false);
    }

    private void LoadGameChecked()
    {
        if (SaveSystem.IsSaveFileEmpty() == false)
        {
            SaveSystem.Load();
        }
    }

    private IEnumerator CheckAndChangePlayerStatus()
    {
        while (true)
        {
            if (PlayerHealth == 0)
            {
                GameOverHandling();
                yield break;
            }

            if (PlayerStamina > 0)
                IncreaseHealthIfStaminaHigh();
            else
                DecreaseHealthIfStaminaLow();

            yield return new WaitForSeconds(3f);
        }
    }

    public void GameOverHandling()
    {
        PlayerController.AudioManager.PlaySFX(PlayerController.AudioManager.Down);

        PlayerController.GameOver();
        m_GameOverMessage.text = "Game Over!\n\nYou traveled through " + CurrentLevel + " levels\n\nKilled " + EnemiesKilledStats + "\n\nPress \"ENTER\" to play again:333";
        m_HealthLabel.text = "Health : " + 0;
    }

    private void IncreaseHealthIfStaminaHigh()
    {
        if (PlayerStamina > 50 && PlayerHealth < 100)
        {
            PlayerHealth += 10;

            PlayerHealth = Mathf.Clamp(PlayerHealth, 0, 100);
            m_HealthLabel.text = "Health : " + PlayerHealth;
        }
        else if (PlayerStamina > 30 && PlayerHealth < 100)
        {
            PlayerHealth += 1;

            PlayerHealth = Mathf.Clamp(PlayerHealth, 0, 100);
            m_HealthLabel.text = "Health : " + PlayerHealth;
        }
    }

    private void DecreaseHealthIfStaminaLow()
    {
        if (PlayerStamina == 0)
        {
            PlayerHealth -= 5;

            PlayerHealth = Mathf.Clamp(PlayerHealth, 0, 100);
            m_HealthLabel.text = "Health : " + PlayerHealth;
        }
    }

    public void PauseGame()
    {
        m_IsGamePaused = true;
    }

    public void ResumeGame()
    {
        m_IsGamePaused = false;
    }

    public bool IsGamePaused()
    {
        return m_IsGamePaused;
    }

    public int GetStamina()
    {
        return PlayerStamina;
    }

    public void Save(ref GameSaveData data)
    {
        data.CurrentLevel = CurrentLevel;
        data.EnemiesKilledStats = EnemiesKilledStats;
        data.PlayerHealth = PlayerHealth;
        data.PlayerStamina = PlayerStamina;
        data.PlayerSpeed = PlayerSpeed;
        data.m_IsGamePaused = m_IsGamePaused;
        data.m_CanMoveTwiceFlag = m_CanMoveTwiceFlag;
        data.StopPlayerCount = StopPlayerCount;
        data.StopEnemyCount = StopEnemyCount;
        data.m_Stats = m_Stats.text;
    }

    public void Load(GameSaveData data)
    {
        CurrentLevel = data.CurrentLevel;
        EnemiesKilledStats = data.EnemiesKilledStats;
        PlayerHealth = data.PlayerHealth;
        PlayerStamina = data.PlayerStamina;
        PlayerSpeed = data.PlayerSpeed;
        m_IsGamePaused = data.m_IsGamePaused;
        m_CanMoveTwiceFlag = data.m_CanMoveTwiceFlag;
        StopPlayerCount = data.StopPlayerCount;
        StopEnemyCount = data.StopEnemyCount;

        m_HealthLabel.text = "Health : " + PlayerHealth;
        m_StaminaNumber.text = "Stamina : " + PlayerStamina;
        m_SpeedLabel.text = "Speed : " + PlayerSpeed;
        m_GameOverMessage.text = "Level: " + CurrentLevel;
        m_Stats.text = data.m_Stats;
    }
}

[System.Serializable]
public struct GameSaveData
{
    public int CurrentLevel;
    public int EnemiesKilledStats;
    public int PlayerHealth;
    public int PlayerStamina;
    public int PlayerSpeed;
    public bool m_IsGamePaused;
    public bool m_CanMoveTwiceFlag;
    public int StopPlayerCount;
    public int StopEnemyCount;
    public bool m_IsEnemyExist;
    public int DefaultEnemyHealth;
    public int DefaultEnemyDamage;
    public int DefaultEnemyHealth2;
    public int DefaultEnemyDamage2;
    public string m_Stats;
}

