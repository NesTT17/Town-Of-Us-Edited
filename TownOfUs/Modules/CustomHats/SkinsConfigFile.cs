﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TownOfUs.Modules.CustomHats;

public class SkinsConfigFile
{
    [JsonPropertyName("hats")] public List<CustomHat> Hats { get; set; }
}