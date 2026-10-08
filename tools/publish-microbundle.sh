#!/usr/bin/env bash
set -euo pipefail

assembly_path="${1:?assembly path is required}"
bundle_id="${2:?bundle id is required}"
version="${3:?version is required}"
output_path="${4:?output path is required}"

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"

dotnet run --project "${repo_root}/tools/PackMicroBundle/PackMicroBundle.csproj" --configuration Release --   "${repo_root}/${assembly_path}"   "${bundle_id}"   "${version}"   "${repo_root}/${output_path}"
