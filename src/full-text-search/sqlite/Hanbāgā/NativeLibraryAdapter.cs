using SQLitePCL;
using System.Runtime.InteropServices;

namespace Hanbāgā
{
    public class NativeLibraryAdapter : IGetFunctionPointer
    {
        readonly IntPtr _library;

        public NativeLibraryAdapter(string name)
            => _library = NativeLibrary.Load(name);

        public IntPtr GetFunctionPointer(string name)
            => NativeLibrary.TryGetExport(_library, name, out var address)
                ? address
                : IntPtr.Zero;
    }
}
