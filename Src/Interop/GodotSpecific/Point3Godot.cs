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

public partial struct Point3
{
    // User-defined conversion from Point to Vector3
    public static implicit operator Vector3(Point3 p)
    {
        return new Vector3(p.x, p.y, p.z);
    }

    //  User-defined conversion from Vector3 to Point
    public static explicit operator Point3(Vector3 p)
    {
        return new Point3(Mathf.FloorToInt(p.X), Mathf.FloorToInt(p.Y), Mathf.FloorToInt(p.Z));
    }
}
#endif