using System.Buffers.Binary;
using System.Text;
using TheSingularityWorkshop.FSM_Serialization;

namespace TheSingularityWorkshop.Experiences.Catalog;

/// <summary>Semantic maturity of an authored room or exhibit.</summary>
public enum ExperienceMaturity : byte
{
    Concept = 0,
    AuthoredData = 1,
    Calculated = 2,
    Simulated = 3,
    Validated = 4
}

/// <summary>Renderer-independent, Experience-owned definition of a room.</summary>
public sealed class ExperienceRoom
{
    public string Key { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string VisitorAction { get; set; } = string.Empty;
    public string AuthoritativeState { get; set; } = string.Empty;
    public string EvidenceRule { get; set; } = string.Empty;
    public ExperienceMaturity Maturity { get; set; }
    public List<string> RequiredCapabilities { get; } = [];
    public List<string> NextRooms { get; } = [];
}

/// <summary>
/// A versioned, typed room catalog. The catalog owns its binary layout; FSM_Serialization
/// supplies the byte-stream boundary. Strings are UTF-8 with signed 32-bit byte lengths.
/// Integers are little-endian. Collection counts are signed 32-bit values.
/// </summary>
public sealed class ExperienceRoomCatalog : IBinarySerializable
{
    private const uint Magic = 0x54414345; // "ECAT" in little-endian bytes
    private const int CurrentFormatVersion = 1;
    private const int MaximumStringBytes = 1_048_576;
    private const int MaximumRooms = 1_000;
    private const int MaximumItemsPerRoom = 1_000;

    public int SchemaVersion { get; set; } = 1;
    public string ExperienceKey { get; set; } = string.Empty;
    public string Status { get; set; } = "preforge";
    public List<ExperienceRoom> Rooms { get; } = [];

    /// <summary>Checks catalog identity, room keys/sequences, and all navigation links.</summary>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();
        if (SchemaVersion != 1) errors.Add("Unsupported catalog schema version.");
        if (string.IsNullOrWhiteSpace(ExperienceKey)) errors.Add("ExperienceKey is required.");
        if (string.IsNullOrWhiteSpace(Status)) errors.Add("Status is required.");
        if (Rooms.Count == 0) errors.Add("At least one room is required.");

        var keys = new HashSet<string>(StringComparer.Ordinal);
        var sequences = new HashSet<int>();
        foreach (var room in Rooms)
        {
            if (string.IsNullOrWhiteSpace(room.Key)) errors.Add("Room key is required.");
            else if (!keys.Add(room.Key)) errors.Add($"Duplicate room key '{room.Key}'.");
            if (room.Sequence < 1 || room.Sequence > 99) errors.Add($"Room '{room.Key}' has an invalid sequence.");
            if (!sequences.Add(room.Sequence)) errors.Add($"Duplicate room sequence '{room.Sequence}'.");
            if (string.IsNullOrWhiteSpace(room.Title)) errors.Add($"Room '{room.Key}' requires a title.");
            if (string.IsNullOrWhiteSpace(room.Purpose)) errors.Add($"Room '{room.Key}' requires a purpose.");
            if (string.IsNullOrWhiteSpace(room.VisitorAction)) errors.Add($"Room '{room.Key}' requires a visitor action.");
            if (string.IsNullOrWhiteSpace(room.AuthoritativeState)) errors.Add($"Room '{room.Key}' requires an authoritative-state description.");
            if (string.IsNullOrWhiteSpace(room.EvidenceRule)) errors.Add($"Room '{room.Key}' requires an evidence rule.");
            if (!Enum.IsDefined(room.Maturity)) errors.Add($"Room '{room.Key}' has an unknown maturity value.");
            if (room.RequiredCapabilities.Distinct(StringComparer.Ordinal).Count() != room.RequiredCapabilities.Count)
                errors.Add($"Room '{room.Key}' contains duplicate required capabilities.");
            if (room.NextRooms.Distinct(StringComparer.Ordinal).Count() != room.NextRooms.Count)
                errors.Add($"Room '{room.Key}' contains duplicate navigation links.");
        }

        foreach (var room in Rooms)
            foreach (var next in room.NextRooms)
                if (!keys.Contains(next)) errors.Add($"Room '{room.Key}' links to missing room '{next}'.");

        return errors;
    }

