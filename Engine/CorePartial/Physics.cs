using System;
using System.Collections.Generic;
using System.Text;

namespace Altseed2
{
    public partial class Collider3D
    {

        /// <summary>
        /// 座標を取得または設定します。
        /// </summary>
        public Vector3F Position
        {
            get => _position ??= cbg_Collider3D_GetPosition(selfPtr);
            set
            {
                if (_position == value) return;
                _position = value;
                cbg_Collider3D_SetPosition(selfPtr, value);
                _transform = null;
            }
        }
        private Vector3F? _position;

        /// <summary>
        /// 回転(弧度法)を取得または設定します。
        /// </summary>
        public Quaternion Rotation
        {
            get => _rotation ??= Quaternion.FromMatrix44F(cbg_Collider3D_GetRotation(selfPtr));
            set
            {
                if (_rotation == value) return;
                _rotation = value;
                cbg_Collider3D_SetRotation(selfPtr, Matrix44F.GetQuaternion(value));
                _transform = null;
            }
        }
        private Quaternion? _rotation;

        /// <summary>
        /// 変形行列を取得または設定します。
        /// </summary>
        public Matrix44F Transform
        {
            get => _transform ??= cbg_Collider3D_GetTransform(selfPtr);
            set
            {
                if (_transform == value) return;
                _transform = value;
                cbg_Collider3D_SetTransform(selfPtr, value);
                _position = null;
                _rotation = null;
            }
        }
        private Matrix44F? _transform;

        public bool GetIsCollidedWith(Collider3D collider, out Vector3F point1, out Vector3F point2)
        {
            return Engine.Collision3DWorld.GetIsCollided(this, collider, out point1, out point2);
        }

        public bool GetIsCollidedWith(Collider3D collider)
        {
            return GetIsCollidedWith(collider, out _, out _);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj) || (obj is Collider3D col && col.selfPtr == selfPtr);
        }

        public static bool operator ==(Collider3D col1, Collider3D col2)
        {
            return col1?.Equals(col2) ?? ((object)col1 == null && (object)col2 == null);
        }

        public static bool operator !=(Collider3D col1, Collider3D col2)
        {
            return !(col1 == col2);
        }
    }

    public partial class PointCloudCollider3D : Collider3D
    {
        /// <summary>
        /// 点群を取得または設定します。
        /// </summary>
        /// <exception cref="ArgumentNullException">設定しようとした値がnull</exception>
        public IReadOnlyList<Vector3F> Cloud
        {
            get => Points?.ToArray();
            set
            {
                Points.FromEnumerable(value);
            }
        }
    }
}
