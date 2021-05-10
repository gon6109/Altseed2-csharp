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

            var vertexes = new List<Vector3F>();
            var uvs = new List<Vector2F>();
            var normals = new List<Vector3F>();

            if (file == null)
                return Enumerable.Empty<Model3D>();

            using (var stream = new MemoryStream(file.Buffer))
            using (var reader = new StreamReader(stream))
            {
                string line;
                Model3D model = null;

                while (true)
                {
                    line = reader.ReadLine();

                    if (line == null)
                        return models;

                    if (line == "")
                        continue;

                    var tokens = line.Split(' ').Where(token => token != "").ToList();

                    if (tokens[0].First() == '#')
                        continue;

                    if ((tokens[0] == "g" || tokens[0] == "o") && tokens.Count >= 2)
                    {
                        model = new Model3D();
                        models.Add(model);
                        model.Name = tokens[1];
                    }

                    if (tokens[0] == "v" && tokens.Count >= 4)
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
                        vertexes.Add(vertex);
                    }

                    if (tokens[0] == "vt" && tokens.Count >= 3)
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
                        uvs.Add(uv);
                    }

                    if (tokens[0] == "vn" && tokens.Count >= 4)
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
                        normals.Add(normal);
                    }

                    if (tokens[0] == "f" && tokens.Count >= 4)
                    {
                        if (model == null)
                        {
                            model = new Model3D();
                            models.Add(model);
                        }

                        if (model.Vertexes.Count == 0)
                        {
                            model.Vertexes.AddRange(vertexes);
                            model.UVs.AddRange(uvs);
                            model.Normals.AddRange(normals);
                        }

                        List<(int vertex, int? uv, int? normal)> face = new List<(int vertex, int? uv, int? normal)>();
                        foreach (var token in tokens.Skip(1))
                        {
                            var indexes = token.Split('/');
                            try
                            {
                                (int vertex, int? uv, int? normal) v = (
                                    Convert.ToInt32(indexes[0]) - 1,
                                    indexes.Length >= 2 && indexes[1] != "" ? (int?)(Convert.ToInt32(indexes[1]) - 1) : null,
                                    indexes.Length >= 3 ? (int?)(Convert.ToInt32(indexes[2]) - 1) : null
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
        }

        /// <summary>
        /// 直方体を作成する
        /// </summary>
        /// <param name="harfSize">対角線の半分ベクトル</param>
        /// <returns></returns>
        public static Model3D CreateBox(Vector3F harfSize)
        {
            var model = new Model3D();

            model.Vertexes.AddRange(new List<Vector3F>()
            {
                new Vector3F(-1, 1, -1),
                new Vector3F(-1, 1, 1),
                new Vector3F(1, 1, 1),
                new Vector3F(1, 1, -1),
                new Vector3F(-1, -1, -1),
                new Vector3F(-1, -1, 1),
                new Vector3F(1, -1, 1),
                new Vector3F(1, -1, -1),
            }.Select(v => v * harfSize));

            model.Normals.AddRange(new List<Vector3F>()
            {
                new Vector3F(1, 0, 0),
                new Vector3F(-1, 0, 0),
                new Vector3F(0, 1, 0),
                new Vector3F(0, -1, 0),
                new Vector3F(0, 0, 1),
                new Vector3F(0, 0, -1),
            });

            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (0, null, 2),
                (1, null, 2),
                (2, null, 2),
                (3, null, 2),
            });
            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (7, null, 3),
                (6, null, 3),
                (5, null, 3),
                (4, null, 3),
            });
            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (0, null, 5),
                (3, null, 5),
                (7, null, 5),
                (4, null, 5),
            });
            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (1, null, 1),
                (0, null, 1),
                (4, null, 1),
                (5, null, 1),
            });
            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (2, null, 4),
                (1, null, 4),
                (5, null, 4),
                (6, null, 4),
            });
            model.Faces.Add(new List<(int vertex, int? uv, int? normal)>
            {
                (3, null, 0),
                (2, null, 0),
                (6, null, 0),
                (7, null, 0),
            });

            return model;
        }

        /// <summary>
        /// UV球を作成する
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="segment">垂直方向の分割数</param>
        /// <param name="ring">水平方向の分割数</param>
        /// <returns></returns>
        public static Model3D CreateUVSphere(float radius, int segment, int ring)
        {
            var model = new Model3D();

            // 頂点数(極点2つ+経線・緯線の交点)
            int vn = 2 + (segment - 1) * ring;
            model.Vertexes.AddRange(Enumerable.Repeat(new Vector3F(), vn));
            model.Normals.AddRange(Enumerable.Repeat(new Vector3F(), vn));
            model.UVs.AddRange(Enumerable.Repeat(new Vector2F(), vn));

            // 北極点
            model.Vertexes[0] = new Vector3F(0, 0, radius);
            model.Normals[0] = new Vector3F(0, 0, 1);
            // 南極点
            model.Vertexes[vn - 1] = new Vector3F(0, 0, -radius);
            model.Normals[vn - 1] = new Vector3F(0, 0, -1);

            // Φ(経線に沿った方向の角度)の増分量
            var dp = MathF.PI / segment;
            // Θ(緯線に沿った方向の角度)の増分量
            var dt = 2.0f * MathF.PI / ring;

            // 経線に沿った方向の角度(北極点で0)
            var phi = dp;

            // 球を構成する頂点の生成
            for (int i = 0; i < segment - 1; ++i)
            {
                var z = radius * MathF.Cos(phi);
                // 緯線に沿った方向の角度
                float theta = 0.0f;
                for (int j = 0; j < ring; ++j)
                {
                    var rad_t = radius * MathF.Sin(phi);

                    int idx = 1 + i * ring + j;
                    model.Vertexes[idx] = new Vector3F(rad_t * MathF.Cos(theta), rad_t * MathF.Sin(theta), z);
                    model.Normals[idx] = model.Vertexes[idx].Normal;
                    model.UVs[idx] = new Vector2F(theta / (2.0f * MathF.PI), phi / (MathF.PI));

                    theta += dt;
                }
                phi += dp;
            }

            // ポリゴン生成
            for (int i = 0; i < segment; ++i)
            {
                // 北極点と最初の緯線を接続
                if (i == 0)
                {
                    var f = new List<(int vertex, int? uv, int? normal)>();
                    for (int j = 0; j < ring; ++j)
                    {
                        int k0 = 1 + i * ring + j;
                        int k1 = 1 + i * ring + (j == ring - 1 ? 0 : j + 1);
                        f.Add((0, 0, 0));
                        f.Add((k0, k0, k0));
                        f.Add((k1, k1, k1));
                        model.Faces.Add(f);
                    }
                }
                // 南極点と最後の緯線を接続
                else if (i == segment - 1)
                {
                    var f = new List<(int vertex, int? uv, int? normal)>();
                    for (int j = 0; j < ring; ++j)
                    {
                        int k0 = 1 + (i - 1) * ring + j;
                        int k1 = 1 + (i - 1) * ring + (j == ring - 1 ? 0 : j + 1);
                        f.Add((vn - 1, vn - 1, vn - 1));
                        f.Add((k1, k1, k1));
                        f.Add((k0, k0, k0));
                        model.Faces.Add(f);
                    }
                }
                else
                {
                    for (int j = 0; j < ring; ++j)
                    {
                        int k0 = 1 + i * ring + j;
                        int k1 = 1 + i * ring + (j == ring - 1 ? 0 : j + 1);
                        int k2 = 1 + (i - 1) * ring + j;
                        int k3 = 1 + (i - 1) * ring + (j == ring - 1 ? 0 : j + 1);

                        var f = new List<(int vertex, int? uv, int? normal)>();

                        // 四角形ポリゴンを用いる
                        f.Add((k0, k0, k0));
                        f.Add((k1, k1, k1));
                        f.Add((k3, k3, k3));
                        f.Add((k2, k2, k2));
                        model.Faces.Add(f);
                    }
                }
            }

            return model;
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
