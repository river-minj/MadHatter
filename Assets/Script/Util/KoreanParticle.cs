public static class KoreanParticle
{
    // 을/를
    public static string EulReul(string word)
    {
        char last = LastChar(word);
        if (last == '\0') return "을";
        return HasFinalConsonant(last) ? "을" : "를";
    }

    // 이/가
    public static string IGA(string word)
    {
        char last = LastChar(word);
        if (last == '\0') return "이";
        return HasFinalConsonant(last) ? "이" : "가";
    }

    // 은/는
    public static string EunNeun(string word)
    {
        char last = LastChar(word);
        if (last == '\0') return "은";
        return HasFinalConsonant(last) ? "은" : "는";
    }

    private static char LastChar(string word)
    {
        if (string.IsNullOrEmpty(word)) return '\0';
        return word[word.Length - 1];
    }

    // 한글 완성형 음절에서 받침 여부 판별
    // 가(0xAC00) ~ 힣(0xD7A3), (코드 - 0xAC00) % 28 == 0 이면 받침 없음
    private static bool HasFinalConsonant(char c)
    {
        if (c < 0xAC00 || c > 0xD7A3) return false;
        return (c - 0xAC00) % 28 != 0;
    }
}
