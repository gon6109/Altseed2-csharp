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

        public static IEnumerable<Model3D> Load(string path)
        {
            var models = new List<Model3D>();
            var file = StaticFile.Create(path);

            if (file == null)
                return Enumerable.Empty<Model3D>();

            using (var stream = new MemoryStream(file.Buffer))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split(' ');
                    if (tokens[0] == "g" && tokens.Length == 2)
                    {
                        var model = new Model3D();
                        models.Add(model);
                        model.Name = tokens[1];

                        while (true)
                        {
                            line = reader.ReadLine();

                            if (line == null)
                                return models;

                            tokens = line.Split(' ');
                            if (tokens[0] == "v" && tokens.Length == 4)
                            {

                            }
                        }
                    }
                }
            }
        }
    }
}
