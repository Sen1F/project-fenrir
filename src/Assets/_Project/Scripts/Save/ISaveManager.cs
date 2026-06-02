using System.Threading.Tasks;

namespace Fenrir.Save
{
    public interface ISaveManager
    {
        SaveData Current { get; }
        Task SaveAsync();
        Task<bool> LoadAsync();
        void MarkDirty();           // flags that a save is needed on next checkpoint
    }
}
