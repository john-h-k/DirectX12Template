namespace DirectX12Template.Common
{
    public unsafe struct Ptr<T> where T : unmanaged
    {
        public Ptr(T* value) => Value = value;
        public T* Value;
    }
}
