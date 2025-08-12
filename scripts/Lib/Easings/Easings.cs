
using Godot;
using System;

namespace TnT.Easings
{
    public enum Ease
    {
        EaseInQuad = 0,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc,
        Linear,
        Spring,
        EaseInBounce,
        EaseOutBounce,
        EaseInOutBounce,
        EaseInBack,
        EaseOutBack,
        EaseInOutBack,
        EaseInElastic,
        EaseOutElastic,
        EaseInOutElastic,
    }
    /// <summary>
    /// Collection of easing functions for smooth interpolation.
    /// Each function smoothly interpolates a value between start and end,
    /// based on the normalized time parameter value, typically between 0 and 1.
    /// </summary>
    public static class Easings
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
        /// Returns the function associated to the easingFunction enum. Cache the returned delegate if used frequently.
        /// </summary>
        public static Function GetEasingFunction(Ease easingFunction)
        {
            switch (easingFunction)
            {
                case Ease.EaseInQuad: return EaseInQuad;
                case Ease.EaseOutQuad: return EaseOutQuad;
                case Ease.EaseInOutQuad: return EaseInOutQuad;
                case Ease.EaseInCubic: return EaseInCubic;
                case Ease.EaseOutCubic: return EaseOutCubic;
                case Ease.EaseInOutCubic: return EaseInOutCubic;
                case Ease.EaseInQuart: return EaseInQuart;
                case Ease.EaseOutQuart: return EaseOutQuart;
                case Ease.EaseInOutQuart: return EaseInOutQuart;
                case Ease.EaseInQuint: return EaseInQuint;
                case Ease.EaseOutQuint: return EaseOutQuint;
                case Ease.EaseInOutQuint: return EaseInOutQuint;
                case Ease.EaseInSine: return EaseInSine;
                case Ease.EaseOutSine: return EaseOutSine;
                case Ease.EaseInOutSine: return EaseInOutSine;
                case Ease.EaseInExpo: return EaseInExpo;
                case Ease.EaseOutExpo: return EaseOutExpo;
                case Ease.EaseInOutExpo: return EaseInOutExpo;
                case Ease.EaseInCirc: return EaseInCirc;
                case Ease.EaseOutCirc: return EaseOutCirc;
                case Ease.EaseInOutCirc: return EaseInOutCirc;
                case Ease.Linear: return Linear;
                case Ease.Spring: return Spring;
                case Ease.EaseInBounce: return EaseInBounce;
                case Ease.EaseOutBounce: return EaseOutBounce;
                case Ease.EaseInOutBounce: return EaseInOutBounce;
                case Ease.EaseInBack: return EaseInBack;
                case Ease.EaseOutBack: return EaseOutBack;
                case Ease.EaseInOutBack: return EaseInOutBack;
                case Ease.EaseInElastic: return EaseInElastic;
                case Ease.EaseOutElastic: return EaseOutElastic;
                case Ease.EaseInOutElastic: return EaseInOutElastic;
                default: return null;
            }
        }

