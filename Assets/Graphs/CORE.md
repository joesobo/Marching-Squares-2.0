```mermaid
graph LR;
  %% Config
  linkStyle default interpolate linear;

  %% ScriptableObjects
  subgraph ScriptableObjects
    CORE((CORE));
    Layer((Layer));
    Noise((Noise));
    Editing((Editing));
    World((World));
  end

   %% Types
  subgraph Types
    BlockTypes((BlockTypes));
    StencilTypes((StencilTypes));
    TerrainTypes((TerrainTypes));
  end

  %% Core Utilities
  subgraph Core
    VoxelCore{{VoxelCore}};
    InfiniteGenerator{{InfiniteGenerator}};

    subgraph ChunkGen
      VoxelChunkGenerator{{VoxelChunkGenerator}};
      VoxelChunk{{VoxelChunk}};
    end

    subgraph Mesh
      VoxelMeshGenerator{{VoxelMeshGenerator}};
      VoxelMesh{{VoxelMesh}};
      MeshShaderController{{MeshShaderController}};
      MeshShader{{MeshShader}};
    end

    MarchingHelper{{MarchingHelper}};
  end

  %% Outlines
  subgraph Outlines
      ColliderGenerator{{ColliderGenerator}};
      OutlineDrawGenerator{{OutlineDrawGenerator}};
      OutlineLogic{{OutlineLogic}};
      OutlineShaderController{{OutlineShaderController}};
      OutlineShader{{OutlineShader}};
    end

  %% Debug
  subgraph Debug
    DebugController{{DebugController}};
  end

  %% Extensions
  subgraph Extensions
    ChunkHelper{{ChunkHelper}};
    Vector2Extension{{Vector2Extension}};
    Vector2SerializationSurrogate{{Vector2SerializationSurrogate}};
    Vector3SerializationSurrogate{{Vector3SerializationSurrogate}};
    TextureScaler{{TextureScaler}};
  end

  %% Terrain Editor
  subgraph TerrainEditorModule
    TerrainEditorController{{TerrainEditorController}};
    TerrainEditorGizmos{{TerrainEditorGizmos}};
    TerrainEditor{{TerrainEditor}};
    EditingStencil{{EditingStencil}};
    EditingStencilCircle{{EditingStencilCircle}};
  end

  %% Terrain Noise
  subgraph TerrainNoise
    TerrainGenerationController{{TerrainGenerationController}};
    NoiseFill{{NoiseFill}};
    NoiseRandom{{NoiseRandom}};
    NoisePerlin{{NoisePerlin}};
  end

  %% Save System
  subgraph SaveSystem
    ChunkSaveManager{{ChunkSaveManager}};
    SerializationManager{{SerializationManager}};
    WorldSaveManager{{WorldSaveManager}};
    RegionSaveManager{{RegionSaveManager}};
  end

  %% Lighting System
  subgraph LightingSystem
    DynamicLighting{{DynamicLighting}};
    LightingFiller{{LightingFiller}};
    LightingGenerator{{LightingGenerator}};
    LightingHelper{{LightingHelper}};
    StaticLighting{{StaticLighting}};
  end

  Player{Player};

  %% Core Connections
  World ==> Layer;

  Layer ==> Noise;

  Noise ==> TerrainTypes;

  VoxelCore ==> InfiniteGenerator;
  VoxelCore ==> CORE;
  VoxelCore ==> World;

  InfiniteGenerator ==> VoxelCore;
  InfiniteGenerator ==> VoxelChunkGenerator;
  InfiniteGenerator ==> Player;
  InfiniteGenerator ==> ChunkHelper;
  InfiniteGenerator ==> ChunkSaveManager;
  InfiniteGenerator ==> LightingFiller;
  InfiniteGenerator ==> LightingGenerator;

  VoxelChunkGenerator ==> VoxelChunk;
  VoxelChunkGenerator ==> VoxelCore;
  VoxelChunkGenerator ==> ChunkSaveManager;

  VoxelChunk ==> VoxelChunkGenerator;
  VoxelChunk ==> VoxelMeshGenerator;
  VoxelChunk ==> ColliderGenerator;
  VoxelChunk ==> TerrainGenerationController;
  VoxelChunk ==> OutlineDrawGenerator;
  VoxelChunk ==> VoxelCore;

  ColliderGenerator ==> OutlineLogic;
  ColliderGenerator ==> OutlineShaderController;

  OutlineDrawGenerator ==> OutlineLogic;
  OutlineDrawGenerator ==> OutlineShaderController;
  OutlineDrawGenerator ==> VoxelCore;

  VoxelMeshGenerator ==> VoxelMesh;

  VoxelMesh ==> VoxelCore;
  VoxelMesh ==> MeshShaderController;

  MeshShaderController ==> VoxelCore;
  MeshShaderController ==> MeshShader;

  OutlineShaderController ==> VoxelCore;
  OutlineShaderController ==> OutlineShader;

  MeshShader ==> MarchingHelper;
  OutlineShader ==> MarchingHelper;

  Vector2Extension ==> OutlineLogic;

  DebugController ==> VoxelCore;
  DebugController ==> Player;

  TerrainEditorController ==> VoxelCore;
  TerrainEditorController ==> Editing;
  TerrainEditorController ==> InfiniteGenerator;
  TerrainEditorController ==> Player;
  TerrainEditorController ==> TerrainEditorGizmos;
  TerrainEditorController ==> ChunkHelper;
  TerrainEditorController ==> TerrainEditor;
  TerrainEditorController ==> LightingFiller;
  TerrainEditorController ==> LightingGenerator;

  TerrainEditorGizmos ==> ChunkHelper;
  TerrainEditorGizmos ==> StencilTypes;

  TerrainEditor ==> VoxelCore;
  TerrainEditor ==> TerrainEditorController;
  TerrainEditor ==> EditingStencil;
  TerrainEditor ==> EditingStencilCircle;
  TerrainEditor ==> BlockTypes;

  TerrainGenerationController ==> TerrainTypes;
  TerrainGenerationController ==> NoiseFill;
  TerrainGenerationController ==> NoiseRandom;
  TerrainGenerationController ==> NoisePerlin;
  TerrainGenerationController ==> VoxelCore;

  NoiseFill ==> NoisePerlin;

  Editing ==> BlockTypes;
  Editing ==> StencilTypes;

  ChunkSaveManager ==> RegionSaveManager;
  ChunkSaveManager ==> VoxelCore;
  ChunkSaveManager ==> SerializationManager;

  SerializationManager ==> Vector2SerializationSurrogate;
  SerializationManager ==> Vector3SerializationSurrogate;

  WorldSaveManager ==> SerializationManager;
  WorldSaveManager ==> VoxelCore;
  WorldSaveManager ==> Player;

  RegionSaveManager ==> SerializationManager;
  RegionSaveManager ==> VoxelCore;
  RegionSaveManager ==> WorldSaveManager;

  DynamicLighting ==> VoxelCore;
  DynamicLighting ==> LightingHelper;

  StaticLighting ==> VoxelCore;
  StaticLighting ==> LightingHelper;

  LightingGenerator ==> VoxelCore;
  LightingGenerator ==> StaticLighting;
  LightingGenerator ==> DynamicLighting;
  LightingGenerator ==> TextureScaler;

  LightingFiller ==> VoxelCore;
```
