const fs = require('fs');
let content = fs.readFileSync('tests/unit/InventoryTests.cs', 'utf8');

// 1. Add namespace for ArenaPerk
content = content.replace(/using System\.Collections\.Generic;/g, "using System.Collections.Generic;\r\nusing PocketSquire.Arena.Core.Perks;");

// 2. Replace empty set instantiation
content = content.replace(/new HashSet<string>\(\)/g, "new List<ArenaPerk>()");

// 3. Replace single-item set instantiations with proper ArenaPerk lists
content = content.replace(/new HashSet<string>\s*\{\s*"satchel_tier_([123])"\s*\}/g, "new List<ArenaPerk> { new ArenaPerk { Id = \"satchel_tier_$1\" } }");

// 4. Replace the two-item set instantiation
content = content.replace(/new HashSet<string>\s*\{\s*"satchel_tier_1",\s*"satchel_tier_3"\s*\}/g, "new List<ArenaPerk> { new ArenaPerk { Id = \"satchel_tier_1\" }, new ArenaPerk { Id = \"satchel_tier_3\" } }");

fs.writeFileSync('tests/unit/InventoryTests.cs', content);
console.log("Replaced references in InventoryTests.cs successfully.");
