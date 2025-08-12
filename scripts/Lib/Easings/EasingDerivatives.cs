
using Godot;
using System;

namespace TnT.Easings
{
    /// <summary>
    /// Collection of derivative easing functions representing speed of easing.
    /// These functions calculate the rate of change (derivative) of the corresponding easing functions.
    /// </summary>
    public static class EasingDerivatives
    {
        // Common math constants
        private const float PI = Mathf.Pi;
        private const float HALF_PI = Mathf.Pi * 0.5f;
        private const float TWO_PI = Mathf.Pi * 2f;
        private const float NATURAL_LOG_OF_2 = 0.6931471805599453f; // ln(2)

        // Back easing constants for overshoot control
        private const float BACK_OVERSHOOT = 1.70158f;
        private const float BACK_IN_OUT_MULTIPLIER = 1.525f;

        // Bounce easing constants for piecewise segments
        private const float BOUNCE_SCALE = 2.75f;
        private const float BOUNCE_MULTIPLIER = 121f / 16f; // 7.5625f
        private const float BOUNCE_OFFSET_1 = 1.5f;
        private const float BOUNCE_OFFSET_2 = 2.25f;
        private const float BOUNCE_OFFSET_3 = 2.625f;
        private const float BOUNCE_BRANCH_ADD1 = 0.75f;
        private const float BOUNCE_BRANCH_ADD2 = 0.9375f;  // 15/16
        private const float BOUNCE_BRANCH_ADD3 = 0.984375f; // 63/64

        // Elastic easing constants
        private const float ELASTIC_PERIOD_SCALE = 0.3f;
        private const float ELASTIC_AMPLITUDE_MULTIPLIER = 2.75f;

        private const float DEFAULT_DURATION = 1f;

        //
        // Delegate type
        //
        public delegate float Function(float start, float end, float value);

        /// <summary>
        /// Returns the derivative (speed) function associated with an easing type.
        /// </summary>
        public static Function GetEasingFunctionDerivative(Ease easingFunction)
        {
            switch (easingFunction)
            {
                case Ease.EaseInQuad: return EaseInQuadD;
                case Ease.EaseOutQuad: return EaseOutQuadD;
                case Ease.EaseInOutQuad: return EaseInOutQuadD;
                case Ease.EaseInCubic: return EaseInCubicD;
                case Ease.EaseOutCubic: return EaseOutCubicD;
                case Ease.EaseInOutCubic: return EaseInOutCubicD;
                case Ease.EaseInQuart: return EaseInQuartD;
                case Ease.EaseOutQuart: return EaseOutQuartD;
                case Ease.EaseInOutQuart: return EaseInOutQuartD;
                case Ease.EaseInQuint: return EaseInQuintD;
                case Ease.EaseOutQuint: return EaseOutQuintD;
                case Ease.EaseInOutQuint: return EaseInOutQuintD;
                case Ease.EaseInSine: return EaseInSineD;
                case Ease.EaseOutSine: return EaseOutSineD;
                case Ease.EaseInOutSine: return EaseInOutSineD;
                case Ease.EaseInExpo: return EaseInExpoD;
                case Ease.EaseOutExpo: return EaseOutExpoD;
                case Ease.EaseInOutExpo: return EaseInOutExpoD;
                case Ease.EaseInCirc: return EaseInCircD;
                case Ease.EaseOutCirc: return EaseOutCircD;
                case Ease.EaseInOutCirc: return EaseInOutCircD;
                case Ease.Linear: return LinearD;
                case Ease.Spring: return SpringD;
                case Ease.EaseInBounce: return EaseInBounceD;
                case Ease.EaseOutBounce: return EaseOutBounceD;
                case Ease.EaseInOutBounce: return EaseInOutBounceD;
                case Ease.EaseInBack: return EaseInBackD;
                case Ease.EaseOutBack: return EaseOutBackD;
                case Ease.EaseInOutBack: return EaseInOutBackD;
                case Ease.EaseInElastic: return EaseInElasticD;
                case Ease.EaseOutElastic: return EaseOutElasticD;
                case Ease.EaseInOutElastic: return EaseInOutElasticD;
                default: return null;
            }
        }