    public void Pack(IBinaryStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var errors = Validate();
        if (errors.Count != 0)
            throw new InvalidOperationException("Cannot pack an invalid room catalog: " + string.Join(" ", errors));

        WriteUInt32(stream, Magic);
        WriteInt32(stream, CurrentFormatVersion);
        WriteInt32(stream, SchemaVersion);
        WriteString(stream, ExperienceKey);
        WriteString(stream, Status);
        WriteInt32(stream, Rooms.Count);
        foreach (var room in Rooms)
        {
            WriteString(stream, room.Key);
            WriteInt32(stream, room.Sequence);
            WriteString(stream, room.Title);
            WriteString(stream, room.Purpose);
            WriteString(stream, room.VisitorAction);
            WriteString(stream, room.AuthoritativeState);
            WriteString(stream, room.EvidenceRule);
            stream.Write([(byte)room.Maturity]);
            WriteStrings(stream, room.RequiredCapabilities);
            WriteStrings(stream, room.NextRooms);
        }
    }

    public void Unpack(IBinaryStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (ReadUInt32(stream) != Magic) throw new InvalidDataException("Not an Experience room catalog binary payload.");
        var formatVersion = ReadInt32(stream);
        if (formatVersion != CurrentFormatVersion) throw new InvalidDataException($"Unsupported binary format version {formatVersion}.");

        var schemaVersion = ReadInt32(stream);
        var experienceKey = ReadString(stream);
        var status = ReadString(stream);
        var roomCount = ReadCount(stream, MaximumRooms, "room");
        var rooms = new List<ExperienceRoom>(roomCount);
        for (var i = 0; i < roomCount; i++)
        {
            var room = new ExperienceRoom
            {
                Key = ReadString(stream),
                Sequence = ReadInt32(stream),
                Title = ReadString(stream),
                Purpose = ReadString(stream),
                VisitorAction = ReadString(stream),
                AuthoritativeState = ReadString(stream),
                EvidenceRule = ReadString(stream),
                Maturity = (ExperienceMaturity)ReadByte(stream)
            };
            room.RequiredCapabilities.AddRange(ReadStrings(stream));
            room.NextRooms.AddRange(ReadStrings(stream));
            rooms.Add(room);
        }

        var restored = new ExperienceRoomCatalog
        {
            SchemaVersion = schemaVersion,
            ExperienceKey = experienceKey,
            Status = status
        };
        restored.Rooms.AddRange(rooms);
        var errors = restored.Validate();
        if (errors.Count != 0)
            throw new InvalidDataException("Decoded catalog is invalid: " + string.Join(" ", errors));

        SchemaVersion = restored.SchemaVersion;
        ExperienceKey = restored.ExperienceKey;
        Status = restored.Status;
        Rooms.Clear();
        Rooms.AddRange(restored.Rooms);
    }

    private static void WriteStrings(IBinaryStream stream, IReadOnlyCollection<string> values)
    {
        WriteInt32(stream, values.Count);
        foreach (var value in values) WriteString(stream, value);
    }

    private static List<string> ReadStrings(IBinaryStream stream)
    {
        var count = ReadCount(stream, MaximumItemsPerRoom, "string");
        var values = new List<string>(count);
        for (var i = 0; i < count; i++) values.Add(ReadString(stream));
        return values;
    }

    private static void WriteString(IBinaryStream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        if (bytes.Length > MaximumStringBytes) throw new InvalidOperationException("Catalog string exceeds the binary format limit.");
        WriteInt32(stream, bytes.Length);
        stream.Write(bytes);
    }

    private static string ReadString(IBinaryStream stream)
    {
        var length = ReadCount(stream, MaximumStringBytes, "string byte");
        var bytes = new byte[length];
        ReadExactly(stream, bytes);
        return new UTF8Encoding(false, true).GetString(bytes);
    }

    private static int ReadCount(IBinaryStream stream, int maximum, string description)
    {
        var count = ReadInt32(stream);
        if (count < 0 || count > maximum) throw new InvalidDataException($"Invalid {description} count/length {count}.");
        return count;
    }

    private static void WriteInt32(IBinaryStream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static int ReadInt32(IBinaryStream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        ReadExactly(stream, bytes);
        return BinaryPrimitives.ReadInt32LittleEndian(bytes);
    }

    private static void WriteUInt32(IBinaryStream stream, uint value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
        stream.Write(bytes);
    }

    private static uint ReadUInt32(IBinaryStream stream)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        ReadExactly(stream, bytes);
        return BinaryPrimitives.ReadUInt32LittleEndian(bytes);
    }

