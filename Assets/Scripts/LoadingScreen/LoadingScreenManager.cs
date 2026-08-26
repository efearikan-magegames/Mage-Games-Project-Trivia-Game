using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yükleme ekranını yöneten singleton yapıdaki sınıf.
/// Yükleme ekranının sahneler arasında kaybolmamasını sağlamak için
/// <see cref="UnityEngine.Object.DontDestroyOnLoad"/> metodu kullanıldı.
/// </summary>
/// <remarks>
/// Bu sınıfın bağlı olduğu canvasın Sort Order değeri yüksek olarak ayarlanmalı.
/// Yoksa diğer UI elementleri yükleme ekranı önünde kalabilir.
/// </remarks>
public class LoadingScreenManager : MonoBehaviour
{
    /// <summary>
    /// Diğer sınıfların bu sınıfa erişerek yükleme ekranının durumuna
    /// müdahale edebilmesi için kullanılan Instance alanı.
    /// </summary>
    public static LoadingScreenManager Instance { get; private set; }

    [Header("Screen Canvas Groups")]
    [SerializeField] private CanvasGroup loadingScreen_CanvasGroup;
    [SerializeField] private CanvasGroup errorScreen_CanvasGroup;

    [Header("Loading Screen Elements")]
    [SerializeField] private Slider loading_Slider;

    [Header("Error Screen Elements")]
    [SerializeField] private TextMeshProUGUI error_Text;
    [SerializeField] private Button loadingFailure_Button;

    [Header("Load Progress")]
    [Tooltip("Yükleme ekranının yükleme ilerlemesinin sahne yüklenmesi için ayrılmış yüzdelik kısım. Değişiklik yapılırsa QuestionManager.questionLoadProgressWeight verisinin değeri de güncellenmelidir.")]
    [SerializeField] private float sceneLoadProgressWeight = 0.5f;
    public float SceneLoadProgressWeight => sceneLoadProgressWeight;

    [Tooltip("Yükleme ekranının yükleme ilerlemesinin soru verilerinin indirilmesi için ayrılmış yüzdelik kısım. Değişiklik yapılırsa SceneLoader.sceneLoadProgressWeight verisinin değeri de güncellenmelidir.")]
    [SerializeField] private float questionLoadProgressWeight = 0.5f;
    public float QuestionLoadProgressWeight => questionLoadProgressWeight;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Yükleme ekranının gösterilmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// aktifleştirir.
    /// </remarks>
    public void ShowLoadingScreen()
    {
        loadingScreen_CanvasGroup.alpha = 1f;
        loadingScreen_CanvasGroup.interactable = true;
        loadingScreen_CanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Yükleme ekranının gizlenmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// deaktifleştirir.
    /// </remarks>
    public void HideLoadingScreen()
    {
        loadingScreen_CanvasGroup.alpha = 0f;
        loadingScreen_CanvasGroup.interactable = false;
        loadingScreen_CanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Yükleme ekranının yüklenme barının
    /// dolum miktarının ayarlanması için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Sadece <c>0</c> - <c>1</c> arasındaki
    /// değerlerle işlem yapılabilir.
    /// </remarks>
    public void SetLoadingProgress(float progress)
    {
        loading_Slider.value = progress;
    }

    /// <summary>
    /// Hata ekranının gösterilmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// aktifleştirir.
    /// </remarks>
    /// <param name="errorMessage">
    /// Gelen hata mesajını ileten veri.
    /// </param>
    /// <param name="onDismiss">
    /// Butona basılınca çağırılacak
    /// callback Action.
    /// </param>
    public void ShowErrorScreen(string errorMessage, Action onDismiss)
    {
        // Eğer açık bir yükleme ekranı varsa önce onu kapat.
        HideLoadingScreen();

        error_Text.text = errorMessage;

        // Metot iki defa çağırılırsa eski callbacklerin birikmemesi için:
        loadingFailure_Button.onClick.RemoveAllListeners();
        loadingFailure_Button.onClick.AddListener(() => onDismiss?.Invoke());

        errorScreen_CanvasGroup.alpha = 1f;
        errorScreen_CanvasGroup.interactable = true;
        errorScreen_CanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Hata ekranının gizlenmesi için
    /// kullanılan public API metot.
    /// </summary>
    /// <remarks>
    /// Ekranın görünürlüğünü,
    /// ekran ile etkileşimini ve
    /// raycastlerin ekrandan geçişini
    /// deaktifleştirir.
    /// </remarks>
    public void HideErrorScreen()
    {
        loadingFailure_Button.onClick.RemoveAllListeners();

        errorScreen_CanvasGroup.alpha = 0f;
        errorScreen_CanvasGroup.interactable = false;
        errorScreen_CanvasGroup.blocksRaycasts = false;
    }
}
