using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour {
    private TerrainEditingScriptableObject terrainEditingSO;
    private List<LayerScriptableObject> layers = new List<LayerScriptableObject>();

    private readonly EditingStencil[] stencils = {
        new EditingStencil(),
        new EditingStencilCircle()
    };

    public void Start() {
        terrainEditingSO = FindObjectOfType<TerrainEditorController>().GetTerrainEditingScriptableObject();
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
    }

    public void EditVoxels(IEnumerable<Voxel> voxels) {
        foreach (Voxel voxel in voxels) {
            voxel.state = (int)terrainEditingSO.EditingType;
        }
    }

    public List<Voxel> GetSelectedVoxels(VoxelChunk chunk, Vector2 selectPoint) {
        EditingStencil activeStencil = stencils[(int)terrainEditingSO.StencilType];
        BlockType editingType = terrainEditingSO.EditingType;
        List<Voxel> selectedVoxels = new List<Voxel>();
        int voxelResolution = layers[0].CORE.voxelResolution;
        int radius = terrainEditingSO.Radius;

        for (int i = -radius; i <= radius; i++) {
            for (int j = -radius; j <= radius; j++) {
                Vector2 hitPosition = new Vector2(selectPoint.x + i, selectPoint.y + j);

                // check if position exists in this chunk
                if (ChunkHelper.ChunkContainsPosition(chunk, hitPosition, voxelResolution)) {
                    Voxel voxel = chunk.voxels[ChunkHelper.GetVoxelIndex(hitPosition, voxelResolution)];

                    if (!selectedVoxels.Contains(voxel) && activeStencil.IsVoxelInStencil(selectPoint, hitPosition, radius)) {
                        selectedVoxels.Add(voxel);
                    }
                }
            }
        }

        // Filter selected voxels by editing type
        List<Voxel> filteredVoxels = new List<Voxel>();
        foreach (Voxel voxel in selectedVoxels) {
            if (voxel.state != (int)editingType) {
                filteredVoxels.Add(voxel);
            }
        }

        return filteredVoxels;
    }
}
