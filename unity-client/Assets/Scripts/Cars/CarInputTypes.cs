namespace NitroRush.Cars
{
    /// <summary>
    /// Contract for driving control inputs (human player keyboard/gamepad or AI driver).
    /// </summary>
    public interface ICarInput
    {
        /// <summary>
        /// Throttle input axis: +1.0 for full throttle forward, -1.0 for full braking/reverse.
        /// </summary>
        float Throttle { get; }

        /// <summary>
        /// Steering input axis: -1.0 for full left, +1.0 for full right.
        /// </summary>
        float Steer { get; }

        /// <summary>
        /// True while handbrake/drift button is actively held.
        /// </summary>
        bool DriftHeld { get; }

        /// <summary>
        /// True while nitro boost trigger button is actively held.
        /// </summary>
        bool NitroHeld { get; }
    }
}
