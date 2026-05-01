using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Chorewars.Tools
{
    /// <summary>
    /// Lightweight OBJ merge utility for prototype packaging.
    /// Expects OBJ files produced by this project (vertices + triangle faces).
    /// </summary>
    public static class HouseMapExporter
    {
        public static string MergeObjFiles(IReadOnlyList<string> inputObjAbsolutePaths, string outputAbsolutePath)
        {
            if (inputObjAbsolutePaths == null || inputObjAbsolutePaths.Count == 0)
                throw new ArgumentException("No input OBJ paths.");

            var outDir = Path.GetDirectoryName(outputAbsolutePath);
            if (!string.IsNullOrWhiteSpace(outDir)) Directory.CreateDirectory(outDir);

            var sb = new StringBuilder(1024 * 1024);
            sb.AppendLine("# BoreDOOM merged OBJ export");
            sb.AppendLine($"# generated_utc {DateTime.UtcNow:O}");

            int vertexOffset = 1;

            foreach (var path in inputObjAbsolutePaths)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) continue;

                sb.AppendLine($"o merged_{Path.GetFileNameWithoutExtension(path)}");

                var verts = new List<Vector3Like>();
                var faces = new List<TriangleFace>();

                ParseObjMinimal(path, verts, faces);

                foreach (var v in verts)
                    sb.AppendLine($"v {v.x.ToString(CultureInfo.InvariantCulture)} {v.y.ToString(CultureInfo.InvariantCulture)} {v.z.ToString(CultureInfo.InvariantCulture)}");

                foreach (var f in faces)
                {
                    int a = f.a + vertexOffset;
                    int b = f.b + vertexOffset;
                    int c = f.c + vertexOffset;
                    sb.AppendLine($"f {a} {b} {c}");
                }

                vertexOffset += verts.Count;
            }

            File.WriteAllText(outputAbsolutePath, sb.ToString());
            return outputAbsolutePath;
        }

        private readonly struct Vector3Like
        {
            public readonly float x, y, z;
            public Vector3Like(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        private readonly struct TriangleFace
        {
            public readonly int a, b, c;
            public TriangleFace(int a, int b, int c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }
        }

        private static void ParseObjMinimal(string path, List<Vector3Like> verts, List<TriangleFace> faces)
        {
            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;

                if (line.StartsWith("v ", StringComparison.Ordinal))
                {
                    if (!TrySplitWords(line, out var parts) || parts.Length < 4) continue;
                    if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) continue;
                    if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) continue;
                    if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) continue;
                    verts.Add(new Vector3Like(x, y, z));
                    continue;
                }

                if (line.StartsWith("f ", StringComparison.Ordinal))
                {
                    // Supports: f i j k  OR  f i//ni j//nj k//nk  OR  f i/j/ni ...
                    if (!TrySplitWords(line, out var parts) || parts.Length < 4) continue;

                    if (!TryParseFaceVertexIndex(parts[1], out var ia)) continue;
                    if (!TryParseFaceVertexIndex(parts[2], out var ib)) continue;
                    if (!TryParseFaceVertexIndex(parts[3], out var ic)) continue;

                    // OBJ indices are 1-based; our intermediate verts list is 0-based.
                    int a = ia - 1;
                    int b = ib - 1;
                    int c = ic - 1;
                    if (a < 0 || b < 0 || c < 0) continue;
                    if (a >= verts.Count || b >= verts.Count || c >= verts.Count) continue;

                    faces.Add(new TriangleFace(a, b, c));
                }
            }
        }

        private static bool TryParseFaceVertexIndex(string token, out int vi)
        {
            vi = 0;
            // token format examples: "12", "12/3/4", "12//4"
            var slash = token.IndexOf('/');
            var head = slash >= 0 ? token.Substring(0, slash) : token;
            return int.TryParse(head, NumberStyles.Integer, CultureInfo.InvariantCulture, out vi);
        }

        private static bool TrySplitWords(string line, out string[] parts)
        {
            var list = new List<string>(8);
            int i = 0;
            while (i < line.Length)
            {
                while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
                if (i >= line.Length) break;

                int start = i;
                while (i < line.Length && !char.IsWhiteSpace(line[i])) i++;
                list.Add(line.Substring(start, i - start));
            }

            parts = list.ToArray();
            return parts.Length > 0;
        }
    }
}
