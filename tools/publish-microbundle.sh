#!/usr/bin/env bash
set -euo pipefail

assembly_path="${1:?assembly path is required}"
bundle_id="${2:?bundle id is required}"
version="${3:?version is required}"
output_path="${4:?output path is required}"

python3 - "$assembly_path" "$bundle_id" "$output_path" <<'PY'
import hashlib
import pathlib
import struct
import sys

assembly = pathlib.Path(sys.argv[1]).read_bytes()
bundle_id = int(sys.argv[2])
output = pathlib.Path(sys.argv[3])

payload = (
    b"FSMB"
    + struct.pack("<I", 1)
    + struct.pack("<Q", bundle_id)
    + struct.pack("<I", len(assembly))
    + assembly
)

output.parent.mkdir(parents=True, exist_ok=True)
output.write_bytes(payload)

print(f"bundle_id={bundle_id}")
print(f"sha256={hashlib.sha256(payload).hexdigest()}")
print(f"bytes={len(payload)}")
print(f"path={output}")
PY
