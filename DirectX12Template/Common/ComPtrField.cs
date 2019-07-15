using System;
using System.Runtime.CompilerServices;
using Windows.AI.MachineLearning;
using TerraFX.Interop;

namespace DirectX12Template.Common
{
    public unsafe struct ComPtrField<T> where T : unmanaged
    {
        public bool Equals(ComPtrField<T> other)
        {
            return _ptr == other._ptr;
        }

        public override bool Equals(object obj)
        {
            return obj is ComPtrField<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return unchecked((int) (long) _ptr);
        }

        private T* _ptr;

        public ComPtrField(T* ptr)
        {
            _ptr = ptr;
        }

        public static implicit operator ComPtrField<T>(void* value) => new ComPtrField<T>((T*)value);

        public static implicit operator ComPtrField<T>(ComPtr<T> value) => new ComPtrField<T>(value.Detach());

        public static implicit operator ComPtrField<IUnknown>(ComPtrField<T> value) => new ComPtrField<IUnknown>((IUnknown*)value._ptr);

        public static implicit operator IUnknown* (ComPtrField<T> value) => (IUnknown*)value._ptr;

        public static bool operator ==(ComPtrField<T> left, ComPtrField<T> right) => left.Ptr == right.Ptr;

        public static bool operator !=(ComPtrField<T> left, ComPtrField<T> right) => !(left == right);

        public T* Detach()
        {
            T* temp = _ptr;
            _ptr = null;
            return temp;
        }

        public T* Ptr => _ptr;

        public ref T* GetPinnableReference()
        {
            fixed (T** p = &_ptr) return ref *p;
        }

        public ref T* ReleaseGetPinnableReference()
        {
            Release();

            return ref GetPinnableReference();
        }

        public T** DangerousGetAddressOf()
        {
            fixed (T** p = &_ptr) return p;
        }

        public T** DangerousReleaseGetAddressOf()
        {
            Release();

            return DangerousGetAddressOf();
        }

        public void Dispose()
        {
            T* temp = _ptr;

            if (temp != null)
            {
                _ptr = null;

                ((IUnknown*)temp)->Release();
            }
        }
        public void Release() => Dispose();
    }
}