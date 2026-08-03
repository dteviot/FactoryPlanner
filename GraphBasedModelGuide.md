# Graph-Based Production Plan Model

## Overview

The graph-based model treats production plans as directed graphs where:
- **Nodes**: `ProductionStep` objects (production steps)
- **Edges**: `MaterialFlow` objects (material transfers between steps)

## Core Concepts

### ProductionStep (Node)
```csharp
public class ProductionStep
{
    public Recipe Recipe { get; set; }
    public double MachinesRequired { get; set; }
    public RecipeMaterial TargetMaterial { get; set; }
    
    // Graph connections
    public List<MaterialFlow> Inflows { get; set; }  // Incoming edges
    public List<MaterialFlow> Outflows { get; set; } // Outgoing edges
}
```

### MaterialFlow (Edge)
```csharp
public class MaterialFlow
{
    public string Material { get; set; }        // What's flowing
    public ProductionStep Producer { get; set; } // Source node
    public ProductionStep Consumer { get; set; } // Target node
    public double Rate { get; set; }            // Flow quantity (per minute)
}
```

## Graph Structure

```
        Material Flow (Edge)
      ↗-------------------↘
Producer (Node)        Consumer (Node)
  ↓                        ↓
ProductionStep          ProductionStep
```

### Example Production Chain Graph
```
Iron Ore Miner → (90/min Iron Ore) → Smelter → (90/min Iron Ingot) → Constructor → (60/min Iron Plate)
      Node1           Edge1              Node2          Edge2              Node3
```

## Future Extensions

### 1. **Graph Algorithms**
- Shortest path for material delivery
- Critical path analysis
- Cycle detection algorithms
- Graph visualization

### 2. **Optimization**
- Minimum cost flow algorithms
- Machine placement optimization
- Power consumption graphs

### 3. **Analysis**
- Bottleneck identification
- Resource utilization graphs
- Production capacity analysis

