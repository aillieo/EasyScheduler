// -----------------------------------------------------------------------
// <copyright file="PlayerLoopRegDef.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AillieoUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine.Assertions;
    using UnityEngine.LowLevel;

    internal class PlayerLoopRegDef
    {
        public Type targetType;
        public Type targetSubType;
        public Type regAsType;
        public Func<SchedulerImpl, PlayerLoopSystem.UpdateFunction> updateDelProvider;

        private static readonly List<PlayerLoopRegDef> playerLoopRegisterDefs = new List<PlayerLoopRegDef>()
        {
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.EarlyUpdate),
                null,
                typeof(Scheduler.PlayerLoop.EarlyUpdate),
                o => o.PlayerLoopEarlyUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.FixedUpdate),
                typeof(UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate),
                typeof(Scheduler.PlayerLoop.FixedUpdate),
                o => o.PlayerLoopFixedUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.PreUpdate),
                null,
                typeof(Scheduler.PlayerLoop.PreUpdate),
                o => o.PlayerLoopPreUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.Update),
                typeof(UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate),
                typeof(Scheduler.PlayerLoop.Update),
                o => o.PlayerLoopUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.PreLateUpdate),
                null,
                typeof(Scheduler.PlayerLoop.PreLateUpdate),
                o => o.PlayerLoopPreLateUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.PreLateUpdate),
                typeof(UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate),
                typeof(Scheduler.PlayerLoop.LateUpdate),
                o => o.PlayerLoopLateUpdate),
            new PlayerLoopRegDef(
                typeof(UnityEngine.PlayerLoop.PostLateUpdate),
                null,
                typeof(Scheduler.PlayerLoop.PostLateUpdate),
                o => o.PlayerLoopPostLateUpdate),
        };

        private PlayerLoopRegDef(Type targetType, Type targetSubType, Type regAsType, Func<SchedulerImpl, PlayerLoopSystem.UpdateFunction> updateDelProvider)
        {
            this.targetType = targetType;
            this.targetSubType = targetSubType;
            this.regAsType = regAsType;
            this.updateDelProvider = updateDelProvider;
        }

        internal static void RegisterPlayerLoops(SchedulerImpl schedulerImplInstance)
        {
            Assert.AreEqual(SchedulerImpl.Instance, schedulerImplInstance);
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var changed = false;

            foreach (var playerLoopRegDef in playerLoopRegisterDefs)
            {
                changed = RegisterPlayerLoop(schedulerImplInstance, ref currentPlayerLoop, playerLoopRegDef) || changed;
            }

            if (changed)
            {
                PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            }

            UnityEngine.Debug.Log(
                string.Join(
                    "\n",
                    PlayerLoop.GetCurrentPlayerLoop()
                    .subSystemList
                    .SelectMany(pls => pls.subSystemList)
                    .Select(pls => pls.type)));
        }

        internal static void UnregisterPlayerLoops(SchedulerImpl schedulerImplInstance)
        {
            Assert.AreEqual(SchedulerImpl.Instance, schedulerImplInstance);

            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var changed = false;

            foreach (var playerLoopRegDef in playerLoopRegisterDefs)
            {
                changed = UnregisterPlayerLoop(ref currentPlayerLoop, playerLoopRegDef) || changed;
            }

            if (changed)
            {
                PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            }
        }

        private static bool RegisterPlayerLoop(SchedulerImpl schedulerImplInstance, ref PlayerLoopSystem currentPlayerLoop, PlayerLoopRegDef playerLoopRegDef)
        {
            for (var i = 0; i < currentPlayerLoop.subSystemList.Length; ++i)
            {
                var loopEventType = currentPlayerLoop.subSystemList[i].type;
                if (loopEventType == playerLoopRegDef.targetType)
                {
                    PlayerLoopSystem system = currentPlayerLoop.subSystemList[i];
                    var subSystemList = new List<PlayerLoopSystem>(system.subSystemList);

                    var newPlayerLoopSystem = new PlayerLoopSystem()
                    {
                        type = playerLoopRegDef.regAsType,
                        updateDelegate = playerLoopRegDef.updateDelProvider(schedulerImplInstance),
                    };

                    var index = 0;
                    if (playerLoopRegDef.targetSubType != null)
                    {
                        index = subSystemList.FindIndex(ss => ss.type == playerLoopRegDef.targetSubType);
                    }

                    subSystemList.Insert(index, newPlayerLoopSystem);

                    system.subSystemList = subSystemList.ToArray();
                    currentPlayerLoop.subSystemList[i] = system;

                    return true;
                }
            }

            return false;
        }

        private static bool UnregisterPlayerLoop(ref PlayerLoopSystem currentPlayerLoop, PlayerLoopRegDef playerLoopRegDef)
        {
            for (var i = 0; i < currentPlayerLoop.subSystemList.Length; ++i)
            {
                var loopEventType = currentPlayerLoop.subSystemList[i].type;
                if (loopEventType == playerLoopRegDef.targetType)
                {
                    PlayerLoopSystem system = currentPlayerLoop.subSystemList[i];
                    var subSystemList = new List<PlayerLoopSystem>(system.subSystemList);

                    var removed = subSystemList.RemoveAll(pls => pls.type == playerLoopRegDef.regAsType);
                    if (removed > 0)
                    {
                        system.subSystemList = subSystemList.ToArray();
                        currentPlayerLoop.subSystemList[i] = system;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
