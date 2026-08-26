/// <summary>
/// Tüm sorular tamamlandıktan ve geriye başka soru kalmadığında
/// geçilen oyun sonu oyun durumu sınıfıdır.
/// </summary>
public class FinishGameState : GameState
{
    /// <summary>
    /// Oyun sonu oyun durumu sınıfının constructor metodu.
    /// </summary>
    /// <param name="context">
    /// <see cref="GameplayManager"/> referansı içeren oyun bağlamını
    /// parent sınıfa iletir.
    /// </param>
    public FinishGameState(GameplayManager context) : base(context) { }

    public override void EnterState()
    {
        // Sonuç ekranı ekrana yansıtılır.
        context.UI.SetResultScore(context.Score);
        context.UI.ShowResultScreen();

        context.UI.SaveScoreRequested += OnSaveScoreRequested;
        context.UI.BackToMainMenu += BackToMainMenu;
    }

    public override void ExitState()
    {
        // Çıkarken sonuç ekranı da kapatılır.
        context.UI.HideResultScreen();

        // Oyun sonu durumu çıkışında aksiyon aboneliği iptal edilir.
        context.UI.SaveScoreRequested -= OnSaveScoreRequested;
        context.UI.BackToMainMenu -= BackToMainMenu;
    }

    /// <summary>
    /// Skor kaydetme butonuna basıldığında
    /// oyun sonu skorunu yerel Liderlik Tablosuna kaydeden metot.
    /// </summary>
    /// <remarks>
    /// Kayıt sonucunun başarısına göre geri bildirim metin kutusu
    /// oyuncuyu bilgilendirmek amacıyla güncellenir.
    /// </remarks>
    /// <param name="nickname">
    /// Oyuncunun kayıt işlemi için girdiği takma ad verisi.
    /// </param>
    private void OnSaveScoreRequested(string nickname)
    {
        if (context.Storage.AddEntryTo_LocalLeaderBoard(nickname, context.Score, context.ElapsedTime))
        {
            context.UI.SetSaveFeedback(false);
            context.UI.SetEntryInputFieldInteractable(false);
        }
        else
        {
            context.UI.SetSaveFeedback(true);
        }
    }

    /// <summary>
    /// Ana menüye dön butonuna basıldığında uyarılan metottur.
    /// Bağlam içerisindeki <see cref="GameplayManager.ReturnToMainMenu"/> 
    /// metodunu çağırarak ana menüye geri dönmeyi sağlar.
    /// </summary>
    private void BackToMainMenu()
    {
        context.ReturnToMainMenu();
    }
}
