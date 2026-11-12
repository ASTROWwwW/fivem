# Legacy C# runtime security regression tests

These tests compile the actual Legacy `MsgPackDeserializer.cs` and
`EventHandlerDictionary.cs` from this repository, with test-only stubs for
unrelated host-native dependencies.

Run from the repository root:

```sh
dotnet run --project tests/security/LegacyCSharp/LegacyCSharpSecurityTests.csproj --configuration Release
```

Coverage:
- Ordinary MessagePack arrays and maps still decode.
- Deeply nested arrays fail as recoverable exceptions instead of stack overflow.
- An invalid argument does not unregister a persistent event handler.
- The same handler still processes a subsequent valid event.
- Exceptions inside handler code no longer permanently unregister that handler.

Limitations:
- This is an isolated .NET 8 regression harness, **not** a test against FXServer.
- Compile and execute under Mono as well when a compatible framework build is available.
- The C# v2 decoder is not linked into this harness; it needs separate runtime tests.
- Always run compatibility and integration tests before shipping the mitigation.
