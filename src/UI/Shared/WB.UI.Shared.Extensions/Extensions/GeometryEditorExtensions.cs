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

        PropertyChangedEventHandler onPropertyChanged = null;
        onPropertyChanged = (s, e) =>
        {
            if ((e.PropertyName == nameof(GeometryEditor.Geometry) || e.PropertyName == nameof(GeometryEditor.IsStarted)) 
                && !geometryEditor.IsStarted)
            {
                geometryEditor.PropertyChanged -= onPropertyChanged;
                tcs.TrySetResult(geometryEditor.Geometry);
            }
        };
        geometryEditor.PropertyChanged += onPropertyChanged;

        try
        {
            startAction.Invoke();
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
        return tcs.Task;
    }
}
