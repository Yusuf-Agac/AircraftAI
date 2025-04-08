using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

public static class UniTaskExtension
{
    public static void CancelAndReset(ref CancellationTokenSource tokenSource)
    {
        tokenSource?.Cancel();
        tokenSource?.Dispose();
        tokenSource = new();
    }
    
    public static async UniTask UniSetPositionOrigin(this Rigidbody rigidbody, Transform origin, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.LastFixedUpdate, CancellationToken token = default)
    {
        while (true)
        {
            await UniTask.Yield(lifeCycleType);
            token.ThrowIfCancellationRequested();
            rigidbody.position = origin.position;
            rigidbody.rotation = origin.rotation;
            rigidbody.linearVelocity = Vector3.zero;
        }
    }
    
    public static async UniTask UniLocalPosition(this Transform transform, Vector3 target, float duration, Curves.Ease curve = Curves.Ease.Linear, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var curveFunction = Curves.GetEasingFunction(curve);
        if (curveFunction is null) return;

        var elapsedTime = 0f;
        var start = transform.localPosition;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(transform) transform.localPosition = Vector3.Lerp(start, target, curveFunction(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(transform) transform.localPosition = target;
    }

    public static async UniTask UniLocalPosition(this Transform transform, Vector3 target, float duration, AnimationCurve curve, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var elapsedTime = 0f;
        var start = transform.localPosition;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(transform) transform.localPosition = Vector3.Lerp(start, target, curve.Evaluate(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(transform) transform.localPosition = target;
    }

    
    public static async UniTask UniLocalScale(this Transform transform, Vector3 target, float duration, Curves.Ease curve = Curves.Ease.Linear, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var curveFunction = Curves.GetEasingFunction(curve);
        if (curveFunction is null) return;

        var elapsedTime = 0f;
        var start = transform.localScale;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(transform) transform.localScale = Vector3.Lerp(start, target, curveFunction(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(transform) transform.localScale = target;
    }

    
    public static async UniTask UniLocalScale(this Transform transform, Vector3 target, float duration, AnimationCurve curve, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var elapsedTime = 0f;
        var start = transform.localScale;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(transform) transform.localScale = Vector3.Lerp(start, target, curve.Evaluate(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(transform) transform.localScale = target;
    }
    
    public static async UniTask UniLocalEulerAngle(this Transform transform, Vector3 target, float duration, Curves.Ease curve = Curves.Ease.Linear, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var curveFunction = Curves.GetEasingFunction(curve);
        if (curveFunction is null) return;

        var elapsedTime = 0f;
        var start = transform.localEulerAngles;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(transform) transform.localEulerAngles = Vector3.Lerp(start, target, curveFunction(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(transform) transform.localEulerAngles = target;
    }

    public static async UniTask UniColor(this Material material, Color target, float duration, Curves.Ease curve = Curves.Ease.Linear, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var curveFunction = Curves.GetEasingFunction(curve);
        if (curveFunction is null) return;

        var elapsedTime = 0f;
        var start = material.color;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(material) material.color = Color.Lerp(start, target, curveFunction(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(material) material.color = target;
    }

    public static async UniTask UniColor(this Material material, Color target, float duration, AnimationCurve curve, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var elapsedTime = 0f;
        var start = material.color;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(material) material.color = Color.Lerp(start, target, curve.Evaluate(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(material) material.color = target;
    }
    
    public static async UniTask UniColor(this Image image, Color target, float duration, Curves.Ease curve = Curves.Ease.Linear, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var curveFunction = Curves.GetEasingFunction(curve);
        if (curveFunction is null) return;

        var elapsedTime = 0f;
        var start = image.color;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(image) image.color = Color.Lerp(start, target, curveFunction(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(image) image.color = target;
    }

    public static async UniTask UniColor(this Image image, Color target, float duration, AnimationCurve curve, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var elapsedTime = 0f;
        var start = image.color;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
            if(image) image.color = Color.Lerp(start, target, curve.Evaluate(t));
            await UniTask.Yield(lifeCycleType);
        }
        
        if(image) image.color = target;
    }
    
    public static async UniTask DoPlayAfter(this AudioSource audioSource, float duration, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            await UniTask.Yield(lifeCycleType);
        }
        
        if(audioSource) audioSource.Play();
    }

    public static async UniTask UniShake(
        this Camera camera, 
        float angle = 10f, 
        float strength = 0.2f, 
        float duration = 1f, 
        float noisePercent = 1f, 
        float dampingPercent = 1f, 
        float rotationPercent = 0.1f,
        CancellationToken token = default)
    {
        const float maxAngle = 15f;

        noisePercent = Mathf.Clamp01(noisePercent);
        dampingPercent = Mathf.Clamp01(dampingPercent);
        rotationPercent = Mathf.Clamp01(rotationPercent);

        var elapsedTime = 0f;
        var angleRadians = angle * Mathf.Deg2Rad - Mathf.PI;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += Time.deltaTime;
            var completionPercent = Mathf.Clamp01(elapsedTime / duration);

            var dampingFactor = Mathf.Pow(1 - Mathf.Pow(completionPercent, Mathf.Lerp(2, 0.25f, dampingPercent)), 3);
            var noiseAngle = (Random.value - 0.5f) * Mathf.PI * noisePercent;

            angleRadians += Mathf.PI + noiseAngle;
            var currentWaypoint = new Vector3(
                Mathf.Cos(angleRadians), 
                Mathf.Sin(angleRadians), 
                Mathf.Cos(angleRadians)
            ) * strength * dampingFactor;

            var targetRotation = Quaternion.Euler(
                new Vector3(currentWaypoint.y, currentWaypoint.x).normalized 
                * rotationPercent * dampingFactor * maxAngle
            );

            if (camera) 
                camera.transform.localRotation = targetRotation;

            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        }

        if (camera) 
            camera.transform.localRotation = Quaternion.identity;
    }
    
    public static async UniTask UniFade(this CanvasGroup canvasGroup, float target, float duration, bool ignoreTimeScale, AnimationCurve curve, PlayerLoopTiming lifeCycleType = PlayerLoopTiming.Update, CancellationToken token = default)
    {
        if (canvasGroup == null) return;

        var elapsedTime = 0f;
        var start = canvasGroup.alpha;

        while (elapsedTime < duration)
        {
            token.ThrowIfCancellationRequested();
            elapsedTime += ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / duration);
        
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(start, target, curve.Evaluate(t));

            await UniTask.Yield(lifeCycleType);
        }

        if (canvasGroup != null) canvasGroup.alpha = target;
    }
}