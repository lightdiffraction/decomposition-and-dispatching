import { AfterViewInit, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import Graph from "graphology";
import sigma from "sigma";
import { ParallelExpressionsService } from '../services/parallel-expressions-services';
import { ExpressionGraph } from '../models/graph';
import { GraphCoordinates } from '../models/graphCoordinates';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { FormsModule } from '@angular/forms';
import forceAtlas2 from "graphology-layout-forceatlas2";
import FA2Layout from "graphology-layout-forceatlas2/worker";
import { Matrix } from '../models/matrix';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, FormsModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements AfterViewInit {
  protected title = 'Parallel expressions';

  protected expression: string = '';

  protected graph: Graph = new Graph();

  protected matrix?: Matrix;

  protected isOperand?: boolean;

  protected clickedNode?: string;

  private renderer: sigma | undefined;

  private graphCoordinates: GraphCoordinates[] | undefined;

  private readonly connection?: HubConnection;

  private createGraphRequired: boolean = false;  

  constructor(private parallelExpressionsService: ParallelExpressionsService) {

    this.connection = new HubConnectionBuilder()
      .withUrl("http://localhost:5095/chathub")
      .build();

    this.connection.on('GraphUpdated', (username: string, output: string) => {
      console.log('GraphUpdated MESSAGE RECEIVED ' + username + ' ' + output);
      /*const sensibleSettings = forceAtlas2.inferSettings(this.graph);
      const fa2Layout = new FA2Layout(this.graph, {
        settings: sensibleSettings,
      });
      fa2Layout.start();*/
    });

    this.connection.on('InProgress', (id: number) => {
      console.log('GraphUpdated NODE UPDATE' + id);
      this.getGraph();
    });

    this.connection.on('Done', (id: number) => {
      console.log('GraphUpdated NODE DONE' + id);
      this.parallelExpressionsService.getGraph().subscribe((g: ExpressionGraph) => {
        this.updateGraph(g); 
      });
    });

    this.connection.start().catch((err) => console.log('ERR', ''));

  }

  ngAfterViewInit() {

    console.log('start')

    this.parallelExpressionsService.getGraph().subscribe((g: ExpressionGraph) => {
      console.log('initial graph', g);
      this.graphCoordinates = this.createGraphCoordinates(g.nodes.length);
      this.createGraph(g);
    });

    /*interval(10000)
      .pipe(
        mergeMap(() => this.parallelExpressionsService.getGraph()),
      )
      .subscribe((g) => {
        console.log(g);
      });*/
  }

  protected start(): void {
    this.createGraphRequired = true;
    this.connection?.send("StartCalculation", this.expression)
      .then(() => console.log('StartCalculation MESSAGE SENT'));
  }

  protected reset(): void {
    this.parallelExpressionsService.reset().subscribe({
      next: (x) => {
        console.log('Reset');
        this.getGraph();
      },
      error: () => {
      }
    });
  }

  private getGraph() {
    this.parallelExpressionsService.getGraph().subscribe((g: ExpressionGraph) => {
      if (this.createGraphRequired) {
        console.log(g)
        this.createGraph(g);
        this.createGraphRequired = false;
      }
      else {
        this.updateGraph(g);
      }
      /*const sensibleSettings = forceAtlas2.inferSettings(this.graph);
      const fa2Layout = new FA2Layout(this.graph, {
        settings: sensibleSettings,
      });
      fa2Layout.start();*/
    });
  }

  private createGraph(expressionGraph: ExpressionGraph) {
    this.graph?.clear();
    this.graph = new Graph();
    this.graphCoordinates = this.createGraphCoordinates(expressionGraph.nodes.length);

    for (let i = 0; i < expressionGraph.nodes.length; i++) {
      const node = expressionGraph.nodes[i];
      const nodeLabel = node.label;

      let color: string;
      if (node.type === 0) //node data
      {
        color = '#99d98c';
      }
      else if (node.status === 0) {
        color = '#c8b6ff';
      }
      else if (node.status === 1) {
        color = '#ff0054';
      }
      else {
        color = '#55a630';
      }

      if (!this.graph.hasNode(nodeLabel)) {

        //const id = expressionGraph.nodes[i].id - 1;
        // x: this.graphCoordinates![id].x
        this.graph.addNode(nodeLabel,
          { label: nodeLabel, x: Math.random(), y: Math.random(), size: 15, color: color });
      }
    }

    for (let i = 0; i < expressionGraph.edges.length; i++) {
      const edge = expressionGraph.edges[i];
      const startLabel = expressionGraph.nodes.find(n => n.id === edge.start)?.label;
      const endLabel = expressionGraph.nodes.find(n => n.id === edge.end)?.label;

      if (!this.graph.hasEdge(startLabel, endLabel)) {
        this.graph.addEdge(startLabel, endLabel, { type: "arrow", size: 4, color: "#023047" });
      }
    }

    this.renderer = new sigma(this.graph, document.getElementById("container") as HTMLElement);

    this.renderer.on("clickNode", ({ node }) => {
      console.log('Clicked ', node);

      this.parallelExpressionsService.getNode(node).subscribe({
        next: (matrix) => {
          console.log('Matrix ', matrix);
          this.matrix = matrix;
          this.isOperand = matrix != null;
          this.clickedNode = node;
        },
        error: () => {
        }
      });
    })

    const sensibleSettings = forceAtlas2.inferSettings(this.graph);
    const fa2Layout = new FA2Layout(this.graph, {
      settings: sensibleSettings,
    });
    fa2Layout.start();
    console.log("fa2Layout.start()");
    this.renderer.refresh();
  }

  private updateGraph(expressionGraph: ExpressionGraph) {
    for (let i = 0; i < expressionGraph.nodes.length; i++) {
      const node = expressionGraph.nodes[i];

      let color: string;
      if (node.type === 0) //node data
      {
        color = '#99d98c';
      }
      else if (node.status === 0) {
        color = '#c8b6ff';
      }
      else if (node.status === 1) {
        color = '#ff0054';
      }
      else {
        color = '#55a630';
      }

      const id = expressionGraph.nodes[i].label;
      this.graph.updateNodeAttribute(id, "color", (value) => color);
    }

    /*this.renderer = new sigma(this.graph, document.getElementById("container") as HTMLElement);
    const sensibleSettings = forceAtlas2.inferSettings(this.graph);
    const fa2Layout = new FA2Layout(this.graph, {
      settings: sensibleSettings,
    });
    fa2Layout.start();
    this.renderer.refresh();
    */
  }

  private createGraphCoordinates(length: number): GraphCoordinates[] {
    let array: GraphCoordinates[] = [];

    for (let i = 0; i < length; i++) {
      //while (true) {
        let randomX = Math.random();
        let randomY = Math.random();
        //if (
          //array.find(item => Math.abs(item.x - randomX) < 0.01 || Math.abs(item.y - randomY) < 0.01) === undefined) {
          array.push({ x: randomX, y: randomY });
          //break;
        //}
      //}
      
    }

    return array;
  }
}
