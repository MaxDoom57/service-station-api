using System;

namespace Application.Interfaces
{
    /// <summary>
    /// Tracks the last activity time of JWT tokens to support sliding-window session refresh.
    /// </summary>
    public interface ITokenActivityService
    {
        /// <summary>
        /// Records (or updates) the last-seen timestamp for the given token.
        /// The activity entry is kept in cache for 1 hour from last touch.
        /// </summary>
        void Touch(string token);

        /// <summary>
        /// Returns true if the token has been active within the last hour.
        /// </summary>
        bool IsActiveWithinWindow(string token);

        /// <summary>
        /// Removes the activity record for the given token (e.g. on logout or after rotation).
        /// </summary>
        void Remove(string token);
    }
}
