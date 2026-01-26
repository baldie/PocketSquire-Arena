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
        public List<string> PrerequisitePerkIds { get; private set; }

        public Perk(string id, string displayName, string description, int minLevel, List<string>? prerequisitePerkIds)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            MinLevel = minLevel;
            PrerequisitePerkIds = prerequisitePerkIds ?? new List<string>();
        }
    }
}

