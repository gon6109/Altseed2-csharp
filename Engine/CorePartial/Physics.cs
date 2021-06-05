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
