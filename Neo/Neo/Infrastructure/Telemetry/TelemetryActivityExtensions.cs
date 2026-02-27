using System;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

namespace Neo.Infrastructure.Telemetry;

/// <summary>
/// Extension methods for adding telemetry to activities.
/// </summary>
public static class TelemetryActivityExtensions
{
    /// <summary>
    /// Sets matrix-related tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to annotate.</param>
    /// <param name="matrix">The matrix to describe.</param>
    /// <param name="prefix">The tag name prefix.</param>
    public static void SetMatrixTags(this Activity activity, Matrix<double> matrix, string prefix = "matrix")
    {
        if (activity == null)
            return;
            
        activity.SetTag($"{prefix}.rows", matrix.RowCount);
        activity.SetTag($"{prefix}.columns", matrix.ColumnCount);
        activity.SetTag($"{prefix}.isSquare", matrix.RowCount == matrix.ColumnCount);
        activity.SetTag($"{prefix}.isSymmetric", matrix.IsSymmetric());
    }

    /// <summary>
    /// Sets solution-related tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to annotate.</param>
    /// <param name="algorithm">The algorithm used.</param>
    /// <param name="success">Whether the operation succeeded.</param>
    /// <param name="duration">The operation duration.</param>
    public static void SetSolutionTags(this Activity activity, string algorithm, bool success, TimeSpan duration)
    {
        if (activity == null)
            return;
            
        activity.SetTag("neo.algorithm", algorithm);
        activity.SetTag("neo.success", success);
        activity.SetTag("neo.duration.ms", duration.TotalMilliseconds);
        activity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
    }

    /// <summary>
    /// Sets exception-related tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to annotate.</param>
    /// <param name="ex">The exception to record.</param>
    public static void SetExceptionTags(this Activity activity, Exception ex)
    {
        if (activity == null)
            return;
            
        activity.SetTag("neo.exception.type", ex.GetType().FullName);
        activity.SetTag("neo.exception.message", ex.Message);
        activity.SetTag("neo.exception.stacktrace", ex.StackTrace);
        activity.SetStatus(ActivityStatusCode.Error, ex.Message);
        // Note: RecordException requires OpenTelemetry.Api package
        // activity.RecordException(ex);
    }

    /// <summary>
    /// Sets cache-related tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to annotate.</param>
    /// <param name="cacheHit">Whether the cache lookup was a hit.</param>
    /// <param name="lookupDuration">The cache lookup duration.</param>
    public static void SetCacheTags(this Activity activity, bool cacheHit, TimeSpan? lookupDuration = null)
    {
        if (activity == null)
            return;
            
        activity.SetTag("neo.cache.hit", cacheHit);
        if (lookupDuration.HasValue)
        {
            activity.SetTag("neo.cache.lookup.ms", lookupDuration.Value.TotalMilliseconds);
        }
    }

    /// <summary>
    /// Sets equation system tags on an activity.
    /// </summary>
    /// <param name="activity">The activity to annotate.</param>
    /// <param name="equationCount">The number of equations.</param>
    /// <param name="variableCount">The number of variables.</param>
    public static void SetEquationSystemTags(this Activity activity, int equationCount, int variableCount)
    {
        if (activity == null)
            return;
            
        activity.SetTag("neo.equation.count", equationCount);
        activity.SetTag("neo.variable.count", variableCount);
        activity.SetTag("neo.isSquare", equationCount == variableCount);
    }
}
