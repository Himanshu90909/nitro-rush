using System;

namespace NitroRush.Progression
{
    [Serializable]
    public class ProfileDto
    {
        public string playerId;
        public string username;
        public int level;
        public int currentXp;
        public int credits;
        public int tokens;
    }

    /// <summary>
    /// Player Level & XP Progression system.
    /// Progression Curve Formula:
    /// - xpForLevel = 100 * (int)Math.Pow(level, 1.5)
    /// </summary>
    public class PlayerProgression
    {
        public ProfileDto Profile { get; private set; } = new ProfileDto();

        public int RequiredXpForNextLevel(int currentLevel)
        {
            return (int)(100.0 * Math.Pow(currentLevel, 1.5));
        }

        public void UpdateProfile(ProfileDto newProfile)
        {
            if (newProfile != null)
            {
                Profile = newProfile;
            }
        }
    }
}
