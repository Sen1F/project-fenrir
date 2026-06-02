using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Fenrir.Config;
using Fenrir.Save;
using UnityEngine;

namespace Fenrir.Tests.EditMode
{
    /// <summary>
    /// Tests SaveManager round-trip: write → read → verify.
    /// Uses a temp file so nothing touches the real save path.
    /// </summary>
    public class SaveManagerTests
    {
        private string      _tempPath;
        private SaveManager _manager;

        [SetUp]
        public void SetUp()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), $"fenrir_test_{System.Guid.NewGuid()}.json");
            _manager  = new SaveManager(overridePath: _tempPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
        }

        [Test]
        public async Task SaveAsync_CreatesFile()
        {
            _manager.MarkDirty();
            await _manager.SaveAsync();
            Assert.IsTrue(File.Exists(_tempPath), "Save file should exist after SaveAsync.");
        }

        [Test]
        public async Task LoadAsync_ReturnsFalse_WhenNoFile()
        {
            bool loaded = await _manager.LoadAsync();
            Assert.IsFalse(loaded, "LoadAsync should return false when no file exists.");
        }

        [Test]
        public async Task RoundTrip_PreservesCharacterName()
        {
            _manager.Current.Character.Name = "Fenrir";
            _manager.MarkDirty();
            await _manager.SaveAsync();

            var freshManager = new SaveManager(overridePath: _tempPath);
            bool loaded = await freshManager.LoadAsync();

            Assert.IsTrue(loaded);
            Assert.AreEqual("Fenrir", freshManager.Current.Character.Name);
        }

        [Test]
        public async Task RoundTrip_PreservesTraitValue()
        {
            _manager.Current.Character.Traits.Set(Traits.TraitKey.Aggression, 72f);
            _manager.MarkDirty();
            await _manager.SaveAsync();

            var freshManager = new SaveManager(overridePath: _tempPath);
            await freshManager.LoadAsync();

            Assert.AreEqual(72f, freshManager.Current.Character.Traits.Get(Traits.TraitKey.Aggression), 0.001f);
        }

        [Test]
        public async Task RoundTrip_PreservesHasAwakened()
        {
            _manager.Current.Character.HasAwakened = true;
            _manager.MarkDirty();
            await _manager.SaveAsync();

            var freshManager = new SaveManager(overridePath: _tempPath);
            await freshManager.LoadAsync();

            Assert.IsTrue(freshManager.Current.Character.HasAwakened);
        }

        [Test]
        public async Task SaveVersion_IsPreserved()
        {
            _manager.MarkDirty();
            await _manager.SaveAsync();

            var freshManager = new SaveManager(overridePath: _tempPath);
            await freshManager.LoadAsync();

            Assert.AreEqual(SaveData.CurrentVersion, freshManager.Current.Version);
        }
    }
}
