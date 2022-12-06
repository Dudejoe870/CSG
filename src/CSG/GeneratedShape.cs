namespace CSG
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    /// <summary>
    /// A <see cref="GeneratedShape"/> is a processed shape built from other <see cref="Shape"/>'s.
    /// </summary>
    public class GeneratedShape : Shape
    {
        /// <summary>
        /// <see cref="Polygon"/>'s that was generated from the other <see cref="Shape"/>'s.
        /// </summary>
        public readonly Polygon[] Polygons;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedShape"/> class.
        /// </summary>
        /// <param name="polygons"></param>
        public GeneratedShape(IEnumerable<Polygon> polygons)
        {
            this.Polygons = polygons.ToArray();
        }

        /// <inheritdoc />
        public override Polygon[] CreatePolygons()
            => this.Polygons;

        /// <inheritdoc />
        protected override void OnBuild(IShapeBuilder builder)
        {
            int p = 0;
            for (int i = 0; i < Polygons.Length; i++)
            {
                var poly = Polygons[i];

                // Use Newell's Method to recalculate the Normals.
                // (The normals calculated previously aren't accurate enough for flat shading,
                // however using these normals for the BSP Tree seems to get it stuck in a infinite loop?)
                var normal = Vector3.Zero;
                for (int j = 0; j < poly.Vertices.Count; j++)
                {
                    var currentVertex = poly.Vertices[j].Position;
                    var nextVertex = poly.Vertices[(j + 1) % poly.Vertices.Count].Position;

                    normal += Vector3.Cross(nextVertex, currentVertex);
                }
                normal = Vector3.Normalize(normal);

                for (int j = 2; j < poly.Vertices.Count; j++)
                {
                    builder.AddVertex(poly.Vertices[0].Position, normal, poly.Vertices[0].TexCoords, poly.Vertices[0].Color);
                    builder.AddIndex(p++);

                    builder.AddVertex(poly.Vertices[j - 1].Position, normal, poly.Vertices[j - 1].TexCoords, poly.Vertices[j - 1].Color);
                    builder.AddIndex(p++);

                    builder.AddVertex(poly.Vertices[j].Position, normal, poly.Vertices[j].TexCoords, poly.Vertices[j].Color);
                    builder.AddIndex(p++);
                }
            }
        }
    }
}
