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

public partial struct DPoint
{
    // User-defined conversion from DPoint to Vector3
    public static explicit operator Vector3(DPoint p)
    {
        return new Vector3((float)p.x, (float)p.y, 0);
    }

    //  User-defined conversion from Vector3 to DPoint
    public static explicit operator DPoint(Vector3 p)
    {
        return new DPoint(p.X, p.Y);
    }

    // User-defined conversion from DPoint to Vector2
    public static explicit operator Vector2(DPoint p)
    {
        return new Vector2((float)p.x, (float)p.y);
    }

    //  User-defined conversion from Vector2 to DPoint
    public static implicit operator DPoint(Vector2 p)
    {
        return new DPoint(p.X, p.Y);
    }
}

#endif