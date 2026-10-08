using System.Buffers.Binary;
using System.Security.Cryptography;

if (args.Length != 4)
{
    Console.Error.WriteLine("Usage: dotnet run -- <assembly-path> <bundle-id> <version> <output-path>");
    return 2;
}

var assemblyPath = args[0];
if (!ulong.TryParse(args[1], out var bundleId) || bundleId == 0)
{
    Console.Error.WriteLine("Bundle ID must be a non-zero unsigned integer.");
    return 2;
}

var version = args[2];
if (string.IsNullOrWhiteSpace(version))
{
    Console.Error.WriteLine("Version is required.");
    return 2;
}

var outputPath = args[3];
var assembly = await File.ReadAllBytesAsync(assemblyPath);

if (assembly.Length > uint.MaxValue)
{
    Console.Error.WriteLine("Assembly is too large for the FSMB v1 payload format.");
    return 2;
}

var payload = new byte[4 + 4 + 8 + 4 + assembly.Length];
var offset = 0;

"FSMB"u8.CopyTo(payload.AsSpan(offset, 4));
offset += 4;

BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset, 4), 1);
offset += 4;

BinaryPrimitives.WriteUInt64LittleEndian(payload.AsSpan(offset, 8), bundleId);
offset += 8;

BinaryPrimitives.WriteUInt32LittleEndian(payload.AsSpan(offset, 4), (uint)assembly.Length);
offset += 4;

assembly.CopyTo(payload.AsSpan(offset));

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
await File.WriteAllBytesAsync(outputPath, payload);

var hash = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();

Console.WriteLine($"bundle_id={bundleId}");
Console.WriteLine($"version={version}");
Console.WriteLine($"sha256={hash}");
Console.WriteLine($"bytes={payload.Length}");
Console.WriteLine($"path={outputPath}");

return 0;
