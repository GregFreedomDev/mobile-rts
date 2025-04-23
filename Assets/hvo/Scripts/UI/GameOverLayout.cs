using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameOverLayout : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button m_ActionBtn;
    [SerializeField] private Button m_BackBtn;
    [SerializeField] private Button m_RetryBtn;
    [SerializeField] private TextMeshProUGUI m_TitleText;
    [SerializeField] private TextMeshProUGUI m_RewardText;
    [SerializeField] private TextMeshProUGUI m_RetryText;
    [SerializeField] private Image m_BackDropImage;
    [SerializeField] private GameObject[] m_Stars;
    [SerializeField] private RectTransform m_PanelTransform;
    [SerializeField] private GameObject m_UIBlocker;

    [Header("Gameplay")]
    public UnityAction OnRetryClicked = delegate { };
    public UnityAction OnContinueClicked = delegate { };
    public UnityAction OnBackClicked = delegate { };

    private const int RESTART_COST = 100;
    private const int MAX_RETRIES = 5;
    private int playerGold;
    private int retriesLeft;
    private bool isVictoryContext = false;
    private Coroutine showRoutine;

    void OnEnable()
    {
        m_ActionBtn.onClick.AddListener(() =>
        {
            OnMainButtonClicked();
            Debug.Log("Action button clicked");
        });
        m_BackBtn.onClick.AddListener(() => {
            AudioManager.Get().PlayBtnClick();
            Debug.Log("OnBack");

            OnBackClicked.Invoke();
        });
    }

    void OnDisable()
    {
        m_ActionBtn.onClick.RemoveAllListeners();
        m_BackBtn.onClick.RemoveAllListeners();
        m_RetryBtn.onClick.RemoveAllListeners();
    }

    public void ShowVictory(int currentGold, int starCount, List<Reward> rewards, int retriesLeft)
    {
        isVictoryContext = true;
        playerGold = currentGold;
        this.retriesLeft = retriesLeft;

        ActivateUI();
        m_TitleText.text = "Victory!";
        SetStars(starCount);
        ShowRewards(rewards);

        m_ActionBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Continue";
        m_ActionBtn.interactable = true;

        ShowRetryOptionIfApplicable();
    }

    public void ShowDefeat(int currentGold)
    {
        isVictoryContext = false;
        playerGold = currentGold;
        this.retriesLeft = 0;

        ActivateUI();
        m_TitleText.text = "Defeat!";
        SetStars(0);

        m_RewardText.text = $"Retry for {RESTART_COST} Gold";
        m_ActionBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Retry";
        m_ActionBtn.interactable = currentGold >= RESTART_COST;

        m_RetryBtn.gameObject.SetActive(false);
    }

    private void ActivateUI()
    {
        if (m_UIBlocker != null)
            m_UIBlocker.SetActive(true);

        gameObject.SetActive(true);
        m_BackDropImage.color = new Color(0, 0, 0, 0.3f);
    }

    private void OnMainButtonClicked()
    {
        AudioManager.Get().PlayBtnClick();

        if (isVictoryContext)
        {
            OnContinueClicked.Invoke();
        }
        else if (playerGold >= RESTART_COST)
        {
            OnRetryClicked.Invoke();
        }
    }

    private void SetStars(int count)
    {
        for (int i = 0; i < m_Stars.Length; i++)
            m_Stars[i].SetActive(i < count);
    }

    private void ShowRewards(List<Reward> rewards)
    {
        if (rewards == null || rewards.Count == 0)
        {
            m_RewardText.text = "No Rewards";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var reward in rewards)
            sb.AppendLine($"{reward.Name} +{reward.Amount}");

        m_RewardText.text = sb.ToString();
    }

    private void ShowRetryOptionIfApplicable()
    {
        if (retriesLeft > 0)
        {
            m_RetryBtn.gameObject.SetActive(true);
            m_RetryText.text = $"Retry ({retriesLeft}/{MAX_RETRIES})";
            m_RetryBtn.interactable = true;

            m_RetryBtn.onClick.RemoveAllListeners();
            m_RetryBtn.onClick.AddListener(() => {
                AudioManager.Get().PlayBtnClick();
                OnRetryClicked.Invoke();
            });
        }
        else
        {
            m_RetryBtn.gameObject.SetActive(false);
        }
    }
}


public class Reward
{
    public string Name;
    public int Amount;

    public Reward(string name, int amount)
    {
        Name = name;
        Amount = amount;
    }
}
