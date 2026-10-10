export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export function normalizePagedResponse<T>(
  response: PagedResponse<T> | T[],
  page: number,
  pageSize: number,
): PagedResponse<T> {
  if (!Array.isArray(response)) return response;

  const totalCount = response.length;
  const totalPages = Math.ceil(totalCount / pageSize);
  const start = (page - 1) * pageSize;
  return {
    items: response.slice(start, start + pageSize),
    page,
    pageSize,
    totalCount,
    totalPages,
    hasPrevious: page > 1,
    hasNext: page < totalPages,
  };
}