    private static byte ReadByte(IBinaryStream stream)
    {
        Span<byte> bytes = stackalloc byte[1];
        ReadExactly(stream, bytes);
        return bytes[0];
    }

    private static void ReadExactly(IBinaryStream stream, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = stream.Read(buffer[read..]);
            if (count <= 0) throw new EndOfStreamException("Binary catalog ended before the field was complete.");
            read += count;
        }
    }
}

/// <summary>Authored, explicitly preforge Fluid Dynamics Laboratory room journey.</summary>
public static class FluidDynamicsLaboratoryCatalog
{
    public static ExperienceRoomCatalog Create() => new()
    {
        ExperienceKey = "fluid-dynamics-laboratory",
        Status = "preforge",
        Rooms =
        {
            Room("water-gallery", 1, "Water Gallery",
                "Introduce the modeled water domain, units, boundaries, and limitations.",
                "Inspect a basin definition and select the model whose scope matches the question.",
                "Versioned basin inputs, boundary conditions, model identity, and declared units.",
                "A water-level field is not a complete three-dimensional fluid state.",
                "wave-basin", "bathymetry-terrain", "evidence-timeline"),
            Room("wave-basin", 2, "Wave Basin",
                "Explore wave evolution with an explicitly selected reference model.",
                "Change supported initial wave conditions and compare model output.",
                "Initial conditions, boundary conditions, time settings, and wave-model output.",
                "Label the 1D wave reference separately from 2D shallow-water behavior.",
                "bathymetry-terrain", "evidence-timeline"),
            Room("bathymetry-terrain", 3, "Bathymetry & Terrain",
                "Inspect how a configured depth field shapes a basin model.",
                "Change the basin depth map and compare supported water-state outputs.",
                "Versioned basin geometry, bathymetry, initial water state, and model output.",
                "Terrain affects water only when the selected model consumes the terrain data.",
                "rainfall-runoff", "shoreline-structures", "evidence-timeline"),
            Room("rainfall-runoff", 4, "Rainfall & Runoff",
                "Study declared rainfall inputs and their supported contribution to basin state.",
                "Vary a rainfall-rate field and inspect the resulting evidence.",
                "Rainfall-rate input and basin state, with explicit model and boundary assumptions.",
                "A rainfall field alone does not model infiltration, drainage, or a complete watershed.",
                "wave-basin", "evidence-timeline"),
            Room("vessel-test-tank", 5, "Vessel Test Tank",
                "Prepare experiments for vessel and floating-body interaction with water.",
                "Configure a vessel preview and inspect which force models are available.",
                "Authored vessel geometry and, only when integrated, coupled body and fluid state.",
                "Do not claim buoyancy, drag, or simulated vessel motion until a validated coupling model supplies it.",
                "wave-basin", "shoreline-structures", "evidence-timeline"),
            Room("shoreline-structures", 6, "Shoreline & Structures",
                "Explore boundary geometry and future water-to-structure interactions.",
                "Compare an authored shoreline or barrier and inspect supported boundary behavior.",
                "Boundary geometry and any explicitly selected, implemented interaction model.",
                "Do not invent structural loads, erosion, or damage from visual contact alone.",
                "vessel-test-tank", "evidence-timeline"),
            Room("evidence-timeline", 7, "Evidence & Timeline",
                "Make basin inputs, model identity, outputs, and revisions inspectable.",
                "Compare experiment revisions and replay or export a recorded run when supported.",
                "Experiment revision, inputs, model/version, outputs, units, and diagnostics.",
                "Label results by maturity and preserve provenance; a fixture is not a live run.",
                "water-gallery")
        }
    };

    private static ExperienceRoom Room(string key, int sequence, string title, string purpose,
        string visitorAction, string authoritativeState, string evidenceRule, params string[] nextRooms)
    {
        var room = new ExperienceRoom
        {
            Key = key, Sequence = sequence, Title = title, Purpose = purpose,
            VisitorAction = visitorAction, AuthoritativeState = authoritativeState,
            EvidenceRule = evidenceRule, Maturity = ExperienceMaturity.Concept
        };
        room.NextRooms.AddRange(nextRooms);
        return room;
    }
}
