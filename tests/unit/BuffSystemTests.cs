using NUnit.Framework;
using PocketSquire.Arena.Core;
using PocketSquire.Arena.Core.Buffs;
using System.IO;
using System;

namespace PocketSquire.Arena.Tests
{
    [TestFixture]
    public class BuffSystemTests
    {
        private string GetProjectRoot()
        {
            string current = Environment.CurrentDirectory;
            while (!Directory.Exists(Path.Combine(current, "Assets")) && Directory.GetParent(current) != null)
            {
                var parent = Directory.GetParent(current);
                if (parent == null) break;
                current = parent.FullName;
            }
            return current;
        }

        [Test]
        public void IBuffComponent_InterfaceExists()
        {
            // This test verifies the IBuffComponent interface is defined
            var type = typeof(IBuffComponent);
            Assert.That(type.IsInterface, Is.True, "IBuffComponent should be an interface");
        }

        [Test]
        public void Buff_CanBeCreated()
        {
            // Arrange & Act
            var buff = new Buff("test_id", "Test Buff", 10.0f);

            // Assert
            Assert.That(buff.Id, Is.EqualTo("test_id"));
            Assert.That(buff.Name, Is.EqualTo("Test Buff"));
            Assert.That(buff.Duration, Is.EqualTo(10.0f));
            Assert.That(buff.Components, Is.Not.Null);
        }

