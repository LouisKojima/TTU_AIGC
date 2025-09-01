using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAVTypes.AmbientColor
{
    public record ColorSliderPosition
    {
        public float color;
        public float saturation = 1;
        public float brightness = 1;

        public ColorSliderPosition() { }
        public ColorSliderPosition(float color, float saturation = 1, float brightness = 1)
        {
            this.color = color;
            this.saturation = saturation;
            this.brightness = brightness;
        }

        public static implicit operator Vector3(ColorSliderPosition csp)
        {
            return new(csp.color, csp.saturation, csp.brightness);
        }
        public static implicit operator ColorSliderPosition(Vector3 v)
        {
            return new(v.x, v.y, v.z);
        }
        public static implicit operator ColorSliderPosition(float color)
        {
            return new(color);
        }
    }

    public enum AmbientColor { RED, YELLOW, GREEN, BLUE, PURPLE, WHITE, OFF }

    public static class AmbientExtensions
    {
        //public static ColorSliderPosition RED = 0;
        //public static ColorSliderPosition YELLOW = 0.17f;
        //public static ColorSliderPosition GREEN = 0.30f;
        //public static ColorSliderPosition BLUE = 0.65f;
        //public static ColorSliderPosition PURPLE = 0.80f;
        //public static ColorSliderPosition WHITE = new(0, 0, 1);
        //public static ColorSliderPosition BLACK = new(0, 0, 0);

        private static Dictionary<AmbientColor, ColorSliderPosition> entries = new()
        {
            { AmbientColor.RED, 0 },
            { AmbientColor.YELLOW, 0.17f },
            { AmbientColor.GREEN, 0.30f },
            { AmbientColor.BLUE, 0.65f },
            { AmbientColor.PURPLE, 0.80f },
            { AmbientColor.WHITE, new(0, 0, 1) },
            { AmbientColor.OFF, new(0, 0, 0) },
        };

        public static ColorSliderPosition GetValue(this AmbientColor input)
        {
            return entries[input];
        }

        public static string GetName(this AmbientColor input)
        {
            return Enum.GetName(typeof(AmbientColor), input);
        }
    }
}
