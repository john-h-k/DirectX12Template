using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop;
using static DirectX12Template.Common.Errors;
using static TerraFX.Interop.Windows;

#nullable enable

namespace DirectX12Template.Common
{
    using HRESULT = Int32;


    public interface COM_IUnknown
    {
        unsafe HRESULT QueryInterface(
            Guid* riid,
            void** ppvObject
        );
        uint AddRef();
        uint Release();
    }




    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class DirectXHelper
    {
        private static int HRESULT(uint i) => (int)i;

        private static readonly Dictionary<int, string> _detailedErrorMap = new Dictionary<int, string>
        {
            [HRESULT(0x00000000)] = "S_OK - Operation successful",
            [HRESULT(0x80004004)] = "E_ABORT - Operation aborted",
            [HRESULT(0x80070005)] = "E_ACCESSDENIED - General access denied error",
            [HRESULT(0x80004005)] = "E_FAIL - Unspecified failure",
            [HRESULT(0x80070006)] = "E_HANDLE - Handle that is not valid",
            [HRESULT(0x80070057)] = "E_INVALIDARG - One or more arguments are not valid",
            [HRESULT(0x80004002)] = "E_NOINTERFACE - No such interface supported",
            [HRESULT(0x80004001)] = "E_NOTIMPL - Not implemented",
            [HRESULT(0x8007000E)] = "E_OUTOFMEMORY - Failed to allocate necessary memory",
            [HRESULT(0x80004003)] = "E_POINTER - Pointer that is not valid",
            [HRESULT(0x8000FFFF)] = "E_UNEXPECTED - Unexpected failure"
        };


        //public static unsafe float* CornflowerBlue = (float*)Marshal.AllocHGlobal(sizeof(float) * 4);
        public static unsafe float* CornflowerBlue = (float*)ArrayPool<float>.Shared.Rent(4).AsMemory().Pin().Pointer;

        static unsafe DirectXHelper()
        {
            CornflowerBlue[0] = 0.392156899f;
            CornflowerBlue[1] = 0.584313750f;
            CornflowerBlue[2] = 0.929411829f;
            CornflowerBlue[3] = 1.000000000f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static unsafe void ReleaseCom(void* com)
        {
            if (com != null)
            {
                ((IUnknown*)com)->Release();
            }
        }

        [Conditional("DEBUG")]
        public static unsafe void ReleaseCom<T>(T* pCom) where T : unmanaged, COM_IUnknown
        {
            if (pCom != null)
            {
                pCom->Release();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void ThrowIfFailed(HRESULT hr)
        {
            if (FAILED(hr))
                ThrowComException(hr);
        }

        private static unsafe char* Pin(string str)
        {
            return (char*)str.AsMemory().Pin().Pointer;
        }

        [Conditional("DEBUG")]
        public static unsafe void NameObject(void* d3Dect, string name)
        {
            ;
            ((ID3D12Object*)d3Dect)->SetName(Pin(name));
        }

        [Conditional("DEBUG")]
        public static unsafe void NameObject<T>(ComPtr<T> d3Dect, string name) where T : unmanaged
        {
            ((ID3D12Object*)d3Dect.Ptr)->SetName(Pin(name));
        }

        [Conditional("DEBUG")]
        public static unsafe void NameObject<T>(ComPtrField<T> d3Dect, string name) where T : unmanaged
        {
            ((ID3D12Object*)d3Dect.Ptr)->SetName(Pin(name));
        }

        private static string GetErrorDescription(HRESULT hr)
        {
            if (_detailedErrorMap.TryGetValue(hr, out string value))
            {
                return $"Unknown exception occured with HRESULT {hr:X8} - \"" +
                                       $"{value}\"";
            }
            else if (ErrorMap.TryGetValue(hr, out value))
            {
                return $"Unknown exception occured with HRESULT {hr:X8} - \"" +
                                       $"{value}\"";
            }
            else
            {
                return $"Unknown exception occured with HRESULT {hr:X8}";
            }

        }

        public static void ThrowComException(string message)
        {
            throw new COMException(message);
        }

        public static void ThrowComException(HRESULT hr)
        {
            throw new COMException(GetErrorDescription(hr), hr);
        }

        public static void ThrowComException(string message, HRESULT hr)
        {
            throw new COMException(message, hr);
        }

        public static float ConvertDipsToPixels(float dips, float dpi)
        {
            const float dipsPerInch = 96;
            return MathF.Floor(dips * dpi / dipsPerInch + 0.5F);
        }
    }
}