        [Test]
        public void StatModifierComponent_ModifiesStatOnApply()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });
            var component = new StatModifierComponent("Defense", 10.0f, false);

            // Act
            component.OnApply(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(15), "Defense should be increased by 10");
        }

        [Test]
        public void StatModifierComponent_RevertsStatOnRemove()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });
            var component = new StatModifierComponent("Defense", 10.0f, false);
            component.OnApply(entity);

            // Act
            component.OnRemove(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(5), "Defense should be reverted to original value");
        }

        [Test]
        public void StatModifierComponent_MultiplierModifiesStat()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Strength = 10 });
            var component = new StatModifierComponent("AttackSpeed", 1.25f, true);

            // Act
            component.OnApply(entity);

            // Assert
            Assert.That(entity.Attributes.Strength, Is.EqualTo(12), "Strength should be multiplied by 1.25 (10 * 1.25 = 12)");
        }

        [Test]
        public void StatModifierComponent_RevertsMultiplierOnRemove()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Strength = 10 });
            var component = new StatModifierComponent("AttackSpeed", 1.25f, true);
            component.OnApply(entity);

            // Act
            component.OnRemove(entity);

            // Assert
            Assert.That(entity.Attributes.Strength, Is.EqualTo(10), "Strength should be reverted to original value");
        }

        [Test]
        public void VFXComponent_CanBeCreated()
        {
            // Arrange & Act
            var component = new VFXComponent("red_glow_aura");

            // Assert
            Assert.That(component.EffectId, Is.EqualTo("red_glow_aura"));
        }

        [Test]
        public void VFXComponent_DoesNotThrowOnApplyRemove()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes());
            var component = new VFXComponent("red_glow_aura");

            // Act & Assert
            Assert.DoesNotThrow(() => component.OnApply(entity));
            Assert.DoesNotThrow(() => component.OnRemove(entity));
        }

        [Test]
        public void Buff_AppliesAllComponents()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });
            var buff = new Buff("test_buff", "Test Buff", 10.0f);
            buff.Components.Add(new StatModifierComponent("Defense", 10.0f, false));
            buff.Components.Add(new VFXComponent("test_vfx"));

            // Act
            buff.Apply(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(15), "Defense should be modified");
            Assert.That(buff.Components.Count, Is.EqualTo(2), "Buff should have 2 components");
        }

        [Test]
        public void Buff_RemovesAllComponents()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });
            var buff = new Buff("test_buff", "Test Buff", 10.0f);
            buff.Components.Add(new StatModifierComponent("Defense", 10.0f, false));
            buff.Apply(entity);

            // Act
            buff.Remove(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(5), "Defense should be reverted");
        }

        [Test]
        public void Buff_SupportsMultipleComponents()
        {
            // Arrange
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5, Strength = 10 });
            var buff = new Buff("multi_buff", "Multi Buff", 10.0f);
            buff.Components.Add(new StatModifierComponent("Defense", 10.0f, false));
            buff.Components.Add(new StatModifierComponent("Strength", 5.0f, false));

            // Act
            buff.Apply(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(15), "Defense should be modified");
            Assert.That(entity.Attributes.Strength, Is.EqualTo(15), "Strength should be modified");
        }

        [Test]
        public void GameWorld_LoadBuffs_LoadsFromJson()
        {
            // Arrange
            string root = GetProjectRoot();
            string path = Path.Combine(root, "Assets/_Game/Data/buffs.json");
            Assert.That(File.Exists(path), Is.True, $"Buff file not found at: {path}");

            // Act
            GameWorld.Load(root);

            // Assert
            Assert.That(GameWorld.AllBuffs.Count, Is.GreaterThan(0), "Buffs list should not be empty");
        }

        [Test]
        public void GameWorld_LoadBuffs_PopulatesBuffProperties()
        {
            // Arrange
            string root = GetProjectRoot();

            // Act
            GameWorld.Load(root);

            // Assert
            var firstBuff = GameWorld.AllBuffs[0];
            Assert.That(firstBuff.Id, Is.Not.Null.And.Not.Empty, "Buff should have an ID");
            Assert.That(firstBuff.Name, Is.Not.Null.And.Not.Empty, "Buff should have a name");
            Assert.That(firstBuff.Duration, Is.GreaterThan(0), "Buff should have positive duration");
            Assert.That(firstBuff.Components, Is.Not.Null, "Buff should have components");
        }

        [Test]
        public void GameWorld_LoadBuffs_LoadsSquireFrenzyBuff()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);

            // Act
            var buff = GameWorld.GetBuffById("squire_frenzy");

            // Assert
            Assert.That(buff, Is.Not.Null, "Squire's Frenzy buff should exist");
            Assert.That(buff!.Name, Is.EqualTo("Squire's Frenzy"));
            Assert.That(buff.Duration, Is.EqualTo(10.0f));
            Assert.That(buff.Components.Count, Is.EqualTo(2), "Squire's Frenzy should have 2 components");
        }

        [Test]
        public void GameWorld_LoadBuffs_LoadsIronSkinBuff()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);

            // Act
            var buff = GameWorld.GetBuffById("iron_skin");

            // Assert
            Assert.That(buff, Is.Not.Null, "Iron Skin buff should exist");
            Assert.That(buff!.Name, Is.EqualTo("Iron Skin"));
            Assert.That(buff.Duration, Is.EqualTo(15.0f));
            Assert.That(buff.Components.Count, Is.EqualTo(1), "Iron Skin should have 1 component");
        }

        [Test]
        public void GameWorld_GetBuffById_ReturnsNullForUnknownId()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);

            // Act
            var result = GameWorld.GetBuffById("nonexistent_buff_12345");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void IronSkinBuff_IncreasesDefense()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);
            var buff = GameWorld.GetBuffById("iron_skin");
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });

            // Act
            buff!.Apply(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(15), "Iron Skin should increase defense by 10");
        }

        [Test]
        public void IronSkinBuff_RevertsDefenseOnRemoval()
        {
            // Arrange
            string root = GetProjectRoot();
            GameWorld.Load(root);
            var buff = GameWorld.GetBuffById("iron_skin");
            var entity = new Entity("Test Entity", 100, 100, new Attributes { Defense = 5 });
            buff!.Apply(entity);

            // Act
            buff.Remove(entity);

            // Assert
            Assert.That(entity.Attributes.Defense, Is.EqualTo(5), "Defense should be reverted after Iron Skin expires");
        }
    }
}
