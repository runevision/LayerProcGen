/*
 * Copyright (c) 2024 Rune Skovbo Johansen, Sythelux Rikd
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if GODOT4
using Godot;

namespace Runevision.Common;

public partial struct DPoint3
{
    // User-defined conversion from DPoint3 to Vector3
    public static explicit operator Vector3(DPoint3 p)
    {
        return new Vector3((float)p.x, (float)p.y, (float)p.z);
    }

    //  User-defined conversion from Vector3 to DPoint3
    public static implicit operator DPoint3(Vector3 p)
    {
        return new DPoint3(p.X, p.Y, p.Z);
    }
}

#endif