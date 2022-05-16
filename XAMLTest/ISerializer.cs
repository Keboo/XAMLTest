﻿using System;
using System.Windows.Media;

namespace XamlTest;

public interface ISerializer
{
    bool CanSerialize(Type type);
    string Serialize(Type type, object? value);
    object? Deserialize(Type type, string value);
}
