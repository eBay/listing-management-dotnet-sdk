#!/usr/bin/env bash

# Copyright 2026 eBay Inc.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

set -euo pipefail

echo "Restoring tools..."
dotnet tool restore

echo "Regenerating REST clients via Kiota..."
rm -rf ./EBay.ListingManagement/Generated

dotnet kiota generate \
  -l csharp \
  -d specs/openapi/feed.yaml \
  -c FeedApiClient \
  -n EBay.ListingManagement.Generated.Feed \
  -o EBay.ListingManagement/Generated/Feed \
  --ebc --co

dotnet kiota generate \
  -l csharp \
  -d specs/openapi/inventory.yaml \
  -c InventoryApiClient \
  -n EBay.ListingManagement.Generated.Inventory \
  -o EBay.ListingManagement/Generated/Inventory \
  --ebc --co

dotnet kiota generate \
  -l csharp \
  -d specs/openapi/media.yaml \
  -c MediaApiClient \
  -n EBay.ListingManagement.Generated.Media \
  -o EBay.ListingManagement/Generated/Media \
  --ebc --co

dotnet kiota generate \
  -l csharp \
  -d specs/openapi/stores.yaml \
  -c StoresApiClient \
  -n EBay.ListingManagement.Generated.Stores \
  -o EBay.ListingManagement/Generated/Stores \
  --ebc --co

echo "Adding license headers to generated files..."
./add-license-headers.sh ./EBay.ListingManagement/Generated

echo "Building (triggers StrawberryShake GraphQL codegen)..."
DOTNET_ROLL_FORWARD=LatestMajor dotnet build EBay.ListingManagement/EBay.ListingManagement.csproj

echo "Done."