        /// <summary>
        /// Derivative of linear interpolation.
        /// Represents a constant rate of change.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float LinearD(float start, float end, float value)
        {
            return end - start;
        }

        /// <summary>
        /// Derivative of quadratic ease-in function.
        /// Rate of change starts slowly and accelerates.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInQuadD(float start, float end, float value)
        {
            float change = end - start;
            return change * 2f * value;
        }

        /// <summary>
        /// Derivative of quadratic ease-out function.
        /// Rate of change starts fast and decelerates to zero.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutQuadD(float start, float end, float value)
        {
            float change = end - start;
            return change * 2f * (1f - value);
        }

        /// <summary>
        /// Derivative of quadratic ease-in-out function.
        /// Starts slow, accelerates, then decelerates near the end.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutQuadD(float start, float end, float value)
        {
            float change = end - start;
            if (value < 0.5f)
            {
                return change * 4f * value;
            }
            else
            {
                return change * 4f * (1f - value);
            }
        }

        /// <summary>
        /// Derivative of cubic ease-in function.
        /// Starts slowly and accelerates following a cubic curve.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInCubicD(float start, float end, float value)
        {
            float change = end - start;
            return change * 3f * value * value;
        }

        /// <summary>
        /// Derivative of cubic ease-out function.
        /// Starts fast and decelerates to zero speed cubically.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutCubicD(float start, float end, float value)
        {
            float change = end - start;
            float t = value - 1f;
            return change * 3f * t * t * (-1f);
        }

        /// <summary>
        /// Derivative of cubic ease-in-out function.
        /// Smooth acceleration and deceleration with cubic speed profile.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutCubicD(float start, float end, float value)
        {
            float change = end - start;
            if (value < 0.5f)
            {
                return change * 6f * value * value;
            }
            else
            {
                float t = value - 1f;
                return change * 6f * t * t * (-1f);
            }
        }

        /// <summary>
        /// Derivative of quartic ease-in function.
        /// Starts very slow and accelerates sharply.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInQuartD(float start, float end, float value)
        {
            float change = end - start;
            return change * 4f * value * value * value;
        }

        /// <summary>
        /// Derivative of quartic ease-out function.
        /// Starts fast and slows down sharply near the end.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutQuartD(float start, float end, float value)
        {
            float change = end - start;
            float t = value - 1f;
            return change * -4f * t * t * t;
        }

        /// <summary>
        /// Derivative of quartic ease-in-out function.
        /// Combines sharp acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutQuartD(float start, float end, float value)
        {
            float change = end - start;
            if (value < 0.5f)
            {
                return change * 16f * value * value * value;
            }
            else
            {
                float t = value - 1f;
                return change * -16f * t * t * t;
            }
        }

        /// <summary>
        /// Derivative of quintic ease-in function.
        /// Starts extremely slow and accelerates steeply.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInQuintD(float start, float end, float value)
        {
            float change = end - start;
            return change * 5f * value * value * value * value;
        }

        /// <summary>
        /// Derivative of quintic ease-out function.
        /// Starts fast and decelerates steeply near the end.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutQuintD(float start, float end, float value)
        {
            float change = end - start;
            float t = value - 1f;
            return change * -5f * t * t * t * t;
        }

        /// <summary>
        /// Derivative of quintic ease-in-out function.
        /// Combines steep acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutQuintD(float start, float end, float value)
        {
            float change = end - start;
            if (value < 0.5f)
            {
                return change * 40f * value * value * value * value;
            }
            else
            {
                float t = value - 1f;
                return change * -40f * t * t * t * t;
            }
        }

        /// <summary>
        /// Derivative of sine ease-in function.
        /// Starts slowly with sine curve acceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInSineD(float start, float end, float value)
        {
            float change = end - start;
            return change * Mathf.Cos(value * HALF_PI);
        }

        /// <summary>
        /// Derivative of sine ease-out function.
        /// Starts fast and decelerates with sine curve.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutSineD(float start, float end, float value)
        {
            float change = end - start;
            return change * -Mathf.Cos(value * HALF_PI);
        }

