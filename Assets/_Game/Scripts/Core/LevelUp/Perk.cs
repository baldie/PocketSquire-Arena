#nullable enable
using System;
using System.Collections.Generic;

namespace PocketSquire.Arena.Core.LevelUp
{
    public class Perk
    {
        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public int MinLevel { get; private set; }
        public List<PlayerClass.ClassName> AllowedClasses { get; }
        public List<string> PrerequisitePerkIds { get; private set; }

        public Perk(string id, string displayName, string description, int minLevel, List<string>? prerequisitePerkIds, List<PlayerClass.ClassName> allowedClasses)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            MinLevel = minLevel;
            AllowedClasses = allowedClasses;
            PrerequisitePerkIds = prerequisitePerkIds ?? new List<string>();
        }
    }
}

