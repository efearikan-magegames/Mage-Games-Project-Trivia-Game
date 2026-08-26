using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Durum sınıflarının <see cref="GameplayManager"/> aracılığı ile
/// oyun arayüzüne müdahale edebilmelerini sağlayan sınıf.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField]    private TextMeshProUGUI     question_Text;
    [SerializeField]    private TextMeshProUGUI     category_Text;
    [SerializeField]    private Button[]            choice_Buttons;
                        private TextMeshProUGUI[]   choice_ButtonTexts;
    [SerializeField]    private Button              nextQuestion_Button;
    [SerializeField]    private CanvasGroup         nextQuestionButton_CanvasGroup;
    [SerializeField]    private TextMeshProUGUI     score_Text;
    [SerializeField]    private TextMeshProUGUI     timerCounter_Text;

    [SerializeField]    private CanvasGroup         resultScreen_CanvasGroup;
    [SerializeField]    private TMP_InputField      userNicknameEntry_InputField;
    [SerializeField]    private Button              saveScore_Button;
    [SerializeField]    private TextMeshProUGUI     saveFeedback_Text;
    [SerializeField]    private Button              backToMainMenu_Button;
    [SerializeField]    private TextMeshProUGUI     resultScore_Displayer;

    [SerializeField]    private Color               defaultButtonColor;

    /// <summary>
    /// Doğru cevabı içeren butonun index değerini tutan veri.
    /// </summary>
    private int correctButtonIndex;
    /// <summary>
    /// Durum sınıflarının doğru cevap butonunun indexine
    /// ulaşabilmeleri için oluşturulmuş alan.
    /// </summary>
    public int CorrectButtonIndex => correctButtonIndex;
    /// <summary>
    /// Buton indexlerini karıştırmak ve saklamak için kullanılan dizi yapısı.
    /// </summary>
    private int[] order;

    /// <summary>
    /// Bir cevap seçeneği seçildiğinde uyarılan metot aksiyonu.
    /// <see langword="int"/> parametresi yapılan seçimde seçilen buton index'ini iletir.
    /// </summary>
    public event Action<int> OnAnswerSelected;
    /// <summary>
    /// <see cref="OnAnswerSelected"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    /// <remarks>
    /// Her seçenek butonu için farklı bir aksiyon ataması yapılıyor.
    /// </remarks>
    private UnityAction[] onAnswerSelected_Lambda;

    /// <summary>
    /// Sonraki soruya gidilmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    public event Action NextQuestionRequested;
    /// <summary>
    /// <see cref="NextQuestionRequested"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction nextQuestionRequested_Lambda;

    /// <summary>
    /// Oyun sonucu yerel Liderlik Tablosuna kaydedilmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    /// <remarks>
    /// Parametre alanı kullanıcıdan alınan takma adını iletmek için kullanılıyor.
    /// </remarks>
    public event Action<string> SaveScoreRequested;
    /// <summary>
    /// <see cref="SaveScoreRequested"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction saveScoreRequested_Lambda;

    /// <summary>
    /// Ana menüye dönülmek istendiğinde uyarılan metot aksiyonu.
    /// </summary>
    public event Action BackToMainMenu;
    /// <summary>
    /// <see cref="BackToMainMenu"/> aksiyonunun lambda notasyonu olarak kullanımı.
    /// <see cref="UnityEvent.RemoveListener"/> metodu referans karşılaştırması yaptığından
    /// lambda notasyonu olarak verilen aksiyonlar yeni bir referans yaratır.
    /// Bu yeni referans bizim kullandığımız aksiyona karşılık gelmediği için
    /// aksiyonu <see cref="Button.onClick"/> metodundan silebilmek için bu referans ile
    /// <see cref="UnityEvent.RemoveListener"/> ile kayıt olarak kullanıyoruz.
    /// </summary>
    private UnityAction backToMainManu_Lambda;

    [Header("Entry Messages")]
    [SerializeField] private string textTooLongError        = "Nickname you entered is too long (Max {0} character)!";
    [SerializeField] private string textIsWhiteSpaceError   = "Nickname you entered is empty or white space!";
    [SerializeField] private string textIsValid             = "Your nickname is valid. To save your score to local Leader Board, click the save button.";
    [SerializeField] private string saveSuccess             = "Score saved successfully to local Leader Board.";
    [SerializeField] private string saveFailure             = "Error occurred while saving. Please try again.";

    [SerializeField] private Color  default_SaveTextColor   = Color.black;
    [SerializeField] private Color  error_SaveTextColor     = Color.red;
    [SerializeField] private Color  success_SaveTextColor   = Color.green;

    [SerializeField] private int nicknameEntryLengthLimit = 20;

    void Start()
    {
        // Buton dizileri seçenek butonu sayısıyla senkronize olarak oluşturuluyor.
        choice_ButtonTexts = new TextMeshProUGUI[choice_Buttons.Length];
        onAnswerSelected_Lambda = new UnityAction[choice_Buttons.Length];

        saveScore_Button.interactable = false;

        order = new int[choice_Buttons.Length];

        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        // Lambda aksiyon referansları tanımlanarak buton dinleyicilerine kaydediliyor.
        for (int i = 0; i < choice_Buttons.Length; i++)
        {
            // Closure tuzağı. For döngü verisi lambda tarafından referans ile
            // yakalandığından döngü sonunda bütün diziye son değeri gönderir.
            // Bu yüzden her döngüde yeni bir veri referansı oluşturarak
            // closure tuzağından kaçınmış oluyoruz.
            int index = i;

            onAnswerSelected_Lambda[i] = () => OnAnswerSelected?.Invoke(index);

            choice_Buttons[i].onClick.AddListener(onAnswerSelected_Lambda[i]);

            choice_ButtonTexts[i] = choice_Buttons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        nextQuestionRequested_Lambda = () => NextQuestionRequested?.Invoke();
        nextQuestion_Button.onClick.AddListener(nextQuestionRequested_Lambda);

        userNicknameEntry_InputField.onValueChanged.AddListener(ValidateNickname);
        saveScoreRequested_Lambda = () => SaveScoreRequested?.Invoke(userNicknameEntry_InputField.text);
        saveScore_Button.onClick.AddListener(saveScoreRequested_Lambda);

        backToMainManu_Lambda = () => BackToMainMenu?.Invoke();
        backToMainMenu_Button.onClick.AddListener(backToMainManu_Lambda);
    }

    void OnDestroy()
    {
        // Obje yok edildiğinde buton dinleyicileri de temizlenir.
        for (int i = 0; i < choice_Buttons.Length; i++)
        {
            int index = i;
            choice_Buttons[i].onClick.RemoveListener(onAnswerSelected_Lambda[index]);
        }

        nextQuestion_Button.onClick.RemoveListener(nextQuestionRequested_Lambda);

        userNicknameEntry_InputField.onValueChanged.RemoveListener(ValidateNickname);
        saveScore_Button.onClick.RemoveListener(saveScoreRequested_Lambda);

        backToMainMenu_Button.onClick.RemoveListener(backToMainManu_Lambda);
    }

    /// <summary>
    /// Soru verileri verilen soruyu ekrana yazdıran metot.
    /// <see cref="QuestionManager"/> sınıfından gelen soru verileri ve
    /// seçenek butonları bu metotta işlenir. <see cref="order"/> dizisinin içeriği,
    /// buton metinlerini saklayan dizinin index değerleri olarak kullanılır.
    /// Bu dizinin içeriği karıştırılarak her sorunun seçenek butonlarının karıştırılması sağlanır.
    /// </summary>
    /// <remarks>
    /// <see cref="Question"/> verisinde <see cref="Question.choices"/> dizisi için <c>0</c> index'i
    /// her zaman doğru cevap olarak ayarlandığından <see cref="correctButtonIndex"/> verisi
    /// <c>order[0]</c> olarak kaydedilir. <see cref="order"/> dizisinin karıştırma işlemi için
    /// Fisher-Yates algoritması kullanılır.
    /// </remarks>
    /// <param name="question">
    /// <see cref="QuestionManager"/> sınıfından gelen soru verileri.
    /// </param>
    public void ShowQuestion(Question question)
    {
        if (question == null || question.choices == null || question.choices.Length < choice_Buttons.Length)
        {
            Debug.LogError("Null or invalid question.");
            return;
        }

        for (int i = order.Length - 1; i > 0; i--)
        {
            int rng = UnityEngine.Random.Range(0, i + 1);

            (order[i], order[rng]) = (order[rng], order[i]);
        }

        correctButtonIndex = order[0];

        question_Text.text = question.question;

        category_Text.text = question.category;

        // Bir önceki sorudan farklı buton renkleri kalabileceğinden seçenek butonu renkleri sıfırlanır.
        for (int i = 0; i < choice_Buttons.Length; i++)
        {
            choice_ButtonTexts[order[i]].text = question.choices[i];
            choice_Buttons[i].image.color = defaultButtonColor;
        }
    }

    /// <summary>
    /// Seçenek butonlarının erişilebilirliğini kontrol eden metot.
    /// </summary>
    /// <param name="interactable">
    /// <see langword="false"/> seçenek butonlarının etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> seçenek butonlarının etkileşime geçilebilirliğini açar.
    /// </param>
    public void SetChoiceButtonsInteractable(bool interactable)
    {
        foreach (Button choice in choice_Buttons)
        {
            choice.interactable = interactable;
        }
    }

    /// <summary>
    /// Sonraki soru butonunun erişilebilirliğini kontrol eden metot.
    /// </summary>
    /// <param name="active">
    /// <see langword="false"/> soru butonunun görünürlüğünü, etkileşime geçilebilirliğini ve raycast geçirgenliğini kapatır,
    /// <see langword="true"/> soru butonunun görünürlüğünü, etkileşime geçilebilirliğini ve raycast geçirgenliğini açar.
    /// </summary>
    public void SetNextQuestionButtonActive(bool active)
    {
        nextQuestionButton_CanvasGroup.alpha = active ? 1f : 0f;
        nextQuestionButton_CanvasGroup.interactable = active;
        nextQuestionButton_CanvasGroup.blocksRaycasts = active;
    }

    /// <summary>
    /// Seçilen cevap index'ine göre
    /// doğru cevabın index'ini karşılaştırır.
    /// Sonuca göre buton renklerini ayarlar.
    /// </summary>
    /// <param name="selectedIndex">
    /// Oyuncunun seçtiği seçenek butonun index değeri.
    /// <c>-1</c> değeri sorunun zaman aşımına uğradığı anlamına gelir.
    /// </param>
    /// <param name="correctIndex">
    /// Sorunun doğru seçeneğinin index değeri.
    /// </param>
    public void ShowAnswerResult(int selectedIndex, int correctIndex)
    {
        if (correctIndex == -1)
        {
            // Geçersiz doğru cevap index'i. Soru geçersiz.
            Debug.LogError("Invalid correct answer index.");
            return;
        }
        
        if (selectedIndex == correctIndex || selectedIndex == -1)
        {
            // Doğru cevap seçeneği seçildi veya zaman aşımına uğrandı.
            choice_Buttons[correctIndex].image.color = Color.green;
        }
        else
        {
            // Yanlış cevap seçeneği seçildi.
            choice_Buttons[selectedIndex].image.color = Color.red;
            choice_Buttons[correctIndex].image.color = Color.green;
        }
    }

    /// <summary>
    /// Sonuç ekranını gösteren metot.
    /// </summary>
    public void ShowResultScreen()
    {
        resultScreen_CanvasGroup.alpha = 1f;
        resultScreen_CanvasGroup.interactable = true;
        resultScreen_CanvasGroup.blocksRaycasts = true;
    }

    /// <summary>
    /// Sonuç ekranını gizleyen metot.
    /// </summary>
    public void HideResultScreen()
    {
        resultScreen_CanvasGroup.alpha = 0f;
        resultScreen_CanvasGroup.interactable = false;
        resultScreen_CanvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Sonuç skorunu belirleyen metot.
    /// </summary>
    /// <param name="score">
    /// Sonuç skoruna yazılması için iletilen skor puanı verisi.
    /// </param>
    public void SetResultScore(int score)
    {
        resultScore_Displayer.text = score.ToString();
    }

    /// <summary>
    /// <see cref="userNicknameEntry_InputField"/> metin kutusunun
    /// içeriği her güncellendiğinde çağırılan ve
    /// girilen metnin geçerliliğini kontrol eden metot.
    /// </summary>
    /// <remarks>
    /// Girilen metne göre <see cref="saveFeedback_Text"/> ve
    /// <see cref="saveScore_Button"/> özellikleri ayarlanır.
    /// </remarks>
    /// <param name="nicknameEntry">
    /// <see cref="userNicknameEntry_InputField"/> metin kutusu
    /// içerisine yazılan metin verisi.
    /// </param>
    private void ValidateNickname(string nicknameEntry)
    {
        // Girilen metin boş veya sadece boşluktan mı oluşuyor?
        if (string.IsNullOrWhiteSpace(nicknameEntry))
        {
            saveFeedback_Text.color = error_SaveTextColor;
            saveFeedback_Text.text = textIsWhiteSpaceError;
            SetSaveResultScoreButtonInteractable(false);
        }
        else
        {
            // Girilen metin harf sınırını aşıyor mu?
            if (nicknameEntry.Length > nicknameEntryLengthLimit)
            {
                saveFeedback_Text.color = error_SaveTextColor;
                saveFeedback_Text.text = string.Format(textTooLongError, nicknameEntryLengthLimit);
                SetSaveResultScoreButtonInteractable(false);
            }
            else
            {
                // Girilen metin geçerli.
                saveFeedback_Text.color = default_SaveTextColor;
                saveFeedback_Text.text = textIsValid;
                SetSaveResultScoreButtonInteractable(true);
            }
            
        }
    }

    /// <summary>
    /// <see cref="ValidateNickname"/> metodunun butonları
    /// aktif veya deaktif hale getirmesine yardımcı olmak için
    /// oluşturulmuş yardımcı metot.
    /// </summary>
    /// <param name="interactable">
    /// <see langword="false"/> sonuç kayıt butonlarının etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> sonuç kayıt butonlarının etkileşime geçilebilirliğini açar.
    /// </param>
    private void SetSaveResultScoreButtonInteractable(bool interactable)
    {
        saveScore_Button.interactable = interactable;
    }

    /// <summary>
    /// Geçerli girdi sonrası Liderlik Tablosu dosyası güncellenirken başka bir girdi yazılamaması için
    /// <see cref="userNicknameEntry_InputField"/> girdi alanını kapatıp açmayı sağlayan metot.
    /// </summary>
    /// <remarks>
    /// Aynı zamanda <see cref="SetSaveResultScoreButtonInteractable"/> metodunu da
    /// gönderilen parametre ile çağırır.
    /// </remarks>
    /// <param name="interactable">
    /// <see langword="false"/> kullanıcı takma adı girdi kutusunun etkileşime geçilebilirliğini kapatır,
    /// <see langword="true"/> kullanıcı takma adı girdi kutusunun etkileşime geçilebilirliğini açar.
    /// </param>
    public void SetEntryInputFieldInteractable(bool interactable)
    {
        SetSaveResultScoreButtonInteractable(interactable);

        userNicknameEntry_InputField.interactable = interactable;
    }

    /// <summary>
    /// Girdi geri bildirimlerinin iletilmesi için kullanılan bir metot.
    /// </summary>
    /// <param name="isError">
    /// Geribildirimin bir hata olup olmadığını belirten bayrak verisi
    /// </param>
    public void SetSaveFeedback(bool isError)
    {
        if (isError)
        {
            saveFeedback_Text.color = error_SaveTextColor;
            saveFeedback_Text.text  = saveFailure;
        }
        else
        {
            saveFeedback_Text.color = success_SaveTextColor;
            saveFeedback_Text.text  = saveSuccess;
        }
    }

    /// <summary>
    /// Bir skor değişimi yaşandığında skoru güncelleyen metot.
    /// </summary>
    /// <param name="score">
    /// Güncellenecek skorun hesaplanmış verisi.
    /// </param>
    public void UpdateScore(int score)
    {
        score_Text.text = score.ToString();
    }

    /// <summary>
    /// Zamanlayıcıda bir süre değişimi yaşandığında
    /// zamanlayıcıyı güncelleyen metot.
    /// </summary>
    /// <remarks>
    /// Zamanlayıcının süre değerini <see langword="int"/> veri tipinde gösteren aşırı yükleme.
    /// </remarks>
    /// <param name="remainingTime">
    /// Güncellenecek zamanlayıcı süresinin değer verisi.
    /// </param>
    public void UpdateTimer(int remainingTime)
    {
        // Tek basamaklı, basmak sayısına göre olmayan gösterim formatı.
        timerCounter_Text.text = remainingTime.ToString("0");
    }

    /// <summary>
    /// Zamanlayıcıda bir süre değişimi yaşandığında
    /// zamanlayıcıyı güncelleyen metot.
    /// </summary>
    /// <remarks>
    /// Zamanlayıcının süre değerini <see langword="float"/> veri tipinde gösteren aşırı yükleme.
    /// </remarks>
    /// <param name="remainingTime">
    /// Güncellenecek zamanlayıcı süresinin değer verisi.
    /// </param>
    public void UpdateTimer(float remainingTime)
    {
        // tek basamaklı, virgülden sonra tek basamak gösteren gösterim formatı.
        timerCounter_Text.text = remainingTime.ToString("0.0");
    }
}
