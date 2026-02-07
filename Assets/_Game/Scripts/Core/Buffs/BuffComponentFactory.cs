#nullable enable
using System;
using Newtonsoft.Json.Linq;

namespace PocketSquire.Arena.Core.Buffs
{
    /// <summary>
    /// Factory for creating buff components from JSON data.
    /// </summary>
    public static class BuffComponentFactory
    {
        /// <summary>
        /// Creates a buff component from a JSON object.
        /// </summary>
        /// <param name="componentData">JSON object containing component type and parameters</param>
        /// <returns>An instance of the appropriate IBuffComponent implementation</returns>
        public static IBuffComponent CreateComponent(JObject componentData)
        {
            string? type = componentData["type"]?.ToString();

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException("Component type is required");
            }

            return type switch
            {
                "StatModifierComponent" => CreateStatModifierComponent(componentData),
                "VFXComponent" => CreateVFXComponent(componentData),
                _ => throw new ArgumentException($"Unknown component type: {type}")
            };
        }

        private static StatModifierComponent CreateStatModifierComponent(JObject data)
        {
            string stat = data["stat"]?.ToString() ?? string.Empty;
            float value = data["value"]?.Value<float>() ?? 0f;
            bool isMultiplier = data["isMultiplier"]?.Value<bool>() ?? false;

            return new StatModifierComponent(stat, value, isMultiplier);
        }

        private static VFXComponent CreateVFXComponent(JObject data)
        {
            string effectId = data["effectId"]?.ToString() ?? string.Empty;
            return new VFXComponent(effectId);
        }
    }
}
