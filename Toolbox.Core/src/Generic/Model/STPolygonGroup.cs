﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core
{
    /// <summary>
    /// Stores a list of face indices and the capabily to map a material to them.
    /// This is used for when a mesh maps multiple materials to itself.
    /// </summary>
    public class STPolygonGroup
    {
        /// <summary>
        /// Gets or sets the index of the material.
        /// </summary>
        public int MaterialIndex { get; set; }

        /// <summary>
        /// Determines the draw order for the polygon group.
        /// Transparent passes are drawn after opaque in another pass.
        /// </summary>
        public bool IsTransparentPass { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="STGenericMaterial"/> 
        /// which determines how the mesh will be rendered.
        /// </summary>
        public STGenericMaterial Material { get; set; }

        /// <summary>
        /// Gets or sets a list of faces used to index vertices.
        /// </summary>
        public List<uint> Faces = new List<uint>();

        /// <summary>
        /// The offset to read the faces at. 
        /// </summary>
        public int FaceOffset { get; set; }

        /// <summary>
        /// Gets or sets the primitive type to be displayed for faces.
        /// </summary>
        public STPrimitiveType PrimitiveType { get; set; } = STPrimitiveType.Triangles;
    }
}
