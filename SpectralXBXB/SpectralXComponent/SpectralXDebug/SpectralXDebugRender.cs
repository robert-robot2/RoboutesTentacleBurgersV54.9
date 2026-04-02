

namespace SpectralXBXB.SpectralXComponent.SpectralXDebug
{
    public class SpectralXDebugRender
    {
        private readonly SpectralXEngine engine;
        private readonly SpectralXViewport viewport;
        private readonly SpectralXInput input;
        private readonly SpectralXCamera camera;
        private readonly SpectralXMeshLibrary meshLibrary;

        public bool Enabled { get; set; } = false;

        public SpectralXDebugRender(
            SpectralXEngine engine,
            SpectralXViewport viewport,
            SpectralXInput input,
            SpectralXCamera camera,
            SpectralXMeshLibrary meshLibrary)
        {
            this.engine = engine;
            this.viewport = viewport;
            this.input = input;
            this.camera = camera;
            this.meshLibrary = meshLibrary;
        }

        public MarkupString Output
        {
            get
            {
                if (!Enabled)
                    return new MarkupString(string.Empty);

                var sb = new StringBuilder();

                sb.AppendLine("<div style='position:fixed;bottom:300px;left:0;");
                sb.AppendLine("padding:10px;background:rgba(0,0,0,0.8);");
                sb.AppendLine("color:white;font-family:monospace;width:300px;'>");

                sb.AppendLine("<h4>Recent Keys:</h4><div>");
                foreach (var key in input.DebugRecentKeys)
                {
                    bool active = input.DebugActiveSequences().Any(seq => seq.Contains(key));
                    sb.AppendLine(
                        $"<span style='display:inline-block;margin-right:5px;padding:2px 5px;" +
                        $"border-radius:3px;background:{(active ? "lime" : "#444")};" +
                        $"color:white;font-weight:bold;'> {key} </span>");
                }
                sb.AppendLine("</div>");

                sb.AppendLine("<h4>Active Sequences:</h4><div>");
                foreach (var seq in input.DebugActiveSequences())
                {
                    sb.AppendLine(
                        $"<span style='display:inline-block;margin:2px;padding:2px 5px;" +
                        $"border-radius:3px;background:orange;font-weight:bold;'> {seq} </span>");
                }
                sb.AppendLine("</div>");

                sb.AppendLine("<h4>Combo Meter:</h4>");
                sb.AppendLine("<div style='width:100%;height:10px;background:#222;border-radius:5px;'>");
                sb.AppendLine(
                    $"<div style='height:100%;width:{input.GetComboProgressPercent()}%;" +
                    "background:lime;border-radius:5px;'></div>");
                sb.AppendLine("</div>");

                sb.AppendLine("<h4>Debug Messages:</h4><div>");
                foreach (var msg in input.DebugMessages.TakeLast(5))
                {
                    sb.AppendLine($"<div>{msg}</div>");
                }
                sb.AppendLine("</div></div>");

                sb.AppendLine("<div style='position:absolute;top:10px;left:10px;");
                sb.AppendLine("color:lime;background:rgba(0,0,0,0.9);");
                sb.AppendLine("padding:10px;font-family:monospace;font-size:12px;'>");

                sb.AppendLine("<b>PERFORMANCE</b><br/>");
                sb.AppendLine($"FPS: {engine.Performance.CurrentFPS:F1}<br/>");
                sb.AppendLine($"Avg: {engine.Performance.AverageFPS:F1}<br/>");
                sb.AppendLine($"Min: {engine.Performance.MinFPS:F1}<br/>");
                sb.AppendLine($"Max: {engine.Performance.MaxFPS:F1}<br/>");
                sb.AppendLine($"Frame: {engine.Performance.FrameTimeMs:F2} ms<br/>");
                sb.AppendLine($"Viewport: {viewport.ViewportWidth} x {viewport.ViewportHeight}<br/>");

                sb.AppendLine("<hr style='border-color:lime;'/>");
                sb.AppendLine("<b>SCENE INFO</b><br/>");
                sb.AppendLine($"Triangles Rendered: {engine.Renderer.TrianglesRendered}<br/>");
                sb.AppendLine($"Quads Rendered: {engine.Renderer.QuadsRendered}<br/>");

                sb.AppendLine("<b>LOADED MESHES</b><br/>");
                foreach (var kvp in meshLibrary.Meshes)
                {
                    var mesh = kvp.Value;

                    sb.AppendLine(
                        $"{kvp.Key}: " +
                        $"{mesh.VertexCount} verts, " +
                        $"{mesh.FaceCount} faces, " +
                        $"{mesh.TriangleFaceCount} tris, " +
                        $"{mesh.QuadFaceCount} quads, " +
                        $"{mesh.EdgeCount} edges<br/>");
                }

                sb.AppendLine("<hr style='border-color:lime;'/>");
                foreach (var metric in engine.Performance.GetMetrics().Values)
                {
                    sb.AppendLine(
                        $"{metric.Name}: {metric.AverageMs:F2} ms " +
                        $"({metric.PercentOfFrame:F1}%)<br/>");
                }

             

                sb.AppendLine("<hr style='");
                sb.AppendLine("color:lime;background:rgba(0,0,0,0.8);");
                sb.AppendLine("padding:10px;font-family:monospace;'>");
                sb.AppendLine($"Camera Position: {camera.Position}");
                sb.AppendLine($"Camera Forward: {camera.Forward}");
                sb.AppendLine($"Camera Right:   {camera.Right}");
                sb.AppendLine($"Camera Up:      {camera.Up}");

             

                return new MarkupString(sb.ToString());
            }
        }
    }
}
