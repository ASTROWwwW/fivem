// No network traffic or FXServer required. Tests the actual Legacy source.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CitizenFX.Core;

internal static class Program
{
    private static int failures;

    private static void Check(bool condition, string description)
    {
        if (!condition)
        {
            failures++;
            Console.Error.WriteLine("FAIL: " + description);
        }
        else
        {
            Console.WriteLine("PASS: " + description);
        }
    }

    private static int CallbackCount(EventHandlerEntry entry)
    {
        var field = typeof(EventHandlerEntry).GetField("m_callbacks", BindingFlags.Instance | BindingFlags.NonPublic);
        return ((List<Delegate>)field.GetValue(entry)).Count;
    }

    private static byte[] NestedArrays(int count)
    {
        var payload = new byte[count + 1];
        for (int i = 0; i < count; i++) payload[i] = 0x91; // single-item array
        payload[count] = 0xC0; // nil terminator
        return payload;
    }

    private static void DecodeTests()
    {
        var simple = MsgPackDeserializer.Deserialize(new byte[] { 0x92, 0x01, 0x02 }) as List<object>;
        Check(simple != null && simple.Count == 2 && (int)simple[0] == 1, "ordinary arrays still decode");

        var map = MsgPackDeserializer.Deserialize(new byte[] { 0x81, 0xA1, 0x6B, 0x91, 0xC0 });
        Check(map is IDictionary<string, object>, "maps and nested arrays still decode");

        Check(MsgPackDeserializer.Deserialize(NestedArrays(20)) is List<object>, "bounded nesting is accepted");

        bool rejected = false;
        try
        {
            MsgPackDeserializer.Deserialize(NestedArrays(300));
        }
        catch (InvalidDataException) { rejected = true; }
        Check(rejected, "deeply nested arrays are rejected recoverably");

        // The old decoder overflowed native stack for sufficiently deep input.
        // No huge input is required to verify the new limit.
        Check(MsgPackDeserializer.Deserialize(new byte[] { 0x90 }) is List<object>,
            "subsequent calls remain usable after a rejected payload");
    }

    private static async Task CallbackTests()
    {
        var entry = new EventHandlerEntry("security:conversion");
        int called = 0;
        Action<int> callback = x => { called += x; };
        entry += callback;

        await entry.Invoke("net:1", new object[] { new List<object> { 7 } });
        Check(called == 0, "malformed argument does not execute callback");
        Check(CallbackCount(entry) == 1, "malformed argument does not unregister callback");

        await entry.Invoke("net:1", new object[] { 3 });
        Check(called == 3, "same callback still processes a valid event");

        var throwing = new EventHandlerEntry("security:exception");
        int attempts = 0;
        Action<int> throws = x => { attempts++; throw new InvalidOperationException("test-only"); };
        throwing += throws;

        await throwing.Invoke("net:1", new object[] { 1 });
        await throwing.Invoke("net:1", new object[] { 1 });
        Check(attempts == 2 && CallbackCount(throwing) == 1,
            "callback exceptions do not permanently unregister handler");
    }

    private static int Main()
    {
        DecodeTests();
        CallbackTests().GetAwaiter().GetResult();
        Console.WriteLine(failures == 0 ? "ALL SECURITY REGRESSIONS PASSED" : failures + " FAILURES");
        return failures == 0 ? 0 : 1;
    }
}
