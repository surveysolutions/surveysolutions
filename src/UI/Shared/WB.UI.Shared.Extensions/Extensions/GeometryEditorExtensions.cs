using System.ComponentModel;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI.Editing;

namespace WB.UI.Shared.Extensions.Extensions;

public static class GeometryEditorExtensions
{
    public static Task<Geometry> StartAsync(this GeometryEditor geometryEditor, Geometry geometry)
    {
        return StartImplAsync(geometryEditor, () => geometryEditor.Start(geometry));
    }
    
    public static Task<Geometry> StartAsync(this GeometryEditor geometryEditor, GeometryType geometryType)
    {
        return StartImplAsync(geometryEditor, () => geometryEditor.Start(geometryType));
    }
    
    private static Task<Geometry> StartImplAsync(GeometryEditor geometryEditor, Action startAction)
    {
        var tcs = new TaskCompletionSource<Geometry>();
        Geometry lastGeometry = geometryEditor.Geometry;

        PropertyChangedEventHandler onPropertyChanged = null;
        onPropertyChanged = (_, e) =>
        {
            if (e.PropertyName == nameof(GeometryEditor.Geometry) && geometryEditor.Geometry != null)
                lastGeometry = geometryEditor.Geometry;

            if (e.PropertyName == nameof(GeometryEditor.IsStarted) && !geometryEditor.IsStarted)
            {
                geometryEditor.PropertyChanged -= onPropertyChanged;
                tcs.TrySetResult(geometryEditor.Geometry ?? lastGeometry);
            }
        };
        geometryEditor.PropertyChanged += onPropertyChanged;

        try
        {
            startAction.Invoke();
            lastGeometry ??= geometryEditor.Geometry;
        }
        catch (Exception ex)
        {
            geometryEditor.PropertyChanged -= onPropertyChanged;
            tcs.TrySetException(ex);
        }
        return tcs.Task;
    }
}
