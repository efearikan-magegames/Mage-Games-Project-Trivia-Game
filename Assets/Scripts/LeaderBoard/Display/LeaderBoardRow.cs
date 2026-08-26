using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// <see cref="LeaderBoard_Data"/> kaydını
/// <see cref="LeaderBoardDisplay.displayRow_Prefab"/> objesinde bulunan
/// ilgili textbox'larda gösteren sınıf bileşeni.
/// </summary>
public class LeaderBoardRow : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI  rank_Text;
    [SerializeField] private TMPro.TextMeshProUGUI  nickname_Text;
    [SerializeField] private Image                  sourceIcon_Image;
    [SerializeField] private Sprite                 localIcon_Sprite;
    [SerializeField] private Sprite                 webIcon_Sprite;
    [SerializeField] private TMPro.TextMeshProUGUI  time_Text;
    [SerializeField] private TMPro.TextMeshProUGUI  score_Text;

    /// <summary>
    /// <see cref="LeaderBoardManager"/> sınıfından gelen
    /// <see cref="LeaderBoard_Data"/> verisi değerlerini
    /// ilgili textbox'a yerleştiren metot.
    /// </summary>
    /// <param name="data">
    /// Gösterilmek üzere textboxlara yerleştirilecek
    /// <see cref="LeaderBoard_Data"/> verisi.
    /// </param>
    public void SetDisplay(LeaderBoard_Data data)
    {
        if (data == null)
        {
            Debug.LogError("Leader Board data is null.");
            return;
        }

        rank_Text.text      = data.rank.ToString();
        nickname_Text.text  = data.nickname;
        time_Text.text      = data.time < 0f ? "-" : $"{Mathf.FloorToInt(data.time / 60f):00}:{Mathf.FloorToInt(data.time % 60f):00}";
        score_Text.text     = data.score.ToString();

        sourceIcon_Image.sprite = data.isLocal ? localIcon_Sprite : webIcon_Sprite;
    }
}
