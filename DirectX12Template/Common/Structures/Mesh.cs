using TerraFX.Interop;

namespace DirectX12Template.Common.Structures
{
    public unsafe struct Mesh
    {
        public static Mesh Create()
        {
            return new Mesh {IndexFormat = DXGI_FORMAT.DXGI_FORMAT_R16_UINT};
        }

        public ID3D12Resource* VertexBufferGpu { get; set; }
        public ID3D12Resource* IndexBufferGpu { get; set; }

        public uint VertexByteStride { get; set; }
        public uint VertexBufferByteSize { get; set; }
        private DXGI_FORMAT IndexFormat { get; set; }
        private uint IndexBufferByteSize { get; set; }
    }


    public readonly struct Submesh
    {
        public Submesh(uint indexCount, uint startIndexLocation, int baseVertexLocation)
        {
            IndexCount = indexCount;
            StartIndexLocation = startIndexLocation;
            BaseVertexLocation = baseVertexLocation;
        }

        public uint IndexCount { get; }
        public uint StartIndexLocation { get; }
        public int BaseVertexLocation { get; }
    }
}