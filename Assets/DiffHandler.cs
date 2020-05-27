using System;

using UnityEngine;

public class DiffHandler : MonoBehaviour, IEquatable<DiffHandler> {
    public Vector2 ImageSpaceCoordinates;
    public int Id;
    public float Radius = 1;

    public bool Equals(DiffHandler other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return base.Equals(other) && Id == other.Id;
    }

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((DiffHandler) obj);
    }

    public override int GetHashCode() {
        unchecked {
            return (base.GetHashCode() * 397) ^ Id;
        }
    }
}
