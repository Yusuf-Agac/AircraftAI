using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Curves
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
    
    private const float NaturalLOGOf2 = 0.693147181f;
    
    public static float Linear(float value)
    {
        return Mathf.Lerp(0, 1, value);
    }

    public static float Spring(float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return 0 + (1 - 0) * value;
    }

    public static float EaseInQuad(float value)
    {
        return 1 * value * value + 0;
    }

    public static float EaseOutQuad(float value)
    {
        return -1 * value * (value - 2) + 0;
    }

    public static float EaseInOutQuad(float value)
    {
        value /= .5f;
        if (value < 1) return 1 * 0.5f * value * value + 0;
        value--;
        return -1 * 0.5f * (value * (value - 2) - 1) + 0;
    }

    public static float EaseInCubic(float value)
    {
        return 1 * value * value * value + 0;
    }

    public static float EaseOutCubic(float value)
    {
        value--;
        return 1 * (value * value * value + 1) + 0;
    }

    public static float EaseInOutCubic(float value)
    {
        value /= .5f;
        if (value < 1) return 1 * 0.5f * value * value * value + 0;
        value -= 2;
        return 1 * 0.5f * (value * value * value + 2) + 0;
    }

    public static float EaseInQuart(float value)
    {
        return 1 * value * value * value * value + 0;
    }

    public static float EaseOutQuart(float value)
    {
        value--;
        return -1 * (value * value * value * value - 1) + 0;
    }

    public static float EaseInOutQuart(float value)
    {
        value /= .5f;
        if (value < 1) return 1 * 0.5f * value * value * value * value + 0;
        value -= 2;
        return -1 * 0.5f * (value * value * value * value - 2) + 0;
    }

    public static float EaseInQuint(float value)
    {
        return 1 * value * value * value * value * value + 0;
    }

    public static float EaseOutQuint(float value)
    {
        value--;
        return 1 * (value * value * value * value * value + 1) + 0;
    }

    public static float EaseInOutQuint(float value)
    {
        value /= .5f;
        if (value < 1) return 1 * 0.5f * value * value * value * value * value + 0;
        value -= 2;
        return 1 * 0.5f * (value * value * value * value * value + 2) + 0;
    }

    public static float EaseInSine(float value)
    {
        return -1 * Mathf.Cos(value * (Mathf.PI * 0.5f)) + 1 + 0;
    }

    public static float EaseOutSine(float value)
    {
        return 1 * Mathf.Sin(value * (Mathf.PI * 0.5f)) + 0;
    }

    public static float EaseInOutSine(float value)
    {
        return -1 * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + 0;
    }

    public static float EaseInExpo(float value)
    {
        return 1 * Mathf.Pow(2, 10 * (value - 1)) + 0;
    }

    public static float EaseOutExpo(float value)
    {
        return 1 * (-Mathf.Pow(2, -10 * value) + 1) + 0;
    }

    public static float EaseInOutExpo(float value)
    {
        value /= .5f;
        if (value < 1) return 1 * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + 0;
        value--;
        return 1 * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + 0;
    }

    public static float EaseInCirc(float value)
    {
        return -1 * (Mathf.Sqrt(1 - value * value) - 1) + 0;
    }

    public static float EaseOutCirc(float value)
    {
        value--;
        return 1 * Mathf.Sqrt(1 - value * value) + 0;
    }

    public static float EaseInOutCirc(float value)
    {
        value /= .5f;
        if (value < 1) return -1 * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + 0;
        value -= 2;
        return 1 * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + 0;
    }

    public static float EaseInBounce(float value)
    {
        float d = 1f;
        return 1 - EaseOutBounce(d - value) + 0;
    }

    public static float EaseOutBounce(float value)
    {
        value /= 1f;
        if (value < (1 / 2.75f))
        {
            return 1 * (7.5625f * value * value) + 0;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return 1 * (7.5625f * (value) * value + .75f) + 0;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return 1 * (7.5625f * (value) * value + .9375f) + 0;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return 1 * (7.5625f * (value) * value + .984375f) + 0;
        }
    }

    public static float EaseInOutBounce(float value)
    {
        float d = 1f;
        if (value < d * 0.5f) return EaseInBounce(value * 2) * 0.5f + 0;
        else return EaseOutBounce(value * 2 - d) * 0.5f + 1 * 0.5f + 0;
    }

    public static float EaseInBack(float value)
    {
        value /= 1;
        float s = 1.70158f;
        return 1 * (value) * value * ((s + 1) * value - s) + 0;
    }

    public static float EaseOutBack(float value)
    {
        float s = 1.70158f;
        value = (value) - 1;
        return 1 * ((value) * value * ((s + 1) * value + s) + 1) + 0;
    }

    public static float EaseInOutBack(float value)
    {
        float s = 1.70158f;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return 1 * 0.5f * (value * value * (((s) + 1) * value - s)) + 0;
        }

        value -= 2;
        s *= (1.525f);
        return 1 * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + 0;
    }

    public static float EaseInElastic(float value)
    {

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return 0;

        if ((value /= d) == 1) return 0 + 1;

        if (a == 0f || a < Mathf.Abs(1))
        {
            a = 1;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + 0;
    }

    public static float EaseOutElastic(float value)
    {

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return 0;

        if ((value /= d) == 1) return 0 + 1;

        if (a == 0f || a < Mathf.Abs(1))
        {
            a = 1;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + 1 + 0);
    }

    public static float EaseInOutElastic(float value)
    {

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (value == 0) return 0;

        if ((value /= d * 0.5f) == 2) return 0 + 1;

        if (a == 0f || a < Mathf.Abs(1))
        {
            a = 1;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + 0;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + 1 + 0;
    }

    public static float LinearD(float value)
    {
        return 1 - 0;
    }

    public static float EaseInQuadD(float value)
    {
        return 2f * (1 - 0) * value;
    }

    public static float EaseOutQuadD(float value)
    {
        return -1 * value - 1 * (value - 2);
    }

    public static float EaseInOutQuadD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return 1 * value;
        }

        value--;

        return 1 * (1 - value);
    }

    public static float EaseInCubicD(float value)
    {
        return 3f * (1 - 0) * value * value;
    }

    public static float EaseOutCubicD(float value)
    {
        value--;
        return 3f * 1 * value * value;
    }

    public static float EaseInOutCubicD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return (3f / 2f) * 1 * value * value;
        }

        value -= 2;

        return (3f / 2f) * 1 * value * value;
    }

    public static float EaseInQuartD(float value)
    {
        return 4f * (1 - 0) * value * value * value;
    }

    public static float EaseOutQuartD(float value)
    {
        value--;
        return -4f * 1 * value * value * value;
    }

    public static float EaseInOutQuartD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return 2f * 1 * value * value * value;
        }

        value -= 2;

        return -2f * 1 * value * value * value;
    }

    public static float EaseInQuintD(float value)
    {
        return 5f * (1 - 0) * value * value * value * value;
    }

    public static float EaseOutQuintD(float value)
    {
        value--;
        return 5f * 1 * value * value * value * value;
    }

    public static float EaseInOutQuintD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return (5f / 2f) * 1 * value * value * value * value;
        }

        value -= 2;

        return (5f / 2f) * 1 * value * value * value * value;
    }

    public static float EaseInSineD(float value)
    {
        return (1 - 0) * 0.5f * Mathf.PI * Mathf.Sin(0.5f * Mathf.PI * value);
    }

    public static float EaseOutSineD(float value)
    {
        return (Mathf.PI * 0.5f) * 1 * Mathf.Cos(value * (Mathf.PI * 0.5f));
    }

    public static float EaseInOutSineD(float value)
    {
        return 1 * 0.5f * Mathf.PI * Mathf.Sin(Mathf.PI * value);
    }

    public static float EaseInExpoD(float value)
    {
        return (10f * NaturalLOGOf2 * (1 - 0) * Mathf.Pow(2f, 10f * (value - 1)));
    }

    public static float EaseOutExpoD(float value)
    {
        return 5f * NaturalLOGOf2 * 1 * Mathf.Pow(2f, 1f - 10f * value);
    }

    public static float EaseInOutExpoD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return 5f * NaturalLOGOf2 * 1 * Mathf.Pow(2f, 10f * (value - 1));
        }

        value--;

        return (5f * NaturalLOGOf2 * 1) / (Mathf.Pow(2f, 10f * value));
    }

    public static float EaseInCircD(float value)
    {
        return ((1 - 0) * value) / Mathf.Sqrt(1f - value * value);
    }

    public static float EaseOutCircD(float value)
    {
        value--;
        return (-1 * value) / Mathf.Sqrt(1f - value * value);
    }

    public static float EaseInOutCircD(float value)
    {
        value /= .5f;

        if (value < 1)
        {
            return (1 * value) / (2f * Mathf.Sqrt(1f - value * value));
        }

        value -= 2;

        return (-1 * value) / (2f * Mathf.Sqrt(1f - value * value));
    }

    public static float EaseInBounceD(float value)
    {
        float d = 1f;

        return EaseOutBounceD(d - value);
    }

    public static float EaseOutBounceD(float value)
    {
        value /= 1f;

        if (value < (1 / 2.75f))
        {
            return 2f * 1 * 7.5625f * value;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return 2f * 1 * 7.5625f * value;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return 2f * 1 * 7.5625f * value;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return 2f * 1 * 7.5625f * value;
        }
    }

    public static float EaseInOutBounceD(float value)
    {
        float d = 1f;

        if (value < d * 0.5f)
        {
            return EaseInBounceD(value * 2) * 0.5f;
        }
        else
        {
            return EaseOutBounceD(value * 2 - d) * 0.5f;
        }
    }

    public static float EaseInBackD(float value)
    {
        float s = 1.70158f;

        return 3f * (s + 1f) * (1 - 0) * value * value - 2f * s * (1 - 0) * value;
    }

    public static float EaseOutBackD(float value)
    {
        float s = 1.70158f;
        value = (value) - 1;

        return 1 * ((s + 1f) * value * value + 2f * value * ((s + 1f) * value + s));
    }

    public static float EaseInOutBackD(float value)
    {
        float s = 1.70158f;
        value /= .5f;

        if ((value) < 1)
        {
            s *= (1.525f);
            return 0.5f * 1 * (s + 1) * value * value + 1 * value * ((s + 1f) * value - s);
        }

        value -= 2;
        s *= (1.525f);
        return 0.5f * 1 * ((s + 1) * value * value + 2f * value * ((s + 1f) * value + s));
    }

    public static float EaseInElasticD(float value)
    {
        return EaseOutElasticD(1f - value);
    }

    public static float EaseOutElasticD(float value)
    {

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (a == 0f || a < Mathf.Abs(1))
        {
            a = 1;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        return (a * Mathf.PI * d * Mathf.Pow(2f, 1f - 10f * value) *
                Mathf.Cos((2f * Mathf.PI * (d * value - s)) / p)) / p - 5f * NaturalLOGOf2 * a *
            Mathf.Pow(2f, 1f - 10f * value) * Mathf.Sin((2f * Mathf.PI * (d * value - s)) / p);
    }

    public static float EaseInOutElasticD(float value)
    {

        float d = 1f;
        float p = d * .3f;
        float s;
        float a = 0;

        if (a == 0f || a < Mathf.Abs(1))
        {
            a = 1;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(1 / a);
        }

        if (value < 1)
        {
            value -= 1;

            return -5f * NaturalLOGOf2 * a * Mathf.Pow(2f, 10f * value) * Mathf.Sin(2 * Mathf.PI * (d * value - 2f) / p) -
                   a * Mathf.PI * d * Mathf.Pow(2f, 10f * value) * Mathf.Cos(2 * Mathf.PI * (d * value - s) / p) / p;
        }

        value -= 1;

        return a * Mathf.PI * d * Mathf.Cos(2f * Mathf.PI * (d * value - s) / p) / (p * Mathf.Pow(2f, 10f * value)) -
               5f * NaturalLOGOf2 * a * Mathf.Sin(2f * Mathf.PI * (d * value - s) / p) / (Mathf.Pow(2f, 10f * value));
    }

    public static float SpringD(float value)
    {
        value = Mathf.Clamp01(value);
        return 1 * (6f * (1f - value) / 5f + 1f) * (-2.2f * Mathf.Pow(1f - value, 1.2f) *
                   Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + Mathf.Pow(1f - value, 2.2f) *
                   (Mathf.PI * (2.5f * value * value * value + 0.2f) + 7.5f * Mathf.PI * value * value * value) *
                   Mathf.Cos(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + 1f) -
               6f * 1 * (Mathf.Pow(1 - value, 2.2f) * Mathf.Sin(Mathf.PI * value * (2.5f * value * value * value + 0.2f)) + value
                   / 5f);

    }
    
    public delegate float Function(float v);

    private static readonly Dictionary<Ease, Function> EasingFunctionMap = new()
    {
        { Ease.EaseInQuad, EaseInQuad },
        { Ease.EaseOutQuad, EaseOutQuad },
        { Ease.EaseInOutQuad, EaseInOutQuad },
        { Ease.EaseInCubic, EaseInCubic },
        { Ease.EaseOutCubic, EaseOutCubic },
        { Ease.EaseInOutCubic, EaseInOutCubic },
        { Ease.EaseInQuart, EaseInQuart },
        { Ease.EaseOutQuart, EaseOutQuart },
        { Ease.EaseInOutQuart, EaseInOutQuart },
        { Ease.EaseInQuint, EaseInQuint },
        { Ease.EaseOutQuint, EaseOutQuint },
        { Ease.EaseInOutQuint, EaseInOutQuint },
        { Ease.EaseInSine, EaseInSine },
        { Ease.EaseOutSine, EaseOutSine },
        { Ease.EaseInOutSine, EaseInOutSine },
        { Ease.EaseInExpo, EaseInExpo },
        { Ease.EaseOutExpo, EaseOutExpo },
        { Ease.EaseInOutExpo, EaseInOutExpo },
        { Ease.EaseInCirc, EaseInCirc },
        { Ease.EaseOutCirc, EaseOutCirc },
        { Ease.EaseInOutCirc, EaseInOutCirc },
        { Ease.Linear, Linear },
        { Ease.Spring, Spring },
        { Ease.EaseInBounce, EaseInBounce },
        { Ease.EaseOutBounce, EaseOutBounce },
        { Ease.EaseInOutBounce, EaseInOutBounce },
        { Ease.EaseInBack, EaseInBack },
        { Ease.EaseOutBack, EaseOutBack },
        { Ease.EaseInOutBack, EaseInOutBack },
        { Ease.EaseInElastic, EaseInElastic },
        { Ease.EaseOutElastic, EaseOutElastic },
        { Ease.EaseInOutElastic, EaseInOutElastic }
    };

    private static readonly Dictionary<Ease, Function> EasingFunctionDerivativeMap = new()
    {
        { Ease.EaseInQuad, EaseInQuadD },
        { Ease.EaseOutQuad, EaseOutQuadD },
        { Ease.EaseInOutQuad, EaseInOutQuadD },
        { Ease.EaseInCubic, EaseInCubicD },
        { Ease.EaseOutCubic, EaseOutCubicD },
        { Ease.EaseInOutCubic, EaseInOutCubicD },
        { Ease.EaseInQuart, EaseInQuartD },
        { Ease.EaseOutQuart, EaseOutQuartD },
        { Ease.EaseInOutQuart, EaseInOutQuartD },
        { Ease.EaseInQuint, EaseInQuintD },
        { Ease.EaseOutQuint, EaseOutQuintD },
        { Ease.EaseInOutQuint, EaseInOutQuintD },
        { Ease.EaseInSine, EaseInSineD },
        { Ease.EaseOutSine, EaseOutSineD },
        { Ease.EaseInOutSine, EaseInOutSineD },
        { Ease.EaseInExpo, EaseInExpoD },
        { Ease.EaseOutExpo, EaseOutExpoD },
        { Ease.EaseInOutExpo, EaseInOutExpoD },
        { Ease.EaseInCirc, EaseInCircD },
        { Ease.EaseOutCirc, EaseOutCircD },
        { Ease.EaseInOutCirc, EaseInOutCircD },
        { Ease.Linear, LinearD },
        { Ease.Spring, SpringD },
        { Ease.EaseInBounce, EaseInBounceD },
        { Ease.EaseOutBounce, EaseOutBounceD },
        { Ease.EaseInOutBounce, EaseInOutBounceD },
        { Ease.EaseInBack, EaseInBackD },
        { Ease.EaseOutBack, EaseOutBackD },
        { Ease.EaseInOutBack, EaseInOutBackD },
        { Ease.EaseInElastic, EaseInElasticD },
        { Ease.EaseOutElastic, EaseOutElasticD },
        { Ease.EaseInOutElastic, EaseInOutElasticD }
    };
    
    [CanBeNull]
    public static Function GetEasingFunction(Ease easingFunction)
    {
        return EasingFunctionMap.TryGetValue(easingFunction, out var function) ? function : null;
    }
    
    [CanBeNull]
    public static Function GetEasingFunctionDerivative(Ease easingFunction)
    {
        return EasingFunctionDerivativeMap.TryGetValue(easingFunction, out var derivative) ? derivative : null;
    }
}