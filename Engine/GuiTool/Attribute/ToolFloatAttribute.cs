﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Altseed2
{

    /// <summary>
    /// 
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class ToolFloatAttribute : ToolAttributeBase
    {
        /// <summary>
        /// 
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// 
        /// </summary>
        public float Min { get; }

        /// <summary>
        /// 
        /// </summary>
        public float Max { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="speed"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public ToolFloatAttribute(string name = null, float speed = 1, float min = -100, float max = 100)
            : base(name)
        {
            Speed = speed;
            Min = min;
            Max = max;
        }
    }
}
