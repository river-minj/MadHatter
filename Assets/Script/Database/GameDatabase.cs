/// <summary>
/// 모든 게임 데이터 Database의 단일 진입점 (Facade)
/// 기존 Database 싱글턴은 그대로 유지되며, 이 클래스는 접근 경로를 통일하는 역할만 담당
///
/// 사용법:
///   기존: ItemDatabase.Instance.GetItem("potion_hp_small")
///   신규: GameDatabase.Instance.Items.GetItem("potion_hp_small")
/// </summary>
public class GameDatabase
{
    public static GameDatabase Instance { get; private set; }

    public DialogueDatabase  Dialogues  => DialogueDatabase.Instance;
    public QuestDatabase     Quests     => QuestDatabase.Instance;
    public NpcDatabase       Npcs       => NpcDatabase.Instance;
    public ItemDatabase      Items      => ItemDatabase.Instance;
    public CompanionDatabase Companions => CompanionDatabase.Instance;
    public DropDatabase      Drops      => DropDatabase.Instance;
    public ShopDatabase      Shops      => ShopDatabase.Instance;

    public static void Initialize()
    {
        Instance = new GameDatabase();
    }
}