        /// <summary>
        /// Linear interpolation between start and end values.
        /// Returns a value proportional to normalized time without easing.
        /// </summary>
        /// <param name="start">Start value of the interpolation.</param>
        /// <param name="end">End value of the interpolation.</param>
        /// <param name="value">Normalized time parameter, generally 0 to 1.</param>
        /// <returns>Interpolated value between start and end.</returns>
        public static float Linear(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, value);
        }

        /// <summary>
        /// Spring easing function.
        /// Simulates a spring oscillation with damping that gradually settles to the end value.
        /// Produces a natural bouncy effect without overshoot constants.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        /// <returns>Eased value representing spring effect.</returns>
        public static float Spring(float start, float end, float value)
        {
            float t = Mathf.Clamp(value, 0f, 1f);
            float change = 0.3f * t;
            float oscillation = Mathf.Cos(t * Mathf.Pi * 4.5f);
            float decay = Mathf.Exp(-change);
            return Mathf.Lerp(start, end, 1 - oscillation * decay);
        }

        /// <summary>
        /// Quadratic easing in - accelerating from zero velocity.
        /// Starts slow and accelerates towards the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInQuad(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return delta * value * value + start;
        }

        /// <summary>
        /// Quadratic easing out - decelerating to zero velocity.
        /// Starts fast and slows down towards the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutQuad(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return -delta * value * (value - 2) + start;
        }

        /// <summary>
        /// Quadratic easing in/out - acceleration until halfway, then deceleration.
        /// Combines ease-in and ease-out for smooth midpoint.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutQuad(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * value * value + start;
            value--;
            return -delta * 0.5f * (value * (value - 2) - 1) + start;
        }

        /// <summary>
        /// Cubic easing in - accelerating from zero velocity.
        /// Starts slow and accelerates faster than quadratic easing.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInCubic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return delta * value * value * value + start;
        }

        /// <summary>
        /// Cubic easing out - decelerating to zero velocity.
        /// Starts fast and slows down smoothly near the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutCubic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value--;
            return delta * (value * value * value + 1) + start;
        }

        /// <summary>
        /// Cubic easing in/out - acceleration until halfway, then deceleration.
        /// Smooth transition between ease-in and ease-out cubic functions.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutCubic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * value * value * value + start;
            value -= 2;
            return delta * 0.5f * (value * value * value + 2) + start;
        }

        /// <summary>
        /// Quartic easing in - accelerating from zero velocity.
        /// Starts even slower than cubic and accelerates rapidly.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInQuart(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return delta * value * value * value * value + start;
        }

        /// <summary>
        /// Quartic easing out - decelerating to zero velocity.
        /// Starts fast and slows sharply at the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutQuart(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value--;
            return -delta * (value * value * value * value - 1) + start;
        }

        /// <summary>
        /// Quartic easing in/out - acceleration until halfway, then deceleration.
        /// Produces a smooth ease in and out with sharper transitions.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutQuart(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * value * value * value * value + start;
            value -= 2;
            return -delta * 0.5f * (value * value * value * value - 2) + start;
        }

        /// <summary>
        /// Quintic easing in - accelerating from zero velocity.
        /// Even more pronounced acceleration than quartic easing.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInQuint(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return delta * value * value * value * value * value + start;
        }

        /// <summary>
        /// Quintic easing out - decelerating to zero velocity.
        /// Starts very fast and slows down sharply near the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutQuint(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value--;
            return delta * (value * value * value * value * value + 1) + start;
        }

        /// <summary>
        /// Quintic easing in/out - acceleration until halfway, then deceleration.
        /// Combines sharp acceleration and deceleration for a smooth curve.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutQuint(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * value * value * value * value * value + start;
            value -= 2;
            return delta * 0.5f * (value * value * value * value * value + 2) + start;
        }

        /// <summary>
        /// Sine easing in - accelerating from zero velocity using sine wave.
        /// Smooth sinusoidal acceleration from start.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInSine(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return -delta * Mathf.Cos(value * HALF_PI) + delta + start;
        }

        /// <summary>
        /// Sine easing out - decelerating to zero velocity using sine wave.
        /// Smooth sinusoidal deceleration to end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutSine(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return delta * Mathf.Sin(value * HALF_PI) + start;
        }

        /// <summary>
        /// Sine easing in/out - acceleration then deceleration using sine wave.
        /// Creates a smooth sinusoidal ease in and out effect.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutSine(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return -delta * 0.5f * (Mathf.Cos(PI * value) - 1) + start;
        }

        /// <summary>
        /// Exponential easing in/out.
        /// Accelerates exponentially until halfway then decelerates exponentially.
        /// Uses base 2 exponentials for smooth speed change.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutExpo(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            if (value == 0) return start;
            if (value == 1) return end;

            float delta = end - start;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * (float)Math.Pow(2, 10 * (value - 1)) + start;
            value--;
            return delta * 0.5f * (-(float)Math.Pow(2, -10 * value) + 2) + start;
        }

        /// <summary>
        /// Circular easing in - accelerating from zero velocity following a circular path.
        /// Starts slow and accelerates with a quarter circle arc.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInCirc(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            return -delta * (Mathf.Sqrt(1 - value * value) - 1) + start;
        }

        /// <summary>
        /// Circular easing out - decelerating to zero velocity following a circular path.
        /// Starts fast and slows near the end with a circular arc.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutCirc(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            value--;
            return delta * Mathf.Sqrt(1 - value * value) + start;
        }

        /// <summary>
        /// Back easing in - overshooting cubic easing (backwards).
        /// Starts by moving slightly backward before accelerating forward.
        /// Creates an overshoot effect by going beyond the start slightly.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInBack(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            float c = BACK_OVERSHOOT;
            return delta * value * value * ((c + 1) * value - c) + start;
        }

        /// <summary>
        /// Back easing out - overshooting cubic easing (forwards).
        /// Moves past the target then comes back to settle.
        /// Creates an overshoot effect going beyond the end and returning.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutBack(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            float c = BACK_OVERSHOOT;
            value--;
            return delta * (value * value * ((c + 1) * value + c) + 1) + start;
        }

        /// <summary>
        /// Back easing in/out - overshooting cubic easing with easing in and out.
        /// Combines back easing in and out with a multiplier for smooth overshoot.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutBack(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            float c = BACK_OVERSHOOT * BACK_IN_OUT_MULTIPLIER;
            value *= 2;
            if (value < 1)
                return delta * 0.5f * (value * value * ((c + 1) * value - c)) + start;
            value -= 2;
            return delta * 0.5f * (value * value * ((c + 1) * value + c) + 2) + start;
        }

        /// <summary>
        /// Elastic easing in - exponentially decaying sine wave starting slow and oscillating.
        /// Produces a spring effect that starts slowly and oscillates increasing frequency.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInElastic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            if (value == 0) return start;
            if (value == 1) return end;

            float p = ELASTIC_PERIOD_SCALE;
            float s = p / ELASTIC_AMPLITUDE_MULTIPLIER;
            value -= 1;
            return -(delta * (float)Math.Pow(2, 10 * value) * Mathf.Sin((value - s) * TWO_PI / p)) + start;
        }

        /// <summary>
        /// Elastic easing out - exponentially decaying sine wave ending slow and oscillating.
        /// Produces a spring effect that overshoots and settles to the end value.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseOutElastic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            if (value == 0) return start;
            if (value == 1) return end;

            float p = ELASTIC_PERIOD_SCALE;
            float s = p / ELASTIC_AMPLITUDE_MULTIPLIER;
            return delta * (float)Math.Pow(2, -10 * value) * Mathf.Sin((value - s) * TWO_PI / p) + delta + start;
        }

        /// <summary>
        /// Elastic easing in/out - combination of exponential decay and oscillation.
        /// Starts slow, oscillates mid animation, then settles smoothly.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time.</param>
        public static float EaseInOutElastic(float start, float end, float value)
        {
            value = Mathf.Clamp(value, 0f, 1f);
            float delta = end - start;
            if (value == 0) return start;
            if (value == 1) return end;

            float p = ELASTIC_PERIOD_SCALE * 1.5f;
            float s = p / ELASTIC_AMPLITUDE_MULTIPLIER;
            value *= 2;

            if (value < 1)
            {
                value -= 1;
                return -0.5f * (delta * (float)Math.Pow(2, 10 * value) * Mathf.Sin((value - s) * TWO_PI / p)) + start;
            }
            value -= 1;
            return delta * 0.5f * (float)Math.Pow(2, -10 * value) * Mathf.Sin((value - s) * TWO_PI / p) + delta * 0.5f + start;
        }        /// <summary>
        /// Exponential easing in function.
        /// Starts slowly and accelerates rapidly towards the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseInExpo(float start, float end, float value)
        {
            return value == 0f ? start : (end - start) * Mathf.Pow(2f, 10f * (value - 1f)) + start;
        }

        /// <summary>
        /// Exponential easing out function.
        /// Starts quickly and slows down exponentially towards the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseOutExpo(float start, float end, float value)
        {
            return value == 1f ? end : (end - start) * (-Mathf.Pow(2f, -10f * value) + 1f) + start;
        }

        /// <summary>
        /// Circular easing in/out function.
        /// Starts slow, speeds up in the middle, and slows down at the end.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseInOutCirc(float start, float end, float value)
        {
            value *= 2f;
            float delta = end - start;
            if (value < 1f)
            {
                return -delta / 2f * (Mathf.Sqrt(1f - value * value) - 1f) + start;
            }
            value -= 2f;
            return delta / 2f * (Mathf.Sqrt(1f - value * value) + 1f) + start;
        }

        /// <summary>
        /// Bounce easing in function.
        /// Starts with a bounce effect and accelerates towards the target.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseInBounce(float start, float end, float value)
        {
            float delta = end - start;
            return delta - EaseOutBounce(0f, delta, 1f - value) + start;
        }

        /// <summary>
        /// Bounce easing out function.
        /// Ends with a bounce effect decaying over time.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseOutBounce(float start, float end, float value)
        {
            float delta = end - start;
            if (value < 1f / BOUNCE_SCALE)
            {
                return delta * (BOUNCE_MULTIPLIER * value * value) + start;
            }
            else if (value < 2f / BOUNCE_SCALE)
            {
                value -= BOUNCE_OFFSET_1 / BOUNCE_SCALE;
                return delta * (BOUNCE_MULTIPLIER * value * value + BOUNCE_BRANCH_ADD1) + start;
            }
            else if (value < 2.5f / BOUNCE_SCALE)
            {
                value -= BOUNCE_OFFSET_2 / BOUNCE_SCALE;
                return delta * (BOUNCE_MULTIPLIER * value * value + BOUNCE_BRANCH_ADD2) + start;
            }
            else
            {
                value -= BOUNCE_OFFSET_3 / BOUNCE_SCALE;
                return delta * (BOUNCE_MULTIPLIER * value * value + BOUNCE_BRANCH_ADD3) + start;
            }
        }

        /// <summary>
        /// Bounce easing in/out function.
        /// Combines bounce easing in and out for a smooth bounce effect at both ends.
        /// </summary>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <param name="value">Normalized time (0 to 1).</param>
        /// <returns>Eased value.</returns>
        public static float EaseInOutBounce(float start, float end, float value)
        {
            float delta = end - start;
            if (value < 0.5f)
            {
                return EaseInBounce(0f, delta, value * 2f) * 0.5f + start;
            }
            else
            {
                return EaseOutBounce(0f, delta, value * 2f - 1f) * 0.5f + delta * 0.5f + start;
            }
        }
    }
}
