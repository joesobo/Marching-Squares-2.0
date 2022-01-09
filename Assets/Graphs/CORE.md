```mermaid
graph LR;
  %% Config
  linkStyle default interpolate linear;

  %% Core Utilities
  subgraph Core
    CORE((CORE));
    VoxelCore{{VoxelCore}};
    InfiniteGenerator{{InfiniteGenerator}};

    subgraph ChunkGen
      VoxelChunkGenerator{{VoxelChunkGenerator}};
      VoxelChunk{{VoxelChunk}};
    end

    subgraph Colliders
      ColliderGenerator{{ColliderGenerator}};
      ColliderLogic{{ColliderLogic}};
    end

    subgraph Mesh
      VoxelMeshGenerator{{VoxelMeshGenerator}};
      VoxelMesh{{VoxelMesh}};
    end

    subgraph MarchingSquares
      MeshShaderController{{MeshShaderController}};
      MeshShader{{MeshShader}};
      OutlineShaderController{{OutlineShaderController}};
      OutlineShader{{OutlineShader}};
      MarchingHelper{{MarchingHelper}};
    end
  end

  %% Debug
  subgraph Debug
    DebugController{{DebugController}};
  end

  %% Extensions
  subgraph Extensions
    ChunkHelper{{ChunkHelper}};
    Vector2Extension{{Vector2Extension}};
  end

  %% Core Connections
  VoxelCore ==> CORE;
  VoxelCore ==> InfiniteGenerator;

  InfiniteGenerator ==> VoxelCore;
  InfiniteGenerator ==> VoxelChunkGenerator;

  VoxelChunkGenerator ==> VoxelCore;
  VoxelChunkGenerator ==> VoxelChunk;

  VoxelChunk ==> VoxelCore;
  VoxelChunk ==> ColliderGenerator;
  VoxelChunk ==> VoxelMeshGenerator;

  ColliderGenerator ==> VoxelCore;
  ColliderGenerator ==> ColliderLogic;
  ColliderGenerator ==> OutlineShaderController;

  VoxelMeshGenerator ==> VoxelMesh;

  VoxelMesh ==> VoxelCore;
  VoxelMesh ==> MeshShaderController;

  MeshShaderController ==> VoxelCore;
  MeshShaderController ==> MeshShader;

  OutlineShaderController ==> VoxelCore;
  OutlineShaderController ==> OutlineShader;

  MeshShader ==> MarchingHelper;
  OutlineShader ==> MarchingHelper;

  ChunkHelper ==> InfiniteGenerator;
  ChunkHelper ==> VoxelChunkGenerator;

  Vector2Extension ==> ColliderLogic;

  DebugController ==> VoxelCore;
```
