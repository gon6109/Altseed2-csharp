using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Altseed2
{
    /// <summary>
    /// Objファイル
    /// </summary>
    public class Model3D
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 頂点
        /// </summary>
        public List<Vector3F> Vertexes { get; }

        /// <summary>
        /// UV
        /// </summary>
        public List<Vector2F> UVs { get; }

        /// <summary>
        /// 法線
        /// </summary>
        public List<Vector3F> Normals { get; }

        /// <summary>
        /// 面
        /// </summary>
        public List<List<(int vertex, int? uv, int? normal)>> Faces { get; }

        public Model3D()
        {
            Vertexes = new List<Vector3F>();
            UVs = new List<Vector2F>();
            Normals = new List<Vector3F>();
            Faces = new List<List<(int vertex, int? uv, int? normal)>>();
        }

        /// <summary>
        /// Obj Fileを読み込む
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerable<Model3D> LoadObjFile(string path)
        {
            var models = new List<Model3D>();
            var file = StaticFile.Create(path);

            int vOffset = 1;
            int uvOffset = 1;
            int normalOffset = 1;

            if (file == null)
                return Enumerable.Empty<Model3D>();

            using (var stream = new MemoryStream(file.Buffer))
            using (var reader = new StreamReader(stream))
            {
                string line;
                string[] tokens;
                Model3D model = null;
                while ((line = reader.ReadLine()) != null)
                {
                    tokens = line.Split(' ');
                    if ((tokens[0] == "g" || tokens[0] == "o") && tokens.Length == 2)
                    {
                        model = new Model3D();
                        models.Add(model);
                        model.Name = tokens[1];
                        break;
                    }
                }

                while (true)
                {
                    line = reader.ReadLine();

                    if (line == null)
                        return models;

                    tokens = line.Split(' ');

                    if ((tokens[0] == "g" || tokens[0] == "o") && tokens.Length == 2)
                    {
                        vOffset += model.Vertexes.Count;
                        uvOffset += model.UVs.Count;
                        normalOffset += model.Normals.Count;
                        model = new Model3D();
                        models.Add(model);
                        model.Name = tokens[1];
                    }

                    if (tokens[0] == "v" && tokens.Length == 4)
                    {
                        Vector3F vertex = new Vector3F();
                        try
                        {
                            vertex.X = Convert.ToSingle(tokens[1]);
                            vertex.Y = Convert.ToSingle(tokens[2]);
                            vertex.Z = Convert.ToSingle(tokens[3]);
                        }
                        catch (Exception e)
                        {
                            Engine.Log.Error(LogCategory.Engine, $"Model3D.Load Error: {e.Message}");
                        }
                        model.Vertexes.Add(vertex);
                    }

                    if (tokens[0] == "vt" && tokens.Length == 3)
                    {
                        Vector2F uv = new Vector2F();
                        try
                        {
                            uv.X = Convert.ToSingle(tokens[1]);
                            uv.Y = Convert.ToSingle(tokens[2]);
                        }
                        catch (Exception e)
                        {
                            Engine.Log.Error(LogCategory.Engine, $"Model3D.Load Error: {e.Message}");
                        }
                        model.UVs.Add(uv);
                    }

                    if (tokens[0] == "vn" && tokens.Length == 4)
                    {
                        Vector3F normal = new Vector3F();
                        try
                        {
                            normal.X = Convert.ToSingle(tokens[1]);
                            normal.Y = Convert.ToSingle(tokens[2]);
                            normal.Z = Convert.ToSingle(tokens[3]);
                        }
                        catch (Exception e)
                        {
                            Engine.Log.Error(LogCategory.Engine, $"Model3D.Load Error: {e.Message}");
                        }
                        model.Normals.Add(normal);
                    }

                    if (tokens[0] == "f" && tokens.Length >= 4)
                    {
                        List<(int vertex, int? uv, int? normal)> face = new List<(int vertex, int? uv, int? normal)>();
                        foreach (var token in tokens.Skip(1))
                        {
                            var indexes = token.Split('/');
                            try
                            {
                                (int vertex, int? uv, int? normal) v = (
                                    Convert.ToInt32(indexes[0]) - vOffset,
                                    indexes.Length >= 2 ? (int?)(Convert.ToInt32(indexes[1]) - uvOffset) : null,
                                    indexes.Length >= 3 ? (int?)(Convert.ToInt32(indexes[2]) - normalOffset) : null
                                    );
                                face.Add(v);
                            }
                            catch (Exception e)
                            {
                                Engine.Log.Error(LogCategory.Engine, $"Model3D.Load Error: {e.Message}");
                                face.Add((0, null, null));
                            }
                        }
                        model.Faces.Add(face);
                    }
                }

            }

            return models;
        }

        /// <summary>
        /// モデルから<see cref="Polygon3DNode"/>を生成する
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public Polygon3DNode ToPolygon3DNode(Color color, bool isCalcNormal = false)
        {
            var node = new Polygon3DNode();
            Dictionary<(int vertex, int? uv, int? normal), int> vertexes = new Dictionary<(int vertex, int? uv, int? normal), int>();

            Dictionary<int, Vector3F> faceNormals = new Dictionary<int, Vector3F>();
            Dictionary<int, Vector3F> vertexNormals = new Dictionary<int, Vector3F>();
            if (isCalcNormal)
            {
                foreach (var (face, i) in Faces.Select((face, i) => (face, i)))
                {
                    var vec1 = Vertexes[face[1].vertex] - Vertexes[face[0].vertex];
                    var vec2 = Vertexes[face[2].vertex] - Vertexes[face[0].vertex];

                    faceNormals[i] = Vector3F.Cross(vec1, vec2).Normal;
                }
            }

            foreach (var v in Faces.SelectMany(face => face))
            {
                if (!vertexes.ContainsKey(v))
                    vertexes[v] = vertexes.Count;
            }

            if (isCalcNormal)
            {
                foreach (var v in vertexes)
                {
                    if (vertexNormals.ContainsKey(v.Key.vertex))
                        continue;

                    var indexes = new HashSet<int>();
                    foreach (var (face, i) in Faces.Select((face, i) => (face, i)))
                    {
                        if (face.Any(obj => obj.vertex == v.Key.vertex))
                            indexes.Add(i);
                    }

                    vertexNormals[v.Key.vertex] = new Vector3F();
                    foreach (var normal in faceNormals.Where(fn => indexes.Contains(fn.Key)).Select(fn => fn.Value))
                    {
                        vertexNormals[v.Key.vertex] += normal;
                    }
                    vertexNormals[v.Key.vertex] = vertexNormals[v.Key.vertex].Normal;
                }
            }

            node.Vertexes = vertexes.OrderBy(v => v.Value)
                .Select(v => new Vertex(Vertexes[v.Key.vertex], GetNormal(v), color, v.Key.uv.HasValue ? UVs[v.Key.uv.Value] : new Vector2F(), new Vector2F()))
                .ToList();

            node.Buffers = Faces.SelectMany(face => DivideToTriangles(face, vertexes)).ToList();

            return node;

            Vector3F GetNormal(KeyValuePair<(int vertex, int? uv, int? normal), int> v)
            {
                if (isCalcNormal)
                    return vertexNormals[v.Key.vertex];
                return v.Key.normal.HasValue ? Normals[v.Key.normal.Value] : new Vector3F();
            }
        }

        IEnumerable<int> DivideToTriangles(List<(int vertex, int? uv, int? normal)> face, Dictionary<(int vertex, int? uv, int? normal), int> vertexes)
        {
            if (face.Count < 3) return Enumerable.Empty<int>();

            List<int> result = new List<int>();
            if (face.Count == 3)
            {
                result.AddRange(face.Select(v => vertexes[v]));
                return result;
            }

            var root = new Vector3F();
            (int vertex, int? uv, int? normal) rootV = (-1, null, null);
            foreach (var v in face)
            {
                if (root.Length < Vertexes[v.vertex].Length)
                {
                    root = Vertexes[v.vertex];
                    rootV = v;
                }
            }

            for (int i = 0; i < face.Count - 2; i++)
            {
                var v1 = face[0];
                var v2 = face[i + 1];
                var v3 = face[i + 2];

                result.Add(vertexes[v1]);
                result.Add(vertexes[v2]);
                result.Add(vertexes[v3]);
            }

            return result;
        }
    }
}
