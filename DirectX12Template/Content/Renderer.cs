using DirectX12Template.Common;

namespace DirectX12Template.Content
{
    public abstract class Renderer
    {
        public abstract void Update(ref StepTimer timer);
        public abstract bool Render();
        public abstract void CreateWindowSizeDependentResources();
        public abstract void SaveState();
    }
}