        /// <summary>
        /// Derivative of sine ease-in-out function.
        /// Combines slow start and end with acceleration in middle.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutSineD(float start, float end, float value)
        {
            float change = end - start;
            return change * Mathf.Sin(value * Mathf.Pi);
        }

        /// <summary>
        /// Derivative of exponential ease-in function.
        /// Accelerates exponentially from zero speed.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInExpoD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f)
                return 0f;
            return change * (float)(Math.Pow(2, 10 * (value - 1)) * NATURAL_LOG_OF_2 * 10);
        }

        /// <summary>
        /// Derivative of exponential ease-out function.
        /// Decelerates exponentially to zero speed.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutExpoD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 1f)
                return 0f;
            return change * (float)(-Math.Pow(2, -10 * value) * NATURAL_LOG_OF_2 * 10);
        }

        /// <summary>
        /// Derivative of exponential ease-in-out function.
        /// Combines exponential acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutExpoD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f || value == 1f)
                return 0f;
            if (value < 0.5f)
            {
                return change * (float)(Math.Pow(2, 20 * value - 10) * NATURAL_LOG_OF_2 * 20);
            }
            else
            {
                return change * (float)(-Math.Pow(2, -20 * value + 10) * NATURAL_LOG_OF_2 * 20);
            }
        }

        /// <summary>
        /// Derivative of circular ease-in function.
        /// Starts slowly following circular path acceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInCircD(float start, float end, float value)
        {
            float change = end - start;
            return change * value / (float)Math.Sqrt(1f - value * value);
        }

        /// <summary>
        /// Derivative of circular ease-out function.
        /// Starts fast and decelerates along circular path.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutCircD(float start, float end, float value)
        {
            float change = end - start;
            float t = value - 1f;
            return change * -t / (float)Math.Sqrt(1f - t * t);
        }

        /// <summary>
        /// Derivative of circular ease-in-out function.
        /// Combines circular acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutCircD(float start, float end, float value)
        {
            float change = end - start;
            if (value < 0.5f)
            {
                return change * 2f * value / (float)Math.Sqrt(1f - 4f * value * value);
            }
            else
            {
                float t = 2f * value - 2f;
                return change * -t / (float)Math.Sqrt(1f - t * t);
            }
        }

        /// <summary>
        /// Derivative of back ease-in function.
        /// Produces overshoot acceleration effect at start.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInBackD(float start, float end, float value)
        {
            float change = end - start;
            float c1 = BACK_OVERSHOOT;
            return change * (3f + c1) * value * value - change * c1 * value;
        }

        /// <summary>
        /// Derivative of back ease-out function.
        /// Produces overshoot deceleration effect at end.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutBackD(float start, float end, float value)
        {
            float change = end - start;
            float c1 = BACK_OVERSHOOT;
            float t = value - 1f;
            return change * (3f + c1) * t * t + change * c1 * t;
        }

        /// <summary>
        /// Derivative of back ease-in-out function.
        /// Combines overshoot acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutBackD(float start, float end, float value)
        {
            float change = end - start;
            float c1 = BACK_OVERSHOOT * BACK_IN_OUT_MULTIPLIER;
            if (value < 0.5f)
            {
                return change * 2f * value * ((3f + c1) * 2f * value - c1);
            }
            else
            {
                float t = 2f * value - 2f;
                return change * 2f * t * ((3f + c1) * t + c1);
            }
        }

        /// <summary>
        /// Derivative of elastic ease-in function.
        /// Produces spring-like oscillations with increasing speed.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInElasticD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f || value == 1f) return 0f;

            float p = ELASTIC_PERIOD_SCALE;
            float s = p / 4f;
            float t = value - 1f;
            float a = (float)Math.Pow(2, 10 * t);
            float b = (float)(Math.Sin((t - s) * TWO_PI / p) * TWO_PI / p);

            return change * a * (b - (10f * NATURAL_LOG_OF_2) * (float)Math.Sin((t - s) * TWO_PI / p));
        }

        /// <summary>
        /// Derivative of elastic ease-out function.
        /// Produces spring-like oscillations with decreasing speed.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseOutElasticD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f || value == 1f) return 0f;

            float p = ELASTIC_PERIOD_SCALE;
            float s = p / 4f;
            float a = (float)Math.Pow(2, -10 * value);
            float b = (float)(Math.Sin((value - s) * TWO_PI / p) * TWO_PI / p);

            return change * a * ((-b) - (10f * NATURAL_LOG_OF_2) * (float)Math.Sin((value - s) * TWO_PI / p));
        }

        /// <summary>
        /// Derivative of elastic ease-in-out function.
        /// Combines spring oscillations with acceleration and deceleration.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float EaseInOutElasticD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f || value == 1f) return 0f;

            float p = ELASTIC_PERIOD_SCALE * 1.5f;
            float s = p / 4f;
            if (value < 0.5f)
            {
                float t = 2f * value - 1f;
                float a = (float)Math.Pow(2, 10 * t);
                float b = (float)(Math.Sin((t - s) * TWO_PI / p) * TWO_PI / p);
                return change * 0.5f * a * (b - (10f * NATURAL_LOG_OF_2) * (float)Math.Sin((t - s) * TWO_PI / p));
            }
            else
            {
                float t = 2f * value - 1f;
                float a = (float)Math.Pow(2, -10 * t);
                float b = (float)(Math.Sin((t - s) * TWO_PI / p) * TWO_PI / p);
                return change * 0.5f * a * ((-b) - (10f * NATURAL_LOG_OF_2) * (float)Math.Sin((t - s) * TWO_PI / p));
            }
        }

        /// <summary>
        /// Derivative of spring function.
        /// Represents the instantaneous velocity of spring easing effect.
        /// </summary>
        /// <param name="start">Start value of interpolation range.</param>
        /// <param name="end">End value of interpolation range.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value representing speed.</returns>
        public static float SpringD(float start, float end, float value)
        {
            float change = end - start;
            if (value == 0f || value == 1f)
                return 0f;

            float a = 4.5f * Mathf.Pi;
            float b = -0.3f * value;
            return change * (float)(Math.Exp(b) * (a * Math.Sin(a * value) - 0.3f * Math.Cos(a * value)));
        }

        /// <summary>
        /// Derivative of bounce easing in function.
        /// Represents the rate of change (speed) at given normalized time.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value (speed).</returns>
        public static float EaseInBounceD(float start, float end, float value)
        {
            float delta = end - start;
            return -EaseOutBounceD(0f, delta, 1f - value);
        }

        /// <summary>
        /// Derivative of bounce easing out function.
        /// Represents the rate of change (speed) at given normalized time.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value (speed).</returns>
        public static float EaseOutBounceD(float start, float end, float value)
        {
            float delta = end - start;
            if (value < 1f / BOUNCE_SCALE)
            {
                return delta * (2f * BOUNCE_MULTIPLIER * value);
            }
            else if (value < 2f / BOUNCE_SCALE)
            {
                value -= BOUNCE_OFFSET_1 / BOUNCE_SCALE;
                return delta * (2f * BOUNCE_MULTIPLIER * value);
            }
            else if (value < 2.5f / BOUNCE_SCALE)
            {
                value -= BOUNCE_OFFSET_2 / BOUNCE_SCALE;
                return delta * (2f * BOUNCE_MULTIPLIER * value);
            }
            else
            {
                value -= BOUNCE_OFFSET_3 / BOUNCE_SCALE;
                return delta * (2f * BOUNCE_MULTIPLIER * value);
            }
        }

        /// <summary>
        /// Derivative of bounce easing in/out function.
        /// Represents the rate of change (speed) at given normalized time.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Derivative value (speed).</returns>
        public static float EaseInOutBounceD(float start, float end, float value)
        {
            float delta = end - start;
            if (value < 0.5f)
            {
                return EaseInBounceD(0f, delta, value * 2f) * 0.5f;
            }
            else
            {
                return EaseOutBounceD(0f, delta, value * 2f - 1f) * 0.5f;
            }
        }
    }
}
