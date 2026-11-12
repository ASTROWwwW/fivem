// Test-only stubs for external CLR/native interfaces; the two files under
// test are included verbatim from code/client/clrcore by the test project.
using System;

namespace CitizenFX.Core
{
    public class FromSourceAttribute : Attribute { }

    public static class Debug
    {
        public static void WriteLine(string format, params object[] args)
        {
            Console.Error.WriteLine(format, args);
        }
    }

    public sealed class RemoteFunctionReference
    {
        public RemoteFunctionReference(byte[] data) { }
        public byte[] InvokeNative(byte[] data) { return new byte[] { 0x90 }; }
    }

    public static class NetworkFunctionManager
    {
        public static object CreateReference(byte[] data, string netSource) { return null; }
    }

    public static class MsgPackSerializer
    {
        public static byte[] Serialize(object value) { return new byte[] { 0x90 }; }
    }

    public struct Vector2 { public Vector2(float x, float y) { } }
    public struct Vector3 { public Vector3(float x, float y, float z) { } }
    public struct Vector4 { public Vector4(float x, float y, float z, float w) { } }
    public struct Quaternion { public Quaternion(float x, float y, float z, float w) { } }
}

namespace CitizenFX.Core.Native
{
    public static class API
    {
        public static void RegisterResourceAsEventHandler(string eventName) { }
    }
}
