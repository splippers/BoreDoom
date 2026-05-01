namespace Chorewars.Core
{
    public static class ScoringEngine
    {
        public static ChoreResult Score(ChoreSession session)
        {
            int points = (int)(session.coveragePercent * 10f
                               + session.efficiencyScore * 5f
                               + session.movementScore * 5f);

            string grade = points switch
            {
                >= 900 => "S",
                >= 750 => "A",
                >= 600 => "B",
                >= 400 => "C",
                _ => "D"
            };

            return new ChoreResult { session = session, totalPoints = points, grade = grade };
        }
    }
}
