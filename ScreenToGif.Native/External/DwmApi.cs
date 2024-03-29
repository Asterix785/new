using System.Runtime.InteropServices;
using ScreenToGif.Domain.Enums.Native;
using ScreenToGif.Domain.Models.Native;
using ScreenToGif.Native.Structs;

namespace ScreenToGif.Native.External
{
    public static class DwmApi
    {
        [DllImport(Constants.DwmApi)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out bool pvAttribute, int cbAttribute);

        [DllImport(Constants.DwmApi)]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out NativeRect pvAttribute, int cbAttribute);

        [DllImport(Constants.DwmApi, PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttributes attr, ref int attrValue, int attrSize);

        [DllImport(Constants.DwmApi, PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

        [DllImport(Constants.DwmApi, PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();
    }
}