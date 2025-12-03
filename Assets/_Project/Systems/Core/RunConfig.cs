using System.Collections.Generic;
using ProjectRoguelike.Procedural;
using ProjectRoguelike.Systems.Hub;

namespace ProjectRoguelike.Core
{
    /// <summary>
    /// Configuration for a procedural run - contains all parameters needed to generate a unique level.
    /// </summary>
    public sealed class RunConfig
    {
        public string DoorName { get; }
        public string LevelName { get; }
        public int Difficulty { get; }
        public int Seed { get; }
        public int TargetRoomCount { get; }
        public List<RoomData> AvailableRooms { get; }

        public RunConfig(string doorName, string levelName, int difficulty, int seed, int targetRoomCount, List<RoomData> availableRooms)
        {
            DoorName = doorName;
            LevelName = levelName;
            Difficulty = difficulty;
            Seed = seed;
            TargetRoomCount = targetRoomCount;
            AvailableRooms = availableRooms ?? new List<RoomData>();
        }

        /// <summary>
        /// Creates a RunConfig from a door configuration.
        /// </summary>
        public static RunConfig FromDoorConfig(HubManager.RunDoorConfig doorConfig, int seed, int targetRoomCount, List<RoomData> availableRooms)
        {
            return new RunConfig(
                doorConfig.doorName,
                doorConfig.levelName,
                doorConfig.difficulty,
                seed,
                targetRoomCount,
                availableRooms
            );
        }
    }
}

