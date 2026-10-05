export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
}

export interface ValidationProblemDetails extends ProblemDetails {
  errors?: Record<string, string[]>;
}

export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly fieldErrors?: Record<string, string[]>;

  constructor(status: number, title: string, detail?: string, fieldErrors?: Record<string, string[]>) {
    super(detail ?? title);
    this.status = status;
    this.title = title;
    this.fieldErrors = fieldErrors;
  }
}

/** Parses a non-OK fetch Response into an ApiError. Falls back gracefully if the body isn't JSON
 * (e.g. a proxy error page) so callers always get a usable error either way. */
export async function parseApiError(response: Response): Promise<ApiError> {
  let body: ValidationProblemDetails | null = null;

  try {
    body = await response.json();
  } catch {
    // non-JSON body; body stays null and the generic message below is used
  }

  return new ApiError(
    response.status,
    body?.title ?? `Request failed with status ${response.status}`,
    body?.detail,
    body?.errors,
  );
}
