using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainEditor : MonoBehaviour {
    private TerrainEditingScriptableObject terrainEditingSO;
    private List<LayerScriptableObject> layers = new List<LayerScriptableObject>();
    private CoreScriptableObject CORE;

    private readonly EditingStencil[] stencils = {
        new EditingStencil(),
        new EditingStencilCircle()
    };

    public void Start() {
        terrainEditingSO = FindObjectOfType<TerrainEditorController>().GetTerrainEditingScriptableObject();
        layers = FindObjectOfType<VoxelCore>().GetAllLayerScriptableObjects();
        CORE = FindObjectOfType<VoxelCore>().GetCoreScriptableObject();
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
        int voxelResolution = CORE.voxelResolution;
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
        return selectedVoxels.Where(voxel => voxel.state != (int)editingType).ToList();
    }
}
