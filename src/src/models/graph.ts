export interface ExpressionGraph {
  nodes: Node[];
  edges: Edge[];
}

export interface Node {
  id: number;
  status: number;
  type: number;
  label: string;
}

export interface Edge {
  start: number;
  end: number;
}
