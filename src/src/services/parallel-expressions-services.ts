import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { Injectable } from "@angular/core";
import { ExpressionGraph } from "../models/graph";
import { ExpressionRequest } from "../models/ExpressionsRequest";
import { Matrix } from "../models/matrix";

@Injectable({
  providedIn: 'root',
})
export class ParallelExpressionsService {
  constructor(private httpClient: HttpClient) {

  }

  public start(request: ExpressionRequest): Observable<unknown> {
    return this.httpClient.post(`/api/ParallelExpressions/parse-matrix-expression`, request);
  }

  public getGraph(): Observable<ExpressionGraph> {
    return this.httpClient.get<ExpressionGraph>(`/api/ParallelExpressions/graph`);
  }

  public reset(): Observable<unknown> {
    return this.httpClient.get<unknown>(`/api/ParallelExpressions/reset`);
  }

  public getNode(operand: string): Observable<Matrix> {
    return this.httpClient.get<Matrix>(`/api/ParallelExpressions/node/${operand}`);
  }
}
