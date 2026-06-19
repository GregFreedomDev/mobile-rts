public static class StarRatingCalculator
{
    // 3 stars: no losses and fast
    // 2 stars: few losses or medium time
    // 1 star:  any victory
    // 0 stars: defeat
    public static int Calculate(bool isVictory, float timeElapsed, int playerUnitsLost, int totalPlayerUnits)
    {
        if (!isVictory) return 0;

        float lossRatio = totalPlayerUnits > 0 ? (float)playerUnitsLost / totalPlayerUnits : 0f;

        if (lossRatio == 0f && timeElapsed < 60f) return 3;
        if (lossRatio <= 0.25f || timeElapsed < 90f) return 2;
        return 1;
    }
}
