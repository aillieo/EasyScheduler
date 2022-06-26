// -----------------------------------------------------------------------
// <copyright file="IScheduledTask.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    /// <summary>
    /// Interface for a scheduled task.
    /// </summary>
    public interface IScheduledTask
    {
        /// <summary>
        /// Unschedule this task.
        /// </summary>
        /// <returns>Unschedule succeed.</returns>
        bool Unschedule();
    }
}
