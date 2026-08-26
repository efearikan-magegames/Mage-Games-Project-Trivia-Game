using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ana menüde kullanılan butonların kullanımlarını yöneten sınıf.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LeaderBoardDisplay leaderBoardDisplay;
    [SerializeField] private SceneLoader gameplaySceneLoader;

    [Header("Menu Buttons")]
    [SerializeField] private Button loadLeaderBoardButton;
    [SerializeField] private Button startGameButton;

    void Start()
    {
        // Ana menüye dönüldüğünde yükleme ekranı veya hata ekranı kapatılır.
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.HideLoadingScreen();
            LoadingScreenManager.Instance.HideErrorScreen();
        }

        loadLeaderBoardButton.onClick.AddListener(LoadLeaderBoard);
        startGameButton.onClick.AddListener(LoadGameplayScene);
    }

    void OnDestroy()
    {
        // Obje yok edildiğinde buton dinleyicileri de temizlenir
        loadLeaderBoardButton.onClick.RemoveListener(LoadLeaderBoard);
        startGameButton.onClick.RemoveListener(LoadGameplayScene);
    }

    /// <summary>
    /// Liderlik Tablosu panelinin açılmasını sağlayan metot.
    /// </summary>
    /// <remarks>
    /// Metot referansı <see cref="Start"/> içerisinde ilgili butona atanır
    /// </remarks>
    private void LoadLeaderBoard()
    {
        leaderBoardDisplay.ShowLeaderBoard();
    }

    /// <summary>
    /// Oyun sahnesinin yüklenmeye başlamasını sağlayan metot.
    /// </summary>
    /// <remarks>
    /// Metot referansı <see cref="Start"/> içerisinde ilgili butona atanır
    /// </remarks>
    private void LoadGameplayScene()
    {
        gameplaySceneLoader.LoadTargetScene();
    }
}
