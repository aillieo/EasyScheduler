// -----------------------------------------------------------------------
// <copyright file="Scheduler.PlayerLoop.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    /// <summary>
    /// Player loop event types used by Scheduler.
    /// </summary>
    public static partial class Scheduler
    {
        internal struct PlayerLoop
        {
            public struct EarlyUpdate
            {
            }

            public struct FixedUpdate
            {
            }

            public struct PreUpdate
            {
            }

            public struct Update
            {
            }

            public struct PreLateUpdate
            {
            }

            public struct LateUpdate
            {
            }

            public struct PostLateUpdate
            {
            }
        }
    }
}
